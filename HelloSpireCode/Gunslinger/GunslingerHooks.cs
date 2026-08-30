using HelloSpire.HelloSpireCode.Gunslinger.Cylinder;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;

namespace HelloSpire.HelloSpireCode.Gunslinger;

/// <summary>
/// Modifies the damage of a Round as it is fired. Implemented by relics (Ivory Handle, True Iron,
/// Engraved Hammer) and powers (Sixth Shot).
/// </summary>
public interface IRoundDamageModifier
{
    /// <summary>Flat damage to add to this Round. Called once per fired Round, before Deadeye.</summary>
    int ModifyRoundDamage(Round round, GunContext gun);
}

/// <summary>Reacts to a Fire resolving, whether it hit or Clicked.</summary>
public interface IFireListener
{
    Task OnFired(PlayerChoiceContext ctx, GunContext gun, FireResult result);
}

/// <summary>Reacts to the hammer being Spun.</summary>
public interface ISpinListener
{
    Task OnSpun(PlayerChoiceContext ctx, GunContext gun);
}

/// <summary>Reacts to a Round being Loaded into a chamber.</summary>
public interface ILoadListener
{
    Task OnLoaded(PlayerChoiceContext ctx, GunContext gun, Round round);
}

/// <summary>Reacts to Weak being applied by the Gunslinger.</summary>
public interface IWeakListener
{
    Task OnWeakApplied(PlayerChoiceContext ctx, GunContext gun, Creature target, int amount);
}

/// <summary>Reacts to Dodge being gained.</summary>
public interface IDodgeListener
{
    Task OnDodgeGained(PlayerChoiceContext ctx, GunContext gun, int amount);
}

/// <summary>Reacts to Armor absorbing part of a hit. Fired from the damage patch.</summary>
public interface IArmorListener
{
    void OnArmorPrevented(Creature owner);
}

/// <summary>Reacts to Armor being gained. Distinct from <see cref="IArmorListener"/>, which fires when Armor spends itself.</summary>
public interface IArmorGainListener
{
    Task OnArmorGained(PlayerChoiceContext ctx, GunContext gun, int amount);
}

/// <summary>Reacts to the cylinder running completely dry.</summary>
public interface ICylinderEmptiedListener
{
    Task OnCylinderEmptied(PlayerChoiceContext ctx, GunContext gun);
}

/// <summary>
/// Dispatch for the mod's own listener interfaces.
///
/// The base game's hook system covers cards being played and damage being taken; it knows nothing
/// about Loading, Firing or Spinning. Rather than have each relic reach into the cylinder, the
/// cylinder announces what happened and any relic or power holding the matching interface reacts.
///
/// Notifications are suppressed while one is already being delivered. Oiled Rag reacts to a Load by
/// Loading, which would otherwise re-enter forever.
/// </summary>
public static class GunslingerHooks
{
    private static bool _dispatching;

    /// <summary>Relics then powers, both filtered to those implementing <typeparamref name="T"/>.</summary>
    public static IEnumerable<T> Listeners<T>(GunContext gun) where T : class
    {
        foreach (var relic in gun.Player.Relics)
            if (relic is T listener) yield return listener;

        var creature = gun.Player.Creature;
        if (creature == null) yield break;

        foreach (var power in creature.Powers.ToList())
            if (power is T listener) yield return listener;
    }

    /// <summary>Total flat bonus every relic and power wants to add to this Round.</summary>
    public static int RoundDamageBonus(Round round, GunContext gun)
    {
        return Listeners<IRoundDamageModifier>(gun).Sum(modifier => modifier.ModifyRoundDamage(round, gun));
    }

    public static Task NotifyFired(PlayerChoiceContext ctx, GunContext gun, FireResult result) =>
        Dispatch<IFireListener>(gun, listener => listener.OnFired(ctx, gun, result));

    public static Task NotifySpun(PlayerChoiceContext ctx, GunContext gun) =>
        Dispatch<ISpinListener>(gun, listener => listener.OnSpun(ctx, gun));

    public static Task NotifyLoaded(PlayerChoiceContext ctx, GunContext gun, Round round) =>
        Dispatch<ILoadListener>(gun, listener => listener.OnLoaded(ctx, gun, round));

    public static Task NotifyWeakApplied(PlayerChoiceContext ctx, GunContext gun, Creature target, int amount) =>
        Dispatch<IWeakListener>(gun, listener => listener.OnWeakApplied(ctx, gun, target, amount));

    public static Task NotifyDodgeGained(PlayerChoiceContext ctx, GunContext gun, int amount) =>
        Dispatch<IDodgeListener>(gun, listener => listener.OnDodgeGained(ctx, gun, amount));

    public static Task NotifyArmorGained(PlayerChoiceContext ctx, GunContext gun, int amount) =>
        Dispatch<IArmorGainListener>(gun, listener => listener.OnArmorGained(ctx, gun, amount));

    public static Task NotifyCylinderEmptied(PlayerChoiceContext ctx, GunContext gun) =>
        Dispatch<ICylinderEmptiedListener>(gun, listener => listener.OnCylinderEmptied(ctx, gun));

    /// <summary>
    /// Synchronous, and called from inside a Harmony patch on the damage pipeline — so it takes a
    /// creature rather than a <see cref="GunContext"/> and never awaits anything.
    /// </summary>
    public static void NotifyArmorPrevented(Creature owner)
    {
        foreach (var power in owner.Powers.ToList())
            if (power is IArmorListener listener) listener.OnArmorPrevented(owner);

        var player = GunslingerEffects.PlayerFor(owner);
        if (player == null) return;

        foreach (var relic in player.Relics)
            if (relic is IArmorListener listener) listener.OnArmorPrevented(owner);
    }

    private static async Task Dispatch<T>(GunContext gun, Func<T, Task> notify) where T : class
    {
        if (_dispatching) return;
        _dispatching = true;
        try
        {
            foreach (var listener in Listeners<T>(gun).ToList()) await notify(listener);
        }
        finally
        {
            _dispatching = false;
        }
    }
}
