using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BaseLib.Utils;
using Godot;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Potions;
using MegaCrit.Sts2.Core.Factories;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.CardPools;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.Nodes.Rooms;
using MegaCrit.Sts2.Core.Nodes.Vfx;
using MegaCrit.Sts2.Core.ValueProps;
using HelloSpire.HelloSpireCode.Alchemist.Lab;
using HelloSpire.HelloSpireCode.Extensions;

namespace HelloSpire.HelloSpireCode.Alchemist.Potions;

/// <summary>
/// Base for the Alchemist's Volatile Potions: joins the real AlchemistPotionPool, same as
/// <see cref="Characters.AlchemistPotion"/>.
///
/// A dedicated "inert" pool was tried first and doesn't work: PotionModel.Pool (needed for
/// tooltips and, apparently, whether the game will let you click Use at all -- confirmed live,
/// both broke) resolves via <c>ModelDb.AllPotionPools.First(pool => pool.AllPotionIds.Contains(Id))</c>,
/// and ModelDb.AllPotionPools (decompiled from sts2.dll) is hardcoded to exactly each Character's
/// own PotionPool plus five specific base-game shared pools -- it does not auto-discover arbitrary
/// custom PotionPoolModel subclasses a mod defines, no matter how they're tagged. A pool nothing in
/// that list points at is unreachable, and any potion registered to it throws "Sequence contains no
/// matching element" the moment anything asks for its Pool.
///
/// So these have to be real members of AlchemistPotionPool. Keeping them out of shops and reward
/// screens is instead handled by <see cref="HideVolatilePotionsFromShopsAndRewardsPatch"/>, which
/// blacklists every VolatileCommonPotion from the one method (PotionFactory.
/// CreateRandomPotionsOutOfCombat) that actually generates a random potion outside combat.
/// </summary>
[Pool(typeof(HelloSpire.HelloSpireCode.Characters.AlchemistPotionPool))]
public abstract class VolatileCommonPotion : BaseLib.Abstracts.CustomPotionModel
{
    public override PotionRarity Rarity => PotionRarity.Common;
    public override PotionUsage Usage => PotionUsage.CombatOnly;

    protected LabContext Lab => LabContext.From(Owner);

    /// <summary>
    /// Potency's raw value if this instance is currently Volatile-tracked, else 0 -- same gate
    /// Belt.PotencyBonus already enforces for the Damage/Block potions PotionUsePatch bumps
    /// automatically. Every potion in this file that isn't Damage/Block-shaped reads this directly
    /// in its own OnUse instead, since each one scales Potency at its own rate.
    /// </summary>
    protected int Potency => Belt.PotencyBonus(Lab, this);
}

/// <summary>
/// Keeps every VolatileCommonPotion, plus the real Poison Ampoule, out of shops and reward screens
/// without needing a separate, unreachable pool (see the doc comment on
/// <see cref="VolatileCommonPotion"/> for why that doesn't work). PotionFactory.
/// CreateRandomPotionsOutOfCombat (decompiled from sts2.dll) is the sole general-purpose "give the
/// player a random potion outside combat" entry point and already accepts a blacklist parameter for
/// exactly this purpose; combat generation is untouched, since WiredLabBridge's own
/// RandomCombatPotion/CombatPotionOptions never call this method at all -- those two are where
/// Poison Ampoule's combat-side exclusion lives instead.
/// </summary>
[HarmonyLib.HarmonyPatch(typeof(PotionFactory), nameof(PotionFactory.CreateRandomPotionsOutOfCombat))]
internal static class HideVolatilePotionsFromShopsAndRewardsPatch
{
    [HarmonyLib.HarmonyPrefix]
    private static void BeforeCreate(ref IEnumerable<PotionModel>? blacklist)
    {
        blacklist = (blacklist ?? [])
            .Concat(ModelDb.AllPotions.Where(p => p is VolatileCommonPotion or UnstableConcoction or PoisonAmpoule or ResidualReagent));
    }
}

