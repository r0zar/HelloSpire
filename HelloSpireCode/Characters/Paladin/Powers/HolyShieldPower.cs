using HelloSpire.HelloSpireCode.Powers;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;

namespace HelloSpire.HelloSpireCode.Characters.PaladinContent;

/// <summary>
/// Whenever a card gains the bearer Block, it gains Spirit more. The Holy fusion: the
/// heal-scaling stat amplifies the lane's Block cards -- Dexterity that prays.
/// </summary>
public sealed class HolyShieldPower : HelloSpirePower
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Single;

    public override decimal ModifyBlockAdditive(Creature target, decimal block, ValueProp props,
        CardModel? cardSource, CardPlay? cardPlay)
    {
        if (cardSource == null || cardSource.Owner?.Creature != Owner) return 0m;
        if (!props.IsPoweredCardOrMonsterMoveBlock()) return 0m;
        // Spirit debt silences the amp rather than shrinking Block below its printed value.
        return Owner.Player is { } player ? System.Math.Max(0, Spirit.Of(player)) : 0m;
    }
}
