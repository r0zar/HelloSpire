using HelloSpire.HelloSpireCode.Gunslinger.Powers;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;

namespace HelloSpire.HelloSpireCode.Gunslinger.Cards;

// The Gadgets: everything on the Gunslinger's belt that is not the gun.
//
// The character shipped with exactly one axis — put Rounds in, take Rounds out — and every card
// in the set sat somewhere on it. A hand with no ammunition card and no Fire card did nothing at
// all, and a draft that missed the cartridge commons had no second plan to fall back on. These
// are the second plan: debuffs, Block and Armor, priced so that a deck holding a dozen of them
// wants the gun for damage rather than for existence.
//
// Every card here implements <see cref="IGadget"/>, which is the whole of the mechanism -- see
// that interface for why it carries no members. The rule it stands for is the one on the card:
// no Load, no Fire, no Cycle, no Spin. Reading state the cylinder happens to own is fine; a
// Gadget that touches a chamber is a cartridge card with the wrong word on it.
//
// The retagged half of the archetype lives with its rarity: Pistol Whip, Shoulder Shot, Gut Shot,
// Warning Shot and Pocket Sand in Commons.cs; Cold Read, Under the Duster, Grit Teeth and Duck
// and Weave in UncommonSkills.cs; Never Still in RareSkills.cs. New cards land here so the shape
// of the package can be read in one file.

// ------------------------------------------------------------------ commons

/// <summary>
/// Apply Weak to ALL enemies and gain a little Block.
///
/// The archetype's opener and its answer to a crowd. Pocket Sand is two Weak on one target for
/// the same Energy, so the two commons split cleanly: one shuts a single big attacker down, this
/// one takes the edge off a room. The Block is deliberately small -- this is a debuff card that
/// covers, not a Defend that debuffs.
///
/// Named around the Alchemist's Flash Powder rather than beside it: card art and localization
/// both resolve by bare class name across the whole mod, so two classes sharing one name silently
/// share a portrait and a string-table key.
/// </summary>
public sealed class BlindingPowder() : GunslingerCard(1, CardType.Skill, CardRarity.Common, TargetType.AllEnemies), IGadget
{
    public override bool GainsBlock => true;

    protected override IEnumerable<DynamicVar> CanonicalVars =>
        [new PowerVar<WeakPower>(1m), new BlockVar(3m, ValueProp.Move)];

    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
        [HoverTipFactory.FromPower<WeakPower>(), Tip(GunslingerTips.Gadget)];

    protected override async Task OnPlay(PlayerChoiceContext ctx, CardPlay play)
    {
        await CreatureCmd.TriggerAnim(Owner.Creature, "Cast", Owner.Character.CastAnimDelay);

        foreach (var enemy in GunslingerEffects.Enemies(Gun))
            await GunslingerEffects.ApplyWeak(ctx, Gun, enemy, DynamicVars["WeakPower"].BaseValue);

        await CreatureCmd.GainBlock(Owner.Creature, DynamicVars.Block, play);
    }

    protected override void OnUpgrade() => DynamicVars.Block.UpgradeValueBy(3m);
}

/// <summary>
/// Deal damage and apply Weak; more damage while you are wearing Armor.
///
/// The common that says out loud what the archetype is for. Armor is the Gunslinger's slow
/// defensive layer and nothing in the set ever paid you for holding it -- it just sat there
/// eroding. Here it is a damage stat, which turns Under the Duster and Grit Teeth from cards you
/// play when you are losing into cards you play on the way in.
///
/// The bonus is a flat step rather than a per-point scale on purpose: Armor stacks are small
/// numbers that decay, and a card whose damage swings with them would be unreadable in hand.
/// </summary>
public sealed class BearTrap() : GunslingerCard(1, CardType.Attack, CardRarity.Common, TargetType.AnyEnemy), IGadget
{
    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DamageVar(6m, ValueProp.Move), new DamageVar("Bonus", 4m, ValueProp.Move),
        new PowerVar<WeakPower>(1m)
    ];

    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
        [HoverTipFactory.FromPower<ArmorPower>(), HoverTipFactory.FromPower<WeakPower>(),
         Tip(GunslingerTips.Gadget)];

    protected override bool ShouldGlowGoldInternal =>
        CombatState != null && Owner.Creature.GetPowerAmount<ArmorPower>() > 0m;

    protected override async Task OnPlay(PlayerChoiceContext ctx, CardPlay play)
    {
        ArgumentNullException.ThrowIfNull(play.Target);

        var damage = DynamicVars.Damage.BaseValue;
        if (Owner.Creature.GetPowerAmount<ArmorPower>() > 0m) damage += DynamicVars["Bonus"].BaseValue;

        await DamageCmd.Attack(damage).FromCard(this).Targeting(play.Target).Execute(ctx);
        await GunslingerEffects.ApplyWeak(ctx, Gun, play.Target, DynamicVars["WeakPower"].BaseValue);
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Damage.UpgradeValueBy(3m);
        DynamicVars["Bonus"].UpgradeValueBy(2m);
    }
}

