using HelloSpire.HelloSpireCode.Alchemist.Lab;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.Nodes.CommonUi;
using MegaCrit.Sts2.Core.ValueProps;

namespace HelloSpire.HelloSpireCode.Alchemist.Cards;

// Uncommon Attacks 1-12. Between them they read every board state the class controls: how full the
// belt is, how empty it is, what you drank, what you Distilled, what you Exhausted, what you
// created, and how much money you have made and spent.

/// <summary>Damage scaling with a full belt. Full Belt's headline card.</summary>
public sealed class BottleBarrage() : AlchemistCard(1, CardType.Attack, CardRarity.Uncommon, TargetType.AnyEnemy)
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [new DamageVar("PerPotion", 3m, ValueProp.Move)];

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

/// <summary>Hit, then pour a Potion out to hit again. Transform: Potion into damage.</summary>
public sealed class Shatterstock() : AlchemistCard(1, CardType.Attack, CardRarity.Uncommon, TargetType.AnyEnemy)
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [new DamageVar(9m, ValueProp.Move)];

    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
        [Tip(AlchemistTips.Distill), Tip(AlchemistTips.Transform)];

    protected override async Task OnPlay(PlayerChoiceContext ctx, CardPlay play)
    {
        ArgumentNullException.ThrowIfNull(play.Target);

        await DamageCmd.Attack(DynamicVars.Damage.BaseValue).FromCard(this).Targeting(play.Target).Execute(ctx);

        if (!(await Belt.Distill(ctx, Lab)).Distilled) return;

        await DamageCmd.Attack(DynamicVars.Damage.BaseValue).FromCard(this).Targeting(play.Target).Execute(ctx);
        await AlchemistHooks.NotifyTransformed(ctx, Lab, TransformVector.PotionToTempo);
    }

    protected override void OnUpgrade() => DynamicVars.Damage.UpgradeValueBy(3m);
}

/// <summary>Rig a held Potion, real or Volatile, to go off twice the next time it's used.</summary>
public sealed class PressureBurst() : AlchemistCard(1, CardType.Skill, CardRarity.Uncommon, TargetType.Self)
{
    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];

    protected override async Task OnPlay(PlayerChoiceContext ctx, CardPlay play)
    {
        var held = Belt.Held(Lab);
        if (held.Count == 0) return;

        var chosen = held.Count == 1 ? held[0] : await LabBridge.Current.ChoosePotion(ctx, Owner, held,
            new LocString("cards", "HELLOSPIRE-ALCHEMIST_DOUBLE_CHOICE.header"));
        if (chosen == null) return;

        var bench = await AlchemistEffects.Bench(ctx, Lab);
        bench?.DoubleActivate.Add(chosen);
    }

    protected override void OnUpgrade() => EnergyCost.UpgradeBy(-1);
}

/// <summary>Free, and better when you have already spent everything.</summary>
public sealed class EmptyBottle() : AlchemistCard(0, CardType.Attack, CardRarity.Uncommon, TargetType.AnyEnemy)
{
    protected override IEnumerable<DynamicVar> CanonicalVars =>
        [new DamageVar(4m, ValueProp.Move), new DamageVar("Bonus", 4m, ValueProp.Move), new CardsVar(1)];

    protected override async Task OnPlay(PlayerChoiceContext ctx, CardPlay play)
    {
        ArgumentNullException.ThrowIfNull(play.Target);

        var empty = Belt.IsEmpty(Lab);
        var bonus = empty ? DynamicVars["Bonus"].BaseValue : 0m;

        await DamageCmd.Attack(DynamicVars.Damage.BaseValue + bonus)
            .FromCard(this).Targeting(play.Target).Execute(ctx);

        if (empty) await AlchemistEffects.Draw(ctx, Lab, DynamicVars.Cards.IntValue);
    }

    protected override void OnUpgrade() => DynamicVars.Damage.UpgradeValueBy(2m);
}

/// <summary>The Exhaust deck's only source of Vulnerable.</summary>
public sealed class CinnabarEdge() : AlchemistCard(1, CardType.Attack, CardRarity.Uncommon, TargetType.AnyEnemy)
{
    protected override IEnumerable<DynamicVar> CanonicalVars =>
        [new DamageVar(9m, ValueProp.Move), new PowerVar<VulnerablePower>(2m)];

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.FromPower<VulnerablePower>()];

    protected override async Task OnPlay(PlayerChoiceContext ctx, CardPlay play)
    {
        ArgumentNullException.ThrowIfNull(play.Target);

        await DamageCmd.Attack(DynamicVars.Damage.BaseValue).FromCard(this).Targeting(play.Target).Execute(ctx);

        if ((AlchemistEffects.Peek(Lab)?.CardsExhaustedThisTurn ?? 0) > 0)
            await AlchemistEffects.ApplyVulnerable(ctx, Lab, play.Target, DynamicVars["VulnerablePower"].BaseValue);
    }

    protected override void OnUpgrade() => DynamicVars.Damage.UpgradeValueBy(3m);
}

