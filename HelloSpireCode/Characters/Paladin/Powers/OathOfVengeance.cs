using System.Threading.Tasks;
using HelloSpire.HelloSpireCode.Characters.PaladinContent.Faith;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;

namespace HelloSpire.HelloSpireCode.Characters.PaladinContent.Powers;

/// <summary>Whenever you play an Attack, gain 1 Faith in Tyr.
/// Countable trigger: keyed off the card play, not off each damage instance.</summary>
public sealed class OathOfVengeance : PaladinPower
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Single;

    public override async Task AfterCardPlayed(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (cardPlay.Card.Owner != Owner.Player) return;
        if (cardPlay.Card.Type != CardType.Attack) return;
        FaithTracks.Gain(Owner.Player.PlayerCombatState, Deity.Tyr, 1);
        await Task.CompletedTask;
    }
}
