using BaseLib.Patches.Content;
using MegaCrit.Sts2.Core.HoverTips;

namespace HelloSpire.HelloSpireCode.Characters.PaladinContent;

/// <summary>
/// Static hover tips for the Paladin's verbs: the seal-and-judgment package and the Tithe face.
/// BaseLib generates the enum values at load time; text resolves from static_hover_tips.json
/// under HELLOSPIRE-{FIELD_NAME}.
/// </summary>
public static class PaladinTips
{
    /// <summary>One held stance; recast replaces, judge consumes.</summary>
    [CustomEnum] public static StaticHoverTip Seal;

    /// <summary>Fire the held Seal's judge payoff, then consume it. Judge powers fire per instance.</summary>
    [CustomEnum] public static StaticHoverTip Judge;

    /// <summary>The discard face: fires when the card is discarded; never Exhausts.</summary>
    [CustomEnum] public static StaticHoverTip Tithe;
}