/// <summary>Pay as much as you like, a Gold at a time. The variable Invest.</summary>
public sealed class BlackMarketBlade() : AlchemistCard(1, CardType.Attack, CardRarity.Uncommon, TargetType.AnyEnemy)
{
    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DamageVar(8m, ValueProp.Move),
        new DamageVar("PerGold", 2m, ValueProp.Move),
        new DynamicVar("MaxInvest", 5m)
    ];

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [Tip(AlchemistTips.Invest)];

    protected override async Task OnPlay(PlayerChoiceContext ctx, CardPlay play)
    {
        ArgumentNullException.ThrowIfNull(play.Target);

        var paid = await Ledger.InvestUpTo(ctx, Lab, DynamicVars["MaxInvest"].IntValue);

        await DamageCmd.Attack(DynamicVars.Damage.BaseValue + DynamicVars["PerGold"].BaseValue * paid)
            .FromCard(this).Targeting(play.Target).Execute(ctx);
    }

    protected override void OnUpgrade() => DynamicVars.Damage.UpgradeValueBy(3m);
}

/// <summary>
/// Heavy damage, and a real, visible cost drop once you've drunk something this turn -- no matter
/// whether that happened before or after this exact copy showed up.
///
/// Was implemented as an after-the-fact Energy refund (matching the Gunslinger's Quickdraw
/// Legend), which nets the same Energy total in the common case but never changes the number
/// printed on the card -- confirmed live: the card looked and cost exactly the same regardless of
/// whether the discount was "active." Rebuilt on CardEnergyCost.SetThisTurnOrUntilPlayed instead
/// (base-game precedent: Enlightenment, Flatten), triggered two ways so timing never matters:
/// IPotionUseListener.OnPotionUsed for a copy already sitting in Hand when you drink something
/// (AlchemistHooks.Listeners checks Hand cards, not just Relics and Powers, for exactly this), and
/// AbstractModel.AfterCardDrawn (decompiled from sts2.dll -- CardModel is itself an AbstractModel,
/// so a card can react to its own draw directly) for a copy that shows up afterward, checking the
/// same PotionsUsedThisTurn counter every other "did you drink something" card in this class reads.
/// </summary>
public sealed class VolatileCompound() : AlchemistCard(2, CardType.Attack, CardRarity.Uncommon, TargetType.AnyEnemy), IPotionUseListener
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [new DamageVar(18m, ValueProp.Move)];

    public Task OnPotionUsed(PlayerChoiceContext ctx, LabContext lab, PotionModel potion)
    {
        EnergyCost.SetThisTurnOrUntilPlayed(1, reduceOnly: true);
        return Task.CompletedTask;
    }

    public override Task AfterCardDrawn(PlayerChoiceContext ctx, CardModel card, bool fromHandDraw)
    {
        if ((AlchemistEffects.Peek(Lab)?.PotionsUsedThisTurn ?? 0) > 0)
            EnergyCost.SetThisTurnOrUntilPlayed(1, reduceOnly: true);
        return Task.CompletedTask;
    }

    protected override async Task OnPlay(PlayerChoiceContext ctx, CardPlay play)
    {
        ArgumentNullException.ThrowIfNull(play.Target);

        await DamageCmd.Attack(DynamicVars.Damage.BaseValue).FromCard(this).Targeting(play.Target).Execute(ctx);
    }

    protected override void OnUpgrade() => DynamicVars.Damage.UpgradeValueBy(4m);
}

/// <summary>AoE you can pay to make bigger.</summary>
public sealed class FlashPowder() : AlchemistCard(1, CardType.Attack, CardRarity.Uncommon, TargetType.AllEnemies)
{
    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DamageVar(6m, ValueProp.Move),
        new DamageVar("Bonus", 5m, ValueProp.Move),
        new DynamicVar("Invest", 4m)
    ];

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [Tip(AlchemistTips.Invest)];

    protected override async Task OnPlay(PlayerChoiceContext ctx, CardPlay play)
    {
        var bonus = await Ledger.Invest(ctx, Lab, DynamicVars["Invest"].IntValue)
            ? DynamicVars["Bonus"].BaseValue
            : 0m;

        foreach (var enemy in AlchemistEffects.Enemies(Lab))
        {
            await DamageCmd.Attack(DynamicVars.Damage.BaseValue + bonus)
                .FromCard(this).Targeting(enemy).Execute(ctx);
        }
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Damage.UpgradeValueBy(2m);
        DynamicVars["Bonus"].UpgradeValueBy(1m);
    }
}

