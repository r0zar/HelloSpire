using HelloSpire.HelloSpireCode.Gunslinger.Cylinder;
using HelloSpire.HelloSpireCode.Gunslinger.Powers;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;

namespace HelloSpire.HelloSpireCode.Gunslinger.Cards;

// Uncommon Attacks 1-15. This is where the cylinder stops being a resource and starts being a
// puzzle: multi-shot, Click payoffs, and cards that care which chamber is coming up.

/// <summary>Bring the best chamber under the hammer, then Fire it.</summary>
public sealed class CalledShot() : GunslingerCard(1, CardType.Attack, CardRarity.Uncommon, TargetType.AnyEnemy)
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [new DynamicVar("Deadeye", 0m)];

    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
        [Tip(GunslingerTips.Fire), HoverTipFactory.FromPower<DeadeyePower>()];

    protected override async Task OnPlay(PlayerChoiceContext ctx, CardPlay play)
    {
        ArgumentNullException.ThrowIfNull(play.Target);

        await Revolver.MoveBestLoadedUnderHammer(ctx, Gun);
        await GunslingerEffects.GainDeadeye(ctx, Gun, DynamicVars["Deadeye"].BaseValue);
        await Revolver.Fire(ctx, Gun, play.Target);
    }

    protected override void OnUpgrade() => DynamicVars["Deadeye"].UpgradeValueBy(4m);
}

/// <summary>Fire 1. A Click is not a failure here — it is card draw.</summary>
public sealed class Quickdraw() : GunslingerCard(0, CardType.Attack, CardRarity.Uncommon, TargetType.AnyEnemy)
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [new CardsVar(2)];

    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [Tip(GunslingerTips.Fire), Tip(GunslingerTips.Click)];

    protected override async Task OnPlay(PlayerChoiceContext ctx, CardPlay play)
    {
        ArgumentNullException.ThrowIfNull(play.Target);

        var result = await Revolver.Fire(ctx, Gun, play.Target);
        if (result.WasClick) await GunslingerEffects.Draw(ctx, Gun, DynamicVars.Cards.IntValue);
    }

    protected override void OnUpgrade() => DynamicVars.Cards.UpgradeValueBy(1m);
}

/// <summary>Fire 2.</summary>
public sealed class DoubleAction() : GunslingerCard(1, CardType.Attack, CardRarity.Uncommon, TargetType.AnyEnemy)
{
    protected override IEnumerable<DynamicVar> CanonicalVars =>
        [new DynamicVar("Fire", 2m), new DynamicVar("Deadeye", 0m)];

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [Tip(GunslingerTips.Fire)];

    protected override async Task OnPlay(PlayerChoiceContext ctx, CardPlay play)
    {
        ArgumentNullException.ThrowIfNull(play.Target);

        await GunslingerEffects.GainDeadeye(ctx, Gun, DynamicVars["Deadeye"].BaseValue);
        await Revolver.FireTimes(ctx, Gun, play.Target, DynamicVars["Fire"].IntValue);
    }

    protected override void OnUpgrade() => DynamicVars["Deadeye"].UpgradeValueBy(3m);
}

/// <summary>Fire 1, straight through Block, whatever is in the chamber.</summary>
public sealed class ThroughTheCoat() : GunslingerCard(1, CardType.Attack, CardRarity.Uncommon, TargetType.AnyEnemy)
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [new DynamicVar("Deadeye", 0m)];

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [Tip(GunslingerTips.Fire)];

    protected override async Task OnPlay(PlayerChoiceContext ctx, CardPlay play)
    {
        ArgumentNullException.ThrowIfNull(play.Target);

        await GunslingerEffects.GainDeadeye(ctx, Gun, DynamicVars["Deadeye"].BaseValue);
        await Revolver.Fire(ctx, Gun, play.Target, new FireOptions { IgnoreBlock = true });
    }

    protected override void OnUpgrade() => DynamicVars["Deadeye"].UpgradeValueBy(4m);
}

/// <summary>Deal damage; punish an undefended enemy with Weak.</summary>
public sealed class Kneecapper() : GunslingerCard(1, CardType.Attack, CardRarity.Uncommon, TargetType.AnyEnemy)
{
    protected override IEnumerable<DynamicVar> CanonicalVars =>
        [new DamageVar(8m, ValueProp.Move), new PowerVar<WeakPower>(2m)];

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.FromPower<WeakPower>()];

    protected override async Task OnPlay(PlayerChoiceContext ctx, CardPlay play)
    {
        ArgumentNullException.ThrowIfNull(play.Target);

        var hadBlock = play.Target.Block > 0m;
        await DamageCmd.Attack(DynamicVars.Damage.BaseValue).FromCard(this).Targeting(play.Target).Execute(ctx);

        if (!hadBlock) await GunslingerEffects.ApplyWeak(ctx, Gun, play.Target, DynamicVars["WeakPower"].BaseValue);
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Damage.UpgradeValueBy(3m);
        DynamicVars["WeakPower"].UpgradeValueBy(1m);
    }
}

