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

// Uncommon Skills 1-16. Distill's payoffs live here (Block, Energy, Infuse, draw, Poison), plus
// the Belt's own utility (Extra Vial, Stabilize, Reconstitute) and the Status sub-theme's first
// real payoff cards (False Bottom, Reagent Recovery).

/// <summary>Distill a Potion for Energy.</summary>
public sealed class DistillationColumn() : AlchemistCard(1, CardType.Skill, CardRarity.Uncommon, TargetType.Self)
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [new DynamicVar("Energy", 2m)];

    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [Tip(AlchemistTips.Distill)];

    protected override async Task OnPlay(PlayerChoiceContext ctx, CardPlay play)
    {
        if ((await Belt.Distill(ctx, Lab)).Distilled)
            await AlchemistEffects.GainEnergy(Lab, DynamicVars["Energy"].BaseValue);
    }

    protected override void OnUpgrade() => DynamicVars["Energy"].UpgradeValueBy(1m);
}

/// <summary>Return a card from your Exhaust pile to your hand, then Exhaust.</summary>
public sealed class Reconstitute() : AlchemistCard(1, CardType.Skill, CardRarity.Uncommon, TargetType.Self)
{
    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];

    protected override async Task OnPlay(PlayerChoiceContext ctx, CardPlay play) =>
        await Alchemy.ReturnFromExhaust(ctx, Lab);
}

/// <summary>Gain Block, and Brew a random Potion.</summary>
public sealed class SpareFlask() : AlchemistCard(1, CardType.Skill, CardRarity.Uncommon, TargetType.Self)
{
    public override bool GainsBlock => true;

    protected override IEnumerable<DynamicVar> CanonicalVars => [new BlockVar(5m, ValueProp.Move)];

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [Tip(AlchemistTips.Brew)];

    protected override async Task OnPlay(PlayerChoiceContext ctx, CardPlay play)
    {
        await AlchemistEffects.GainBlock(Lab, DynamicVars.Block.BaseValue);
        await Belt.BrewRandom(ctx, Lab);
    }

    protected override void OnUpgrade() => DynamicVars.Block.UpgradeValueBy(3m);
}

/// <summary>
/// One more Potion Slot for this combat.
///
/// The Slot is Volatile-only, which is the entire reason it is temporary — a Volatile Potion is
/// removed at combat end anyway, so no separate expiry rule is needed. It also means this can
/// never be used to bank a found Rare Potion.
/// </summary>
public sealed class ExtraVial() : AlchemistCard(1, CardType.Skill, CardRarity.Uncommon, TargetType.Self)
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [new DynamicVar("Slots", 1m)];

    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];

    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
        [Tip(AlchemistTips.ThePotionBelt), Tip(AlchemistTips.Volatile)];

    protected override async Task OnPlay(PlayerChoiceContext ctx, CardPlay play) =>
        await Belt.GrantTemporarySlots(ctx, Lab, DynamicVars["Slots"].IntValue);

    protected override void OnUpgrade() => EnergyCost.UpgradeBy(-1);
}

/// <summary>Make a held Potion permanent.</summary>
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
}

/// <summary>Distill a Potion, and Infuse Unstable Concoction.</summary>
public sealed class CatalyticWash() : AlchemistCard(1, CardType.Skill, CardRarity.Uncommon, TargetType.Self)
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [new DamageVar("Bonus", 8m, ValueProp.Move)];

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [Tip(AlchemistTips.Distill), Tip(AlchemistTips.Infuse)];

    protected override async Task OnPlay(PlayerChoiceContext ctx, CardPlay play)
    {
        if ((await Belt.Distill(ctx, Lab)).Distilled)
            await Belt.Infuse(ctx, Lab, damage: DynamicVars["Bonus"].BaseValue);
    }

    protected override void OnUpgrade() => DynamicVars["Bonus"].UpgradeValueBy(2m);
}

/// <summary>Distill a Potion for Block and Energy.</summary>
public sealed class TinctureTrade() : AlchemistCard(1, CardType.Skill, CardRarity.Uncommon, TargetType.Self)
{
    public override bool GainsBlock => true;

    protected override IEnumerable<DynamicVar> CanonicalVars =>
        [new BlockVar(6m, ValueProp.Move), new DynamicVar("Energy", 1m)];

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [Tip(AlchemistTips.Distill)];

    protected override async Task OnPlay(PlayerChoiceContext ctx, CardPlay play)
    {
        if (!(await Belt.Distill(ctx, Lab)).Distilled) return;

        await AlchemistEffects.GainBlock(Lab, DynamicVars.Block.BaseValue);
        await AlchemistEffects.GainEnergy(Lab, DynamicVars["Energy"].BaseValue);
    }

    protected override void OnUpgrade() => DynamicVars.Block.UpgradeValueBy(2m);
}

/// <summary>Exhaust a card, for Energy next turn.</summary>
public sealed class Liquidate() : AlchemistCard(1, CardType.Skill, CardRarity.Uncommon, TargetType.Self)
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [new DynamicVar("Energy", 2m)];

    protected override async Task OnPlay(PlayerChoiceContext ctx, CardPlay play)
    {
        if (!await Alchemy.ExhaustOne(ctx, Lab)) return;
        await PowerCmd.Apply<EnergyNextTurnPower>(ctx, Owner.Creature,
            DynamicVars["Energy"].BaseValue, Owner.Creature, this);
    }

    protected override void OnUpgrade() => DynamicVars["Energy"].UpgradeValueBy(1m);
}

