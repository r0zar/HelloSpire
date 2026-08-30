using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Nodes.Combat;

namespace HelloSpire.HelloSpireCode.Characters.PaladinContent;

/// <summary>
/// The Paladin wears the Ironclad's rig, repainted. The Paladin's CustomVisualPath is left at
/// the placeholder default (the Ironclad creature scene), so the full spine skeleton and all
/// six animations come along for free; this patch then swaps the atlas texture pages for our
/// gold-and-white repaint on the Paladin's instance only.
///
/// The swap duplicates the SpineSkeletonDataResource and SpineAtlasResource before touching
/// them, so the real Ironclad (present in multiplayer lobbies) keeps his own paint. Property
/// names ("skeleton_data_res", "atlas_res", "textures") are the spine-godot 4.2 runtime's; the
/// whole thing is guarded so a mismatch degrades to the normal Ironclad look, never a crash.
/// </summary>
[HarmonyPatch(typeof(Creature), nameof(Creature.CreateVisuals))]
internal static class PaladinSkin
{
    private static Texture2D? _page1;
    private static Texture2D? _page2;

    [HarmonyPostfix]
    private static void Reskin(Creature __instance, NCreatureVisuals? __result)
    {
        if (__result == null || __instance.Player?.Character is not Paladin) return;

        var body = __result.GetNodeOrNull<Node2D>("%Visuals");
        if (body == null || body.GetClass() != "SpineSprite") return;

        if (body.Get("skeleton_data_res").As<Resource>() is not { } data) return;
        if (data.Get("atlas_res").As<Resource>() is not { } atlas) return;

        _page1 ??= GD.Load<Texture2D>("res://HelloSpire/images/creature/paladin_atlas.png");
        _page2 ??= GD.Load<Texture2D>("res://HelloSpire/images/creature/paladin_atlas_2.png");
        if (_page1 == null) return;

        var old = atlas.Get("textures").AsGodotArray();
        var textures = new Godot.Collections.Array();
        for (var i = 0; i < old.Count; i++)
            textures.Add(i == 0 ? Variant.From(_page1) : i == 1 && _page2 != null ? Variant.From(_page2) : old[i]);

        var atlas2 = (Resource)atlas.Duplicate();
        atlas2.Set("textures", textures);
        var data2 = (Resource)data.Duplicate();
        data2.Set("atlas_res", atlas2);
        body.Call("set_skeleton_data_res", data2);
    }
}
