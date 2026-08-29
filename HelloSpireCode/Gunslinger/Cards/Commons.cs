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

// The 20 commons. Between them they teach the whole character: ammunition costs a card, Firing
// spends it, and the plain Attacks are what you fall back on when the gun is empty.

/// <summary>Fire 1. Draw 1 card.</summary>
public sealed class SnapShot() : GunslingerCard(1, CardType.Attack, CardRarity.Common, TargetType.AnyEnemy)
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [new CardsVar(1), new DynamicVar("Deadeye", 0m)];

    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
        [Tip(GunslingerTips.Fire), HoverTipFactory.FromPower<DeadeyePower>()];

    protected override async Task OnPlay(PlayerChoiceContext ctx, CardPlay play)
    {
        ArgumentNullException.ThrowIfNull(play.Target);

        await GunslingerEffects.GainDeadeye(ctx, Gun, DynamicVars["Deadeye"].BaseValue);
        await Revolver.Fire(ctx, Gun, play.Target);
        await GunslingerEffects.Draw(ctx, Gun, DynamicVars.Cards.IntValue);
    }

    protected override void OnUpgrade() => DynamicVars["Deadeye"].UpgradeValueBy(2m);
}

/// <summary>Fire 3. The cheapest way to dump a prepared cylinder.</summary>
public sealed class FanTheHammer() : GunslingerCard(2, CardType.Attack, CardRarity.Common, TargetType.AnyEnemy)
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [new DynamicVar("Fire", 3m)];

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [Tip(GunslingerTips.Fire)];

    protected override async Task OnPlay(PlayerChoiceContext ctx, CardPlay play)
    {
        ArgumentNullException.ThrowIfNull(play.Target);
        await Revolver.FireTimes(ctx, Gun, play.Target, DynamicVars["Fire"].IntValue);
    }

    protected override void OnUpgrade() => DynamicVars["Fire"].UpgradeValueBy(1m);
}

/// <summary>Fire 1. If the Cylinder is empty afterward, deal additional damage.</summary>
public sealed class LastRound() : GunslingerCard(1, CardType.Attack, CardRarity.Common, TargetType.AnyEnemy)
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [new DamageVar("Bonus", 6m, ValueProp.Move)];

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [Tip(GunslingerTips.Fire)];

    protected override bool ShouldGlowGoldInternal => Revolver.Peek(Gun)?.LoadedCount == 1;

    protected override async Task OnPlay(PlayerChoiceContext ctx, CardPlay play)
    {
        ArgumentNullException.ThrowIfNull(play.Target);

        await Revolver.Fire(ctx, Gun, play.Target);

        if (Revolver.Peek(Gun) is { IsEmpty: true })
        {
            await DamageCmd.Attack(DynamicVars["Bonus"].BaseValue)
                .FromCard(this).Targeting(play.Target).Execute(ctx);
        }
    }

    protected override void OnUpgrade() => DynamicVars["Bonus"].UpgradeValueBy(3m);
}

/// <summary>Fire 1. If a Round hits, apply Weak.</summary>
public sealed class SuppressingFire() : GunslingerCard(1, CardType.Attack, CardRarity.Common, TargetType.AnyEnemy)
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [new PowerVar<WeakPower>(1m)];

    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
        [Tip(GunslingerTips.Fire), HoverTipFactory.FromPower<WeakPower>()];

    protected override async Task OnPlay(PlayerChoiceContext ctx, CardPlay play)
    {
        ArgumentNullException.ThrowIfNull(play.Target);

        var result = await Revolver.Fire(ctx, Gun, play.Target);
        if (result.Hit) await GunslingerEffects.ApplyWeak(ctx, Gun, play.Target, DynamicVars["WeakPower"].BaseValue);
    }

    protected override void OnUpgrade() => DynamicVars["WeakPower"].UpgradeValueBy(1m);
}

/// <summary>Fire 1. If a Round hits, splash the rest of the room.</summary>
public sealed class Ricochet() : GunslingerCard(1, CardType.Attack, CardRarity.Common, TargetType.AnyEnemy)
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [new DamageVar("Splash", 4m, ValueProp.Move)];

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

    protected override void OnUpgrade() => DynamicVars["Splash"].UpgradeValueBy(2m);
}

