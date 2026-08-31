using BaseLib.Patches.Content;
using MegaCrit.Sts2.Core.HoverTips;

namespace HelloSpire.HelloSpireCode.Alchemist;

/// <summary>
/// Static hover tips for the Alchemist's verbs. Not card keywords — actions that appear inside
/// card text ("Brew a Block Potion.", "Invest 3 Gold.") and need an explanation on hover.
///
/// BaseLib generates the enum values at load time; the text resolves from
/// <c>static_hover_tips.json</c> under the <c>HELLOSPIRE-{FIELD_NAME}</c> key.
/// </summary>
public static class AlchemistTips
{
    /// <summary>Create a Potion in the first empty Potion Slot.</summary>
    [CustomEnum] public static StaticHoverTip Brew;

    /// <summary>Discard a Potion without resolving it, for the card's listed reward.</summary>
    [CustomEnum] public static StaticHoverTip Distill;

    /// <summary>Optionally pay Gold from your actual run total for an additional effect.</summary>
    [CustomEnum] public static StaticHoverTip Invest;

    /// <summary>Optionally pay Max HP, permanently, for an additional effect.</summary>
    [CustomEnum] public static StaticHoverTip Render;

    /// <summary>Potions you use have their damage and Block increased by this much.</summary>
    [CustomEnum] public static StaticHoverTip Potency;

    /// <summary>Consuming one kind of value specifically to produce a different kind.</summary>
    [CustomEnum] public static StaticHoverTip Transform;

    /// <summary>The Potion belt: how many Slots you have and what is in them.</summary>
    [CustomEnum] public static StaticHoverTip ThePotionBelt;
}
