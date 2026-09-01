using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;

namespace HelloSpire.HelloSpireCode.Characters.PaladinContent;

/// <summary>
/// The confession seal, banked: give something up, and judgment gives it back.
/// Judge: return a random card from your discard pile to your hand -- the Holy recursion
/// at common. No loop: true heal casts still Exhaust, so nothing recurs that shouldn't.
/// </summary>
public sealed class SealOfThePenitentPower : SealPower
{
    public override async Task OnJudged(PlayerChoiceContext ctx, Creature target)
    {
        if (Owner.Player is not { } player) return;
        var discard = PileType.Discard.GetPile(player);
        var card = player.RunState.Rng.CombatCardSelection.NextItem(discard.Cards);
        if (card == null) return;
        await CardPileCmd.Add(card, PileType.Hand.GetPile(player));
    }
}
