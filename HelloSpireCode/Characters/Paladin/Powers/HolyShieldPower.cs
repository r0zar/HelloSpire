using System.Threading.Tasks;
using HelloSpire.HelloSpireCode.Powers;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.ValueProps;

namespace HelloSpire.HelloSpireCode.Characters.PaladinContent;

/// <summary>
/// At the start of the bearer's turn, gain Block equal to their Spirit. The Holy wall:
/// the lane's heal-scaling stat doubles as its defense, and every Spirit source grows it.
/// </summary>
public sealed class HolyShieldPower : HelloSpirePower
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Single;

    public override async Task AfterPlayerTurnStart(PlayerChoiceContext choiceContext, Player player)
    {
        if (player.Creature != Owner) return;
        var spirit = Spirit.Of(player);
        if (spirit <= 0) return;
        Flash();
        await CreatureCmd.GainBlock(Owner, spirit, ValueProp.Unpowered, null);
    }
}
