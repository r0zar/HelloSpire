using HelloSpire.HelloSpireCode.Gunslinger.Cylinder;
using HelloSpire.HelloSpireCode.Potions;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Potions;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;

namespace HelloSpire.HelloSpireCode.Gunslinger.Potions;

/// <summary>
/// Shared plumbing for the Gunslinger's potions — just enough to reach the gun from the potion bar.
/// </summary>
public abstract class GunslingerPotion : HelloSpirePotion
{
    public override PotionUsage Usage => PotionUsage.CombatOnly;
    public override PotionTargetType TargetType => PotionTargetType.None;

    protected GunContext Gun => GunContext.From(Owner);
}

/// <summary>Load three Lead Rounds. The answer to drawing a hand with no ammunition in it.</summary>
public sealed class SpeedloaderFlask : GunslingerPotion
{
    public override PotionRarity Rarity => PotionRarity.Common;

    public override async Task OnUse(PlayerChoiceContext ctx, Creature? target)
    {
        await Revolver.Load(ctx, Gun, Rounds.Lead, 3);
    }
}

/// <summary>Gain 10 Deadeye. Turns whatever is under the hammer into a finisher.</summary>
public sealed class SightlineTonic : GunslingerPotion
{
    public override PotionRarity Rarity => PotionRarity.Uncommon;

    public override async Task OnUse(PlayerChoiceContext ctx, Creature? target)
    {
        await GunslingerEffects.GainDeadeye(ctx, Gun, 10);
    }
}

/// <summary>Gain 2 Dodge. Two hits off the biggest intent in the fight.</summary>
public sealed class GhostSmoke : GunslingerPotion
{
    public override PotionRarity Rarity => PotionRarity.Rare;

    public override async Task OnUse(PlayerChoiceContext ctx, Creature? target)
    {
        await GunslingerEffects.GainDodge(ctx, Gun, 2);
    }
}
