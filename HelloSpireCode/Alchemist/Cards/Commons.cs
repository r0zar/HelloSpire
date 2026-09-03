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

// The 21 commons: 11 Attacks, 8 Skills, 2 Powers.
//
// Deliberately unexciting, and deliberately proactive -- almost none of them ask "did you do X
// this turn" anymore. Most either Brew a specific Volatile Potion by name (so the deck teaches
// "the Alchemist makes Potions" before it teaches what Distill/Infuse do with them), apply Poison
// directly, or Infuse Unstable Concoction outright.

// ---------------------------------------------------------------------------- Attacks

/// <summary>Deal damage, and Brew a Volatile Weak Potion.</summary>
public sealed class CopperShot() : AlchemistCard(1, CardType.Attack, CardRarity.Common, TargetType.AnyEnemy)
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [new DamageVar(7m, ValueProp.Move)];

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [Tip(AlchemistTips.Brew)];

    protected override async Task OnPlay(PlayerChoiceContext ctx, CardPlay play)
    {
        ArgumentNullException.ThrowIfNull(play.Target);

        await DamageCmd.Attack(DynamicVars.Damage.BaseValue).FromCard(this).Targeting(play.Target).Execute(ctx);
        await Belt.Brew(ctx, Lab, LabBridge.Current.NamedPotion(BasePotion.Weak));
    }

    protected override void OnUpgrade() => DynamicVars.Damage.UpgradeValueBy(3m);
}

/// <summary>Deal damage. If you Exhausted a card this turn, apply Vulnerable.</summary>
public sealed class CinnabarEdge() : AlchemistCard(1, CardType.Attack, CardRarity.Uncommon, TargetType.AnyEnemy)
{
    protected override IEnumerable<DynamicVar> CanonicalVars =>
        [new DamageVar(9m, ValueProp.Move), new DynamicVar("Vulnerable", 1m)];

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.FromPower<VulnerablePower>()];

    protected override async Task OnPlay(PlayerChoiceContext ctx, CardPlay play)
    {
        ArgumentNullException.ThrowIfNull(play.Target);

        await DamageCmd.Attack(DynamicVars.Damage.BaseValue).FromCard(this).Targeting(play.Target).Execute(ctx);

        if ((AlchemistEffects.Peek(Lab)?.CardsExhaustedThisTurn ?? 0) > 0)
            await AlchemistEffects.ApplyVulnerable(ctx, Lab, play.Target, DynamicVars["Vulnerable"].BaseValue);
    }

    protected override void OnUpgrade() => DynamicVars.Damage.UpgradeValueBy(3m);
}

/// <summary>Hit a random enemy.</summary>
public sealed class GlassShard() : AlchemistCard(1, CardType.Attack, CardRarity.Common, TargetType.Self)
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [new DamageVar(10m, ValueProp.Move)];

    protected override async Task OnPlay(PlayerChoiceContext ctx, CardPlay play)
    {
        var target = AlchemistEffects.RandomEnemy(Lab);
        if (target == null) return;

        await DamageCmd.Attack(DynamicVars.Damage.BaseValue).FromCard(this).Targeting(target).Execute(ctx);
    }

    protected override void OnUpgrade() => DynamicVars.Damage.UpgradeValueBy(3m);
}

/// <summary>Free damage. If your Potion Belt is full, draw a card.</summary>
public sealed class QuickSilver() : AlchemistCard(0, CardType.Attack, CardRarity.Common, TargetType.AnyEnemy)
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [new DamageVar(4m, ValueProp.Move)];

    protected override async Task OnPlay(PlayerChoiceContext ctx, CardPlay play)
    {
        ArgumentNullException.ThrowIfNull(play.Target);

        await DamageCmd.Attack(DynamicVars.Damage.BaseValue).FromCard(this).Targeting(play.Target).Execute(ctx);

        if (Belt.IsFull(Lab))
            await AlchemistEffects.Draw(ctx, Lab, 1);
    }

    protected override void OnUpgrade() => DynamicVars.Damage.UpgradeValueBy(2m);
}

