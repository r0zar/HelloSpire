using System.Collections.Generic;
using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Nodes.Combat;

namespace HelloSpire.HelloSpireCode.Characters;

/// <summary>
/// All three HelloSpire characters wear the Ironclad's spine rig, each repainted at render time
/// by its own palette-remap shader through spine-godot's material slot (set_normal_material):
/// Paladin gold-and-white, Gunslinger black-and-tan with ember glow, Alchemist greens with a
/// toxic glow. Resource surgery is off the table (SpineAtlasResource rebuilds and drops
/// injected textures); a material is instance-only and degrades to plain Ironclad if missing.
/// </summary>
[HarmonyPatch(typeof(Creature), nameof(Creature.CreateVisuals))]
internal static class CharacterSkins
{
    private static readonly Dictionary<string, ShaderMaterial> Cache = new();

    [HarmonyPostfix]
    private static void Reskin(Creature __instance, NCreatureVisuals? __result)
    {
        var shaderPath = __instance.Player?.Character switch
        {
            Paladin => "res://HelloSpire/shaders/paladin_repaint.gdshader",
            Gunslinger => "res://HelloSpire/shaders/gunslinger_repaint.gdshader",
            Alchemist => "res://HelloSpire/shaders/alchemist_repaint.gdshader",
            _ => null,
        };
        if (shaderPath == null || __result == null) return;

        var body = __result.GetNodeOrNull<Node2D>("%Visuals");
        if (body == null || body.GetClass() != "SpineSprite") return;

        if (!Cache.TryGetValue(shaderPath, out var material))
        {
            if (GD.Load<Shader>(shaderPath) is not { } shader) return;
            material = new ShaderMaterial { Shader = shader };
            Cache[shaderPath] = material;
        }
        body.Call("set_normal_material", material);
    }
}
