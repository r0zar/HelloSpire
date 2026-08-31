using HelloSpire.HelloSpireCode.Alchemist.Lab;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;

namespace HelloSpire.HelloSpireCode.Alchemist.Cards;

// Uncommon Skills 13-28. Every Transform vector in the class appears at least once in this file:
// card into Gold, card into Potion, Potion into tempo, Gold into Potion, Gold into a card, Gold
// into an Upgrade. This is the tier where the archetypes actually take shape.

/// <summary>Pour a Potion out for two Energy. The Distillation deck's engine.</summary>
public sealed class DistillationColumn() : AlchemistCard(1, CardType.Skill, CardRarity.Uncommon, TargetType.Self)
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [new EnergyVar(2), new CardsVar(0)];

    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];

    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
        [Tip(AlchemistTips.Distill), Tip(AlchemistTips.Transform), EnergyHoverTip];

    protected override async Task OnPlay(PlayerChoiceContext ctx, CardPlay play)
    {
        if (!(await Belt.Distill(ctx, Lab)).Distilled) return;

        await AlchemistEffects.GainEnergy(Lab, DynamicVars.Energy.BaseValue);
        await AlchemistEffects.Draw(ctx, Lab, DynamicVars.Cards.IntValue);
        await AlchemistHooks.NotifyTransformed(ctx, Lab, TransformVector.PotionToTempo);
    }

    protected override void OnUpgrade() => DynamicVars.Cards.UpgradeValueBy(1m);
}

/// <summary>Make another one of something you already drank.</summary>
public sealed class Reconstitute() : AlchemistCard(1, CardType.Skill, CardRarity.Uncommon, TargetType.Self)
{
    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];

    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
        [Tip(AlchemistTips.Brew), Tip(AlchemistTips.Volatile)];

    protected override async Task OnPlay(PlayerChoiceContext ctx, CardPlay play)
    {
        var used = AlchemistEffects.Peek(Lab)?.UsedThisCombat;
        if (used == null || used.Count == 0) return;

        var chosen = used.Count == 1
            ? used[0]
            : await LabBridge.Current.ChoosePotion(ctx, Owner, used);

        await Belt.Brew(ctx, Lab, chosen);
    }

    protected override void OnUpgrade() => EnergyCost.UpgradeBy(-1);
}

/// <summary>Six Gold for a Potion of your choosing. Transform: Gold into Potion.</summary>
public sealed class BuyIngredients() : AlchemistCard(0, CardType.Skill, CardRarity.Uncommon, TargetType.Self)
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [new DynamicVar("Invest", 6m)];

    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];

    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
        [Tip(AlchemistTips.Invest), Tip(AlchemistTips.Brew), Tip(AlchemistTips.Transform)];

    protected override async Task OnPlay(PlayerChoiceContext ctx, CardPlay play)
    {
        if (!await Ledger.Invest(ctx, Lab, DynamicVars["Invest"].IntValue)) return;

        // The design offers a choice of three. Until the bridge can present one, the pick is a
        // straight random draw from the same curated pool — the Gold still bought a Potion.
        if (await Belt.BrewRandom(ctx, Lab) != null)
            await AlchemistHooks.NotifyTransformed(ctx, Lab, TransformVector.GoldToPotion);
    }

    protected override void OnUpgrade() => DynamicVars["Invest"].UpgradeValueBy(-2m);
}

/// <summary>Eight Gold for a card. Transform: Gold into a card.</summary>
public sealed class Commission() : AlchemistCard(1, CardType.Skill, CardRarity.Uncommon, TargetType.Self)
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [new DynamicVar("Invest", 8m)];

    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];

    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
        [Tip(AlchemistTips.Invest), Tip(AlchemistTips.Transform)];

    protected override async Task OnPlay(PlayerChoiceContext ctx, CardPlay play)
    {
        if (!await Ledger.Invest(ctx, Lab, DynamicVars["Invest"].IntValue)) return;

        await Alchemy.Create(ctx, Lab, LabBridge.Current.RandomCard(Owner));
        await AlchemistHooks.NotifyTransformed(ctx, Lab, TransformVector.GoldToCard);
    }

    protected override void OnUpgrade() => DynamicVars["Invest"].UpgradeValueBy(-2m);
}

