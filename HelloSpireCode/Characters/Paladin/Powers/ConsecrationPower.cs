using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.ValueProps;
using HelloSpire.HelloSpireCode.Powers;

namespace HelloSpire.HelloSpireCode.Characters.PaladinContent;

/// <summary>At the start of your side's turn, deal Amount damage to ALL enemies.</summary>
public sealed class ConsecrationPower : HelloSpirePower
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;

    public override async Task BeforeSideTurnStart(PlayerChoiceContext choiceContext, CombatSide side,
        IReadOnlyList<Creature> participants, ICombatState combatState)
    {
        if (side != Owner.Side || combatState.HittableEnemies.Count == 0) return;
        Flash();
        await CreatureCmd.Damage(choiceContext, combatState.HittableEnemies, Amount,
            ValueProp.Unpowered, Owner);
    }
}
