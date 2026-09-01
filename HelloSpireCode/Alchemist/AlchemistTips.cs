using BaseLib.Patches.Content;
using MegaCrit.Sts2.Core.HoverTips;

namespace HelloSpire.HelloSpireCode.Alchemist;

/// <summary>
/// Static hover tips for the Alchemist's verbs. Not card keywords — actions that appear inside
/// card text ("Brew a Block Potion.", "Infuse 3 Damage.") and need an explanation on hover.
///
/// BaseLib generates the enum values at load time; the text resolves from
/// <c>static_hover_tips.json</c> under the <c>HELLOSPIRE-{FIELD_NAME}</c> key.
/// </summary>
public static class AlchemistTips
{
    /// <summary>Create a Potion in the first empty Potion Slot.</summary>
    [CustomEnum] public static StaticHoverTip Brew;

    /// <summary>A Potion that behaves normally but is removed at the end of combat.</summary>
    [CustomEnum] public static StaticHoverTip Volatile;

    /// <summary>Discard a Potion without resolving it, for the card's listed reward.</summary>
    [CustomEnum] public static StaticHoverTip Distill;

    /// <summary>Volatile Potions you use have their damage and Block increased by this much.</summary>
    [CustomEnum] public static StaticHoverTip Potency;

    /// <summary>The Potion belt: how many Slots you have and what is in them.</summary>
    [CustomEnum] public static StaticHoverTip ThePotionBelt;

    /// <summary>Add a stored effect to Unstable Concoction.</summary>
    [CustomEnum] public static StaticHoverTip Infuse;

    /// <summary>Use Unstable Concoction, resolving everything Infused into it.</summary>
    [CustomEnum] public static StaticHoverTip Unleash;
}
