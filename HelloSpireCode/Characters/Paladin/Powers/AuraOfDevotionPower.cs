using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HelloSpire.HelloSpireCode.Powers;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models.Powers;

namespace HelloSpire.HelloSpireCode.Characters.PaladinContent;

/// <summary>
/// ALL players gain Amount Plating at the start of the bearer's turn. The party armor engine.
///
/// Grants in AfterSideTurnStartLate, NOT AfterPlayerTurnStart: the engine decrements every
/// player's Plating in the base AfterSideTurnStart pass, which runs AFTER the per-player
/// turn-start hooks -- granting there meant the fresh Plating was immediately decayed in the
/// same boundary (at Amount 1 the aura was a visible no-op). The Late pass lands after the
/// decrement, so the aura's Plating survives its own round.
/// </summary>
public sealed class AuraOfDevotionPower : HelloSpirePower
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;

    public override async Task AfterSideTurnStartLate(CombatSide side, IReadOnlyList<Creature> participants,
        ICombatState state)
    {
        if (side != CombatSide.Player || !participants.Contains(Owner) || Owner.IsDead) return;
        Flash();
        foreach (var ally in state.PlayerCreatures.Where(c => c.IsAlive))
            await PowerCmd.Apply<PlatingPower>(new ThrowingPlayerChoiceContext(), ally, Amount, Owner, null);
    }
}
