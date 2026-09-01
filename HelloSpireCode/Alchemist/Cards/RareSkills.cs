using MegaCrit.Sts2.Core.CardSelection;
using HelloSpire.HelloSpireCode.Alchemist.Lab;
using HelloSpire.HelloSpireCode.Alchemist.Potions;
using HelloSpire.HelloSpireCode.Alchemist.Powers;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Potions;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;

namespace HelloSpire.HelloSpireCode.Alchemist.Cards;

// Rare Skills 8-20. Almost every one of these Infuses Unstable Concoction or applies Poison on top
// of an effect that used to cost Gold or Max HP -- the top of the curve is now built entirely out
// of the same Potion/Distill/Exhaust verbs the rest of the class already teaches.

/// <summary>
/// Procure a real, persistent Potion.
///
/// The base game's Colorless card, adopted into this pool because the name and the mechanic are
/// too exact to ignore. The major exception to the Volatile rule: this Potion survives combat,
/// which is why it stays Rare and Exhausts.
/// </summary>
public sealed class Alchemize() : AlchemistCard(1, CardType.Skill, CardRarity.Rare, TargetType.Self)
{
    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [Tip(AlchemistTips.Brew)];

    protected override async Task OnPlay(PlayerChoiceContext ctx, CardPlay play)
    {
        await CreatureCmd.TriggerAnim(Owner.Creature, "Cast", Owner.Character.CastAnimDelay);

        // Not Volatile: Procured Potions are real inventory.
        await Belt.Brew(ctx, Lab, LabBridge.Current.RandomCombatPotion(Owner, null), volatilePotion: false);
    }

    protected override void OnUpgrade() => EnergyCost.UpgradeBy(-1);
}

/// <summary>Melt the entire rest of your Hand into Master Brew.</summary>
public sealed class HeavyTransmute() : AlchemistCard(1, CardType.Skill, CardRarity.Rare, TargetType.Self)
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [new DamageVar("PerCard", 6m, ValueProp.Move)];

    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [Tip(AlchemistTips.Infuse)];

    protected override async Task OnPlay(PlayerChoiceContext ctx, CardPlay play)
    {
        var burned = await Alchemy.ExhaustAllOther(ctx, Lab);
        if (burned.Count == 0) return;

        Belt.Infuse(Lab, damage: DynamicVars["PerCard"].BaseValue * burned.Count);
    }

    protected override void OnUpgrade() => DynamicVars["PerCard"].UpgradeValueBy(1m);
}

/// <summary>Fill the belt, and Infuse for every Potion that landed. The Brewer's capstone.</summary>
public sealed class MagnumOpus() : AlchemistCard(3, CardType.Skill, CardRarity.Rare, TargetType.Self)
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [new DamageVar("PerPotion", 5m, ValueProp.Move)];

    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];

    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
        [Tip(AlchemistTips.Brew), Tip(AlchemistTips.Volatile), Tip(AlchemistTips.ThePotionBelt), Tip(AlchemistTips.Infuse)];

    protected override async Task OnPlay(PlayerChoiceContext ctx, CardPlay play)
    {
        await CreatureCmd.TriggerAnim(Owner.Creature, "Cast", Owner.Character.CastAnimDelay);
        var filled = await Belt.FillEmpty(ctx, Lab);
        if (filled > 0) Belt.Infuse(Lab, damage: DynamicVars["PerPotion"].BaseValue * filled);
    }

    protected override void OnUpgrade() => DynamicVars["PerPotion"].UpgradeValueBy(1m);
}

/// <summary>Permanently Upgrade a card in Hand. The top of the class's Upgrade line, free of any cost gate.</summary>
public sealed class Masterwork() : AlchemistCard(2, CardType.Skill, CardRarity.Rare, TargetType.Self)
{
    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];

    protected override async Task OnPlay(PlayerChoiceContext ctx, CardPlay play) =>
        await Alchemy.UpgradeOnePermanently(ctx, Lab);

    protected override void OnUpgrade() => EnergyCost.UpgradeBy(-1);
}

