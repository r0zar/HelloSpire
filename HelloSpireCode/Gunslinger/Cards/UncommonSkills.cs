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

// Uncommon Skills 16-31: the ammunition menu, and the defensive layers that make the Gunslinger
// able to stand still for a turn while the gun refills.

/// <summary>Load a Lead Round and a Crippling Round.</summary>
public sealed class Bandolier() : GunslingerCard(1, CardType.Skill, CardRarity.Uncommon, TargetType.Self)
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [new DynamicVar("Lead", 1m)];

    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
        [Tip(GunslingerTips.Load), HoverTipFactory.FromPower<WeakPower>()];

    protected override async Task OnPlay(PlayerChoiceContext ctx, CardPlay play)
    {
        await Revolver.Load(ctx, Gun, Rounds.Lead, DynamicVars["Lead"].IntValue);
        await Revolver.Load(ctx, Gun, Rounds.Crippling);
    }

    protected override void OnUpgrade() => DynamicVars["Lead"].UpgradeValueBy(1m);
}

/// <summary>Top the gun right up with Lead.</summary>
public sealed class Speedloader() : GunslingerCard(2, CardType.Skill, CardRarity.Uncommon, TargetType.Self)
{
    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [Tip(GunslingerTips.Load)];

    protected override async Task OnPlay(PlayerChoiceContext ctx, CardPlay play)
    {
        await Revolver.FillEmpty(ctx, Gun, Rounds.Lead);
    }

    protected override void OnUpgrade() => EnergyCost.UpgradeBy(-1);
}

/// <summary>
/// Load specialist ammunition alongside a Lead Round.
///
/// The design offers Heavy / Crippling / Guard as a player choice. With no chamber UI to choose in,
/// the card reads the board the way the player would: Guard when something is winding up to hit
/// you, Crippling when nothing is Weak yet, and Heavy when neither is true and damage is the point.
/// </summary>
public sealed class CustomLoad() : GunslingerCard(1, CardType.Skill, CardRarity.Uncommon, TargetType.Self)
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [new DynamicVar("Special", 1m)];

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [Tip(GunslingerTips.Load)];

    protected override async Task OnPlay(PlayerChoiceContext ctx, CardPlay play)
    {
        await Revolver.Load(ctx, Gun, PickAmmunition(), DynamicVars["Special"].IntValue);
        await Revolver.Load(ctx, Gun, Rounds.Lead);
    }

    private Func<Round> PickAmmunition()
    {
        var enemies = GunslingerEffects.Enemies(Gun);
        if (enemies.Any(GunslingerEffects.IntendsToAttack)) return Rounds.Guard;
        if (enemies.Count > 0 && !enemies.Any(enemy => enemy.HasPower<WeakPower>())) return Rounds.Crippling;
        return Rounds.Heavy;
    }

    protected override void OnUpgrade() => DynamicVars["Special"].UpgradeValueBy(1m);
}

/// <summary>Load Piercing Rounds — the answer to a Block-heavy fight.</summary>
public sealed class PiercingCartridge() : GunslingerCard(1, CardType.Skill, CardRarity.Uncommon, TargetType.Self)
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [new DynamicVar("Load", 2m)];

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [Tip(GunslingerTips.Load)];

    protected override async Task OnPlay(PlayerChoiceContext ctx, CardPlay play) =>
        await Revolver.Load(ctx, Gun, Rounds.Piercing, DynamicVars["Load"].IntValue);

    protected override void OnUpgrade() => DynamicVars["Load"].UpgradeValueBy(1m);
}

/// <summary>Load Guard Rounds, so that shooting and defending stop competing.</summary>
public sealed class GuardCartridge() : GunslingerCard(1, CardType.Skill, CardRarity.Uncommon, TargetType.Self)
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [new DynamicVar("Load", 2m)];

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [Tip(GunslingerTips.Load)];

    protected override async Task OnPlay(PlayerChoiceContext ctx, CardPlay play) =>
        await Revolver.Load(ctx, Gun, Rounds.Guard, DynamicVars["Load"].IntValue);

    protected override void OnUpgrade() => DynamicVars["Load"].UpgradeValueBy(1m);
}

/// <summary>Load a Smoke Round. Gain Block.</summary>
public sealed class SmokeCartridge() : GunslingerCard(1, CardType.Skill, CardRarity.Uncommon, TargetType.Self)
{
    public override bool GainsBlock => true;

    protected override IEnumerable<DynamicVar> CanonicalVars => [new BlockVar(4m, ValueProp.Move)];

    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
        [Tip(GunslingerTips.Load), HoverTipFactory.FromPower<DodgePower>()];

    protected override async Task OnPlay(PlayerChoiceContext ctx, CardPlay play)
    {
        await Revolver.Load(ctx, Gun, Rounds.Smoke);
        await CreatureCmd.GainBlock(Owner.Creature, DynamicVars.Block, play);
    }

