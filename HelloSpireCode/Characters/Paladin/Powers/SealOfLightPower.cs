using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;

namespace HelloSpire.HelloSpireCode.Characters.PaladinContent;

/// <summary>
/// While active: whenever you play an Attack, heal Amount. Judged: heal 4 plus Spirit.
/// The healing Seal -- turns Judgment into a Spirit-scaled heal button.
/// </summary>
public sealed class SealOfLightPower : SealPower
{
    public const decimal JudgeHeal = 4m;

    public override async Task AfterCardPlayed(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (cardPlay.Card.Owner?.Creature != Owner || cardPlay.Card.Type != CardType.Attack) return;
        Flash();
        await CreatureCmd.Heal(Owner, Amount);
    }

    public override async Task OnJudged(PlayerChoiceContext ctx, Creature target)
    {
        if (Owner.Player is not { } player) return;
        await Spirit.Heal(player, JudgeHeal);
    }
}