/// <summary>
/// Weaker Volatile counterparts of the Common Potions in the Alchemist's combat pool (see
/// WiredLabBridge.RandomCombatPotion/CombatPotionOptions/NamedPotion). Every OnUse below is ported
/// from the real class, decompiled from sts2.dll, at a reduced value -- Volatile Vulnerable
/// Potion's 2 Vulnerable (vs. the real one's 3) is the case that surfaced this file; the rest
/// follow the same "weaker than what a shop would sell you" rule, using ChatGPT's originally
/// proposed numbers where they map onto a potion that actually exists in this game's pool, and an
/// equivalent reduction (3 card choices -> 2) for the four card-generation Commons ChatGPT's table
/// never covered.
///
/// Volatile Poison Potion is the one exception: there is no real vanilla Poison Potion to
/// decompile, so it copies the Alchemist's own bespoke <see cref="PoisonPotion"/>
/// (AlchemistPotions.cs) instead, at the same reduced-value rule.
///
/// Potency scales every one of these, not just the Damage/Block-shaped ones PotionUsePatch bumps
/// automatically -- each potion reads the base class's <see cref="VolatileCommonPotion.Potency"/>
/// in its own OnUse at its own rate: Poison/Speed/Flex add it 1:1 (and start from a higher base of
/// 3 to make that worthwhile), Weak/Vulnerable/Strength/Dexterity/Swift/Energy add one extra point
/// per 3 Potency, and the four card-generation potions (Attack/Colorless/Power/Skill) offer one
/// extra card choice per 3 Potency instead of a numeric bonus, capped at 4 choices total.
///
/// CustomPackedImagePath points each one at the REAL vanilla potion's own sprite -- confirmed via
/// sts2.dll (ImageHelper.GetImagePath resolves a vanilla potion's Id.Entry, e.g. "vulnerable_potion",
/// to exactly "res://images/atlases/potion_atlas.sprites/vulnerable_potion.tres") -- rather than
/// shipping 15 duplicate PNGs. BaseLib's CustomPotionModel accepts any Texture2D-loadable path
/// here, base-game asset or not, so this is the same sprite the real potion uses, not a copy.
/// </summary>
public sealed class VolatileAttackPotion : VolatileCommonPotion
{
    public override TargetType TargetType => TargetType.Self;
    public override string? CustomPackedImagePath => "res://images/atlases/potion_atlas.sprites/attack_potion.tres";

    protected override async Task OnUse(PlayerChoiceContext ctx, Creature? target)
    {
        var count = Math.Min(4, 1 + Potency / 3);
        var cards = CardFactory.GetDistinctForCombat(Owner, Owner.Character.CardPool
            .GetUnlockedCards(Owner.UnlockState, Owner.RunState.CardMultiplayerConstraint)
            .Where(c => c.Type == CardType.Attack), count, Owner.RunState.Rng.CombatCardGeneration).ToList();
        var chosen = await CardSelectCmd.FromChooseACardScreen(ctx, cards, Owner, canSkip: true);
        if (chosen == null) return;
        chosen.SetToFreeThisTurn();
        await CardPileCmd.AddGeneratedCardToCombat(chosen, PileType.Hand, Owner);
    }
}

public sealed class VolatileBlockPotion : VolatileCommonPotion
{
    public override TargetType TargetType => TargetType.AnyPlayer;
    public override string? CustomPackedImagePath => "res://images/atlases/potion_atlas.sprites/block_potion.tres";

    protected override IEnumerable<DynamicVar> CanonicalVars => [new BlockVar(8m, ValueProp.Unpowered)];
    public override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.Static(StaticHoverTip.Block)];

    protected override async Task OnUse(PlayerChoiceContext ctx, Creature? target)
    {
        PotionModel.AssertValidForTargetedPotion(target);
        await CreatureCmd.GainBlock(target, DynamicVars.Block, null);
    }
}

public sealed class VolatileColorlessPotion : VolatileCommonPotion
{
    public override TargetType TargetType => TargetType.Self;
    public override string? CustomPackedImagePath => "res://images/atlases/potion_atlas.sprites/colorless_potion.tres";

