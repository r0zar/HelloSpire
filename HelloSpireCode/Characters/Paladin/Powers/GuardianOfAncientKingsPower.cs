using HelloSpire.HelloSpireCode.Powers;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;

namespace HelloSpire.HelloSpireCode.Characters.PaladinContent;

/// <summary>
/// Whenever one of the bearer's cards grants them Plating, it grants Amount more. The kings
/// stand behind every stone laid: the amp for the cheap-Plating engine (Prayer, Blessing of
/// Protection, Avenger's Shield, Hammer of the Righteous, Shield of the Righteous...).
/// Cards only -- relic and power ticks are not blessed.
/// </summary>
public sealed class GuardianOfAncientKingsPower : HelloSpirePower
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;

    public override decimal ModifyPowerAmountGivenAdditive(PowerModel power, Creature giver,
        decimal amount, Creature? target, CardModel? cardSource)
    {
        if (power is not PlatingPower || cardSource == null) return 0m;
        if (giver != Owner || target != Owner) return 0m;
        Flash();
        return Amount;
    }
}
