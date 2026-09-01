using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;

namespace HelloSpire.HelloSpireCode.Characters.PaladinContent;

/// <summary>
/// The Ret on-ramp. While held: Attacks deal +Amount. Judge: deal 10 -- swing harder now,
/// or cash the verdict.
/// </summary>
public sealed class SealOfRighteousnessPower : SealPower
{
    public const decimal JudgeDamage = 10m;

    public override decimal ModifyDamageAdditive(Creature? target, decimal amount, ValueProp props,
        Creature? dealer, CardModel? cardSource)
    {
        if (PassivesDisabled || Owner != dealer || !props.IsPoweredAttack()) return 0m;
        return Amount;
    }

    public override async Task OnJudged(PlayerChoiceContext ctx, Creature target) =>
        await CreatureCmd.Damage(ctx, [target], JudgeDamage, ValueProp.Unpowered, Owner);
}
