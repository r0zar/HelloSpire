using HarmonyLib;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Models;

namespace HelloSpire.HelloSpireCode.Alchemist.Lab;

/// <summary>
/// QuickSilver's "if you played a Skill this turn" needs a turn-scoped counter that fires for
/// every Skill play, not just ones that already go through one of this class's own helper
/// functions (Alchemy.Exhaust, Alchemy.Create, etc. each track their own thing already). There is
/// no existing hook for "any card of type X was played" in this codebase, so this patches
/// CardModel.OnPlayWrapper (public, decompiled from sts2.dll) directly, the same base-game entry
/// point <see cref="PotionUsePatch"/> uses for Potions.
///
/// A prefix rather than the async-postfix-continuation dance PotionUsePatch needs: this only counts
/// "was a Skill played this turn" by the time a later card reads it, and the action queue resolves
/// one card fully before the next one starts, so incrementing when the play begins is exactly as
/// correct as incrementing when it ends for anything checking the counter afterward.
///
/// Fires for every character's every Skill, same as PotionUsePatch fires for every Potion -- the
/// Owner-is-Alchemist check is load-bearing for the same reason (AlchemistEffects.Bench would
/// otherwise attach a LabPower to a Skill-playing Silent or Watcher).
/// </summary>
[HarmonyPatch(typeof(CardModel), nameof(CardModel.OnPlayWrapper))]
internal static class SkillPlayTrackerPatch
{
    [HarmonyPrefix]
    private static void BeforeOnPlayWrapper(CardModel __instance)
    {
        if (__instance.Type != CardType.Skill) return;

        var player = __instance.Owner;
        if (player?.Character is not HelloSpire.HelloSpireCode.Characters.Alchemist) return;

        var bench = player.Creature?.GetPower<LabPower>();
        if (bench != null) bench.SkillsPlayedThisTurn++;
    }
}
