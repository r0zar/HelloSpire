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

// The five multiplayer cards. See design/multiplayer-cards.md for the set they belong to and the
// synergies they are shaped around.
//
// The Gunslinger's job in a party is amplification: it turns down what the room hits for and
// turns up what everyone else hits for, and in exchange it gets the setup turns it cannot
// otherwise afford. Every card here does one of those two things.

/// <summary>
/// Base for the multiplayer five.
///
/// It exists for one reason: there is exactly one place to apply the multiplayer gate once we
/// know what the gate is. Every shipped character carries five cards outside its 80-card pool,
/// so the mechanism exists in the game — but it has not been identified in sts2.dll yet, and
/// these cards inherit <see cref="Characters.GunslingerCard"/>'s [Pool] attribute, so today they
/// are ordinary members of the Gunslinger's reward pool and will show up in solo runs.
///
/// TODO(Phase 9): find the flag/pool/rarity the base game uses for multiplayer-only cards and
/// apply it here. Until then this is a known, deliberate wrong.
/// </summary>
public abstract class GunslingerMultiplayerCard(int cost, CardType type, CardRarity rarity, TargetType target)
    : GunslingerCard(cost, type, rarity, target);

/// <summary>
/// Another player draws 2 cards, and hands you 2 Rounds of whatever their class carries.
///
/// The character's worst turn is the one spent loading instead of shooting. This makes that turn
/// somebody else's good turn, which is the whole argument for having a Gunslinger in the party.
///
/// What comes back depends on who you asked — see <see cref="AmmoAffinity"/> — so the card is a
/// different card in every lobby, and reading the party is part of playing it. Solo it draws for
/// you and hands you Lead, which is the old card exactly.
/// </summary>
public sealed class HandMeThat() : GunslingerMultiplayerCard(1, CardType.Skill, CardRarity.Uncommon, TargetType.AnyAlly)
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [new CardsVar(2), new DynamicVar("Load", 2m)];

    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
        [Tip(GunslingerTips.Load), Tip(GunslingerTips.MatchedAmmo)];

    protected override async Task OnPlay(PlayerChoiceContext ctx, CardPlay play)
    {
        await CreatureCmd.TriggerAnim(Owner.Creature, "Cast", Owner.Character.CastAnimDelay);

        var ally = GunslingerEffects.ResolveAlly(Gun, play.Target);
        await GunslingerEffects.DrawFor(ctx, ally, DynamicVars.Cards.IntValue);

        await Revolver.Load(ctx, Gun, AmmoAffinity.For(ally), DynamicVars["Load"].IntValue);
    }

    protected override void OnUpgrade() => DynamicVars.Cards.UpgradeValueBy(1m);
}

/// <summary>
/// Apply Debilitate to ALL enemies.
///
/// The most party-dependent card the character has. Debilitate doubles Weak and Vulnerable, and
/// the Gunslinger has essentially no native Vulnerable — so alone this is a Weak-doubler and not
/// much else. Next to anyone who brings Vulnerable it doubles a debuff this character could not
/// have applied, which is the point.
/// </summary>
public sealed class SoftenedUp() : GunslingerMultiplayerCard(1, CardType.Skill, CardRarity.Uncommon, TargetType.AllEnemies)
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [new PowerVar<DebilitatePower>(1m)];

    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.FromPower<DebilitatePower>()];

    protected override async Task OnPlay(PlayerChoiceContext ctx, CardPlay play)
    {
        await CreatureCmd.TriggerAnim(Owner.Creature, "Cast", Owner.Character.CastAnimDelay);

        foreach (var enemy in GunslingerEffects.Enemies(Gun))
            await GunslingerEffects.ApplyDebilitate(ctx, Gun, enemy, DynamicVars["DebilitatePower"].BaseValue);
    }

    protected override void OnUpgrade() => DynamicVars["DebilitatePower"].UpgradeValueBy(1m);
}

