using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;

namespace HelloSpire.HelloSpireCode.Characters.PaladinContent;

/// <summary>
/// The Holy seal, banked: a pure judge charge.
/// Judge: relight a candle -- a random healing card returns from the Exhaust pile to your hand.
/// Not a heal (no loops); it extends the candle-clock, the most Holy thing a judge can buy.
/// </summary>
public sealed class SealOfLightPower : SealPower
{
    public override async Task OnJudged(PlayerChoiceContext ctx, Creature target)
    {
        if (Owner.Player is not { } player) return;
        var candles = PileType.Exhaust.GetPile(player).Cards.Where(c => c is IHealingCard).ToList();
        if (candles.Count == 0) return;
        Flash();
        var card = player.RunState.Rng.CombatCardGeneration.NextItem(candles);
        await CardPileCmd.Add(card, PileType.Hand.GetPile(player));
    }
}
