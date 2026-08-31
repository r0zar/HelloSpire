using HelloSpire.HelloSpireCode.Gunslinger.Cylinder;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Rooms;

using HelloSpire.HelloSpireCode.Powers;

namespace HelloSpire.HelloSpireCode.Gunslinger.Powers;

/// <summary>
/// The revolver itself, and the Gunslinger's entire resource state.
///
/// It is a Power rather than a free-floating object for three reasons: powers are created per
/// creature, they are wiped when combat ends, and they already have a place on screen. The player
/// sees how many chambers are loaded and where the hammer sits without any new UI.
///
/// Nothing mutates this directly — every Load, Fire, Cycle and Spin goes through
/// <see cref="Revolver"/>, which is where the rules actually live.
/// </summary>
public sealed class CylinderPower : HelloSpirePower
{
    public const int ChamberCount = 6;

    public override PowerType Type => PowerType.Buff;

    /// <summary>One revolver, never two. Re-applying must not stack.</summary>
    public override PowerStackType StackType => PowerStackType.Single;

    /// <summary>Chamber contents, clockwise. A null entry is an empty chamber.</summary>
    public readonly Round?[] Chambers = new Round?[ChamberCount];

    /// <summary>Index of the chamber currently under the hammer.</summary>
    public int Hammer { get; set; }

    /// <summary>Successful Rounds fired this combat. Drives the "every 6th Round" payoffs.</summary>
    public int RoundsFiredThisCombat { get; set; }

    /// <summary>Reset each turn; Hunker Down pays out only if the gun has stayed quiet.</summary>
    public bool FiredThisTurn { get; set; }

    /// <summary>Reset each turn; Reversal fires twice off the back of a defensive turn.</summary>
    public bool ArmorGainedThisTurn { get; set; }

    /// <summary>
    /// The last Round put into a chamber this combat. Quick Load reloads more of it, and it is the
    /// only piece of cylinder state that outlives the chamber it was in.
    /// </summary>
    public Round? LastLoaded { get; set; }

    /// <summary>
    /// Spins this combat. Nothing in the rules reads it — the cylinder widget does, so that a Spin
    /// that happens to land on the chamber it started from still visibly spins the gun.
    /// </summary>
    public int SpinCount { get; set; }

    /// <summary>
    /// The combat these chambers belong to.
    ///
    /// Power models outlive the combat they were applied in — the same instance is handed back the
    /// next time it is applied, which is why every power in this mod that holds a per-combat latch
    /// clears it in <see cref="AfterCombatEnd"/>. For the cylinder that meant walking into
    /// the second fight of a run with the first fight's ammunition still in the gun. This is the
    /// belt to <see cref="AfterCombatEnd"/>'s braces: whatever happened at the end of the last
    /// combat, the first cylinder operation of a new one finds an empty gun.
    /// </summary>
    private ICombatState? _combat;

    /// <summary>
    /// Empties the gun if this is a combat the cylinder has not seen before. Called by every entry
    /// point in <see cref="Cylinder.Revolver"/> before it reads or writes a chamber, and cheap
    /// enough to call on every one of them: after the first, it is a reference comparison.
    /// </summary>
    public void SyncCombat()
    {
        var combat = Owner?.CombatState;
        if (combat == null || ReferenceEquals(combat, _combat)) return;

        _combat = combat;
        ClearForNewCombat();
    }

    /// <summary>
    /// Back to an empty gun: no Rounds, hammer on chamber 1, every counter and latch at zero.
    ///
    /// Old Iron and the rest of the opening-load relics run at the top of the player's first turn,
    /// which is after this — so the fight still starts with something in the cylinder, it is just
    /// something this fight put there.
    /// </summary>
    public void ClearForNewCombat()
    {
        Array.Clear(Chambers);
        Hammer = 0;
        RoundsFiredThisCombat = 0;
        FiredThisTurn = false;
        ArmorGainedThisTurn = false;
        LastLoaded = null;
        SpinCount = 0;
        SyncDisplay();
    }

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DynamicVar("Loaded", 0m),
        new DynamicVar("Chamber", 1m)
    ];

    public int LoadedCount => Chambers.Count(round => round != null);
    public bool IsEmpty => LoadedCount == 0;
    public bool IsFull => LoadedCount == ChamberCount;

    /// <summary>The Round about to be fired, or null if the hammer sits on an empty chamber.</summary>
    public Round? UnderHammer => Chambers[Hammer];

    /// <summary>Chamber index <paramref name="steps"/> clockwise from the hammer.</summary>
    public int Offset(int steps) => ((Hammer + steps) % ChamberCount + ChamberCount) % ChamberCount;

    public void Advance(int steps = 1) => Hammer = Offset(steps);

    /// <summary>
    /// Raised after any cylinder anywhere changes.
    ///
    /// The on-screen cylinder listens to this instead of polling. It is static because the widget
    /// is built when the combat UI activates, which is before the first Gunslinger effect has
    /// created a <see cref="CylinderPower"/> for it to subscribe to; listeners filter on
    /// <see cref="MegaCrit.Sts2.Core.Entities.Powers.PowerModel.Owner"/>. Every subscriber must
    /// unsubscribe when it leaves the tree.
    /// </summary>
    public static event Action<CylinderPower>? AnyChanged;

    /// <summary>
    /// Pushes the chamber state into the power's display vars. Called after every mutation so the
    /// power's tooltip never lies about what is in the gun.
    /// </summary>
    public void SyncDisplay()
    {
        SetVar("Loaded", LoadedCount);
        SetVar("Chamber", Hammer + 1);
        AnyChanged?.Invoke(this);
    }

    public override Task BeforeSideTurnStart(PlayerChoiceContext ctx, CombatSide side, IReadOnlyList<Creature> participants, ICombatState state)
    {
        if (side == Owner.Side)
        {
            FiredThisTurn = false;
            ArmorGainedThisTurn = false;
            SyncDisplay();
        }
        return Task.CompletedTask;
    }

    /// <summary>
    /// The gun is emptied when the fight ends, not carried into the next one.
    ///
    /// <see cref="SyncCombat"/> would catch a survivor anyway, but clearing here means the widget
    /// zeroes out with the rest of the combat UI rather than a frame into the next fight.
    /// </summary>
    public override Task AfterCombatEnd(CombatRoom room)
    {
        _combat = null;
        ClearForNewCombat();
        return Task.CompletedTask;
    }

    private void SetVar(string name, decimal value)
    {
        if (!DynamicVars.TryGetValue(name, out var dynVar)) return;
        var delta = value - dynVar.BaseValue;
        if (delta != 0m) dynVar.UpgradeValueBy(delta);
    }
}
