using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Potions;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;

namespace HelloSpire.HelloSpireCode.Characters.PaladinContent.Potions;

/// <summary>
/// Shared plumbing for the Paladin's potions -- just enough to reach the player from the potion
/// bar. Three potions, one per rarity, one per archetype spine: the Common heals (Spirit), the
/// Uncommon judges (Seals), the Rare holds the wall (Block and Buffer).
/// </summary>
public abstract class PaladinPotion : Characters.PaladinPotion
{
    public override PotionUsage Usage => PotionUsage.CombatOnly;
    public override TargetType TargetType => TargetType.Self;
}

/// <summary>
/// Gain 2 Spirit, then heal 8 + Spirit. The Spirit lands first, so the potion pays 10 on the spot
/// and more in a deck that has been banking -- and unlike Mend it leaves the Spirit behind, so the
/// heal cards drawn after it are worth more too. The emergency button the healer spine was missing:
/// every heal in the kit costs a card AND Energy, and the turn you need one you have neither.
/// </summary>
public sealed class AnointingOil : PaladinPotion
{
    public const int SpiritGain = 2;
    public const decimal BaseHeal = 8m;

    public override PotionRarity Rarity => PotionRarity.Common;

    protected override async Task OnUse(PlayerChoiceContext ctx, Creature? target)
    {
        await Spirit.Gain(ctx, Owner, SpiritGain);
        await Spirit.Heal(Owner, BaseHeal);
    }
}

/// <summary>
/// Deal 10, then Judge twice. The bank-cashing problem in a bottle: seals bank fine and the
/// trigger cards are what you fail to draw, so a stocked bank sits there while the fight ends.
/// Damage first so it is never a dead card -- a seal-less use is still a Fire Potion, and
/// IJudgeTrigger powers fire on an empty bank regardless.
///
/// Unpowered damage, like every other potion in the pack: Strength does not ride the belt.
/// </summary>
public sealed class VialOfVerdict : PaladinPotion
{
    public const decimal BaseDamage = 10m;
    public const int JudgeCount = 2;

    public override PotionRarity Rarity => PotionRarity.Uncommon;
    public override TargetType TargetType => TargetType.AnyEnemy;

    protected override async Task OnUse(PlayerChoiceContext ctx, Creature? target)
    {
        if (target == null) return;
        await CreatureCmd.Damage(ctx, target, BaseDamage, ValueProp.Unpowered, Owner.Creature, null);
        await Seals.Judge(ctx, Owner, target, JudgeCount);
    }
}

/// <summary>
/// Gain 15 Block and 1 Buffer -- Divine Shield in a bottle, with a wall behind it. The Rare that
/// answers the turn the tank cannot answer: the big telegraphed hit lands on the Buffer, and the
/// chip damage around it lands on the Block. Unpowered Block, so Dexterity does not ride it.
/// </summary>
public sealed class SanctifiedDraught : PaladinPotion
{
    public const decimal BaseBlock = 15m;
    public const decimal BufferStacks = 1m;

    public override PotionRarity Rarity => PotionRarity.Rare;

    protected override async Task OnUse(PlayerChoiceContext ctx, Creature? target)
    {
        await CreatureCmd.GainBlock(Owner.Creature, BaseBlock, ValueProp.Unpowered, null);
        await PowerCmd.Apply<BufferPower>(ctx, Owner.Creature, BufferStacks, Owner.Creature, null);
    }
}
