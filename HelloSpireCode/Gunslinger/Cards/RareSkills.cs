using HelloSpire.HelloSpireCode.Gunslinger.Cylinder;
using HelloSpire.HelloSpireCode.Gunslinger.Powers;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;

namespace HelloSpire.HelloSpireCode.Gunslinger.Cards;

// Rare Skills 10-18: total control over the cylinder at one end, and a genuine coin-flip at the other.

/// <summary>
/// Chamber a Dead Man's Round, Spin, and pull the trigger on yourself.
///
/// One in six says the chamber it landed in was the loaded one. The rest of the time you are up an
/// Energy and two cards, with a 24-damage Round still somewhere in the gun.
/// </summary>
public sealed class RussianRoulette() : GunslingerCard(0, CardType.Skill, CardRarity.Rare, TargetType.Self)
{
    protected override IEnumerable<DynamicVar> CanonicalVars =>
        [new DynamicVar("RoundDamage", 24m), new EnergyVar(1), new CardsVar(2)];

    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];

    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
        [Tip(GunslingerTips.Load), Tip(GunslingerTips.Spin), Tip(GunslingerTips.SelfFire), Tip(GunslingerTips.Click)];

    protected override async Task OnPlay(PlayerChoiceContext ctx, CardPlay play)
    {
        await Revolver.LoadRandomEmpty(ctx, Gun, Rounds.DeadMans(DynamicVars["RoundDamage"].IntValue));
        await Revolver.Spin(ctx, Gun);

        var result = await Revolver.SelfFire(ctx, Gun);
        if (!result.WasClick) return;

        await GunslingerEffects.GainEnergy(Gun, DynamicVars.Energy.BaseValue);
        await GunslingerEffects.Draw(ctx, Gun, DynamicVars.Cards.IntValue);
    }

    protected override void OnUpgrade() => DynamicVars["RoundDamage"].UpgradeValueBy(6m);
}

/// <summary>Pack the cylinder into one uninterrupted run of Rounds, heaviest first.</summary>
public sealed class StackTheCylinder() : GunslingerCard(1, CardType.Skill, CardRarity.Rare, TargetType.Self)
{
    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [Tip(GunslingerTips.TheCylinder)];

    protected override async Task OnPlay(PlayerChoiceContext ctx, CardPlay play) =>
        await Revolver.StackForBurst(ctx, Gun);

    protected override void OnUpgrade() => EnergyCost.UpgradeBy(-1);
}

/// <summary>
/// Fill the gun with whichever ammunition the fight is asking for.
///
/// As with Custom Load, the choice is made from the board rather than from a menu: Piercing into
/// Block, Guard into an incoming Attack, Crippling when nothing is Weak, Heavy otherwise.
/// </summary>
public sealed class PerfectReload() : GunslingerCard(2, CardType.Skill, CardRarity.Rare, TargetType.Self)
{
    public override bool GainsBlock => true;

    protected override IEnumerable<DynamicVar> CanonicalVars => [new BlockVar(5m, ValueProp.Move)];

    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [Tip(GunslingerTips.Load)];

    protected override async Task OnPlay(PlayerChoiceContext ctx, CardPlay play)
    {
        await Revolver.FillEmpty(ctx, Gun, PickAmmunition());
        await CreatureCmd.GainBlock(Owner.Creature, DynamicVars.Block, play);
    }

    private Func<Round> PickAmmunition()
    {
        var enemies = GunslingerEffects.Enemies(Gun);
        if (enemies.Any(enemy => enemy.Block > 0m)) return Rounds.Piercing;
        if (enemies.Any(GunslingerEffects.IntendsToAttack)) return Rounds.Guard;
        if (enemies.Count > 0 && !enemies.Any(enemy => enemy.HasPower<WeakPower>())) return Rounds.Crippling;
        return Rounds.Heavy;
    }

    protected override void OnUpgrade() => EnergyCost.UpgradeBy(-1);
}

/// <summary>
/// Gain 1 Intangible: a whole enemy turn reduced to a scratch.
///
/// This is the character's premium defence and, with the Ghost Smoke potion, one of only two
/// sources of it in the pack. It used to read "gain 2 Dodge" — two individual hits prevented,
/// gone at the start of your next turn — and the whole Dodge keyword existed to make that
/// countable. Intangible is the base game's own word for the same idea and needs no keyword, no
/// per-hit bookkeeping and no branch in the damage patch, which is the entire reason for the
/// swap.
///
/// One stack, not two, and the upgrade buys the Energy rather than a second stack. Intangible is
/// worth several times what a Dodge was, so the card is priced as a rare panic button that
/// answers one enemy turn completely, not as something an engine hands out.
/// </summary>
public sealed class GhostStep() : GunslingerCard(1, CardType.Skill, CardRarity.Rare, TargetType.Self)
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [new PowerVar<IntangiblePower>(1m)];

    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.FromPower<IntangiblePower>()];

    protected override async Task OnPlay(PlayerChoiceContext ctx, CardPlay play) =>
        await GunslingerEffects.GainIntangible(ctx, Gun, DynamicVars["IntangiblePower"].BaseValue);

    protected override void OnUpgrade() => EnergyCost.UpgradeBy(-1);
}

