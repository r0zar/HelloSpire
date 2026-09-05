using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Potions;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;

namespace HelloSpire.HelloSpireCode.Alchemist;

/// <summary>The base-game Potions the Alchemist Brews by name rather than at random.</summary>
public enum BasePotion
{
    /// <summary>Gain 12 Block. Aegis Formula.</summary>
    Block,

    /// <summary>Deal 10 damage to ALL enemies. Flash Powder.</summary>
    ExplosiveAmpoule,

    /// <summary>Gain 2 Energy.</summary>
    Energy,

    /// <summary>Apply 2 Vulnerable. Cinnabar Edge.</summary>
    Vulnerable,

    /// <summary>Apply 2 Weak. Copper Shot.</summary>
    Weak,

    /// <summary>Gain 3 Dexterity this turn. Quick Silver.</summary>
    Speed,

    /// <summary>Choose 1 of 2 Attack cards, free this turn. Flask Toss.</summary>
    Attack,

    /// <summary>Deal 12 damage. Firebrand.</summary>
    Fire,

    /// <summary>Gain 1 Strength. Steady Pour.</summary>
    Strength,

    /// <summary>Apply 4 Poison. Venomous Ampoule, Reactive Mixture.</summary>
    Poison,

    /// <summary>Gain 1 Dexterity. Steady Pour.</summary>
    Dexterity,

    /// <summary>Apply 3 Poison to ALL enemies. VolatilePoisonAmpoule -- not offered anywhere; a
    /// future card naming it here would be the only way to Brew one.</summary>
    PoisonAmpoule
}

/// <summary>
/// The quarantine.
///
/// The Alchemist needs things from the base game that this repository has no working example of:
/// writing to and reading the **Potion belt**, and putting a **choice** in front of the player.
/// The Gunslinger needed neither — its cylinder is a custom Power and it deliberately makes its
/// choices from board state rather than from a menu (see PerfectReload) — so there is no precedent
/// in this codebase to copy.
///
/// Rather than scatter guesses across eighty cards, every one of those operations is declared here
/// and nowhere else. The default implementation below is deliberately inert: the mod compiles,
/// loads and plays, the Alchemist's damage/Block/draw/Power effects all work, and the belt/choice
/// operations no-op with a log line instead of doing something wrong.
///
/// **To finish the character, implement <see cref="ILabBridge"/> against the real API and assign
/// <see cref="Current"/> during mod initialization.** Nothing else needs to change.
///
/// <see cref="Brew"/> returning null is the same state as a full belt, which the design already
/// defines: the card resolves, the Potion is lost.
/// </summary>
public interface ILabBridge
{
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

    /// <summary>The belt's ordinary "throw this Potion away" command -- what the generic discard button calls.</summary>
    Task Discard(PlayerChoiceContext ctx, Player player, PotionModel potion);

    /// <summary>A random Potion from the curated Combat Potion pool — see design/alchemist.md.</summary>
    PotionModel? RandomCombatPotion(Player player, PotionRarity? rarity = null);

    /// <summary>
    /// Up to <paramref name="count"/> DISTINCT random Potions from the curated Combat Potion pool.
    /// Drawn from the synced RNG, so every client computes the same list -- only a chosen index
    /// ever needs to cross the network.
    /// </summary>
    IReadOnlyList<PotionModel> CombatPotionOptions(Player player, int count, PotionRarity? rarity = null);

    /// <summary>
    /// A random Potion from the Draw pool -- the four Volatile Potions that hand you a free card
    /// (Attack/Colorless/Power/Skill) -- kept separate from the Combat Potion pool above and never
    /// offered by it. Spare Flask.
    /// </summary>
    PotionModel? RandomDrawPotion(Player player);

    /// <summary>
    /// One of the handful of base-game Potions the Alchemist Brews by name.
    ///
    /// An enum rather than a <c>Type</c> so that no card file has to reference a base-game potion
    /// class directly — the whole point of this interface is that the unverified names live in one
    /// place. Adding a named Potion means adding an enum value here, not a using directive there.
    /// </summary>
    PotionModel? NamedPotion(BasePotion which);

    // -------------------------------------------------------------------------- choices

    /// <summary>Grow the player's real Potion Slots. Vial Bandolier, Extra Vial, Widen the Belt.</summary>
    Task GainSlots(Player player, int count);

    /// <summary>Shrink the player's real Potion Slots. The combat-end half of GainSlots.</summary>
    Task LoseSlots(Player player, int count);

    /// <summary>Ask the player to pick one of the offered Potions. Buy Ingredients.</summary>
    Task<PotionModel?> ChoosePotionOption(PlayerChoiceContext ctx, Player player, IReadOnlyList<PotionModel> options);

    /// <summary>Ask the player to pick one held Potion, or null if they hold none.</summary>
    /// <param name="prompt">
    /// The popup's title. Defaults to the generic "Choose a Potion to Brew" wording, which is
    /// wrong for anything that isn't Brewing (Distill, Stabilize, Pressure Burst) -- pass a real
    /// one for those.
    /// </param>
    /// <param name="allowStop">
    /// Offer a "Done" option that returns null even though Potions are still held. Grand
    /// Combustion's "Distill any number" reads this loop-by-loop; every other caller wants a
    /// mandatory pick once it has committed to asking; leave this false for those.
    /// </param>
    Task<PotionModel?> ChoosePotion(PlayerChoiceContext ctx, Player player, IReadOnlyList<PotionModel> from, LocString? prompt = null, bool allowStop = false);

