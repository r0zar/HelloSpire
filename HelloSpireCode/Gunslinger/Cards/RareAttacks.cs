using HelloSpire.HelloSpireCode.Gunslinger.Cylinder;
using HelloSpire.HelloSpireCode.Gunslinger.Powers;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;

namespace HelloSpire.HelloSpireCode.Gunslinger.Cards;

// Rare Attacks 1-9. Payoffs for the two extremes the character sets up towards: a cylinder packed
// full, or a cylinder down to its last Round.

/// <summary>Gain Deadeye, then Fire 6. The signature salvo.</summary>
public sealed class HighNoon() : GunslingerCard(3, CardType.Attack, CardRarity.Rare, TargetType.AnyEnemy)
{
    protected override IEnumerable<DynamicVar> CanonicalVars =>
        [new PowerVar<DeadeyePower>(3m), new DynamicVar("Fire", 6m)];

    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];

    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
        [HoverTipFactory.FromPower<DeadeyePower>(), Tip(GunslingerTips.Fire)];

    protected override async Task OnPlay(PlayerChoiceContext ctx, CardPlay play)
    {
        ArgumentNullException.ThrowIfNull(play.Target);

        await GunslingerEffects.GainDeadeye(ctx, Gun, DynamicVars["DeadeyePower"].BaseValue);
        await Revolver.FireTimes(ctx, Gun, play.Target, DynamicVars["Fire"].IntValue);
    }

    protected override void OnUpgrade() => EnergyCost.UpgradeBy(-1);
}

/// <summary>Fire 1. If that Round was the only one left, it hits far harder.</summary>
public sealed class OneBulletLeft() : GunslingerCard(1, CardType.Attack, CardRarity.Rare, TargetType.AnyEnemy)
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [new DynamicVar("Multiplier", 2m)];

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [Tip(GunslingerTips.Fire)];

    protected override bool ShouldGlowGoldInternal => Revolver.Peek(Gun)?.LoadedCount == 1;

    protected override async Task OnPlay(PlayerChoiceContext ctx, CardPlay play)
    {
        ArgumentNullException.ThrowIfNull(play.Target);

        var alone = Revolver.Peek(Gun)?.LoadedCount == 1;
        var options = alone
            ? new FireOptions { Multiplier = DynamicVars["Multiplier"].BaseValue }
            : FireOptions.Default;

        await Revolver.Fire(ctx, Gun, play.Target, options);
    }

    protected override void OnUpgrade() => DynamicVars["Multiplier"].UpgradeValueBy(1m);
}

/// <summary>Fire 2, and cash in a fully debuffed target.</summary>
public sealed class ExecutionersCalm() : GunslingerCard(2, CardType.Attack, CardRarity.Rare, TargetType.AnyEnemy)
{
    protected override IEnumerable<DynamicVar> CanonicalVars =>
        [new DynamicVar("Fire", 2m), new DynamicVar("Bonus", 50m)];

    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
    [
        Tip(GunslingerTips.Fire),
        HoverTipFactory.FromPower<WeakPower>(),
        HoverTipFactory.FromPower<DebilitatePower>()
    ];

    protected override async Task OnPlay(PlayerChoiceContext ctx, CardPlay play)
    {
        ArgumentNullException.ThrowIfNull(play.Target);

        var crushed = play.Target.HasPower<WeakPower>() && play.Target.HasPower<DebilitatePower>();
        var options = crushed
            ? new FireOptions { Multiplier = 1m + DynamicVars["Bonus"].BaseValue / 100m }
            : FireOptions.Default;

        await Revolver.FireTimes(ctx, Gun, play.Target, DynamicVars["Fire"].IntValue, options);
    }

    protected override void OnUpgrade() => DynamicVars["Bonus"].UpgradeValueBy(25m);
}

/// <summary>The emptier the gun, the more the one Round left is worth.</summary>
public sealed class LongShot() : GunslingerCard(2, CardType.Attack, CardRarity.Rare, TargetType.AnyEnemy)
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [new DynamicVar("Deadeye", 3m)];

    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
        [HoverTipFactory.FromPower<DeadeyePower>(), Tip(GunslingerTips.Fire)];

    protected override async Task OnPlay(PlayerChoiceContext ctx, CardPlay play)
    {
        ArgumentNullException.ThrowIfNull(play.Target);

        var cylinder = await Revolver.Get(ctx, Gun);
        var empty = cylinder == null ? 0 : CylinderPower.ChamberCount - cylinder.LoadedCount;

        await GunslingerEffects.GainDeadeye(ctx, Gun, DynamicVars["Deadeye"].BaseValue * empty);
        await Revolver.Fire(ctx, Gun, play.Target);
    }

    protected override void OnUpgrade() => DynamicVars["Deadeye"].UpgradeValueBy(1m);
}

