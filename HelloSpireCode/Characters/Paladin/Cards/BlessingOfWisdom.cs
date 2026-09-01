using System;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;

namespace HelloSpire.HelloSpireCode.Characters.PaladinContent.Cards;

/// <summary>
/// A player draws 2 cards, then discards 1. Card flow as a gift -- and a discard trigger
/// wherever it lands. Solo, the gift is yours.
/// </summary>
public sealed class BlessingOfWisdom() : PaladinCard(1, CardType.Skill, CardRarity.Uncommon, TargetType.AnyPlayer)
{
    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        ArgumentNullException.ThrowIfNull(cardPlay.Target);
        await CreatureCmd.TriggerAnim(Owner.Creature, "Cast", Owner.Character.CastAnimDelay);
        var receiver = cardPlay.Target.Player ?? Owner;
        await CardPileCmd.Draw(choiceContext, 2, receiver);
        await PaladinEffects.DiscardChosen(choiceContext, receiver, 1, this);
    }

    protected override void OnUpgrade() => EnergyCost.UpgradeBy(-1);
}
