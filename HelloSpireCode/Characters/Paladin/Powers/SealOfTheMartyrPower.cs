using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;

namespace HelloSpire.HelloSpireCode.Characters.PaladinContent;

/// <summary>
/// The Prot emergency, banked: a pure judge charge (the Thorns were paid on cast).
/// Judge: ALL enemies lose 5 Strength until end of turn.
/// </summary>
public sealed class SealOfTheMartyrPower : SealPower
{
    public const decimal JudgeStrengthDown = 5m;

    public override async Task OnJudged(PlayerChoiceContext ctx, Creature target)
    {
        if (Owner.CombatState is not { } state) return;
        foreach (var enemy in state.HittableEnemies)
            await PowerCmd.Apply<HumblingShacklesPower>(ctx, enemy, JudgeStrengthDown, Owner, null);
    }
}
