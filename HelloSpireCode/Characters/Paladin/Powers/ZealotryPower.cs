using System.Threading.Tasks;
using HelloSpire.HelloSpireCode.Powers;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models.Powers;

namespace HelloSpire.HelloSpireCode.Characters.PaladinContent;

/// <summary>Whenever you Judge an enemy, gain Amount Strength. Fires per judge instance.</summary>
public sealed class ZealotryPower : HelloSpirePower, IJudgeTrigger
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;

    public async Task OnJudgeInstance(PlayerChoiceContext ctx, Creature target) =>
        await PowerCmd.Apply<StrengthPower>(ctx, Owner, Amount, Owner, null);
}
