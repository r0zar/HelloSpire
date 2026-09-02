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
    /// Tithe cards glow gold on this screen the way Sly cards do in the base game -- the
    /// engine's FromHandForDiscard hardcodes the Sly glow, so we set our own delegate
    /// (Tithe OR the base Sly condition) and call FromHand directly.
    /// </summary>
    public static async Task DiscardChosen(PlayerChoiceContext ctx, Player player, int count, AbstractModel source)
    {
        var prefs = new CardSelectorPrefs(CardSelectorPrefs.DiscardSelectionPrompt, count)
        {
            ShouldGlowGold = c =>
                c is PaladinCard { HasTithe: true } ||
                (c.IsSlyThisTurn && (c.CanPlay(out var reason, out _) || reason.HasResourceCostReason())),
        };
        var chosen = (await CardSelectCmd.FromHand(ctx, player, prefs, null, source)).ToList();
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