/// <summary>
/// Free: apply Weak and gain 1 Armor, then Exhaust.
///
/// The cheapest Gadget in the set, and the one that makes the payoff Powers worth playing. Every
/// engine in this package counts Gadgets rather than measuring them, so a zero-cost one that
/// still does two real things is the card that turns Tinker's Kit and Gadgeteer from two-card
/// combos into an engine. It Exhausts so it cannot be looped into an infinite Armor supply.
/// </summary>
public sealed class Tripwire() : GunslingerCard(0, CardType.Skill, CardRarity.Common, TargetType.AnyEnemy), IGadget
{
    protected override IEnumerable<DynamicVar> CanonicalVars =>
        [new PowerVar<WeakPower>(1m), new PowerVar<ArmorPower>(1m)];

    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];

    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
        [HoverTipFactory.FromPower<WeakPower>(), HoverTipFactory.FromPower<ArmorPower>(),
         Tip(GunslingerTips.Gadget)];

    protected override async Task OnPlay(PlayerChoiceContext ctx, CardPlay play)
    {
        ArgumentNullException.ThrowIfNull(play.Target);

        await GunslingerEffects.ApplyWeak(ctx, Gun, play.Target, DynamicVars["WeakPower"].BaseValue);
        await GunslingerEffects.GainArmor(ctx, Gun, DynamicVars["ArmorPower"].BaseValue);
    }

    protected override void OnUpgrade() => DynamicVars["ArmorPower"].UpgradeValueBy(1m);
}

// ------------------------------------------------------------------ uncommons

/// <summary>
/// Gain Block, and Weak the whole room.
///
/// Blinding Powder grown up: the same shape at a rate that holds a turn together on its own. It is
/// the Gadget deck's Defend, and against three attackers the Weak is worth more than the Block.
/// </summary>
public sealed class SmokeBomb() : GunslingerCard(1, CardType.Skill, CardRarity.Uncommon, TargetType.AllEnemies), IGadget
{
    public override bool GainsBlock => true;

    protected override IEnumerable<DynamicVar> CanonicalVars =>
        [new BlockVar(6m, ValueProp.Move), new PowerVar<WeakPower>(1m)];

    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
        [HoverTipFactory.FromPower<WeakPower>(), Tip(GunslingerTips.Gadget)];

    protected override async Task OnPlay(PlayerChoiceContext ctx, CardPlay play)
    {
        await CreatureCmd.TriggerAnim(Owner.Creature, "Cast", Owner.Character.CastAnimDelay);
        await CreatureCmd.GainBlock(Owner.Creature, DynamicVars.Block, play);

        foreach (var enemy in GunslingerEffects.Enemies(Gun))
            await GunslingerEffects.ApplyWeak(ctx, Gun, enemy, DynamicVars["WeakPower"].BaseValue);
    }

    protected override void OnUpgrade() => DynamicVars.Block.UpgradeValueBy(3m);
}

/// <summary>
/// Gain Armor and draw a card.
///
/// The card that keeps a Gadget deck moving. Armor decks stall because their defensive cards are
/// all terminal -- they gain a layer and hand the turn back -- so the deck runs out of cards long
/// before it runs out of Energy. This is the cantrip that fixes that, and the reason the
/// archetype can carry as many one-off Gadgets as it does.
/// </summary>
public sealed class FieldKit() : GunslingerCard(1, CardType.Skill, CardRarity.Uncommon, TargetType.Self), IGadget
{
    protected override IEnumerable<DynamicVar> CanonicalVars =>
        [new PowerVar<ArmorPower>(2m), new CardsVar(1)];

    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
        [HoverTipFactory.FromPower<ArmorPower>(), Tip(GunslingerTips.Gadget)];

    protected override async Task OnPlay(PlayerChoiceContext ctx, CardPlay play)
    {
        await GunslingerEffects.GainArmor(ctx, Gun, DynamicVars["ArmorPower"].BaseValue);
        await GunslingerEffects.Draw(ctx, Gun, DynamicVars.Cards.IntValue);
    }

    protected override void OnUpgrade() => DynamicVars.Cards.UpgradeValueBy(1m);
}

