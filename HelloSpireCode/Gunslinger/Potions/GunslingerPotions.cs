using HelloSpire.HelloSpireCode.Gunslinger.Cylinder;
using HelloSpire.HelloSpireCode.Characters;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Potions;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;

namespace HelloSpire.HelloSpireCode.Gunslinger.Potions;

/// <summary>
/// Shared plumbing for the Gunslinger's potions — just enough to reach the gun from the potion bar.
/// </summary>
public abstract class GunslingerPotion : Characters.GunslingerPotion
{
    public override PotionUsage Usage => PotionUsage.CombatOnly;
    public override TargetType TargetType => TargetType.Self;

    protected GunContext Gun => GunContext.From(Owner);
}

/// <summary>
/// Load three Lead Rounds and one random special Round. The answer to drawing a hand with no
/// ammunition in it, and — like the rest of the character's ammunition — not entirely up to you.
/// </summary>
public sealed class SpeedloaderFlask : GunslingerPotion
{
    public override PotionRarity Rarity => PotionRarity.Common;

    protected override async Task OnUse(PlayerChoiceContext ctx, Creature? target)
    {
        await Revolver.Load(ctx, Gun, Rounds.Lead, 3);
        await Revolver.Load(ctx, Gun, Rounds.RandomSpecial(Gun), 1);
    }
}

/// <summary>Gain 10 Deadeye. Turns whatever is under the hammer into a finisher.</summary>
public sealed class SightlineTonic : GunslingerPotion
{
    public override PotionRarity Rarity => PotionRarity.Uncommon;

    protected override async Task OnUse(PlayerChoiceContext ctx, Creature? target)
    {
        await GunslingerEffects.GainDeadeye(ctx, Gun, 10);
    }
}

/// <summary>
/// Gain 1 Intangible: the whole of the next enemy turn, refused.
///
/// One stack rather than the two Dodge this used to grant, for the same reason Ghost Step dropped
/// to one — Intangible answers an entire turn, not an individual hit, so two of them is two turns
/// of a boss fight out of a Rare potion.
/// </summary>
public sealed class GhostSmoke : GunslingerPotion
{
    public override PotionRarity Rarity => PotionRarity.Rare;

    protected override async Task OnUse(PlayerChoiceContext ctx, Creature? target)
    {
        await GunslingerEffects.GainIntangible(ctx, Gun, 1);
    }
}
