using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Potions;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;

namespace HelloSpire.HelloSpireCode.Alchemist.Potions;

/// <summary>
/// The Alchemist's unique Potion: Brewed at 3 base Damage by Alchemical Satchel at the start of
/// every combat, then built up over the turn by every card that Infuses it, and used exactly like
/// any other Potion once you're ready to cash it in -- whoever it's used on takes the stored
/// Damage and Poison, and the stored Block and Energy come back to you.
///
/// A real Potion sitting in a real Potion Slot, not a Power with its own UI: a truly separate
/// slot outside the belt has no working base-game hook to build on (Player.PotionSlots has no
/// exemption mechanism anywhere), and reusing the belt means Unstable Concoction gets the entire
/// existing Potion-use flow -- click, pick a target, resolve -- for free, including Potency
/// (PotionUsePatch already bumps every DamageVar/BlockVar on a Volatile Potion before OnUse runs).
/// </summary>
public sealed class UnstableConcoction : AlchemistPotion
{
    public override PotionRarity Rarity => PotionRarity.Common;
    public override TargetType TargetType => TargetType.AnyEnemy;

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DamageVar(3m, ValueProp.Unpowered),
        new BlockVar(0m, ValueProp.Unpowered),
        new DynamicVar("Poison", 0m),
        new DynamicVar("Energy", 0m),
        new DynamicVar("Vulnerable", 0m)
    ];

    protected override async Task OnUse(PlayerChoiceContext ctx, Creature? target)
    {
        var damage = DynamicVars.Damage.BaseValue;
        var block = DynamicVars.Block.BaseValue;
        var poison = DynamicVars["Poison"].BaseValue;
        var energy = DynamicVars["Energy"].BaseValue;
        var vulnerable = DynamicVars["Vulnerable"].BaseValue;

        if (damage > 0 && target != null)
            await CreatureCmd.Damage(ctx, target, damage, ValueProp.Unpowered, Owner.Creature, null);

        if (poison > 0 && target != null)
            await AlchemistEffects.ApplyPoison(ctx, Lab, target, poison);

        if (vulnerable > 0 && target != null)
            await AlchemistEffects.ApplyVulnerable(ctx, Lab, target, vulnerable);

        if (block > 0)
            await AlchemistEffects.GainBlock(Lab, block);

        if (energy > 0)
            await AlchemistEffects.GainEnergy(Lab, energy);
    }
}
