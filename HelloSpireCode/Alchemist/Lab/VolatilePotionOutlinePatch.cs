using System;
using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.Potions;

namespace HelloSpire.HelloSpireCode.Alchemist.Lab;

/// <summary>
/// Volatile Potions are tracked by identity (see <see cref="LabPower.Volatile"/>), not by giving
/// each one its own subclass -- correct for game logic, but it means a freshly-Brewed Volatile
/// Block Potion is the literal same class, sprite and tooltip as one bought at a Merchant. Nothing
/// on the belt shows the difference.
///
/// NPotion (decompiled from sts2.dll) already renders two layers per belt slot: an Image
/// TextureRect for the icon, and a separate Outline TextureRect meant for exactly this kind of
/// highlight, normally driven by a per-potion silhouette asset (PotionModel.Outline) that most
/// potions -- including everything the Alchemist Brews -- don't ship. Rather than author a white
/// silhouette PNG to match all ~45 real potions' shapes, this reuses the potion's own icon as the
/// Outline texture and applies a shader (volatile_potion_outline.gdshader) that turns its alpha
/// edge into a solid purple line -- same sprite, no new art per potion.
///
/// Known gap: this only redraws the outline when Reload() runs, which happens when a potion's
/// belt slot is created, not on every Volatile-state change. A potion that stops being Volatile
/// mid-combat (Stabilize, once ported) keeps its purple outline until its slot is recreated.
///
/// VERIFY AGAINST sts2.dll if NPotion's shape ever changes -- Reload is private, so this patches
/// the exact layout confirmed there rather than a public contract.
/// </summary>
[HarmonyPatch(typeof(NPotion), "Reload")]
internal static class VolatilePotionOutlinePatch
{
    private static ShaderMaterial? _material;

    private static ShaderMaterial? Material()
    {
        if (_material != null) return _material;
        if (GD.Load<Shader>("res://HelloSpire/shaders/volatile_potion_outline.gdshader") is not { } shader)
            return null;
        return _material = new ShaderMaterial { Shader = shader };
    }

    [HarmonyPostfix]
    private static void AfterReload(NPotion __instance)
    {
        if (!__instance.IsNodeReady()) return;

        PotionModel model;
        try { model = __instance.Model; }
        catch (InvalidOperationException) { return; }

        var player = model.Owner;
        if (player?.Character is not HelloSpire.HelloSpireCode.Characters.Alchemist) return;

        var bench = AlchemistEffects.Peek(LabContext.From(player));
        if (bench == null || !bench.Volatile.Contains(model)) return;

        if (Material() is not { } material) return;

        __instance.Outline.Texture = model.Image;
        __instance.Outline.Material = material;
    }
}
