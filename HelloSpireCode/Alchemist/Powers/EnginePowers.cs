using HelloSpire.HelloSpireCode.Alchemist.Cards;
using HelloSpire.HelloSpireCode.Alchemist.Lab;
using HelloSpire.HelloSpireCode.Alchemist.Potions;
using HelloSpire.HelloSpireCode.Powers;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Potions;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;

using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
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

/// <summary>The first time Poison is applied each turn, regardless of source, apply additional Poison to the same target.</summary>
public sealed class ResidualToxinsPower : AlchemistEnginePower, IPoisonAppliedListener
{
    public async Task OnPoisonApplied(PlayerChoiceContext ctx, LabContext lab, Creature target, decimal amount)
    {
        if (UsedThisTurn) return;
        UsedThisTurn = true;

        Flash();
        await AlchemistEffects.ApplyPoison(ctx, lab, target, Amount);
    }
}

/// <summary>Whenever you Infuse a lot in one action, gain Energy.</summary>
public sealed class ConcentratePower : AlchemistEnginePower, IInfuseListener
{
    /// <summary>The single-call threshold. Set by the card; 10 base.</summary>
    public decimal Threshold { get; set; } = 10m;

    public async Task OnInfused(PlayerChoiceContext ctx, LabContext lab, decimal amount)
    {
        if (amount < Threshold) return;

        Flash();
        await AlchemistEffects.GainEnergy(lab, Amount);
    }
}

/// <summary>The first time you Brew each turn, draw a card.</summary>
public sealed class BrewingHabitPower : AlchemistEnginePower, IBrewListener
{
    public async Task OnBrewed(PlayerChoiceContext ctx, LabContext lab, PotionModel potion)
    {
        if (UsedThisTurn) return;
        UsedThisTurn = true;

        Flash();
        await AlchemistEffects.Draw(ctx, lab, (int)Amount);
    }
}

/// <summary>The first Brew each turn is worth Potency and a little Infuse. Makes a setup turn less of a gap.</summary>
public sealed class ThermalBufferPower : AlchemistEnginePower, IBrewListener
{
    /// <summary>Block Infused into Unstable Concoction. Set by the card; 3 base, 4 upgraded.</summary>
    public decimal BlockInfuse { get; set; } = 3m;

    public async Task OnBrewed(PlayerChoiceContext ctx, LabContext lab, PotionModel potion)
    {
        if (UsedThisTurn) return;
        UsedThisTurn = true;

        Flash();
        await AlchemistEffects.GainPotency(ctx, lab, Amount);
        await Belt.Infuse(ctx, lab, block: BlockInfuse);
        await Belt.LeaveResidualReagent(ctx, lab);
    }
}

/// <summary>
/// The first Exhaust each turn Infuses Damage, up to a hard per-combat cap.
///
/// The cap is the whole design. An uncapped trigger makes stalling a fight the correct play,
/// which is the guardrail exists to prevent, regardless of what currency it pays out in.
/// </summary>
public sealed class ReagentPressPower : AlchemistEnginePower, IExhaustListener
{
    /// <summary>Triggers remaining this combat. Set by the card; 3 base, 4 upgraded.</summary>
    public int TriggersLeft { get; set; } = 3;

    public async Task OnExhausted(PlayerChoiceContext ctx, LabContext lab)
    {
        if (UsedThisTurn || TriggersLeft <= 0) return;
        UsedThisTurn = true;
        TriggersLeft--;

        Flash();
        await Belt.Infuse(ctx, lab, damage: Amount);
        await Belt.LeaveResidualReagent(ctx, lab);
    }
}

/// <summary>Distilling buys Block on the way past. The Distillation deck's defence.</summary>
public sealed class EfficientDistillationPower : AlchemistEnginePower, IDistillListener
{
    public async Task OnDistilled(PlayerChoiceContext ctx, LabContext lab, PotionModel potion)
    {
        Flash();
        await AlchemistEffects.GainBlock(lab, Amount);
    }
}

/// <summary>The first Potion you use each turn also draws a card.</summary>
public sealed class ReactiveLaboratoryPower : AlchemistEnginePower, IPotionUseListener
{
    public async Task OnPotionUsed(PlayerChoiceContext ctx, LabContext lab, PotionModel potion, Creature? target)
    {
        if (UsedThisTurn) return;
        UsedThisTurn = true;

        Flash();
        await AlchemistEffects.Draw(ctx, lab, (int)Amount);
    }
}

/// <summary>Whenever you Brew a Poison Potion, apply Poison to a random enemy.</summary>
public sealed class ToxicCulturePower : AlchemistEnginePower, IBrewListener
{
    public async Task OnBrewed(PlayerChoiceContext ctx, LabContext lab, PotionModel potion)
    {
        if (potion is not (PoisonPotion or VolatilePoisonPotion)) return;

        var target = AlchemistEffects.RandomEnemy(lab);
        if (target == null) return;

        Flash();
        await AlchemistEffects.ApplyPoison(ctx, lab, target, Amount);
    }
}

