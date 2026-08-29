using System.Reflection;
using HarmonyLib;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Hooks;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;

namespace HelloSpire.HelloSpireCode.Gunslinger.Powers;

/// <summary>
/// The only place this mod touches the damage pipeline.
///
/// Armor and Dodge both have to reduce damage on its way in, and the base game has no per-power
/// override for that — so they hang off <c>Hook.ModifyDamage</c>, which every damage instance passes
/// through. Everything else in the character is ordinary content code; if this one patch has to be
/// reworked for a game update, nothing else moves.
///
/// VERIFY AGAINST sts2.dll. The parameter names (<c>damage</c>, <c>props</c>, <c>target</c>,
/// <c>cardSource</c>) are the ones BaseLib itself binds to on this method, so they are solid; the
/// decimal return value is inferred. <see cref="Prepare"/> refuses to apply the patch rather than
/// crash if the shape is different, which leaves Armor and Dodge inert but the run playable.
///
/// Known caveat: <c>Hook.ModifyDamage</c> runs before Block is subtracted, so this recomputes the
/// split itself — a hit that Block fully absorbs must not erode Armor. If the game ever calls this
/// hook to render a forecast rather than to deal damage, Dodge and Armor will over-consume; that is
/// the first thing to check if they drain faster than expected in play.
/// </summary>
[HarmonyPatch]
internal static class GunslingerDamagePatch
{
    private static MethodInfo? _modifyDamage;

    private static bool Prepare()
    {
        _modifyDamage ??= AccessTools.Method(typeof(Hook), nameof(Hook.ModifyDamage));

        if (_modifyDamage == null)
        {
            MainFile.Logger.Info("Hook.ModifyDamage not found; Armor and Dodge will not reduce damage.");
            return false;
        }

        if (_modifyDamage.ReturnType != typeof(decimal))
        {
            MainFile.Logger.Info(
                $"Hook.ModifyDamage returns {_modifyDamage.ReturnType}, expected decimal; " +
                "Armor and Dodge will not reduce damage.");
            return false;
        }

        return true;
    }

    private static MethodBase TargetMethod() => _modifyDamage!;

    [HarmonyPostfix]
    private static void ReduceIncomingDamage(ref decimal __result, Creature target, ValueProp props,
        CardModel? cardSource)
    {
        // Enemies attack through their moves, never through cards. A non-null card source is the
        // player hitting something, which Armor and Dodge have no business touching.
        if (cardSource != null) return;
        if (target == null || __result <= 0m) return;

        var dodge = target.GetPower<DodgePower>();
        if (dodge is { Amount: > 0 })
        {
            __result = 0m;
            _ = PowerCmd.ModifyAmount(null!, dodge, -1m, null, null, false);
            return;
        }

        var armor = target.GetPower<ArmorPower>();
        if (armor is not { Amount: > 0 }) return;

        // Unblockable damage skips Block entirely, so the whole instance is "unblocked".
        var block = props.HasFlag(ValueProp.Unblockable) ? 0m : target.Block;
        var unblocked = __result - block;
        if (unblocked <= 0m) return;

        var reduced = Math.Max(0m, unblocked - armor.Amount);
        __result = block + reduced;

        GunslingerHooks.NotifyArmorPrevented(target);

        if (armor.SkipNextDecrease)
        {
            armor.SkipNextDecrease = false;
            return;
        }

        _ = PowerCmd.ModifyAmount(null!, armor, -1m, null, null, false);
    }
}
