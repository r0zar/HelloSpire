using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.ValueProps;

namespace HelloSpire.HelloSpireCode.Characters.PaladinContent;

/// <summary>
/// The cycle Seal: first Attack each turn draws a card. Judged: deal damage equal to your hand
/// size, then draw Amount -- knowledge is power, and the passive feeds the judge.
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
        if (PassivesDisabled || _usedThisTurn || cardPlay.Card.Owner?.Creature != Owner ||
            cardPlay.Card.Type != CardType.Attack || Owner.Player is not { } player) return;
        _usedThisTurn = true;
        Flash();
        await CardPileCmd.Draw(choiceContext, 1, player, false);
    }

    public override async Task OnJudged(PlayerChoiceContext ctx, Creature target)
    {
        if (Owner.Player is not { } player) return;
        var handSize = PileType.Hand.GetPile(player).Cards.Count;
        if (handSize > 0)
            await CreatureCmd.Damage(ctx, [target], handSize, ValueProp.Unpowered, Owner);
        await CardPileCmd.Draw(ctx, (int)Amount, player, false);
    }
}
