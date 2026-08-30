using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.ValueProps;

namespace HelloSpire.HelloSpireCode.Characters.PaladinContent;

/// <summary>
/// The cycle Seal: first Attack each turn draws a card; Judged, deal 2 and draw Amount --
/// every judgment lands on the target.
/// </summary>
public sealed class SealOfWisdomPower : SealPower
{
    private bool _usedThisTurn;

    public override async Task AfterPlayerTurnStart(PlayerChoiceContext choiceContext, Player player)
    {
        if (player.Creature == Owner) _usedThisTurn = false;
        await Task.CompletedTask;
    }

    public override async Task AfterCardPlayed(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (_usedThisTurn || cardPlay.Card.Owner?.Creature != Owner ||
            cardPlay.Card.Type != CardType.Attack || Owner.Player is not { } player) return;
        _usedThisTurn = true;
        Flash();
        await CardPileCmd.Draw(choiceContext, 1, player, false);
    }

    public override async Task OnJudged(PlayerChoiceContext ctx, Creature target)
    {
        if (Owner.Player is not { } player) return;
        await CreatureCmd.Damage(ctx, [target], 2m, ValueProp.Unpowered, Owner);
        await CardPileCmd.Draw(ctx, (int)Amount, player, false);
    }
}
