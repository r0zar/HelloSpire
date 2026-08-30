using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models.Powers;

namespace HelloSpire.HelloSpireCode.Characters.PaladinContent;

/// <summary>
/// The starter Seal: your first Attack each turn heals Amount (Burning Blood weight -- the
/// per-attack version out-healed it badly and rewarded stalling). Judged: apply 1 Weak --
/// blinding radiance; the healing lives in the passive, judgments always aim at the target.
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
        if (_usedThisTurn || cardPlay.Card.Owner?.Creature != Owner ||
            cardPlay.Card.Type != CardType.Attack) return;
        _usedThisTurn = true;
        Flash();
        await CreatureCmd.Heal(Owner, Amount);
    }

    public override async Task OnJudged(PlayerChoiceContext ctx, Creature target) =>
        await PowerCmd.Apply<WeakPower>(ctx, target, 1m, Owner, null);
}
