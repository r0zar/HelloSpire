using BaseLib.Patches.Content;
using MegaCrit.Sts2.Core.HoverTips;

namespace HelloSpire.HelloSpireCode.Characters.PaladinContent;

/// <summary>
/// Static hover tips for the Paladin's keywords. BaseLib generates the enum values at load time;
/// text resolves from static_hover_tips.json under HELLOSPIRE-{FIELD_NAME}.
/// </summary>
public static class PaladinTips
{
    /// <summary>When a Blessed card is played, your Auras pulse again.</summary>
    [CustomEnum] public static StaticHoverTip Blessed;

    /// <summary>An Aura is a Power that affects the whole party and can be pulsed by Blessed cards.</summary>
    [CustomEnum] public static StaticHoverTip Aura;
}
