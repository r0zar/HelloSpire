using System.Linq;
using HelloSpire.HelloSpireCode.Powers;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Potions;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;

using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Rooms;

namespace HelloSpire.HelloSpireCode.Alchemist.Lab;

/// <summary>
/// The bench: every piece of per-combat Alchemist state in one place.
///
/// A Power rather than a free-floating object, for the same three reasons the Gunslinger's
/// cylinder is one — powers are created per creature, they are wiped when combat ends, and they
/// already have somewhere to live on screen.
///
/// Nothing mutates this directly. <see cref="Belt"/> owns the Potion half and <see cref="Ledger"/>
/// owns the Gold half, which is where the rules actually live.
/// </summary>
public sealed class LabPower : HelloSpirePower
{
    public override PowerType Type => PowerType.Buff;

    /// <summary>One bench, never two.</summary>
    public override PowerStackType StackType => PowerStackType.Single;

    // ------------------------------------------------------------------ Potions

    /// <summary>
    /// Potions this bench Brewed and has not yet lost, by reference.
    ///
    /// Volatile is tracked here rather than as a flag on the Potion because a Potion model is
    /// shared game state — marking one Volatile would mark every copy of it, including one the
    /// player bought at a Merchant. Identity is the only safe key.
    /// </summary>
    public readonly HashSet<PotionModel> Volatile = [];

    /// <summary>
    /// Potions marked for a second activation: the next time one of these is used, PotionUsePatch
    /// runs its OnUse a second time before the Potion is removed from the tracker. One-shot --
    /// removed the moment it fires, so marking a Potion twice does not stack into three
    /// activations. Two sources mark it: Pressure Burst picks a target explicitly; Eternal
    /// Crucible's power claims the first Potion used each turn, in PotionUsePatch's prefix, before
    /// that Potion has resolved even once.
    /// </summary>
    public readonly HashSet<PotionModel> DoubleActivate = [];

    /// <summary>Potions used this combat, in order. Reconstitute and Refill the Retort read this.</summary>
    public readonly List<PotionModel> UsedThisCombat = [];

    /// <summary>Extra Potion Slots granted for this combat only. All of them are Volatile-only.</summary>
    public int TemporarySlots { get; set; }

    public int PotionsUsedThisTurn { get; set; }
    public int SlotsEmptiedThisTurn { get; set; }
    public int BrewedThisTurn { get; set; }
    public int DistilledThisTurn { get; set; }

    // ------------------------------------------------------------------ Gold

    /// <summary>Gold this bench has generated during this combat.</summary>
    public int GoldGainedThisCombat { get; set; }

    /// <summary>Gold Invested during this combat. Midas Needle and Compound Interest read it.</summary>
    public int GoldSpentThisCombat { get; set; }

    public int GoldGainedThisTurn { get; set; }
    public int GoldSpentThisTurn { get; set; }

    // ------------------------------------------------------------------ Transform bookkeeping

    /// <summary>Cards Exhausted this turn, by any means. Cinnabar Edge and the Exhaust engines read it.</summary>
    public int CardsExhaustedThisTurn { get; set; }

    /// <summary>Cards created into Hand this turn. Reactive Slash reads it.</summary>
    public int CardsCreatedThisTurn { get; set; }

    /// <summary>
    /// The type of the card played immediately before whichever one is currently resolving -- null
    /// before any card has been played this combat. Set by LastCardTypeTrackerPatch, which stashes
    /// the old <see cref="LastCardType"/> here before overwriting it with the new play, so a card
    /// reading this during its own OnPlay sees the card before it, not itself. QuickSilver reads it.
    /// </summary>
    public CardType? PreviousCardType { get; set; }