/// <summary>Deal damage. No ammunition involved — the answer to an empty gun.</summary>
public sealed class PistolWhip() : GunslingerCard(1, CardType.Attack, CardRarity.Common, TargetType.AnyEnemy)
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [new DamageVar(9m, ValueProp.Move)];

    protected override async Task OnPlay(PlayerChoiceContext ctx, CardPlay play)
    {
        ArgumentNullException.ThrowIfNull(play.Target);
        await DamageCmd.Attack(DynamicVars.Damage.BaseValue)
            .FromCard(this).Targeting(play.Target)
            .WithHitFx("vfx/vfx_attack_blunt", null, "blunt_attack.mp3").Execute(ctx);
    }

    protected override void OnUpgrade() => DynamicVars.Damage.UpgradeValueBy(3m);
}

/// <summary>Deal damage and gain Block.</summary>
public sealed class ShoulderShot() : GunslingerCard(1, CardType.Attack, CardRarity.Common, TargetType.AnyEnemy)
{
    public override bool GainsBlock => true;

    protected override IEnumerable<DynamicVar> CanonicalVars =>
        [new DamageVar(7m, ValueProp.Move), new BlockVar(4m, ValueProp.Move)];

    protected override async Task OnPlay(PlayerChoiceContext ctx, CardPlay play)
    {
        ArgumentNullException.ThrowIfNull(play.Target);

        await DamageCmd.Attack(DynamicVars.Damage.BaseValue).FromCard(this).Targeting(play.Target).Execute(ctx);
        await CreatureCmd.GainBlock(Owner.Creature, DynamicVars.Block, play);
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Damage.UpgradeValueBy(2m);
        DynamicVars.Block.UpgradeValueBy(1m);
    }
}

/// <summary>Deal damage; more if the target is already Weak.</summary>
public sealed class GutShot() : GunslingerCard(1, CardType.Attack, CardRarity.Common, TargetType.AnyEnemy)
{
    protected override IEnumerable<DynamicVar> CanonicalVars =>
        [new DamageVar(8m, ValueProp.Move), new DamageVar("Bonus", 4m, ValueProp.Move)];

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.FromPower<WeakPower>()];

    protected override bool ShouldGlowGoldInternal =>
        CombatState?.HittableEnemies.Any(enemy => enemy.HasPower<WeakPower>()) ?? false;

    protected override async Task OnPlay(PlayerChoiceContext ctx, CardPlay play)
    {
        ArgumentNullException.ThrowIfNull(play.Target);

        var damage = DynamicVars.Damage.BaseValue;
        if (play.Target.HasPower<WeakPower>()) damage += DynamicVars["Bonus"].BaseValue;

        await DamageCmd.Attack(damage).FromCard(this).Targeting(play.Target).Execute(ctx);
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Damage.UpgradeValueBy(2m);
        DynamicVars["Bonus"].UpgradeValueBy(2m);
    }
}

/// <summary>Cheap Weak, and it leaves the deck behind it.</summary>
public sealed class WarningShot() : GunslingerCard(0, CardType.Attack, CardRarity.Common, TargetType.AnyEnemy)
{
    protected override IEnumerable<DynamicVar> CanonicalVars =>
        [new DamageVar(3m, ValueProp.Move), new PowerVar<WeakPower>(1m)];

    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.FromPower<WeakPower>()];

    protected override async Task OnPlay(PlayerChoiceContext ctx, CardPlay play)
    {
        ArgumentNullException.ThrowIfNull(play.Target);

        await DamageCmd.Attack(DynamicVars.Damage.BaseValue).FromCard(this).Targeting(play.Target).Execute(ctx);
        await GunslingerEffects.ApplyWeak(ctx, Gun, play.Target, DynamicVars["WeakPower"].BaseValue);
    }

    protected override void OnUpgrade() => DynamicVars.Damage.UpgradeValueBy(2m);
}

/// <summary>Deal damage; more while every chamber is still loaded.</summary>
public sealed class PointBlank() : GunslingerCard(1, CardType.Attack, CardRarity.Common, TargetType.AnyEnemy)
{
    protected override IEnumerable<DynamicVar> CanonicalVars =>
        [new DamageVar(10m, ValueProp.Move), new DamageVar("Bonus", 4m, ValueProp.Move)];