/// <summary>
/// Deal damage to ALL enemies and Weak them all.
///
/// The gap this fills is the one that made a gun-less draft unplayable: the Gunslinger's only
/// real room-clear is No Witnesses, which is Rare and needs a loaded chamber. A Gadget deck with
/// no answer to three enemies is not a deck, so this is the archetype's Act 2 card -- expensive,
/// unconditional, and worth the same on an empty cylinder as a full one.
/// </summary>
public sealed class ScattergunShell() : GunslingerCard(2, CardType.Attack, CardRarity.Uncommon, TargetType.AllEnemies), IGadget
{
    protected override IEnumerable<DynamicVar> CanonicalVars =>
        [new DamageVar(8m, ValueProp.Move), new PowerVar<WeakPower>(1m)];

    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
        [HoverTipFactory.FromPower<WeakPower>(), Tip(GunslingerTips.Gadget)];

    protected override async Task OnPlay(PlayerChoiceContext ctx, CardPlay play)
    {
        foreach (var enemy in GunslingerEffects.Enemies(Gun))
        {
            await DamageCmd.Attack(DynamicVars.Damage.BaseValue).FromCard(this).Targeting(enemy).Execute(ctx);
        }

        // Applied in a second pass: anything that died to the damage above is no longer hittable,
        // and Enemies() is re-read so a corpse never takes a debuff.
        foreach (var enemy in GunslingerEffects.Enemies(Gun))
            await GunslingerEffects.ApplyWeak(ctx, Gun, enemy, DynamicVars["WeakPower"].BaseValue);
    }

    protected override void OnUpgrade() => DynamicVars.Damage.UpgradeValueBy(3m);
}

/// <summary>
/// The first Gadget you play each turn draws a card.
///
/// The Uncommon engine, and the cheap one. It pays out once a turn rather than per Gadget, which
/// keeps it honest next to a zero-cost Tripwire and makes it a card the gun deck can also want --
/// Pistol Whip and Shoulder Shot are Gadgets, and both are in every deck.
/// </summary>
public sealed class TinkersKit() : GunslingerCard(1, CardType.Power, CardRarity.Uncommon, TargetType.Self)
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [new PowerVar<TinkersKitPower>(1m)];

    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
        [HoverTipFactory.FromPower<TinkersKitPower>(), Tip(GunslingerTips.Gadget)];

    // Not itself a Gadget: a Power that counted its own play would make the very first trigger an
    // ordering question, and the card reads better as the thing that watches Gadgets than as one.

    protected override async Task OnPlay(PlayerChoiceContext ctx, CardPlay play)
    {
        await CreatureCmd.TriggerAnim(Owner.Creature, "Cast", Owner.Character.CastAnimDelay);
        await PowerCmd.Apply<TinkersKitPower>(ctx, Owner.Creature,
            DynamicVars["TinkersKitPower"].BaseValue, Owner.Creature, this);
    }

    protected override void OnUpgrade() => DynamicVars["TinkersKitPower"].UpgradeValueBy(1m);
}

// ------------------------------------------------------------------ rares

/// <summary>
/// Whenever you play a Gadget, gain Armor.
///
/// The archetype's capstone, and the card that makes a Gadget count worth building towards. Every
/// other Armor source in the set is a card that does nothing else; this one rides on cards you
/// were playing anyway, which is what turns a pile of debuff commons into a defensive engine.
///
/// Armor rather than Block because Armor is the layer that survives the turn. A deck that plays
/// four Gadgets a turn under this is holding a wall by turn three, which is a real win condition
/// for a character who otherwise only has one.
/// </summary>
public sealed class Gadgeteer() : GunslingerCard(2, CardType.Power, CardRarity.Rare, TargetType.Self)
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [new PowerVar<GadgeteerPower>(1m)];

    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
        [HoverTipFactory.FromPower<GadgeteerPower>(), HoverTipFactory.FromPower<ArmorPower>(),
         Tip(GunslingerTips.Gadget)];

    // Not itself a Gadget -- see Tinker's Kit.

    protected override async Task OnPlay(PlayerChoiceContext ctx, CardPlay play)
    {
        await CreatureCmd.TriggerAnim(Owner.Creature, "Cast", Owner.Character.CastAnimDelay);
        await PowerCmd.Apply<GadgeteerPower>(ctx, Owner.Creature,
            DynamicVars["GadgeteerPower"].BaseValue, Owner.Creature, this);
    }

    protected override void OnUpgrade() => DynamicVars["GadgeteerPower"].UpgradeValueBy(1m);
}
