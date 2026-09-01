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

// Rare Attacks 1-7. The class's damage ceiling, and every one of them either Infuses Unstable
// Concoction, applies Poison, or scales off board state built up over the whole fight.

/// <summary>Deal damage, and Infuse a lot more into Unstable Concoction.</summary>
public sealed class PhilosophersFlame() : AlchemistCard(2, CardType.Attack, CardRarity.Rare, TargetType.AnyEnemy)
{
    protected override IEnumerable<DynamicVar> CanonicalVars =>
        [new DamageVar(20m, ValueProp.Move), new DamageVar("Bonus", 20m, ValueProp.Move)];

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [Tip(AlchemistTips.Infuse)];

    protected override async Task OnPlay(PlayerChoiceContext ctx, CardPlay play)
    {
        ArgumentNullException.ThrowIfNull(play.Target);

        await DamageCmd.Attack(DynamicVars.Damage.BaseValue).FromCard(this).Targeting(play.Target).Execute(ctx);
        Belt.Infuse(Lab, damage: DynamicVars["Bonus"].BaseValue);
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Damage.UpgradeValueBy(4m);
        DynamicVars["Bonus"].UpgradeValueBy(4m);
    }
}

/// <summary>Damage to ALL, more and Poisoned the more Potions you've used this turn.</summary>
public sealed class ChainReaction() : AlchemistCard(2, CardType.Attack, CardRarity.Rare, TargetType.AllEnemies)
{
    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DamageVar(10m, ValueProp.Move),
        new DamageVar("PerPotion", 4m, ValueProp.Move),
        new DynamicVar("Poison", 1m),
        new DynamicVar("Max", 3m)
    ];

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.FromPower<PoisonPower>()];

    protected override async Task OnPlay(PlayerChoiceContext ctx, CardPlay play)
    {
        var used = Math.Min((AlchemistEffects.Peek(Lab)?.PotionsUsedThisTurn ?? 0), DynamicVars["Max"].IntValue);
        var damage = DynamicVars.Damage.BaseValue + DynamicVars["PerPotion"].BaseValue * used;

        foreach (var enemy in AlchemistEffects.Enemies(Lab))
        {
            await DamageCmd.Attack(damage).FromCard(this).Targeting(enemy).Execute(ctx);
            for (var i = 0; i < used; i++)
                await AlchemistEffects.ApplyPoison(ctx, Lab, enemy, DynamicVars["Poison"].BaseValue);
        }
    }

    protected override void OnUpgrade() => DynamicVars.Damage.UpgradeValueBy(3m);
}

/// <summary>
/// Damage for every Potion you've already Distilled this fight.
///
/// The Distillation deck's finisher. Uncapped in spirit, capped in practice: a long fight CAN turn
/// it into a one-card kill -- that is the payoff.
/// </summary>
public sealed class RefinedNeedle() : AlchemistCard(1, CardType.Attack, CardRarity.Rare, TargetType.AnyEnemy)
{
    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DamageVar(5m, ValueProp.Move),
        new DamageVar("PerPotion", 2m, ValueProp.Move),
        new DynamicVar("Max", 30m)
    ];

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [Tip(AlchemistTips.Distill)];

    protected override async Task OnPlay(PlayerChoiceContext ctx, CardPlay play)
    {
        ArgumentNullException.ThrowIfNull(play.Target);

        var distilled = AlchemistEffects.Peek(Lab)?.DistilledThisCombat ?? 0;
        var bonus = Math.Min(DynamicVars["PerPotion"].BaseValue * distilled, DynamicVars["Max"].BaseValue);

        await DamageCmd.Attack(DynamicVars.Damage.BaseValue + bonus)
            .FromCard(this).Targeting(play.Target).Execute(ctx);
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
        if (bonus > 0) Belt.Infuse(Lab, damage: bonus / 2m);
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Damage.UpgradeValueBy(2m);
        DynamicVars["PerEnergy"].UpgradeValueBy(1m);
    }
}

/// <summary>Damage, and two free Attacks and a little Infuse to follow it.</summary>
public sealed class HomunculusAssault() : AlchemistCard(2, CardType.Attack, CardRarity.Rare, TargetType.AnyEnemy)
{
    protected override IEnumerable<DynamicVar> CanonicalVars =>
        [new DamageVar(14m, ValueProp.Move), new CardsVar(2), new DamageVar("Bonus", 8m, ValueProp.Move)];

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [Tip(AlchemistTips.Infuse)];

    protected override async Task OnPlay(PlayerChoiceContext ctx, CardPlay play)
    {
        ArgumentNullException.ThrowIfNull(play.Target);

        await DamageCmd.Attack(DynamicVars.Damage.BaseValue).FromCard(this).Targeting(play.Target).Execute(ctx);

        for (var i = 0; i < DynamicVars.Cards.IntValue; i++)
            await Alchemy.Create(ctx, Lab, LabBridge.Current.RandomCard(Owner, type: CardType.Attack));

        Belt.Infuse(Lab, damage: DynamicVars["Bonus"].BaseValue);
    }

    protected override void OnUpgrade() => DynamicVars.Damage.UpgradeValueBy(4m);
}

/// <summary>Kill something and Brew a Rare Potion for it, plus a little Infuse.</summary>
public sealed class GildedExecution() : AlchemistCard(2, CardType.Attack, CardRarity.Rare, TargetType.AnyEnemy)
{
    protected override IEnumerable<DynamicVar> CanonicalVars =>
        [new DamageVar(18m, ValueProp.Move), new DamageVar("Bonus", 10m, ValueProp.Move)];

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [Tip(AlchemistTips.Brew), Tip(AlchemistTips.Infuse)];

    protected override async Task OnPlay(PlayerChoiceContext ctx, CardPlay play)
    {
        ArgumentNullException.ThrowIfNull(play.Target);

        await DamageCmd.Attack(DynamicVars.Damage.BaseValue).FromCard(this).Targeting(play.Target).Execute(ctx);

        if (play.Target.CurrentHp <= 0)
        {
            await Belt.BrewRandom(ctx, Lab, PotionRarity.Rare);
            Belt.Infuse(Lab, damage: DynamicVars["Bonus"].BaseValue);
        }
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Damage.UpgradeValueBy(5m);
        DynamicVars["Bonus"].UpgradeValueBy(5m);
    }
}

/// <summary>
/// Empty the whole belt into the room.
///
/// The Distillation deck's finisher: as much of what you were saving as you're willing to give up
/// becomes damage and Poison, right now.
/// </summary>
public sealed class GrandCombustion() : AlchemistCard(3, CardType.Attack, CardRarity.Rare, TargetType.AllEnemies)
{
    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DamageVar(18m, ValueProp.Move),
        new DamageVar("PerPotion", 6m, ValueProp.Move),
        new DynamicVar("Poison", 2m)
    ];

    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];

    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
        [Tip(AlchemistTips.Distill), HoverTipFactory.FromPower<PoisonPower>()];

    protected override async Task OnPlay(PlayerChoiceContext ctx, CardPlay play)
    {
        var poured = await Belt.DistillAny(ctx, Lab, Belt.Held(Lab).Count);
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
