using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.CardSelection;
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
    /// <summary>
    /// Ask the player to choose and discard <paramref name="count"/> cards from hand, on the
    /// game's own discard-selection screen (the Acrobatics/Dagger Throw path -- NOT the
    /// choose-a-card popup, which hard-caps at three cards and threw on a full hand).
    /// Discarding fewer is fine when the hand runs dry: effect first, discard as trailing cost.
    /// </summary>
    public static async Task DiscardChosen(PlayerChoiceContext ctx, Player player, int count, AbstractModel source)
    {
        var chosen = (await CardSelectCmd.FromHandForDiscard(ctx, player,
            new CardSelectorPrefs(CardSelectorPrefs.DiscardSelectionPrompt, count), null, source)).ToList();
        if (chosen.Count > 0)
            await CardCmd.Discard(ctx, chosen);
    }

    /// <summary>The living player creature with the lowest HP fraction -- "the most wounded player".</summary>
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
