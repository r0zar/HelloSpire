using HelloSpire.HelloSpireCode.Alchemist.Lab;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;

namespace HelloSpire.HelloSpireCode.Alchemist.Cards;

// Rare Attacks 1-7. The class's damage ceiling, and every one of them is paid for out of a pool
// rather than out of Energy — Gold spent, Potions poured out, cards Exhausted.

/// <summary>Ten Gold doubles it. The largest single Invest on an Attack.</summary>
public sealed class PhilosophersFlame() : AlchemistCard(2, CardType.Attack, CardRarity.Rare, TargetType.AnyEnemy)
{
    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DamageVar(20m, ValueProp.Move),
        new DamageVar("Bonus", 20m, ValueProp.Move),
        new InvestVar(10m)
    ];

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [Tip(AlchemistTips.Invest)];

    protected override async Task OnPlay(PlayerChoiceContext ctx, CardPlay play)
    {
        ArgumentNullException.ThrowIfNull(play.Target);

        var bonus = await Ledger.Invest(ctx, Lab, DynamicVars["Invest"].IntValue, this)
            ? DynamicVars["Bonus"].BaseValue
            : 0m;

        await DamageCmd.Attack(DynamicVars.Damage.BaseValue + bonus)
            .FromCard(this).Targeting(play.Target).Execute(ctx);
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Damage.UpgradeValueBy(4m);
        DynamicVars["Bonus"].UpgradeValueBy(4m);
    }
}

/// <summary>X Energy, X cards traded out of Hand for random Volatile Potions. No cap: dump the whole hand if you dare.</summary>
public sealed class ChainReaction() : AlchemistCard(0, CardType.Skill, CardRarity.Rare, TargetType.Self)
{
    protected override bool HasEnergyCostX => true;

    protected override IEnumerable<DynamicVar> CanonicalVars => [new DynamicVar("Bonus", 0m)];

    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
        [Tip(AlchemistTips.Transform), Tip(AlchemistTips.Brew), Tip(AlchemistTips.Volatile)];

    protected override async Task OnPlay(PlayerChoiceContext ctx, CardPlay play)
    {
        var count = ResolveEnergyXValue();

        for (var i = 0; i < count; i++)
        {
            var candidates = Alchemy.OtherCardsInHand(Lab);
            if (candidates.Count == 0) break;

            var chosen = await LabBridge.Current.ChooseCard(ctx, Owner, candidates, this);
            if (chosen == null) break;

            await Alchemy.Exhaust(ctx, Lab, chosen);
            await Belt.BrewRandom(ctx, Lab);
        }

        for (var i = 0; i < DynamicVars["Bonus"].IntValue; i++)
            await Belt.BrewRandom(ctx, Lab);
    }

    protected override void OnUpgrade() => DynamicVars["Bonus"].UpgradeValueBy(1m);
}

/// <summary>
/// Damage for every Gold you have already burned this fight.
///
/// The Investor deck's finisher, and the only card that rewards spending rather than the thing
/// spent on. Uncapped: a long fight CAN turn it into a one-card kill -- that is the payoff.
/// </summary>
public sealed class MidasNeedle() : AlchemistCard(1, CardType.Attack, CardRarity.Rare, TargetType.AnyEnemy)
{
    protected override IEnumerable<DynamicVar> CanonicalVars =>
        [new DamageVar(5m, ValueProp.Move)];

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [Tip(AlchemistTips.Invest)];

    protected override async Task OnPlay(PlayerChoiceContext ctx, CardPlay play)
    {
        ArgumentNullException.ThrowIfNull(play.Target);

        var bonus = Ledger.SpentThisCombat(Lab);

        await DamageCmd.Attack(DynamicVars.Damage.BaseValue + bonus)
            .FromCard(this).Targeting(play.Target).Execute(ctx);
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Damage.UpgradeValueBy(3m);
    }
}

/// <summary>Feed it your most expensive card. Transform: card into damage, at Rare rates.</summary>
public sealed class MatterAnnihilation() : AlchemistCard(2, CardType.Attack, CardRarity.Rare, TargetType.AnyEnemy)
{
    protected override IEnumerable<DynamicVar> CanonicalVars =>
        [new DamageVar(8m, ValueProp.Move), new DamageVar("PerEnergy", 7m, ValueProp.Move)];

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [Tip(AlchemistTips.Transform)];

