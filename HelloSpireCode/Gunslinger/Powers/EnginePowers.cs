using HelloSpire.HelloSpireCode.Gunslinger.Cylinder;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;

namespace HelloSpire.HelloSpireCode.Gunslinger.Powers;

/// <summary>
/// Shared plumbing for the Gunslinger's Power cards: a <see cref="GunContext"/> built from the
/// creature the power sits on, and a once-per-turn latch, which almost all of them need.
/// </summary>
public abstract class GunslingerEnginePower : HelloSpirePower
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;

    /// <summary>Set by the effect that fires; cleared at the start of the owner's turn.</summary>
    protected bool UsedThisTurn { get; set; }

    /// <summary>The gun this power is attached to, or null outside of a player's combat.</summary>
    protected GunContext? Gun
    {
        get
        {
            var player = GunslingerEffects.PlayerFor(Owner);
            return player == null ? null : GunContext.From(player);
        }
    }

    public override Task BeforeSideTurnStart(PlayerChoiceContext ctx, CombatSide side, CombatState state)
    {
        if (side == Owner.Side) UsedThisTurn = false;
        return OnOwnerTurnStart(ctx, side, state);
    }

    protected virtual Task OnOwnerTurnStart(PlayerChoiceContext ctx, CombatSide side, CombatState state) =>
        Task.CompletedTask;
}

/// <summary>Every 6th Round you Fire, draw. Rewards emptying the cylinder in rhythm rather than in bursts.</summary>
public sealed class GunfightersRhythmPower : GunslingerEnginePower, IFireListener
{
    public async Task OnFired(PlayerChoiceContext ctx, GunContext gun, FireResult result)
    {
        if (result.WasClick) return;

        var cylinder = Revolver.Peek(gun);
        if (cylinder == null || cylinder.RoundsFiredThisCombat % 6 != 0) return;

        Flash();
        await GunslingerEffects.Draw(ctx, gun, (int)Amount);
    }
}

/// <summary>
/// The first time each turn Armor prevents damage, bank Block for next turn.
/// Armor absorbs during the enemy's turn, so the payout lands where it is useful.
/// </summary>
public sealed class HardLeatherPower : GunslingerEnginePower, IArmorListener
{
    public void OnArmorPrevented(Creature owner)
    {
        if (UsedThisTurn || owner != Owner) return;
        UsedThisTurn = true;

        Flash();

        // Fired from inside the damage pipeline patch, which cannot await. The Block lands on the
        // BlockNextTurnPower and is collected at the start of the next turn either way.
        _ = PowerCmd.Apply<BlockNextTurnPower>(Owner, Amount, Owner, null, false);
    }
}

/// <summary>The first Round you Fire each turn also buys a little Block.</summary>
public sealed class SmokeAndLeadPower : GunslingerEnginePower, IFireListener
{
    public async Task OnFired(PlayerChoiceContext ctx, GunContext gun, FireResult result)
    {
        if (UsedThisTurn || result.WasClick) return;
        UsedThisTurn = true;

        Flash();
        await GunslingerEffects.GainBlock(gun, Amount);
    }
}

/// <summary>The first Spin each turn pays out as Deadeye, turning a gamble into a setup step.</summary>
public sealed class SureHandPower : GunslingerEnginePower, ISpinListener
{
    public async Task OnSpun(PlayerChoiceContext ctx, GunContext gun)
    {
        if (UsedThisTurn) return;
        UsedThisTurn = true;

        Flash();
        await GunslingerEffects.GainDeadeye(ctx, gun, Amount);
    }
}

/// <summary>
/// The first card you play each turn that Fires effectively costs 1 less.
///
/// Implemented as a refund on the first Fire of the turn rather than as a cost reduction: the base
/// game's cost hooks cannot see "this card will Fire" without the card having been played, and the
/// two only differ when the Energy was not there to spend in the first place.
/// </summary>
public sealed class QuickdrawLegendPower : GunslingerEnginePower, IFireListener
{
    public async Task OnFired(PlayerChoiceContext ctx, GunContext gun, FireResult result)
    {
        if (UsedThisTurn) return;
        UsedThisTurn = true;

        Flash();
        await GunslingerEffects.GainEnergy(gun, Amount);
    }
}