/// <summary>Upgrade a card for the fight; pay four Gold to Upgrade the whole Hand instead.</summary>
public sealed class FieldUpgrade() : AlchemistCard(1, CardType.Skill, CardRarity.Uncommon, TargetType.Self)
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [new DynamicVar("Invest", 4m)];

    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
        [Tip(AlchemistTips.Invest), Tip(AlchemistTips.Transform)];

    protected override async Task OnPlay(PlayerChoiceContext ctx, CardPlay play)
    {
        if (await Ledger.Invest(ctx, Lab, DynamicVars["Invest"].IntValue))
        {
            await Alchemy.UpgradeHandForCombat(ctx, Lab);
            await AlchemistHooks.NotifyTransformed(ctx, Lab, TransformVector.GoldToUpgrade);
            return;
        }

        await Alchemy.UpgradeOneForCombat(ctx, Lab);
    }

    protected override void OnUpgrade() => EnergyCost.UpgradeBy(-1);
}

/// <summary>Block you can top up a Gold at a time.</summary>
public sealed class GildedGuard() : AlchemistCard(1, CardType.Skill, CardRarity.Uncommon, TargetType.Self)
{
    public override bool GainsBlock => true;

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new BlockVar(7m, ValueProp.Move),
        new BlockVar("PerGold", 2m, ValueProp.Move),
        new DynamicVar("MaxInvest", 5m)
    ];

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [Tip(AlchemistTips.Invest)];

    protected override async Task OnPlay(PlayerChoiceContext ctx, CardPlay play)
    {
        var paid = await Ledger.InvestUpTo(ctx, Lab, DynamicVars["MaxInvest"].IntValue);

        await AlchemistEffects.GainBlock(Lab,
            DynamicVars.Block.BaseValue + DynamicVars["PerGold"].BaseValue * paid);
    }

    protected override void OnUpgrade() => DynamicVars.Block.UpgradeValueBy(3m);
}

/// <summary>
/// Transmute that pays by the pound: the more Energy the card you feed it cost, the more it is
/// worth. Capped, so a stray Curse cannot be turned into a shop trip.
/// </summary>
public sealed class Liquidate() : AlchemistCard(1, CardType.Skill, CardRarity.Uncommon, TargetType.Self)
{
    protected override IEnumerable<DynamicVar> CanonicalVars =>
        [new DynamicVar("Gold", 3m), new DynamicVar("PerEnergy", 2m), new DynamicVar("Cap", 9m)];

    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [Tip(AlchemistTips.Transform)];

    protected override async Task OnPlay(PlayerChoiceContext ctx, CardPlay play)
    {
        var candidates = Alchemy.OtherCardsInHand(Lab);
        if (candidates.Count == 0) return;

        var chosen = await LabBridge.Current.ChooseCard(ctx, Owner, candidates, this);
        if (chosen == null) return;

        var gold = Math.Min(
            DynamicVars["Gold"].IntValue + DynamicVars["PerEnergy"].IntValue * Math.Max(0, chosen.EnergyCost.Canonical),
            DynamicVars["Cap"].IntValue);

        await Alchemy.Exhaust(ctx, Lab, chosen);
        await Ledger.GainGold(ctx, Lab, gold);
        await AlchemistHooks.NotifyTransformed(ctx, Lab, TransformVector.CardToGold);
    }

    protected override void OnUpgrade()
    {
        DynamicVars["Gold"].UpgradeValueBy(1m);
        DynamicVars["Cap"].UpgradeValueBy(2m);
    }
}

