using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;

using HelloSpire.HelloSpireCode.Powers;

namespace HelloSpire.HelloSpireCode.Gunslinger.Powers;

/// <summary>
/// Deadeye X: every Round you Fire for the rest of this turn deals X additional Attack damage.
/// All Deadeye is removed at the end of your turn.
///
/// It used to be spent by the first Round that landed, which made the whole keyword a single
/// +X on one shot — a card that read "gain 5 Deadeye" was worth strictly less than one that read
/// "deal 5 more damage", and stacking it before a Fire 6 was actively wrong. As a turn-long
/// aura it does what the character is built around instead: set the gun up, then spend the
/// cylinder into a turn you sharpened.
///
/// The power still holds no arithmetic of its own — <see cref="Cylinder.Revolver.Fire"/> reads it,
/// because only the cylinder knows whether a shot was a Round or a Click, and a Click gets nothing.
///
/// Expiry hangs off the *enemy* side's turn start, not the owner's, and that is deliberate.
/// Defensive powers clear at the owner's turn start, because defence has to survive into the
/// enemy's turn to do its job. Deadeye is the opposite: only the Gunslinger Fires, and only on
/// its own turn, so the first moment nothing can use it any more is the moment the player's turn
/// ends — which is exactly when the other side's turn starts.
///
/// Clearing at the owner's turn start would also have been a trap. Bottomless Bandolier and
/// Ride Together both hand out Deadeye from that same turn-start sweep, and nothing fixes the
/// order powers are visited in: half the time the fresh Deadeye would have been wiped by a hook
/// running to clear the last turn's. Expiring a side earlier means nothing that grants Deadeye
/// and nothing that clears it ever run in the same sweep.
/// </summary>
public sealed class DeadeyePower : HelloSpirePower
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;

    public override async Task BeforeSideTurnStart(PlayerChoiceContext ctx, CombatSide side,
        IReadOnlyList<Creature> participants, ICombatState state)
    {
        if (side == Owner.Side) return;
        await PowerCmd.Remove(this);
    }
}

/// <summary>
/// Armor X: reduce each instance of unblocked Attack damage by X, then lose 1 Armor.
///
/// Stronger than Block across future turns, weaker against a long multi-hit intent, and distinct
/// from Plating (which pays out as Block at end of turn). The reduction itself happens in
/// <see cref="GunslingerDamagePatch"/>, which is the only place the damage pipeline is touched.
///
/// Reducing and spending are split across those two places on purpose. The damage hook is asked
/// "how big is this hit" more than once per hit — the intent forecast asks it too — so spending
/// there emptied a stack of Armor before anything had actually swung. The hook raises
/// <see cref="AbsorbedPending"/> instead, and this power spends it below, from a hook that only
/// runs for damage that is really being dealt.
/// </summary>
public sealed class ArmorPower : HelloSpirePower
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;

    /// <summary>Iron Will lets the first decrease each turn be skipped.</summary>
    public bool SkipNextDecrease { get; set; }

    /// <summary>Raised by the damage patch when this Armor really did reduce an incoming hit.</summary>
    public bool AbsorbedPending { get; set; }

    public override async Task BeforeDamageReceived(PlayerChoiceContext choiceContext, Creature target,
        decimal amount, ValueProp props, Creature? dealer, CardModel? cardSource)
    {
        if (target != Owner || cardSource != null || !AbsorbedPending) return;
        AbsorbedPending = false;

        // Hard Leather waits on this. It is announced here rather than from the patch so that it
        // fires once per absorbed hit rather than once per forecast redraw.
        GunslingerHooks.NotifyArmorPrevented(Owner);

        if (SkipNextDecrease)
        {
            SkipNextDecrease = false;
            return;
        }

        await PowerCmd.ModifyAmount(choiceContext, this, -1m, null, null, false);
    }

    public override Task BeforeSideTurnStart(PlayerChoiceContext ctx, CombatSide side, IReadOnlyList<Creature> participants, ICombatState state)
    {
        // A forecast can raise the flag for a hit that never lands. Clearing it each turn keeps a
        // stale one from eating the first real hit of the next.
        AbsorbedPending = false;
        return Task.CompletedTask;
    }
}

/// <summary>
/// The next Round you Load goes under the hammer instead of into the first empty chamber.
/// Consumed by <see cref="Cylinder.Revolver.Load"/> as soon as it redirects a Round.
/// </summary>
public sealed class StackedChamberPower : HelloSpirePower
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Single;

    /// <summary>
    /// Spends the redirect. Awaited by the caller: a Load that puts several Rounds in has to see
    /// this power gone before it picks the second one's chamber, or every Round in the batch ends
    /// up stacked into the same one.
    /// </summary>
    public Task Consume() => PowerCmd.Remove(this);
}

/// <summary>
/// Block that arrives at the start of your next turn, then goes away. The payout half of Hard
/// Leather and the reason Smoke and Lead defends the turn after you shoot rather than this one.
/// </summary>
public sealed class BlockNextTurnPower : HelloSpirePower
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;

    public override async Task BeforeSideTurnStart(PlayerChoiceContext ctx, CombatSide side, IReadOnlyList<Creature> participants, ICombatState state)
    {
        if (side != Owner.Side) return;

        await CreatureCmd.GainBlock(Owner, Amount, ValueProp.Move, null, false);
        await PowerCmd.Remove(this);
    }
}

/// <summary>
/// Never Still's delayed half: an Energy and a card or two at the start of your next turn.
/// </summary>
public sealed class NeverStillPower : HelloSpirePower
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;

    /// <summary>Cards drawn alongside the Energy. Set by the card that applied this.</summary>
    public int CardsToDraw { get; set; } = 1;

    public override async Task BeforeSideTurnStart(PlayerChoiceContext ctx, CombatSide side, IReadOnlyList<Creature> participants, ICombatState state)
    {
        if (side != Owner.Side) return;

        var player = state.Players.FirstOrDefault(p => p.Creature == Owner);
        if (player != null)
        {
            await PlayerCmd.GainEnergy(Amount, player);
            if (CardsToDraw > 0) await CardPileCmd.Draw(ctx, CardsToDraw, player);
        }

        await PowerCmd.Remove(this);
    }
}
