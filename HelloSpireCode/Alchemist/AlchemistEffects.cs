using HelloSpire.HelloSpireCode.Alchemist.Lab;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;

namespace HelloSpire.HelloSpireCode.Alchemist;

/// <summary>
/// Every game command the Alchemist issues funnels through here.
///
/// Same contract as the Gunslinger's equivalent: exactly one file touches the base game's command
/// signatures, so if the game changes <c>CreatureCmd.GainBlock</c> or how Gold is stored, ninety
/// cards keep compiling and only this file needs a fix.
///
/// That contract matters far more for this character than it did for the last one. The Gunslinger's
/// cylinder is entirely self-contained — a custom Power holding six slots — so nothing it did
/// needed base-game systems the mod had not already used. The Alchemist spends **Gold**, writes to
/// the **Potion belt**, and reduces **Max HP**, and this repo has no working example of any of the
/// three. See <see cref="Unverified"/>.
/// </summary>
public static class AlchemistEffects
{
    // ------------------------------------------------------------------ verified surface
    // Everything below here is on signatures this mod already has working code for.

    public static async Task GainBlock(LabContext lab, decimal amount)
    {
        if (amount <= 0) return;
        await CreatureCmd.GainBlock(lab.Self, amount, ValueProp.Move, null, false);
    }

    public static async Task GainBlock(Creature creature, decimal amount)
    {
        if (amount <= 0) return;
        await CreatureCmd.GainBlock(creature, amount, ValueProp.Move, null, false);
    }

    public static async Task LoseHp(PlayerChoiceContext ctx, LabContext lab, decimal amount)
    {
        if (amount <= 0) return;
        await CreatureCmd.Damage(ctx, lab.Self, amount,
            ValueProp.Unblockable | ValueProp.Unpowered, lab.Self, null);
    }

    public static async Task ApplyWeak(PlayerChoiceContext ctx, LabContext lab, Creature target, decimal amount)
    {
        if (amount <= 0) return;
        await PowerCmd.Apply<WeakPower>(ctx, target, amount, lab.Self, lab.Card);
    }

    public static async Task ApplyVulnerable(PlayerChoiceContext ctx, LabContext lab, Creature target, decimal amount)
    {
        if (amount <= 0) return;
        await PowerCmd.Apply<VulnerablePower>(ctx, target, amount, lab.Self, lab.Card);
    }

    public static async Task ApplyPoison(PlayerChoiceContext ctx, LabContext lab, Creature target, decimal amount)
    {
        if (amount <= 0) return;
        await PowerCmd.Apply<PoisonPower>(ctx, target, amount, lab.Self, lab.Card);
    }

    public static async Task GainStrength(PlayerChoiceContext ctx, LabContext lab, decimal amount)
    {
        if (amount <= 0) return;
        await PowerCmd.Apply<StrengthPower>(ctx, lab.Self, amount, lab.Self, lab.Card);
    }

    public static async Task GainDexterity(PlayerChoiceContext ctx, LabContext lab, decimal amount)
    {
        if (amount <= 0) return;
        await PowerCmd.Apply<DexterityPower>(ctx, lab.Self, amount, lab.Self, lab.Card);
    }

    public static async Task Draw(PlayerChoiceContext ctx, LabContext lab, int count)
    {
        if (count <= 0) return;
        await CardPileCmd.Draw(ctx, count, lab.Player);
    }

    public static async Task DrawFor(PlayerChoiceContext ctx, Player player, int count)
    {
        if (count <= 0) return;
        await CardPileCmd.Draw(ctx, count, player);
    }

    public static async Task GainEnergy(LabContext lab, decimal amount)
    {
        if (amount <= 0) return;
        await PlayerCmd.GainEnergy(amount, lab.Player);
    }

    public static async Task GainEnergyFor(Player player, decimal amount)
    {
        if (amount <= 0) return;
        await PlayerCmd.GainEnergy(amount, player);
    }

    /// <summary>The combat this bench is being used in, or null outside combat.</summary>
    public static CombatState? State(LabContext lab) =>
        (lab.Card?.CombatState ?? lab.Player.Creature.CombatState) as CombatState;

    /// <summary>Enemies that can still be hit, in combat order.</summary>
    public static IReadOnlyList<Creature> Enemies(LabContext lab)
    {
        var state = State(lab);
        return state == null ? [] : state.HittableEnemies.ToList();
    }

    public static Creature? RandomEnemy(LabContext lab)
    {
        var enemies = Enemies(lab);
        if (enemies.Count == 0) return null;
        if (enemies.Count == 1) return enemies[0];

        // NextInt's upper bound is exclusive; Count - 1 here made the last enemy unreachable --
        // the same off-by-one already found and fixed in Alchemy.ExhaustRandomOther.
        var index = lab.Player.RunState.Rng.CombatTargets.NextInt(0, enemies.Count);
        return enemies[Math.Clamp(index, 0, enemies.Count - 1)];
    }

    /// <summary>Everyone in the fight, the Alchemist included. A list of one in single-player.</summary>
    public static IReadOnlyList<Player> AllPlayers(LabContext lab)
    {
        var state = State(lab);
        return state == null ? [] : state.Players.ToList();
    }

    /// <summary>Everyone except the Alchemist. Empty in single-player.</summary>
    public static IReadOnlyList<Player> Allies(LabContext lab) =>
        AllPlayers(lab).Where(player => player != lab.Player).ToList();

    /// <summary>The ally a card was pointed at, falling back to the Alchemist when solo.</summary>
    public static Player ResolveAlly(LabContext lab, Creature? target)
    {
        if (target == null || target == lab.Self) return lab.Player;
        return AllPlayers(lab).FirstOrDefault(player => player.Creature == target) ?? lab.Player;
    }

    public static bool HasRelic<T>(LabContext lab) where T : RelicModel => HasRelic<T>(lab.Player);

    public static bool HasRelic<T>(Player player) where T : RelicModel =>
        player.Relics.Any(relic => relic is T);

    public static Player? PlayerFor(Creature creature) =>
        creature.CombatState?.Players.FirstOrDefault(player => player.Creature == creature);

    // ------------------------------------------------------------------ the bench

    /// <summary>The bench if combat has already created it, without creating one.</summary>
    public static LabPower? Peek(LabContext lab) => lab.Player.Creature?.GetPower<LabPower>();

    /// <summary>
    /// The bench, creating it if this is the first Alchemist effect of the combat.
    /// Applied silently — the bench is always there, it is not a buff the player just gained.
    /// </summary>
    public static async Task<LabPower?> Bench(PlayerChoiceContext ctx, LabContext lab)
    {
        var creature = lab.Player.Creature;
        if (creature == null) return null;

        var existing = creature.GetPower<LabPower>();
        if (existing != null) return existing;

        return await PowerCmd.Apply<LabPower>(ctx, creature, 1m, creature, lab.Card);
    }

    /// <summary>Current Potency, or zero. Only ever read by <see cref="Lab.Belt"/>.</summary>
    public static int Potency(LabContext lab) =>
        (int)(lab.Player.Creature?.GetPower<PotencyPower>()?.Amount ?? 0m);

    public static async Task GainPotency(PlayerChoiceContext ctx, LabContext lab, decimal amount)
    {
        if (amount <= 0) return;
        await PowerCmd.Apply<PotencyPower>(ctx, lab.Self, amount, lab.Self, lab.Card);
    }
}