    protected override async Task OnUse(PlayerChoiceContext ctx, Creature? target)
    {
        var count = Math.Min(4, 1 + Potency / 3);
        var cards = CardFactory.GetDistinctForCombat(Owner,
            ModelDb.CardPool<ColorlessCardPool>().GetUnlockedCards(Owner.UnlockState, Owner.RunState.CardMultiplayerConstraint),
            count, Owner.RunState.Rng.CombatCardGeneration).ToList();
        var chosen = await CardSelectCmd.FromChooseACardScreen(ctx, cards, Owner, canSkip: true);
        if (chosen == null) return;
        chosen.SetToFreeThisTurn();
        await CardPileCmd.AddGeneratedCardToCombat(chosen, PileType.Hand, Owner);
    }
}

public sealed class VolatileDexterityPotion : VolatileCommonPotion
{
    public override TargetType TargetType => TargetType.AnyPlayer;
    public override string? CustomPackedImagePath => "res://images/atlases/potion_atlas.sprites/dexterity_potion.tres";

    protected override IEnumerable<DynamicVar> CanonicalVars => [new PowerVar<DexterityPower>(1m)];
    public override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.FromPower<DexterityPower>()];

    protected override async Task OnUse(PlayerChoiceContext ctx, Creature? target)
    {
        PotionModel.AssertValidForTargetedPotion(target);
        var amount = DynamicVars.Dexterity.BaseValue + Potency / 3;
        await PowerCmd.Apply<DexterityPower>(ctx, target, amount, Owner.Creature, null);
    }
}

public sealed class VolatileEnergyPotion : VolatileCommonPotion
{
    public override TargetType TargetType => TargetType.AnyPlayer;
    public override string? CustomPackedImagePath => "res://images/atlases/potion_atlas.sprites/energy_potion.tres";

    protected override IEnumerable<DynamicVar> CanonicalVars => [new EnergyVar(1)];
    public override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.ForEnergy(this)];

    protected override async Task OnUse(PlayerChoiceContext ctx, Creature? target)
    {
        PotionModel.AssertValidForTargetedPotion(target);
        NCombatRoom.Instance?.PlaySplashVfx(target, new Color("f2e35c"));
        var energy = DynamicVars.Energy.BaseValue + Potency / 3;
        await PlayerCmd.GainEnergy(energy, target.Player);
    }
}

public sealed class VolatileExplosiveAmpoule : VolatileCommonPotion
{
    public override TargetType TargetType => TargetType.AllEnemies;
    public override string? CustomPackedImagePath => "res://images/atlases/potion_atlas.sprites/explosive_ampoule.tres";

    protected override IEnumerable<DynamicVar> CanonicalVars => [new DamageVar(6m, ValueProp.Unpowered)];

    protected override async Task OnUse(PlayerChoiceContext ctx, Creature? target)
    {
        var player = Owner.Creature;
        var damage = DynamicVars.Damage;
        var targets = player.CombatState.HittableEnemies;
        foreach (var enemy in targets)
            NCombatRoom.Instance?.CombatVfxContainer.AddChildSafely(NFireSmokePuffVfx.Create(enemy));
        await Cmd.CustomScaledWait(0.2f, 0.3f);
        await CreatureCmd.Damage(ctx, targets, damage.BaseValue, damage.Props, player, null);
    }
}

public sealed class VolatileFirePotion : VolatileCommonPotion
{
    public override TargetType TargetType => TargetType.AnyEnemy;
    public override string? CustomPackedImagePath => "res://images/atlases/potion_atlas.sprites/fire_potion.tres";

    protected override IEnumerable<DynamicVar> CanonicalVars => [new DamageVar(9m, ValueProp.Unpowered)];

    protected override async Task OnUse(PlayerChoiceContext ctx, Creature? target)
    {
        PotionModel.AssertValidForTargetedPotion(target);
        var damage = DynamicVars.Damage;
        NCombatRoom.Instance?.CombatVfxContainer.AddChildSafely(NGroundFireVfx.Create(target));
        await CreatureCmd.Damage(ctx, target, damage.BaseValue, damage.Props, Owner.Creature, null);
    }
}

public sealed class VolatileFlexPotion : VolatileCommonPotion
{
    public override TargetType TargetType => TargetType.AnyPlayer;
    public override string? CustomPackedImagePath => "res://images/atlases/potion_atlas.sprites/flex_potion.tres";

