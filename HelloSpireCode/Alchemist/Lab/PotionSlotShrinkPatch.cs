using System.Collections.Generic;
using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Nodes.Potions;

namespace HelloSpire.HelloSpireCode.Alchemist.Lab;

/// <summary>
/// Vial Bandolier's temporary Potion Slots are real slots (see Belt.GrantTemporarySlots), taken
/// back at combat end by LabPower.AfterCombatEnd via LabBridge.LoseSlots, which shrinks
/// Player.MaxPotionCount correctly -- confirmed by decompiling Player.SetMaxPotionCountInternal,
/// which relocates or discards any Potion in a truncated slot and fires MaxPotionCountChanged.
///
/// The belt UI does not follow. NPotionContainer.GrowPotionHolders (decompiled from sts2.dll) is
/// exactly what its name says: it only ever adds holder nodes in response to that event, for
/// however many the count grew by. Nothing in the base game ever shrinks MaxPotionCount mid-run, so
/// nobody wrote the other half -- the empty holder nodes from a spent Vial Bandolier just sit there
/// forever, looking like real slots the player no longer has, even though MaxPotionCount itself is
/// already correct underneath them.
///
/// Fixed by trimming the holder list down to the new count ourselves before the original method
/// runs. Harmless on an actual grow: the trim loop below is a no-op whenever the count went up, and
/// the original's own loop still runs afterward exactly as before. Only ever removes empty holders,
/// from the end -- if the last holder still has a Potion in it this stops rather than destroy
/// something the player can see, though by the time a real shrink reaches this patch the truncated
/// slots are always already empty (Volatile Potions are discarded, and any real Potion relocated,
/// before LoseSlots runs -- see AfterCombatEnd's ordering comment).
///
/// VERIFY AGAINST sts2.dll if NPotionContainer's private fields or GrowPotionHolders change shape.
/// </summary>
[HarmonyPatch(typeof(NPotionContainer), "GrowPotionHolders")]
internal static class PotionSlotShrinkPatch
{
    private static readonly AccessTools.FieldRef<NPotionContainer, List<NPotionHolder>> HoldersRef =
        AccessTools.FieldRefAccess<NPotionContainer, List<NPotionHolder>>("_holders");

    private static readonly AccessTools.FieldRef<NPotionContainer, Control> ContainerRef =
        AccessTools.FieldRefAccess<NPotionContainer, Control>("_potionHolders");

    [HarmonyPrefix]
    private static void BeforeGrowPotionHolders(NPotionContainer __instance, int newMaxPotionSlots)
    {
        var holders = HoldersRef(__instance);
        var container = ContainerRef(__instance);

        while (holders.Count > newMaxPotionSlots)
        {
            var last = holders[^1];
            if (last.HasPotion) break;

            holders.RemoveAt(holders.Count - 1);
            container.RemoveChild(last);
            last.QueueFree();
        }
    }
}
