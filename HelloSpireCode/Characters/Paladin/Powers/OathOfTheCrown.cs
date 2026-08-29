using System.Threading.Tasks;
using HelloSpire.HelloSpireCode.Characters.PaladinContent.Faith;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;

namespace HelloSpire.HelloSpireCode.Characters.PaladinContent.Powers;

/// <summary>Whenever you play a card that gains Block, gain 1 Faith in Torm.
/// Uses CardModel.GainsBlock rather than AfterBlockGained so a multi-source Block card counts once.</summary>
public sealed class OathOfTheCrown : PaladinPower
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Single;

    public override async Task AfterCardPlayed(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (cardPlay.Card.Owner != Owner.Player) return;
        if (!cardPlay.Card.GainsBlock) return;
        FaithTracks.Gain(Owner.Player.PlayerCombatState, Deity.Torm, 1);
        await Task.CompletedTask;
    }
}