    protected override bool ShouldGlowGoldInternal => Revolver.Peek(Gun)?.IsFull ?? false;

    protected override async Task OnPlay(PlayerChoiceContext ctx, CardPlay play)
    {
        ArgumentNullException.ThrowIfNull(play.Target);

        var damage = DynamicVars.Damage.BaseValue;
        if (Revolver.Peek(Gun) is { IsFull: true }) damage += DynamicVars["Bonus"].BaseValue;

        await DamageCmd.Attack(damage).FromCard(this).Targeting(play.Target).Execute(ctx);
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Damage.UpgradeValueBy(3m);
        DynamicVars["Bonus"].UpgradeValueBy(1m);
    }
}

/// <summary>Load 2 Lead Rounds. The workhorse reload.</summary>
public sealed class FreshCartridges() : GunslingerCard(1, CardType.Skill, CardRarity.Common, TargetType.Self)
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [new DynamicVar("Load", 2m)];

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [Tip(GunslingerTips.Load)];

    protected override async Task OnPlay(PlayerChoiceContext ctx, CardPlay play)
    {
        await CreatureCmd.TriggerAnim(Owner.Creature, "Cast", Owner.Character.CastAnimDelay);
        await Revolver.Load(ctx, Gun, Rounds.Lead, DynamicVars["Load"].IntValue);
    }

    protected override void OnUpgrade() => DynamicVars["Load"].UpgradeValueBy(1m);
}

/// <summary>Load 1 Lead Round for free, once.</summary>
public sealed class QuickLoad() : GunslingerCard(0, CardType.Skill, CardRarity.Common, TargetType.Self)
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [new DynamicVar("Load", 1m)];

    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [Tip(GunslingerTips.Load)];

    protected override async Task OnPlay(PlayerChoiceContext ctx, CardPlay play)
    {
        await Revolver.Load(ctx, Gun, Rounds.Lead, DynamicVars["Load"].IntValue);
    }

    protected override void OnUpgrade() => DynamicVars["Load"].UpgradeValueBy(1m);
}

/// <summary>Load 1 Heavy Round. Gain Block.</summary>
public sealed class HeavyCartridge() : GunslingerCard(1, CardType.Skill, CardRarity.Common, TargetType.Self)
{
    public override bool GainsBlock => true;

    protected override IEnumerable<DynamicVar> CanonicalVars => [new BlockVar(3m, ValueProp.Move)];

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [Tip(GunslingerTips.Load)];

    protected override async Task OnPlay(PlayerChoiceContext ctx, CardPlay play)
    {
        await Revolver.Load(ctx, Gun, Rounds.Heavy);
        await CreatureCmd.GainBlock(Owner.Creature, DynamicVars.Block, play);
    }

    protected override void OnUpgrade() => DynamicVars.Block.UpgradeValueBy(2m);
}

/// <summary>Load 1 Crippling Round. Gain Block.</summary>
public sealed class CripplingCartridge() : GunslingerCard(1, CardType.Skill, CardRarity.Common, TargetType.Self)
{
    public override bool GainsBlock => true;

    protected override IEnumerable<DynamicVar> CanonicalVars => [new BlockVar(3m, ValueProp.Move)];

    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
        [Tip(GunslingerTips.Load), HoverTipFactory.FromPower<WeakPower>()];

    protected override async Task OnPlay(PlayerChoiceContext ctx, CardPlay play)
    {
        await Revolver.Load(ctx, Gun, Rounds.Crippling);
        await CreatureCmd.GainBlock(Owner.Creature, DynamicVars.Block, play);
    }

    protected override void OnUpgrade() => DynamicVars.Block.UpgradeValueBy(2m);
}

/// <summary>Gain Block.</summary>
public sealed class TakeCover() : GunslingerCard(1, CardType.Skill, CardRarity.Common, TargetType.Self)
{
    public override bool GainsBlock => true;

    protected override IEnumerable<DynamicVar> CanonicalVars => [new BlockVar(7m, ValueProp.Move)];

    protected override async Task OnPlay(PlayerChoiceContext ctx, CardPlay play)
    {
        await CreatureCmd.GainBlock(Owner.Creature, DynamicVars.Block, play);
    }

