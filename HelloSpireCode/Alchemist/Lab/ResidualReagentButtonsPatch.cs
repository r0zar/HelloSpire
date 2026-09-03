using HarmonyLib;
using HelloSpire.HelloSpireCode.Alchemist.Potions;
using MegaCrit.Sts2.Core.Nodes.Potions;

namespace HelloSpire.HelloSpireCode.Alchemist.Lab;

/// <summary>
/// Residual Reagent should never be a choice the player can make -- not Used, not Discarded, only
/// ever removed by Distilling it away. There's no per-instance permission hook on either action at
/// the model/command level (see the class doc comment on ResidualReagent for why), so this reaches
/// one level up: NPotionPopup (decompiled from sts2.dll) recomputes both its Use and Discard
/// buttons' enabled state in one place, RefreshButtons(), called whenever the popup opens or the
/// game state it depends on changes (turn start, end-turn toggle, combat state) -- a Postfix here
/// forces both buttons off for a Residual Reagent every time that happens, so the player is never
/// offered either option to begin with.
///
/// VERIFY AGAINST sts2.dll if NPotionPopup's shape ever changes -- RefreshButtons and the two
/// button fields are public only because the assembly is Publicized for modding, not because
/// they're a documented contract.
/// </summary>
[HarmonyPatch(typeof(NPotionPopup), nameof(NPotionPopup.RefreshButtons))]
internal static class ResidualReagentButtonsPatch
{
    [HarmonyPostfix]
    private static void AfterRefreshButtons(NPotionPopup __instance)
    {
        if (__instance.Potion is not ResidualReagent) return;

        __instance._useButton.Disable();
        __instance._discardButton.Disable();
    }
}
