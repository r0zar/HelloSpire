using MegaCrit.Sts2.Core.CardSelection;
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

// Rare Skills 8-20, including the four Render cards.
//
// Read the Render block below before changing any of it. Max HP in this kit only ever goes down —
// there is no buy-back, by design (design/alchemist.md, Override 1) — so each of these four has to
// buy something Gold cannot buy at any price. If a Render card ever reads as "a bigger Invest",
// it is the wrong card.

/// <summary>
/// Procure a real, persistent Potion.
///
/// The base game's Colorless card, adopted into this pool because the name and the mechanic are
/// too exact to ignore. The major exception to the Volatile rule: this Potion survives combat,
/// which is why it stays Rare and Exhausts.
///
/// TODO(Phase 3): suppress the Colorless copy while playing the Alchemist, or two cards share a
/// name. Flagged as open in design/alchemist.md.
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

/// <summary>
/// Melt the entire rest of your Hand into money.
///
/// The signature Gold card, and the reason the Transmutation deck wants to draw badly. Every dead
/// Strike, every Status, every Curse without Eternal is worth four Gold.
/// </summary>
public sealed class HeavyTransmute() : AlchemistCard(1, CardType.Skill, CardRarity.Rare, TargetType.Self)
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [new DynamicVar("PerCard", 4m)];

    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [Tip(AlchemistTips.Transform)];

    protected override async Task OnPlay(PlayerChoiceContext ctx, CardPlay play)
    {
        var burned = await Alchemy.ExhaustAllOther(ctx, Lab);
        if (burned.Count == 0) return;

        await Ledger.GainGold(ctx, Lab, DynamicVars["PerCard"].IntValue * burned.Count);
        await AlchemistHooks.NotifyTransformed(ctx, Lab, TransformVector.CardToGold);
    }

    protected override void OnUpgrade() => DynamicVars["PerCard"].UpgradeValueBy(1m);
}

/// <summary>Fill the belt. The Brewer's capstone, and the card Potency wants most.</summary>
public sealed class MagnumOpus() : AlchemistCard(3, CardType.Skill, CardRarity.Rare, TargetType.Self)
{
    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];

    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
        [Tip(AlchemistTips.Brew), Tip(AlchemistTips.Volatile), Tip(AlchemistTips.ThePotionBelt)];

    protected override async Task OnPlay(PlayerChoiceContext ctx, CardPlay play)
    {
        await CreatureCmd.TriggerAnim(Owner.Creature, "Cast", Owner.Character.CastAnimDelay);
        await Belt.FillEmpty(ctx, Lab);
    }

    protected override void OnUpgrade() => EnergyCost.UpgradeBy(-1);
}

/// <summary>Fifty Gold for a permanent Upgrade. The top of the Invest table.</summary>
public sealed class Masterwork() : AlchemistCard(2, CardType.Skill, CardRarity.Rare, TargetType.Self)
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [new InvestVar(50m)];

    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];

    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
        [Tip(AlchemistTips.Invest), Tip(AlchemistTips.Transform)];

    protected override async Task OnPlay(PlayerChoiceContext ctx, CardPlay play)
    {
        if (!await Ledger.Invest(ctx, Lab, DynamicVars["Invest"].IntValue, this)) return;

        if (await Alchemy.UpgradeOnePermanently(ctx, Lab))
            await AlchemistHooks.NotifyTransformed(ctx, Lab, TransformVector.GoldToUpgrade);
    }

    protected override void OnUpgrade() => DynamicVars["Invest"].UpgradeValueBy(-10m);
}

/// <summary>Pour a Potion out for a flat Energy-and-card return.</summary>
public sealed class EssenceDistillation() : AlchemistCard(1, CardType.Skill, CardRarity.Uncommon, TargetType.Self)
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [new EnergyVar(2), new CardsVar(1)];

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

    protected override void OnUpgrade()
    {
        DynamicVars.Energy.UpgradeValueBy(1m);
        DynamicVars.Cards.UpgradeValueBy(1m);
    }
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

/// <summary>Ten Gold, right now, for the turn you needed. The emergency button.</summary>
public sealed class GoldStandard() : AlchemistCard(0, CardType.Skill, CardRarity.Rare, TargetType.Self)
{
    protected override IEnumerable<DynamicVar> CanonicalVars =>
        [new InvestVar(10m), new EnergyVar(2), new CardsVar(2)];

    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [Tip(AlchemistTips.Invest), EnergyHoverTip];