    protected override void OnUpgrade() => DynamicVars.Block.UpgradeValueBy(3m);
}

/// <summary>Gain Block and Armor. The common that introduces the second defensive layer.</summary>
public sealed class DusterUp() : GunslingerCard(1, CardType.Skill, CardRarity.Common, TargetType.Self)
{
    public override bool GainsBlock => true;

    protected override IEnumerable<DynamicVar> CanonicalVars =>
        [new BlockVar(5m, ValueProp.Move), new PowerVar<ArmorPower>(1m)];

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.FromPower<ArmorPower>()];

    protected override async Task OnPlay(PlayerChoiceContext ctx, CardPlay play)
    {
        await CreatureCmd.GainBlock(Owner.Creature, DynamicVars.Block, play);
        await GunslingerEffects.GainArmor(ctx, Gun, DynamicVars["ArmorPower"].BaseValue);
    }

    protected override void OnUpgrade() => DynamicVars.Block.UpgradeValueBy(2m);
}

/// <summary>Gain Block. Cycle 1, and take more Block if that leaves the hammer on an empty chamber.</summary>
public sealed class RollAside() : GunslingerCard(1, CardType.Skill, CardRarity.Common, TargetType.Self)
{
    public override bool GainsBlock => true;

    protected override IEnumerable<DynamicVar> CanonicalVars =>
        [new BlockVar(5m, ValueProp.Move), new BlockVar("Bonus", 3m, ValueProp.Move)];

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [Tip(GunslingerTips.Cycle)];

    protected override async Task OnPlay(PlayerChoiceContext ctx, CardPlay play)
    {
        await CreatureCmd.GainBlock(Owner.Creature, DynamicVars.Block, play);
        await Revolver.Cycle(ctx, Gun);

        if (Revolver.Peek(Gun) is { UnderHammer: null })
        {
            await GunslingerEffects.GainBlock(Gun, DynamicVars["Bonus"].BaseValue);
        }
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Block.UpgradeValueBy(2m);
        DynamicVars["Bonus"].UpgradeValueBy(1m);
    }
}

/// <summary>Gain Deadeye. Bank it for whichever chamber matters.</summary>
public sealed class SteadyHand() : GunslingerCard(1, CardType.Skill, CardRarity.Common, TargetType.Self)
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [new PowerVar<DeadeyePower>(5m)];

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.FromPower<DeadeyePower>()];

    protected override async Task OnPlay(PlayerChoiceContext ctx, CardPlay play)
    {
        await GunslingerEffects.GainDeadeye(ctx, Gun, DynamicVars["DeadeyePower"].BaseValue);
    }

    protected override void OnUpgrade() => DynamicVars["DeadeyePower"].UpgradeValueBy(3m);
}

/// <summary>Spin. Draw 1 card.</summary>
public sealed class SpinCylinder() : GunslingerCard(0, CardType.Skill, CardRarity.Common, TargetType.Self)
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [new CardsVar(1), new DynamicVar("Deadeye", 0m)];

    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [Tip(GunslingerTips.Spin)];

    protected override async Task OnPlay(PlayerChoiceContext ctx, CardPlay play)
    {
        await Revolver.Spin(ctx, Gun);
        await GunslingerEffects.GainDeadeye(ctx, Gun, DynamicVars["Deadeye"].BaseValue);
        await GunslingerEffects.Draw(ctx, Gun, DynamicVars.Cards.IntValue);
    }

    protected override void OnUpgrade() => DynamicVars["Deadeye"].UpgradeValueBy(3m);
}

/// <summary>Apply Weak. No gun required.</summary>
public sealed class PocketSand() : GunslingerCard(1, CardType.Skill, CardRarity.Common, TargetType.AnyEnemy)
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [new PowerVar<WeakPower>(2m)];

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.FromPower<WeakPower>()];

    protected override async Task OnPlay(PlayerChoiceContext ctx, CardPlay play)
    {
        ArgumentNullException.ThrowIfNull(play.Target);
        await GunslingerEffects.ApplyWeak(ctx, Gun, play.Target, DynamicVars["WeakPower"].BaseValue);
    }

    protected override void OnUpgrade() => DynamicVars["WeakPower"].UpgradeValueBy(1m);
}