    protected override void OnUpgrade() => DynamicVars.Block.UpgradeValueBy(2m);
}

/// <summary>Cycle 1. Gain Deadeye. Free, and the cleanest way to skip a bad chamber.</summary>
public sealed class ReCock() : GunslingerCard(0, CardType.Skill, CardRarity.Uncommon, TargetType.Self)
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [new PowerVar<DeadeyePower>(2m)];

    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
        [Tip(GunslingerTips.Cycle), HoverTipFactory.FromPower<DeadeyePower>()];

    protected override async Task OnPlay(PlayerChoiceContext ctx, CardPlay play)
    {
        await Revolver.Cycle(ctx, Gun);
        await GunslingerEffects.GainDeadeye(ctx, Gun, DynamicVars["DeadeyePower"].BaseValue);
    }

    protected override void OnUpgrade() => DynamicVars["DeadeyePower"].UpgradeValueBy(2m);
}

/// <summary>
/// Cycle up to X to find a loaded chamber, and draw if you are already sitting on one.
///
/// "Up to" is resolved for the player: the card advances the fewest chambers that put a Round under
/// the hammer, and stops where it started if one is already there.
/// </summary>
public sealed class CheckTheCylinder() : GunslingerCard(0, CardType.Skill, CardRarity.Uncommon, TargetType.Self)
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [new DynamicVar("Cycle", 2m), new CardsVar(1)];

    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [Tip(GunslingerTips.Cycle)];

    protected override async Task OnPlay(PlayerChoiceContext ctx, CardPlay play)
    {
        var cylinder = await Revolver.Get(ctx, Gun);
        if (cylinder == null) return;

        if (cylinder.UnderHammer == null)
        {
            for (var steps = 1; steps <= DynamicVars["Cycle"].IntValue; steps++)
            {
                if (cylinder.Chambers[cylinder.Offset(steps)] == null) continue;
                await Revolver.Cycle(ctx, Gun, steps);
                break;
            }
        }

        if (cylinder.UnderHammer != null) await GunslingerEffects.Draw(ctx, Gun, DynamicVars.Cards.IntValue);
    }

    protected override void OnUpgrade() => DynamicVars["Cycle"].UpgradeValueBy(1m);
}

/// <summary>Set up the next Round to land exactly where the hammer is. Gain Deadeye.</summary>
public sealed class StackedChamber() : GunslingerCard(1, CardType.Skill, CardRarity.Uncommon, TargetType.Self)
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [new PowerVar<DeadeyePower>(5m)];

    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
        [Tip(GunslingerTips.Load), HoverTipFactory.FromPower<DeadeyePower>()];

    protected override async Task OnPlay(PlayerChoiceContext ctx, CardPlay play)
    {
        await PowerCmd.Apply<StackedChamberPower>(Owner.Creature, 1m, Owner.Creature, this, false);
        await GunslingerEffects.GainDeadeye(ctx, Gun, DynamicVars["DeadeyePower"].BaseValue);
    }

    protected override void OnUpgrade() => DynamicVars["DeadeyePower"].UpgradeValueBy(3m);
}

/// <summary>Gain Armor.</summary>
public sealed class UnderTheDuster() : GunslingerCard(1, CardType.Skill, CardRarity.Uncommon, TargetType.Self)
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [new PowerVar<ArmorPower>(3m)];

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.FromPower<ArmorPower>()];

    protected override async Task OnPlay(PlayerChoiceContext ctx, CardPlay play) =>
        await GunslingerEffects.GainArmor(ctx, Gun, DynamicVars["ArmorPower"].BaseValue);

    protected override void OnUpgrade() => DynamicVars["ArmorPower"].UpgradeValueBy(1m);
}

/// <summary>Gain Block; more if you kept the gun holstered this turn.</summary>
public sealed class HunkerDown() : GunslingerCard(1, CardType.Skill, CardRarity.Uncommon, TargetType.Self)
{
    public override bool GainsBlock => true;

    protected override IEnumerable<DynamicVar> CanonicalVars =>
        [new BlockVar(8m, ValueProp.Move), new BlockVar("Bonus", 4m, ValueProp.Move)];

    protected override async Task OnPlay(PlayerChoiceContext ctx, CardPlay play)
    {
        await CreatureCmd.GainBlock(Owner.Creature, DynamicVars.Block, play);

        if (Revolver.Peek(Gun) is not { FiredThisTurn: true })
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

/// <summary>Gain Dodge. Expensive, one-shot, and the only unconditional Dodge below Rare.</summary>
public sealed class DuckAndWeave() : GunslingerCard(2, CardType.Skill, CardRarity.Uncommon, TargetType.Self)
{
    protected override IEnumerable<DynamicVar> CanonicalVars =>
        [new PowerVar<DodgePower>(1m), new BlockVar(0m, ValueProp.Move)];

    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.FromPower<DodgePower>()];

    protected override async Task OnPlay(PlayerChoiceContext ctx, CardPlay play)
    {
        await GunslingerEffects.GainDodge(ctx, Gun, DynamicVars["DodgePower"].BaseValue);
        await GunslingerEffects.GainBlock(Gun, DynamicVars.Block.BaseValue);
    }

    protected override void OnUpgrade() => DynamicVars.Block.UpgradeValueBy(6m);
}

/// <summary>Block that only shows up when something is actually coming, and Armor against a big hit.</summary>
public sealed class DiveForCover() : GunslingerCard(1, CardType.Skill, CardRarity.Uncommon, TargetType.Self)
{
    private const int ArmorThreshold = 20;

