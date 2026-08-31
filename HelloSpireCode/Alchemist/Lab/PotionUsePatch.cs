using System.Reflection;
using System.Threading.Tasks;
using HarmonyLib;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;

namespace HelloSpire.HelloSpireCode.Alchemist.Lab;

/// <summary>
/// The only place this mod hooks "a Potion was used" for the Alchemist.
///
/// <see cref="Belt.OnPotionUsed"/> already does everything Closed System, Cork Stopper, Residual
/// Heat and Reactive Mixture need — it just had nothing calling it. There is no base-game hook
/// shaped for this: <c>Hook.AfterPotionUsed</c> (see sts2.xml) hands a postfix
/// <c>IRunState</c>/<c>ICombatState</c>/<c>PotionModel</c>/<c>Creature</c>, none of which is a
/// <c>Player</c> or the <see cref="PlayerChoiceContext"/> <see cref="Belt.OnPotionUsed"/> needs, and
/// <see cref="MegaCrit.Sts2.Core.Models.AbstractModel.AfterPotionUsed"/> is documented as only firing
/// for relics out of combat.
///
/// <see cref="MegaCrit.Sts2.Core.Models.PotionModel.OnUseWrapper"/> is the real target instead
/// (decompiled from sts2.dll): it is the method that runs the potion's own effect and then calls
/// <c>Hook.AfterPotionUsed</c>, and it already has both the <c>PotionModel</c> instance (so
/// <c>Owner</c> gives the <c>Player</c>) and the exact <see cref="PlayerChoiceContext"/> in scope as
/// its own parameter — no reconstructing one from scratch, no digging a <c>Player</c> out of a
/// <c>Creature</c>.
///
/// <c>OnUseWrapper</c> is <c>async Task</c>, so a plain Harmony postfix would fire the instant the
/// method returns its (still-incomplete) Task — before the potion's own effect has actually run,
/// which is the wrong moment for "the potion was used". The fix is the standard async-Harmony
/// pattern: take <c>__result</c> by <c>ref</c> and replace it with a wrapper Task that awaits the
/// original to completion first, then does the notification. Whatever awaits
/// <c>OnUseWrapper</c> (<c>UsePotionAction.ExecuteAction</c>) ends up awaiting the wrapper instead,
/// so from the caller's point of view nothing changes except that our bit of work now also
/// completes before the await returns.
///
/// This patch fires for every character's every Potion use, not just the Alchemist's — Potions are
/// a base-game system, and <c>PotionModel.OnUseWrapper</c> has no idea a modded character exists.
/// The Owner-is-Alchemist check below is load-bearing: <see cref="AlchemistEffects.Bench"/> (which
/// <see cref="Belt.OnPotionUsed"/> calls into) *applies* a <c>LabPower</c> to whatever Creature it's
/// given if that Creature doesn't have one yet. Skipping the check would silently attach the
/// Alchemist's bench Power to an Ironclad, Silent, Defect or Watcher the first time they drank any
/// Potion in any run — a real cross-character bug, not a theoretical one.
///
/// VERIFY AGAINST sts2.dll. <see cref="Prepare"/> refuses to apply the patch rather than crash if
/// the method's shape has changed, which leaves the potion-use engines inert but the run playable —
/// the same fallback discipline as <see cref="Gunslinger.Powers.GunslingerDamagePatch"/>.
/// </summary>
[HarmonyPatch]
internal static class PotionUsePatch
{
    private static MethodInfo? _onUseWrapper;

    private static bool Prepare()
    {
        _onUseWrapper ??= AccessTools.Method(typeof(PotionModel), nameof(PotionModel.OnUseWrapper));

        if (_onUseWrapper == null)
        {
            MainFile.Logger.Info("PotionModel.OnUseWrapper not found; Alchemist potion-use engines (Closed System, " +
                                  "Cork Stopper, Residual Heat, Reactive Mixture) will not fire.");
            return false;
        }

        if (_onUseWrapper.ReturnType != typeof(Task))
        {
            MainFile.Logger.Info($"PotionModel.OnUseWrapper returns {_onUseWrapper.ReturnType}, expected Task; " +
                                  "Alchemist potion-use engines will not fire.");
            return false;
        }

        return true;
    }

    private static MethodBase TargetMethod() => _onUseWrapper!;

    [HarmonyPostfix]
    private static void AfterOnUseWrapper(PotionModel __instance, PlayerChoiceContext choiceContext, ref Task __result)
    {
        __result = RunThenNotify(__result, __instance, choiceContext);
    }

    /// <summary>Await the real potion use first, then notify — never the other way around.</summary>
    private static async Task RunThenNotify(Task original, PotionModel potion, PlayerChoiceContext ctx)
    {
        await original;

        var player = potion.Owner;
        if (player?.Character is not HelloSpire.HelloSpireCode.Characters.Alchemist) return;

        await Belt.OnPotionUsed(ctx, LabContext.From(player), potion);
    }
}