/// <summary>Deal damage. If you used a Potion this turn, deal additional damage.</summary>
public sealed class FlaskToss() : AlchemistCard(1, CardType.Attack, CardRarity.Common, TargetType.AnyEnemy)
{
    protected override IEnumerable<DynamicVar> CanonicalVars =>
        [new DamageVar(7m, ValueProp.Move), new DamageVar("Bonus", 4m, ValueProp.Move)];

    protected override async Task OnPlay(PlayerChoiceContext ctx, CardPlay play)
    {
        ArgumentNullException.ThrowIfNull(play.Target);

        var damage = DynamicVars.Damage.BaseValue;
        if ((AlchemistEffects.Peek(Lab)?.PotionsUsedThisTurn ?? 0) > 0)
            damage += DynamicVars["Bonus"].BaseValue;

        await DamageCmd.Attack(damage).FromCard(this).Targeting(play.Target).Execute(ctx);
    }

    protected override void OnUpgrade() => DynamicVars.Damage.UpgradeValueBy(2m);
}

/// <summary>Deal damage. If you used a Potion this turn, draw a card.</summary>
public sealed class BrewedEdge() : AlchemistCard(1, CardType.Attack, CardRarity.Common, TargetType.AnyEnemy)
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [new DamageVar(10m, ValueProp.Move)];

    protected override async Task OnPlay(PlayerChoiceContext ctx, CardPlay play)
    {
        ArgumentNullException.ThrowIfNull(play.Target);

        await DamageCmd.Attack(DynamicVars.Damage.BaseValue).FromCard(this).Targeting(play.Target).Execute(ctx);

        if ((AlchemistEffects.Peek(Lab)?.PotionsUsedThisTurn ?? 0) > 0)
            await AlchemistEffects.Draw(ctx, Lab, 1);
    }

    protected override void OnUpgrade() => DynamicVars.Damage.UpgradeValueBy(3m);
}

/// <summary>Deal damage.</summary>
public sealed class CrucibleBlow() : AlchemistCard(1, CardType.Attack, CardRarity.Common, TargetType.AnyEnemy)
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [new DamageVar(9m, ValueProp.Move)];

    protected override async Task OnPlay(PlayerChoiceContext ctx, CardPlay play)
    {
        ArgumentNullException.ThrowIfNull(play.Target);

        await DamageCmd.Attack(DynamicVars.Damage.BaseValue).FromCard(this).Targeting(play.Target).Execute(ctx);
    }

    protected override void OnUpgrade() => DynamicVars.Damage.UpgradeValueBy(1m);
}

/// <summary>Deal damage, and Brew a Poison Potion.</summary>
public sealed class CausticFlask() : AlchemistCard(1, CardType.Attack, CardRarity.Uncommon, TargetType.AnyEnemy)
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [new DamageVar(6m, ValueProp.Move)];

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [Tip(AlchemistTips.Brew)];

    protected override async Task OnPlay(PlayerChoiceContext ctx, CardPlay play)
    {
        ArgumentNullException.ThrowIfNull(play.Target);

        await DamageCmd.Attack(DynamicVars.Damage.BaseValue).FromCard(this).Targeting(play.Target).Execute(ctx);
        await Belt.Brew(ctx, Lab, LabBridge.Current.NamedPotion(BasePotion.Poison));
    }

    protected override void OnUpgrade() => DynamicVars.Damage.UpgradeValueBy(3m);
}

/// <summary>Deal damage, and leave a Volatile Reagent behind in the discard pile.</summary>
public sealed class VolatileStrike() : AlchemistCard(1, CardType.Attack, CardRarity.Common, TargetType.AnyEnemy)
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [new DamageVar(8m, ValueProp.Move)];

    protected override async Task OnPlay(PlayerChoiceContext ctx, CardPlay play)
    {
        ArgumentNullException.ThrowIfNull(play.Target);

        await DamageCmd.Attack(DynamicVars.Damage.BaseValue).FromCard(this).Targeting(play.Target).Execute(ctx);
        await Alchemy.CreateVolatileReagent(ctx, Lab, PileType.Discard);
    }

    protected override void OnUpgrade() => DynamicVars.Damage.UpgradeValueBy(3m);
}