/// <summary>Pour a Potion out; the better the Potion, the better the return.</summary>
public sealed class EssenceDistillation() : AlchemistCard(1, CardType.Skill, CardRarity.Rare, TargetType.Self)
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [new DamageVar("Bonus", 5m, ValueProp.Move)];

    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];

    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
        [Tip(AlchemistTips.Distill), Tip(AlchemistTips.Infuse)];

    protected override async Task OnPlay(PlayerChoiceContext ctx, CardPlay play)
    {
        var result = await Belt.Distill(ctx, Lab);
        if (!result.Distilled) return;

        var cards = result.Rarity switch
        {
            PotionRarity.Rare => 3,
            PotionRarity.Uncommon => 2,
            _ => 1
        };

        await AlchemistEffects.Draw(ctx, Lab, cards);

        if (result.Rarity == PotionRarity.Rare)
            Belt.Infuse(Lab, damage: DynamicVars["Bonus"].BaseValue);
    }

    protected override void OnUpgrade() => DynamicVars["Bonus"].UpgradeValueBy(2m);
}

/// <summary>The next Potion this turn is not consumed. The best line in the class, and it knows it.</summary>
public sealed class BottledTime() : AlchemistCard(1, CardType.Skill, CardRarity.Rare, TargetType.Self)
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [new PowerVar<BottledTimePower>(1m)];

    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.FromPower<BottledTimePower>()];

    protected override async Task OnPlay(PlayerChoiceContext ctx, CardPlay play)
    {
        await CreatureCmd.TriggerAnim(Owner.Creature, "Cast", Owner.Character.CastAnimDelay);
        await PowerCmd.Apply<BottledTimePower>(ctx, Owner.Creature,
            DynamicVars["BottledTimePower"].BaseValue, Owner.Creature, this);
    }

    protected override void OnUpgrade() => EnergyCost.UpgradeBy(-1);
}

/// <summary>Energy and cards, free, plus a healthy Infuse. The emergency button.</summary>
public sealed class GoldStandard() : AlchemistCard(0, CardType.Skill, CardRarity.Rare, TargetType.Self)
{
    protected override IEnumerable<DynamicVar> CanonicalVars =>
        [new EnergyVar(1), new CardsVar(2), new DynamicVar("Energy", 6m)];

    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [EnergyHoverTip, Tip(AlchemistTips.Infuse)];

    protected override async Task OnPlay(PlayerChoiceContext ctx, CardPlay play)
    {
        await AlchemistEffects.GainEnergy(Lab, DynamicVars.Energy.BaseValue);
        await AlchemistEffects.Draw(ctx, Lab, DynamicVars.Cards.IntValue);
        Belt.Infuse(Lab, energy: DynamicVars["Energy"].BaseValue);
    }

    protected override void OnUpgrade() => DynamicVars.Cards.UpgradeValueBy(1m);
}

/// <summary>Every other card in Hand becomes Block, cards, and Infuse. The Exhaust deck's capstone.</summary>
public sealed class PerfectSolvent() : AlchemistCard(1, CardType.Skill, CardRarity.Rare, TargetType.Self)
{
    public override bool GainsBlock => true;

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new BlockVar("PerCard", 5m, ValueProp.Move),
        new BlockVar("Infuse", 4m, ValueProp.Move)
    ];

    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [Tip(AlchemistTips.Infuse)];

    protected override async Task OnPlay(PlayerChoiceContext ctx, CardPlay play)
    {
        var burned = (await Alchemy.ExhaustAllOther(ctx, Lab)).Count;
        if (burned == 0) return;

        await AlchemistEffects.GainBlock(Lab, DynamicVars["PerCard"].BaseValue * burned);
        await AlchemistEffects.Draw(ctx, Lab, burned);
        Belt.Infuse(Lab, block: DynamicVars["Infuse"].BaseValue * burned);
    }

    protected override void OnUpgrade() => DynamicVars["PerCard"].UpgradeValueBy(1m);
}

/// <summary>
/// A real, permanent, unrestricted Potion Slot -- no cost gate, just the Energy and the Exhaust.
///
/// Deliberately asymmetric with Extra Vial and Bandolier: cheap slots are temporary and can only
/// hold Volatile Potions; this is the one path to a real one.
/// </summary>
public sealed class WidenTheBelt() : AlchemistCard(2, CardType.Skill, CardRarity.Rare, TargetType.Self)
{
    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [Tip(AlchemistTips.ThePotionBelt)];

