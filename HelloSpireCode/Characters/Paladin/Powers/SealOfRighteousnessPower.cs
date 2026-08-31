using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;

namespace HelloSpire.HelloSpireCode.Characters.PaladinContent;

/// <summary>
/// While active: each hit of your Attacks procs Amount extra damage. Judged: deal 5 -- small, because Judgment
/// now fires every Seal you have and each one is priced for the stack. The starter Seal, granted by the Holy Book --
/// the Defect's Cracked Core shape: passive trickle, real evoke.
/// </summary>
public sealed class SealOfRighteousnessPower : SealPower
{
    public const decimal JudgeDamage = 5m;

    /// <summary>
    /// A real on-hit proc, not a damage add: each hit of an Attack lands, then the seal strikes
    /// separately for Amount. Multi-hit Attacks proc per hit. The proc itself is Unpowered, so it
    /// is not an Attack and cannot trigger itself.
    /// </summary>
    public override async Task AfterDamageReceived(PlayerChoiceContext choiceContext, Creature target,
        DamageResult result, ValueProp props, Creature? dealer, CardModel? cardSource)
    {
        if (PassivesDisabled || dealer != Owner || target == Owner || target.IsDead ||
            !props.IsPoweredAttack()) return;
        Flash();
        await CreatureCmd.Damage(choiceContext, [target], Amount, ValueProp.Unpowered, Owner);
    }

    public override async Task OnJudged(PlayerChoiceContext ctx, Creature target) =>
        await CreatureCmd.Damage(ctx, [target], JudgeDamage, ValueProp.Unpowered, Owner);
}
