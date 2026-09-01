using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;

using HelloSpire.HelloSpireCode.Alchemist;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Nodes.CommonUi;
namespace HelloSpire.HelloSpireCode.Alchemist.Lab;

/// <summary>
/// Hand manipulation: Exhaust, discard, bottom, create, Upgrade.
///
/// Half the Alchemist's card set says "Exhaust another card in your Hand" and then does something
/// with the fact. This is where "another" is defined once — never the card doing the Exhausting,
/// which is the bug every one of those cards would otherwise have its own copy of.
///
/// The Exhaust counter the payoff cards read is incremented here too, so it counts Exhausts by any
/// route rather than only the ones a card noticed.
/// </summary>
public static class Alchemy
{
    /// <summary>Cards in Hand other than <paramref name="except"/>.</summary>
    public static IReadOnlyList<CardModel> OtherCardsInHand(LabContext lab, CardModel? except = null)
    {
        except ??= lab.Card;
        return LabBridge.Current.Hand(lab.Player).Where(card => card != except).ToList();
    }

    /// <summary>
    /// Exhaust one other card from Hand, chosen by the player. False when the Hand is otherwise
    /// empty or the player declined — every caller treats that as "the optional half did not
    /// happen", never as an error.
    /// </summary>
    public static async Task<bool> ExhaustOne(PlayerChoiceContext ctx, LabContext lab)
    {
        var candidates = OtherCardsInHand(lab);
        if (candidates.Count == 0) return false;

        var chosen = await LabBridge.Current.ChooseCard(ctx, lab.Player, candidates, lab.Card);
        if (chosen == null) return false;

        await Exhaust(ctx, lab, chosen);
        return true;
    }

    /// <summary>
    /// Exhaust one named card. CardsExhaustedThisTurn and NotifyExhausted happen automatically
    /// from here -- see LabPower.AfterCardExhausted, a real base-game hook that fires for every
    /// Exhaust in combat, including cards that Exhaust themselves (Aegis Formula, Pocket Formula,
    /// Stabilize, Pressure Burst, ...) and never call this method at all. Ensuring the bench
    /// exists first matters: that hook can only dispatch to a LabPower already attached, not
    /// create one on demand.
    /// </summary>
    public static async Task Exhaust(PlayerChoiceContext ctx, LabContext lab, CardModel card)
    {
        await AlchemistEffects.Bench(ctx, lab);
        await LabBridge.Current.Exhaust(ctx, lab.Player, card);
    }

    /// <summary>Exhaust up to <paramref name="max"/> other cards. Returns how many actually went.</summary>
    public static async Task<int> ExhaustUpTo(PlayerChoiceContext ctx, LabContext lab, int max)
    {
        var count = 0;
        while (count < max && await ExhaustOne(ctx, lab)) count++;
        return count;
    }

    /// <summary>Exhaust every other card in Hand. Heavy Transmute. Returns what went.</summary>
    public static async Task<IReadOnlyList<CardModel>> ExhaustAllOther(PlayerChoiceContext ctx, LabContext lab)
    {
        var cards = OtherCardsInHand(lab);
        foreach (var card in cards) await Exhaust(ctx, lab, card);
        return cards;
    }

    /// <summary>Exhaust one other card at random. Mercury Lance's cost — the player does not choose.</summary>
    public static async Task<CardModel?> ExhaustRandomOther(PlayerChoiceContext ctx, LabContext lab)
    {
        var candidates = OtherCardsInHand(lab);
        if (candidates.Count == 0) return null;

        // NextInt's upper bound is exclusive; Count - 1 here would make the last candidate
        // unreachable.
        var index = lab.Player.RunState.Rng.CombatTargets.NextInt(0, candidates.Count);
        var chosen = candidates[Math.Clamp(index, 0, candidates.Count - 1)];

        await Exhaust(ctx, lab, chosen);
        return chosen;
    }