    protected override async Task OnPlay(PlayerChoiceContext ctx, CardPlay play) =>
        // Permanent: granted straight through the bridge with no bench record, so the
        // combat-end cleanup never takes it back -- player slots are run state and it persists.
        await LabBridge.Current.GainSlots(Owner, 1);

    protected override void OnUpgrade() => EnergyCost.UpgradeBy(-1);
}

/// <summary>Copy a card in Hand, free for the turn, and Infuse a little Damage besides.</summary>
public sealed class HomunculusPact() : AlchemistCard(2, CardType.Skill, CardRarity.Rare, TargetType.Self)
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [new DamageVar("Bonus", 8m, ValueProp.Move)];

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [Tip(AlchemistTips.Infuse)];

    protected override async Task OnPlay(PlayerChoiceContext ctx, CardPlay play)
    {
        var candidates = Alchemy.OtherCardsInHand(Lab);
        if (candidates.Count == 0) return;

        var chosen = await LabBridge.Current.ChooseCard(ctx, Owner, candidates, this,
            CardSelectorPrefs.TransformSelectionPrompt);
        if (chosen == null) return;

        await Alchemy.Create(ctx, Lab, chosen.CreateClone());
        Belt.Infuse(Lab, damage: DynamicVars["Bonus"].BaseValue);
    }

    protected override void OnUpgrade() => EnergyCost.UpgradeBy(-1);
}

/// <summary>
/// Brew the Philosopher's Stone, and Infuse a big chunk of Damage besides.
///
/// The only source of the Stone, and the Stone is not Volatile — so it survives the fight and can
/// be hoarded for a boss.
/// </summary>
public sealed class TheGreatWork() : AlchemistCard(3, CardType.Skill, CardRarity.Rare, TargetType.Self)
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [new DamageVar("Bonus", 15m, ValueProp.Move)];

    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [Tip(AlchemistTips.Brew), Tip(AlchemistTips.Infuse)];

    protected override async Task OnPlay(PlayerChoiceContext ctx, CardPlay play)
    {
        await Belt.Brew(ctx, Lab, ModelDb.Potion<PhilosophersStone>().ToMutable(), volatilePotion: false);
        Belt.Infuse(Lab, damage: DynamicVars["Bonus"].BaseValue);
    }

    protected override void OnUpgrade() => DynamicVars["Bonus"].UpgradeValueBy(3m);
}

/// <summary>Energy and cards, and a healthy Infuse. Free.</summary>
public sealed class EquivalentExchange() : AlchemistCard(0, CardType.Skill, CardRarity.Rare, TargetType.Self)
{
    protected override IEnumerable<DynamicVar> CanonicalVars =>
        [new EnergyVar(2), new CardsVar(2), new DamageVar("Bonus", 5m, ValueProp.Move)];

    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [EnergyHoverTip, Tip(AlchemistTips.Infuse)];

    protected override async Task OnPlay(PlayerChoiceContext ctx, CardPlay play)
    {
        await AlchemistEffects.GainEnergy(Lab, DynamicVars.Energy.BaseValue);
        await AlchemistEffects.Draw(ctx, Lab, DynamicVars.Cards.IntValue);
        Belt.Infuse(Lab, damage: DynamicVars["Bonus"].BaseValue);
    }

    protected override void OnUpgrade() => DynamicVars.Cards.UpgradeValueBy(1m);
}

/// <summary>
/// Permanently Upgrade a card in Hand, and apply Poison on the way out.
///
/// Masterwork's little cousin, with a debuff riding along since there's no cost left to gate it.
/// </summary>
public sealed class TransmuteFlesh() : AlchemistCard(2, CardType.Skill, CardRarity.Rare, TargetType.AnyEnemy)
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [new DynamicVar("Poison", 5m)];

    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.FromPower<PoisonPower>()];

    protected override async Task OnPlay(PlayerChoiceContext ctx, CardPlay play)
    {
        ArgumentNullException.ThrowIfNull(play.Target);
        if (Alchemy.OtherCardsInHand(Lab).Count == 0) return;

        await Alchemy.UpgradeOnePermanently(ctx, Lab);
        await AlchemistEffects.ApplyPoison(ctx, Lab, play.Target, DynamicVars["Poison"].BaseValue);
    }

    protected override void OnUpgrade() => DynamicVars["Poison"].UpgradeValueBy(2m);
}
