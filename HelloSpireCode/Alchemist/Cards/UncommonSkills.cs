using HelloSpire.HelloSpireCode.Alchemist.Lab;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;

namespace HelloSpire.HelloSpireCode.Alchemist.Cards;

// Uncommon Skills 13-28. This is the tier where the archetypes actually take shape: Distillation,
// Full Belt, Poison and the Unstable Concoction payoffs all get their first real support here.

/// <summary>Pour a Potion out for Potency. The Distillation deck's engine.</summary>
public sealed class DistillationColumn() : AlchemistCard(1, CardType.Skill, CardRarity.Uncommon, TargetType.Self)
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [new PowerVar<PotencyPower>(2m)];

    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];

    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
        [Tip(AlchemistTips.Distill), HoverTipFactory.FromPower<PotencyPower>()];

    protected override async Task OnPlay(PlayerChoiceContext ctx, CardPlay play)
    {
        if (!(await Belt.Distill(ctx, Lab)).Distilled) return;

        await AlchemistEffects.GainPotency(ctx, Lab, DynamicVars["PotencyPower"].BaseValue);
    }

    protected override void OnUpgrade() => EnergyCost.UpgradeBy(-1);
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
            : await LabBridge.Current.ChoosePotion(ctx, Owner, used,
                new LocString("cards", "HELLOSPIRE-ALCHEMIST_RECONSTITUTE_CHOICE.header"));

        // UsedThisCombat holds the instance that was actually drunk -- the game already removed
        // it from the player when it was used, and re-Procuring a consumed instance leaves a dead
        // Potion in the slot (visible, unusable, never cleaned up). Brew a fresh copy instead.
        await Belt.Brew(ctx, Lab, chosen?.CanonicalInstance.ToMutable());
    }

    protected override void OnUpgrade() => EnergyCost.UpgradeBy(-1);
}

/// <summary>Brew a random Common Combat Potion, and a second one if you've already used one this turn.</summary>
public sealed class BuyIngredients() : AlchemistCard(0, CardType.Skill, CardRarity.Uncommon, TargetType.Self)
{
    protected override IEnumerable<IHoverTip> ExtraHoverTips => [Tip(AlchemistTips.Brew)];

    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];

    protected override async Task OnPlay(PlayerChoiceContext ctx, CardPlay play)
    {
        await Belt.BrewRandom(ctx, Lab);

        if ((AlchemistEffects.Peek(Lab)?.PotionsUsedThisTurn ?? 0) > 0)
            await Belt.BrewRandom(ctx, Lab);
    }

    protected override void OnUpgrade() => EnergyCost.UpgradeBy(-1);
}

/// <summary>Create a card in Hand, free for the turn.</summary>
public sealed class Commission() : AlchemistCard(1, CardType.Skill, CardRarity.Uncommon, TargetType.Self)
{
    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];

    protected override async Task OnPlay(PlayerChoiceContext ctx, CardPlay play) =>
        await Alchemy.Create(ctx, Lab, LabBridge.Current.RandomCard(Owner));

    protected override void OnUpgrade() => EnergyCost.UpgradeBy(-1);
}

/// <summary>Upgrade a card for the fight; an empty Potion Slot buys a second Upgrade.</summary>
public sealed class FieldUpgrade() : AlchemistCard(1, CardType.Skill, CardRarity.Uncommon, TargetType.Self)
{
    protected override IEnumerable<IHoverTip> ExtraHoverTips => [Tip(AlchemistTips.ThePotionBelt)];

    protected override async Task OnPlay(PlayerChoiceContext ctx, CardPlay play)
    {
        if (!await Alchemy.UpgradeOneForCombat(ctx, Lab)) return;

        if (Belt.EmptySlots(Lab) > 0)
            await Alchemy.UpgradeOneForCombat(ctx, Lab);
    }

    protected override void OnUpgrade() => EnergyCost.UpgradeBy(-1);
}

/// <summary>Block, more the more Potions you've already used this turn.</summary>
public sealed class GildedGuard() : AlchemistCard(1, CardType.Skill, CardRarity.Uncommon, TargetType.Self)
{
    public override bool GainsBlock => true;

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new BlockVar(7m, ValueProp.Move),
        new BlockVar("PerPotion", 2m, ValueProp.Move)
    ];

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [Tip(AlchemistTips.Infuse)];

    protected override async Task OnPlay(PlayerChoiceContext ctx, CardPlay play)
    {
        await AlchemistEffects.GainBlock(Lab, DynamicVars.Block.BaseValue);

        var used = AlchemistEffects.Peek(Lab)?.PotionsUsedThisTurn ?? 0;
        var bonus = DynamicVars["PerPotion"].BaseValue * used;
        await Belt.Infuse(ctx, Lab, block: bonus);
    }

    protected override void OnUpgrade() => DynamicVars.Block.UpgradeValueBy(3m);
}

