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
/// Deadeye X: the next Round you successfully Fire deals X additional Attack damage, then all
/// Deadeye is removed.
///
/// The power holds no logic of its own — <see cref="Cylinder.Revolver.Fire"/> reads and clears it,
/// because only the cylinder knows whether a shot was a Round or a Click. A Click must not eat it.
/// </summary>
public sealed class DeadeyePower : HelloSpirePower
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;
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
/// Dodge X: prevent all damage from the next X individual enemy Attack hits this turn.
///
/// It stops one hit, not a whole intent, and it does not survive the turn — reading the intent is
/// the skill it rewards, which is why it is deliberately not Intangible.
/// </summary>
public sealed class DodgePower : HelloSpirePower
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;

    /// <summary>Raised by the damage patch when this Dodge really did eat a hit. See <see cref="ArmorPower"/>.</summary>
    public bool PreventedPending { get; set; }

    public override async Task BeforeDamageReceived(PlayerChoiceContext choiceContext, Creature target,
        decimal amount, ValueProp props, Creature? dealer, CardModel? cardSource)
    {
        if (target != Owner || cardSource != null || !PreventedPending) return;
        PreventedPending = false;

        await PowerCmd.ModifyAmount(choiceContext, this, -1m, null, null, false);
    }

    public override async Task BeforeSideTurnStart(PlayerChoiceContext ctx, CombatSide side, IReadOnlyList<Creature> participants, ICombatState state)
    {
        PreventedPending = false;
        if (side != Owner.Side) return;
        await PowerCmd.Remove(this);
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

    public void Consume()
    {
        _ = PowerCmd.Remove(this);
    }
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