/// <summary>Damage that grows with how much you have earned this fight. Uncapped: let it run.</summary>
public sealed class AuricNeedle() : AlchemistCard(1, CardType.Attack, CardRarity.Uncommon, TargetType.AnyEnemy)
{
    protected override IEnumerable<DynamicVar> CanonicalVars =>
        [new DamageVar(7m, ValueProp.Move)];

    protected override async Task OnPlay(PlayerChoiceContext ctx, CardPlay play)
    {
        ArgumentNullException.ThrowIfNull(play.Target);

        var bonus = Ledger.GainedThisCombat(Lab);

        await DamageCmd.Attack(DynamicVars.Damage.BaseValue + bonus)
            .FromCard(this).Targeting(play.Target).Execute(ctx);
    }

    protected override void OnUpgrade() => DynamicVars.Damage.UpgradeValueBy(3m);
}

/// <summary>Two hits, three if you poured something out this turn.</summary>
public sealed class Corkscrew() : AlchemistCard(1, CardType.Attack, CardRarity.Uncommon, TargetType.AnyEnemy)
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [new DamageVar(5m, ValueProp.Move)];

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [Tip(AlchemistTips.Distill)];

    protected override async Task OnPlay(PlayerChoiceContext ctx, CardPlay play)
    {
        ArgumentNullException.ThrowIfNull(play.Target);

        var hits = (AlchemistEffects.Peek(Lab)?.DistilledThisTurn ?? 0) > 0 ? 3 : 2;

        for (var i = 0; i < hits; i++)
        {
            await DamageCmd.Attack(DynamicVars.Damage.BaseValue)
                .FromCard(this).Targeting(play.Target).Execute(ctx);
        }
    }

    protected override void OnUpgrade() => DynamicVars.Damage.UpgradeValueBy(2m);
}

/// <summary>Trade a card in Hand for a stranger of the same rarity. Upgraded, the stranger arrives Upgraded too.</summary>
public sealed class ReactiveSlash() : AlchemistCard(1, CardType.Skill, CardRarity.Uncommon, TargetType.Self)
{
    protected override IEnumerable<IHoverTip> ExtraHoverTips => [Tip(AlchemistTips.Transform)];

    protected override async Task OnPlay(PlayerChoiceContext ctx, CardPlay play)
    {
        var candidates = Alchemy.OtherCardsInHand(Lab);
        if (candidates.Count == 0) return;

        var chosen = await LabBridge.Current.ChooseCard(ctx, Owner, candidates, this);
        if (chosen == null) return;

        // Basic-rarity cards (Strike/Defend) have no "random other Basic" pool to draw from --
        // RandomCard's own filter excludes Basic/Status/Curse entirely. Rather than Exhaust one
        // for nothing, a Basic steps up to a random Common instead of matching its own rarity.
        var rarity = chosen.Rarity == CardRarity.Basic ? CardRarity.Common : chosen.Rarity;
        var replacement = LabBridge.Current.RandomCard(Owner, rarity: rarity);

        await Alchemy.Exhaust(ctx, Lab, chosen);
        await Alchemy.Create(ctx, Lab, replacement);

        // No numeric var to scale, so the upgrade is a qualitative one checked here rather than
        // in OnUpgrade: IsUpgraded is already true by the time OnPlay runs an upgraded copy.
        if (IsUpgraded && replacement is { IsUpgradable: true })
            CardCmd.Upgrade(replacement, CardPreviewStyle.None);
    }

    protected override void OnUpgrade() => EnergyCost.UpgradeBy(-1);
}

/// <summary>
/// The biggest uncommon hit in the class, and it eats a card out of your Hand at random to do it.
///
/// Random rather than chosen on purpose: this is the only Exhaust in the set the player does not
/// get to aim, which is what stops it being a strictly better Crucible Blow.
/// </summary>
public sealed class MercuryLance() : AlchemistCard(2, CardType.Attack, CardRarity.Uncommon, TargetType.AnyEnemy)
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [new DamageVar(20m, ValueProp.Move)];

    protected override async Task OnPlay(PlayerChoiceContext ctx, CardPlay play)
    {
        ArgumentNullException.ThrowIfNull(play.Target);

        await DamageCmd.Attack(DynamicVars.Damage.BaseValue).FromCard(this).Targeting(play.Target).Execute(ctx);
        await Alchemy.ExhaustRandomOther(ctx, Lab);
    }

    protected override void OnUpgrade() => DynamicVars.Damage.UpgradeValueBy(5m);
}
