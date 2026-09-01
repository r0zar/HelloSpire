using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models.Powers;

namespace HelloSpire.HelloSpireCode.Characters.PaladinContent;

/// <summary>The debuff seal, banked: a pure judge charge. Judge: apply 2 Weak and 2 Vulnerable.</summary>
public sealed class SealOfCommandPower : SealPower
{
    public const decimal JudgeWeak = 2m;
    public const decimal JudgeVulnerable = 2m;

    public override async Task OnJudged(PlayerChoiceContext ctx, Creature target)
    {
        await PowerCmd.Apply<WeakPower>(ctx, target, JudgeWeak, Owner, null);
        await PowerCmd.Apply<VulnerablePower>(ctx, target, JudgeVulnerable, Owner, null);
    }
}
