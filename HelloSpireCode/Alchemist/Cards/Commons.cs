using HelloSpire.HelloSpireCode.Alchemist.Lab;
using HelloSpire.HelloSpireCode.Alchemist.Powers;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;

namespace HelloSpire.HelloSpireCode.Alchemist.Cards;

// The 20 commons: 8 Attacks, 11 Skills, 1 Power.
//
// The backbone, and deliberately unexciting. Almost every one of them reads a board state the
// character controls -- did you drink something this turn, is the belt empty, did you Brew or
// Exhaust or Distill -- so that the commons teach the payoff conditions the uncommons and rares
// are built on, without any of them being exciting enough to want in every deck. Most either
// Infuse Unstable Concoction with a bonus, or apply Poison as a delayed second half of the hit.

// ---------------------------------------------------------------------------- Attacks

/// <summary>Deal damage, and Infuse Unstable Concoction if you've already used a Potion this turn.</summary>
public sealed class FlaskToss() : AlchemistCard(1, CardType.Attack, CardRarity.Common, TargetType.AnyEnemy)
{
    protected override IEnumerable<DynamicVar> CanonicalVars =>
        [new DamageVar(8m, ValueProp.Move), new DamageVar("Bonus", 4m, ValueProp.Move)];

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [Tip(AlchemistTips.Infuse)];

    protected override async Task OnPlay(PlayerChoiceContext ctx, CardPlay play)
    {
        ArgumentNullException.ThrowIfNull(play.Target);

        await DamageCmd.Attack(DynamicVars.Damage.BaseValue).FromCard(this).Targeting(play.Target).Execute(ctx);

        if ((AlchemistEffects.Peek(Lab)?.PotionsUsedThisTurn ?? 0) > 0)
            Belt.Infuse(Lab, damage: DynamicVars["Bonus"].BaseValue);
    }

    protected override void OnUpgrade() => DynamicVars.Damage.UpgradeValueBy(3m);
}

/// <summary>Hit a random enemy, and Infuse Unstable Concoction if the belt has room.</summary>
public sealed class GlassShard() : AlchemistCard(1, CardType.Attack, CardRarity.Common, TargetType.Self)
{
    protected override IEnumerable<DynamicVar> CanonicalVars =>
        [new DamageVar(9m, ValueProp.Move), new DamageVar("Bonus", 4m, ValueProp.Move)];

    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
        [Tip(AlchemistTips.ThePotionBelt), Tip(AlchemistTips.Infuse)];

    protected override async Task OnPlay(PlayerChoiceContext ctx, CardPlay play)
    {
        var target = AlchemistEffects.RandomEnemy(Lab);
        if (target == null) return;

        await DamageCmd.Attack(DynamicVars.Damage.BaseValue).FromCard(this).Targeting(target).Execute(ctx);

        if (Belt.EmptySlots(Lab) > 0)
            Belt.Infuse(Lab, damage: DynamicVars["Bonus"].BaseValue);
    }

    protected override void OnUpgrade() => DynamicVars.Damage.UpgradeValueBy(3m);
}

/// <summary>Damage to ALL, and Poison to ALL if you've already used a Potion this turn.</summary>
public sealed class ScatterFlask() : AlchemistCard(1, CardType.Skill, CardRarity.Common, TargetType.Self)
{
    protected override IEnumerable<DynamicVar> CanonicalVars =>
        [new DamageVar(5m, ValueProp.Move), new DynamicVar("Poison", 2m)];

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.FromPower<PoisonPower>()];

    protected override async Task OnPlay(PlayerChoiceContext ctx, CardPlay play)
    {
        var usedPotion = (AlchemistEffects.Peek(Lab)?.PotionsUsedThisTurn ?? 0) > 0;

        foreach (var enemy in AlchemistEffects.Enemies(Lab))
        {
            await DamageCmd.Attack(DynamicVars.Damage.BaseValue).FromCard(this).Targeting(enemy).Execute(ctx);
            if (usedPotion) await AlchemistEffects.ApplyPoison(ctx, Lab, enemy, DynamicVars["Poison"].BaseValue);
        }
    }

    protected override void OnUpgrade() => DynamicVars.Damage.UpgradeValueBy(2m);
}

