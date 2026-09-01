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

// Rare Attacks 1-7. The class's damage ceiling -- every one of them either Infuses Unstable
// Concoction, applies a lot of Poison, or scales off board state built up over the whole fight.

/// <summary>Deal damage, and Infuse a lot more into Unstable Concoction.</summary>
public sealed class PhilosophersFlame() : AlchemistCard(3, CardType.Attack, CardRarity.Rare, TargetType.AnyEnemy)
{
    protected override IEnumerable<DynamicVar> CanonicalVars =>
        [new DamageVar(20m, ValueProp.Move), new DamageVar("Bonus", 20m, ValueProp.Move)];

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [Tip(AlchemistTips.Infuse)];

    protected override async Task OnPlay(PlayerChoiceContext ctx, CardPlay play)
    {
        ArgumentNullException.ThrowIfNull(play.Target);

        await DamageCmd.Attack(DynamicVars.Damage.BaseValue).FromCard(this).Targeting(play.Target).Execute(ctx);
        await Belt.Infuse(ctx, Lab, damage: DynamicVars["Bonus"].BaseValue);
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Damage.UpgradeValueBy(4m);
        DynamicVars["Bonus"].UpgradeValueBy(4m);
    }
}

/// <summary>Damage to ALL, more for every Potion in your Belt.</summary>
public sealed class ChainReaction() : AlchemistCard(2, CardType.Attack, CardRarity.Rare, TargetType.AllEnemies)
{
    protected override IEnumerable<DynamicVar> CanonicalVars =>
        [new DamageVar(14m, ValueProp.Move), new DamageVar("PerPotion", 3m, ValueProp.Move)];

    protected override async Task OnPlay(PlayerChoiceContext ctx, CardPlay play)
    {
        var damage = DynamicVars.Damage.BaseValue + DynamicVars["PerPotion"].BaseValue * Belt.Held(Lab).Count;

        foreach (var enemy in AlchemistEffects.Enemies(Lab))
            await DamageCmd.Attack(damage).FromCard(this).Targeting(enemy).Execute(ctx);
    }

    protected override void OnUpgrade() => DynamicVars.Damage.UpgradeValueBy(3m);
}

/// <summary>Feed it your most expensive card, for damage now and Infuse besides.</summary>
public sealed class MatterAnnihilation() : AlchemistCard(2, CardType.Attack, CardRarity.Rare, TargetType.AnyEnemy)
{
    protected override IEnumerable<DynamicVar> CanonicalVars =>
        [new DamageVar(10m, ValueProp.Move), new DamageVar("PerEnergy", 8m, ValueProp.Move)];

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [Tip(AlchemistTips.Infuse)];

    protected override async Task OnPlay(PlayerChoiceContext ctx, CardPlay play)
    {
        ArgumentNullException.ThrowIfNull(play.Target);

        var candidates = Alchemy.OtherCardsInHand(Lab);
        var damage = DynamicVars.Damage.BaseValue;
        var bonus = 0m;

        if (candidates.Count > 0)
        {
            var chosen = await LabBridge.Current.ChooseCard(ctx, Owner, candidates, this);
            if (chosen != null)
            {
                bonus = DynamicVars["PerEnergy"].BaseValue * Math.Max(0, chosen.EnergyCost.Canonical);
                damage += bonus;
                await Alchemy.Exhaust(ctx, Lab, chosen);
            }
        }

        await DamageCmd.Attack(damage).FromCard(this).Targeting(play.Target).Execute(ctx);
        if (bonus > 0) await Belt.Infuse(ctx, Lab, damage: bonus / 2m);
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Damage.UpgradeValueBy(2m);
        DynamicVars["PerEnergy"].UpgradeValueBy(1m);
    }
}

/// <summary>Damage, and two free Attacks to follow it.</summary>
public sealed class HomunculusAssault() : AlchemistCard(2, CardType.Attack, CardRarity.Rare, TargetType.AnyEnemy)
{
    protected override IEnumerable<DynamicVar> CanonicalVars =>
        [new DamageVar(14m, ValueProp.Move), new CardsVar(2)];

