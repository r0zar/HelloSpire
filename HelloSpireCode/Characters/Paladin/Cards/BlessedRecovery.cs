using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;

namespace HelloSpire.HelloSpireCode.Characters.PaladinContent.Cards;

/// <summary>
/// Gain 3 Spirit, then heal a player 5 + Spirit. Exhaust. Tithe: draw a card.
/// The Spirit burst -- it gains before it heals, so it pays for itself.
/// </summary>
public sealed class BlessedRecovery() : PaladinCard(2, CardType.Skill, CardRarity.Uncommon, TargetType.AnyPlayer), IHealingCard
{
    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];
    protected override IEnumerable<DynamicVar> CanonicalVars =>
        [new SpiritHealVar(5m, "Spirit"), new DynamicVar("Spirit", 3m)];
    protected override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.Static(PaladinTips.Tithe)];

    public override bool HasTithe => true;

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        ArgumentNullException.ThrowIfNull(cardPlay.Target);
        await CreatureCmd.TriggerAnim(Owner.Creature, "Cast", Owner.Character.CastAnimDelay);
        await Spirit.Gain(choiceContext, Owner, (int)DynamicVars["Spirit"].BaseValue, this);
        await Spirit.Heal(Owner, cardPlay.Target, DynamicVars.Heal.BaseValue);
    }

    protected override async Task OnTithe(PlayerChoiceContext ctx) =>
        await CardPileCmd.Draw(ctx, 1, Owner);

    protected override void OnUpgrade() => DynamicVars["Spirit"].UpgradeValueBy(1m);
}
