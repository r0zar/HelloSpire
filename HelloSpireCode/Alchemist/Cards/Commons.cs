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
// character controls — did you drink something this turn, is the belt empty, did you make money —
// so that the commons teach the payoff conditions the uncommons and rares are built on, without
// any of them being exciting enough to want in every deck.

// ---------------------------------------------------------------------------- Attacks

/// <summary>Deal damage, more if you drank something this turn.</summary>
public sealed class FlaskToss() : AlchemistCard(1, CardType.Attack, CardRarity.Common, TargetType.AnyEnemy)
{
    protected override IEnumerable<DynamicVar> CanonicalVars =>
        [new DamageVar(8m, ValueProp.Move), new DamageVar("Bonus", 4m, ValueProp.Move)];

    protected override bool ShouldGlowGoldInternal => (AlchemistEffects.Peek(Lab)?.PotionsUsedThisTurn ?? 0) > 0;

    protected override async Task OnPlay(PlayerChoiceContext ctx, CardPlay play)
    {
        ArgumentNullException.ThrowIfNull(play.Target);

        var bonus = (AlchemistEffects.Peek(Lab)?.PotionsUsedThisTurn ?? 0) > 0
            ? DynamicVars["Bonus"].BaseValue
            : 0m;

        await DamageCmd.Attack(DynamicVars.Damage.BaseValue + bonus)
            .FromCard(this).Targeting(play.Target).Execute(ctx);
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Damage.UpgradeValueBy(2m);
        DynamicVars["Bonus"].UpgradeValueBy(1m);
    }
}

/// <summary>Deal damage, more if the belt has room. The empty-belt cluster's cheapest member.</summary>
public sealed class GlassShard() : AlchemistCard(1, CardType.Attack, CardRarity.Common, TargetType.AnyEnemy)
{
    protected override IEnumerable<DynamicVar> CanonicalVars =>
        [new DamageVar(9m, ValueProp.Move), new DamageVar("Bonus", 3m, ValueProp.Move)];

    protected override async Task OnPlay(PlayerChoiceContext ctx, CardPlay play)
    {
        ArgumentNullException.ThrowIfNull(play.Target);

        var bonus = Belt.EmptySlots(Lab) > 0 ? DynamicVars["Bonus"].BaseValue : 0m;

        await DamageCmd.Attack(DynamicVars.Damage.BaseValue + bonus)
            .FromCard(this).Targeting(play.Target).Execute(ctx);
    }

    protected override void OnUpgrade() => DynamicVars.Damage.UpgradeValueBy(3m);
}

/// <summary>AoE, more if you drank something this turn.</summary>
public sealed class ScatterFlask() : AlchemistCard(1, CardType.Attack, CardRarity.Common, TargetType.AllEnemies)
{
    protected override IEnumerable<DynamicVar> CanonicalVars =>
        [new DamageVar(5m, ValueProp.Move), new DamageVar("Bonus", 3m, ValueProp.Move)];

    protected override async Task OnPlay(PlayerChoiceContext ctx, CardPlay play)
    {
        var bonus = (AlchemistEffects.Peek(Lab)?.PotionsUsedThisTurn ?? 0) > 0
            ? DynamicVars["Bonus"].BaseValue
            : 0m;

        foreach (var enemy in AlchemistEffects.Enemies(Lab))
        {
            await DamageCmd.Attack(DynamicVars.Damage.BaseValue + bonus)
                .FromCard(this).Targeting(enemy).Execute(ctx);
        }
    }

    protected override void OnUpgrade() => DynamicVars.Damage.UpgradeValueBy(2m);
}

/// <summary>Deal damage, more if you made money this turn. The Transmutation deck's attack.</summary>
public sealed class GildedScalpel() : AlchemistCard(1, CardType.Attack, CardRarity.Common, TargetType.AnyEnemy)
{
    protected override IEnumerable<DynamicVar> CanonicalVars =>
        [new DamageVar(8m, ValueProp.Move), new DamageVar("Bonus", 3m, ValueProp.Move)];

    protected override bool ShouldGlowGoldInternal => Ledger.GainedThisTurn(Lab);

