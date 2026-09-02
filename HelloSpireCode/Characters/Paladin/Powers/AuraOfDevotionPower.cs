using System.Linq;
using System.Threading.Tasks;
using HelloSpire.HelloSpireCode.Powers;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models.Powers;

namespace HelloSpire.HelloSpireCode.Characters.PaladinContent;

/// <summary>ALL players gain Amount Plating at the start of the bearer's turn. The party armor engine.</summary>
public sealed class AuraOfDevotionPower : HelloSpirePower
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;

    public override async Task AfterPlayerTurnStart(PlayerChoiceContext choiceContext, Player player)
    {
        if (player.Creature != Owner || Owner.CombatState is not { } state) return;
        Flash();
        foreach (var ally in state.PlayerCreatures.Where(c => c.IsAlive))
            await PowerCmd.Apply<PlatingPower>(choiceContext, ally, Amount, Owner, null);
    }
}
