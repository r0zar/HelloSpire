using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.ValueProps;

namespace HelloSpire.HelloSpireCode.Characters.PaladinContent;

/// <summary>
/// The tank Seal, judge face only: deal Amount to ALL enemies. Its passive is real ThornsPower,
/// applied by the card -- no reinvented thorns (same call as Consecrated Ground).
/// </summary>
public sealed class SealOfTheMartyrPower : SealPower
{
    public override async Task OnJudged(PlayerChoiceContext ctx, Creature target)
    {
        if (Owner.CombatState is not { } state || state.HittableEnemies.Count == 0) return;
        await CreatureCmd.Damage(ctx, state.HittableEnemies, Amount, ValueProp.Unpowered, Owner);
    }
}