    protected override async Task OnPlay(PlayerChoiceContext ctx, CardPlay play)
    {
        if (!await Ledger.Invest(ctx, Lab, DynamicVars["Invest"].IntValue, this)) return;

        await AlchemistEffects.GainEnergy(Lab, DynamicVars.Energy.BaseValue);
        await AlchemistEffects.Draw(ctx, Lab, DynamicVars.Cards.IntValue);
    }

    protected override void OnUpgrade() => DynamicVars["Invest"].UpgradeValueBy(-3m);
}

/// <summary>Three dead cards become Block and three live ones. The Exhaust deck's capstone.</summary>
public sealed class PerfectSolvent() : AlchemistCard(1, CardType.Skill, CardRarity.Rare, TargetType.Self)
{
    public override bool GainsBlock => true;

    protected override IEnumerable<DynamicVar> CanonicalVars =>
        [new BlockVar("PerCard", 5m, ValueProp.Move), new DynamicVar("Max", 3m)];

    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [Tip(AlchemistTips.Transform)];

    protected override async Task OnPlay(PlayerChoiceContext ctx, CardPlay play)
    {
        var burned = await Alchemy.ExhaustUpTo(ctx, Lab, DynamicVars["Max"].IntValue);
        if (burned == 0) return;

        await AlchemistEffects.GainBlock(Lab, DynamicVars["PerCard"].BaseValue * burned);
        await AlchemistEffects.Draw(ctx, Lab, burned);
    }

    protected override void OnUpgrade() => DynamicVars["PerCard"].UpgradeValueBy(1m);
}

/// <summary>
/// Seventy-five Gold for a real, permanent, unrestricted Potion Slot.
///
/// Deliberately asymmetric with Extra Vial and Bandolier: cheap slots are temporary and can only
/// hold Volatile Potions, and the one path to a permanent unrestricted slot costs as much as
/// Masterwork. Permanent resources cost permanent opportunity.
/// </summary>
public sealed class WidenTheBelt() : AlchemistCard(2, CardType.Skill, CardRarity.Rare, TargetType.Self)
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [new InvestVar(75m)];

    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];

    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
        [Tip(AlchemistTips.Invest), Tip(AlchemistTips.ThePotionBelt)];

    protected override async Task OnPlay(PlayerChoiceContext ctx, CardPlay play)
    {
        if (!await Ledger.Invest(ctx, Lab, DynamicVars["Invest"].IntValue, this)) return;

        // Permanent: granted straight through the bridge with no bench record, so the
        // combat-end cleanup never takes it back -- player slots are run state and it persists.
        await LabBridge.Current.GainSlots(Owner, 1);
    }

    protected override void OnUpgrade() => DynamicVars["Invest"].UpgradeValueBy(-15m);
}

// ---------------------------------------------------------------------------- the Render four
//
// Max HP is the only resource in this game that does not come back. There is no Regenerative
// Tincture in this kit and there is not going to be one — a buy-back valve turns a sacrifice into
// a purchase, and a purchase is not a heavy decision.
//
// So all four of these buy something Gold cannot: a copy of a card, a unique object, a permanent
// Upgrade for a player who has already spent their money, or two Energy and two cards on the turn
// the run is otherwise over. Each is optional, none is ever hidden, and declining always resolves
// nothing rather than something bad.
//
// If a fifth card ever wants to call Ledger.Render, that is the signal the mechanic is drifting
// from an event into a template. Four is the number.

/// <summary>Render 4 Max HP to permanently copy a card in your Hand.</summary>
public sealed class HomunculusPact() : AlchemistCard(2, CardType.Skill, CardRarity.Rare, TargetType.Self)
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [new DynamicVar("Render", 4m)];

    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
        [Tip(AlchemistTips.Render), Tip(AlchemistTips.Transform)];

    protected override async Task OnPlay(PlayerChoiceContext ctx, CardPlay play)
    {
        var candidates = Alchemy.OtherCardsInHand(Lab);
        if (candidates.Count == 0) return;

        var chosen = await LabBridge.Current.ChooseCard(ctx, Owner, candidates, this,
            CardSelectorPrefs.TransformSelectionPrompt);
        if (chosen == null) return;

        if (!await Ledger.Render(ctx, Lab, DynamicVars["Render"].IntValue)) return;

        await Alchemy.CreatePermanently(Lab, chosen);
        await AlchemistHooks.NotifyTransformed(ctx, Lab, TransformVector.MaxHpToCard);
    }

    protected override void OnUpgrade() => DynamicVars["Render"].UpgradeValueBy(-1m);
}

