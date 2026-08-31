using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;

using HelloSpire.HelloSpireCode.Alchemist;
namespace HelloSpire.HelloSpireCode.Alchemist.Lab;

/// <summary>
/// The rules of Gold and the body: Invest, Render, and the per-combat bookkeeping the payoff cards
/// read off.
///
/// The two mechanics live in one file because of the one asymmetry between them that is easy to
/// miss otherwise: **Gold comes back and Max HP does not.** That is also why they are no longer
/// the same shape. Render stays an optional Pay/Decline prompt — Max HP is permanent, so the
/// player must be asked. Invest is mandatory: it pays automatically whenever the player can afford
/// it, with no prompt, since Gold is a recoverable run resource and there is nothing worth
/// interrupting play to ask about.
/// </summary>
public static class Ledger
{
    // ------------------------------------------------------------------ Gold in

    public static int Gold(LabContext lab) => LabBridge.Current.Gold(lab.Player);

    /// <summary>
    /// Real, permanent Gold. Transmute, Liquidate, Heavy Transmute and Gilded Execution.
    ///
    /// Golden Crucible's flat bonus is applied here rather than on each card, so a relic that says
    /// "whenever a card grants you Gold" means exactly that in all six places.
    /// </summary>
    public static async Task GainGold(PlayerChoiceContext ctx, LabContext lab, int amount)
    {
        if (amount <= 0) return;

        amount += AlchemistHooks.GoldBonus(lab, amount);
        await LabBridge.Current.GainGold(lab.Player, amount);

        var bench = await AlchemistEffects.Bench(ctx, lab);
        if (bench != null)
        {
            bench.GoldGainedThisCombat += amount;
            bench.GoldGainedThisTurn += amount;
        }

        await AlchemistHooks.NotifyGoldGained(ctx, lab, amount);
    }

    // ------------------------------------------------------------------ Gold out

    /// <summary>
    /// Invest. Mandatory, not optional: pays automatically and returns true if the player can
    /// afford it, or returns false and spends nothing otherwise. There is no Decline -- Gold is a
    /// recoverable run resource, unlike Render's Max HP, so there is no choice worth asking about.
    ///
    /// The contract every Invest card still depends on: being short of the cost never breaks a
    /// card, it only skips the bonus. An Invest clause is a ceiling raiser, never the reason a card
    /// exists — a card that is dead because the player went shopping is a broken card.
    ///
    /// Gilded Ledger's discount is applied here so it works on all twenty-odd Invest clauses.
    ///
    /// <paramref name="card"/> is the Invest clause's own card, when the caller has one to give --
    /// if something has already cut that card's Energy cost to 0 for this play, the Invest Gold
    /// cost is free too, same as InvestVar already shows on the card. Optional because Ledger has
    /// no other user, but every real call site has a card in hand to pass.
    /// </summary>
    public static async Task<bool> Invest(PlayerChoiceContext ctx, LabContext lab, int cost, CardModel? card = null)
    {
        cost = card != null && card.EnergyCost.GetAmountToSpend() == 0
            ? 0
            : Math.Max(1, cost - AlchemistHooks.InvestDiscount(lab));

        if (!await LabBridge.Current.OfferInvest(ctx, lab.Player, cost)) return false;

        var bench = await AlchemistEffects.Bench(ctx, lab);
        if (bench != null)
        {
            bench.GoldSpentThisCombat += cost;
            bench.GoldSpentThisTurn += cost;
        }

        await AlchemistHooks.NotifyInvested(ctx, lab, cost);
        return true;
    }

    /// <summary>
    /// Invest up to a maximum, a Gold at a time. Black Market Blade and Gilded Guard scale off the
    /// amount actually paid.
    ///
    /// Offered as repeated single-Gold decisions rather than as one variable prompt, because the
    /// bridge only has to know how to ask one question. It costs a few extra prompts and saves a
    /// second choice API.
    /// </summary>
    public static async Task<int> InvestUpTo(PlayerChoiceContext ctx, LabContext lab, int max, CardModel? card = null)
    {
        var paid = 0;
        while (paid < max && await Invest(ctx, lab, 1, card)) paid++;
        return paid;
    }

    // ------------------------------------------------------------------ the body

    /// <summary>
    /// Offer a Render. Returns true only if the player paid.
    ///
    /// Unlike <see cref="Invest"/>, this stays an optional Pay/Decline prompt — never hidden, base
    /// effect always resolves either way — because of the weight difference: **there is no way to
    /// get Max HP back**. See design/alchemist.md, Override 1. There is deliberately no
    /// counterpart to this method; do not add one.
    ///
    /// Four cards in eighty call this. If a fifth ever wants to, that is the signal the mechanic
    /// is drifting from an event into a template.
    /// </summary>
    public static async Task<bool> Render(PlayerChoiceContext ctx, LabContext lab, int cost)
    {
        if (!await LabBridge.Current.OfferRender(ctx, lab.Player, cost)) return false;

        await AlchemistHooks.NotifyRendered(ctx, lab, cost);
        return true;
    }

    // ------------------------------------------------------------------ what the payoffs read

    public static int GainedThisCombat(LabContext lab) => AlchemistEffects.Peek(lab)?.GoldGainedThisCombat ?? 0;

    public static int SpentThisCombat(LabContext lab) => AlchemistEffects.Peek(lab)?.GoldSpentThisCombat ?? 0;

    public static bool GainedThisTurn(LabContext lab) => (AlchemistEffects.Peek(lab)?.GoldGainedThisTurn ?? 0) > 0;

    public static bool SpentThisTurn(LabContext lab) => (AlchemistEffects.Peek(lab)?.GoldSpentThisTurn ?? 0) > 0;
}
