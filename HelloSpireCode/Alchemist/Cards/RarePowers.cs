using HelloSpire.HelloSpireCode.Alchemist.Powers;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;

namespace HelloSpire.HelloSpireCode.Alchemist.Cards;

// Rare Powers 21-25. One engine per pool: Unstable Concoction, Potions, Exhaust, Potency.

/// <summary>Whenever you Unleash Unstable Concoction, gain a little Energy. Makes cashing it in even better.</summary>
public sealed class CompoundInterest() : AlchemistCard(1, CardType.Power, CardRarity.Rare, TargetType.Self)
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [new PowerVar<CompoundInterestPower>(1m)];

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.FromPower<CompoundInterestPower>()];

    protected override async Task OnPlay(PlayerChoiceContext ctx, CardPlay play)
    {
        await CreatureCmd.TriggerAnim(Owner.Creature, "Cast", Owner.Character.CastAnimDelay);
        await PowerCmd.Apply<CompoundInterestPower>(ctx, Owner.Creature,
            DynamicVars["CompoundInterestPower"].BaseValue, Owner.Creature, this);
    }

    protected override void OnUpgrade() => DynamicVars["CompoundInterestPower"].UpgradeValueBy(1m);
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

/// <summary>The first Potion you Brew each turn draws a card and grants Block.</summary>
public sealed class BrewingEngine() : AlchemistCard(2, CardType.Power, CardRarity.Rare, TargetType.Self)
{
    protected override IEnumerable<DynamicVar> CanonicalVars =>
        [new CardsVar(1), new BlockVar("Block", 3m, ValueProp.Move)];

    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
        [Tip(AlchemistTips.Brew), HoverTipFactory.FromPower<BrewingEnginePower>()];

    protected override async Task OnPlay(PlayerChoiceContext ctx, CardPlay play)
    {
        await CreatureCmd.TriggerAnim(Owner.Creature, "Cast", Owner.Character.CastAnimDelay);
        var power = await PowerCmd.Apply<BrewingEnginePower>(ctx, Owner.Creature,
            DynamicVars.Cards.BaseValue, Owner.Creature, this);

        if (power != null) power.Block = DynamicVars["Block"].BaseValue;
    }

    protected override void OnUpgrade() => EnergyCost.UpgradeBy(-1);
}

/// <summary>The first card you Exhaust each turn draws a card and Infuses Damage.</summary>
public sealed class ConservationOfMatter() : AlchemistCard(2, CardType.Power, CardRarity.Rare, TargetType.Self)
{
    protected override IEnumerable<DynamicVar> CanonicalVars =>
        [new CardsVar(1), new DamageVar("Infuse", 3m, ValueProp.Move)];

    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
        [Tip(AlchemistTips.Infuse), HoverTipFactory.FromPower<ConservationOfMatterPower>()];

    protected override async Task OnPlay(PlayerChoiceContext ctx, CardPlay play)
    {
        await CreatureCmd.TriggerAnim(Owner.Creature, "Cast", Owner.Character.CastAnimDelay);
        var power = await PowerCmd.Apply<ConservationOfMatterPower>(ctx, Owner.Creature,
            DynamicVars.Cards.BaseValue, Owner.Creature, this);

        if (power != null) power.Infuse = DynamicVars["Infuse"].BaseValue;
    }

    protected override void OnUpgrade() => DynamicVars["Infuse"].UpgradeValueBy(1m);
}

/// <summary>
/// Every Potion you pour out makes the next one stronger, and Infuses a little Poison besides.
///
/// The Brewer's only compounding line, and the reason Distill is not purely a sacrifice. Potency
/// applies to Volatile Potions only, so this never leaks into the Rare Potion a shop happened to
/// sell you.
/// </summary>
public sealed class DistillationMastery() : AlchemistCard(2, CardType.Power, CardRarity.Rare, TargetType.Self)
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [new PowerVar<DistillationMasteryPower>(1m)];

    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
    [
        Tip(AlchemistTips.Distill),
        Tip(AlchemistTips.Potency),
        Tip(AlchemistTips.Infuse),
        HoverTipFactory.FromPower<DistillationMasteryPower>()
    ];

    protected override async Task OnPlay(PlayerChoiceContext ctx, CardPlay play)
    {
        await CreatureCmd.TriggerAnim(Owner.Creature, "Cast", Owner.Character.CastAnimDelay);
        await PowerCmd.Apply<DistillationMasteryPower>(ctx, Owner.Creature,
            DynamicVars["DistillationMasteryPower"].BaseValue, Owner.Creature, this);
    }

    protected override void OnUpgrade() => DynamicVars["DistillationMasteryPower"].UpgradeValueBy(1m);
}
