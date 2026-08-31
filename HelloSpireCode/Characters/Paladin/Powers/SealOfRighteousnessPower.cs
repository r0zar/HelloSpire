using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Context;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;

namespace HelloSpire.HelloSpireCode.Characters.PaladinContent;

/// <summary>
/// Gaining it grants Amount Strength. Judged: deal 5 -- small, because Judgment
/// now fires every Seal you have and each one is priced for the stack. The starter Seal, granted by the Holy Book --
/// the Defect's Cracked Core shape: passive trickle, real evoke.
/// </summary>
public sealed class SealOfRighteousnessPower : SealPower
{
    public const decimal JudgeDamage = 5m;

    /// <summary>
    /// Gaining the seal grants real Strength -- an additive-damage passive was Strength wearing a
    /// seal skin, and an on-hit proc sprays 1s on every attack. Same mirror pattern as the game's
    /// TemporaryStrengthPower, minus the end-of-turn take-back: the Strength is kept.
    /// </summary>
    public override async Task BeforeApplied(Creature target, decimal amount, Creature? applier, CardModel? cardSource) =>
        await PowerCmd.Apply<StrengthPower>(new ThrowingPlayerChoiceContext(), target, amount, applier, cardSource, silent: true);

    public override async Task AfterPowerAmountChanged(PlayerChoiceContext choiceContext, PowerModel power,
        decimal amount, Creature? applier, CardModel? cardSource)
    {
        if (power != this || amount == Amount) return;
        await PowerCmd.Apply<StrengthPower>(choiceContext, Owner, amount, applier, cardSource, silent: true);
    }

    public override async Task OnJudged(PlayerChoiceContext ctx, Creature target) =>
        await CreatureCmd.Damage(ctx, [target], JudgeDamage, ValueProp.Unpowered, Owner);
}