    protected override IEnumerable<DynamicVar> CanonicalVars => [new PowerVar<StrengthPower>(3m)];
    public override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.FromPower<StrengthPower>()];

    protected override async Task OnUse(PlayerChoiceContext ctx, Creature? target)
    {
        PotionModel.AssertValidForTargetedPotion(target);
        var amount = DynamicVars.Strength.BaseValue + Potency;
        await PowerCmd.Apply<FlexPotionPower>(ctx, target, amount, Owner.Creature, null);
    }
}

public sealed class VolatilePowerPotion : VolatileCommonPotion
{
    public override TargetType TargetType => TargetType.Self;
    public override string? CustomPackedImagePath => "res://images/atlases/potion_atlas.sprites/power_potion.tres";

    protected override async Task OnUse(PlayerChoiceContext ctx, Creature? target)
    {
        var count = Math.Min(4, 1 + Potency / 3);
        var cards = CardFactory.GetDistinctForCombat(Owner, Owner.Character.CardPool
            .GetUnlockedCards(Owner.UnlockState, Owner.RunState.CardMultiplayerConstraint)
            .Where(c => c.Type == CardType.Power), count, Owner.RunState.Rng.CombatCardGeneration).ToList();
        var chosen = await CardSelectCmd.FromChooseACardScreen(ctx, cards, Owner, canSkip: true);
        if (chosen == null) return;
        chosen.SetToFreeThisTurn();
        await CardPileCmd.AddGeneratedCardToCombat(chosen, PileType.Hand, Owner);
    }
}

public sealed class VolatileSkillPotion : VolatileCommonPotion
{
    public override TargetType TargetType => TargetType.Self;
    public override string? CustomPackedImagePath => "res://images/atlases/potion_atlas.sprites/skill_potion.tres";

    protected override async Task OnUse(PlayerChoiceContext ctx, Creature? target)
    {
        var count = Math.Min(4, 1 + Potency / 3);
        var cards = CardFactory.GetDistinctForCombat(Owner, Owner.Character.CardPool
            .GetUnlockedCards(Owner.UnlockState, Owner.RunState.CardMultiplayerConstraint)
            .Where(c => c.Type == CardType.Skill), count, Owner.RunState.Rng.CombatCardGeneration).ToList();
        var chosen = await CardSelectCmd.FromChooseACardScreen(ctx, cards, Owner, canSkip: true);
        if (chosen == null) return;
        chosen.SetToFreeThisTurn();
        await CardPileCmd.AddGeneratedCardToCombat(chosen, PileType.Hand, Owner);
    }
}

public sealed class VolatileSpeedPotion : VolatileCommonPotion
{
    public override TargetType TargetType => TargetType.AnyPlayer;
    public override string? CustomPackedImagePath => "res://images/atlases/potion_atlas.sprites/speed_potion.tres";

    protected override IEnumerable<DynamicVar> CanonicalVars => [new PowerVar<DexterityPower>(3m)];
    public override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.FromPower<DexterityPower>()];

    protected override async Task OnUse(PlayerChoiceContext ctx, Creature? target)
    {
        PotionModel.AssertValidForTargetedPotion(target);
        var amount = DynamicVars.Dexterity.BaseValue + Potency;
        await PowerCmd.Apply<SpeedPotionPower>(ctx, target, amount, Owner.Creature, null);
    }
}

public sealed class VolatileStrengthPotion : VolatileCommonPotion
{
    public override TargetType TargetType => TargetType.AnyPlayer;
    public override string? CustomPackedImagePath => "res://images/atlases/potion_atlas.sprites/strength_potion.tres";

    protected override IEnumerable<DynamicVar> CanonicalVars => [new PowerVar<StrengthPower>(1m)];
    public override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.FromPower<StrengthPower>()];

    protected override async Task OnUse(PlayerChoiceContext ctx, Creature? target)
    {
        PotionModel.AssertValidForTargetedPotion(target);
        NCombatRoom.Instance?.PlaySplashVfx(target, new Color("fd2155"));
        var amount = DynamicVars.Strength.BaseValue + Potency / 3;
        await PowerCmd.Apply<StrengthPower>(ctx, target, amount, Owner.Creature, null);
    }
}

