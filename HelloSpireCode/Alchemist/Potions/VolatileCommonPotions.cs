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

namespace HelloSpire.HelloSpireCode.Alchemist.Potions;

/// <summary>
/// A registered pool that nothing ever queries. BaseLib's CustomContentDictionary hard-requires
/// every CustomPotionModel to carry a [Pool(typeof(...))] attribute (confirmed live: the game
/// refused to start at all -- "Model ... must be marked with a PoolAttribute" -- when
/// VolatileCommonPotion had none), so joining a real pool isn't optional. AlchemistPotionPool was
/// the obvious choice, but it's the exact pool MegaCrit.Sts2.Core.Factories.PotionFactory reads
/// from Player.Character.PotionPool for shops and reward screens -- joining it would make these
/// potions purchasable and offerable, which the design explicitly forbids. This pool exists solely
/// to satisfy the attribute requirement: no Character's PotionPool property ever returns it, so
/// nothing outside WiredLabBridge's own curated list (see VolatileCommonPotion.VolatileCommonPool
/// in WiredLabBridge.cs) ever enumerates it.
/// </summary>
public sealed class VolatilePotionPool : BaseLib.Abstracts.CustomPotionPoolModel;

/// <summary>
/// Base for the Alchemist's Volatile Potions: registers with ModelDb like any custom potion (so
/// localization and <see cref="CustomPackedImagePath"/> resolution work) via the inert
/// <see cref="VolatilePotionPool"/> above, rather than <see cref="Characters.AlchemistPotion"/>'s
/// AlchemistPotionPool -- these must never turn up at a Merchant or a reward screen. Only
/// WiredLabBridge's own curated list hands one out, via Belt.Brew, and Belt.Brew always marks its
/// result Volatile.
/// </summary>
[Pool(typeof(VolatilePotionPool))]
public abstract class VolatileCommonPotion : BaseLib.Abstracts.CustomPotionModel
{
    public override PotionRarity Rarity => PotionRarity.Common;
    public override PotionUsage Usage => PotionUsage.CombatOnly;
}

/// <summary>
/// Weaker Volatile counterparts of the 15 real Common Potions in the Alchemist's combat pool (see
/// WiredLabBridge.RandomCombatPotion/CombatPotionOptions/NamedPotion). Every OnUse below is ported
/// from the real class, decompiled from sts2.dll, at a reduced value -- Volatile Vulnerable
/// Potion's 2 Vulnerable (vs. the real one's 3) is the case that surfaced this file; the rest
/// follow the same "weaker than what a shop would sell you" rule, using ChatGPT's originally
/// proposed numbers where they map onto a potion that actually exists in this game's pool, and an
/// equivalent reduction (3 card choices -> 2) for the four card-generation Commons ChatGPT's table
/// never covered.
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
        var cards = CardFactory.GetDistinctForCombat(Owner, Owner.Character.CardPool
            .GetUnlockedCards(Owner.UnlockState, Owner.RunState.CardMultiplayerConstraint)
            .Where(c => c.Type == CardType.Attack), 2, Owner.RunState.Rng.CombatCardGeneration).ToList();
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
        var cards = CardFactory.GetDistinctForCombat(Owner,
            ModelDb.CardPool<ColorlessCardPool>().GetUnlockedCards(Owner.UnlockState, Owner.RunState.CardMultiplayerConstraint),
            2, Owner.RunState.Rng.CombatCardGeneration).ToList();
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
        await PowerCmd.Apply<DexterityPower>(ctx, target, DynamicVars.Dexterity.BaseValue, Owner.Creature, null);
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
        await PlayerCmd.GainEnergy(DynamicVars.Energy.BaseValue, target.Player);
    }
}

public sealed class VolatileExplosiveAmpoule : VolatileCommonPotion
{
    public override TargetType TargetType => TargetType.AllEnemies;
    public override string? CustomPackedImagePath => "res://images/atlases/potion_atlas.sprites/explosive_ampoule.tres";

    protected override IEnumerable<DynamicVar> CanonicalVars => [new DamageVar(7m, ValueProp.Unpowered)];

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

