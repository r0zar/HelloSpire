using HelloSpire.HelloSpireCode.Alchemist.Powers;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;

using HelloSpire.HelloSpireCode.Alchemist.Lab;
using MegaCrit.Sts2.Core.ValueProps;
namespace HelloSpire.HelloSpireCode.Alchemist.Cards;

// Uncommon Powers 29-35. Each attaches a payout to a verb the character already uses every turn —
// Brewing, Exhausting, Investing, drinking, or emptying a slot. None of them changes what you do;
// they change what it is worth.

/// <summary>Gain Potency. The Brewer's only scaling stat.</summary>
public sealed class Concentrate() : AlchemistCard(1, CardType.Power, CardRarity.Uncommon, TargetType.Self)
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [new PowerVar<PotencyPower>(2m)];

    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
        [Tip(AlchemistTips.Potency), HoverTipFactory.FromPower<PotencyPower>()];

    protected override async Task OnPlay(PlayerChoiceContext ctx, CardPlay play)
    {
        await CreatureCmd.TriggerAnim(Owner.Creature, "Cast", Owner.Character.CastAnimDelay);
        await AlchemistEffects.GainPotency(ctx, Lab, DynamicVars["PotencyPower"].BaseValue);
    }

    protected override void OnUpgrade() => DynamicVars["PotencyPower"].UpgradeValueBy(1m);
}

/// <summary>The first Brew each turn is worth Block. Makes a setup turn less of a hole.</summary>
public sealed class HeatBath() : AlchemistCard(1, CardType.Power, CardRarity.Uncommon, TargetType.Self)
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [new PowerVar<HeatBathPower>(4m)];

    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
        [Tip(AlchemistTips.Brew), HoverTipFactory.FromPower<HeatBathPower>()];

    protected override async Task OnPlay(PlayerChoiceContext ctx, CardPlay play)
    {
        await CreatureCmd.TriggerAnim(Owner.Creature, "Cast", Owner.Character.CastAnimDelay);
        await PowerCmd.Apply<HeatBathPower>(ctx, Owner.Creature,
            DynamicVars["HeatBathPower"].BaseValue, Owner.Creature, this);
    }

    protected override void OnUpgrade() => DynamicVars["HeatBathPower"].UpgradeValueBy(2m);
}

/// <summary>
/// The first Exhaust each turn mints a Gold, three times a fight.
///
/// The cap is not a nerf, it is the design. An uncapped in-combat Gold trigger makes stalling the
/// correct play, and a deckbuilder where the best line is "do nothing for four turns" is broken.
/// </summary>
public sealed class CoinPress() : AlchemistCard(1, CardType.Power, CardRarity.Uncommon, TargetType.Self)
{
    protected override IEnumerable<DynamicVar> CanonicalVars =>
        [new PowerVar<CoinPressPower>(1m), new DynamicVar("Triggers", 3m)];

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.FromPower<CoinPressPower>()];

    protected override async Task OnPlay(PlayerChoiceContext ctx, CardPlay play)
    {
        await CreatureCmd.TriggerAnim(Owner.Creature, "Cast", Owner.Character.CastAnimDelay);

        var power = await PowerCmd.Apply<CoinPressPower>(ctx, Owner.Creature,
            DynamicVars["CoinPressPower"].BaseValue, Owner.Creature, this);

        if (power != null) power.TriggersLeft = DynamicVars["Triggers"].IntValue;
    }

    protected override void OnUpgrade() => DynamicVars["Triggers"].UpgradeValueBy(1m);
}

/// <summary>Spending Gold buys Block on the way past.</summary>
public sealed class MerchantsInstinct() : AlchemistCard(1, CardType.Power, CardRarity.Uncommon, TargetType.Self)
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [new PowerVar<MerchantsInstinctPower>(3m)];

    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
        [Tip(AlchemistTips.Invest), HoverTipFactory.FromPower<MerchantsInstinctPower>()];

    protected override async Task OnPlay(PlayerChoiceContext ctx, CardPlay play)
    {
        await CreatureCmd.TriggerAnim(Owner.Creature, "Cast", Owner.Character.CastAnimDelay);
        await PowerCmd.Apply<MerchantsInstinctPower>(ctx, Owner.Creature,
            DynamicVars["MerchantsInstinctPower"].BaseValue, Owner.Creature, this);
    }

    protected override void OnUpgrade() => DynamicVars["MerchantsInstinctPower"].UpgradeValueBy(2m);
}

/// <summary>The first Potion each turn also draws. Turns the belt into a second hand.</summary>
public sealed class ReactiveMixture() : AlchemistCard(1, CardType.Power, CardRarity.Uncommon, TargetType.Self)
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [new PowerVar<ReactiveMixturePower>(1m)];

    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
        [HoverTipFactory.FromPower<ReactiveMixturePower>()];

    protected override async Task OnPlay(PlayerChoiceContext ctx, CardPlay play)
    {
        await CreatureCmd.TriggerAnim(Owner.Creature, "Cast", Owner.Character.CastAnimDelay);

        await PowerCmd.Apply<ReactiveMixturePower>(ctx, Owner.Creature,
            DynamicVars["ReactiveMixturePower"].BaseValue, Owner.Creature, this);
    }

    protected override void OnUpgrade() => DynamicVars["ReactiveMixturePower"].UpgradeValueBy(1m);
}

/// <summary>The first slot emptied each turn is worth Block, whether you drank it or poured it out.</summary>
public sealed class ClosedSystem() : AlchemistCard(1, CardType.Power, CardRarity.Uncommon, TargetType.Self)
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [new PowerVar<ClosedSystemPower>(4m)];

    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
        [Tip(AlchemistTips.ThePotionBelt), HoverTipFactory.FromPower<ClosedSystemPower>()];

    protected override async Task OnPlay(PlayerChoiceContext ctx, CardPlay play)
    {
        await CreatureCmd.TriggerAnim(Owner.Creature, "Cast", Owner.Character.CastAnimDelay);
        await PowerCmd.Apply<ClosedSystemPower>(ctx, Owner.Creature,
            DynamicVars["ClosedSystemPower"].BaseValue, Owner.Creature, this);
    }

    protected override void OnUpgrade() => DynamicVars["ClosedSystemPower"].UpgradeValueBy(2m);
}

/// <summary>Everything you conjure this fight arrives Upgraded. The Gilded Scholar's engine.</summary>
public sealed class RefinersEye() : AlchemistCard(2, CardType.Power, CardRarity.Uncommon, TargetType.Self)
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [new PowerVar<RefinersEyePower>(1m)];

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.FromPower<RefinersEyePower>()];

    protected override async Task OnPlay(PlayerChoiceContext ctx, CardPlay play)
    {
        await CreatureCmd.TriggerAnim(Owner.Creature, "Cast", Owner.Character.CastAnimDelay);
        await PowerCmd.Apply<RefinersEyePower>(ctx, Owner.Creature,
            DynamicVars["RefinersEyePower"].BaseValue, Owner.Creature, this);
    }

    protected override void OnUpgrade() => EnergyCost.UpgradeBy(-1);
}
