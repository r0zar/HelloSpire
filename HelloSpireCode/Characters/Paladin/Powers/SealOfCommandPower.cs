using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;

namespace HelloSpire.HelloSpireCode.Characters.PaladinContent;

/// <summary>
/// The debuff seal. While held: debuffs you apply to enemies get +Amount stacks.
/// Judge: apply 2 Weak and 2 Vulnerable -- Censure's judge stacks the bomb.
/// </summary>
public sealed class SealOfCommandPower : SealPower
{
    public const decimal JudgeWeak = 2m;
    public const decimal JudgeVulnerable = 2m;

    public override decimal ModifyPowerAmountGivenAdditive(PowerModel power, Creature giver,
        decimal amount, Creature? target, CardModel? cardSource)
    {
        if (PassivesDisabled || giver != Owner || target == null || target.Side == Owner.Side) return 0m;
        if (power.Type != PowerType.Debuff) return 0m;
        return Amount;
    }

    public override async Task OnJudged(PlayerChoiceContext ctx, Creature target)
    {
        await PowerCmd.Apply<WeakPower>(ctx, target, JudgeWeak, Owner, null);
        await PowerCmd.Apply<VulnerablePower>(ctx, target, JudgeVulnerable, Owner, null);
    }
}
