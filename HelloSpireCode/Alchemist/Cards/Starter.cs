using HelloSpire.HelloSpireCode.Alchemist.Lab;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;

namespace HelloSpire.HelloSpireCode.Alchemist.Cards;

// The starting ten: four Strikes, four Defends, Aegis Formula, and Infusion.
//
// Between them they teach one system and only one. The belt has space, cards put Potions in it,
// Potions cost no Energy to drink, and a Brewed Potion is gone at the end of the fight whether you
// used it or not. Poison is deliberately absent until the first card reward.

/// <summary>Deal damage.</summary>
public sealed class StrikeAlchemist() : AlchemistCard(1, CardType.Attack, CardRarity.Basic, TargetType.AnyEnemy)
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [new DamageVar(6m, ValueProp.Move)];

    protected override HashSet<CardTag> CanonicalTags => [CardTag.Strike];

    protected override async Task OnPlay(PlayerChoiceContext ctx, CardPlay play)
    {
        ArgumentNullException.ThrowIfNull(play.Target);
        await DamageCmd.Attack(DynamicVars.Damage.BaseValue)
            .FromCard(this).Targeting(play.Target)
            .WithHitFx("vfx/vfx_attack_blunt", null, "blunt_attack.mp3").Execute(ctx);
    }

    protected override void OnUpgrade() => DynamicVars.Damage.UpgradeValueBy(3m);
}

/// <summary>Gain Block.</summary>
public sealed class DefendAlchemist() : AlchemistCard(1, CardType.Skill, CardRarity.Basic, TargetType.Self)
{
    public override bool GainsBlock => true;

    protected override IEnumerable<DynamicVar> CanonicalVars => [new BlockVar(5m, ValueProp.Move)];

    protected override HashSet<CardTag> CanonicalTags => [CardTag.Defend];

    protected override async Task OnPlay(PlayerChoiceContext ctx, CardPlay play) =>
        await CreatureCmd.GainBlock(Owner.Creature, DynamicVars.Block, play);

    protected override void OnUpgrade() => DynamicVars.Block.UpgradeValueBy(3m);
}

/// <summary>
/// Brew a Volatile Block Potion.
///
/// The defensive half, and the reason the character survives Act 1 at 68 HP: Block held in
/// reserve, spendable on the turn it is needed rather than the turn it was drawn.
/// </summary>
public sealed class AegisFormula() : AlchemistCard(1, CardType.Skill, CardRarity.Basic, TargetType.Self)
{
    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];

    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
        [Tip(AlchemistTips.Brew), Tip(AlchemistTips.Volatile)];

    protected override async Task OnPlay(PlayerChoiceContext ctx, CardPlay play)
    {
        await CreatureCmd.TriggerAnim(Owner.Creature, "Cast", Owner.Character.CastAnimDelay);
        await Belt.Brew(ctx, Lab, LabBridge.Current.NamedPotion(BasePotion.Block));
    }

    protected override void OnUpgrade() => EnergyCost.UpgradeBy(-1);
}

/// <summary>Infuse Unstable Concoction. The class's defining conversion, at its cheapest and plainest.</summary>
public sealed class Infusion() : AlchemistCard(1, CardType.Skill, CardRarity.Basic, TargetType.Self)
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [new DynamicVar("Bonus", 6m)];

    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [Tip(AlchemistTips.Infuse)];

    protected override async Task OnPlay(PlayerChoiceContext ctx, CardPlay play)
    {
        await Belt.Infuse(ctx, Lab, damage: DynamicVars["Bonus"].BaseValue);
    }

    protected override void OnUpgrade() => DynamicVars["Bonus"].UpgradeValueBy(2m);
}