    protected override async Task OnPlay(PlayerChoiceContext ctx, CardPlay play)
    {
        ArgumentNullException.ThrowIfNull(play.Target);

        var bonus = Ledger.GainedThisTurn(Lab) ? DynamicVars["Bonus"].BaseValue : 0m;

        await DamageCmd.Attack(DynamicVars.Damage.BaseValue + bonus)
            .FromCard(this).Targeting(play.Target).Execute(ctx);
    }

    protected override void OnUpgrade() => DynamicVars.Damage.UpgradeValueBy(3m);
}

/// <summary>Big damage, bigger for three Gold. The card that teaches Invest.</summary>
public sealed class PyricBurst() : AlchemistCard(2, CardType.Attack, CardRarity.Common, TargetType.AnyEnemy)
{
    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DamageVar(16m, ValueProp.Move),
        new DamageVar("Bonus", 6m, ValueProp.Move),
        new DynamicVar("Invest", 3m)
    ];

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [Tip(AlchemistTips.Invest)];

    protected override async Task OnPlay(PlayerChoiceContext ctx, CardPlay play)
    {
        ArgumentNullException.ThrowIfNull(play.Target);

        var paid = await Ledger.Invest(ctx, Lab, DynamicVars["Invest"].IntValue);
        var bonus = paid ? DynamicVars["Bonus"].BaseValue : 0m;

        await DamageCmd.Attack(DynamicVars.Damage.BaseValue + bonus)
            .FromCard(this).Targeting(play.Target).Execute(ctx);
    }

    protected override void OnUpgrade() => DynamicVars.Damage.UpgradeValueBy(4m);
}

/// <summary>Free damage, doubled if a slot opened up this turn.</summary>
public sealed class QuickSilver() : AlchemistCard(0, CardType.Attack, CardRarity.Common, TargetType.AnyEnemy)
{
    protected override IEnumerable<DynamicVar> CanonicalVars =>
        [new DamageVar(4m, ValueProp.Move), new DamageVar("Bonus", 4m, ValueProp.Move)];

    protected override async Task OnPlay(PlayerChoiceContext ctx, CardPlay play)
    {
        ArgumentNullException.ThrowIfNull(play.Target);

        var bonus = (AlchemistEffects.Peek(Lab)?.SlotsEmptiedThisTurn ?? 0) > 0
            ? DynamicVars["Bonus"].BaseValue
            : 0m;

        await DamageCmd.Attack(DynamicVars.Damage.BaseValue + bonus)
            .FromCard(this).Targeting(play.Target).Execute(ctx);
    }

    protected override void OnUpgrade() => DynamicVars.Damage.UpgradeValueBy(2m);
}

/// <summary>Deal damage; feed it a card from your Hand for more. Transform: card into damage.</summary>
public sealed class CrucibleBlow() : AlchemistCard(1, CardType.Attack, CardRarity.Common, TargetType.AnyEnemy)
{
    protected override IEnumerable<DynamicVar> CanonicalVars =>
        [new DamageVar(8m, ValueProp.Move), new DamageVar("Bonus", 6m, ValueProp.Move)];

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [Tip(AlchemistTips.Transform)];

    protected override async Task OnPlay(PlayerChoiceContext ctx, CardPlay play)
    {
        ArgumentNullException.ThrowIfNull(play.Target);

        var bonus = await Alchemy.ExhaustOne(ctx, Lab) ? DynamicVars["Bonus"].BaseValue : 0m;

        await DamageCmd.Attack(DynamicVars.Damage.BaseValue + bonus)
            .FromCard(this).Targeting(play.Target).Execute(ctx);
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Damage.UpgradeValueBy(2m);
        DynamicVars["Bonus"].UpgradeValueBy(1m);
    }
}

/// <summary>Deal damage; two Gold buys a Weak. The cheapest Invest in the set.</summary>
public sealed class CopperShot() : AlchemistCard(1, CardType.Attack, CardRarity.Common, TargetType.AnyEnemy)
{
    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DamageVar(7m, ValueProp.Move),
        new PowerVar<WeakPower>(1m),
        new DynamicVar("Invest", 2m)
    ];

    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
        [Tip(AlchemistTips.Invest), HoverTipFactory.FromPower<WeakPower>()];

    protected override async Task OnPlay(PlayerChoiceContext ctx, CardPlay play)
    {
        ArgumentNullException.ThrowIfNull(play.Target);

        await DamageCmd.Attack(DynamicVars.Damage.BaseValue)
            .FromCard(this).Targeting(play.Target).Execute(ctx);

        if (await Ledger.Invest(ctx, Lab, DynamicVars["Invest"].IntValue))
            await AlchemistEffects.ApplyWeak(ctx, Lab, play.Target, DynamicVars["WeakPower"].BaseValue);
    }

    protected override void OnUpgrade() => DynamicVars.Damage.UpgradeValueBy(3m);
}

