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

/// <summary>Loads itself and then fires. The one card that never cares what the gun was doing.</summary>
public sealed class LeadStorm() : GunslingerCard(2, CardType.Attack, CardRarity.Common, TargetType.AnyEnemy)
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

/// <summary>
/// Fire 1. If that empties the Cylinder, a Reload comes back to Hand from wherever it ended up.
///
/// Firing the gun dry used to be a bonus damage roll; now it hands you the answer to being dry,
/// which is the more interesting version of the same beat. The search covers the Exhaust pile
/// too, so the card still works in a deck that has already burned its Reloads.
/// </summary>
public sealed class LastRound() : GunslingerCard(1, CardType.Attack, CardRarity.Common, TargetType.AnyEnemy)
{
    protected override IEnumerable<IHoverTip> ExtraHoverTips => [Tip(GunslingerTips.Fire)];

    protected override bool ShouldGlowGoldInternal => Revolver.Peek(Gun)?.LoadedCount == 1;

    protected override async Task OnPlay(PlayerChoiceContext ctx, CardPlay play)
    {
        ArgumentNullException.ThrowIfNull(play.Target);

        await Revolver.Fire(ctx, Gun, play.Target);

        if (Revolver.Peek(Gun) is { IsEmpty: true })
        {
            await GunslingerEffects.ReturnToHand<Reload>(Gun);
        }
    }

    protected override void OnUpgrade() => EnergyCost.UpgradeBy(-1);
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

/// <summary>
/// Load 2 Lead Rounds and 1 random Round. The workhorse reload, with something odd in the box.
///
/// The wildcard is the common that teaches the whole ammunition menu: over a run you will fire
/// every kind of Round out of this card without ever having drafted for it, which is both how the
/// character learns and where its best turns come from.
/// </summary>
public sealed class FreshCartridges() : GunslingerCard(1, CardType.Skill, CardRarity.Common, TargetType.Self)
{
    public override bool GainsBlock => true;

    protected override IEnumerable<DynamicVar> CanonicalVars =>
        [new DynamicVar("Load", 2m), new DynamicVar("Wild", 1m), new BlockVar(2m, ValueProp.Move)];

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [Tip(GunslingerTips.Load)];

    protected override async Task OnPlay(PlayerChoiceContext ctx, CardPlay play)
    {
        await CreatureCmd.TriggerAnim(Owner.Creature, "Cast", Owner.Character.CastAnimDelay);
        await Revolver.Load(ctx, Gun, Rounds.Lead, DynamicVars["Load"].IntValue);

        // Rolled once per Round, so an upgraded copy is two different surprises rather than a pair.
        for (var i = 0; i < DynamicVars["Wild"].IntValue; i++)
            await Revolver.Load(ctx, Gun, Rounds.RandomOrdinary(Gun), 1);

        await CreatureCmd.GainBlock(Owner.Creature, DynamicVars.Block, play);
    }

    protected override void OnUpgrade() => DynamicVars["Wild"].UpgradeValueBy(1m);
}

/// <summary>
/// Load 1-2 more of whatever you Loaded last, for free, once.
///
/// It reaches into the same box the last card reached into, which is what makes it premium after a
/// Cartridge and merely fine after a Reload. It is never dead: the starter relic has already
/// Loaded before the first turn, and with nothing Loaded at all it falls back to Lead.
/// </summary>
public sealed class QuickLoad() : GunslingerCard(0, CardType.Skill, CardRarity.Common, TargetType.Self)
{
    public override bool GainsBlock => true;

    protected override IEnumerable<DynamicVar> CanonicalVars =>
        [new DynamicVar("LoadMin", 1m), new DynamicVar("LoadMax", 2m), new BlockVar(2m, ValueProp.Move)];

    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [Tip(GunslingerTips.Load)];

    protected override async Task OnPlay(PlayerChoiceContext ctx, CardPlay play)
    {
        await Revolver.LoadBetween(ctx, Gun, Revolver.LastLoaded(Gun),
            DynamicVars["LoadMin"].IntValue, DynamicVars["LoadMax"].IntValue);
        await CreatureCmd.GainBlock(Owner.Creature, DynamicVars.Block, play);
    }

    protected override void OnUpgrade()
    {
        DynamicVars["LoadMin"].UpgradeValueBy(1m);
        DynamicVars["LoadMax"].UpgradeValueBy(1m);
    }
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

/// <summary>
/// Gain Block, and thumb a Round in if the hammer is sitting on nothing.
///
/// The character's one unconditional Block common, so it keeps the full pure-Block rate rather
/// than paying for the Load. The Load is conditional on the chamber under the hammer being empty,
/// which means it is free exactly when the gun is dry — the turn you were going to spend hiding
/// anyway — and never overwrites a Round you were about to Fire.
/// </summary>
public sealed class TakeCover() : GunslingerCard(1, CardType.Skill, CardRarity.Common, TargetType.Self)
{
    public override bool GainsBlock => true;

