using System.Linq;
using System.Threading.Tasks;
using HelloSpire.HelloSpireCode.Powers;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;

namespace HelloSpire.HelloSpireCode.Characters.PaladinContent;

/// <summary>
/// At the start of your turn, deal damage equal to your Plating to ALL enemies. The wall
/// fights back -- worthless without the Plating engine, scary with it. (Reworked from
/// Plating-per-hit, which was better in non-Prot decks than Prot ones: free defense with
/// zero build investment, and Guardian could not even amp its ticks.)
/// </summary>
public sealed class ArdentDefenderPower : HelloSpirePower
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Single;

    public override async Task AfterPlayerTurnStart(PlayerChoiceContext choiceContext, Player player)
    {
        if (player != Owner.Player) return;
        var plating = Owner.GetPowerAmount<PlatingPower>();
        if (plating <= 0) return;
        if (Owner.CombatState is not { } state) return;
        var enemies = state.HittableEnemies.ToList();
        if (enemies.Count == 0) return;
        Flash();
        await CreatureCmd.Damage(choiceContext, enemies, plating, ValueProp.Unpowered, Owner);
    }
}