/// <summary>Fire 1. Weak targets get pinned down with Debilitate.</summary>
public sealed class PinningShot() : GunslingerCard(1, CardType.Attack, CardRarity.Uncommon, TargetType.AnyEnemy)
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [new PowerVar<DebilitatePower>(1m)];

    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
    [
        Tip(GunslingerTips.Fire),
        HoverTipFactory.FromPower<WeakPower>(),
        HoverTipFactory.FromPower<DebilitatePower>()
    ];

    protected override async Task OnPlay(PlayerChoiceContext ctx, CardPlay play)
    {
        ArgumentNullException.ThrowIfNull(play.Target);

        await Revolver.Fire(ctx, Gun, play.Target);

        if (play.Target.HasPower<WeakPower>())
        {
            await GunslingerEffects.ApplyDebilitate(ctx, Gun, play.Target, DynamicVars["DebilitatePower"].BaseValue);
        }
    }

    protected override void OnUpgrade() => DynamicVars["DebilitatePower"].UpgradeValueBy(1m);
}

/// <summary>Fire 1. If a Round hits, the room catches the spray.</summary>
public sealed class Crossfire() : GunslingerCard(1, CardType.Attack, CardRarity.Uncommon, TargetType.AnyEnemy)
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [new DamageVar("Splash", 5m, ValueProp.Move)];

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [Tip(GunslingerTips.Fire)];

    protected override async Task OnPlay(PlayerChoiceContext ctx, CardPlay play)
    {
        ArgumentNullException.ThrowIfNull(play.Target);

        var result = await Revolver.Fire(ctx, Gun, play.Target);
        if (!result.Hit) return;

        var others = GunslingerEffects.OtherEnemies(Gun, play.Target);
        if (others.Count == 0) return;

        foreach (var other in others)
            await DamageCmd.Attack(DynamicVars["Splash"].BaseValue).FromCard(this).Targeting(other).Execute(ctx);
    }

    protected override void OnUpgrade() => DynamicVars["Splash"].UpgradeValueBy(3m);
}

/// <summary>Spin, then Fire 2 wherever the barrel happens to point.</summary>
public sealed class TrickShot() : GunslingerCard(1, CardType.Attack, CardRarity.Uncommon, TargetType.RandomEnemy)
{
    protected override IEnumerable<DynamicVar> CanonicalVars =>
        [new DynamicVar("Fire", 2m), new DynamicVar("Bonus", 2m)];

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [Tip(GunslingerTips.Spin), Tip(GunslingerTips.Fire)];

    protected override async Task OnPlay(PlayerChoiceContext ctx, CardPlay play)
    {
        await Revolver.Spin(ctx, Gun);
        await Revolver.FireAtRandom(ctx, Gun, DynamicVars["Fire"].IntValue,
            new FireOptions { BonusDamage = DynamicVars["Bonus"].IntValue });
    }

    protected override void OnUpgrade() => DynamicVars["Bonus"].UpgradeValueBy(2m);
}

/// <summary>Fire until the gun goes quiet. A full cylinder makes this the biggest turn in the deck.</summary>
public sealed class RunTheCylinder() : GunslingerCard(2, CardType.Attack, CardRarity.Uncommon, TargetType.AnyEnemy)
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [new DynamicVar("Bonus", 0m)];

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [Tip(GunslingerTips.Fire), Tip(GunslingerTips.Click)];

    protected override async Task OnPlay(PlayerChoiceContext ctx, CardPlay play)
    {
        ArgumentNullException.ThrowIfNull(play.Target);

        await Revolver.FireUntilClick(ctx, Gun, play.Target, CylinderPower.ChamberCount,
            new FireOptions { BonusDamage = DynamicVars["Bonus"].IntValue });
    }

    protected override void OnUpgrade() => DynamicVars["Bonus"].UpgradeValueBy(2m);
}

/// <summary>Fire 6. Everything, right now, Clicks and all.</summary>
public sealed class EmptyTheCylinder() : GunslingerCard(3, CardType.Attack, CardRarity.Uncommon, TargetType.AnyEnemy)
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [new DynamicVar("Fire", 6m)];

    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [Tip(GunslingerTips.Fire)];

    protected override async Task OnPlay(PlayerChoiceContext ctx, CardPlay play)
    {
        ArgumentNullException.ThrowIfNull(play.Target);
        await Revolver.FireTimes(ctx, Gun, play.Target, DynamicVars["Fire"].IntValue);
    }

    protected override void OnUpgrade() => EnergyCost.UpgradeBy(-1);
}

/// <summary>Fire 2, and turn every Round that lands into Block.</summary>
public sealed class CoveringFire() : GunslingerCard(2, CardType.Attack, CardRarity.Uncommon, TargetType.AnyEnemy)
{
    public override bool GainsBlock => true;

