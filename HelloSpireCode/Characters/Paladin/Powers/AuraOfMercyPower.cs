using HelloSpire.HelloSpireCode.Powers;
using MegaCrit.Sts2.Core.Entities.Powers;

namespace HelloSpire.HelloSpireCode.Characters.PaladinContent;

/// <summary>
/// Marker for Aura of Mercy: whenever the bearer gains Spirit, ALL players heal that much HP.
/// The logic lives in <see cref="Spirit.Gain"/> -- the one funnel every Spirit gain goes through --
/// so this class holds none.
/// </summary>
public sealed class AuraOfMercyPower : HelloSpirePower
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Single;
}