/// <summary>Burn a Status or a Curse for an Energy. Answers the thing that clogs this deck worst.</summary>
public sealed class SmeltTheWeak() : AlchemistCard(0, CardType.Skill, CardRarity.Uncommon, TargetType.Self)
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [new EnergyVar(1), new CardsVar(0)];

    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [EnergyHoverTip];

    protected override async Task OnPlay(PlayerChoiceContext ctx, CardPlay play)
    {
        if (!await Alchemy.ExhaustJunk(ctx, Lab)) return;

        await AlchemistEffects.GainEnergy(Lab, DynamicVars.Energy.BaseValue);
        await AlchemistEffects.Draw(ctx, Lab, DynamicVars.Cards.IntValue);
    }

    protected override void OnUpgrade() => DynamicVars.Cards.UpgradeValueBy(1m);
}

/// <summary>Block now, and a Potion if the belt has room for one.</summary>
public sealed class SpareFlask() : AlchemistCard(1, CardType.Skill, CardRarity.Uncommon, TargetType.Self)
{
    public override bool GainsBlock => true;

    protected override IEnumerable<DynamicVar> CanonicalVars => [new BlockVar(5m, ValueProp.Move)];

    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];

    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
        [Tip(AlchemistTips.Brew), Tip(AlchemistTips.Volatile)];

    protected override async Task OnPlay(PlayerChoiceContext ctx, CardPlay play)
    {
        await AlchemistEffects.GainBlock(Lab, DynamicVars.Block.BaseValue);

        if (Belt.EmptySlots(Lab) > 0) await Belt.BrewRandom(ctx, Lab);
    }

    protected override void OnUpgrade() => DynamicVars.Block.UpgradeValueBy(3m);
}

/// <summary>
/// Forty Gold makes a Brewed Potion permanent.
///
/// The permanent tier of the Invest table, and the only way a Volatile Potion ever survives a
/// fight. Priced against a Merchant's Rare Potion on purpose: this should be a decision about the
/// rest of the run, not a combat trick.
/// </summary>
public sealed class Stabilize() : AlchemistCard(1, CardType.Skill, CardRarity.Uncommon, TargetType.Self)
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [new DynamicVar("Invest", 40m)];

    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];

    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
        [Tip(AlchemistTips.Invest), Tip(AlchemistTips.Volatile)];

    protected override async Task OnPlay(PlayerChoiceContext ctx, CardPlay play)
    {
        var bench = await AlchemistEffects.Bench(ctx, Lab);
        if (bench == null || bench.Volatile.Count == 0) return;

        var candidates = bench.Volatile.ToList();
        var chosen = candidates.Count == 1
            ? candidates[0]
            : await LabBridge.Current.ChoosePotion(ctx, Owner, candidates);

        if (chosen == null) return;
        if (!await Ledger.Invest(ctx, Lab, DynamicVars["Invest"].IntValue)) return;

        bench.Volatile.Remove(chosen);
    }

    protected override void OnUpgrade() => DynamicVars["Invest"].UpgradeValueBy(-10m);
}

/// <summary>Block for every slot you are not using. Empty Belt's defence.</summary>
public sealed class SafetyGoggles() : AlchemistCard(1, CardType.Skill, CardRarity.Uncommon, TargetType.Self)
{
    public override bool GainsBlock => true;

    protected override IEnumerable<DynamicVar> CanonicalVars => [new BlockVar("PerSlot", 3m, ValueProp.Move)];

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [Tip(AlchemistTips.ThePotionBelt)];

    protected override async Task OnPlay(PlayerChoiceContext ctx, CardPlay play) =>
        await AlchemistEffects.GainBlock(Lab, DynamicVars["PerSlot"].BaseValue * Belt.EmptySlots(Lab));

    protected override void OnUpgrade() => DynamicVars["PerSlot"].UpgradeValueBy(1m);
}

/// <summary>Three Gold buys a card and makes it better. The cheapest Gold-to-Upgrade in the set.</summary>
public sealed class CostOfKnowledge() : AlchemistCard(0, CardType.Skill, CardRarity.Uncommon, TargetType.Self)
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [new DynamicVar("Invest", 3m), new CardsVar(1)];

    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];

    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
        [Tip(AlchemistTips.Invest), Tip(AlchemistTips.Transform)];

    protected override async Task OnPlay(PlayerChoiceContext ctx, CardPlay play)
    {
        if (!await Ledger.Invest(ctx, Lab, DynamicVars["Invest"].IntValue)) return;

        await AlchemistEffects.Draw(ctx, Lab, DynamicVars.Cards.IntValue);
        await Alchemy.UpgradeOneForCombat(ctx, Lab);
        await AlchemistHooks.NotifyTransformed(ctx, Lab, TransformVector.GoldToUpgrade);
    }

    protected override void OnUpgrade() => DynamicVars["Invest"].UpgradeValueBy(-1m);
}