    public override bool GainsBlock => true;

    protected override IEnumerable<DynamicVar> CanonicalVars =>
        [new BlockVar(9m, ValueProp.Move), new PowerVar<ArmorPower>(1m), new DynamicVar("Threshold", ArmorThreshold)];

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.FromPower<ArmorPower>()];

    protected override bool ShouldGlowGoldInternal =>
        CombatState?.HittableEnemies.Any(GunslingerEffects.IntendsToAttack) ?? false;

    protected override async Task OnPlay(PlayerChoiceContext ctx, CardPlay play)
    {
        var incoming = GunslingerEffects.IncomingAttackDamage(Gun);
        if (incoming <= 0) return;

        await CreatureCmd.GainBlock(Owner.Creature, DynamicVars.Block, play);

        if (incoming >= ArmorThreshold)
        {
            await GunslingerEffects.GainArmor(ctx, Gun, DynamicVars["ArmorPower"].BaseValue);
        }
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Block.UpgradeValueBy(3m);
        DynamicVars["ArmorPower"].UpgradeValueBy(1m);
    }
}

/// <summary>Pay a little HP for a lot of defence.</summary>
public sealed class GritTeeth() : GunslingerCard(1, CardType.Skill, CardRarity.Uncommon, TargetType.Self)
{
    public override bool GainsBlock => true;

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new BlockVar(10m, ValueProp.Move),
        new PowerVar<ArmorPower>(2m),
        new DamageVar("SelfDamage", 2m, ValueProp.Unblockable | ValueProp.Unpowered)
    ];

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.FromPower<ArmorPower>()];

    protected override async Task OnPlay(PlayerChoiceContext ctx, CardPlay play)
    {
        await GunslingerEffects.LoseHp(ctx, Gun, DynamicVars["SelfDamage"].BaseValue);
        await CreatureCmd.GainBlock(Owner.Creature, DynamicVars.Block, play);
        await GunslingerEffects.GainArmor(ctx, Gun, DynamicVars["ArmorPower"].BaseValue);
    }

    protected override void OnUpgrade() => DynamicVars.Block.UpgradeValueBy(3m);
}

/// <summary>Spin, and take whatever the cylinder gives you: cover, or a way out.</summary>
public sealed class DeadMansBluff() : GunslingerCard(1, CardType.Skill, CardRarity.Uncommon, TargetType.Self)
{
    public override bool GainsBlock => true;

    protected override IEnumerable<DynamicVar> CanonicalVars =>
        [new BlockVar(9m, ValueProp.Move), new PowerVar<DodgePower>(1m)];

    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];

    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
        [Tip(GunslingerTips.Spin), HoverTipFactory.FromPower<DodgePower>()];

    protected override async Task OnPlay(PlayerChoiceContext ctx, CardPlay play)
    {
        await Revolver.Spin(ctx, Gun);

        if (Revolver.Peek(Gun) is { UnderHammer: null })
        {
            await GunslingerEffects.GainDodge(ctx, Gun, DynamicVars["DodgePower"].BaseValue);
            return;
        }

        await CreatureCmd.GainBlock(Owner.Creature, DynamicVars.Block, play);
    }

    protected override void OnUpgrade() => DynamicVars.Block.UpgradeValueBy(3m);
}

/// <summary>Weak and Debilitate together, which is where the Gunslinger's defence really comes from.</summary>
public sealed class ColdRead() : GunslingerCard(1, CardType.Skill, CardRarity.Uncommon, TargetType.AnyEnemy)
{
    protected override IEnumerable<DynamicVar> CanonicalVars =>
        [new PowerVar<WeakPower>(1m), new PowerVar<DebilitatePower>(1m)];

    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];

    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
        [HoverTipFactory.FromPower<WeakPower>(), HoverTipFactory.FromPower<DebilitatePower>()];

    protected override async Task OnPlay(PlayerChoiceContext ctx, CardPlay play)
    {
        ArgumentNullException.ThrowIfNull(play.Target);

        await GunslingerEffects.ApplyWeak(ctx, Gun, play.Target, DynamicVars["WeakPower"].BaseValue);
        await GunslingerEffects.ApplyDebilitate(ctx, Gun, play.Target, DynamicVars["DebilitatePower"].BaseValue);
    }

    protected override void OnUpgrade() => DynamicVars["WeakPower"].UpgradeValueBy(1m);
}
