using System.Reflection;
using HarmonyLib;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Hooks;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;

namespace HelloSpire.HelloSpireCode.Gunslinger.Powers;

/// <summary>
/// The only place this mod touches the damage pipeline.
///
/// Armor has to reduce damage on its way in, and the base game has no per-power override for that
/// — so it hangs off <c>Hook.ModifyDamage</c>, which every damage instance passes through.
/// Everything else in the character is ordinary content code; if this one patch has to be reworked
/// for a game update, nothing else moves.
///
/// Armor is the only thing left here. Dodge used to sit above it, zeroing the next hit outright,
/// and it is gone: the character's premium defence is now the base game's Intangible, which the
/// game reduces on its own and this patch never sees.
///
/// VERIFY AGAINST sts2.dll. The parameter names (<c>damage</c>, <c>props</c>, <c>target</c>,
/// <c>cardSource</c>) are the ones BaseLib itself binds to on this method, so they are solid; the
/// decimal return value is inferred. <see cref="Prepare"/> refuses to apply the patch rather than
/// crash if the shape is different, which leaves Armor inert but the run playable.
///
/// Known caveat: <c>Hook.ModifyDamage</c> runs before Block is subtracted, so this recomputes the
/// split itself — a hit that Block fully absorbs must not erode Armor.
///
/// This patch only ever *reduces*; it never spends. The hook is called to answer "how much would
/// this hit be for", which the game also asks while drawing intent forecasts, so consuming a stack
/// here drained Armor to nothing before an enemy had swung. Instead it raises a pending flag, and
/// the power spends itself from <c>BeforeDamageReceived</c> — a hook that fires once, for damage
/// that is really being dealt. That keeps this function idempotent, which is the property the hook
/// actually requires.
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
            MainFile.Logger.Info("Hook.ModifyDamage not found; Armor will not reduce damage.");
            return false;
        }

        if (_modifyDamage.ReturnType != typeof(decimal))
        {
            MainFile.Logger.Info(
                $"Hook.ModifyDamage returns {_modifyDamage.ReturnType}, expected decimal; " +
                "Armor will not reduce damage.");
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
        // player hitting something, which Armor has no business touching.
        if (cardSource != null) return;

        // Unpowered damage says, in the game's own vocabulary, that powers do not apply to it --
        // and Armor is a power. Everything the Gunslinger does to itself is flagged this way
        // (GunslingerEffects.LoseHp), so without this the character's own costs would be free
        // whenever it happened to be armoured: Russian Roulette's Self-Fire, Grit Teeth's HP cost
        // and the Black Powder Round's recoil all become pure upside. The design is explicit that
        // Self-Fire is "not reduced by Block or Armor"; this is the line that makes that true. It
        // also protects any future non-Attack HP loss the game sends through this hook.
        if (props.HasFlag(ValueProp.Unpowered)) return;

        if (target == null || __result <= 0m) return;

        var armor = target.GetPower<ArmorPower>();
        if (armor is not { Amount: > 0 }) return;

        // Unblockable damage skips Block entirely, so the whole instance is "unblocked".
        var block = props.HasFlag(ValueProp.Unblockable) ? 0m : target.Block;
        var unblocked = __result - block;
        if (unblocked <= 0m) return;

        var reduced = Math.Max(0m, unblocked - armor.Amount);
        __result = block + reduced;

        armor.AbsorbedPending = true;
    }
}
