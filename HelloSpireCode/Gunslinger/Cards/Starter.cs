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

/// <summary>Load 2 Lead Rounds. Half the starting deck's job is putting bullets in the gun.</summary>
public sealed class Reload() : GunslingerCard(1, CardType.Skill, CardRarity.Basic, TargetType.Self)
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [new DynamicVar("Load", 2m)];

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [Tip(GunslingerTips.Load), Tip(GunslingerTips.TheCylinder)];

    protected override async Task OnPlay(PlayerChoiceContext ctx, CardPlay play)
    {
        await CreatureCmd.TriggerAnim(Owner.Creature, "Cast", Owner.Character.CastAnimDelay);
        await Revolver.Load(ctx, Gun, Rounds.Lead, DynamicVars["Load"].IntValue);
    }

    protected override void OnUpgrade() => DynamicVars["Load"].UpgradeValueBy(1m);
}

/// <summary>
/// Fire 1, for free.
///
/// Not to be confused with the uncommon <see cref="Quickdraw"/>, which pays off a Click. This is
/// the starter, and it is a blank trigger pull until you have loaded something.
/// </summary>
public sealed class QuickDraw() : GunslingerCard(0, CardType.Attack, CardRarity.Basic, TargetType.AnyEnemy)
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [new DynamicVar("Deadeye", 0m)];

    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
        [Tip(GunslingerTips.Fire), Tip(GunslingerTips.Click)];

    protected override async Task OnPlay(PlayerChoiceContext ctx, CardPlay play)
    {
        ArgumentNullException.ThrowIfNull(play.Target);

        await GunslingerEffects.GainDeadeye(ctx, Gun, DynamicVars["Deadeye"].BaseValue);
        await Revolver.Fire(ctx, Gun, play.Target);
    }

    protected override void OnUpgrade() => DynamicVars["Deadeye"].UpgradeValueBy(2m);
}
