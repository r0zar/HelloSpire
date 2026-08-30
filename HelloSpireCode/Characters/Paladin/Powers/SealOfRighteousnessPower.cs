using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;

namespace HelloSpire.HelloSpireCode.Characters.PaladinContent;

/// <summary>
/// While active: your Attacks deal +Amount damage (applied at 2, the Strength pattern). Judged:
/// deal 10 damage -- clearly above a Seal-boosted Strike (8), since the judge is Judgment's
/// entire payload. The starter Seal, granted by the Holy Book --
/// the Defect's Cracked Core shape: passive trickle, real evoke.
/// </summary>
public sealed class SealOfRighteousnessPower : SealPower
{
    public const decimal JudgeDamage = 10m;

    public override decimal ModifyDamageAdditive(Creature? target, decimal amount, ValueProp props,
        Creature? dealer, CardModel? cardSource)
    {
        if (Owner != dealer || !props.IsPoweredAttack()) return 0m;
        return Amount;
    }

    public override async Task OnJudged(PlayerChoiceContext ctx, Creature target) =>
        await CreatureCmd.Damage(ctx, [target], JudgeDamage, ValueProp.Unpowered, Owner);
}