    protected override IEnumerable<DynamicVar> CanonicalVars => [new DamageVar(12m, ValueProp.Unpowered)];

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
        await PowerCmd.Apply<FlexPotionPower>(ctx, target, DynamicVars.Strength.BaseValue, Owner.Creature, null);
        await PowerCmd.Apply<WeakPower>(ctx, target, 1m, Owner.Creature, null);
    }
}

public sealed class VolatilePowerPotion : VolatileCommonPotion
{
    public override TargetType TargetType => TargetType.Self;
    public override string? CustomPackedImagePath => "res://images/atlases/potion_atlas.sprites/power_potion.tres";

    protected override async Task OnUse(PlayerChoiceContext ctx, Creature? target)
    {
        var cards = CardFactory.GetDistinctForCombat(Owner, Owner.Character.CardPool
            .GetUnlockedCards(Owner.UnlockState, Owner.RunState.CardMultiplayerConstraint)
            .Where(c => c.Type == CardType.Power), 2, Owner.RunState.Rng.CombatCardGeneration).ToList();
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
        var cards = CardFactory.GetDistinctForCombat(Owner, Owner.Character.CardPool
            .GetUnlockedCards(Owner.UnlockState, Owner.RunState.CardMultiplayerConstraint)
            .Where(c => c.Type == CardType.Skill), 2, Owner.RunState.Rng.CombatCardGeneration).ToList();
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
        await PowerCmd.Apply<SpeedPotionPower>(ctx, target, DynamicVars.Dexterity.BaseValue, Owner.Creature, null);
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
        await PowerCmd.Apply<StrengthPower>(ctx, target, DynamicVars.Strength.BaseValue, Owner.Creature, null);
    }
}

public sealed class VolatileSwiftPotion : VolatileCommonPotion
{
    public override TargetType TargetType => TargetType.AnyPlayer;
    public override string? CustomPackedImagePath => "res://images/atlases/potion_atlas.sprites/swift_potion.tres";

    protected override IEnumerable<DynamicVar> CanonicalVars => [new CardsVar(2)];

    protected override async Task OnUse(PlayerChoiceContext ctx, Creature? target)
    {
        PotionModel.AssertValidForTargetedPotion(target);
        NCombatRoom.Instance?.PlaySplashVfx(target, new Color("45e6d0"));
        await CardPileCmd.Draw(ctx, DynamicVars.Cards.BaseValue, target.Player);
    }
}

public sealed class VolatileVulnerablePotion : VolatileCommonPotion
{
    public override TargetType TargetType => TargetType.AnyEnemy;
    public override string? CustomPackedImagePath => "res://images/atlases/potion_atlas.sprites/vulnerable_potion.tres";

    protected override IEnumerable<DynamicVar> CanonicalVars => [new PowerVar<VulnerablePower>(2m)];
    public override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.FromPower<VulnerablePower>()];

    protected override async Task OnUse(PlayerChoiceContext ctx, Creature? target)
    {
        PotionModel.AssertValidForTargetedPotion(target);
        NCombatRoom.Instance?.PlaySplashVfx(target, new Color("fd2155"));
        await PowerCmd.Apply<VulnerablePower>(ctx, target, DynamicVars.Vulnerable.BaseValue, Owner.Creature, null);
    }
}

public sealed class VolatileWeakPotion : VolatileCommonPotion
{
    public override TargetType TargetType => TargetType.AnyEnemy;
    public override string? CustomPackedImagePath => "res://images/atlases/potion_atlas.sprites/weak_potion.tres";

    protected override IEnumerable<DynamicVar> CanonicalVars => [new PowerVar<WeakPower>(2m)];
    public override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.FromPower<WeakPower>()];

    protected override async Task OnUse(PlayerChoiceContext ctx, Creature? target)
    {
        PotionModel.AssertValidForTargetedPotion(target);
        NCombatRoom.Instance?.PlaySplashVfx(target, new Color("94f882"));
        await PowerCmd.Apply<WeakPower>(ctx, target, DynamicVars.Weak.BaseValue, Owner.Creature, null);
    }
}