/// <summary>Deal a lot of damage, and Brew a Volatile Fire Potion.</summary>
public sealed class Firebrand() : AlchemistCard(2, CardType.Attack, CardRarity.Uncommon, TargetType.AnyEnemy)
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [new DamageVar(7m, ValueProp.Move)];

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [Tip(AlchemistTips.Brew)];

    protected override async Task OnPlay(PlayerChoiceContext ctx, CardPlay play)
    {
        ArgumentNullException.ThrowIfNull(play.Target);

        await DamageCmd.Attack(DynamicVars.Damage.BaseValue).FromCard(this).Targeting(play.Target).Execute(ctx);
        await Belt.Brew(ctx, Lab, LabBridge.Current.NamedPotion(BasePotion.Fire));
    }

    protected override void OnUpgrade() => DynamicVars.Damage.UpgradeValueBy(4m);
}

// ---------------------------------------------------------------------------- Skills

/// <summary>Gain Block, and apply Poison. Replaces the old Aegis Formula slot now that Aegis
/// Formula itself is a Basic-only starter.</summary>
public sealed class CoagulatingAgent() : AlchemistCard(1, CardType.Skill, CardRarity.Common, TargetType.AnyEnemy)
{
    public override bool GainsBlock => true;

    protected override IEnumerable<DynamicVar> CanonicalVars =>
        [new BlockVar(7m, ValueProp.Move), new DynamicVar("Poison", 3m)];

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.FromPower<PoisonPower>()];

    protected override async Task OnPlay(PlayerChoiceContext ctx, CardPlay play)
    {
        ArgumentNullException.ThrowIfNull(play.Target);

        await AlchemistEffects.GainBlock(Lab, DynamicVars.Block.BaseValue);
        await AlchemistEffects.ApplyPoison(ctx, Lab, play.Target, DynamicVars["Poison"].BaseValue);
    }

    protected override void OnUpgrade() => DynamicVars.Block.UpgradeValueBy(3m);
}

/// <summary>Deal damage and Poison every enemy.</summary>
public sealed class ScatterFlask() : AlchemistCard(1, CardType.Attack, CardRarity.Common, TargetType.AllEnemies)
{
    protected override IEnumerable<DynamicVar> CanonicalVars =>
        [new DamageVar(2m, ValueProp.Move), new DynamicVar("Poison", 3m)];

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.FromPower<PoisonPower>()];

    protected override async Task OnPlay(PlayerChoiceContext ctx, CardPlay play)
    {
        foreach (var enemy in AlchemistEffects.Enemies(Lab))
        {
            await DamageCmd.Attack(DynamicVars.Damage.BaseValue).FromCard(this).Targeting(enemy).Execute(ctx);
            await AlchemistEffects.ApplyPoison(ctx, Lab, enemy, DynamicVars["Poison"].BaseValue);
        }
    }

    protected override void OnUpgrade() => DynamicVars.Damage.UpgradeValueBy(2m);
}

/// <summary>Brew a Volatile Energy Potion.</summary>
public sealed class EnergyFlask() : AlchemistCard(0, CardType.Skill, CardRarity.Uncommon, TargetType.Self)
{
    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [Tip(AlchemistTips.Brew)];

    protected override async Task OnPlay(PlayerChoiceContext ctx, CardPlay play) =>
        await Belt.Brew(ctx, Lab, LabBridge.Current.NamedPotion(BasePotion.Energy));
}

/// <summary>Brew an Explosive Ampoule.</summary>
public sealed class PyricFormula() : AlchemistCard(1, CardType.Skill, CardRarity.Common, TargetType.Self)
{
    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];

    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
        [Tip(AlchemistTips.Brew), Tip(AlchemistTips.Volatile)];

    protected override async Task OnPlay(PlayerChoiceContext ctx, CardPlay play)
    {
        await CreatureCmd.TriggerAnim(Owner.Creature, "Cast", Owner.Character.CastAnimDelay);
        await Belt.Brew(ctx, Lab, LabBridge.Current.NamedPotion(BasePotion.ExplosiveAmpoule));
    }

    protected override void OnUpgrade() => EnergyCost.UpgradeBy(-1);
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

/// <summary>Exhaust a card for Block and a little Infuse, free.</summary>
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
        await Belt.Infuse(ctx, Lab, block: DynamicVars["Bonus"].BaseValue);
        await Belt.LeaveResidualReagent(ctx, Lab);
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Block.UpgradeValueBy(2m);
        DynamicVars["Bonus"].UpgradeValueBy(1m);
    }
}

