using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;

namespace HelloSpire.HelloSpireCode.Characters.PaladinContent;

/// <summary>
/// The Holy common, banked: a pure judge charge.
/// Judge: enemy loses 3 Strength until end of turn -- a defensive valve, never a heal.
/// </summary>
public sealed class SealOfHumilityPower : SealPower
{
    public const decimal JudgeStrengthDown = 3m;

    public override async Task OnJudged(PlayerChoiceContext ctx, Creature target) =>
        await PowerCmd.Apply<HumblingShacklesPower>(ctx, target, JudgeStrengthDown, Owner, null);
}
