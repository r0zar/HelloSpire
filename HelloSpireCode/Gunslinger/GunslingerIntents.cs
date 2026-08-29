using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.MonsterMoves.Intents;

namespace HelloSpire.HelloSpireCode.Gunslinger;

/// <summary>
/// Reading enemy intents.
///
/// Three cards ask what the enemy is about to do — Showdown fires again into a telegraphed Attack,
/// Dive for Cover scales off how much is incoming, and the answer decides what Custom Load chambers.
/// The damage figure is only ever an estimate, so a read that fails degrades to "nothing is coming"
/// rather than throwing in the middle of a card.
/// </summary>
public static class GunslingerIntents
{
    /// <summary>True when this creature is telegraphing at least one Attack.</summary>
    public static bool IntendsToAttack(Creature creature)
    {
        var intents = creature.Monster?.NextMove?.Intents;
        return intents != null && intents.Any(intent => intent is AttackIntent);
    }

    /// <summary>Total telegraphed Attack damage from this creature, repeats included.</summary>
    public static int AttackDamageFrom(Creature creature, IReadOnlyList<Creature> targets)
    {
        var intents = creature.Monster?.NextMove?.Intents;
        if (intents == null) return 0;

        var total = 0;
        foreach (var intent in intents)
        {
            if (intent is not AttackIntent attack) continue;

            try
            {
                total += attack.GetTotalDamage(targets, creature);
            }
            catch (Exception e)
            {
                MainFile.Logger.Info($"Could not total intent damage for {creature}: {e.Message}");
            }
        }
        return total;
    }

    /// <summary>Total telegraphed Attack damage across every enemy still in the fight.</summary>
    public static int TotalIncomingAttackDamage(GunContext gun)
    {
        var state = GunslingerEffects.State(gun);
        if (state == null) return 0;

        var targets = state.Players.Select(player => player.Creature).ToList();
        return GunslingerEffects.Enemies(gun).Sum(enemy => AttackDamageFrom(enemy, targets));
    }
}