/// <summary>
/// Feed it a card; the more Energy it cost, the more it's worth. Uncapped -- feed it whatever you
/// like. A card too cheap to be worth much still draws you one, so this is never a dead card.
/// </summary>
public sealed class Liquidate() : AlchemistCard(1, CardType.Skill, CardRarity.Uncommon, TargetType.Self)
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [new EnergyVar(2), new CardsVar(1)];

    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];

    protected override async Task OnPlay(PlayerChoiceContext ctx, CardPlay play)
    {
        var candidates = Alchemy.OtherCardsInHand(Lab);
        if (candidates.Count == 0) return;

        var chosen = await LabBridge.Current.ChooseCard(ctx, Owner, candidates, this);
        if (chosen == null) return;

        var costly = chosen.EnergyCost.Canonical >= 2;

        await Alchemy.Exhaust(ctx, Lab, chosen);

        if (costly)
            await PowerCmd.Apply<EnergyNextTurnPower>(ctx, Owner.Creature, DynamicVars.Energy.BaseValue, Owner.Creature, this);
        else
            await AlchemistEffects.Draw(ctx, Lab, DynamicVars.Cards.IntValue);
    }

    protected override void OnUpgrade() => DynamicVars.Energy.UpgradeValueBy(1m);
}

/// <summary>Burn a Status or a Curse for an Energy and a little Infuse. Answers the thing that clogs this deck worst.</summary>
public sealed class SmeltTheWeak() : AlchemistCard(0, CardType.Skill, CardRarity.Uncommon, TargetType.Self)
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [new EnergyVar(1), new DamageVar("Bonus", 3m, ValueProp.Move)];

    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [EnergyHoverTip, Tip(AlchemistTips.Infuse)];

    protected override async Task OnPlay(PlayerChoiceContext ctx, CardPlay play)
    {
        if (!await Alchemy.ExhaustJunk(ctx, Lab)) return;

        await AlchemistEffects.GainEnergy(Lab, DynamicVars.Energy.BaseValue);
        await Belt.Infuse(ctx, Lab, damage: DynamicVars["Bonus"].BaseValue);
    }

    protected override void OnUpgrade() => DynamicVars["Bonus"].UpgradeValueBy(2m);
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

/// <summary>Remove Volatile from a held Potion, making it a real, permanent one.</summary>
public sealed class Stabilize() : AlchemistCard(1, CardType.Skill, CardRarity.Uncommon, TargetType.Self)
{
    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [Tip(AlchemistTips.Volatile)];

    protected override async Task OnPlay(PlayerChoiceContext ctx, CardPlay play)
    {
        var bench = await AlchemistEffects.Bench(ctx, Lab);
        if (bench == null || bench.Volatile.Count == 0) return;

        var candidates = bench.Volatile.ToList();
        var chosen = candidates.Count == 1
            ? candidates[0]
            : await LabBridge.Current.ChoosePotion(ctx, Owner, candidates,
                new LocString("cards", "HELLOSPIRE-ALCHEMIST_STABILIZE_CHOICE.header"));

        if (chosen == null) return;
        bench.Volatile.Remove(chosen);
    }

    protected override void OnUpgrade() => EnergyCost.UpgradeBy(-1);
}

/// <summary>Block for every slot you are not using. Empty Belt's defence.</summary>
public sealed class SafetyGoggles() : AlchemistCard(1, CardType.Skill, CardRarity.Uncommon, TargetType.Self)
{
    public override bool GainsBlock => true;

    protected override IEnumerable<DynamicVar> CanonicalVars => [new BlockVar("PerSlot", 4m, ValueProp.Move)];

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [Tip(AlchemistTips.ThePotionBelt)];

    protected override async Task OnPlay(PlayerChoiceContext ctx, CardPlay play) =>
        await AlchemistEffects.GainBlock(Lab, DynamicVars["PerSlot"].BaseValue * Belt.EmptySlots(Lab));

    protected override void OnUpgrade() => DynamicVars["PerSlot"].UpgradeValueBy(1m);
}

