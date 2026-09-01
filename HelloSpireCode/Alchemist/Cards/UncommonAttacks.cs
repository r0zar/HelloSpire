using HelloSpire.HelloSpireCode.Alchemist.Lab;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;

namespace HelloSpire.HelloSpireCode.Alchemist.Cards;

// Uncommon Attacks 1-12. Between them they read every board state the class controls: how full the
// belt is, how empty it is, what you drank, what you Distilled, what you Exhausted, what you
// created, and how much you've Brewed or Distilled this whole fight.

/// <summary>Damage scaling with a full belt. Full Belt's headline card.</summary>
public sealed class BottleBarrage() : AlchemistCard(1, CardType.Attack, CardRarity.Uncommon, TargetType.AnyEnemy)
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [new DamageVar("PerPotion", 4m, ValueProp.Move)];

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [Tip(AlchemistTips.ThePotionBelt)];

    protected override async Task OnPlay(PlayerChoiceContext ctx, CardPlay play)
    {
        ArgumentNullException.ThrowIfNull(play.Target);

        var damage = DynamicVars["PerPotion"].BaseValue * Belt.Held(Lab).Count;
        if (damage <= 0) return;

        await DamageCmd.Attack(damage).FromCard(this).Targeting(play.Target).Execute(ctx);
    }

    protected override void OnUpgrade() => DynamicVars["PerPotion"].UpgradeValueBy(1m);
}

/// <summary>Deal damage, then Distill a Potion to Infuse Unstable Concoction.</summary>
public sealed class Shatterstock() : AlchemistCard(1, CardType.Attack, CardRarity.Uncommon, TargetType.AnyEnemy)
{
    protected override IEnumerable<DynamicVar> CanonicalVars =>
        [new DamageVar(9m, ValueProp.Move), new DamageVar("Bonus", 9m, ValueProp.Move)];

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [Tip(AlchemistTips.Distill), Tip(AlchemistTips.Infuse)];

    protected override async Task OnPlay(PlayerChoiceContext ctx, CardPlay play)
    {
        ArgumentNullException.ThrowIfNull(play.Target);

        await DamageCmd.Attack(DynamicVars.Damage.BaseValue).FromCard(this).Targeting(play.Target).Execute(ctx);

        if ((await Belt.Distill(ctx, Lab)).Distilled)
            Belt.Infuse(Lab, damage: DynamicVars["Bonus"].BaseValue);
    }

    protected override void OnUpgrade() => DynamicVars.Damage.UpgradeValueBy(3m);
}

/// <summary>Deal damage; Infuse if the belt is full, apply Poison if you've already Exhausted this turn.</summary>
public sealed class PressureBurst() : AlchemistCard(1, CardType.Attack, CardRarity.Uncommon, TargetType.AnyEnemy)
{
    protected override IEnumerable<DynamicVar> CanonicalVars =>
        [new DamageVar(8m, ValueProp.Move), new DamageVar("Bonus", 8m, ValueProp.Move), new DynamicVar("Poison", 2m)];

    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
        [Tip(AlchemistTips.Infuse), HoverTipFactory.FromPower<PoisonPower>()];

    protected override async Task OnPlay(PlayerChoiceContext ctx, CardPlay play)
    {
        ArgumentNullException.ThrowIfNull(play.Target);

        await DamageCmd.Attack(DynamicVars.Damage.BaseValue).FromCard(this).Targeting(play.Target).Execute(ctx);

        if (Belt.IsFull(Lab))
            Belt.Infuse(Lab, damage: DynamicVars["Bonus"].BaseValue);

        if ((AlchemistEffects.Peek(Lab)?.CardsExhaustedThisTurn ?? 0) > 0)
            await AlchemistEffects.ApplyPoison(ctx, Lab, play.Target, DynamicVars["Poison"].BaseValue);
    }

    protected override void OnUpgrade() => DynamicVars.Damage.UpgradeValueBy(3m);
}

/// <summary>Free, and better -- with a little Infuse -- when you're holding nothing.</summary>
public sealed class EmptyBottle() : AlchemistCard(0, CardType.Attack, CardRarity.Uncommon, TargetType.AnyEnemy)
{
    protected override IEnumerable<DynamicVar> CanonicalVars =>
        [new DamageVar(4m, ValueProp.Move), new DamageVar("Bonus", 6m, ValueProp.Move), new DamageVar("Infuse", 4m, ValueProp.Move)];

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [Tip(AlchemistTips.Infuse)];

