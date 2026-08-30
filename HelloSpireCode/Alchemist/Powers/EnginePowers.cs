using HelloSpire.HelloSpireCode.Alchemist.Lab;
using HelloSpire.HelloSpireCode.Powers;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Potions;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;

using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Rooms;
using HelloSpire.HelloSpireCode.Alchemist;
using MegaCrit.Sts2.Core.ValueProps;
namespace HelloSpire.HelloSpireCode.Alchemist.Powers;

/// <summary>
/// Shared plumbing for the Alchemist's Power cards: a <see cref="LabContext"/> built from the
/// creature the power sits on, and a once-per-turn latch, which almost all of them need.
/// </summary>
public abstract class AlchemistEnginePower : HelloSpirePower
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;

    /// <summary>Set by the effect that fires; cleared at the start of the owner's turn.</summary>
    protected bool UsedThisTurn { get; set; }

    /// <summary>The bench this power is attached to, or null outside of a player's combat.</summary>
    protected LabContext? Lab
    {
        get
        {
            var player = AlchemistEffects.PlayerFor(Owner);
            return player == null ? null : LabContext.From(player);
        }
    }

    public override Task BeforeSideTurnStart(PlayerChoiceContext ctx, CombatSide side, IReadOnlyList<Creature> participants, ICombatState state)
    {
        if (side == Owner.Side) UsedThisTurn = false;
        return OnOwnerTurnStart(ctx, side, state);
    }

    protected virtual Task OnOwnerTurnStart(PlayerChoiceContext ctx, CombatSide side, ICombatState state) =>
        Task.CompletedTask;
}

/// <summary>The first Potion you drink each turn throws a little heat at somebody.</summary>
public sealed class ResidualHeatPower : AlchemistEnginePower, IPotionUseListener
{
    public async Task OnPotionUsed(PlayerChoiceContext ctx, LabContext lab, PotionModel potion)
    {
        if (UsedThisTurn) return;
        UsedThisTurn = true;

        var target = AlchemistEffects.RandomEnemy(lab);
        if (target == null) return;

        Flash();
        await CreatureCmd.Damage(ctx, target, Amount, ValueProp.Unpowered, Owner, null);
    }
}

/// <summary>The first Brew each turn is worth a little Block. Makes a setup turn less of a gap.</summary>
public sealed class HeatBathPower : AlchemistEnginePower, IBrewListener
{
    public async Task OnBrewed(PlayerChoiceContext ctx, LabContext lab, PotionModel potion)
    {
        if (UsedThisTurn) return;
        UsedThisTurn = true;

        Flash();
        await AlchemistEffects.GainBlock(lab, Amount);
    }
}

/// <summary>
/// The first Exhaust each turn mints a Gold, up to a hard per-combat cap.
///
/// The cap is the whole design. An uncapped Gold trigger makes stalling a fight the correct play,
/// which is the failure mode the Gold guardrails exist to prevent.
/// </summary>
public sealed class CoinPressPower : AlchemistEnginePower, IExhaustListener
{
    /// <summary>Triggers remaining this combat. Set by the card; 3 base, 4 upgraded.</summary>
    public int TriggersLeft { get; set; } = 3;

    public async Task OnExhausted(PlayerChoiceContext ctx, LabContext lab)
    {
        if (UsedThisTurn || TriggersLeft <= 0) return;
        UsedThisTurn = true;
        TriggersLeft--;

        Flash();
        await Ledger.GainGold(ctx, lab, (int)Amount);
    }
}

/// <summary>Spending Gold buys Block on the way past. The Investor deck's defence.</summary>
public sealed class MerchantsInstinctPower : AlchemistEnginePower, IInvestListener
{
    public async Task OnInvested(PlayerChoiceContext ctx, LabContext lab, int cost)
    {
        Flash();
        await AlchemistEffects.GainBlock(lab, Amount);
    }
}

/// <summary>The first Potion each turn also draws. Turns the belt into a second hand.</summary>
public sealed class ReactiveMixturePower : AlchemistEnginePower, IPotionUseListener
{
    /// <summary>Block granted alongside the card. Zero until upgraded.</summary>
    public int BlockBonus { get; set; }

