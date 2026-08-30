using HarmonyLib;
using MegaCrit.Sts2.Core.Context;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Nodes.RestSite;
using MegaCrit.Sts2.Core.Nodes.Rooms;

namespace HelloSpire.HelloSpireCode.Characters;

/// <summary>
/// The combat repaint, everywhere else the character body shows up. Rest sites and the shop
/// instantiate the character's spine scenes directly (RestSiteAnimPath / MerchantAnimPath, both
/// inherited from the Ironclad), so without this the unpainted Ironclad sits at the campfire
/// and browses the shop. Same materials as CharacterSkins, applied to every SpineSprite in the
/// instantiated scene.
/// </summary>
internal static class RoomSkins
{
    [HarmonyPatch(typeof(NRestSiteCharacter), nameof(NRestSiteCharacter.Create))]
    private static class RestSite
    {
        [HarmonyPostfix]
        private static void Reskin(Player player, NRestSiteCharacter __result)
        {
            if (CharacterSkins.MaterialFor(player.Character) is { } material)
                CharacterSkins.ApplyToSpines(__result, material);
        }
    }

    [HarmonyPatch(typeof(NMerchantRoom), "AfterRoomIsLoaded")]
    private static class Merchant
    {
        [HarmonyPostfix]
        private static void Reskin(NMerchantRoom __instance)
        {
            var players = __instance._players;
            var visuals = __instance._playerVisuals;
            for (var i = 0; i < players.Count && i < visuals.Count; i++)
                if (CharacterSkins.MaterialFor(players[i].Character) is { } material)
                    CharacterSkins.ApplyToSpines(visuals[i], material);
        }
    }
}
