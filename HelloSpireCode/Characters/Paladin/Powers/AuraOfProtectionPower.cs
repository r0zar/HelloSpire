using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HelloSpire.HelloSpireCode.Powers;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.ValueProps;

namespace HelloSpire.HelloSpireCode.Characters.PaladinContent;

/// <summary>
/// Aura: at the END of your side's turn, ALL players gain Amount Block -- the Metallicize/Plating
/// timing. Start-of-turn was a bug in effect: the Block landed right as the turn's own Block
/// reset wiped it, so the aura appeared to do nothing.
/// </summary>
public sealed class AuraOfProtectionPower : HelloSpirePower
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;

    public override async Task AfterSideTurnEnd(PlayerChoiceContext choiceContext, CombatSide side,
        IEnumerable<Creature> participants)
    {
        if (side != Owner.Side || Owner.IsDead) return;
        Flash();
        if (Owner.CombatState is not { } state) return;
        foreach (var player in state.PlayerCreatures.Where(c => c.IsAlive))
            await CreatureCmd.GainBlock(player, Amount, ValueProp.Unpowered, null);
    }
}
