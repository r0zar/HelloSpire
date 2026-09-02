using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Models;

namespace HelloSpire.HelloSpireCode.Alchemist;

/// <summary>
/// Who is working the bench.
///
/// Cards are the usual answer, but the Alchemical Satchel Brews at the start of combat and
/// Panacea of Plenty Brews from the potion bar — neither has a card to attribute the action to.
/// Every Brew, Distill and Infuse takes one of these instead of a <see cref="CardModel"/>
/// so relics and potions are not second-class citizens.
///
/// A card converts implicitly, so card code still reads <c>Belt.Brew(ctx, this, ...)</c>.
///
/// Deliberately the same shape as the Gunslinger's GunContext. Two characters in one pack that
/// both need "the player, plus optionally the card that caused this" should not invent it twice.
/// </summary>
public readonly record struct LabContext(Player Player, CardModel? Card)
{
    public Creature Self => Player.Creature;

    public static implicit operator LabContext(CardModel card) => new(card.Owner, card);

    public static LabContext From(Player player) => new(player, null);
}