/// <summary>Gain a wall of Armor that lasts as long as the fight does.</summary>
public sealed class ArmoredLongcoat() : GunslingerCard(2, CardType.Power, CardRarity.Rare, TargetType.Self)
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [new PowerVar<ArmorPower>(5m)];

    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.FromPower<ArmorPower>()];

    protected override async Task OnPlay(PlayerChoiceContext ctx, CardPlay play) =>
        await GunslingerEffects.GainArmor(ctx, Gun, DynamicVars["ArmorPower"].BaseValue);

    protected override void OnUpgrade() => DynamicVars["ArmorPower"].UpgradeValueBy(2m);
}

/// <summary>
/// Cover this turn, and come back next turn with the tempo to do something about it.
///
/// The card's shape has not changed — buy the enemy turn, get paid at the start of yours — only
/// the currency has. It used to spend a Dodge on the buying half, and there is no Dodge any more;
/// Block does the same job here without pulling Intangible, which is Rare-and-scarce by design,
/// into a card whose real payload is the Energy.
/// </summary>
public sealed class NeverStill() : GunslingerCard(1, CardType.Skill, CardRarity.Rare, TargetType.Self), IGadget
{
    public override bool GainsBlock => true;

    protected override IEnumerable<DynamicVar> CanonicalVars =>
        [new BlockVar(8m, ValueProp.Move), new EnergyVar(1), new CardsVar(1)];

    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [EnergyHoverTip, Tip(GunslingerTips.Gadget)];

    protected override async Task OnPlay(PlayerChoiceContext ctx, CardPlay play)
    {
        await CreatureCmd.GainBlock(Owner.Creature, DynamicVars.Block, play);

        var power = await PowerCmd.Apply<NeverStillPower>(ctx, Owner.Creature, DynamicVars.Energy.BaseValue, Owner.Creature, this);

        if (power != null) power.CardsToDraw = DynamicVars.Cards.IntValue;
    }

    protected override void OnUpgrade() => DynamicVars.Cards.UpgradeValueBy(1m);
}

/// <summary>Enough Deadeye that the next Round hardly matters — only that it lands.</summary>
public sealed class DeadeyeFocus() : GunslingerCard(1, CardType.Skill, CardRarity.Rare, TargetType.Self)
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [new PowerVar<DeadeyePower>(12m)];

    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.FromPower<DeadeyePower>()];

    protected override async Task OnPlay(PlayerChoiceContext ctx, CardPlay play) =>
        await GunslingerEffects.GainDeadeye(ctx, Gun, DynamicVars["DeadeyePower"].BaseValue);

    protected override void OnUpgrade() => DynamicVars["DeadeyePower"].UpgradeValueBy(4m);
}

/// <summary>Read the cylinder: a loaded chamber comes up next and pays cards, an empty one buys cover.</summary>
public sealed class SixthSense() : GunslingerCard(1, CardType.Skill, CardRarity.Rare, TargetType.Self)
{
    protected override IEnumerable<DynamicVar> CanonicalVars =>
        [new CardsVar(2), new PowerVar<ArmorPower>(1m), new BlockVar(0m, ValueProp.Move)];

    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];

    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
        [Tip(GunslingerTips.TheCylinder), HoverTipFactory.FromPower<ArmorPower>()];

    protected override async Task OnPlay(PlayerChoiceContext ctx, CardPlay play)
    {
        if (await Revolver.MoveBestLoadedUnderHammer(ctx, Gun))
        {
            await GunslingerEffects.Draw(ctx, Gun, DynamicVars.Cards.IntValue);
            return;
        }

        await GunslingerEffects.GainArmor(ctx, Gun, DynamicVars["ArmorPower"].BaseValue);
        await GunslingerEffects.GainBlock(Gun, DynamicVars.Block.BaseValue);
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Cards.UpgradeValueBy(1m);
        DynamicVars.Block.UpgradeValueBy(5m);
    }
}

/// <summary>
/// Free, and the gun picks the ammunition. The purest expression of the character's bargain:
/// you get a chamber for nothing and give up all say in what is in it.
/// </summary>
public sealed class LuckyShot() : GunslingerCard(0, CardType.Skill, CardRarity.Rare, TargetType.Self)
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [new DynamicVar("Wild", 1m)];

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [Tip(GunslingerTips.Load)];

    protected override async Task OnPlay(PlayerChoiceContext ctx, CardPlay play)
    {
        // Rolled once per Round, so an upgraded copy is two different surprises rather than a pair.
        for (var i = 0; i < DynamicVars["Wild"].IntValue; i++)
            await Revolver.Load(ctx, Gun, Rounds.RandomOrdinary(Gun), 1);
    }

    protected override void OnUpgrade() => DynamicVars["Wild"].UpgradeValueBy(1m);
}

/// <summary>Load Rending Rounds — the Gunslinger's only repeatable source of Debilitate.</summary>
public sealed class RendingCartridge() : GunslingerCard(1, CardType.Skill, CardRarity.Rare, TargetType.Self)
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [new DynamicVar("Load", 2m)];

    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];

    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
        [Tip(GunslingerTips.Load), HoverTipFactory.FromPower<DebilitatePower>()];

    protected override async Task OnPlay(PlayerChoiceContext ctx, CardPlay play) =>
        await Revolver.Load(ctx, Gun, Rounds.Rending, DynamicVars["Load"].IntValue);

    protected override void OnUpgrade() => DynamicVars["Load"].UpgradeValueBy(1m);
}
