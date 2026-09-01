using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;

namespace HelloSpire.HelloSpireCode.Characters.PaladinContent.Cards;

/// <summary>
/// Heal the most wounded player 10; they gain 10 Block. Exhaust. Tithe: gain 2 Block.
/// The intervention: swoop to whoever is dying -- solo, that is you.
/// </summary>
public sealed class DivineIntervention() : PaladinCard(1, CardType.Skill, CardRarity.Rare, TargetType.Self), IHealingCard
{
    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];
    public override bool GainsBlock => true;
    protected override IEnumerable<DynamicVar> CanonicalVars => [new SpiritHealVar(10m)];
    protected override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.Static(PaladinTips.Tithe)];

    public override bool HasTithe => true;

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await CreatureCmd.TriggerAnim(Owner.Creature, "Cast", Owner.Character.CastAnimDelay);
        var wounded = PaladinEffects.MostWounded(Owner.Creature);
        await Spirit.Heal(Owner, wounded, DynamicVars.Heal.BaseValue);
        await CreatureCmd.GainBlock(wounded, 10m, ValueProp.Move, cardPlay);
    }

    protected override async Task OnTithe(PlayerChoiceContext ctx) =>
        await CreatureCmd.GainBlock(Owner.Creature, 2m, ValueProp.Unpowered, null);

    protected override void OnUpgrade() => DynamicVars.Heal.UpgradeValueBy(5m);
}
