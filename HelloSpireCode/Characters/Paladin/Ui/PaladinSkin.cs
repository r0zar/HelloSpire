using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Nodes.Combat;

namespace HelloSpire.HelloSpireCode.Characters.PaladinContent;

/// <summary>
/// The Paladin wears the Ironclad's rig, repainted at render time. The Paladin's
/// CustomVisualPath is left at the placeholder default (the Ironclad creature scene), so the
/// full spine skeleton and all six animations come along for free; this patch hands the
/// Paladin's SpineSprite a palette-remap shader through spine-godot's own material slot
/// (set_normal_material), which recolors reds/leathers to gold-and-white per pixel.
///
/// Why a shader and not swapped atlas textures: SpineAtlasResource rebuilds its internal
/// state from atlas_data when reassigned, which discards injected texture pages and renders
/// nothing. A material never touches the skeleton data, applies to this instance only, and
/// degrades to the normal Ironclad look if anything is missing.
/// </summary>
[HarmonyPatch(typeof(Creature), nameof(Creature.CreateVisuals))]
internal static class PaladinSkin
{
    private static ShaderMaterial? _material;

    [HarmonyPostfix]
    private static void Reskin(Creature __instance, NCreatureVisuals? __result)
    {
        if (__result == null || __instance.Player?.Character is not Paladin) return;

        var body = __result.GetNodeOrNull<Node2D>("%Visuals");
        if (body == null || body.GetClass() != "SpineSprite") return;

        if (_material == null)
        {
            if (GD.Load<Shader>("res://HelloSpire/shaders/paladin_repaint.gdshader") is not { } shader) return;
            _material = new ShaderMaterial { Shader = shader };
        }
        body.Call("set_normal_material", _material);
    }
}