    protected override IEnumerable<DynamicVar> CanonicalVars =>
        [new BlockVar(7m, ValueProp.Move), new DynamicVar("Load", 1m)];

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [Tip(GunslingerTips.Load)];

    protected override async Task OnPlay(PlayerChoiceContext ctx, CardPlay play)
    {
        await CreatureCmd.GainBlock(Owner.Creature, DynamicVars.Block, play);

        // Peek is null before the first Gunslinger effect of the combat, which is also "nothing
        // under the hammer" — Load creates the cylinder in that case.
        if (Revolver.Peek(Gun) is null or { UnderHammer: null })
        {
            await Revolver.Load(ctx, Gun, Rounds.Lead, DynamicVars["Load"].IntValue);
        }
    }

    protected override void OnUpgrade() => DynamicVars.Block.UpgradeValueBy(3m);
}

/// <summary>
/// Gain Block and Armor, and chamber a Guard Round. The common that introduces the second
/// defensive layer, and the one that says defence and ammunition are the same turn.
///
/// The Guard Round is bought with 2 Block off the printed value, which puts the card at the
/// hybrid block/ammo rate the Cartridge commons are priced at. It pays that back the turn you
/// Fire it — a Guard Round is 5 damage and 5 more Block — so the card is a slower 5 Block that
/// also shoots, rather than a straight downgrade.
/// </summary>
public sealed class DusterUp() : GunslingerCard(1, CardType.Skill, CardRarity.Common, TargetType.Self)
{
    public override bool GainsBlock => true;

    protected override IEnumerable<DynamicVar> CanonicalVars =>
        [new BlockVar(3m, ValueProp.Move), new PowerVar<ArmorPower>(1m)];

    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
        [Tip(GunslingerTips.Load), HoverTipFactory.FromPower<ArmorPower>()];

    protected override async Task OnPlay(PlayerChoiceContext ctx, CardPlay play)
    {
        await CreatureCmd.GainBlock(Owner.Creature, DynamicVars.Block, play);
        await GunslingerEffects.GainArmor(ctx, Gun, DynamicVars["ArmorPower"].BaseValue);
        await Revolver.Load(ctx, Gun, Rounds.Guard);
    }

    protected override void OnUpgrade() => DynamicVars.Block.UpgradeValueBy(2m);
}

/// <summary>
/// Gain Block. Cycle 1, and if that leaves the hammer on an empty chamber, take more Block and
/// fill it.
///
/// The empty-chamber branch was already the card's payoff; the Load rides on the same branch
/// rather than being priced separately, so the card never loads over ammunition it just cycled to.
/// </summary>
public sealed class RollAside() : GunslingerCard(1, CardType.Skill, CardRarity.Common, TargetType.Self)
{
    public override bool GainsBlock => true;

    protected override IEnumerable<DynamicVar> CanonicalVars =>
        [new BlockVar(5m, ValueProp.Move), new BlockVar("Bonus", 3m, ValueProp.Move), new DynamicVar("Load", 1m)];

    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
        [Tip(GunslingerTips.Cycle), Tip(GunslingerTips.Load)];

    protected override async Task OnPlay(PlayerChoiceContext ctx, CardPlay play)
    {
        await CreatureCmd.GainBlock(Owner.Creature, DynamicVars.Block, play);
        await Revolver.Cycle(ctx, Gun);

        if (Revolver.Peek(Gun) is { UnderHammer: null })
        {
            await GunslingerEffects.GainBlock(Gun, DynamicVars["Bonus"].BaseValue);
            await Revolver.Load(ctx, Gun, Rounds.Lead, DynamicVars["Load"].IntValue);
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

/// <summary>
/// Apply 2 Weak. No gun required.
///
/// Upgrading makes it free rather than making it 3 Weak. The Gunslinger's problem is that setting
/// the gun up and using it compete for the same Energy every turn, so a debuff that costs nothing
/// to slot in is worth more to the deck than a third stack of Weak on one enemy.
/// </summary>
public sealed class PocketSand() : GunslingerCard(1, CardType.Skill, CardRarity.Common, TargetType.AnyEnemy)
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [new PowerVar<WeakPower>(2m)];

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.FromPower<WeakPower>()];

    protected override async Task OnPlay(PlayerChoiceContext ctx, CardPlay play)
    {
        ArgumentNullException.ThrowIfNull(play.Target);
        await GunslingerEffects.ApplyWeak(ctx, Gun, play.Target, DynamicVars["WeakPower"].BaseValue);
    }

    protected override void OnUpgrade() => EnergyCost.UpgradeBy(-1);
}
