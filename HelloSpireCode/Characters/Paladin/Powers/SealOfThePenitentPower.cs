using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.ValueProps;

namespace HelloSpire.HelloSpireCode.Characters.PaladinContent;

/// <summary>The Tithe seal, banked: a pure judge charge. Judge: deal 6 to ALL enemies.</summary>
public sealed class SealOfThePenitentPower : SealPower
{
    public const decimal JudgeDamage = 6m;

    public override async Task OnJudged(PlayerChoiceContext ctx, Creature target)
    {
        if (Owner.CombatState is not { } state || state.HittableEnemies.Count == 0) return;
        await CreatureCmd.Damage(ctx, state.HittableEnemies, JudgeDamage, ValueProp.Unpowered, Owner);
    }
}