/// <summary>
/// ALL players gain Block. Cycle 1, and if that lands on a loaded chamber, ALL players gain more.
///
/// A flat party-Block card is one any character could print. Paying for the bonus with a Cycle
/// makes it a Gunslinger card: the hammer moves whether you wanted it to or not, which is
/// sometimes the setup you needed and sometimes walks straight past a lined-up Heavy Round.
/// </summary>
public sealed class CoveringPartner() : GunslingerMultiplayerCard(1, CardType.Skill, CardRarity.Uncommon, TargetType.Self)
{
    public override bool GainsBlock => true;

    protected override IEnumerable<DynamicVar> CanonicalVars =>
        [new BlockVar(5m, ValueProp.Move), new BlockVar("Bonus", 3m, ValueProp.Move)];

    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
        [Tip(GunslingerTips.Cycle), Tip(GunslingerTips.TheCylinder)];

    protected override async Task OnPlay(PlayerChoiceContext ctx, CardPlay play)
    {
        await GunslingerEffects.GainBlockAll(Gun, DynamicVars.Block.BaseValue);

        await Revolver.Cycle(ctx, Gun);

        if (Revolver.Peek(Gun) is { UnderHammer: not null })
            await GunslingerEffects.GainBlockAll(Gun, DynamicVars["Bonus"].BaseValue);
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Block.UpgradeValueBy(2m);
        DynamicVars["Bonus"].UpgradeValueBy(1m);
    }
}

/// <summary>
/// Fire 3. Apply Weak to ALL enemies.
///
/// The payload: the burst turn that also turns the room's damage down for everyone at the table.
/// The Weak lands after the shots so that a Crippling Round's own Weak stacks with it rather
/// than being wasted on a target that was already Weak.
/// </summary>
public sealed class SuppressiveVolley() : GunslingerMultiplayerCard(2, CardType.Attack, CardRarity.Uncommon, TargetType.AnyEnemy)
{
    protected override IEnumerable<DynamicVar> CanonicalVars =>
        [new DynamicVar("Fire", 3m), new PowerVar<WeakPower>(1m)];

    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
        [Tip(GunslingerTips.Fire), HoverTipFactory.FromPower<WeakPower>()];

    protected override async Task OnPlay(PlayerChoiceContext ctx, CardPlay play)
    {
        ArgumentNullException.ThrowIfNull(play.Target);

        await Revolver.FireTimes(ctx, Gun, play.Target, DynamicVars["Fire"].IntValue);

        foreach (var enemy in GunslingerEffects.Enemies(Gun))
            await GunslingerEffects.ApplyWeak(ctx, Gun, enemy, DynamicVars["WeakPower"].BaseValue);
    }

    protected override void OnUpgrade() => DynamicVars["WeakPower"].UpgradeValueBy(1m);
}

/// <summary>
/// The first time each turn each other player plays an Attack, Load 1 Lead Round.
///
/// The engine slot: allies' actions become this character's resource. Ammunition is normally paid
/// for in cards, and in a party everyone else is attacking anyway — so their turn loads the gun.
/// Blank in single-player, which is correct for a card that only exists because of the party.
/// </summary>
public sealed class RideTogether() : GunslingerMultiplayerCard(2, CardType.Power, CardRarity.Uncommon, TargetType.Self)
{
    protected override IEnumerable<DynamicVar> CanonicalVars =>
        [new PowerVar<RideTogetherPower>(1m), new DynamicVar("Deadeye", 0m)];

    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
        [Tip(GunslingerTips.Load), HoverTipFactory.FromPower<RideTogetherPower>()];

    protected override async Task OnPlay(PlayerChoiceContext ctx, CardPlay play)
    {
        await CreatureCmd.TriggerAnim(Owner.Creature, "Cast", Owner.Character.CastAnimDelay);

        var power = await PowerCmd.Apply<RideTogetherPower>(
            ctx, Owner.Creature, DynamicVars["RideTogetherPower"].BaseValue, Owner.Creature, this);

        if (power != null) power.DeadeyeBonus = DynamicVars["Deadeye"].IntValue;
    }

    protected override void OnUpgrade() => DynamicVars["Deadeye"].UpgradeValueBy(2m);
}
