using MegaCrit.Sts2.Core.Entities.Potions;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;

using MegaCrit.Sts2.Core.Models;
namespace HelloSpire.HelloSpireCode.Alchemist;

/// <summary>What kind of value a Transform produced. Alchemist's Ledger does not care which.</summary>
public enum TransformVector
{
    CardToGold,
    CardToPotion,
    PotionToTempo,
    GoldToPotion,
    GoldToCard,
    GoldToUpgrade,
    MaxHpToCard,
    MaxHpToPotion,
    MaxHpToUpgrade
}

/// <summary>Reacts to a Potion being Brewed. Ceramic Retort, Heat Bath.</summary>
public interface IBrewListener
{
    Task OnBrewed(PlayerChoiceContext ctx, LabContext lab, PotionModel potion);
}

/// <summary>Reacts to a Potion being Distilled. Distillation Mastery.</summary>
public interface IDistillListener
{
    Task OnDistilled(PlayerChoiceContext ctx, LabContext lab, PotionModel potion);
}

/// <summary>Reacts to a Potion being used. Cork Stopper, Residual Heat, Reactive Mixture.</summary>
public interface IPotionUseListener
{
    Task OnPotionUsed(PlayerChoiceContext ctx, LabContext lab, PotionModel potion);
}

/// <summary>Reacts to a Potion Slot becoming empty, by use or by Distill. Closed System.</summary>
public interface ISlotEmptiedListener
{
    Task OnSlotEmptied(PlayerChoiceContext ctx, LabContext lab);
}

/// <summary>Reacts to a card being Exhausted. Coin Press, Conservation of Matter.</summary>
public interface IExhaustListener
{
    Task OnExhausted(PlayerChoiceContext ctx, LabContext lab);
}

/// <summary>Reacts to Gold arriving during combat. Assayer's Lens, Golden Engine.</summary>
public interface IGoldListener
{
    Task OnGoldGained(PlayerChoiceContext ctx, LabContext lab, int amount);
}

/// <summary>Adds flat Gold to every in-combat Gold gain. Golden Crucible.</summary>
public interface IGoldModifier
{
    int ModifyGoldGain(LabContext lab, int amount);
}

/// <summary>Reduces the cost of every Invest clause. Gilded Ledger.</summary>
public interface IInvestDiscount
{
    int DiscountInvest(LabContext lab);
}

/// <summary>Reacts to an Invest being paid. Merchant's Instinct, Glass Homunculus.</summary>
public interface IInvestListener
{
    Task OnInvested(PlayerChoiceContext ctx, LabContext lab, int cost);
}

/// <summary>Reacts to Max HP being spent. Sanguine Circuit — which pays in cards, never in HP.</summary>
public interface IRenderListener
{
    Task OnRendered(PlayerChoiceContext ctx, LabContext lab, int cost);
}

/// <summary>Reacts to any Transform, whichever vector it used. Alchemist's Ledger.</summary>
public interface ITransformListener
{
    Task OnTransformed(PlayerChoiceContext ctx, LabContext lab, TransformVector vector);
}

/// <summary>
/// Dispatch for the Alchemist's own listener interfaces.
///
/// Same shape and same reasoning as the Gunslinger's: the base game's hooks cover cards being
/// played and damage being taken, and know nothing about Brewing, Distilling, Investing or
/// Rendering. Rather than have every relic reach into the bench, the bench announces what happened
/// and anything holding the matching interface reacts.
///
/// Re-entrancy is suppressed the same way, and for the same reason — Ceramic Retort reacting to a
/// Brew must not be able to trigger a Brew that re-enters it.
/// </summary>
public static class AlchemistHooks
{
    private static bool _dispatching;

    /// <summary>Relics then powers, both filtered to those implementing <typeparamref name="T"/>.</summary>
    public static IEnumerable<T> Listeners<T>(LabContext lab) where T : class
    {
        foreach (var relic in lab.Player.Relics)
            if (relic is T listener) yield return listener;

        var creature = lab.Player.Creature;
        if (creature == null) yield break;

        foreach (var power in creature.Powers.ToList())
            if (power is T listener) yield return listener;
    }

    /// <summary>Total flat bonus every relic and power wants to add to a Gold gain.</summary>
    public static int GoldBonus(LabContext lab, int amount) =>
        Listeners<IGoldModifier>(lab).Sum(modifier => modifier.ModifyGoldGain(lab, amount));

    /// <summary>Total discount on an Invest cost. Applied before the minimum of 1.</summary>
    public static int InvestDiscount(LabContext lab) =>
        Listeners<IInvestDiscount>(lab).Sum(discount => discount.DiscountInvest(lab));

    public static Task NotifyBrewed(PlayerChoiceContext ctx, LabContext lab, PotionModel potion) =>
        Dispatch<IBrewListener>(lab, listener => listener.OnBrewed(ctx, lab, potion));

    public static Task NotifyDistilled(PlayerChoiceContext ctx, LabContext lab, PotionModel potion) =>
        Dispatch<IDistillListener>(lab, listener => listener.OnDistilled(ctx, lab, potion));

    public static Task NotifyPotionUsed(PlayerChoiceContext ctx, LabContext lab, PotionModel potion) =>
        Dispatch<IPotionUseListener>(lab, listener => listener.OnPotionUsed(ctx, lab, potion));

    public static Task NotifySlotEmptied(PlayerChoiceContext ctx, LabContext lab) =>
        Dispatch<ISlotEmptiedListener>(lab, listener => listener.OnSlotEmptied(ctx, lab));

    public static Task NotifyExhausted(PlayerChoiceContext ctx, LabContext lab) =>
        Dispatch<IExhaustListener>(lab, listener => listener.OnExhausted(ctx, lab));

    public static Task NotifyGoldGained(PlayerChoiceContext ctx, LabContext lab, int amount) =>
        Dispatch<IGoldListener>(lab, listener => listener.OnGoldGained(ctx, lab, amount));

    public static Task NotifyInvested(PlayerChoiceContext ctx, LabContext lab, int cost) =>
        Dispatch<IInvestListener>(lab, listener => listener.OnInvested(ctx, lab, cost));

    public static Task NotifyRendered(PlayerChoiceContext ctx, LabContext lab, int cost) =>
        Dispatch<IRenderListener>(lab, listener => listener.OnRendered(ctx, lab, cost));

    /// <summary>
    /// Announce a Transform. Called by the cards that actually convert one kind of value into
    /// another, per the keyword's definition in design/alchemist.md — spending Energy to play a
    /// card is never, on its own, a Transform.
    /// </summary>
    public static Task NotifyTransformed(PlayerChoiceContext ctx, LabContext lab, TransformVector vector) =>
        Dispatch<ITransformListener>(lab, listener => listener.OnTransformed(ctx, lab, vector));

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
