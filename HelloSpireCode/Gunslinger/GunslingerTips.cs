using BaseLib.Patches.Content;
using MegaCrit.Sts2.Core.HoverTips;

namespace HelloSpire.HelloSpireCode.Gunslinger;

/// <summary>
/// Static hover tips for the Gunslinger's verbs. These are not card keywords — they are actions that
/// appear inside card text ("Fire 2.", "Cycle 1.") and need an explanation on hover.
///
/// BaseLib generates the enum values at load time; the text is resolved from
/// <c>static_hover_tips.json</c> under the <c>HELLOSPIRE-{FIELD_NAME}</c> key.
/// </summary>
public static class GunslingerTips
{
    /// <summary>Place a Round into the first empty chamber clockwise from the hammer.</summary>
    [CustomEnum] public static StaticHoverTip Load;

    /// <summary>Resolve the chamber under the hammer, then advance the hammer.</summary>
    [CustomEnum] public static StaticHoverTip Fire;

    /// <summary>Advance the hammer without firing.</summary>
    [CustomEnum] public static StaticHoverTip Cycle;

    /// <summary>Move the hammer to a random chamber.</summary>
    [CustomEnum] public static StaticHoverTip Spin;

    /// <summary>A Fire that resolves an empty chamber.</summary>
    [CustomEnum] public static StaticHoverTip Click;

    /// <summary>Fire the chamber at yourself for the Round's printed damage as HP loss.</summary>
    [CustomEnum] public static StaticHoverTip SelfFire;

    /// <summary>The revolver itself: six chambers and a hammer.</summary>
    [CustomEnum] public static StaticHoverTip TheCylinder;

    /// <summary>Which Round each class hands the Gunslinger. See <see cref="Cylinder.AmmoAffinity"/>.</summary>
    [CustomEnum] public static StaticHoverTip MatchedAmmo;
}
