using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;

namespace HelloSpire.HelloSpireCode.Characters.PaladinContent;

/// <summary>
/// Shared verbs for Paladin cards, in the Spirit/Seals house style: cards read as design
/// language, the mechanics live in one place.
/// </summary>
public static class PaladinEffects
{
    /// <summary>The hand, minus the card that is asking (it may still be mid-resolution).</summary>
    public static IReadOnlyList<CardModel> Hand(Player player, CardModel? excluding = null) =>
        PileType.Hand.GetPile(player).Cards.Where(c => c != excluding).ToList();

    /// <summary>
    /// Ask the player to choose and discard <paramref name="count"/> cards, one at a time.
    /// Discarding fewer is fine when the hand runs dry -- the effect already happened
    /// (effect first, discard as trailing cost).
    /// </summary>
    public static async Task DiscardChosen(PlayerChoiceContext ctx, Player player, int count, CardModel? excluding = null)
    {
        for (var i = 0; i < count; i++)
        {
            var hand = Hand(player, excluding);
            if (hand.Count == 0) return;
            var chosen = await CardSelectCmd.FromChooseACardScreen(ctx, hand, player);
            if (chosen == null) return;
            await CardCmd.Discard(ctx, chosen);
        }
    }

    /// <summary>The living player creature with the lowest current HP -- "the most wounded player".</summary>
    public static Creature MostWounded(Creature self)
    {
        var players = self.CombatState.PlayerCreatures.Where(c => c.IsAlive).ToList();
        return players.Count == 0 ? self : players.OrderBy(c => (double)c.CurrentHp / c.MaxHp).First();
    }

    /// <summary>A random living enemy, or null when the field is empty.</summary>
    public static Creature? RandomEnemy(Player owner)
    {
        var state = owner.Creature.CombatState;
        var enemies = state.HittableEnemies;
        return enemies.Count == 0 ? null : owner.RunState.Rng.CombatTargets.NextItem(enemies);
    }
}
