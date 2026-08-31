using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models.Powers;

namespace HelloSpire.HelloSpireCode.Characters.PaladinContent;

/// <summary>
/// The control Seal: first Attack each turn applies 1 Weak; Judged, the target loses 4 Strength this turn.
/// </summary>
public sealed class SealOfJusticePower : SealPower
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

        // Single-target attacks carry their target on the CardPlay; AoE attacks (AllEnemies)
        // carry null and hit everything, so the debuff lands on everything they hit.
        List<Creature> targets = [];
        if (cardPlay.Target is { IsDead: false } single) targets.Add(single);
        else if (cardPlay.Card.TargetType == TargetType.AllEnemies && Owner.CombatState is { } state)
            targets.AddRange(state.HittableEnemies);
        if (targets.Count == 0) return;

        _usedThisTurn = true;
        Flash();
        foreach (var target in targets)
            await PowerCmd.Apply<WeakPower>(choiceContext, target, 1m, Owner, null);
    }

    public override async Task OnJudged(PlayerChoiceContext ctx, Creature target) =>
        await PowerCmd.Apply<SealOfJusticeShacklesPower>(ctx, target, 4m, Owner, null);
}