    protected override async Task OnPlay(PlayerChoiceContext ctx, CardPlay play)
    {
        ArgumentNullException.ThrowIfNull(play.Target);

        await DamageCmd.Attack(DynamicVars.Damage.BaseValue).FromCard(this).Targeting(play.Target).Execute(ctx);

        for (var i = 0; i < DynamicVars.Cards.IntValue; i++)
            await Alchemy.Create(ctx, Lab, LabBridge.Current.RandomCard(Owner, type: CardType.Attack));
    }

    protected override void OnUpgrade() => DynamicVars.Damage.UpgradeValueBy(4m);
}

/// <summary>Kill something and Brew a Rare Potion for it.</summary>
public sealed class GildedExecution() : AlchemistCard(2, CardType.Attack, CardRarity.Rare, TargetType.AnyEnemy)
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [new DamageVar(18m, ValueProp.Move)];

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [Tip(AlchemistTips.Brew)];

    protected override async Task OnPlay(PlayerChoiceContext ctx, CardPlay play)
    {
        ArgumentNullException.ThrowIfNull(play.Target);

        await DamageCmd.Attack(DynamicVars.Damage.BaseValue).FromCard(this).Targeting(play.Target).Execute(ctx);

        if (play.Target.CurrentHp <= 0)
            await Belt.BrewRandom(ctx, Lab, PotionRarity.Rare);
    }

    protected override void OnUpgrade() => DynamicVars.Damage.UpgradeValueBy(5m);
}

/// <summary>
/// Empty the whole belt into the room.
///
/// The Distillation deck's finisher: everything you were saving becomes damage and Poison to
/// every enemy, right now -- no partial version, no choice, the whole belt goes.
/// </summary>
public sealed class GrandCombustion() : AlchemistCard(3, CardType.Attack, CardRarity.Rare, TargetType.AllEnemies)
{
    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DamageVar(16m, ValueProp.Move),
        new DamageVar("PerPotion", 8m, ValueProp.Move),
        new DynamicVar("Poison", 2m)
    ];

    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
        [Tip(AlchemistTips.Distill), HoverTipFactory.FromPower<PoisonPower>()];

    protected override async Task OnPlay(PlayerChoiceContext ctx, CardPlay play)
    {
        var poured = 0;
        foreach (var potion in Belt.Held(Lab).ToList())
            if ((await Belt.Distill(ctx, Lab, potion)).Distilled) poured++;

        var damage = DynamicVars.Damage.BaseValue + DynamicVars["PerPotion"].BaseValue * poured;
        var poison = DynamicVars["Poison"].BaseValue * poured;

        foreach (var enemy in AlchemistEffects.Enemies(Lab))
        {
            await DamageCmd.Attack(damage).FromCard(this).Targeting(enemy).Execute(ctx);
            if (poison > 0) await AlchemistEffects.ApplyPoison(ctx, Lab, enemy, poison);
        }
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Damage.UpgradeValueBy(4m);
        DynamicVars["PerPotion"].UpgradeValueBy(1m);
    }
}

/// <summary>Deal damage, and Infuse Unstable Concoction for every different Potion type you've used this fight.</summary>
public sealed class CatalyticExplosion() : AlchemistCard(2, CardType.Attack, CardRarity.Rare, TargetType.AnyEnemy)
{
    protected override IEnumerable<DynamicVar> CanonicalVars =>
        [new DamageVar(12m, ValueProp.Move), new DamageVar("PerType", 5m, ValueProp.Move)];

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [Tip(AlchemistTips.Infuse)];

    protected override async Task OnPlay(PlayerChoiceContext ctx, CardPlay play)
    {
        ArgumentNullException.ThrowIfNull(play.Target);

        await DamageCmd.Attack(DynamicVars.Damage.BaseValue).FromCard(this).Targeting(play.Target).Execute(ctx);

        var types = AlchemistEffects.Peek(Lab)?.UsedThisCombat.Select(p => p.GetType()).Distinct().Count() ?? 0;
        await Belt.Infuse(ctx, Lab, damage: DynamicVars["PerType"].BaseValue * types);
    }

    protected override void OnUpgrade() => DynamicVars.Damage.UpgradeValueBy(4m);
}