/// <summary>Deal damage, then apply Poison if you've Brewed a Potion this turn.</summary>
public sealed class ToxicScalpel() : AlchemistCard(1, CardType.Attack, CardRarity.Common, TargetType.AnyEnemy)
{
    protected override IEnumerable<DynamicVar> CanonicalVars =>
        [new DamageVar(8m, ValueProp.Move), new DynamicVar("Poison", 2m)];

    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
        [Tip(AlchemistTips.Brew), HoverTipFactory.FromPower<PoisonPower>()];

    protected override async Task OnPlay(PlayerChoiceContext ctx, CardPlay play)
    {
        ArgumentNullException.ThrowIfNull(play.Target);

        await DamageCmd.Attack(DynamicVars.Damage.BaseValue).FromCard(this).Targeting(play.Target).Execute(ctx);

        if ((AlchemistEffects.Peek(Lab)?.BrewedThisTurn ?? 0) > 0)
            await AlchemistEffects.ApplyPoison(ctx, Lab, play.Target, DynamicVars["Poison"].BaseValue);
    }

    protected override void OnUpgrade() => DynamicVars.Damage.UpgradeValueBy(3m);
}

/// <summary>Big damage, and Infuse more into Unstable Concoction.</summary>
public sealed class PyricBurst() : AlchemistCard(2, CardType.Attack, CardRarity.Common, TargetType.AnyEnemy)
{
    protected override IEnumerable<DynamicVar> CanonicalVars =>
        [new DamageVar(16m, ValueProp.Move), new DamageVar("Bonus", 6m, ValueProp.Move)];

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [Tip(AlchemistTips.Infuse)];

    protected override async Task OnPlay(PlayerChoiceContext ctx, CardPlay play)
    {
        ArgumentNullException.ThrowIfNull(play.Target);

        await DamageCmd.Attack(DynamicVars.Damage.BaseValue).FromCard(this).Targeting(play.Target).Execute(ctx);
        Belt.Infuse(Lab, damage: DynamicVars["Bonus"].BaseValue);
    }

    protected override void OnUpgrade() => DynamicVars.Damage.UpgradeValueBy(4m);
}

/// <summary>Free damage, and Infuse Unstable Concoction if a Potion Slot became empty this turn.</summary>
public sealed class QuickSilver() : AlchemistCard(0, CardType.Attack, CardRarity.Common, TargetType.AnyEnemy)
{
    protected override IEnumerable<DynamicVar> CanonicalVars =>
        [new DamageVar(4m, ValueProp.Move), new DamageVar("Bonus", 5m, ValueProp.Move)];

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [Tip(AlchemistTips.Infuse)];

    protected override async Task OnPlay(PlayerChoiceContext ctx, CardPlay play)
    {
        ArgumentNullException.ThrowIfNull(play.Target);

        await DamageCmd.Attack(DynamicVars.Damage.BaseValue).FromCard(this).Targeting(play.Target).Execute(ctx);

        if ((AlchemistEffects.Peek(Lab)?.SlotsEmptiedThisTurn ?? 0) > 0)
            Belt.Infuse(Lab, damage: DynamicVars["Bonus"].BaseValue);
    }

    protected override void OnUpgrade() => DynamicVars.Damage.UpgradeValueBy(2m);
}

/// <summary>Deal damage, and Infuse Unstable Concoction if you've already Exhausted a card this turn.</summary>
public sealed class CrucibleBlow() : AlchemistCard(1, CardType.Attack, CardRarity.Common, TargetType.AnyEnemy)
{
    protected override IEnumerable<DynamicVar> CanonicalVars =>
        [new DamageVar(8m, ValueProp.Move), new DamageVar("Bonus", 5m, ValueProp.Move)];

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [Tip(AlchemistTips.Infuse)];

    protected override async Task OnPlay(PlayerChoiceContext ctx, CardPlay play)
    {
        ArgumentNullException.ThrowIfNull(play.Target);

        await DamageCmd.Attack(DynamicVars.Damage.BaseValue).FromCard(this).Targeting(play.Target).Execute(ctx);

        if ((AlchemistEffects.Peek(Lab)?.CardsExhaustedThisTurn ?? 0) > 0)
            Belt.Infuse(Lab, damage: DynamicVars["Bonus"].BaseValue);
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Damage.UpgradeValueBy(2m);
        DynamicVars["Bonus"].UpgradeValueBy(1m);
    }
}

