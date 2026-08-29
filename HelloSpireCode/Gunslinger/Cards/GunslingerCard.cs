using HelloSpire.HelloSpireCode.Characters;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.HoverTips;

namespace HelloSpire.HelloSpireCode.Gunslinger.Cards;

/// <summary>
/// Base for every Gunslinger card.
///
/// Adds two conveniences on top of the mod's card base: implicit access to the gun this card is
/// being played from, and a short way to attach the Load / Fire / Cycle / Spin hover tips that
/// carry the character's vocabulary.
/// </summary>
public abstract class GunslingerCard(int cost, CardType type, CardRarity rarity, TargetType target)
    : Characters.GunslingerCard(cost, type, rarity, target)
{
    /// <summary>This card, as the source of a cylinder operation.</summary>
    protected GunContext Gun => this;

    protected static IHoverTip Tip(StaticHoverTip tip) => HoverTipFactory.Static(tip);
}
