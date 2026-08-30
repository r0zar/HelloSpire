using MegaCrit.Sts2.Core.Entities.Powers;
using HelloSpire.HelloSpireCode.Powers;

namespace HelloSpire.HelloSpireCode.Characters.PaladinContent;

/// <summary>
/// The bearer is the Beacon: whenever a Paladin heal lands on any player, the bearer also heals
/// Amount. Holds no logic -- Spirit.Heal is the funnel every Paladin heal goes through, and it
/// checks for this power there.
/// </summary>
public sealed class BeaconOfLightPower : HelloSpirePower
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;
}
