using HelloSpire.HelloSpireCode.Alchemist.Lab;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Potions;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;

namespace HelloSpire.HelloSpireCode.Alchemist.Cards;

// Rare Skills 1-8. The Belt's utility ceiling: Alchemize/Widen the Belt/Extra-Vial-style slot and
// Potion access, plus the two "empty your Hand" capstones (Heavy Transmute, Perfect Solvent).

/// <summary>
/// Brew a real Rare Potion -- and a real one, not Volatile: it survives past the end of combat
/// like <see cref="TheGreatWork"/>'s Philosopher's Stone.
///
/// The base game's Colorless card, adopted into this pool because the name and the mechanic are
/// too exact to ignore.
/// </summary>
public sealed class Alchemize() : AlchemistCard(1, CardType.Skill, CardRarity.Rare, TargetType.Self)
{
    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [Tip(AlchemistTips.Brew)];

    protected override async Task OnPlay(PlayerChoiceContext ctx, CardPlay play)
    {
        await CreatureCmd.TriggerAnim(Owner.Creature, "Cast", Owner.Character.CastAnimDelay);
        await Belt.BrewRandom(ctx, Lab, PotionRarity.Rare, volatilePotion: false);
    }

    protected override void OnUpgrade() => EnergyCost.UpgradeBy(-1);
}

/// <summary>Melt the entire rest of your Hand into Unstable Concoction.</summary>
public sealed class HeavyTransmute() : AlchemistCard(1, CardType.Skill, CardRarity.Rare, TargetType.Self)
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [new DamageVar("PerCard", 6m, ValueProp.Move)];

    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [Tip(AlchemistTips.Infuse)];

    protected override async Task OnPlay(PlayerChoiceContext ctx, CardPlay play)
    {
        var burned = await Alchemy.ExhaustAllOther(ctx, Lab);
        if (burned.Count == 0) return;

        await Belt.Infuse(ctx, Lab, damage: DynamicVars["PerCard"].BaseValue * burned.Count);
    }

    protected override void OnUpgrade() => DynamicVars["PerCard"].UpgradeValueBy(1m);
}

/// <summary>Fill the belt, and Infuse for every Potion that landed. The Brewer's capstone.</summary>
public sealed class MagnumOpus() : AlchemistCard(3, CardType.Skill, CardRarity.Rare, TargetType.Self)
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [new DamageVar("PerPotion", 4m, ValueProp.Move)];

    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];

    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
        [Tip(AlchemistTips.Brew), Tip(AlchemistTips.Volatile), Tip(AlchemistTips.ThePotionBelt), Tip(AlchemistTips.Infuse)];

    protected override async Task OnPlay(PlayerChoiceContext ctx, CardPlay play)
    {
        await CreatureCmd.TriggerAnim(Owner.Creature, "Cast", Owner.Character.CastAnimDelay);
        var filled = await Belt.FillEmpty(ctx, Lab);
        if (filled > 0) await Belt.Infuse(ctx, Lab, damage: DynamicVars["PerPotion"].BaseValue * filled);
    }

    protected override void OnUpgrade() => DynamicVars["PerPotion"].UpgradeValueBy(1m);
}

/// <summary>Gain Potency, and draw cards.</summary>
public sealed class EssenceDistillation() : AlchemistCard(1, CardType.Skill, CardRarity.Rare, TargetType.Self)
{
    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];

    protected override IEnumerable<DynamicVar> CanonicalVars =>
        [new PowerVar<PotencyPower>(2m), new CardsVar(2)];

    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
        [Tip(AlchemistTips.Potency), HoverTipFactory.FromPower<PotencyPower>()];

    protected override async Task OnPlay(PlayerChoiceContext ctx, CardPlay play)
    {
        await AlchemistEffects.GainPotency(ctx, Lab, DynamicVars["PotencyPower"].BaseValue);
        await AlchemistEffects.Draw(ctx, Lab, DynamicVars.Cards.IntValue);
    }

    protected override void OnUpgrade() => EnergyCost.UpgradeBy(-1);
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

/// <summary>Every other card in Hand becomes Block and cards. The Exhaust deck's capstone.</summary>
public sealed class PerfectSolvent() : AlchemistCard(1, CardType.Skill, CardRarity.Rare, TargetType.Self)
{
    public override bool GainsBlock => true;

    protected override IEnumerable<DynamicVar> CanonicalVars => [new BlockVar("PerCard", 5m, ValueProp.Move)];

    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];

    protected override async Task OnPlay(PlayerChoiceContext ctx, CardPlay play)
    {
        var burned = (await Alchemy.ExhaustAllOther(ctx, Lab)).Count;
        if (burned == 0) return;

        await AlchemistEffects.GainBlock(Lab, DynamicVars["PerCard"].BaseValue * burned);
        await AlchemistEffects.Draw(ctx, Lab, burned);
    }

    protected override void OnUpgrade() => DynamicVars["PerCard"].UpgradeValueBy(1m);
}

/// <summary>
/// A real, permanent Potion Slot -- no cost gate, just the Energy and the Exhaust.
///
/// Asymmetric with Extra Vial: cheap Slots are temporary, this one is not. Both accept any
/// Potion equally now; the difference is purely how long the Slot lasts.
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

/// <summary>A big dose of Poison, a big dose of Poison Infuse, a Poison Potion, and a Volatile Reagent besides.</summary>
public sealed class Overdose() : AlchemistCard(2, CardType.Skill, CardRarity.Rare, TargetType.AnyEnemy)
{
    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];

    protected override IEnumerable<DynamicVar> CanonicalVars =>
        [new DynamicVar("Poison", 10m), new DynamicVar("Bonus", 10m)];

    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
        [HoverTipFactory.FromPower<PoisonPower>(), Tip(AlchemistTips.Infuse), Tip(AlchemistTips.Brew)];

    protected override async Task OnPlay(PlayerChoiceContext ctx, CardPlay play)
    {
        ArgumentNullException.ThrowIfNull(play.Target);

        await AlchemistEffects.ApplyPoison(ctx, Lab, play.Target, DynamicVars["Poison"].BaseValue);
        await Belt.Infuse(ctx, Lab, poison: DynamicVars["Bonus"].BaseValue);
        await Belt.Brew(ctx, Lab, LabBridge.Current.NamedPotion(BasePotion.Poison));
        await Alchemy.CreateVolatileReagent(ctx, Lab, PileType.Discard);
    }

    protected override void OnUpgrade() => DynamicVars["Poison"].UpgradeValueBy(2m);
}