/// <summary>Trade two dead cards for two live ones. The Exhaust deck's filter.</summary>
public sealed class CatalyticWash() : AlchemistCard(1, CardType.Skill, CardRarity.Uncommon, TargetType.Self)
{
    protected override IEnumerable<DynamicVar> CanonicalVars =>
        [new DynamicVar("Max", 2m), new CardsVar(0)];

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [Tip(AlchemistTips.Transform)];

    protected override async Task OnPlay(PlayerChoiceContext ctx, CardPlay play)
    {
        var exhausted = await Alchemy.ExhaustUpTo(ctx, Lab, DynamicVars["Max"].IntValue);

        await AlchemistEffects.Draw(ctx, Lab, exhausted + DynamicVars.Cards.IntValue);
    }

    protected override void OnUpgrade() => DynamicVars.Cards.UpgradeValueBy(1m);
}

/// <summary>Bury a card, dig two more, and take some Block for having drunk something.</summary>
public sealed class FalseBottom() : AlchemistCard(1, CardType.Skill, CardRarity.Uncommon, TargetType.Self)
{
    public override bool GainsBlock => true;

    protected override IEnumerable<DynamicVar> CanonicalVars =>
        [new CardsVar(2), new BlockVar(4m, ValueProp.Move)];

    protected override async Task OnPlay(PlayerChoiceContext ctx, CardPlay play)
    {
        await Alchemy.BottomOne(ctx, Lab);
        await AlchemistEffects.Draw(ctx, Lab, DynamicVars.Cards.IntValue);

        if ((AlchemistEffects.Peek(Lab)?.PotionsUsedThisTurn ?? 0) > 0)
            await AlchemistEffects.GainBlock(Lab, DynamicVars.Block.BaseValue);
    }

    protected override void OnUpgrade() => EnergyCost.UpgradeBy(-1);
}

/// <summary>Pour a Potion out for Block and a card. Distillation's defensive half.</summary>
public sealed class TinctureTrade() : AlchemistCard(1, CardType.Skill, CardRarity.Uncommon, TargetType.Self)
{
    public override bool GainsBlock => true;

    protected override IEnumerable<DynamicVar> CanonicalVars =>
        [new BlockVar(8m, ValueProp.Move), new CardsVar(1)];

    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
        [Tip(AlchemistTips.Distill), Tip(AlchemistTips.Transform)];

    protected override async Task OnPlay(PlayerChoiceContext ctx, CardPlay play)
    {
        if (!(await Belt.Distill(ctx, Lab)).Distilled) return;

        await AlchemistEffects.GainBlock(Lab, DynamicVars.Block.BaseValue);
        await AlchemistEffects.Draw(ctx, Lab, DynamicVars.Cards.IntValue);
        await AlchemistHooks.NotifyTransformed(ctx, Lab, TransformVector.PotionToTempo);
    }

    protected override void OnUpgrade() => DynamicVars.Block.UpgradeValueBy(3m);
}

/// <summary>Two more slots for this combat. Volatile-only, so it can never bank a found Potion.</summary>
public sealed class VialBandolier() : AlchemistCard(1, CardType.Skill, CardRarity.Uncommon, TargetType.Self)
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [new DynamicVar("Slots", 2m)];

    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];

    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
        [Tip(AlchemistTips.ThePotionBelt), Tip(AlchemistTips.Volatile)];

    protected override async Task OnPlay(PlayerChoiceContext ctx, CardPlay play) =>
        await Belt.GrantTemporarySlots(ctx, Lab, DynamicVars["Slots"].IntValue);

    protected override void OnUpgrade() => EnergyCost.UpgradeBy(-1);
}