    /// <summary>The type of the most recently played card, including the one currently resolving.</summary>
    public CardType? LastCardType { get; set; }

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DynamicVar("Held", 0m),
        new DynamicVar("Gold", 0m)
    ];

    /// <summary>Everything that resets between the owner's turns.</summary>
    public override Task BeforeSideTurnStart(PlayerChoiceContext ctx, CombatSide side, IReadOnlyList<Creature> participants, ICombatState state)
    {
        if (side != Owner.Side) return Task.CompletedTask;

        PotionsUsedThisTurn = 0;
        SlotsEmptiedThisTurn = 0;
        BrewedThisTurn = 0;
        DistilledThisTurn = 0;
        GoldGainedThisTurn = 0;
        GoldSpentThisTurn = 0;
        CardsExhaustedThisTurn = 0;
        CardsCreatedThisTurn = 0;

        return Task.CompletedTask;
    }

    /// <summary>
    /// Real base-game hook (decompiled from sts2.dll: AbstractModel.AfterCardExhausted, dispatched
    /// by Hook.AfterCardExhausted for every Exhaust in combat), and now the single source of truth
    /// for CardsExhaustedThisTurn and NotifyExhausted -- Alchemy.Exhaust no longer does either
    /// itself. This is what makes self-Exhaust cards (Aegis Formula, Pocket Formula, Stabilize,
    /// Pressure Burst, ...) count for Cinnabar Edge and Conservation of Matter; those never call
    /// Alchemy.Exhaust at all, since the base game Exhausts them directly as their own cost.
    ///
    /// Filtered to this bench's own owner: the hook fires for every listener across the whole
    /// combat, not just this creature's own cards, and an ally's Exhaust in multiplayer should
    /// not read as "you Exhausted a card."
    /// </summary>
    public override async Task AfterCardExhausted(PlayerChoiceContext ctx, CardModel card, bool causedByEthereal)
    {
        if (Owner.Player is not { } player || card.Owner != player) return;

        CardsExhaustedThisTurn++;
        await AlchemistHooks.NotifyExhausted(ctx, LabContext.From(player));
    }

    /// <summary>
    /// The bench closes: Volatile Potions are discarded (they never leave combat), and then the
    /// temporary slots are taken back. That order matters -- discarding first empties the doomed
    /// slots, so a real Potion sitting in one relocates instead of being lost (the game moves
    /// occupants of removed slots into earlier free ones).
    /// </summary>
    public override async Task AfterCombatEnd(CombatRoom room)
    {
        if (Owner.Player is not { } player) return;

        foreach (var potion in LabBridge.Current.Held(player).Where(Volatile.Contains).ToList())
            await LabBridge.Current.Discard(null!, player, potion);
        Volatile.Clear();

        if (TemporarySlots > 0)
        {
            await LabBridge.Current.LoseSlots(player, TemporarySlots);
            TemporarySlots = 0;
        }
    }
}

/// <summary>
/// Potency: whenever you use a Volatile Potion, its damage and Block values increase by this much.
///
/// A Power so that it reads like Strength and Dexterity, which is exactly what it is — a stat with
/// one deliberate restriction. It never applies to a found, bought or Procured Potion; that
/// restriction is the whole reason the class is allowed an exception to "Potions ignore stats" at
/// all, and <see cref="Belt"/> is the only place that enforces it.
/// </summary>
public sealed class PotencyPower : HelloSpirePower
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;
}

/// <summary>
/// The next <see cref="Amount"/> Potions used this turn are not consumed. Bottled Time's payload.
///
/// TryClaim is called by PotionUsePatch for every Potion an Alchemist uses (real or Volatile --
/// there is nothing here that distinguishes them, and there never was; the trigger was always
/// generic). One stack is spent per save via PowerCmd.ModifyAmount, the same way any other
/// Counter-type Power shrinks; the Power removes itself once Amount reaches 0, same as always.
/// </summary>
public sealed class BottledTimePower : HelloSpirePower
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;

    /// <summary>Spend one stack for the Potion currently resolving, if any remain.</summary>
    public async Task<bool> TryClaim(PlayerChoiceContext ctx)
    {
        if (Amount <= 0) return false;
        await PowerCmd.ModifyAmount(ctx, this, -1m, null, null);
        return true;
    }

    public override async Task BeforeSideTurnStart(PlayerChoiceContext ctx, CombatSide side, IReadOnlyList<Creature> participants, ICombatState state)
    {
        if (side != Owner.Side) return;
        await PowerCmd.Remove(this);
    }
}

/// <summary>
/// The next Skill played is played twice. Catalytic Wash's payload.
///
/// Not a custom hack: the base game already has a first-class hook for "play this card N times"
/// (CardModel.GeneratePlayCount -> AbstractModel.ModifyCardPlayCount, decompiled from sts2.dll),
/// which the base game's own OneTwoPunchPower uses for "your next Attack is played twice" -- this
/// is that same mechanism, gated on Skill instead of Attack. ModifyCardPlayCount and
/// AfterModifyingCardPlayCount run in the same play, back to back, so decrementing in the latter
/// spends the stack for the exact card that just got doubled, never a later one.
/// </summary>
public sealed class SkillReplayPower : HelloSpirePower
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;

    public override int ModifyCardPlayCount(CardModel card, Creature? target, int playCount)
    {
        if (card.Owner.Creature != Owner || card.Type != CardType.Skill) return playCount;
        return playCount + 1;
    }

    public override async Task AfterModifyingCardPlayCount(CardModel card) => await PowerCmd.Decrement(this);

    public override async Task AfterSideTurnEnd(PlayerChoiceContext ctx, CombatSide side, IEnumerable<Creature> participants)
    {
        if (participants.Contains(Owner)) await PowerCmd.Remove(this);
    }
}
