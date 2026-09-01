using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Context;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;

namespace HelloSpire.HelloSpireCode.Characters.PaladinContent;

/// <summary>
/// The Prot emergency seal. While held: +Amount Thorns (granted on arm, taken back when the
/// seal leaves -- consumed or replaced). Judge: ALL enemies lose 5 Strength until end of turn,
/// the get-out-of-jail button that saves the team.
/// </summary>
public sealed class SealOfTheMartyrPower : SealPower
{
    public const decimal JudgeStrengthDown = 5m;

    public override async Task BeforeApplied(Creature target, decimal amount, Creature? applier, CardModel? cardSource) =>
        await PowerCmd.Apply<ThornsPower>(new ThrowingPlayerChoiceContext(), target, amount, applier, cardSource, silent: true);

    public override async Task AfterRemoved(Creature oldOwner) =>
        await PowerCmd.Apply<ThornsPower>(new ThrowingPlayerChoiceContext(), oldOwner, -Amount, oldOwner, null, silent: true);

    public override async Task OnJudged(PlayerChoiceContext ctx, Creature target)
    {
        if (Owner.CombatState is not { } state) return;
        foreach (var enemy in state.HittableEnemies)
            await PowerCmd.Apply<HumblingShacklesPower>(ctx, enemy, JudgeStrengthDown, Owner, null);
    }
}
