using HelloSpire.HelloSpireCode.Gunslinger.Powers;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;

namespace HelloSpire.HelloSpireCode.Gunslinger.Cards;

// Rare Powers 19-25. Each turns one of the character's habits into an engine.

/// <summary>The first card you play each turn that Fires effectively costs 1 less.</summary>
public sealed class QuickdrawLegend() : GunslingerCard(2, CardType.Power, CardRarity.Rare, TargetType.Self)
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [new PowerVar<QuickdrawLegendPower>(1m)];

    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
        [Tip(GunslingerTips.Fire), HoverTipFactory.FromPower<QuickdrawLegendPower>()];

    protected override async Task OnPlay(PlayerChoiceContext ctx, CardPlay play)
    {
        await CreatureCmd.TriggerAnim(Owner.Creature, "Cast", Owner.Character.CastAnimDelay);
        await PowerCmd.Apply<QuickdrawLegendPower>(ctx, Owner.Creature, DynamicVars["QuickdrawLegendPower"].BaseValue, Owner.Creature, this);
    }

    protected override void OnUpgrade() => EnergyCost.UpgradeBy(-1);
}

/// <summary>A special Round loads itself every turn the cylinder has room.</summary>
public sealed class BottomlessBandolier() : GunslingerCard(2, CardType.Power, CardRarity.Rare, TargetType.Self)
{
    protected override IEnumerable<DynamicVar> CanonicalVars =>
        [new PowerVar<BottomlessBandolierPower>(1m), new DynamicVar("Deadeye", 0m)];

    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
        [Tip(GunslingerTips.Load), HoverTipFactory.FromPower<BottomlessBandolierPower>()];

    protected override async Task OnPlay(PlayerChoiceContext ctx, CardPlay play)
    {
        await CreatureCmd.TriggerAnim(Owner.Creature, "Cast", Owner.Character.CastAnimDelay);

        var power = await PowerCmd.Apply<BottomlessBandolierPower>(ctx, Owner.Creature, DynamicVars["BottomlessBandolierPower"].BaseValue, Owner.Creature, this);

        if (power != null) power.DeadeyeBonus = DynamicVars["Deadeye"].IntValue;
    }

    protected override void OnUpgrade() => DynamicVars["Deadeye"].UpgradeValueBy(2m);
}

/// <summary>After you Spin, Cycle — enough control to make gambling a plan.</summary>
public sealed class LoadedDice() : GunslingerCard(1, CardType.Power, CardRarity.Rare, TargetType.Self)
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [new PowerVar<LoadedDicePower>(1m)];

    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
        [Tip(GunslingerTips.Spin), Tip(GunslingerTips.Cycle), HoverTipFactory.FromPower<LoadedDicePower>()];

    protected override async Task OnPlay(PlayerChoiceContext ctx, CardPlay play)
    {
        await CreatureCmd.TriggerAnim(Owner.Creature, "Cast", Owner.Character.CastAnimDelay);
        await PowerCmd.Apply<LoadedDicePower>(ctx, Owner.Creature, DynamicVars["LoadedDicePower"].BaseValue, Owner.Creature, this);
    }

    protected override void OnUpgrade() => DynamicVars["LoadedDicePower"].UpgradeValueBy(1m);
}

/// <summary>The first time each turn Armor would decrease, it does not.</summary>
public sealed class IronWill() : GunslingerCard(2, CardType.Power, CardRarity.Rare, TargetType.Self)
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [new PowerVar<IronWillPower>(1m)];

    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
        [HoverTipFactory.FromPower<ArmorPower>(), HoverTipFactory.FromPower<IronWillPower>()];

    protected override async Task OnPlay(PlayerChoiceContext ctx, CardPlay play)
    {
        await CreatureCmd.TriggerAnim(Owner.Creature, "Cast", Owner.Character.CastAnimDelay);
        await PowerCmd.Apply<IronWillPower>(ctx, Owner.Creature, DynamicVars["IronWillPower"].BaseValue, Owner.Creature, this);
    }

    protected override void OnUpgrade() => EnergyCost.UpgradeBy(-1);
}

/// <summary>Whenever you gain Armor, gain Block as well.</summary>
public sealed class Untouchable() : GunslingerCard(2, CardType.Power, CardRarity.Rare, TargetType.Self)
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [new PowerVar<UntouchablePower>(1m)];

    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
        [HoverTipFactory.FromPower<ArmorPower>(), HoverTipFactory.FromPower<UntouchablePower>()];

    protected override async Task OnPlay(PlayerChoiceContext ctx, CardPlay play)
    {
        await CreatureCmd.TriggerAnim(Owner.Creature, "Cast", Owner.Character.CastAnimDelay);
        await PowerCmd.Apply<UntouchablePower>(ctx, Owner.Creature, DynamicVars["UntouchablePower"].BaseValue, Owner.Creature, this);
    }

    protected override void OnUpgrade() => DynamicVars["UntouchablePower"].UpgradeValueBy(1m);
}

/// <summary>The first Weak you apply each turn drags a Debilitate with it.</summary>
public sealed class DebilitatingPresence() : GunslingerCard(2, CardType.Power, CardRarity.Rare, TargetType.Self)
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [new PowerVar<DebilitatingPresencePower>(1m)];

    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
    [
        HoverTipFactory.FromPower<WeakPower>(),
        HoverTipFactory.FromPower<DebilitatePower>(),
        HoverTipFactory.FromPower<DebilitatingPresencePower>()
    ];

    protected override async Task OnPlay(PlayerChoiceContext ctx, CardPlay play)
    {
        await CreatureCmd.TriggerAnim(Owner.Creature, "Cast", Owner.Character.CastAnimDelay);
        await PowerCmd.Apply<DebilitatingPresencePower>(ctx, Owner.Creature, DynamicVars["DebilitatingPresencePower"].BaseValue, Owner.Creature, this);
    }

    protected override void OnUpgrade() => EnergyCost.UpgradeBy(-1);
}

/// <summary>Every 6th Round lands like a cannon and hands back an Energy.</summary>
public sealed class SixthShot() : GunslingerCard(3, CardType.Power, CardRarity.Rare, TargetType.Self)
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [new PowerVar<SixthShotPower>(15m)];

    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
        [Tip(GunslingerTips.Fire), HoverTipFactory.FromPower<SixthShotPower>(), EnergyHoverTip];

    protected override async Task OnPlay(PlayerChoiceContext ctx, CardPlay play)
    {
        await CreatureCmd.TriggerAnim(Owner.Creature, "Cast", Owner.Character.CastAnimDelay);
        await PowerCmd.Apply<SixthShotPower>(ctx, Owner.Creature, DynamicVars["SixthShotPower"].BaseValue, Owner.Creature, this);
    }

    protected override void OnUpgrade() => DynamicVars["SixthShotPower"].UpgradeValueBy(5m);
}
