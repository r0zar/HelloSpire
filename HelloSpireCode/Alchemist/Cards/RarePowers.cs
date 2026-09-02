using HelloSpire.HelloSpireCode.Alchemist.Lab;
using HelloSpire.HelloSpireCode.Alchemist.Potions;
using HelloSpire.HelloSpireCode.Alchemist.Powers;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;

namespace HelloSpire.HelloSpireCode.Alchemist.Cards;

// Rare Powers 1-6. One engine per pillar: Infuse (Accumulation), Potion-use (Eternal
// Crucible), Brew (Brewing Engine), Distill (Distillation Mastery), Status (Volatile Laboratory),
// and the class's one non-Volatile trophy (The Great Work).

/// <summary>Whenever you Infuse, draw a card for every 10 Infused.</summary>
public sealed class Accumulation() : AlchemistCard(2, CardType.Power, CardRarity.Rare, TargetType.Self)
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [new CardsVar(1)];

    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
        [Tip(AlchemistTips.Infuse), HoverTipFactory.FromPower<AccumulationPower>()];

    protected override async Task OnPlay(PlayerChoiceContext ctx, CardPlay play)
    {
        await CreatureCmd.TriggerAnim(Owner.Creature, "Cast", Owner.Character.CastAnimDelay);
        await PowerCmd.Apply<AccumulationPower>(ctx, Owner.Creature,
            DynamicVars.Cards.BaseValue, Owner.Creature, this);
    }

    protected override void OnUpgrade() => EnergyCost.UpgradeBy(-1);
}

/// <summary>The first Potion each combat resolves twice and is consumed once.</summary>
public sealed class EternalCrucible() : AlchemistCard(2, CardType.Power, CardRarity.Rare, TargetType.Self)
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [new PowerVar<EternalCruciblePower>(1m)];

    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
        [HoverTipFactory.FromPower<EternalCruciblePower>()];

    protected override async Task OnPlay(PlayerChoiceContext ctx, CardPlay play)
    {
        await CreatureCmd.TriggerAnim(Owner.Creature, "Cast", Owner.Character.CastAnimDelay);
        await PowerCmd.Apply<EternalCruciblePower>(ctx, Owner.Creature,
            DynamicVars["EternalCruciblePower"].BaseValue, Owner.Creature, this);
    }

    protected override void OnUpgrade() => EnergyCost.UpgradeBy(-1);
}

/// <summary>Whenever you Brew, draw a card.</summary>
public sealed class BrewingEngine() : AlchemistCard(2, CardType.Power, CardRarity.Rare, TargetType.Self)
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [new CardsVar(1)];

    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
        [Tip(AlchemistTips.Brew), HoverTipFactory.FromPower<BrewingEnginePower>()];

    protected override async Task OnPlay(PlayerChoiceContext ctx, CardPlay play)
    {
        await CreatureCmd.TriggerAnim(Owner.Creature, "Cast", Owner.Character.CastAnimDelay);
        await PowerCmd.Apply<BrewingEnginePower>(ctx, Owner.Creature,
            DynamicVars.Cards.BaseValue, Owner.Creature, this);
    }

    protected override void OnUpgrade() => EnergyCost.UpgradeBy(-1);
}

/// <summary>
/// Distilling makes the next Potion you Brew stronger.
///
/// The Brewer's only compounding line, and the reason Distill is not purely a sacrifice.
/// </summary>
public sealed class DistillationMastery() : AlchemistCard(2, CardType.Power, CardRarity.Rare, TargetType.Self)
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [new DynamicVar("Bonus", 50m)];

    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
        [Tip(AlchemistTips.Distill), Tip(AlchemistTips.Brew), HoverTipFactory.FromPower<DistillationMasteryPower>()];

    protected override async Task OnPlay(PlayerChoiceContext ctx, CardPlay play)
    {
        await CreatureCmd.TriggerAnim(Owner.Creature, "Cast", Owner.Character.CastAnimDelay);
        var power = await PowerCmd.Apply<DistillationMasteryPower>(ctx, Owner.Creature, 1m, Owner.Creature, this);

        if (power != null) power.Multiplier = 1m + DynamicVars["Bonus"].BaseValue / 100m;
    }

    protected override void OnUpgrade() => DynamicVars["Bonus"].UpgradeValueBy(25m);
}

/// <summary>Whenever you create a Status, Infuse Unstable Concoction.</summary>
public sealed class VolatileLaboratory() : AlchemistCard(1, CardType.Power, CardRarity.Rare, TargetType.Self)
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [new PowerVar<VolatileLaboratoryPower>(3m)];

    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
        [Tip(AlchemistTips.Infuse), HoverTipFactory.FromPower<VolatileLaboratoryPower>()];

    protected override async Task OnPlay(PlayerChoiceContext ctx, CardPlay play)
    {
        await CreatureCmd.TriggerAnim(Owner.Creature, "Cast", Owner.Character.CastAnimDelay);
        await PowerCmd.Apply<VolatileLaboratoryPower>(ctx, Owner.Creature,
            DynamicVars["VolatileLaboratoryPower"].BaseValue, Owner.Creature, this);
    }

    protected override void OnUpgrade() => DynamicVars["VolatileLaboratoryPower"].UpgradeValueBy(1m);
}

/// <summary>Gain Potency. The class's plainest scaling stat, made permanent.</summary>
public sealed class PotentMixture() : AlchemistCard(1, CardType.Power, CardRarity.Rare, TargetType.Self)
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [new PowerVar<PotencyPower>(3m)];

    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
        [Tip(AlchemistTips.Potency), HoverTipFactory.FromPower<PotencyPower>()];

    protected override async Task OnPlay(PlayerChoiceContext ctx, CardPlay play)
    {
        await CreatureCmd.TriggerAnim(Owner.Creature, "Cast", Owner.Character.CastAnimDelay);
        await AlchemistEffects.GainPotency(ctx, Lab, DynamicVars["PotencyPower"].BaseValue);
    }

    protected override void OnUpgrade() => DynamicVars["PotencyPower"].UpgradeValueBy(2m);
}

/// <summary>
/// Brew the Philosopher's Stone.
///
/// The only source of the Stone, and the Stone is not Volatile — so it survives the fight and can
/// be hoarded for a boss. The one Brew in the whole class that isn't temporary.
/// </summary>
public sealed class TheGreatWork() : AlchemistCard(3, CardType.Power, CardRarity.Rare, TargetType.Self)
{
    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [Tip(AlchemistTips.Brew)];

    protected override async Task OnPlay(PlayerChoiceContext ctx, CardPlay play) =>
        await Belt.Brew(ctx, Lab, ModelDb.Potion<PhilosophersStone>().ToMutable(), volatilePotion: false);
}