    public async Task OnPotionUsed(PlayerChoiceContext ctx, LabContext lab, PotionModel potion)
    {
        if (UsedThisTurn) return;
        UsedThisTurn = true;

        Flash();
        await AlchemistEffects.Draw(ctx, lab, (int)Amount);
        if (BlockBonus > 0) await AlchemistEffects.GainBlock(lab, BlockBonus);
    }
}

/// <summary>
/// The first Slot emptied each turn is worth Block.
///
/// Fires on Distill as well as on drinking, which is the point — it is the Empty Belt archetype's
/// engine, and Distilling is that archetype's fastest way to empty the belt.
/// </summary>
public sealed class ClosedSystemPower : AlchemistEnginePower, ISlotEmptiedListener
{
    public async Task OnSlotEmptied(PlayerChoiceContext ctx, LabContext lab)
    {
        if (UsedThisTurn) return;
        UsedThisTurn = true;

        Flash();
        await AlchemistEffects.GainBlock(lab, Amount);
    }
}

/// <summary>
/// Cards you create in combat arrive Upgraded.
///
/// Holds no logic — <see cref="Alchemy.Create"/> checks for it, because creation is funnelled
/// through one function specifically so this power only has to exist in one place.
/// </summary>
public sealed class RefinersEyePower : AlchemistEnginePower;

/// <summary>Some of what you Invested comes back when the fight ends.</summary>
public sealed class CompoundInterestPower : AlchemistEnginePower
{
    /// <summary>Percentage of Invested Gold returned. 25 base, 33 upgraded.</summary>
    public int Percent { get; set; } = 25;

    public override async Task AfterCombatEnd(CombatRoom room)
    {
        if (Lab is not { } lab) return;

        var refund = Ledger.SpentThisCombat(lab) * Percent / 100;
        if (refund <= 0) return;

        Flash();
        await Ledger.GainGold(new ThrowingPlayerChoiceContext(), lab, refund);
    }
}

/// <summary>The first Potion each combat resolves twice and is consumed once.</summary>
public sealed class EternalCruciblePower : AlchemistEnginePower, IPotionUseListener
{
    private bool _usedThisCombat;

    public Task OnPotionUsed(PlayerChoiceContext ctx, LabContext lab, PotionModel potion)
    {
        if (_usedThisCombat) return Task.CompletedTask;
        _usedThisCombat = true;

        Flash();

        // TODO(Phase 3): re-resolving a Potion needs the same potion-resolution patch that drives
        // Belt.OnPotionUsed. The trigger and the once-per-combat latch are correct; the second
        // resolution is the missing half.
        return Task.CompletedTask;
    }

    public override Task AfterCombatEnd(CombatRoom room)
    {
        _usedThisCombat = false;
        return Task.CompletedTask;
    }
}

/// <summary>The first Gold each turn draws a card and buys a little Block.</summary>
public sealed class GoldenEnginePower : AlchemistEnginePower, IGoldListener
{
    /// <summary>Block granted alongside the card.</summary>
    public int BlockBonus { get; set; } = 2;

    public async Task OnGoldGained(PlayerChoiceContext ctx, LabContext lab, int amount)
    {
        if (UsedThisTurn) return;
        UsedThisTurn = true;

        Flash();
        await AlchemistEffects.Draw(ctx, lab, (int)Amount);
        await AlchemistEffects.GainBlock(lab, BlockBonus);
    }
}

/// <summary>The first Exhaust each turn replaces itself.</summary>
public sealed class ConservationOfMatterPower : AlchemistEnginePower, IExhaustListener
{
    public async Task OnExhausted(PlayerChoiceContext ctx, LabContext lab)
    {
        if (UsedThisTurn) return;
        UsedThisTurn = true;

        Flash();
        await AlchemistEffects.Draw(ctx, lab, (int)Amount);
    }
}

/// <summary>
/// Distilling makes the next Volatile Potion stronger.
///
/// The Brewer archetype's only real scaling axis — without it, that deck is a pile of one-shot
/// consumables with no way to grow.
/// </summary>
public sealed class DistillationMasteryPower : AlchemistEnginePower, IDistillListener
{
    public async Task OnDistilled(PlayerChoiceContext ctx, LabContext lab, PotionModel potion)
    {
        Flash();
        await AlchemistEffects.GainPotency(ctx, lab, Amount);
    }
}