/// <summary>Apply Poison and Weak.</summary>
public sealed class BitterSolvent() : AlchemistCard(1, CardType.Skill, CardRarity.Common, TargetType.AnyEnemy)
{
    protected override IEnumerable<DynamicVar> CanonicalVars =>
        [new DynamicVar("Poison", 3m), new PowerVar<WeakPower>(1m)];

    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
        [HoverTipFactory.FromPower<PoisonPower>(), HoverTipFactory.FromPower<WeakPower>()];

    protected override async Task OnPlay(PlayerChoiceContext ctx, CardPlay play)
    {
        ArgumentNullException.ThrowIfNull(play.Target);

        await AlchemistEffects.ApplyPoison(ctx, Lab, play.Target, DynamicVars["Poison"].BaseValue);
        await AlchemistEffects.ApplyWeak(ctx, Lab, play.Target, DynamicVars["WeakPower"].BaseValue);
    }

    protected override void OnUpgrade() => DynamicVars["Poison"].UpgradeValueBy(1m);
}

/// <summary>Infuse Block into Unstable Concoction.</summary>
public sealed class SteadyPour() : AlchemistCard(1, CardType.Skill, CardRarity.Common, TargetType.Self)
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [new BlockVar(8m, ValueProp.Move)];

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [Tip(AlchemistTips.Infuse)];

    protected override async Task OnPlay(PlayerChoiceContext ctx, CardPlay play) =>
        await Belt.Infuse(ctx, Lab, block: DynamicVars.Block.BaseValue);

    protected override void OnUpgrade() => DynamicVars.Block.UpgradeValueBy(3m);
}

/// <summary>Leave a Volatile Reagent in the draw pile, and apply Poison.</summary>
public sealed class ContaminatedSample() : AlchemistCard(1, CardType.Skill, CardRarity.Common, TargetType.AnyEnemy)
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [new DynamicVar("Poison", 3m)];

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.FromPower<PoisonPower>()];

    protected override async Task OnPlay(PlayerChoiceContext ctx, CardPlay play)
    {
        ArgumentNullException.ThrowIfNull(play.Target);

        await Alchemy.CreateVolatileReagent(ctx, Lab, PileType.Draw);
        await AlchemistEffects.ApplyPoison(ctx, Lab, play.Target, DynamicVars["Poison"].BaseValue);
    }
}

// ---------------------------------------------------------------------------- Powers

/// <summary>The first time Poison is applied each turn applies additional Poison.</summary>
public sealed class ResidualToxins() : AlchemistCard(1, CardType.Power, CardRarity.Common, TargetType.Self)
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [new PowerVar<ResidualToxinsPower>(3m)];

    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
        [HoverTipFactory.FromPower<ResidualToxinsPower>()];

    protected override async Task OnPlay(PlayerChoiceContext ctx, CardPlay play)
    {
        await CreatureCmd.TriggerAnim(Owner.Creature, "Cast", Owner.Character.CastAnimDelay);
        await PowerCmd.Apply<ResidualToxinsPower>(ctx, Owner.Creature,
            DynamicVars["ResidualToxinsPower"].BaseValue, Owner.Creature, this);
    }

    protected override void OnUpgrade() => DynamicVars["ResidualToxinsPower"].UpgradeValueBy(2m);
}

/// <summary>The first time you Brew each turn, draw a card.</summary>
public sealed class BrewingHabit() : AlchemistCard(1, CardType.Power, CardRarity.Common, TargetType.Self)
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [new PowerVar<BrewingHabitPower>(1m)];

    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
        [Tip(AlchemistTips.Brew), HoverTipFactory.FromPower<BrewingHabitPower>()];

    protected override async Task OnPlay(PlayerChoiceContext ctx, CardPlay play)
    {
        await CreatureCmd.TriggerAnim(Owner.Creature, "Cast", Owner.Character.CastAnimDelay);
        await PowerCmd.Apply<BrewingHabitPower>(ctx, Owner.Creature,
            DynamicVars["BrewingHabitPower"].BaseValue, Owner.Creature, this);
    }

    protected override void OnUpgrade() => EnergyCost.UpgradeBy(-1);
}