/// <summary>Deal damage and apply Poison; a Potion already used this turn also buys Weak.</summary>
public sealed class CopperShot() : AlchemistCard(1, CardType.Attack, CardRarity.Common, TargetType.AnyEnemy)
{
    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DamageVar(7m, ValueProp.Move),
        new DynamicVar("Poison", 2m),
        new PowerVar<WeakPower>(1m)
    ];

    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
        [HoverTipFactory.FromPower<PoisonPower>(), HoverTipFactory.FromPower<WeakPower>()];

    protected override async Task OnPlay(PlayerChoiceContext ctx, CardPlay play)
    {
        ArgumentNullException.ThrowIfNull(play.Target);

        await DamageCmd.Attack(DynamicVars.Damage.BaseValue)
            .FromCard(this).Targeting(play.Target).Execute(ctx);

        await AlchemistEffects.ApplyPoison(ctx, Lab, play.Target, DynamicVars["Poison"].BaseValue);

        if ((AlchemistEffects.Peek(Lab)?.PotionsUsedThisTurn ?? 0) > 0)
            await AlchemistEffects.ApplyWeak(ctx, Lab, play.Target, DynamicVars["WeakPower"].BaseValue);
    }

    protected override void OnUpgrade() => DynamicVars.Damage.UpgradeValueBy(3m);
}

// ---------------------------------------------------------------------------- Skills

/// <summary>Infuse Unstable Concoction. The class's defining conversion, at its cheapest and plainest.</summary>
public sealed class Transmute() : AlchemistCard(1, CardType.Skill, CardRarity.Common, TargetType.Self)
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [new DynamicVar("Bonus", 6m)];

    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [Tip(AlchemistTips.Infuse)];

    protected override Task OnPlay(PlayerChoiceContext ctx, CardPlay play)
    {
        Belt.Infuse(Lab, damage: DynamicVars["Bonus"].BaseValue);
        return Task.CompletedTask;
    }

    protected override void OnUpgrade() => DynamicVars["Bonus"].UpgradeValueBy(2m);
}

/// <summary>Transmute's small cousin: less into Unstable Concoction, some real Block, and free.</summary>
public sealed class SalvageReagents() : AlchemistCard(0, CardType.Skill, CardRarity.Common, TargetType.Self)
{
    public override bool GainsBlock => true;

    protected override IEnumerable<DynamicVar> CanonicalVars =>
        [new BlockVar(4m, ValueProp.Move), new BlockVar("Bonus", 3m, ValueProp.Move)];

    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [Tip(AlchemistTips.Infuse)];

    protected override async Task OnPlay(PlayerChoiceContext ctx, CardPlay play)
    {
        if (!await Alchemy.ExhaustOne(ctx, Lab)) return;

        await AlchemistEffects.GainBlock(Lab, DynamicVars.Block.BaseValue);
        Belt.Infuse(Lab, block: DynamicVars["Bonus"].BaseValue);
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Block.UpgradeValueBy(2m);
        DynamicVars["Bonus"].UpgradeValueBy(1m);
    }
}

/// <summary>Brew something. The plainest statement of what the character does.</summary>
public sealed class PocketFormula() : AlchemistCard(1, CardType.Skill, CardRarity.Common, TargetType.Self)
{
    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];

    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
        [Tip(AlchemistTips.Brew), Tip(AlchemistTips.Volatile)];

    protected override async Task OnPlay(PlayerChoiceContext ctx, CardPlay play)
    {
        await CreatureCmd.TriggerAnim(Owner.Creature, "Cast", Owner.Character.CastAnimDelay);
        await Belt.BrewRandom(ctx, Lab);
    }

    protected override void OnUpgrade() => EnergyCost.UpgradeBy(-1);
}

/// <summary>Block that scales with how little you are carrying.</summary>
public sealed class GlassApron() : AlchemistCard(1, CardType.Skill, CardRarity.Common, TargetType.Self)
{
    public override bool GainsBlock => true;