    protected override async Task OnPlay(PlayerChoiceContext ctx, CardPlay play)
    {
        ArgumentNullException.ThrowIfNull(play.Target);

        var candidates = Alchemy.OtherCardsInHand(Lab);
        var damage = DynamicVars.Damage.BaseValue;

        if (candidates.Count > 0)
        {
            var chosen = await LabBridge.Current.ChooseCard(ctx, Owner, candidates, this);
            if (chosen != null)
            {
                damage += DynamicVars["PerEnergy"].BaseValue * Math.Max(0, chosen.EnergyCost.Canonical);
                await Alchemy.Exhaust(ctx, Lab, chosen);
            }
        }

        await DamageCmd.Attack(damage).FromCard(this).Targeting(play.Target).Execute(ctx);
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Damage.UpgradeValueBy(2m);
        DynamicVars["PerEnergy"].UpgradeValueBy(1m);
    }
}

/// <summary>Damage, and eight Gold buys two free Attacks to follow it.</summary>
public sealed class HomunculusAssault() : AlchemistCard(2, CardType.Attack, CardRarity.Rare, TargetType.AnyEnemy)
{
    protected override IEnumerable<DynamicVar> CanonicalVars =>
        [new DamageVar(14m, ValueProp.Move), new InvestVar(8m), new CardsVar(2)];

    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
        [Tip(AlchemistTips.Invest), Tip(AlchemistTips.Transform)];

    protected override async Task OnPlay(PlayerChoiceContext ctx, CardPlay play)
    {
        ArgumentNullException.ThrowIfNull(play.Target);

        await DamageCmd.Attack(DynamicVars.Damage.BaseValue).FromCard(this).Targeting(play.Target).Execute(ctx);

        if (!await Ledger.Invest(ctx, Lab, DynamicVars["Invest"].IntValue, this)) return;

        for (var i = 0; i < DynamicVars.Cards.IntValue; i++)
            await Alchemy.Create(ctx, Lab, LabBridge.Current.RandomCard(Owner, type: CardType.Attack));

        await AlchemistHooks.NotifyTransformed(ctx, Lab, TransformVector.GoldToCard);
    }

    protected override void OnUpgrade() => DynamicVars.Damage.UpgradeValueBy(4m);
}

/// <summary>Kill something and get paid for it. The Transmutation deck's reason to close fights fast.</summary>
public sealed class GildedExecution() : AlchemistCard(2, CardType.Attack, CardRarity.Rare, TargetType.AnyEnemy)
{
    protected override IEnumerable<DynamicVar> CanonicalVars =>
        [new DamageVar(18m, ValueProp.Move), new DynamicVar("Gold", 15m)];

    protected override async Task OnPlay(PlayerChoiceContext ctx, CardPlay play)
    {
        ArgumentNullException.ThrowIfNull(play.Target);

        await DamageCmd.Attack(DynamicVars.Damage.BaseValue).FromCard(this).Targeting(play.Target).Execute(ctx);

        if (play.Target.CurrentHp <= 0)
            await Ledger.GainGold(ctx, Lab, DynamicVars["Gold"].IntValue);
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Damage.UpgradeValueBy(5m);
        DynamicVars["Gold"].UpgradeValueBy(5m);
    }
}

/// <summary>
/// Empty the whole belt into the room.
///
/// The Distillation deck's finisher, and the clearest statement of the Transform thesis in the
/// class: everything you were saving becomes damage, right now, and the belt is bare afterwards.
/// </summary>
public sealed class GrandCombustion() : AlchemistCard(3, CardType.Attack, CardRarity.Rare, TargetType.AllEnemies)
{
    protected override IEnumerable<DynamicVar> CanonicalVars =>
        [new DamageVar(18m, ValueProp.Move), new DamageVar("PerPotion", 6m, ValueProp.Move)];

    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];

    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
        [Tip(AlchemistTips.Distill), Tip(AlchemistTips.Transform)];

    protected override async Task OnPlay(PlayerChoiceContext ctx, CardPlay play)
    {
        var poured = await Belt.DistillAny(ctx, Lab, Belt.Held(Lab).Count);
        var damage = DynamicVars.Damage.BaseValue + DynamicVars["PerPotion"].BaseValue * poured;

        foreach (var enemy in AlchemistEffects.Enemies(Lab))
            await DamageCmd.Attack(damage).FromCard(this).Targeting(enemy).Execute(ctx);

        if (poured > 0) await AlchemistHooks.NotifyTransformed(ctx, Lab, TransformVector.PotionToTempo);
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Damage.UpgradeValueBy(4m);
        DynamicVars["PerPotion"].UpgradeValueBy(1m);
    }
}