// ---------------------------------------------------------------------------- Skills

/// <summary>Exhaust a card, gain Gold. The class's defining conversion, at its cheapest.</summary>
public sealed class Transmute() : AlchemistCard(1, CardType.Skill, CardRarity.Common, TargetType.Self)
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [new DynamicVar("Gold", 5m)];

    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [Tip(AlchemistTips.Transform)];

    protected override async Task OnPlay(PlayerChoiceContext ctx, CardPlay play)
    {
        if (!await Alchemy.ExhaustOne(ctx, Lab)) return;

        await Ledger.GainGold(ctx, Lab, DynamicVars["Gold"].IntValue);
        await AlchemistHooks.NotifyTransformed(ctx, Lab, TransformVector.CardToGold);
    }

    protected override void OnUpgrade() => DynamicVars["Gold"].UpgradeValueBy(2m);
}

/// <summary>Transmute's small cousin: less Gold, some Block, and free.</summary>
public sealed class SalvageReagents() : AlchemistCard(0, CardType.Skill, CardRarity.Common, TargetType.Self)
{
    public override bool GainsBlock => true;

    protected override IEnumerable<DynamicVar> CanonicalVars =>
        [new BlockVar(4m, ValueProp.Move), new DynamicVar("Gold", 2m)];

    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [Tip(AlchemistTips.Transform)];

    protected override async Task OnPlay(PlayerChoiceContext ctx, CardPlay play)
    {
        if (!await Alchemy.ExhaustOne(ctx, Lab)) return;

        await AlchemistEffects.GainBlock(Lab, DynamicVars.Block.BaseValue);
        await Ledger.GainGold(ctx, Lab, DynamicVars["Gold"].IntValue);
        await AlchemistHooks.NotifyTransformed(ctx, Lab, TransformVector.CardToGold);
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Block.UpgradeValueBy(2m);
        DynamicVars["Gold"].UpgradeValueBy(1m);
    }
}

/// <summary>Brew something. The plainest statement of what the character does.</summary>
public sealed class PocketFormula() : AlchemistCard(1, CardType.Skill, CardRarity.Common, TargetType.Self)
{
    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];

    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
        [Tip(AlchemistTips.Brew), Tip(AlchemistTips.Volatile), Tip(AlchemistTips.Transform)];

    protected override async Task OnPlay(PlayerChoiceContext ctx, CardPlay play)
    {
        await CreatureCmd.TriggerAnim(Owner.Creature, "Cast", Owner.Character.CastAnimDelay);

        if (await Belt.BrewRandom(ctx, Lab) != null)
            await AlchemistHooks.NotifyTransformed(ctx, Lab, TransformVector.CardToPotion);
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

/// <summary>Block, more if you drank something this turn.</summary>
public sealed class SteadyPour() : AlchemistCard(1, CardType.Skill, CardRarity.Common, TargetType.Self)
{
    public override bool GainsBlock => true;

    protected override IEnumerable<DynamicVar> CanonicalVars =>
        [new BlockVar(8m, ValueProp.Move), new BlockVar("Bonus", 3m, ValueProp.Move)];

    protected override async Task OnPlay(PlayerChoiceContext ctx, CardPlay play)
    {
        var bonus = (AlchemistEffects.Peek(Lab)?.PotionsUsedThisTurn ?? 0) > 0
            ? DynamicVars["Bonus"].BaseValue
            : 0m;

        await AlchemistEffects.GainBlock(Lab, DynamicVars.Block.BaseValue + bonus);
    }

    protected override void OnUpgrade() => DynamicVars.Block.UpgradeValueBy(3m);
}

/// <summary>Pour a Potion out for cards. Transform: Potion into tempo, at zero cost.</summary>
public sealed class Dilute() : AlchemistCard(0, CardType.Skill, CardRarity.Common, TargetType.Self)
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [new CardsVar(2)];

    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];

    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
        [Tip(AlchemistTips.Distill), Tip(AlchemistTips.Transform)];

    protected override async Task OnPlay(PlayerChoiceContext ctx, CardPlay play)
    {
        if (!(await Belt.Distill(ctx, Lab)).Distilled) return;

        await AlchemistEffects.Draw(ctx, Lab, DynamicVars.Cards.IntValue);
        await AlchemistHooks.NotifyTransformed(ctx, Lab, TransformVector.PotionToTempo);
    }

    protected override void OnUpgrade() => DynamicVars.Cards.UpgradeValueBy(1m);
}

