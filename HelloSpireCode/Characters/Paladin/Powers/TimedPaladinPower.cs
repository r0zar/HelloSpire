using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;

namespace HelloSpire.HelloSpireCode.Characters.PaladinContent.Powers;

/// <summary>
/// A buff that lasts N turns. Amount is turns remaining: it ticks down at the start of each of
/// the owner's turns and the power removes itself at zero. Applied with 3 during turn N, it is
/// active on turns N, N+1 and N+2. Seals and the timed rares are built on this.
/// </summary>
public abstract class TimedPaladinPower : PaladinPower
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;

    public override async Task AfterPlayerTurnStart(PlayerChoiceContext choiceContext, Player player)
    {
        if (player.Creature != Owner) return;
        await PowerCmd.ModifyAmount(choiceContext, this, -1m, Owner, null, true);
        if (Amount <= 0) await PowerCmd.Remove(this);
    }
}
