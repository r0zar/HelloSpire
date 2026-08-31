using System.Linq;
using HelloSpire.HelloSpireCode.Powers;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
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

    /// <summary>Potions used this combat, in order. Reconstitute and Refill the Retort read this.</summary>
    public readonly List<PotionModel> UsedThisCombat = [];

    /// <summary>Extra Potion Slots granted for this combat only. All of them are Volatile-only.</summary>
    public int TemporarySlots { get; set; }

    public int PotionsUsedThisTurn { get; set; }
    public int SlotsEmptiedThisTurn { get; set; }
    public int BrewedThisTurn { get; set; }
    public int DistilledThisTurn { get; set; }

    // ------------------------------------------------------------------ Gold

    /// <summary>Gold this bench has generated during this combat. Auric Needle scales off it.</summary>
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
/// The next Potion used this turn is not consumed. Bottled Time's payload.
/// Removed by <see cref="Belt"/> as soon as it saves a Potion, or at the owner's next turn.
/// </summary>
public sealed class BottledTimePower : HelloSpirePower
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;

    public override async Task BeforeSideTurnStart(PlayerChoiceContext ctx, CombatSide side, IReadOnlyList<Creature> participants, ICombatState state)
    {
        if (side != Owner.Side) return;
        await PowerCmd.Remove(this);
    }
}
