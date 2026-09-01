using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;

namespace HelloSpire.HelloSpireCode.Characters.PaladinContent;

/// <summary>
/// The card-flow seal. While held: the first Attack you play each turn draws Amount.
/// Judge: draw 2, then discard 1 -- digs and feeds discard payoffs at once.
/// </summary>
public sealed class SealOfWisdomPower : SealPower
{
    public const int JudgeDraw = 2;
    public const int JudgeDiscard = 1;

    private bool _usedThisTurn;

    public override async Task AfterPlayerTurnStart(PlayerChoiceContext choiceContext, Player player)
    {
        if (player.Creature == Owner) _usedThisTurn = false;
        await Task.CompletedTask;
    }

    public override async Task AfterCardPlayed(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (PassivesDisabled || _usedThisTurn || cardPlay.Card.Owner?.Creature != Owner ||
            cardPlay.Card.Type != CardType.Attack) return;
        _usedThisTurn = true;
        Flash();
        await CardPileCmd.Draw(choiceContext, Amount, cardPlay.Card.Owner!);
    }

    public override async Task OnJudged(PlayerChoiceContext ctx, Creature target)
    {
        if (Owner.Player is not { } player) return;
        await CardPileCmd.Draw(ctx, JudgeDraw, player);
        await PaladinEffects.DiscardChosen(ctx, player, JudgeDiscard, this);
    }
}