/// <summary>
/// The first Slot emptied each turn is worth Block, and a little Infuse besides.
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
        await Belt.Infuse(ctx, lab, block: 2m);
        await Belt.LeaveResidualReagent(ctx, lab);
    }
}

/// <summary>
/// Cards you create in combat arrive Upgraded.
///
/// Holds no logic — <see cref="Alchemy.Create"/> checks for it, because creation is funnelled
/// through one function specifically so this power only has to exist in one place.
/// </summary>
public sealed class RefinersEyePower : AlchemistEnginePower;

/// <summary>Whenever you create a Status, Infuse Damage.</summary>
public sealed class VolatileLaboratoryPower : AlchemistEnginePower, IStatusCreatedListener
{
    public async Task OnStatusCreated(PlayerChoiceContext ctx, LabContext lab)
    {
        Flash();
        await Belt.Infuse(ctx, lab, damage: Amount);
        await Belt.LeaveResidualReagent(ctx, lab);
    }
}

/// <summary>Whenever you Infuse, draw a card for every 10 Infused in that action.</summary>
public sealed class AccumulationPower : AlchemistEnginePower, IInfuseListener
{
    private const decimal Threshold = 10m;

    public async Task OnInfused(PlayerChoiceContext ctx, LabContext lab, decimal amount)
    {
        var draws = (int)(amount / Threshold) * (int)Amount;
        if (draws <= 0) return;

        Flash();
        await AlchemistEffects.Draw(ctx, lab, draws);
    }
}

/// <summary>
/// The first Potion each combat resolves twice and is consumed once.
///
/// The actual double-resolution is claimed and performed by PotionUsePatch's prefix, before the
/// Potion's own effect runs -- not here. IPotionUseListener.OnPotionUsed (what every other engine
/// in this file reacts through) only fires AFTER a Potion has already resolved once, which is one
/// step too late to make it resolve a second time. TryClaim is the once-per-COMBAT latch (unlike
/// every other engine here, which resets every turn) -- PotionUsePatch calls it and, if it
/// succeeds, adds the Potion to LabPower.DoubleActivate, the same one-shot mechanism Pressure
/// Burst used to mark a chosen Potion.
/// </summary>
public sealed class EternalCruciblePower : AlchemistEnginePower
{
    private bool _usedThisCombat;

    public bool TryClaim()
    {
        if (_usedThisCombat) return false;
        _usedThisCombat = true;
        Flash();
        return true;
    }
}

/// <summary>Whenever you Brew, draw a card.</summary>
public sealed class BrewingEnginePower : AlchemistEnginePower, IBrewListener
{
    public async Task OnBrewed(PlayerChoiceContext ctx, LabContext lab, PotionModel potion)
    {
        Flash();
        await AlchemistEffects.Draw(ctx, lab, (int)Amount);
    }
}

/// <summary>
/// Distilling makes the next Potion you Brew stronger.
///
/// The Brewer archetype's only real scaling axis — without it, that deck is a pile of one-shot
/// consumables with no way to grow. The multiplier itself lives on LabPower
/// (<see cref="LabPower.BrewBonusMultiplier"/>) and is consumed by <see cref="Belt.Brew"/>, since
/// it has to apply generically to whatever gets Brewed next, not to anything this Power can see.
/// </summary>
public sealed class DistillationMasteryPower : AlchemistEnginePower, IDistillListener
{
    /// <summary>The multiplier granted. Set by the card; 1.5 base.</summary>
    public decimal Multiplier { get; set; } = 1.5m;

    public Task OnDistilled(PlayerChoiceContext ctx, LabContext lab, PotionModel potion)
    {
        Flash();
        var bench = AlchemistEffects.Peek(lab);
        if (bench != null) bench.BrewBonusMultiplier = Multiplier;
        return Task.CompletedTask;
    }
}

/// <summary>Whenever you use a Potion, gain Strength this turn.</summary>
public sealed class BottledFuryPower : AlchemistEnginePower, IPotionUseListener
{
    public async Task OnPotionUsed(PlayerChoiceContext ctx, LabContext lab, PotionModel potion, Creature? target)
    {
        Flash();
        await PowerCmd.Apply<BottledFuryStrengthPower>(ctx, Owner, Amount, Owner, null);
    }
}

/// <summary>
/// The temporary Strength Bottled Fury grants -- a plain <see cref="TemporaryStrengthPower"/>,
/// same Dark Shackles pattern as the Paladin's Seal of Justice, just positive. Not an
/// AlchemistEnginePower: it needs the vanilla temp-Strength apply/restore bookkeeping instead.
/// </summary>
public sealed class BottledFuryStrengthPower : TemporaryStrengthPower
{
    public override AbstractModel OriginModel => ModelDb.Card<BottledFury>();
    protected override bool IsPositive => true;
}