    protected override async Task OnPlay(PlayerChoiceContext ctx, CardPlay play)
    {
        ArgumentNullException.ThrowIfNull(play.Target);

        var empty = Belt.IsEmpty(Lab);
        var bonus = empty ? DynamicVars["Bonus"].BaseValue : 0m;

        await DamageCmd.Attack(DynamicVars.Damage.BaseValue + bonus)
            .FromCard(this).Targeting(play.Target).Execute(ctx);

        if (empty) Belt.Infuse(Lab, damage: DynamicVars["Infuse"].BaseValue);
    }

    protected override void OnUpgrade() => DynamicVars.Damage.UpgradeValueBy(2m);
}

/// <summary>Deal damage, and Brew a Vulnerable Potion.</summary>
public sealed class CinnabarEdge() : AlchemistCard(1, CardType.Attack, CardRarity.Uncommon, TargetType.AnyEnemy)
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [new DamageVar(9m, ValueProp.Move)];

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [Tip(AlchemistTips.Brew)];

    protected override async Task OnPlay(PlayerChoiceContext ctx, CardPlay play)
    {
        ArgumentNullException.ThrowIfNull(play.Target);

        await DamageCmd.Attack(DynamicVars.Damage.BaseValue).FromCard(this).Targeting(play.Target).Execute(ctx);

        await Belt.Brew(ctx, Lab, LabBridge.Current.NamedPotion(BasePotion.Vulnerable));
    }

    protected override void OnUpgrade() => DynamicVars.Damage.UpgradeValueBy(3m);
}

/// <summary>Deal damage, and Infuse more the more you've Distilled this fight.</summary>
public sealed class AlembicBlade() : AlchemistCard(1, CardType.Attack, CardRarity.Uncommon, TargetType.AnyEnemy)
{
    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DamageVar(8m, ValueProp.Move),
        new DamageVar("PerPotion", 3m, ValueProp.Move)
    ];

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [Tip(AlchemistTips.Distill), Tip(AlchemistTips.Infuse)];

    protected override async Task OnPlay(PlayerChoiceContext ctx, CardPlay play)
    {
        ArgumentNullException.ThrowIfNull(play.Target);

        await DamageCmd.Attack(DynamicVars.Damage.BaseValue).FromCard(this).Targeting(play.Target).Execute(ctx);

        var distilled = AlchemistEffects.Peek(Lab)?.DistilledThisCombat ?? 0;
        var bonus = DynamicVars["PerPotion"].BaseValue * distilled;
        Belt.Infuse(Lab, damage: bonus);
    }

    protected override void OnUpgrade() => DynamicVars.Damage.UpgradeValueBy(3m);
}

/// <summary>Deal damage, and apply Poison if you've already used a Potion this turn.</summary>
public sealed class VolatileCompound() : AlchemistCard(2, CardType.Attack, CardRarity.Uncommon, TargetType.AnyEnemy)
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [new DamageVar(18m, ValueProp.Move), new DynamicVar("Poison", 5m)];

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.FromPower<PoisonPower>()];

    protected override async Task OnPlay(PlayerChoiceContext ctx, CardPlay play)
    {
        ArgumentNullException.ThrowIfNull(play.Target);

        await DamageCmd.Attack(DynamicVars.Damage.BaseValue).FromCard(this).Targeting(play.Target).Execute(ctx);

        if ((AlchemistEffects.Peek(Lab)?.PotionsUsedThisTurn ?? 0) > 0)
            await AlchemistEffects.ApplyPoison(ctx, Lab, play.Target, DynamicVars["Poison"].BaseValue);
    }

    protected override void OnUpgrade() => DynamicVars.Damage.UpgradeValueBy(4m);
}

/// <summary>AoE, with Poison for ALL if you've already used a Potion this turn.</summary>
public sealed class FlashPowder() : AlchemistCard(1, CardType.Attack, CardRarity.Uncommon, TargetType.AllEnemies)
{
    protected override IEnumerable<DynamicVar> CanonicalVars =>
        [new DamageVar(6m, ValueProp.Move), new DynamicVar("Poison", 3m)];

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

/// <summary>Deal damage, more the more you've Brewed this fight.</summary>
public sealed class CatalystNeedle() : AlchemistCard(1, CardType.Attack, CardRarity.Uncommon, TargetType.AnyEnemy)
{
    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DamageVar(7m, ValueProp.Move),
        new DamageVar("PerPotion", 1m, ValueProp.Move)
    ];

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [Tip(AlchemistTips.Brew)];