    /// <summary>
    /// Ask the player to pick one card from a set, typically their Hand.
    /// </summary>
    /// <param name="source">
    /// The card, potion or relic that kicked off this selection (e.g. the Transmute being played).
    /// The real hand-selection screen requires a non-null source internally; pass the card whose
    /// effect is asking, never null.
    /// </param>
    Task<CardModel?> ChooseCard(PlayerChoiceContext ctx, Player player, IReadOnlyList<CardModel> from, CardModel? source, LocString? prompt = null);

    // -------------------------------------------------------------------------- hand and piles

    /// <summary>The player's current Hand, in order.</summary>
    IReadOnlyList<CardModel> Hand(Player player);

    /// <summary>The player's current Discard pile, in order.</summary>
    IReadOnlyList<CardModel> DiscardPile(Player player);

    /// <summary>The player's current Exhaust pile, in order. Reconstitute reads it.</summary>
    IReadOnlyList<CardModel> ExhaustPile(Player player);

    /// <summary>Exhaust a specific card from Hand.</summary>
    Task Exhaust(PlayerChoiceContext ctx, Player player, CardModel card);

    /// <summary>Discard a specific card from Hand. Market Sense.</summary>
    Task DiscardCard(PlayerChoiceContext ctx, Player player, CardModel card);

    /// <summary>Put a card from Hand on the bottom of the Draw Pile. False Bottom.</summary>
    Task BottomOfDraw(PlayerChoiceContext ctx, Player player, CardModel card);

    /// <summary>Move a card from anywhere -- Draw, Discard or Exhaust -- into Hand. Reconstitute.</summary>
    Task ReturnToHand(PlayerChoiceContext ctx, Player player, CardModel card);

    /// <summary>A random card from a pool, for the creation cards. Null when the pool is unavailable.</summary>
    CardModel? RandomCard(Player player, CardRarity? rarity = null, CardType? type = null);

    /// <summary>Create a card into Hand. Homunculus Assault and the other creation engines.</summary>
    Task CreateInHand(PlayerChoiceContext ctx, Player player, CardModel card, bool costsZeroThisTurn);

    /// <summary>
    /// Create a fresh instance of a canonical card (typically a Status, e.g. Volatile Reagent) and
    /// add it directly to a pile other than Hand. Separate from <see cref="CreateInHand"/> because
    /// that method takes an already-instantiated card; this one instantiates it too, the same way
    /// <see cref="RandomCard"/> does internally.
    /// </summary>
    Task CreateStatusInPile(PlayerChoiceContext ctx, Player player, CardModel canonicalCard, PileType pile);

    /// <summary>Upgrade a card for this combat only.</summary>
    Task UpgradeForCombat(PlayerChoiceContext ctx, Player player, CardModel card);

    /// <summary>Upgrade a card permanently, for the rest of the run. Currently unused.</summary>
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
        Report("discarding a Potion");
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

    public Task<PotionModel?> ChoosePotion(PlayerChoiceContext ctx, Player player, IReadOnlyList<PotionModel> from, LocString? prompt = null, bool allowStop = false)
    {
        Report("choosing a Potion");
        return Task.FromResult<PotionModel?>(null);
    }

    public IReadOnlyList<PotionModel> CombatPotionOptions(Player player, int count, PotionRarity? rarity = null)
    {
        Report("listing Potion options");
        return [];
    }

    public PotionModel? RandomDrawPotion(Player player)
    {
        Report("the Draw Potion pool");
        return null;
    }

    public Task GainSlots(Player player, int count)
    {
        Report("gaining Potion Slots");
        return Task.CompletedTask;
    }

    public Task LoseSlots(Player player, int count)
    {
        Report("losing Potion Slots");
        return Task.CompletedTask;
    }

    public Task<PotionModel?> ChoosePotionOption(PlayerChoiceContext ctx, Player player, IReadOnlyList<PotionModel> options)
    {
        Report("choosing a Potion option");
        return Task.FromResult<PotionModel?>(null);
    }

    public Task<CardModel?> ChooseCard(PlayerChoiceContext ctx, Player player, IReadOnlyList<CardModel> from, CardModel? source, LocString? prompt = null)
    {
        Report("choosing a card");
        return Task.FromResult<CardModel?>(null);
    }

    public IReadOnlyList<CardModel> Hand(Player player)
    {
        Report("reading the Hand");
        return [];
    }

    public IReadOnlyList<CardModel> DiscardPile(Player player)
    {
        Report("reading the Discard pile");
        return [];
    }

    public IReadOnlyList<CardModel> ExhaustPile(Player player)
    {
        Report("reading the Exhaust pile");
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

    public Task ReturnToHand(PlayerChoiceContext ctx, Player player, CardModel card)
    {
        Report("returning a card to Hand");
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

    public Task CreateStatusInPile(PlayerChoiceContext ctx, Player player, CardModel canonicalCard, PileType pile)
    {
        Report("creating a Status card in a pile");
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
