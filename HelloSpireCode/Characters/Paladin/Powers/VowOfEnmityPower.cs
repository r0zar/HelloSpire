using System.Threading.Tasks;
using HelloSpire.HelloSpireCode.Powers;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models.Powers;

namespace HelloSpire.HelloSpireCode.Characters.PaladinContent;

/// <summary>
/// Whenever you Judge an enemy, draw a card and gain Amount Plating. All three lanes in one
/// vow: the Ret verb pays out card flow and armor, per judge instance.
/// </summary>
public sealed class VowOfEnmityPower : HelloSpirePower, IJudgeTrigger
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;

    public async Task OnJudgeInstance(PlayerChoiceContext ctx, Creature target)
    {
        if (Owner.Player is not { } player) return;
        await CardPileCmd.Draw(ctx, 1, player);
        await PowerCmd.Apply<PlatingPower>(ctx, Owner, Amount, Owner, null);
    }
}