    /// <summary>Exhaust a Status or non-Eternal Curse. Smelt the Weak only wants the junk.</summary>
    public static async Task<bool> ExhaustJunk(PlayerChoiceContext ctx, LabContext lab)
    {
        var junk = OtherCardsInHand(lab)
            .Where(card => card.Type == CardType.Status ||
                           (card.Type == CardType.Curse && !card.Keywords.Contains(CardKeyword.Eternal)))
            .ToList();

        if (junk.Count == 0) return false;

        var chosen = junk.Count == 1 ? junk[0] : await LabBridge.Current.ChooseCard(ctx, lab.Player, junk, lab.Card);
        if (chosen == null) return false;

        await Exhaust(ctx, lab, chosen);
        return true;
    }

    public static async Task<bool> DiscardOne(PlayerChoiceContext ctx, LabContext lab)
    {
        var candidates = OtherCardsInHand(lab);
        if (candidates.Count == 0) return false;

        var chosen = await LabBridge.Current.ChooseCard(ctx, lab.Player, candidates, lab.Card);
        if (chosen == null) return false;

        await LabBridge.Current.DiscardCard(ctx, lab.Player, chosen);
        return true;
    }

    public static async Task<bool> BottomOne(PlayerChoiceContext ctx, LabContext lab)
    {
        var candidates = OtherCardsInHand(lab);
        if (candidates.Count == 0) return false;

        var chosen = await LabBridge.Current.ChooseCard(ctx, lab.Player, candidates, lab.Card);
        if (chosen == null) return false;

        await LabBridge.Current.BottomOfDraw(ctx, lab.Player, chosen);
        return true;
    }

    /// <summary>
    /// Create a card into Hand and count it.
    ///
    /// Refiner's Eye upgrades whatever comes out of here, which is why creation goes through one
    /// function rather than each card calling the bridge.
    /// </summary>
    public static async Task Create(PlayerChoiceContext ctx, LabContext lab, CardModel? card, bool freeThisTurn = true)
    {
        if (card == null) return;

        if (AlchemistEffects.Peek(lab)?.Owner.GetPower<Powers.RefinersEyePower>() != null)
            CardCmd.Upgrade(card, CardPreviewStyle.None);

        await LabBridge.Current.CreateInHand(ctx, lab.Player, card, freeThisTurn);

        var bench = await AlchemistEffects.Bench(ctx, lab);
        if (bench != null) bench.CardsCreatedThisTurn++;
    }

    /// <summary>
    /// Permanently add a card to the deck, not just this combat's Hand -- a real, lasting card,
    /// same weight class as UpgradeOnePermanently. Homunculus Pact.
    /// </summary>
    public static async Task CreatePermanently(LabContext lab, CardModel card) =>
        await LabBridge.Current.CreatePermanently(lab.Player, card);

    /// <summary>Let the player pick a card in Hand and Upgrade it for this combat.</summary>
    public static async Task<bool> UpgradeOneForCombat(PlayerChoiceContext ctx, LabContext lab)
    {
        var candidates = OtherCardsInHand(lab);
        if (candidates.Count == 0) return false;

        var chosen = await LabBridge.Current.ChooseCard(ctx, lab.Player, candidates, lab.Card);
        if (chosen == null) return false;

        await LabBridge.Current.UpgradeForCombat(ctx, lab.Player, chosen);
        return true;
    }

    public static async Task UpgradeHandForCombat(PlayerChoiceContext ctx, LabContext lab)
    {
        foreach (var card in OtherCardsInHand(lab))
            await LabBridge.Current.UpgradeForCombat(ctx, lab.Player, card);
    }

    /// <summary>
    /// Permanently Upgrade one card in Hand, for the rest of the run. Masterwork and Transmute
    /// Flesh — the two most expensive cards in the class, in the two currencies that do not
    /// come back on their own.
    /// </summary>
    public static async Task<bool> UpgradeOnePermanently(PlayerChoiceContext ctx, LabContext lab)
    {
        var candidates = OtherCardsInHand(lab).Where(card => !card.IsUpgraded).ToList();
        if (candidates.Count == 0) return false;

        var chosen = await LabBridge.Current.ChooseCard(ctx, lab.Player, candidates, lab.Card);
        if (chosen == null) return false;

        await LabBridge.Current.UpgradePermanently(lab.Player, chosen);
        return true;
    }
}
