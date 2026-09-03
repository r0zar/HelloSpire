using HelloSpire.HelloSpireCode.Alchemist.Lab;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Potions;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;

using HelloSpire.HelloSpireCode.Alchemist;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;
namespace HelloSpire.HelloSpireCode.Alchemist.Potions;

/// <summary>Shared plumbing for the Alchemist's potions — just enough to reach the bench.</summary>
public abstract class AlchemistPotion : Characters.AlchemistPotion
{
    public override PotionUsage Usage => PotionUsage.CombatOnly;
    public override TargetType TargetType => TargetType.Self;

    protected LabContext Lab => LabContext.From(Owner);
}

/// <summary>Exhaust a card and draw two. Deck filtering that costs no Energy.</summary>
public sealed class SolventFlask : AlchemistPotion
{
    public override PotionRarity Rarity => PotionRarity.Common;

    protected override async Task OnUse(PlayerChoiceContext ctx, Creature? target)
    {
        if (!await Alchemy.ExhaustOne(ctx, Lab)) return;
        await AlchemistEffects.Draw(ctx, Lab, 2);
    }
}

/// <summary>Heavy Poison to everything, and an Energy to spend it with.</summary>
public sealed class AurumTincture : AlchemistPotion
{
    public override PotionRarity Rarity => PotionRarity.Uncommon;

    protected override async Task OnUse(PlayerChoiceContext ctx, Creature? target)
    {
        foreach (var enemy in AlchemistEffects.Enemies(Lab))
            await AlchemistEffects.ApplyPoison(ctx, Lab, enemy, 8m);

        await AlchemistEffects.GainEnergy(Lab, 1m);
    }
}

/// <summary>Apply a big chunk of Poison.</summary>
public sealed class PoisonPotion : AlchemistPotion
{
    public override PotionRarity Rarity => PotionRarity.Common;
    public override TargetType TargetType => TargetType.AnyEnemy;

    protected override async Task OnUse(PlayerChoiceContext ctx, Creature? target)
    {
        if (target == null) return;
        await AlchemistEffects.ApplyPoison(ctx, Lab, target, 6m);
    }
}

/// <summary>
/// Junk: what Infuse knocks loose into the belt (see <see cref="Lab.Belt.Infuse"/>). Unusable
/// (Usage.None) and can't be Discarded from the belt (see
/// <see cref="BlockResidualReagentDiscardPatch"/>) -- Distilling it is the one way it ever leaves a
/// Slot early, which works because <see cref="Lab.Belt.Distill"/> removes the Potion directly
/// (<see cref="LabBridge.RemovePotion"/>) rather than going through the same Discard command the
/// belt's generic discard button uses. Falls out at combat end regardless, same as any other
/// Volatile Potion. Non-Brewable and kept out of shops and rewards the same way Poison Ampoule is
/// -- Infusing is the only way to ever get one.
/// </summary>
public sealed class ResidualReagent : AlchemistPotion
{
    public override PotionRarity Rarity => PotionRarity.Common;
    public override PotionUsage Usage => PotionUsage.None;

    protected override Task OnUse(PlayerChoiceContext ctx, Creature? target) => Task.CompletedTask;
}

/// <summary>
/// Residual Reagent can't be Discarded -- not Distilled (that's a different, direct removal; see
/// the class doc comment above), just the belt's ordinary "throw this Potion away" button, which
/// routes through this one static command regardless of who calls it. PotionCmd.Discard takes no
/// Player parameter (it resolves the owner from the Potion itself), so a Prefix here is the only
/// place a per-Potion block can live -- same "the engine has no per-instance hook, so Harmony
/// supplies one" shape as HideVolatilePotionsFromShopsAndRewardsPatch.
/// </summary>
[HarmonyLib.HarmonyPatch(typeof(PotionCmd), nameof(PotionCmd.Discard))]
internal static class BlockResidualReagentDiscardPatch
{
    [HarmonyLib.HarmonyPrefix]
    private static bool BeforeDiscard(PotionModel potion) => potion is not ResidualReagent;
}

/// <summary>
/// Apply Poison to ALL enemies. Non-Brewable: no random Brew, no Alchemize, no Panacea may ever
/// produce it, and it's kept out of shops and rewards -- same shape as The Great Work's
/// Philosopher's Stone. Stabilizing a Volatile Poison Ampoule is the only way it exists.
/// </summary>
public sealed class PoisonAmpoule : AlchemistPotion
{
    public override PotionRarity Rarity => PotionRarity.Common;

    protected override async Task OnUse(PlayerChoiceContext ctx, Creature? target)
    {
        foreach (var enemy in AlchemistEffects.Enemies(Lab))
            await AlchemistEffects.ApplyPoison(ctx, Lab, enemy, 5m);
    }
}

/// <summary>One saved Rare Potion becomes a full belt of combat tools.</summary>
public sealed class PanaceaOfPlenty : AlchemistPotion
{
    public override PotionRarity Rarity => PotionRarity.Rare;

    protected override async Task OnUse(PlayerChoiceContext ctx, Creature? target) =>
        await Belt.FillEmpty(ctx, Lab);
}

/// <summary>
/// The Great Work made real. One source only.
///
/// Non-Brewable: no random Brew, no Alchemize, no Panacea may ever produce it —
/// <see cref="WiredLabBridge"/>'s Combat Potion pool excludes it explicitly. The Great Work is the
/// only way it exists: not a strong effect on its own, a unique object that survives the fight
/// (Brewed non-Volatile) and can be carried to a boss.
/// </summary>
public sealed class PhilosophersStone : AlchemistPotion
{
    public override PotionRarity Rarity => PotionRarity.Rare;

    protected override async Task OnUse(PlayerChoiceContext ctx, Creature? target)
    {
        foreach (var enemy in AlchemistEffects.Enemies(Lab))
        {
            await CreatureCmd.Damage(ctx, enemy, 20m, ValueProp.Unpowered, Owner.Creature, null);
        }

        await AlchemistEffects.GainBlock(Lab, 20m);
        await AlchemistEffects.GainEnergy(Lab, 2m);
        await AlchemistEffects.Draw(ctx, Lab, 3);
        await AlchemistEffects.GainStrength(ctx, Lab, 2m);
        await AlchemistEffects.GainDexterity(ctx, Lab, 2m);
    }
}
