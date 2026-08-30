using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.ValueProps;

namespace HelloSpire.HelloSpireCode.Characters.PaladinContent.Powers;

/// <summary>
/// At the start of your turn, all allies gain Block equal to this power's Amount.
///
/// "All allies" is every living player creature, the owner included -- so solo it is a modest
/// self-Power and in a party of three it is three times the Block across the group, with no
/// multiplayer-specific text. Mirrors DemonFormPower's start-of-turn shape and BeaconOfHope's
/// teammate enumeration.
/// </summary>
public sealed class AuraOfProtectionPower : PaladinPower
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;

    public override async Task AfterSideTurnStart(CombatSide side, IReadOnlyList<Creature> participants, ICombatState combatState)
    {
        if (!participants.Contains(Owner)) return;
        Flash();
        foreach (var ally in combatState.PlayerCreatures.Where(c => c.IsAlive))
            await CreatureCmd.GainBlock(ally, Amount, ValueProp.Unpowered, null);
    }
}
