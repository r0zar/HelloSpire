using HelloSpire.HelloSpireCode.Alchemist.Powers;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;

using MegaCrit.Sts2.Core.ValueProps;
namespace HelloSpire.HelloSpireCode.Alchemist.Cards;

// Rare Powers 21-25. One engine per pool: Gold in, Gold out, Potions, Exhaust, Potency.

/// <summary>Some of what you Invested comes back when the fight ends. Makes spending survivable.</summary>
public sealed class CompoundInterest() : AlchemistCard(1, CardType.Power, CardRarity.Rare, TargetType.Self)
{
    protected override IEnumerable<DynamicVar> CanonicalVars =>
        [new PowerVar<CompoundInterestPower>(1m), new DynamicVar("Percent", 25m)];

    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
        [Tip(AlchemistTips.Invest), HoverTipFactory.FromPower<CompoundInterestPower>()];

    protected override async Task OnPlay(PlayerChoiceContext ctx, CardPlay play)
    {
        await CreatureCmd.TriggerAnim(Owner.Creature, "Cast", Owner.Character.CastAnimDelay);

        var power = await PowerCmd.Apply<CompoundInterestPower>(ctx, Owner.Creature,
            DynamicVars["CompoundInterestPower"].BaseValue, Owner.Creature, this);

        if (power != null) power.Percent = DynamicVars["Percent"].IntValue;
    }

    protected override void OnUpgrade() => DynamicVars["Percent"].UpgradeValueBy(8m);
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

/// <summary>The first Gold each turn draws a card and buys a little Block.</summary>
public sealed class GoldenEngine() : AlchemistCard(2, CardType.Power, CardRarity.Rare, TargetType.Self)
{
    protected override IEnumerable<DynamicVar> CanonicalVars =>
        [new PowerVar<GoldenEnginePower>(1m), new BlockVar("Bonus", 2m, ValueProp.Move)];

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.FromPower<GoldenEnginePower>()];

    protected override async Task OnPlay(PlayerChoiceContext ctx, CardPlay play)
    {
        await CreatureCmd.TriggerAnim(Owner.Creature, "Cast", Owner.Character.CastAnimDelay);

        var power = await PowerCmd.Apply<GoldenEnginePower>(ctx, Owner.Creature,
            DynamicVars["GoldenEnginePower"].BaseValue, Owner.Creature, this);

        if (power != null) power.BlockBonus = DynamicVars["Bonus"].IntValue;
    }

    protected override void OnUpgrade() => DynamicVars["Bonus"].UpgradeValueBy(2m);
}

/// <summary>The first card you burn each turn replaces itself.</summary>
public sealed class ConservationOfMatter() : AlchemistCard(2, CardType.Power, CardRarity.Rare, TargetType.Self)
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [new PowerVar<ConservationOfMatterPower>(1m)];

    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
        [Tip(AlchemistTips.Transform), HoverTipFactory.FromPower<ConservationOfMatterPower>()];

    protected override async Task OnPlay(PlayerChoiceContext ctx, CardPlay play)
    {
        await CreatureCmd.TriggerAnim(Owner.Creature, "Cast", Owner.Character.CastAnimDelay);
        await PowerCmd.Apply<ConservationOfMatterPower>(ctx, Owner.Creature,
            DynamicVars["ConservationOfMatterPower"].BaseValue, Owner.Creature, this);
    }

    protected override void OnUpgrade() => EnergyCost.UpgradeBy(-1);
}

/// <summary>
/// Every Potion you pour out makes the next one stronger.
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
