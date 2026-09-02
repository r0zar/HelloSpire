using HarmonyLib;
using MegaCrit.Sts2.Core.Models;

namespace HelloSpire.HelloSpireCode.Alchemist.Lab;

/// <summary>
/// QuickSilver's "if the last card you played was a Skill" needs to know the type of whatever
/// card resolved immediately before the one currently playing. There is no existing hook for
/// "any card was played" in this codebase, so this patches CardModel.OnPlayWrapper (public,
/// decompiled from sts2.dll) directly, the same base-game entry point PotionUsePatch uses for
/// Potions and SkillPlayTrackerPatch used before it.
///
/// A plain synchronous prefix is enough, unlike PotionUsePatch's async-postfix dance: a Harmony
/// prefix runs before ANY part of the original method executes, sync or async, so this is
/// guaranteed to update the bench before the card's own OnPlay body -- which might read
/// PreviousCardType -- ever starts. The old value of LastCardType is stashed into
/// PreviousCardType before being overwritten with this card's own type, so a card reading
/// PreviousCardType during its own OnPlay sees the card played before it, never itself.
///
/// Fires for every character's every card, same as PotionUsePatch fires for every Potion -- the
/// Owner-is-Alchemist check is load-bearing for the same reason (AlchemistEffects.Bench would
/// otherwise attach a LabPower to a card-playing Silent or Watcher).
/// </summary>
[HarmonyPatch(typeof(CardModel), nameof(CardModel.OnPlayWrapper))]
internal static class LastCardTypeTrackerPatch
{
    [HarmonyPrefix]
    private static void BeforeOnPlayWrapper(CardModel __instance)
    {
        var player = __instance.Owner;
        if (player?.Character is not HelloSpire.HelloSpireCode.Characters.Alchemist) return;

        var bench = player.Creature?.GetPower<LabPower>();
        if (bench == null) return;

        bench.PreviousCardType = bench.LastCardType;
        bench.LastCardType = __instance.Type;
    }
}