    protected override async Task OnPlay(PlayerChoiceContext ctx, CardPlay play)
    {
        ArgumentNullException.ThrowIfNull(play.Target);

        var brewed = AlchemistEffects.Peek(Lab)?.BrewedThisCombat ?? 0;
        var bonus = DynamicVars["PerPotion"].BaseValue * brewed;

        await DamageCmd.Attack(DynamicVars.Damage.BaseValue + bonus)
            .FromCard(this).Targeting(play.Target).Execute(ctx);
    }

    protected override void OnUpgrade() => DynamicVars.Damage.UpgradeValueBy(2m);
}

/// <summary>Two hits, three if you Distilled this turn.</summary>
public sealed class Corkscrew() : AlchemistCard(1, CardType.Attack, CardRarity.Uncommon, TargetType.AnyEnemy)
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [new DamageVar(5m, ValueProp.Move)];

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [Tip(AlchemistTips.Distill)];

    protected override async Task OnPlay(PlayerChoiceContext ctx, CardPlay play)
    {
        ArgumentNullException.ThrowIfNull(play.Target);

        var distilled = (AlchemistEffects.Peek(Lab)?.DistilledThisTurn ?? 0) > 0;
        var hits = distilled ? 3 : 2;

        for (var i = 0; i < hits; i++)
        {
            await DamageCmd.Attack(DynamicVars.Damage.BaseValue)
                .FromCard(this).Targeting(play.Target).Execute(ctx);
        }
    }

    protected override void OnUpgrade() => DynamicVars.Damage.UpgradeValueBy(2m);
}

/// <summary>Deal damage, and Infuse more if you've created a card this turn.</summary>
public sealed class ReactiveSlash() : AlchemistCard(1, CardType.Attack, CardRarity.Uncommon, TargetType.AnyEnemy)
{
    protected override IEnumerable<DynamicVar> CanonicalVars =>
        [new DamageVar(10m, ValueProp.Move), new DamageVar("Bonus", 6m, ValueProp.Move)];

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [Tip(AlchemistTips.Infuse)];

    protected override async Task OnPlay(PlayerChoiceContext ctx, CardPlay play)
    {
        ArgumentNullException.ThrowIfNull(play.Target);

        await DamageCmd.Attack(DynamicVars.Damage.BaseValue).FromCard(this).Targeting(play.Target).Execute(ctx);

        if ((AlchemistEffects.Peek(Lab)?.CardsCreatedThisTurn ?? 0) > 0)
            Belt.Infuse(Lab, damage: DynamicVars["Bonus"].BaseValue);
    }

    protected override void OnUpgrade() => DynamicVars.Damage.UpgradeValueBy(3m);
}

/// <summary>
/// The biggest uncommon hit in the class, and it eats a card out of your Hand at random to do it.
///
/// Random rather than chosen on purpose: this is the only Exhaust in the set the player does not
/// get to aim, which is what stops it being a strictly better Crucible Blow.
/// </summary>
public sealed class MercuryLance() : AlchemistCard(2, CardType.Attack, CardRarity.Uncommon, TargetType.AnyEnemy)
{
    protected override IEnumerable<DynamicVar> CanonicalVars =>
        [new DamageVar(20m, ValueProp.Move), new DamageVar("Bonus", 5m, ValueProp.Move)];

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [Tip(AlchemistTips.Infuse)];

    protected override async Task OnPlay(PlayerChoiceContext ctx, CardPlay play)
    {
        ArgumentNullException.ThrowIfNull(play.Target);

        await DamageCmd.Attack(DynamicVars.Damage.BaseValue).FromCard(this).Targeting(play.Target).Execute(ctx);
        await Alchemy.ExhaustRandomOther(ctx, Lab);
        Belt.Infuse(Lab, damage: DynamicVars["Bonus"].BaseValue);
    }

    protected override void OnUpgrade() => DynamicVars.Damage.UpgradeValueBy(5m);
}
