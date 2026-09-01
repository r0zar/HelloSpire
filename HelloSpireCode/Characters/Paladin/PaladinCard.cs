using System.Threading.Tasks;
using BaseLib.Abstracts;
using BaseLib.Extensions;
using BaseLib.Utils;
using HelloSpire.HelloSpireCode.Extensions;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;

namespace HelloSpire.HelloSpireCode.Characters;

/// <summary>
/// Base class for Paladin cards. The [Pool] attribute puts every subclass into the
/// Paladin card pool automatically, so individual cards never declare it.
/// Card art resolves by class name, which is unique mod-wide, so all characters
/// share the images/card_portraits tree.
///
/// Tithe: a card with a Tithe face runs <see cref="OnTithe"/> when it is DISCARDED (cards in
/// every pile receive the AfterCardDiscarded hook, so the discarded card hears about itself --
/// no Harmony needed). The card then simply sits in the discard pile: a Tithe cast never
/// Exhausts, which is the healing law's whole point -- the true cast is the candle, the face
/// is the flicker that cycles back forever.
/// </summary>
[Pool(typeof(PaladinCardPool))]
public abstract class PaladinCard(int cost, CardType type, CardRarity rarity, TargetType target) :
    CustomCardModel(cost, type, rarity, target)
{
    //Normal art: 1000x760   Full art: 606x852
    public override string CustomPortraitPath => $"{Id.Entry.RemovePrefix().ToLowerInvariant()}.png".BigCardImagePath();

    //Small variants: normal 250x190, fullart 250x350
    public override string PortraitPath => $"{Id.Entry.RemovePrefix().ToLowerInvariant()}.png".CardImagePath();
    public override string BetaPortraitPath => $"beta/{Id.Entry.RemovePrefix().ToLowerInvariant()}.png".CardImagePath();

    /// <summary>
    /// The played target, or the caster when the game supplied none. AnyPlayer cards get NO
    /// target in solo play (the log says "(no target)") -- "a player" defaults to yourself.
    /// </summary>
    protected MegaCrit.Sts2.Core.Entities.Creatures.Creature TargetOrOwner(CardPlay cardPlay) =>
        cardPlay.Target ?? Owner.Creature;

    /// <summary>True on cards that carry a Tithe face. Derived cards opt in by overriding OnTithe.</summary>
    public virtual bool HasTithe => false;

    /// <summary>The Tithe face: fires when this card is discarded. Small, recurring, never Exhausts.</summary>
    protected virtual Task OnTithe(PlayerChoiceContext ctx) => Task.CompletedTask;

    public override async Task AfterCardDiscarded(PlayerChoiceContext choiceContext, CardModel card)
    {
        await base.AfterCardDiscarded(choiceContext, card);
        if (card != this || !HasTithe) return;
        await OnTithe(choiceContext);
    }
}

/// <summary>
/// Marker for cards whose true cast heals (direct heal or Regen). Last Rites counts these in the
/// Exhaust pile -- the candles spent this combat.
/// </summary>
public interface IHealingCard;
