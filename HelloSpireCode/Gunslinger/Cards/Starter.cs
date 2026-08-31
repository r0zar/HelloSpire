using HelloSpire.HelloSpireCode.Gunslinger.Cylinder;
using HelloSpire.HelloSpireCode.Gunslinger.Powers;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;

namespace HelloSpire.HelloSpireCode.Gunslinger.Cards;

// The starting ten: four Strikes, four Defends, Reload and Quick Draw.
//
// Between them they teach the character in one fight — ammunition exists, Firing spends it, a
// zero-cost Fire is only good if you prepared, and Strikes are still there when the gun is dry.

/// <summary>Deal damage.</summary>
public sealed class StrikeGunslinger() : GunslingerCard(1, CardType.Attack, CardRarity.Basic, TargetType.AnyEnemy)
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [new DamageVar(6m, ValueProp.Move)];

    protected override HashSet<CardTag> CanonicalTags => [CardTag.Strike];

    protected override async Task OnPlay(PlayerChoiceContext ctx, CardPlay play)
    {
        ArgumentNullException.ThrowIfNull(play.Target);
        await DamageCmd.Attack(DynamicVars.Damage.BaseValue)
            .FromCard(this).Targeting(play.Target)
            .WithHitFx("vfx/vfx_attack_blunt", null, "blunt_attack.mp3").Execute(ctx);
    }

    protected override void OnUpgrade() => DynamicVars.Damage.UpgradeValueBy(3m);
}

/// <summary>Gain Block.</summary>
public sealed class DefendGunslinger() : GunslingerCard(1, CardType.Skill, CardRarity.Basic, TargetType.Self)
{
    public override bool GainsBlock => true;

    protected override IEnumerable<DynamicVar> CanonicalVars => [new BlockVar(5m, ValueProp.Move)];

    protected override HashSet<CardTag> CanonicalTags => [CardTag.Defend];

    protected override async Task OnPlay(PlayerChoiceContext ctx, CardPlay play) =>
        await CreatureCmd.GainBlock(Owner.Creature, DynamicVars.Block, play);

    protected override void OnUpgrade() => DynamicVars.Block.UpgradeValueBy(3m);
}

/// <summary>
/// Load 2-4 Lead Rounds and gain a little Block. Half the starting deck's job is putting bullets
/// in the gun.
///
/// The count is rolled rather than fixed, which is the character's whole temperament in the very
/// first card you play: a handful of shells goes in, and how many is the gun's business. The floor
/// is what the card used to be worth, so a bad roll is never worse than the old card.
///
/// The 3 Block is the fix for the turn-one dilemma. Reloading used to be a turn where nothing
/// happened at all — no damage, no defence, one card spent on a promise — which is where the
/// "Gunslinger feels weak" complaint actually starts. It is deliberately under a Defend, so the
/// card is still a reload that covers you rather than a Defend that also loads.
/// </summary>
public sealed class Reload() : GunslingerCard(1, CardType.Skill, CardRarity.Basic, TargetType.Self)
{
    public override bool GainsBlock => true;

    protected override IEnumerable<DynamicVar> CanonicalVars =>
        [new DynamicVar("LoadMin", 2m), new DynamicVar("LoadMax", 4m), new BlockVar(3m, ValueProp.Move)];

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [Tip(GunslingerTips.Load), Tip(GunslingerTips.TheCylinder)];

    protected override async Task OnPlay(PlayerChoiceContext ctx, CardPlay play)
    {
        await CreatureCmd.TriggerAnim(Owner.Creature, "Cast", Owner.Character.CastAnimDelay);
        await Revolver.LoadBetween(ctx, Gun, Rounds.Lead,
            DynamicVars["LoadMin"].IntValue, DynamicVars["LoadMax"].IntValue);
        await CreatureCmd.GainBlock(Owner.Creature, DynamicVars.Block, play);
    }

    protected override void OnUpgrade()
    {
        DynamicVars["LoadMin"].UpgradeValueBy(1m);
        DynamicVars["LoadMax"].UpgradeValueBy(1m);
        DynamicVars.Block.UpgradeValueBy(2m);
    }
}

/// <summary>
/// Fire 2, for free. Upgraded, Fire 2-3.
///
/// Not to be confused with the uncommon <see cref="Quickdraw"/>, which pays off a Click. This is
/// the starter, and it is two blank trigger pulls until you have loaded something.
///
/// It fires twice rather than once because one Round per free card was the arithmetic that made
/// the character feel weak: two cards and an Energy to put six damage on a target is worse than a
/// Strike. Two chambers a card is the rate the rest of the deck is priced against.
///
/// The upgrade used to hand out Deadeye, and it was the worst upgrade in the pack: 2 more damage
/// on one Round, printed on a card whose whole point is that it is free and fires twice. A third
/// chamber some of the time is worth far more to the same card, and it upgrades the starter along
/// the axis the character actually cares about — how much of the cylinder a card can spend — while
/// staying honest about the gun deciding how much comes out. It is also the same "roughly what you
/// asked for, occasionally more" roll the other starter, <see cref="Reload"/>, is built on.
/// </summary>
public sealed class QuickDraw() : GunslingerCard(0, CardType.Attack, CardRarity.Basic, TargetType.AnyEnemy)
{
    protected override IEnumerable<DynamicVar> CanonicalVars =>
        [new DynamicVar("FireMin", 2m), new DynamicVar("FireMax", 2m)];

    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
        [Tip(GunslingerTips.Fire), Tip(GunslingerTips.Click)];

    protected override async Task OnPlay(PlayerChoiceContext ctx, CardPlay play)
    {
        ArgumentNullException.ThrowIfNull(play.Target);

        var times = Revolver.Roll(Gun, DynamicVars["FireMin"].IntValue, DynamicVars["FireMax"].IntValue);
        await Revolver.FireTimes(ctx, Gun, play.Target, times);
    }

    protected override void OnUpgrade() => DynamicVars["FireMax"].UpgradeValueBy(1m);
}
