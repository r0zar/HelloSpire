using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;

namespace HelloSpire.HelloSpireCode.Characters.PaladinContent;

/// <summary>
/// The Holy seal, banked: a pure judge charge. Judge: gain 2 Spirit -- the candles grow
/// brighter, never more numerous. (A return-from-exhaust judge was tried and rejected: over
/// deck cycles it made healing unbounded, which breaks the candle-clock.)
/// </summary>
public sealed class SealOfLightPower : SealPower
{
    public const int JudgeSpirit = 2;

    public override async Task OnJudged(PlayerChoiceContext ctx, Creature target)
    {
        if (Owner.Player is not { } player) return;
        await Spirit.Gain(ctx, player, JudgeSpirit);
    }
}