public sealed class VolatileSwiftPotion : VolatileCommonPotion
{
    public override TargetType TargetType => TargetType.AnyPlayer;
    public override string? CustomPackedImagePath => "res://images/atlases/potion_atlas.sprites/swift_potion.tres";

    protected override IEnumerable<DynamicVar> CanonicalVars => [new CardsVar(1)];

    protected override async Task OnUse(PlayerChoiceContext ctx, Creature? target)
    {
        PotionModel.AssertValidForTargetedPotion(target);
        NCombatRoom.Instance?.PlaySplashVfx(target, new Color("45e6d0"));
        var cards = DynamicVars.Cards.BaseValue + Potency / 3;
        await CardPileCmd.Draw(ctx, cards, target.Player);
    }
}

public sealed class VolatileVulnerablePotion : VolatileCommonPotion
{
    public override TargetType TargetType => TargetType.AnyEnemy;
    public override string? CustomPackedImagePath => "res://images/atlases/potion_atlas.sprites/vulnerable_potion.tres";

    protected override IEnumerable<DynamicVar> CanonicalVars => [new PowerVar<VulnerablePower>(1m)];
    public override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.FromPower<VulnerablePower>()];

    protected override async Task OnUse(PlayerChoiceContext ctx, Creature? target)
    {
        PotionModel.AssertValidForTargetedPotion(target);
        NCombatRoom.Instance?.PlaySplashVfx(target, new Color("fd2155"));
        var amount = DynamicVars.Vulnerable.BaseValue + Potency / 3;
        await PowerCmd.Apply<VulnerablePower>(ctx, target, amount, Owner.Creature, null);
    }
}

public sealed class VolatilePoisonPotion : VolatileCommonPotion
{
    public override TargetType TargetType => TargetType.AnyEnemy;
    public override string? CustomPackedImagePath => "poison_potion.png".PotionImagePath();

    protected override IEnumerable<DynamicVar> CanonicalVars => [new DynamicVar("Poison", 3m)];
    public override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.FromPower<PoisonPower>()];

    protected override async Task OnUse(PlayerChoiceContext ctx, Creature? target)
    {
        PotionModel.AssertValidForTargetedPotion(target);
        var amount = DynamicVars["Poison"].BaseValue + Potency;
        await PowerCmd.Apply<PoisonPower>(ctx, target, amount, Owner.Creature, null);
    }
}

/// <summary>
/// Apply Poison to ALL enemies. The Volatile counterpart to the real <see cref="PoisonAmpoule"/>
/// -- weaker, at Poison's usual 1:1 Potency scaling from a base of 3. Not offered anywhere, same as
/// every VolatileCommonPotion; the only route in is a card naming BasePotion.PoisonAmpoule.
/// </summary>
public sealed class VolatilePoisonAmpoule : VolatileCommonPotion
{
    public override TargetType TargetType => TargetType.Self;
    public override string? CustomPackedImagePath => "poison_potion.png".PotionImagePath();

    protected override IEnumerable<DynamicVar> CanonicalVars => [new DynamicVar("Poison", 3m)];
    public override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.FromPower<PoisonPower>()];

    protected override async Task OnUse(PlayerChoiceContext ctx, Creature? target)
    {
        var amount = DynamicVars["Poison"].BaseValue + Potency;
        foreach (var enemy in AlchemistEffects.Enemies(Lab))
            await PowerCmd.Apply<PoisonPower>(ctx, enemy, amount, Owner.Creature, null);
    }
}

public sealed class VolatileWeakPotion : VolatileCommonPotion
{
    public override TargetType TargetType => TargetType.AnyEnemy;
    public override string? CustomPackedImagePath => "res://images/atlases/potion_atlas.sprites/weak_potion.tres";

    protected override IEnumerable<DynamicVar> CanonicalVars => [new PowerVar<WeakPower>(1m)];
    public override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.FromPower<WeakPower>()];

    protected override async Task OnUse(PlayerChoiceContext ctx, Creature? target)
    {
        PotionModel.AssertValidForTargetedPotion(target);
        NCombatRoom.Instance?.PlaySplashVfx(target, new Color("94f882"));
        var amount = DynamicVars.Weak.BaseValue + Potency / 3;
        await PowerCmd.Apply<WeakPower>(ctx, target, amount, Owner.Creature, null);
    }
}
