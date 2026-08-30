using HelloSpire.HelloSpireCode.Alchemist.Lab;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Potions;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;

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

/// <summary>
/// Fifteen Gold and an Energy.
///
/// Excluded from random Brew, because it makes real Gold — the curated Combat Potion pool must
/// never produce anything whose value outlives the fight. Still a legitimate shop or reward
/// Potion, and still fine from Alchemize.
/// </summary>
public sealed class AurumTincture : AlchemistPotion
{
    public override PotionRarity Rarity => PotionRarity.Uncommon;

    protected override async Task OnUse(PlayerChoiceContext ctx, Creature? target)
    {
        await Ledger.GainGold(ctx, Lab, 15);
        await AlchemistEffects.GainEnergy(Lab, 1m);
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
/// Non-Brewable: no random Brew, no Alchemize, no Panacea may ever produce it. The Great Work is
/// the only way it exists, which is what six permanent Max HP is buying — not a strong effect, a
/// unique object that survives the fight and can be carried to a boss.
///
/// TODO(Phase 3): the Combat Potion pool must exclude this explicitly once the bridge is wired.
/// </summary>
public sealed class PhilosophersStone : AlchemistPotion
{
    public override PotionRarity Rarity => PotionRarity.Rare;

    protected override async Task OnUse(PlayerChoiceContext ctx, Creature? target)
    {
        foreach (var enemy in AlchemistEffects.Enemies(Lab))
        {
            await DamageCmd.Attack(20m).FromCreature(Owner.Creature).Targeting(enemy).Execute(ctx);
        }

        await AlchemistEffects.GainBlock(Lab, 20m);
        await AlchemistEffects.GainEnergy(Lab, 2m);
        await AlchemistEffects.Draw(ctx, Lab, 3);
        await AlchemistEffects.GainStrength(ctx, Lab, 2m);
        await AlchemistEffects.GainDexterity(ctx, Lab, 2m);
    }
}
