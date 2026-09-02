using System.Threading.Tasks;
using HelloSpire.HelloSpireCode.Powers;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;

namespace HelloSpire.HelloSpireCode.Characters.PaladinContent;

/// <summary>
/// The echo of Renew: at the start of the bearer's next turn, heal them Amount and fade.
/// Amount is snapshotted at cast (base + the caster's Spirit then) -- one honest number on
/// the icon, no re-reading Spirit at tick time.
/// </summary>
public sealed class RenewPower : HelloSpirePower
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;

    public override async Task AfterPlayerTurnStart(PlayerChoiceContext choiceContext, Player player)
    {
        if (player != Owner.Player) return;
        Flash();
        await CreatureCmd.Heal(Owner, Amount);
        await PowerCmd.Remove(this);
    }
}
