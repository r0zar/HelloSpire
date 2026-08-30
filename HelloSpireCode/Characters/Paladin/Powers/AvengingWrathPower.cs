using MegaCrit.Sts2.Core.Entities.Powers;
using HelloSpire.HelloSpireCode.Powers;

namespace HelloSpire.HelloSpireCode.Characters.PaladinContent;

/// <summary>
/// Whenever you Judge, the Seal's effect triggers twice. Holds no logic: Seals.Judge is the one
/// funnel every Judgment goes through, and it checks for this power there.
/// </summary>
public sealed class AvengingWrathPower : HelloSpirePower
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Single;
}
