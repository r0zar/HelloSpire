using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;

namespace HelloSpire.HelloSpireCode.Characters.PaladinContent;

/// <summary>
/// The starter Seal: your first Attack each turn heals Amount (Burning Blood weight -- the
/// per-attack version out-healed it badly and rewarded stalling). Judged: gain 1 Spirit --
/// devotion deepens. The exception to judgments-are-offensive, by design.
/// </summary>
public sealed class SealOfLightPower : SealPower
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
            cardPlay.Card.Type != CardType.Attack) return;
        _usedThisTurn = true;
        Flash();
        await PowerCmd.Apply<RegenPower>(choiceContext, Owner, Amount, Owner, null);
    }

    public override async Task OnJudged(PlayerChoiceContext ctx, Creature target)
    {
        if (Owner.Player is not { } player) return;
        await Spirit.Gain(ctx, player, 1);
    }
}