/// <summary>A special Round appears in the gun each turn, so the cylinder is never truly dry.</summary>
public sealed class BottomlessBandolierPower : GunslingerEnginePower
{
    /// <summary>Deadeye granted alongside the Round. Zero until the card is upgraded.</summary>
    public int DeadeyeBonus { get; set; }

    protected override async Task OnOwnerTurnStart(PlayerChoiceContext ctx, CombatSide side, CombatState state)
    {
        if (side != Owner.Side) return;
        if (Gun is not { } gun) return;

        var cylinder = Revolver.Peek(gun);
        if (cylinder == null || cylinder.IsFull) return;

        Flash();

        var rng = gun.Player.RunState.Rng.Chaotic;
        var pick = Math.Clamp(rng.NextInt(0, Rounds.Special.Length - 1), 0, Rounds.Special.Length - 1);
        await Revolver.Load(ctx, gun, Rounds.Special[pick]);

        if (DeadeyeBonus > 0) await GunslingerEffects.GainDeadeye(ctx, gun, DeadeyeBonus);
    }
}

/// <summary>
/// After you Spin, Cycle up to X — enough control to turn a bad roll into a usable one.
///
/// The design offers this as a choice. With no cylinder UI to choose in, it takes the choice a
/// player would: if the hammer landed on an empty chamber, Cycle just far enough to reach a loaded
/// one, and otherwise leave a good result alone.
/// </summary>
public sealed class LoadedDicePower : GunslingerEnginePower, ISpinListener
{
    public async Task OnSpun(PlayerChoiceContext ctx, GunContext gun)
    {
        var cylinder = Revolver.Peek(gun);
        if (cylinder == null || cylinder.UnderHammer != null || cylinder.IsEmpty) return;

        for (var steps = 1; steps <= (int)Amount; steps++)
        {
            if (cylinder.Chambers[cylinder.Offset(steps)] == null) continue;

            Flash();
            await Revolver.Cycle(ctx, gun, steps);
            return;
        }
    }
}

/// <summary>The first time each turn Armor would erode, it holds.</summary>
public sealed class IronWillPower : GunslingerEnginePower
{
    protected override Task OnOwnerTurnStart(PlayerChoiceContext ctx, CombatSide side, CombatState state)
    {
        if (side != Owner.Side) return Task.CompletedTask;

        var armor = Owner.GetPower<ArmorPower>();
        if (armor != null) armor.SkipNextDecrease = true;

        return Task.CompletedTask;
    }
}

/// <summary>Dodge is rare and expensive; this makes each point of it also worth a chunk of Block.</summary>
public sealed class UntouchablePower : GunslingerEnginePower, IDodgeListener
{
    public async Task OnDodgeGained(PlayerChoiceContext ctx, GunContext gun, int amount)
    {
        Flash();
        await GunslingerEffects.GainBlock(gun, Amount * amount);
    }
}

/// <summary>
/// The first Weak you apply each turn drags a Debilitate along with it — which, since Debilitate
/// doubles Weak, is the Gunslinger's only real way to stack debuff pressure.
/// </summary>
public sealed class DebilitatingPresencePower : GunslingerEnginePower, IWeakListener
{
    public async Task OnWeakApplied(PlayerChoiceContext ctx, GunContext gun, Creature target, int amount)
    {
        if (UsedThisTurn) return;
        UsedThisTurn = true;

        Flash();
        await GunslingerEffects.ApplyDebilitate(ctx, gun, target, Amount);
    }
}

/// <summary>
/// Every 6th Round hits far harder and refunds an Energy — the capstone that pays off counting
/// chambers all fight.
/// </summary>
public sealed class SixthShotPower : GunslingerEnginePower, IRoundDamageModifier, IFireListener
{
    private static bool IsSixthShot(GunContext gun)
    {
        var cylinder = Revolver.Peek(gun);
        return cylinder != null && (cylinder.RoundsFiredThisCombat + 1) % 6 == 0;
    }

    public int ModifyRoundDamage(Round round, GunContext gun) => IsSixthShot(gun) ? (int)Amount : 0;

    public async Task OnFired(PlayerChoiceContext ctx, GunContext gun, FireResult result)
    {
        if (result.WasClick) return;

        var cylinder = Revolver.Peek(gun);
        if (cylinder == null || cylinder.RoundsFiredThisCombat % 6 != 0) return;

        Flash();
        await GunslingerEffects.GainEnergy(gun, 1);
    }
}
