using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Models;

namespace HelloSpire.HelloSpireCode.Gunslinger;

/// <summary>
/// Who is working the gun.
///
/// Cards are the usual answer, but Old Iron loads at the start of combat and Speedloader Flask
/// loads from the potion bar — neither has a card to attribute the action to. Every cylinder
/// operation takes one of these instead of a <see cref="CardModel"/> so relics and potions are not
/// second-class citizens.
///
/// A card converts implicitly, so card code still reads <c>Revolver.Fire(ctx, this, target)</c>.
/// </summary>
public readonly record struct GunContext(Player Player, CardModel? Card)
{
    public Creature Self => Player.Creature;

    public static implicit operator GunContext(CardModel card) => new(card.Owner, card);

    public static GunContext From(Player player) => new(player, null);
}
