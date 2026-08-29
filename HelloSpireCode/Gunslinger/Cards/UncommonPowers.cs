using HelloSpire.HelloSpireCode.Gunslinger.Powers;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;

namespace HelloSpire.HelloSpireCode.Gunslinger.Cards;

// Uncommon Powers 32-35. Each one attaches a payout to a verb the character already uses every
// turn — firing, spinning, or letting Armor take a hit.

/// <summary>Every 6th Round you Fire, draw.</summary>
public sealed class GunfightersRhythm() : GunslingerCard(1, CardType.Power, CardRarity.Uncommon, TargetType.Self)
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [new PowerVar<GunfightersRhythmPower>(1m)];

    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
        [Tip(GunslingerTips.Fire), HoverTipFactory.FromPower<GunfightersRhythmPower>()];

    protected override async Task OnPlay(PlayerChoiceContext ctx, CardPlay play)
    {
        await CreatureCmd.TriggerAnim(Owner.Creature, "Cast", Owner.Character.CastAnimDelay);
        await PowerCmd.Apply<GunfightersRhythmPower>(ctx, Owner.Creature, DynamicVars["GunfightersRhythmPower"].BaseValue, Owner.Creature, this);
    }

    protected override void OnUpgrade() => DynamicVars["GunfightersRhythmPower"].UpgradeValueBy(1m);
}

/// <summary>The first time each turn Armor prevents damage, gain Block next turn.</summary>
public sealed class HardLeather() : GunslingerCard(1, CardType.Power, CardRarity.Uncommon, TargetType.Self)
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [new PowerVar<HardLeatherPower>(3m)];

    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
        [HoverTipFactory.FromPower<ArmorPower>(), HoverTipFactory.FromPower<HardLeatherPower>()];

    protected override async Task OnPlay(PlayerChoiceContext ctx, CardPlay play)
    {
        await CreatureCmd.TriggerAnim(Owner.Creature, "Cast", Owner.Character.CastAnimDelay);
        await PowerCmd.Apply<HardLeatherPower>(ctx, Owner.Creature, DynamicVars["HardLeatherPower"].BaseValue, Owner.Creature, this);
    }

    protected override void OnUpgrade() => DynamicVars["HardLeatherPower"].UpgradeValueBy(2m);
}

/// <summary>The first time each turn you Fire a Round, gain Block.</summary>
public sealed class SmokeAndLead() : GunslingerCard(1, CardType.Power, CardRarity.Uncommon, TargetType.Self)
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [new PowerVar<SmokeAndLeadPower>(3m)];

    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
        [Tip(GunslingerTips.Fire), HoverTipFactory.FromPower<SmokeAndLeadPower>()];

    protected override async Task OnPlay(PlayerChoiceContext ctx, CardPlay play)
    {
        await CreatureCmd.TriggerAnim(Owner.Creature, "Cast", Owner.Character.CastAnimDelay);
        await PowerCmd.Apply<SmokeAndLeadPower>(ctx, Owner.Creature, DynamicVars["SmokeAndLeadPower"].BaseValue, Owner.Creature, this);
    }

    protected override void OnUpgrade() => DynamicVars["SmokeAndLeadPower"].UpgradeValueBy(1m);
}

/// <summary>The first time each turn you Spin, gain Deadeye.</summary>
public sealed class SureHand() : GunslingerCard(1, CardType.Power, CardRarity.Uncommon, TargetType.Self)
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [new PowerVar<SureHandPower>(4m)];

    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
    [
        Tip(GunslingerTips.Spin),
        HoverTipFactory.FromPower<DeadeyePower>(),
        HoverTipFactory.FromPower<SureHandPower>()
    ];

    protected override async Task OnPlay(PlayerChoiceContext ctx, CardPlay play)
    {
        await CreatureCmd.TriggerAnim(Owner.Creature, "Cast", Owner.Character.CastAnimDelay);
        await PowerCmd.Apply<SureHandPower>(ctx, Owner.Creature, DynamicVars["SureHandPower"].BaseValue, Owner.Creature, this);
    }

    protected override void OnUpgrade() => DynamicVars["SureHandPower"].UpgradeValueBy(2m);
}
