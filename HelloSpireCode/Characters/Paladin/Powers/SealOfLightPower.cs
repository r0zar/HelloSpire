using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;

namespace HelloSpire.HelloSpireCode.Characters.PaladinContent;

/// <summary>
/// The Holy seal, armed: no passive (the Spirit was paid on cast).
/// Judge: draw 3, discard 2 -- the deep dig that finds the heal and pays Tithe faces on the way.
/// </summary>
public sealed class SealOfLightPower : SealPower
{
    public const int JudgeDraw = 3;
    public const int JudgeDiscard = 2;

    public override async Task OnJudged(PlayerChoiceContext ctx, Creature target)
    {
        if (Owner.Player is not { } player) return;
        await CardPileCmd.Draw(ctx, JudgeDraw, player);
        await PaladinEffects.DiscardChosen(ctx, player, JudgeDiscard, this);
    }
}