/// <summary>Jam something far too powerful into the chamber and pull the trigger anyway.</summary>
public sealed class BlackPowder() : GunslingerCard(1, CardType.Attack, CardRarity.Rare, TargetType.AnyEnemy)
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [new DynamicVar("RoundDamage", 16m)];

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [Tip(GunslingerTips.Load), Tip(GunslingerTips.Fire)];

    protected override async Task OnPlay(PlayerChoiceContext ctx, CardPlay play)
    {
        ArgumentNullException.ThrowIfNull(play.Target);

        await Revolver.ReplaceUnderHammer(ctx, Gun, Rounds.BlackPowder(DynamicVars["RoundDamage"].IntValue));
        await Revolver.Fire(ctx, Gun, play.Target);
    }

    protected override void OnUpgrade() => DynamicVars["RoundDamage"].UpgradeValueBy(4m);
}

/// <summary>One shot per debuff on the target. Against a fully worked-over enemy, three.</summary>
public sealed class LastWord() : GunslingerCard(2, CardType.Attack, CardRarity.Rare, TargetType.AnyEnemy)
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [new DynamicVar("Bonus", 0m)];

    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
    [
        Tip(GunslingerTips.Fire),
        HoverTipFactory.FromPower<WeakPower>(),
        HoverTipFactory.FromPower<DebilitatePower>()
    ];

    protected override async Task OnPlay(PlayerChoiceContext ctx, CardPlay play)
    {
        ArgumentNullException.ThrowIfNull(play.Target);

        var options = new FireOptions { BonusDamage = DynamicVars["Bonus"].IntValue };

        await Revolver.Fire(ctx, Gun, play.Target, options);
        if (play.Target.HasPower<WeakPower>()) await Revolver.Fire(ctx, Gun, play.Target, options);
        if (play.Target.HasPower<DebilitatePower>()) await Revolver.Fire(ctx, Gun, play.Target, options);
    }

    protected override void OnUpgrade() => DynamicVars["Bonus"].UpgradeValueBy(2m);
}

/// <summary>Fire the chamber under the hammer at everything in the room at once.</summary>
public sealed class NoWitnesses() : GunslingerCard(3, CardType.Attack, CardRarity.Rare, TargetType.AllEnemies)
{
    protected override IEnumerable<IHoverTip> ExtraHoverTips => [Tip(GunslingerTips.Fire)];

    protected override async Task OnPlay(PlayerChoiceContext ctx, CardPlay play)
    {
        await Revolver.FireAtAll(ctx, Gun);
    }

    protected override void OnUpgrade() => EnergyCost.UpgradeBy(-1);
}

/// <summary>Fire 1, and hit with the same Round again. The damage repeats; the effect does not.</summary>
public sealed class DoubleTap() : GunslingerCard(1, CardType.Attack, CardRarity.Rare, TargetType.AnyEnemy)
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [new DynamicVar("Repeats", 1m)];

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [Tip(GunslingerTips.Fire)];

    protected override async Task OnPlay(PlayerChoiceContext ctx, CardPlay play)
    {
        ArgumentNullException.ThrowIfNull(play.Target);

        await Revolver.Fire(ctx, Gun, play.Target,
            new FireOptions { ExtraDamageRepeats = DynamicVars["Repeats"].IntValue });
    }

    protected override void OnUpgrade() => DynamicVars["Repeats"].UpgradeValueBy(1m);
}

/// <summary>Firing the last Round out of the gun buys the tempo to reload it.</summary>
public sealed class FinalChamber() : GunslingerCard(1, CardType.Attack, CardRarity.Rare, TargetType.AnyEnemy)
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [new EnergyVar(2), new CardsVar(0)];

    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [Tip(GunslingerTips.Fire), EnergyHoverTip];

    protected override bool ShouldGlowGoldInternal => Revolver.Peek(Gun)?.LoadedCount == 1;

    protected override async Task OnPlay(PlayerChoiceContext ctx, CardPlay play)
    {
        ArgumentNullException.ThrowIfNull(play.Target);

        await Revolver.Fire(ctx, Gun, play.Target);
        if (Revolver.Peek(Gun) is not { IsEmpty: true }) return;

        await GunslingerEffects.GainEnergy(Gun, DynamicVars.Energy.BaseValue);
        await GunslingerEffects.Draw(ctx, Gun, DynamicVars.Cards.IntValue);
    }

    protected override void OnUpgrade() => DynamicVars.Cards.UpgradeValueBy(2m);
}
