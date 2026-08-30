using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.ValueProps;

namespace HelloSpire.HelloSpireCode.Gunslinger.Cylinder;

/// <summary>
/// A single piece of ammunition sitting in a chamber.
///
/// Rounds are plain data + behaviour, not game models: they are never drafted, never in a pile, and
/// only ever live inside a <see cref="Powers.CylinderPower"/>. Their damage is dealt by the card that
/// Fires them, so Strength / Weak / Vulnerable all apply normally (see <see cref="Revolver.Fire"/>).
/// </summary>
public abstract class Round
{
    /// <summary>Localization slug, used for the chamber readout and hover text.</summary>
    public abstract string Key { get; }

    /// <summary>Printed damage. Self-Fire uses this raw, ignoring every modifier.</summary>
    public abstract int Damage { get; }

    /// <summary>Lead Rounds are the baseline ammunition; a few relics care about the distinction.</summary>
    public virtual bool IsLead => false;

    /// <summary>How the Round's damage behaves. Piercing Rounds override this to ignore Block.</summary>
    public virtual ValueProp Props => ValueProp.Move;

    /// <summary>
    /// The Round's non-damage effect, resolved after its damage lands.
    /// <paramref name="target"/> is null when the Round was fired without a specific enemy.
    /// </summary>
    public virtual Task Resolve(PlayerChoiceContext ctx, GunContext gun, Creature? target) => Task.CompletedTask;

    /// <summary>
    /// Another Round of exactly this kind, including any per-instance payload.
    ///
    /// Quick Load reloads "the last Round type you Loaded", which needs a way to make a second
    /// one from an example. Rounds are immutable data — the only mutable-looking members are
    /// init-only ints — so a shallow copy is a faithful duplicate and no subclass has to
    /// implement anything.
    /// </summary>
    public Round Duplicate() => (Round)MemberwiseClone();
}

public sealed class LeadRound : Round
{
    public override string Key => "LEAD_ROUND";
    public override int Damage => 7;
    public override bool IsLead => true;
}

public sealed class HeavyRound : Round
{
    public override string Key => "HEAVY_ROUND";
    public override int Damage => 12;
}

public sealed class CripplingRound : Round
{
    public override string Key => "CRIPPLING_ROUND";
    public override int Damage => 5;

    public override async Task Resolve(PlayerChoiceContext ctx, GunContext gun, Creature? target)
    {
        if (target == null) return;
        await GunslingerEffects.ApplyWeak(ctx, gun, target, 1);
    }
}

public sealed class PiercingRound : Round
{
    public override string Key => "PIERCING_ROUND";
    public override int Damage => 8;
    public override ValueProp Props => ValueProp.Unblockable;
}

public sealed class GuardRound : Round
{
    public override string Key => "GUARD_ROUND";
    public override int Damage => 5;

    public override async Task Resolve(PlayerChoiceContext ctx, GunContext gun, Creature? target)
    {
        await GunslingerEffects.GainBlock(gun, 5);
    }
}

public sealed class SmokeRound : Round
{
    public override string Key => "SMOKE_ROUND";
    public override int Damage => 3;

    public override async Task Resolve(PlayerChoiceContext ctx, GunContext gun, Creature? target)
    {
        await GunslingerEffects.GainDodge(ctx, gun, 1);
    }
}

public sealed class RendingRound : Round
{
    public override string Key => "RENDING_ROUND";
    public override int Damage => 6;

    public override async Task Resolve(PlayerChoiceContext ctx, GunContext gun, Creature? target)
    {
        if (target == null) return;
        await GunslingerEffects.ApplyDebilitate(ctx, gun, target, 1);
    }
}

public sealed class BlackPowderRound : Round
{
    public override string Key => "BLACK_POWDER_ROUND";

    /// <summary>The Black Powder card upgrades the Round it chambers to 20.</summary>
    public int PrintedDamage { get; init; } = 16;
    public override int Damage => PrintedDamage;

    public override async Task Resolve(PlayerChoiceContext ctx, GunContext gun, Creature? target)
    {
        if (target == null) return;
        await GunslingerEffects.LoseHp(ctx, gun, 3);
    }
}

public sealed class DeadMansRound : Round
{
    public override string Key => "DEAD_MANS_ROUND";

    /// <summary>Russian Roulette upgrades its payload to 30.</summary>
    public int PrintedDamage { get; init; } = 24;
    public override int Damage => PrintedDamage;
}
