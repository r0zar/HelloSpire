using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Nodes.Combat;

namespace HelloSpire.HelloSpireCode.Characters;

/// <summary>
/// The battle-UI energy counter is placeholder art too -- the Paladin showed the Ironclad's red
/// orb, the Alchemist the Silent's green one. Each character's repaint shader already maps
/// exactly those palettes (red to gold, green to brown), so this postfix hands the counter's
/// art layers the same material CharacterSkins uses for the body. The number label and the
/// particle containers are left untouched.
/// </summary>
[HarmonyPatch(typeof(NEnergyCounter), "_Ready")]
internal static class EnergyCounterSkins
{
    [HarmonyPostfix]
    private static void Reskin(NEnergyCounter __instance)
    {
        var material = CharacterSkins.MaterialFor(__instance._player?.Character);
        if (material == null) return;
        Paint(__instance, material);
    }

    private static void Paint(Node node, Material material)
    {
        foreach (var child in node.GetChildren())
        {
            var name = child.Name.ToString();
            if (name == "Label" || name.StartsWith("EnergyVfx")) continue;
            if (child is CanvasItem item) item.Material = material;
            Paint(child, material);
        }
    }
}