/// <summary>Exhaust a Status or Curse, for Energy and Infuse.</summary>
public sealed class SmeltTheWeak() : AlchemistCard(0, CardType.Skill, CardRarity.Uncommon, TargetType.Self)
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [new DamageVar("Infuse", 4m, ValueProp.Move)];

    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [Tip(AlchemistTips.Infuse)];

    protected override async Task OnPlay(PlayerChoiceContext ctx, CardPlay play)
    {
        if (!await Alchemy.ExhaustJunk(ctx, Lab)) return;

        await AlchemistEffects.GainEnergy(Lab, 1m);
        await Belt.Infuse(ctx, Lab, damage: DynamicVars["Infuse"].BaseValue);
    }
}

/// <summary>Leave a Volatile Reagent in the discard pile, and draw two cards.</summary>
public sealed class FalseBottom() : AlchemistCard(1, CardType.Skill, CardRarity.Uncommon, TargetType.Self)
{
    protected override async Task OnPlay(PlayerChoiceContext ctx, CardPlay play)
    {
        await Alchemy.CreateVolatileReagent(ctx, Lab, PileType.Discard);
        await AlchemistEffects.Draw(ctx, Lab, 2);
    }
}

/// <summary>Gain Block. If you have no Potions, draw two cards.</summary>
public sealed class BrewUnderPressure() : AlchemistCard(1, CardType.Skill, CardRarity.Uncommon, TargetType.Self)
{
    public override bool GainsBlock => true;

    protected override IEnumerable<DynamicVar> CanonicalVars => [new BlockVar(10m, ValueProp.Move)];

    protected override async Task OnPlay(PlayerChoiceContext ctx, CardPlay play)
    {
        await AlchemistEffects.GainBlock(Lab, DynamicVars.Block.BaseValue);

        if (Belt.Held(Lab).Count == 0)
            await AlchemistEffects.Draw(ctx, Lab, 2);
    }

    protected override void OnUpgrade() => DynamicVars.Block.UpgradeValueBy(3m);
}

/// <summary>Distill a Potion, and apply Poison.</summary>
public sealed class ToxicDistillate() : AlchemistCard(1, CardType.Skill, CardRarity.Uncommon, TargetType.AnyEnemy)
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [new DynamicVar("Poison", 5m)];

    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
        [Tip(AlchemistTips.Distill), HoverTipFactory.FromPower<PoisonPower>()];

    protected override async Task OnPlay(PlayerChoiceContext ctx, CardPlay play)
    {
        ArgumentNullException.ThrowIfNull(play.Target);

        if ((await Belt.Distill(ctx, Lab)).Distilled)
            await AlchemistEffects.ApplyPoison(ctx, Lab, play.Target, DynamicVars["Poison"].BaseValue);
    }

    protected override void OnUpgrade() => DynamicVars["Poison"].UpgradeValueBy(2m);
}

/// <summary>Brew a Poison Potion and a Weak Potion.</summary>
public sealed class ReactiveMixture() : AlchemistCard(1, CardType.Skill, CardRarity.Uncommon, TargetType.Self)
{
    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [Tip(AlchemistTips.Brew)];

    protected override async Task OnPlay(PlayerChoiceContext ctx, CardPlay play)
    {
        await Belt.Brew(ctx, Lab, LabBridge.Current.NamedPotion(BasePotion.Poison));
        await Belt.Brew(ctx, Lab, LabBridge.Current.NamedPotion(BasePotion.Weak));
    }
}

/// <summary>Exhaust a Status from the discard pile, for cards and Block.</summary>
public sealed class ReagentRecovery() : AlchemistCard(1, CardType.Skill, CardRarity.Uncommon, TargetType.Self)
{
    public override bool GainsBlock => true;

    protected override IEnumerable<DynamicVar> CanonicalVars => [new BlockVar(4m, ValueProp.Move)];

    protected override async Task OnPlay(PlayerChoiceContext ctx, CardPlay play)
    {
        if (!await Alchemy.ExhaustJunkFromDiscard(ctx, Lab)) return;

        await AlchemistEffects.Draw(ctx, Lab, 2);
        await AlchemistEffects.GainBlock(Lab, DynamicVars.Block.BaseValue);
    }

    protected override void OnUpgrade() => DynamicVars.Block.UpgradeValueBy(2m);
}

/// <summary>Gain Potency, and Distill a Potion. The class's plainest scaling stat, paid for directly.</summary>
public sealed class PotentDistillation() : AlchemistCard(1, CardType.Skill, CardRarity.Uncommon, TargetType.Self)
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [new PowerVar<PotencyPower>(2m)];

    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
        [Tip(AlchemistTips.Potency), Tip(AlchemistTips.Distill), HoverTipFactory.FromPower<PotencyPower>()];

    protected override async Task OnPlay(PlayerChoiceContext ctx, CardPlay play)
    {
        await AlchemistEffects.GainPotency(ctx, Lab, DynamicVars["PotencyPower"].BaseValue);
        await Belt.Distill(ctx, Lab);
    }

    protected override void OnUpgrade() => DynamicVars["PotencyPower"].UpgradeValueBy(1m);
}

/// <summary>Exhaust a Status or Curse in your hand, and draw two cards.</summary>
public sealed class SolventFlask() : AlchemistCard(1, CardType.Skill, CardRarity.Uncommon, TargetType.Self)
{
    protected override async Task OnPlay(PlayerChoiceContext ctx, CardPlay play)
    {
        if (!await Alchemy.ExhaustJunk(ctx, Lab)) return;
        await AlchemistEffects.Draw(ctx, Lab, 2);
    }
}