/// <summary>Block, and more Block if you feed it a card.</summary>
public sealed class RecycleGlass() : AlchemistCard(1, CardType.Skill, CardRarity.Common, TargetType.Self)
{
    public override bool GainsBlock => true;

    protected override IEnumerable<DynamicVar> CanonicalVars =>
        [new BlockVar(5m, ValueProp.Move), new BlockVar("Bonus", 5m, ValueProp.Move)];

    protected override async Task OnPlay(PlayerChoiceContext ctx, CardPlay play)
    {
        await AlchemistEffects.GainBlock(Lab, DynamicVars.Block.BaseValue);

        if (await Alchemy.ExhaustOne(ctx, Lab))
            await AlchemistEffects.GainBlock(Lab, DynamicVars["Bonus"].BaseValue);
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Block.UpgradeValueBy(2m);
        DynamicVars["Bonus"].UpgradeValueBy(1m);
    }
}

/// <summary>Four Gold for an Energy. The exchange rate the whole Investor deck is measured against.</summary>
public sealed class CoinPurse() : AlchemistCard(0, CardType.Skill, CardRarity.Common, TargetType.Self)
{
    protected override IEnumerable<DynamicVar> CanonicalVars =>
        [new EnergyVar(1), new DynamicVar("Invest", 4m)];

    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [Tip(AlchemistTips.Invest), EnergyHoverTip];

    protected override async Task OnPlay(PlayerChoiceContext ctx, CardPlay play)
    {
        if (await Ledger.Invest(ctx, Lab, DynamicVars["Invest"].IntValue))
            await AlchemistEffects.GainEnergy(Lab, DynamicVars.Energy.BaseValue);
    }

    protected override void OnUpgrade() => DynamicVars["Invest"].UpgradeValueBy(-1m);
}

/// <summary>Weak, plus Block if you drank something. The control common.</summary>
public sealed class BitterSolvent() : AlchemistCard(1, CardType.Skill, CardRarity.Common, TargetType.AnyEnemy)
{
    public override bool GainsBlock => true;

    protected override IEnumerable<DynamicVar> CanonicalVars =>
        [new PowerVar<WeakPower>(2m), new BlockVar(4m, ValueProp.Move)];

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.FromPower<WeakPower>()];

    protected override async Task OnPlay(PlayerChoiceContext ctx, CardPlay play)
    {
        ArgumentNullException.ThrowIfNull(play.Target);

        await AlchemistEffects.ApplyWeak(ctx, Lab, play.Target, DynamicVars["WeakPower"].BaseValue);

        if ((AlchemistEffects.Peek(Lab)?.PotionsUsedThisTurn ?? 0) > 0)
            await AlchemistEffects.GainBlock(Lab, DynamicVars.Block.BaseValue);
    }

    protected override void OnUpgrade() => DynamicVars["WeakPower"].UpgradeValueBy(1m);
}

/// <summary>Dig, and get paid a little for having already spent.</summary>
public sealed class MarketSense() : AlchemistCard(1, CardType.Skill, CardRarity.Common, TargetType.Self)
{
    public override bool GainsBlock => true;

    protected override IEnumerable<DynamicVar> CanonicalVars =>
        [new CardsVar(2), new BlockVar(3m, ValueProp.Move)];

    protected override async Task OnPlay(PlayerChoiceContext ctx, CardPlay play)
    {
        await AlchemistEffects.Draw(ctx, Lab, DynamicVars.Cards.IntValue);
        await Alchemy.DiscardOne(ctx, Lab);

        if (Ledger.SpentThisTurn(Lab))
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

/// <summary>The first Potion you drink each turn also throws a little heat.</summary>
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
