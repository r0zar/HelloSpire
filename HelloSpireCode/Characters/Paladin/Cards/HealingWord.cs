using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;

namespace HelloSpire.HelloSpireCode.Characters.PaladinContent.Cards;

/// <summary>
/// Heal a player 4 + Spirit. Discard a card. Exhaust. Tithe: gain 1 Spirit.
/// The classic quick heal (replaces Penance): free, costs a card from hand, and its face is
/// the set's one Spirit-granting Tithe -- a deliberate amendment to the no-Spirit-on-faces
/// law, at drip rate.
/// </summary>
public sealed class HealingWord() : PaladinCard(0, CardType.Skill, CardRarity.Common, TargetType.AnyPlayer), IHealingCard
{
    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];
    protected override IEnumerable<DynamicVar> CanonicalVars => [new SpiritHealVar(4m)];
    protected override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.Static(PaladinTips.Tithe)];

    public override bool HasTithe => true;

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        var target = TargetOrOwner(cardPlay);
        await CreatureCmd.TriggerAnim(Owner.Creature, "Cast", Owner.Character.CastAnimDelay);
        await Spirit.Heal(Owner, target, DynamicVars.Heal.BaseValue);
        await PaladinEffects.DiscardChosen(choiceContext, Owner, 1, this);
    }

    protected override async Task OnTithe(PlayerChoiceContext ctx) =>
        await Spirit.Gain(ctx, Owner, 1, this);

    protected override void OnUpgrade() => DynamicVars.Heal.UpgradeValueBy(3m);
}
