using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;

namespace HelloSpire.HelloSpireCode.Characters.PaladinContent;

/// <summary>The card-flow seal, banked: a pure judge charge. Judge: draw 2, then discard 1.</summary>
public sealed class SealOfWisdomPower : SealPower
{
    public const int JudgeDraw = 2;
    public const int JudgeDiscard = 1;

    public override async Task OnJudged(PlayerChoiceContext ctx, Creature target)
    {
        if (Owner.Player is not { } player) return;
        await CardPileCmd.Draw(ctx, JudgeDraw, player);
        await PaladinEffects.DiscardChosen(ctx, player, JudgeDiscard, this);
    }
}
