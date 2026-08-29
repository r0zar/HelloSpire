using HelloSpire.HelloSpireCode.Gunslinger.Powers;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;

namespace HelloSpire.HelloSpireCode.Gunslinger;

/// <summary>
/// Every game command the Gunslinger issues funnels through here.
///
/// The point is to have exactly one place that touches the base game's command signatures: if the
/// game changes <c>CreatureCmd.GainBlock</c> or <c>PowerCmd.Apply</c>, eighty cards keep compiling
/// and only this file needs a fix.
/// </summary>
public static class GunslingerEffects
{
    public static async Task GainBlock(GunContext gun, decimal amount)
    {
        if (amount <= 0) return;
        await CreatureCmd.GainBlock(gun.Self, amount, ValueProp.Move, null, false);
    }

    public static async Task LoseHp(PlayerChoiceContext ctx, GunContext gun, decimal amount)
    {
        if (amount <= 0) return;
        await CreatureCmd.Damage(ctx, gun.Self, amount,
            ValueProp.Unblockable | ValueProp.Unpowered, gun.Self, null);
    }

    public static async Task ApplyWeak(PlayerChoiceContext ctx, GunContext gun, Creature target, decimal amount)
    {
        if (amount <= 0) return;
        await PowerCmd.Apply<WeakPower>(ctx, target, amount, gun.Self, gun.Card);
        await GunslingerHooks.NotifyWeakApplied(ctx, gun, target, (int)amount);
    }

    public static async Task ApplyDebilitate(PlayerChoiceContext ctx, GunContext gun, Creature target, decimal amount)
    {
        if (amount <= 0) return;
        await PowerCmd.Apply<DebilitatePower>(ctx, target, amount, gun.Self, gun.Card);
    }

    public static async Task GainDeadeye(PlayerChoiceContext ctx, GunContext gun, decimal amount)
    {
        if (amount <= 0) return;
        await PowerCmd.Apply<DeadeyePower>(ctx, gun.Self, amount, gun.Self, gun.Card);
    }

    public static async Task GainArmor(PlayerChoiceContext ctx, GunContext gun, decimal amount)
    {
        if (amount <= 0) return;
        await PowerCmd.Apply<ArmorPower>(ctx, gun.Self, amount, gun.Self, gun.Card);

        // Reversal asks whether Armor was gained this turn, and the cylinder is where per-turn
        // Gunslinger state lives.
        var cylinder = await Cylinder.Revolver.Get(ctx, gun);
        if (cylinder != null) cylinder.ArmorGainedThisTurn = true;
    }

    public static async Task GainDodge(PlayerChoiceContext ctx, GunContext gun, decimal amount)
    {
        if (amount <= 0) return;
        await PowerCmd.Apply<DodgePower>(ctx, gun.Self, amount, gun.Self, gun.Card);
        await GunslingerHooks.NotifyDodgeGained(ctx, gun, (int)amount);
    }

    public static async Task Draw(PlayerChoiceContext ctx, GunContext gun, int count)
    {
        if (count <= 0) return;
        await CardPileCmd.Draw(ctx, count, gun.Player);
    }

    public static async Task GainEnergy(GunContext gun, decimal amount)
    {
        if (amount <= 0) return;
        await PlayerCmd.GainEnergy(amount, gun.Player);
    }

    /// <summary>
    /// The combat this gun is being used in. Cards know it directly; relics and potions have to ask
    /// the combat manager, since they have no card to route through.
    /// </summary>
    public static CombatState? State(GunContext gun) =>
        (gun.Card?.CombatState ?? gun.Player.Creature.CombatState) as CombatState;

    /// <summary>Enemies that can still be hit, in combat order. Empty when there is no combat.</summary>
    public static IReadOnlyList<Creature> Enemies(GunContext gun)
    {
        var state = State(gun);
        return state == null ? [] : state.HittableEnemies.ToList();
    }

    /// <summary>Every enemy other than <paramref name="except"/>. Ricochet and Crossfire splash.</summary>
    public static IReadOnlyList<Creature> OtherEnemies(GunContext gun, Creature? except)
    {
        return Enemies(gun).Where(enemy => enemy != except).ToList();
    }

    /// <summary>A random hittable enemy, or null when the fight is already over.</summary>
    public static Creature? RandomEnemy(GunContext gun)
    {
        var enemies = Enemies(gun);
        if (enemies.Count == 0) return null;
        if (enemies.Count == 1) return enemies[0];

        var index = gun.Player.RunState.Rng.CombatTargets.NextInt(0, enemies.Count - 1);
        return enemies[Math.Clamp(index, 0, enemies.Count - 1)];
    }

    /// <summary>True when the enemy is carrying an Attack intent this turn.</summary>
    public static bool IntendsToAttack(Creature creature) => GunslingerIntents.IntendsToAttack(creature);

    /// <summary>Total incoming Attack damage this turn across every enemy. Dive for Cover reads this.</summary>
    public static int IncomingAttackDamage(GunContext gun) => GunslingerIntents.TotalIncomingAttackDamage(gun);

    public static bool HasRelic<T>(GunContext gun) where T : RelicModel => HasRelic<T>(gun.Player);

    public static bool HasRelic<T>(Player player) where T : RelicModel =>
        player.Relics.Any(relic => relic is T);

    /// <summary>The Player that owns this creature, when only the creature is in hand (power hooks).</summary>
    public static Player? PlayerFor(Creature creature)
    {
        return creature.CombatState?.Players
            .FirstOrDefault(player => player.Creature == creature);
    }
}
