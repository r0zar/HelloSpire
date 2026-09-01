using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Potions;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;

using MegaCrit.Sts2.Core.Models;
namespace HelloSpire.HelloSpireCode.Alchemist;

/// <summary>Reacts to a Potion being Brewed. Ceramic Retort, Thermal Buffer.</summary>
public interface IBrewListener
{
    Task OnBrewed(PlayerChoiceContext ctx, LabContext lab, PotionModel potion);
}

/// <summary>Reacts to a Potion being Distilled. Distillation Mastery, Golden Crucible, Gilded Ledger.</summary>
public interface IDistillListener
{
    Task OnDistilled(PlayerChoiceContext ctx, LabContext lab, PotionModel potion);
}

/// <summary>
/// Reacts to a Potion being used. Cork Stopper, Residual Toxins.
///
/// <paramref name="target"/> is whatever the Potion was used on -- null for a self-only Potion.
/// Threaded through from PotionUsePatch, the only place in the mod this information exists.
/// </summary>
public interface IPotionUseListener
{
    Task OnPotionUsed(PlayerChoiceContext ctx, LabContext lab, PotionModel potion, Creature? target);
}

/// <summary>Reacts to a Potion Slot becoming empty, by use or by Distill. Closed System.</summary>
public interface ISlotEmptiedListener
{
    Task OnSlotEmptied(PlayerChoiceContext ctx, LabContext lab);
}

/// <summary>Reacts to a card being Exhausted. Reagent Press.</summary>
public interface IExhaustListener
{
    Task OnExhausted(PlayerChoiceContext ctx, LabContext lab);
}

/// <summary>Reacts to Unstable Concoction being Infused. Concentrate.</summary>
public interface IInfuseListener
{
    Task OnInfused(PlayerChoiceContext ctx, LabContext lab, decimal amount);
}

/// <summary>Reacts to a Status card being created. Volatile Laboratory.</summary>
public interface IStatusCreatedListener
{
    Task OnStatusCreated(PlayerChoiceContext ctx, LabContext lab);
}

/// <summary>
/// Dispatch for the Alchemist's own listener interfaces.
///
/// Same shape and same reasoning as the Gunslinger's: the base game's hooks cover cards being
/// played and damage being taken, and know nothing about Brewing, Distilling or Exhausting.
/// Rather than have every relic reach into the bench, the bench announces what happened and
/// anything holding the matching interface reacts.
///
/// Re-entrancy is suppressed the same way, and for the same reason — Ceramic Retort reacting to a
/// Brew must not be able to trigger a Brew that re-enters it.
/// </summary>
public static class AlchemistHooks
{
    private static bool _dispatching;

    /// <summary>
    /// Relics, then Hand cards, then powers, all filtered to those implementing
    /// <typeparamref name="T"/>. Hand cards are here so a card can react to board state while it
    /// is still sitting unplayed -- Volatile Compound's live cost reduction is the first user: it
    /// implements IPotionUseListener directly (no Power needed) and sets its own
    /// EnergyCost.SetThisTurnOrUntilPlayed the moment a Potion is used, rather than refunding
    /// Energy after the fact with no visible change to the card.
    /// </summary>
    public static IEnumerable<T> Listeners<T>(LabContext lab) where T : class
    {
        foreach (var relic in lab.Player.Relics)
            if (relic is T listener) yield return listener;

        foreach (var card in LabBridge.Current.Hand(lab.Player))
            if (card is T listener) yield return listener;

        var creature = lab.Player.Creature;
        if (creature == null) yield break;

        foreach (var power in creature.Powers.ToList())
            if (power is T listener) yield return listener;
    }

    public static Task NotifyBrewed(PlayerChoiceContext ctx, LabContext lab, PotionModel potion) =>
        Dispatch<IBrewListener>(lab, listener => listener.OnBrewed(ctx, lab, potion));

    public static Task NotifyDistilled(PlayerChoiceContext ctx, LabContext lab, PotionModel potion) =>
        Dispatch<IDistillListener>(lab, listener => listener.OnDistilled(ctx, lab, potion));

    public static Task NotifyPotionUsed(PlayerChoiceContext ctx, LabContext lab, PotionModel potion, Creature? target) =>
        Dispatch<IPotionUseListener>(lab, listener => listener.OnPotionUsed(ctx, lab, potion, target));

    public static Task NotifySlotEmptied(PlayerChoiceContext ctx, LabContext lab) =>
        Dispatch<ISlotEmptiedListener>(lab, listener => listener.OnSlotEmptied(ctx, lab));

    public static Task NotifyExhausted(PlayerChoiceContext ctx, LabContext lab) =>
        Dispatch<IExhaustListener>(lab, listener => listener.OnExhausted(ctx, lab));

    public static Task NotifyInfused(PlayerChoiceContext ctx, LabContext lab, decimal amount) =>
        Dispatch<IInfuseListener>(lab, listener => listener.OnInfused(ctx, lab, amount));

    public static Task NotifyStatusCreated(PlayerChoiceContext ctx, LabContext lab) =>
        Dispatch<IStatusCreatedListener>(lab, listener => listener.OnStatusCreated(ctx, lab));

    private static async Task Dispatch<T>(LabContext lab, Func<T, Task> notify) where T : class
    {
        if (_dispatching) return;
        _dispatching = true;
        try
        {
            foreach (var listener in Listeners<T>(lab).ToList()) await notify(listener);
        }
        finally
        {
            _dispatching = false;
        }
    }
}
