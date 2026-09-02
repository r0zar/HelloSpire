using System.Threading.Tasks;
using HelloSpire.HelloSpireCode.Powers;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;

namespace HelloSpire.HelloSpireCode.Characters.PaladinContent;

/// <summary>
/// At the start of your turn, gain Amount Spirit. The Holy ramp engine -- a deliberate
/// amendment to the Spirit-gains-Exhaust law: the trickle is bounded because Spirit only
/// cashes out through heals, which all Exhaust.
/// </summary>
public sealed class DevotionPower : HelloSpirePower
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;

    public override async Task AfterPlayerTurnStart(PlayerChoiceContext choiceContext, Player player)
    {
        if (player != Owner.Player) return;
        Flash();
        await Spirit.Gain(choiceContext, player, (int)Amount);
    }
}
