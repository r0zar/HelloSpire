using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using HelloSpire.HelloSpireCode.Powers;

namespace HelloSpire.HelloSpireCode.Characters.PaladinContent;

/// <summary>At the start of your side's turn, gain Amount Spirit. The Spirit engine.</summary>
public sealed class DevotionPower : HelloSpirePower
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;

    public override async Task BeforeSideTurnStart(PlayerChoiceContext choiceContext, CombatSide side,
        IReadOnlyList<Creature> participants, ICombatState combatState)
    {
        if (side != Owner.Side || Owner.Player is not { } player) return;
        Flash();
        await Spirit.Gain(choiceContext, player, (int)Amount);
    }
}