    protected override IEnumerable<DynamicVar> CanonicalVars =>
        [new BlockVar(7m, ValueProp.Move), new BlockVar("PerSlot", 1m, ValueProp.Move)];

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [Tip(AlchemistTips.ThePotionBelt)];

    protected override async Task OnPlay(PlayerChoiceContext ctx, CardPlay play)
    {
        var block = DynamicVars.Block.BaseValue
                    + DynamicVars["PerSlot"].BaseValue * Belt.EmptySlots(Lab);

        await AlchemistEffects.GainBlock(Lab, block);
    }

    protected override void OnUpgrade() => DynamicVars.Block.UpgradeValueBy(3m);
}

/// <summary>Block, and Infuse Unstable Concoction if you've already used a Potion this turn.</summary>
public sealed class SteadyPour() : AlchemistCard(1, CardType.Skill, CardRarity.Common, TargetType.Self)
{
    public override bool GainsBlock => true;

    protected override IEnumerable<DynamicVar> CanonicalVars =>
        [new BlockVar(8m, ValueProp.Move), new BlockVar("Bonus", 4m, ValueProp.Move)];

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [Tip(AlchemistTips.Infuse)];

    protected override async Task OnPlay(PlayerChoiceContext ctx, CardPlay play)
    {
        await AlchemistEffects.GainBlock(Lab, DynamicVars.Block.BaseValue);

        if ((AlchemistEffects.Peek(Lab)?.PotionsUsedThisTurn ?? 0) > 0)
            Belt.Infuse(Lab, block: DynamicVars["Bonus"].BaseValue);
    }

    protected override void OnUpgrade() => DynamicVars.Block.UpgradeValueBy(3m);
}

/// <summary>Pour a Potion out for cards and a little Unstable Concoction.</summary>
public sealed class Dilute() : AlchemistCard(0, CardType.Skill, CardRarity.Common, TargetType.Self)
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [new CardsVar(2), new DynamicVar("Energy", 2m)];

    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [Tip(AlchemistTips.Distill), Tip(AlchemistTips.Infuse)];

    protected override async Task OnPlay(PlayerChoiceContext ctx, CardPlay play)
    {
        if (!(await Belt.Distill(ctx, Lab)).Distilled) return;

        await AlchemistEffects.Draw(ctx, Lab, DynamicVars.Cards.IntValue);
        Belt.Infuse(Lab, energy: DynamicVars["Energy"].BaseValue);
    }

    protected override void OnUpgrade() => DynamicVars.Cards.UpgradeValueBy(1m);
}

/// <summary>Block, and Infuse Unstable Concoction if you've already Exhausted another card this turn.</summary>
public sealed class RecycleGlass() : AlchemistCard(1, CardType.Skill, CardRarity.Common, TargetType.Self)
{
    public override bool GainsBlock => true;

    protected override IEnumerable<DynamicVar> CanonicalVars =>
        [new BlockVar(5m, ValueProp.Move), new BlockVar("Bonus", 5m, ValueProp.Move)];

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [Tip(AlchemistTips.Infuse)];

    protected override async Task OnPlay(PlayerChoiceContext ctx, CardPlay play)
    {
        await AlchemistEffects.GainBlock(Lab, DynamicVars.Block.BaseValue);

        if ((AlchemistEffects.Peek(Lab)?.CardsExhaustedThisTurn ?? 0) > 0)
            Belt.Infuse(Lab, block: DynamicVars["Bonus"].BaseValue);
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Block.UpgradeValueBy(2m);
        DynamicVars["Bonus"].UpgradeValueBy(1m);
    }
}

/// <summary>A little Energy into Unstable Concoction, free.</summary>
public sealed class EnergyFlask() : AlchemistCard(0, CardType.Skill, CardRarity.Common, TargetType.Self)
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [new DynamicVar("Energy", 1m)];

    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [Tip(AlchemistTips.Infuse)];

    protected override Task OnPlay(PlayerChoiceContext ctx, CardPlay play)
    {
        Belt.Infuse(Lab, energy: DynamicVars["Energy"].BaseValue);
        return Task.CompletedTask;
    }

    protected override void OnUpgrade() => DynamicVars["Energy"].UpgradeValueBy(1m);
}

