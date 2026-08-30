using BaseLib.Abstracts;
using BaseLib.Patches.UI;

namespace HelloSpire.HelloSpireCode.Gunslinger.Cylinder;

/// <summary>
/// The registration that gets <see cref="CylinderDisplay"/> on screen.
///
/// BaseLib builds a combat-UI visual for each registered custom resource, and that hook is the
/// mod's only way into the combat UI without patching it. The resource itself carries no state —
/// the cylinder lives on <see cref="Powers.CylinderPower"/>, where combat cleanup already handles
/// it — so this exists purely as the peg the widget hangs from. This mirrors how the Paladin's
/// Faith tiles reach the screen.
///
/// <c>setEachTurn</c> is left at the default so nothing is reset behind the power's back.
/// </summary>
public sealed class CylinderResource() : BasicCustomResource("HelloSpire.Cylinder")
{
    public override ICustomResourceVisualsHandler ResourceVisualsHandler() => new CylinderDisplay();
}
