using System.Threading.Tasks;
using HelloSpire.HelloSpireCode.Powers;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.ValueProps;

namespace HelloSpire.HelloSpireCode.Characters.PaladinContent;

/// <summary>Whenever you Judge an enemy, deal Amount to ALL enemies. Fires per judge instance -- multi-judges cascade.</summary>
public sealed class SanctifiedWrathPower : HelloSpirePower, IJudgeTrigger
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;

    public async Task OnJudgeInstance(PlayerChoiceContext ctx, Creature target)
    {
        if (Owner.CombatState is not { } state || state.HittableEnemies.Count == 0) return;
        await CreatureCmd.Damage(ctx, state.HittableEnemies, Amount, ValueProp.Unpowered, Owner);
    }
}
