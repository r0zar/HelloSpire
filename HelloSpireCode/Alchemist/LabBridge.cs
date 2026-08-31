using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Potions;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;

namespace HelloSpire.HelloSpireCode.Alchemist;

/// <summary>The base-game Potions the Alchemist Brews by name rather than at random.</summary>
public enum BasePotion
{
    /// <summary>Gain 12 Block. Aegis Formula.</summary>
    Block,

    /// <summary>Deal 10 damage to ALL enemies. Pyric Formula.</summary>
    ExplosiveAmpoule,

    /// <summary>Gain 2 Energy.</summary>
    Energy
}

/// <summary>
/// The quarantine.
///
/// The Alchemist needs four things from the base game that this repository has no working example
/// of: reading and spending **Gold**, writing to and reading the **Potion belt**, reducing
/// **Max HP**, and putting a **choice** in front of the player. The Gunslinger needed none of them
/// — its cylinder is a custom Power and it deliberately makes its choices from board state rather
/// than from a menu (see PerfectReload) — so there is no precedent in this codebase to copy, and
/// the exact signatures have not been read out of sts2.dll yet.
///
/// Rather than scatter guesses across ninety cards, every one of those operations is declared here
/// and nowhere else. The default implementation below is deliberately inert: the mod compiles,
/// loads and plays, the Alchemist's damage/Block/draw/Power effects all work, and the four
/// unverified systems no-op with a log line instead of doing something wrong.
///
/// **To finish the character, implement <see cref="ILabBridge"/> against the real API and assign
/// <see cref="Current"/> during mod initialization.** Nothing else needs to change.
///
/// Two of the defaults are load-bearing safety decisions, not placeholders to rush past:
///
/// - <see cref="OfferInvest"/> and <see cref="OfferRender"/> both return **false** — Decline.
///   Gold is a persistent run resource and Max HP is permanent; auto-paying either one without
///   asking would spend the player's run behind their back. Every Invest and Render card is
///   specified to resolve its base effect in full on a Decline, so declining is always correct
///   and never leaves a card dead. A card that is weaker than designed is a bug; a card that
///   quietly empties the player's purse is a much worse one.
/// - <see cref="Brew"/> returning null is the same state as a full belt, which the design already
///   defines: the card resolves, the Potion is lost.
///
/// TODO(Phase 3): read the Potion, Gold, Max HP and player-choice APIs out of sts2.dll and ship a
/// real implementation. Until then the Alchemist is a playable but incomplete character, and that
/// is stated on the tin rather than hidden.
/// </summary>
public interface ILabBridge
{
    // -------------------------------------------------------------------------- Gold

    /// <summary>The player's actual, persistent Gold.</summary>
    int Gold(Player player);

    /// <summary>Add real Gold to the run total. Transmute and friends.</summary>
    Task GainGold(Player player, int amount);

    /// <summary>
    /// Ask the player to Invest. Pay removes the Gold and returns true; Decline returns false.
    /// Must return false without prompting when the player holds less than <paramref name="cost"/>.
    /// </summary>
    Task<bool> OfferInvest(PlayerChoiceContext ctx, Player player, int cost);

    // -------------------------------------------------------------------------- Max HP

    /// <summary>
    /// Ask the player to Render. Pay reduces Max HP (and current HP) by <paramref name="cost"/>
    /// and returns true. Must return false without prompting when paying would take Max HP to
    /// zero or below.
    ///
    /// There is no counterpart to this. Render is one-way by design — see design/alchemist.md,
    /// Override 1. Do not add a GainMaxHp here.
    /// </summary>
    Task<bool> OfferRender(PlayerChoiceContext ctx, Player player, int cost);

    // -------------------------------------------------------------------------- the belt

    /// <summary>Potions the player is currently holding, in slot order.</summary>
    IReadOnlyList<PotionModel> Held(Player player);

    /// <summary>Total Potion Slots, before this combat's temporary ones.</summary>
    int SlotCount(Player player);

    /// <summary>
    /// Put a Potion in the first empty slot. Returns the Potion placed, or null if the belt was
    /// full — which is not an error, it is the documented Brew-failure behaviour.
    /// </summary>
    Task<PotionModel?> Brew(PlayerChoiceContext ctx, Player player, PotionModel potion);

    /// <summary>Remove a held Potion without resolving it. Distill's actual effect.</summary>
    Task Discard(PlayerChoiceContext ctx, Player player, PotionModel potion);

    /// <summary>A random Potion from the curated Combat Potion pool — see design/alchemist.md.</summary>
    PotionModel? RandomCombatPotion(Player player, PotionRarity? rarity = null);

    /// <summary>
    /// One of the handful of base-game Potions the Alchemist Brews by name.
    ///
    /// An enum rather than a <c>Type</c> so that no card file has to reference a base-game potion
    /// class directly — the whole point of this interface is that the unverified names live in one
    /// place. Adding a named Potion means adding an enum value here, not a using directive there.
    /// </summary>
    PotionModel? NamedPotion(BasePotion which);

    // -------------------------------------------------------------------------- choices

    /// <summary>Ask the player to pick one held Potion, or null if they hold none.</summary>
    Task<PotionModel?> ChoosePotion(PlayerChoiceContext ctx, Player player, IReadOnlyList<PotionModel> from);

    /// <summary>
    /// Ask the player to pick one card from a set, typically their Hand.
    /// </summary>
    /// <param name="source">
    /// The card, potion or relic that kicked off this selection (e.g. the Transmute being played).
    /// The real hand-selection screen requires a non-null source internally; pass the card whose
    /// effect is asking, never null.
    /// </param>
    Task<CardModel?> ChooseCard(PlayerChoiceContext ctx, Player player, IReadOnlyList<CardModel> from, CardModel? source);

