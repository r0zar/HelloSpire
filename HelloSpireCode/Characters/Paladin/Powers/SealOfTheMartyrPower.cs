using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;

namespace HelloSpire.HelloSpireCode.Characters.PaladinContent;

/// <summary>
/// The tank Seal: enemies that hit you take Amount damage; Judged, deal Amount to ALL enemies.
/// </summary>
public sealed class SealOfTheMartyrPower : SealPower
{
    public override async Task BeforeDamageReceived(PlayerChoiceContext choiceContext, Creature target,
        decimal amount, ValueProp props, Creature? dealer, CardModel? cardSource)
    {
        if (PassivesDisabled || target != Owner || dealer == null || dealer.IsDead || !props.IsPoweredAttack()) return;
        Flash();
        await CreatureCmd.Damage(choiceContext, dealer, Amount,
            ValueProp.Unpowered | ValueProp.SkipHurtAnim, Owner, null);
    }

    public override async Task OnJudged(PlayerChoiceContext ctx, Creature target)
    {
        if (Owner.CombatState is not { } state || state.HittableEnemies.Count == 0) return;
        await CreatureCmd.Damage(ctx, state.HittableEnemies, Amount, ValueProp.Unpowered, Owner);
    }
}
