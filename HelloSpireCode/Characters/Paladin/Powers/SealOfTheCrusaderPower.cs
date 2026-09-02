using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.ValueProps;

namespace HelloSpire.HelloSpireCode.Characters.PaladinContent;

/// <summary>
/// The Ret rare seal, armed: no passive (the Strength was paid on cast).
/// Judge: deal 20 -- the hammer blow.
/// </summary>
public sealed class SealOfTheCrusaderPower : SealPower
{
    public const decimal JudgeDamage = 20m;

    public override async Task OnJudged(PlayerChoiceContext ctx, Creature target) =>
        await CreatureCmd.Damage(ctx, [target], JudgeDamage, ValueProp.Unpowered, Owner);
}
