using System.Collections.Generic;
using System.IO;
using System.Linq;
using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Bindings.MegaSpine;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Nodes.Combat;

namespace HelloSpire.HelloSpireCode.Characters;

/// <summary>
/// Per-character spine skeletons. A character that ships plain Spine files in the mod's
/// spine/&lt;character&gt;/ folder (one .atlas, its .skel beside it, page .png files as the
/// atlas names them) gets its combat body swapped to that rig when visuals are created —
/// the inherited Ironclad rig is replaced for that character only, and nothing else in
/// the game (vanilla Ironclad included) sees the custom assets.
///
/// Loading technique borrowed from CustomSkeletonLoader: the game's spine-godot module
/// loads plain files from any filesystem path (SpineAtlasResource.load_from_atlas_file +
/// SpineSkeletonFileResource.load_from_file composed into a SpineSkeletonDataResource) —
/// no .spskel/.spatlas wrappers or pck import required.
///
/// Missing folder or failed load degrades silently to the CharacterSkins shader repaint,
/// same spirit as that patch's own fallback. Rest-site/shop scenes still use the shader
/// repaint for now (they instantiate separate spine scenes; see RoomSkins).
/// </summary>
[HarmonyPatch(typeof(Creature), nameof(Creature.CreateVisuals))]
internal static class CharacterSkeletons
{
    private static readonly Dictionary<string, MegaSkeletonDataResource?> Cache = new();

    private static string? FolderFor(MegaCrit.Sts2.Core.Models.CharacterModel? character) => character switch
    {
        Paladin => "paladin",
        Gunslinger => "gunslinger",
        Alchemist => "alchemist",
        _ => null,
    };

    /// <summary>The custom rig for a HelloSpire character, or null to keep the inherited one.</summary>
    internal static MegaSkeletonDataResource? SkeletonFor(MegaCrit.Sts2.Core.Models.CharacterModel? character)
    {
        if (FolderFor(character) is not { } folder) return null;
        if (Cache.TryGetValue(folder, out var cached)) return cached;
        var built = Build(folder);
        Cache[folder] = built; // negative results cached too — one disk probe per run
        return built;
    }

    private static MegaSkeletonDataResource? Build(string folder)
    {
        var modDir = Path.GetDirectoryName(typeof(CharacterSkeletons).Assembly.Location);
        if (modDir == null) return null;
        var dir = Path.Combine(modDir, "spine", folder);
        if (!Directory.Exists(dir)) return null;
        var atlasPath = Directory.GetFiles(dir, "*.atlas").FirstOrDefault();
        if (atlasPath == null) return null;
        var skelPath = Path.ChangeExtension(atlasPath, ".skel");
        if (!File.Exists(skelPath)) return null;

        var atlas = ClassDB.Instantiate("SpineAtlasResource").AsGodotObject();
        if (atlas == null) return null;
        if (atlas.Call("load_from_atlas_file", atlasPath).AsInt64() != 0)
        {
            GD.PushWarning($"[HelloSpire] spine atlas failed to load: {atlasPath}");
            return null;
        }
        var skelFile = ClassDB.Instantiate("SpineSkeletonFileResource").AsGodotObject();
        if (skelFile == null) return null;
        if (skelFile.Call("load_from_file", skelPath).AsInt64() != 0)
        {
            GD.PushWarning($"[HelloSpire] spine skeleton failed to load: {skelPath}");
            return null;
        }
        var data = ClassDB.Instantiate("SpineSkeletonDataResource").AsGodotObject();
        if (data == null) return null;
        data.Set("atlas_res", atlas);
        data.Set("skeleton_file_res", skelFile);
        GD.Print($"[HelloSpire] loaded custom skeleton for '{folder}' from {dir}");
        return new MegaSkeletonDataResource(data);
    }

    [HarmonyPostfix]
    private static void SwapSkeleton(Creature __instance, NCreatureVisuals? __result)
    {
        // NEVER throw out of this postfix: an exception here aborts the combat setup
        // loop and every creature in the fight loses its node (learned the hard way).
        try
        {
            if (__result == null || SkeletonFor(__instance.Player?.Character) is not { } rig) return;
            var body = __result.GetNodeOrNull<Node2D>("%Visuals");
            if (body == null || body.GetClass() != "SpineSprite") return;

            var sprite = new MegaSprite(body);
            // The animation state may not exist yet at CreateVisuals time —
            // GetAnimationState THROWS in that case (per sts2.xml, prefer Try*).
            string? current = null;
            try { current = sprite.GetAnimationState()?.GetCurrent(0)?.GetAnimation()?.GetName(); }
            catch { /* no state yet; the game starts idle right after */ }

            sprite.SetSkeletonDataRes(rig);

            try
            {
                if (current != null && sprite.GetSkeleton()?.GetData()?.HasAnimation(current) == true)
                    sprite.GetAnimationState()?.SetAnimation(current, true, 0);
            }
            catch { /* restore is best-effort; default idle takes over */ }
        }
        catch (System.Exception e)
        {
            GD.PushWarning($"[HelloSpire] skeleton swap failed, keeping inherited rig: {e.Message}");
        }
    }
}