    // -------------------------------------------------------------------------- hand and piles

    /// <summary>The player's current Hand, in order.</summary>
    IReadOnlyList<CardModel> Hand(Player player);

    /// <summary>Exhaust a specific card from Hand.</summary>
    Task Exhaust(PlayerChoiceContext ctx, Player player, CardModel card);

    /// <summary>Discard a specific card from Hand. Market Sense.</summary>
    Task DiscardCard(PlayerChoiceContext ctx, Player player, CardModel card);

    /// <summary>Put a card from Hand on the bottom of the Draw Pile. False Bottom.</summary>
    Task BottomOfDraw(PlayerChoiceContext ctx, Player player, CardModel card);

    /// <summary>A random card from a pool, for the creation cards. Null when the pool is unavailable.</summary>
    CardModel? RandomCard(Player player, CardRarity? rarity = null, CardType? type = null);

    /// <summary>Create a card into Hand. Commission, Homunculus Pact and the creation engines.</summary>
    Task CreateInHand(PlayerChoiceContext ctx, Player player, CardModel card, bool costsZeroThisTurn);

    /// <summary>Upgrade a card for this combat only.</summary>
    Task UpgradeForCombat(PlayerChoiceContext ctx, Player player, CardModel card);

    /// <summary>Upgrade a card permanently, for the rest of the run. Masterwork and Transmute Flesh.</summary>
    Task UpgradePermanently(Player player, CardModel card);
}

/// <summary>
/// Inert default. Every method is a no-op that logs once, so an unimplemented bridge shows up in
/// the log as a specific missing feature rather than as a card that silently does nothing.
/// </summary>
public sealed class UnwiredLabBridge : ILabBridge
{
    private readonly HashSet<string> _reported = [];

    private void Report(string what)
    {
        if (!_reported.Add(what)) return;
        MainFile.Logger.Info($"[Alchemist] {what} is not wired up yet; see LabBridge. Effect skipped.");
    }

    public int Gold(Player player)
    {
        Report("reading Gold");
        return 0;
    }

    public Task GainGold(Player player, int amount)
    {
        Report("gaining Gold");
        return Task.CompletedTask;
    }

    public Task<bool> OfferInvest(PlayerChoiceContext ctx, Player player, int cost)
    {
        Report("Invest");
        return Task.FromResult(false); // Decline. Never spend the player's Gold unasked.
    }

    public Task<bool> OfferRender(PlayerChoiceContext ctx, Player player, int cost)
    {
        Report("Render");
        return Task.FromResult(false); // Decline. Max HP is permanent; never take it unasked.
    }

    public IReadOnlyList<PotionModel> Held(Player player)
    {
        Report("reading the Potion belt");
        return [];
    }

    public int SlotCount(Player player)
    {
        Report("reading Potion Slot count");
        return 0;
    }

    public Task<PotionModel?> Brew(PlayerChoiceContext ctx, Player player, PotionModel potion)
    {
        Report("Brew");
        return Task.FromResult<PotionModel?>(null); // Same as a full belt: the Potion is lost.
    }

    public Task Discard(PlayerChoiceContext ctx, Player player, PotionModel potion)
    {
        Report("Distill");
        return Task.CompletedTask;
    }

    public PotionModel? RandomCombatPotion(Player player, PotionRarity? rarity = null)
    {
        Report("the Combat Potion pool");
        return null;
    }

    public PotionModel? NamedPotion(BasePotion which)
    {
        Report($"Brewing a {which} Potion by name");
        return null;
    }

    public Task<PotionModel?> ChoosePotion(PlayerChoiceContext ctx, Player player, IReadOnlyList<PotionModel> from)
    {
        Report("choosing a Potion");
        return Task.FromResult<PotionModel?>(null);
    }

    public Task<CardModel?> ChooseCard(PlayerChoiceContext ctx, Player player, IReadOnlyList<CardModel> from, CardModel? source)
    {
        Report("choosing a card");
        return Task.FromResult<CardModel?>(null);
    }

    public IReadOnlyList<CardModel> Hand(Player player)
    {
        Report("reading the Hand");
        return [];
    }

    public Task Exhaust(PlayerChoiceContext ctx, Player player, CardModel card)
    {
        Report("Exhausting a chosen card");
        return Task.CompletedTask;
    }

    public Task DiscardCard(PlayerChoiceContext ctx, Player player, CardModel card)
    {
        Report("discarding a chosen card");
        return Task.CompletedTask;
    }

    public Task BottomOfDraw(PlayerChoiceContext ctx, Player player, CardModel card)
    {
        Report("bottoming a card");
        return Task.CompletedTask;
    }

    public CardModel? RandomCard(Player player, CardRarity? rarity = null, CardType? type = null)
    {
        Report("picking a random card");
        return null;
    }

    public Task CreateInHand(PlayerChoiceContext ctx, Player player, CardModel card, bool costsZeroThisTurn)
    {
        Report("creating a card in Hand");
        return Task.CompletedTask;
    }

    public Task UpgradeForCombat(PlayerChoiceContext ctx, Player player, CardModel card)
    {
        Report("Upgrading for this combat");
        return Task.CompletedTask;
    }

    public Task UpgradePermanently(Player player, CardModel card)
    {
        Report("Upgrading permanently");
        return Task.CompletedTask;
    }
}

/// <summary>Where the rest of the character looks the bridge up.</summary>
public static class LabBridge
{
    /// <summary>Swap this during mod initialization once a real implementation exists.</summary>
    public static ILabBridge Current { get; set; } = new UnwiredLabBridge();
}