/// <summary>Draw a card and Upgrade it for the fight, free.</summary>
public sealed class CostOfKnowledge() : AlchemistCard(0, CardType.Skill, CardRarity.Uncommon, TargetType.Self)
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [new CardsVar(1)];

    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];

    protected override async Task OnPlay(PlayerChoiceContext ctx, CardPlay play)
    {
        var drawn = await CardPileCmd.Draw(ctx, DynamicVars.Cards.IntValue, Owner);
        foreach (var card in drawn)
            await LabBridge.Current.UpgradeForCombat(ctx, Owner, card);
    }

    protected override void OnUpgrade() => DynamicVars.Cards.UpgradeValueBy(1m);
}

/// <summary>Distill a Potion: the next Skill you play is played twice.</summary>
public sealed class CatalyticWash() : AlchemistCard(1, CardType.Skill, CardRarity.Uncommon, TargetType.Self)
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [new PowerVar<SkillReplayPower>(1m)];

    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
        [Tip(AlchemistTips.Distill), HoverTipFactory.FromPower<SkillReplayPower>()];

    protected override async Task OnPlay(PlayerChoiceContext ctx, CardPlay play)
    {
        if (!(await Belt.Distill(ctx, Lab)).Distilled) return;

        await PowerCmd.Apply<SkillReplayPower>(ctx, Owner.Creature,
            DynamicVars["SkillReplayPower"].BaseValue, Owner.Creature, this);
    }

    protected override void OnUpgrade() => EnergyCost.UpgradeBy(-1);
}

/// <summary>Bury a card, dig two more, and Infuse Block for having already drunk something.</summary>
public sealed class FalseBottom() : AlchemistCard(1, CardType.Skill, CardRarity.Uncommon, TargetType.Self)
{
    protected override IEnumerable<DynamicVar> CanonicalVars =>
        [new CardsVar(2), new BlockVar("Bonus", 5m, ValueProp.Move)];

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [Tip(AlchemistTips.Infuse)];

    protected override async Task OnPlay(PlayerChoiceContext ctx, CardPlay play)
    {
        await Alchemy.BottomOne(ctx, Lab);
        await AlchemistEffects.Draw(ctx, Lab, DynamicVars.Cards.IntValue);

        if ((AlchemistEffects.Peek(Lab)?.PotionsUsedThisTurn ?? 0) > 0)
            await Belt.Infuse(ctx, Lab, block: DynamicVars["Bonus"].BaseValue);
    }

    protected override void OnUpgrade() => EnergyCost.UpgradeBy(-1);
}

/// <summary>Pour a Potion out for Block, a card, and a little Infuse. Distillation's defensive half.</summary>
public sealed class TinctureTrade() : AlchemistCard(1, CardType.Skill, CardRarity.Uncommon, TargetType.Self)
{
    public override bool GainsBlock => true;

    protected override IEnumerable<DynamicVar> CanonicalVars =>
        [new BlockVar(6m, ValueProp.Move), new CardsVar(1), new BlockVar("Bonus", 4m, ValueProp.Move)];

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [Tip(AlchemistTips.Distill), Tip(AlchemistTips.Infuse)];

    protected override async Task OnPlay(PlayerChoiceContext ctx, CardPlay play)
    {
        if (!(await Belt.Distill(ctx, Lab)).Distilled) return;

        await AlchemistEffects.GainBlock(Lab, DynamicVars.Block.BaseValue);
        await AlchemistEffects.Draw(ctx, Lab, DynamicVars.Cards.IntValue);
        await Belt.Infuse(ctx, Lab, block: DynamicVars["Bonus"].BaseValue);
    }

    protected override void OnUpgrade() => DynamicVars.Block.UpgradeValueBy(3m);
}

/// <summary>Two more Potion Slots for this combat. Volatile-only, so it can never bank a found Potion.</summary>
public sealed class VialBandolier() : AlchemistCard(1, CardType.Power, CardRarity.Uncommon, TargetType.Self)
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [new DynamicVar("Slots", 2m)];

    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];

    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
        [Tip(AlchemistTips.ThePotionBelt), Tip(AlchemistTips.Volatile)];

    protected override async Task OnPlay(PlayerChoiceContext ctx, CardPlay play) =>
        await Belt.GrantTemporarySlots(ctx, Lab, DynamicVars["Slots"].IntValue);

    protected override void OnUpgrade() => EnergyCost.UpgradeBy(-1);
}
