using HelloSpire.HelloSpireCode.Characters;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.HoverTips;

using HelloSpire.HelloSpireCode.Alchemist;
namespace HelloSpire.HelloSpireCode.Alchemist.Cards;

/// <summary>
/// Base for every Alchemist card.
///
/// Adds two conveniences on top of the mod's card base: implicit access to the bench this card is
/// being played from, and a short way to attach the Brew / Distill / Infuse / Unleash hover tips
/// that carry the character's vocabulary.
/// </summary>
public abstract class AlchemistCard(int cost, CardType type, CardRarity rarity, TargetType target)
    : Characters.AlchemistCard(cost, type, rarity, target)
{
    /// <summary>This card, as the source of a bench operation.</summary>
    protected LabContext Lab => this;

    protected static IHoverTip Tip(StaticHoverTip tip) => HoverTipFactory.Static(tip);
}
