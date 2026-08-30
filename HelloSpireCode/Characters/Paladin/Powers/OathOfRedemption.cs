using System.Threading.Tasks;
using HelloSpire.HelloSpireCode.Characters.PaladinContent.Faith;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;

namespace HelloSpire.HelloSpireCode.Characters.PaladinContent.Powers;

/// <summary>Whenever you heal, gain 1 Faith in Ilmater.
///
/// There is no AfterHeal hook. CreatureCmd.Heal fires AfterCurrentHpChanged with a positive
/// delta and damage fires it with a negative one, so we gate on sign. Healing any player counts
/// (an ally heal is the co-op engine), and Heal passes the requested amount rather than the
/// clamped one, so a heal at full HP still fires and heals are never dead cards.</summary>
public sealed class OathOfRedemption : PaladinPower
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Single;

    public override async Task AfterCurrentHpChanged(Creature creature, decimal delta)
    {
        if (delta <= 0m) return;
        if (!creature.IsPlayer) return;
        FaithTracks.Gain(Owner.Player, Deity.Ilmater, 1);
        await Task.CompletedTask;
    }
}