    protected override IEnumerable<DynamicVar> CanonicalVars =>
        [new DynamicVar("Fire", 2m), new BlockVar(4m, ValueProp.Move)];

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [Tip(GunslingerTips.Fire)];

    protected override async Task OnPlay(PlayerChoiceContext ctx, CardPlay play)
    {
        ArgumentNullException.ThrowIfNull(play.Target);

        var results = await Revolver.FireTimes(ctx, Gun, play.Target, DynamicVars["Fire"].IntValue);
        var hits = results.Count(result => result.Hit);

        for (var i = 0; i < hits; i++)
        {
            await CreatureCmd.GainBlock(Owner.Creature, DynamicVars.Block, play);
        }
    }

    protected override void OnUpgrade() => DynamicVars.Block.UpgradeValueBy(1m);
}

/// <summary>Loads itself and then fires. The one card that never cares what the gun was doing.</summary>
public sealed class LeadStorm() : GunslingerCard(2, CardType.Attack, CardRarity.Uncommon, TargetType.AnyEnemy)
{
    protected override IEnumerable<DynamicVar> CanonicalVars =>
        [new DynamicVar("Load", 2m), new DynamicVar("Fire", 2m)];

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [Tip(GunslingerTips.Load), Tip(GunslingerTips.Fire)];

    protected override async Task OnPlay(PlayerChoiceContext ctx, CardPlay play)
    {
        ArgumentNullException.ThrowIfNull(play.Target);

        await Revolver.Load(ctx, Gun, Rounds.Lead, DynamicVars["Load"].IntValue);
        await Revolver.FireTimes(ctx, Gun, play.Target, DynamicVars["Fire"].IntValue);
    }

    protected override void OnUpgrade()
    {
        DynamicVars["Load"].UpgradeValueBy(1m);
        DynamicVars["Fire"].UpgradeValueBy(1m);
    }
}

/// <summary>A pile of Deadeye onto the very next Round, then two shots to place it.</summary>
public sealed class Hammerfall() : GunslingerCard(2, CardType.Attack, CardRarity.Uncommon, TargetType.AnyEnemy)
{
    protected override IEnumerable<DynamicVar> CanonicalVars =>
        [new PowerVar<DeadeyePower>(8m), new DynamicVar("Fire", 2m)];

    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
        [HoverTipFactory.FromPower<DeadeyePower>(), Tip(GunslingerTips.Fire)];

    protected override async Task OnPlay(PlayerChoiceContext ctx, CardPlay play)
    {
        ArgumentNullException.ThrowIfNull(play.Target);

        await GunslingerEffects.GainDeadeye(ctx, Gun, DynamicVars["DeadeyePower"].BaseValue);
        await Revolver.FireTimes(ctx, Gun, play.Target, DynamicVars["Fire"].IntValue);
    }

    protected override void OnUpgrade() => DynamicVars["DeadeyePower"].UpgradeValueBy(4m);
}

/// <summary>Deal damage, and Fire as well if they were winding up to hit you.</summary>
public sealed class Showdown() : GunslingerCard(1, CardType.Attack, CardRarity.Uncommon, TargetType.AnyEnemy)
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [new DamageVar(6m, ValueProp.Move)];

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [Tip(GunslingerTips.Fire)];

    protected override bool ShouldGlowGoldInternal =>
        CombatState?.HittableEnemies.Any(GunslingerEffects.IntendsToAttack) ?? false;

    protected override async Task OnPlay(PlayerChoiceContext ctx, CardPlay play)
    {
        ArgumentNullException.ThrowIfNull(play.Target);

        var attacking = GunslingerEffects.IntendsToAttack(play.Target);
        await DamageCmd.Attack(DynamicVars.Damage.BaseValue).FromCard(this).Targeting(play.Target).Execute(ctx);

        if (attacking) await Revolver.Fire(ctx, Gun, play.Target);
    }

    protected override void OnUpgrade() => DynamicVars.Damage.UpgradeValueBy(3m);
}

/// <summary>Turtling up pays out as an extra shot.</summary>
public sealed class Reversal() : GunslingerCard(1, CardType.Attack, CardRarity.Uncommon, TargetType.AnyEnemy)
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [new DynamicVar("Deadeye", 0m)];

    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
        [Tip(GunslingerTips.Fire), HoverTipFactory.FromPower<ArmorPower>()];

    protected override bool ShouldGlowGoldInternal => Revolver.Peek(Gun)?.ArmorGainedThisTurn ?? false;

    protected override async Task OnPlay(PlayerChoiceContext ctx, CardPlay play)
    {
        ArgumentNullException.ThrowIfNull(play.Target);

        await GunslingerEffects.GainDeadeye(ctx, Gun, DynamicVars["Deadeye"].BaseValue);

        var shots = Revolver.Peek(Gun)?.ArmorGainedThisTurn == true ? 2 : 1;
        await Revolver.FireTimes(ctx, Gun, play.Target, shots);
    }

    protected override void OnUpgrade() => DynamicVars["Deadeye"].UpgradeValueBy(3m);
}
