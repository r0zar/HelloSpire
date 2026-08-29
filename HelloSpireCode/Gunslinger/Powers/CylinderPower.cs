using HelloSpire.HelloSpireCode.Gunslinger.Cylinder;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;

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
    /// Pushes the chamber state into the power's display vars. Called after every mutation so the
    /// power's tooltip never lies about what is in the gun.
    /// </summary>
    public void SyncDisplay()
    {
        SetVar("Loaded", LoadedCount);
        SetVar("Chamber", Hammer + 1);
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

    private void SetVar(string name, decimal value)
    {
        if (!DynamicVars.TryGetValue(name, out var dynVar)) return;
        var delta = value - dynVar.BaseValue;
        if (delta != 0m) dynVar.UpgradeValueBy(delta);
    }
}