/// <summary>Apply Weak and Poison, and Block if you've already used a Potion this turn.</summary>
public sealed class BitterSolvent() : AlchemistCard(1, CardType.Skill, CardRarity.Common, TargetType.AnyEnemy)
{
    public override bool GainsBlock => true;

    protected override IEnumerable<DynamicVar> CanonicalVars =>
        [new PowerVar<WeakPower>(2m), new DynamicVar("Poison", 2m), new BlockVar(4m, ValueProp.Move)];

    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
        [HoverTipFactory.FromPower<WeakPower>(), HoverTipFactory.FromPower<PoisonPower>()];

    protected override async Task OnPlay(PlayerChoiceContext ctx, CardPlay play)
    {
        ArgumentNullException.ThrowIfNull(play.Target);

        await AlchemistEffects.ApplyWeak(ctx, Lab, play.Target, DynamicVars["WeakPower"].BaseValue);
        await AlchemistEffects.ApplyPoison(ctx, Lab, play.Target, DynamicVars["Poison"].BaseValue);

        if ((AlchemistEffects.Peek(Lab)?.PotionsUsedThisTurn ?? 0) > 0)
            await AlchemistEffects.GainBlock(Lab, DynamicVars.Block.BaseValue);
    }

    protected override void OnUpgrade() => DynamicVars["WeakPower"].UpgradeValueBy(1m);
}

/// <summary>Dig, and get paid a little Block for having already Distilled this turn.</summary>
public sealed class MarketSense() : AlchemistCard(1, CardType.Skill, CardRarity.Common, TargetType.Self)
{
    public override bool GainsBlock => true;

    protected override IEnumerable<DynamicVar> CanonicalVars =>
        [new CardsVar(2), new BlockVar(4m, ValueProp.Move)];

    protected override async Task OnPlay(PlayerChoiceContext ctx, CardPlay play)
    {
        await AlchemistEffects.Draw(ctx, Lab, DynamicVars.Cards.IntValue);
        await Alchemy.DiscardOne(ctx, Lab);

        if ((AlchemistEffects.Peek(Lab)?.DistilledThisTurn ?? 0) > 0)
            await AlchemistEffects.GainBlock(Lab, DynamicVars.Block.BaseValue);
    }

    protected override void OnUpgrade() => DynamicVars.Cards.UpgradeValueBy(1m);
}

/// <summary>
/// One more Potion Slot for this combat.
///
/// The Slot is Volatile-only, which is the entire reason it is temporary — a Volatile Potion is
/// removed at combat end anyway, so no separate expiry rule is needed. It also means this can
/// never be used to bank a found Rare Potion.
/// </summary>
public sealed class ExtraVial() : AlchemistCard(1, CardType.Skill, CardRarity.Common, TargetType.Self)
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [new DynamicVar("Slots", 1m)];

    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];

    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
        [Tip(AlchemistTips.ThePotionBelt), Tip(AlchemistTips.Volatile)];

    protected override async Task OnPlay(PlayerChoiceContext ctx, CardPlay play) =>
        await Belt.GrantTemporarySlots(ctx, Lab, DynamicVars["Slots"].IntValue);

    protected override void OnUpgrade() => EnergyCost.UpgradeBy(-1);
}

// ---------------------------------------------------------------------------- Power

/// <summary>The first Potion you drink each turn also throws a little heat, and poisons it.</summary>
public sealed class ResidualHeat() : AlchemistCard(1, CardType.Power, CardRarity.Common, TargetType.Self)
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [new PowerVar<ResidualHeatPower>(3m)];

    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
        [HoverTipFactory.FromPower<ResidualHeatPower>()];

    protected override async Task OnPlay(PlayerChoiceContext ctx, CardPlay play)
    {
        await CreatureCmd.TriggerAnim(Owner.Creature, "Cast", Owner.Character.CastAnimDelay);
        await PowerCmd.Apply<ResidualHeatPower>(ctx, Owner.Creature,
            DynamicVars["ResidualHeatPower"].BaseValue, Owner.Creature, this);
    }

    protected override void OnUpgrade() => DynamicVars["ResidualHeatPower"].UpgradeValueBy(2m);
}