/// <summary>
/// Render 6 Max HP to make the Philosopher's Stone.
///
/// The only source of the Stone, and the Stone is not Volatile — so it survives the fight and can
/// be hoarded for a boss. That permanence is what six Max HP is actually buying: not an effect,
/// an object.
/// </summary>
public sealed class TheGreatWork() : AlchemistCard(3, CardType.Skill, CardRarity.Rare, TargetType.Self)
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [new DynamicVar("Render", 4m)];

    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];

    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
        [Tip(AlchemistTips.Render), Tip(AlchemistTips.Brew), Tip(AlchemistTips.Transform)];

    protected override async Task OnPlay(PlayerChoiceContext ctx, CardPlay play)
    {
        if (!await Ledger.Render(ctx, Lab, DynamicVars["Render"].IntValue)) return;

        await Belt.Brew(ctx, Lab, ModelDb.Potion<PhilosophersStone>().ToMutable(), volatilePotion: false);
        await AlchemistHooks.NotifyTransformed(ctx, Lab, TransformVector.MaxHpToPotion);
    }

    protected override void OnUpgrade() => DynamicVars["Render"].UpgradeValueBy(-1m);
}

/// <summary>
/// Render 3 Max HP for two Energy and two cards.
///
/// The cheapest Render, and the one that reads most like a trap. Three Max HP is a Rest Site's
/// worth of healing you will never get back, spent on one turn — correct exactly when that turn
/// is the difference between finishing the fight and not.
/// </summary>
public sealed class EquivalentExchange() : AlchemistCard(0, CardType.Skill, CardRarity.Rare, TargetType.Self)
{
    protected override IEnumerable<DynamicVar> CanonicalVars =>
        [new DynamicVar("Render", 2m), new EnergyVar(2), new CardsVar(2), new PowerVar<PotencyPower>(2m)];

    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];

    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
        [Tip(AlchemistTips.Render), EnergyHoverTip, HoverTipFactory.FromPower<PotencyPower>()];

    protected override async Task OnPlay(PlayerChoiceContext ctx, CardPlay play)
    {
        if (!await Ledger.Render(ctx, Lab, DynamicVars["Render"].IntValue)) return;

        await AlchemistEffects.GainEnergy(Lab, DynamicVars.Energy.BaseValue);
        await AlchemistEffects.Draw(ctx, Lab, DynamicVars.Cards.IntValue);
        await AlchemistEffects.GainPotency(ctx, Lab, DynamicVars["PotencyPower"].BaseValue);
    }

    protected override void OnUpgrade() => DynamicVars["Render"].UpgradeValueBy(-1m);
}

/// <summary>
/// Render 5 Max HP for a permanent Upgrade.
///
/// Masterwork for a player who has already spent their Gold, and the cleanest statement the
/// mechanic can make: a permanent improvement to the deck, paid for with a permanent reduction of
/// the player. Same output as fifty Gold, in the currency that never comes back.
/// </summary>
public sealed class TransmuteFlesh() : AlchemistCard(2, CardType.Skill, CardRarity.Rare, TargetType.Self)
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [new DynamicVar("Render", 3m)];

    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];

    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
        [Tip(AlchemistTips.Render), Tip(AlchemistTips.Transform)];

    protected override async Task OnPlay(PlayerChoiceContext ctx, CardPlay play)
    {
        if (Alchemy.OtherCardsInHand(Lab).Count == 0) return;
        if (!await Ledger.Render(ctx, Lab, DynamicVars["Render"].IntValue)) return;

        if (await Alchemy.UpgradeOnePermanently(ctx, Lab))
            await AlchemistHooks.NotifyTransformed(ctx, Lab, TransformVector.MaxHpToUpgrade);
    }

    protected override void OnUpgrade() => DynamicVars["Render"].UpgradeValueBy(-1m);
}
