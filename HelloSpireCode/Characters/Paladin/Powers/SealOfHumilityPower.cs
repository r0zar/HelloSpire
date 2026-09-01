using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;

namespace HelloSpire.HelloSpireCode.Characters.PaladinContent;

/// <summary>
/// The Holy analog of Righteousness: while held, your healing effects restore Amount additional
/// HP (Spirit.Heal reads the held seal -- the same single funnel that applies Spirit).
/// Judge: enemy loses 3 Strength until end of turn -- a defensive valve, never a heal,
/// so re-arming can never loop.
/// </summary>
public sealed class SealOfHumilityPower : SealPower
{
    public const decimal JudgeStrengthDown = 3m;

    /// <summary>Read by Spirit.Heal; zero when Chained Gauntlet disables passives.</summary>
    public decimal HealBonus => PassivesDisabled ? 0m : Amount;

    public override async Task OnJudged(PlayerChoiceContext ctx, Creature target) =>
        await PowerCmd.Apply<HumblingShacklesPower>(ctx, target, JudgeStrengthDown, Owner, null);
}
