using System;
using System.Linq;
using System.Threading.Tasks;
using HelloSpire.HelloSpireCode.Powers;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;

namespace HelloSpire.HelloSpireCode.Characters.PaladinContent;

/// <summary>
/// Spirit, the Paladin's stat: what Strength is to attacks and Dexterity is to Block, Spirit is to
/// healing. Sits in the power bar next to Strength ("power" is just the engine's word for any
/// per-combat stat on a creature; the player sees a gold icon titled Spirit).
///
/// The game has no combat-heal modify hook (only rest-site heals have one), so this class holds
/// no logic: Paladin heal cards add the owner's Spirit at heal time via <see cref="Spirit.Heal"/>.
/// </summary>
public sealed class SpiritPower : HelloSpirePower
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;
}

/// <summary>
/// Spirit in the red: the light dimmed. Effective Spirit = SpiritPower - SpiritDebtPower, so
/// while in debt, heals restore less. Spirit gains pay the debt down before banking any Spirit.
/// A separate debuff power because the engine removes a Counter power at zero -- it cannot
/// itself hold a negative number.
/// </summary>
public sealed class SpiritDebtPower : HelloSpirePower
{
    public override PowerType Type => PowerType.Debuff;
    public override PowerStackType StackType => PowerStackType.Counter;
}

/// <summary>Helpers so cards read as design language: Spirit.Of, Spirit.Gain, Spirit.Heal.</summary>
public static class Spirit
{
    /// <summary>Effective Spirit: banked minus debt. Can be negative.</summary>
    public static int Of(Player p) =>
        p.Creature.GetPowerAmount<SpiritPower>() - p.Creature.GetPowerAmount<SpiritDebtPower>();

    public static async Task Gain(PlayerChoiceContext ctx, Player p, int n, CardModel? source = null)
    {
        if (n == 0) return;

        if (n > 0)
        {
            // Libram of Wrath: fuel bought with the heal identity -- no Spirit while held.
            // (Losses still land: the Libram spares you nothing.)
            if (p.Relics.OfType<Relics.LibramOfWrath>().Any()) return;

            // Gains pay down debt first; only the remainder banks as Spirit.
            var debt = p.Creature.GetPowerAmount<SpiritDebtPower>();
            var payment = Math.Min(debt, n);
            if (payment > 0)
                await PowerCmd.Apply<SpiritDebtPower>(ctx, p.Creature, -payment, p.Creature, source);
            var remainder = n - payment;
            if (remainder > 0)
                await PowerCmd.Apply<SpiritPower>(ctx, p.Creature, remainder, p.Creature, source);

            // Aura of Mercy: mercy softens the blow before it lands -- the bearer's Spirit gains
            // shave that much Strength off ALL enemies until end of turn. Loop-proof (no heal),
            // and every Spirit engine becomes party protection.
            if (p.Creature.GetPower<AuraOfMercyPower>() is { } aura && p.Creature.CombatState is { } state)
            {
                aura.Flash();
                foreach (var enemy in state.HittableEnemies.ToList())
                    await PowerCmd.Apply<HumblingShacklesPower>(ctx, enemy, n, p.Creature, null);
            }
        }
        else
        {
            // Losses drain banked Spirit first; anything beyond that becomes debt. The light dims.
            var loss = -n;
            var banked = p.Creature.GetPowerAmount<SpiritPower>();
            var taken = Math.Min(banked, loss);
            if (taken > 0)
                await PowerCmd.Apply<SpiritPower>(ctx, p.Creature, -taken, p.Creature, source);
            var debt = loss - taken;
            if (debt > 0)
                await PowerCmd.Apply<SpiritDebtPower>(ctx, p.Creature, debt, p.Creature, source);
        }
    }

    /// <summary>A heal boosted by the healer's Spirit. All Paladin heal cards route through this.</summary>
    public static Task Heal(Player healer, decimal baseAmount) =>
        Heal(healer, healer.Creature, baseAmount);

    /// <summary>
    /// Heal any target with the caster's Spirit added -- the ally-heal form (Spirit debt reduces
    /// it, floored at zero). Also the funnel where Beacon of Light rides: any other player
    /// bearing the Beacon heals its Amount too.
    /// </summary>
    public static async Task Heal(Player healer, Creature target, decimal baseAmount)
    {
        await CreatureCmd.Heal(target, Math.Max(0m, baseAmount + Of(healer)));
        if (target.CombatState is not { } state) return;
        foreach (var bearer in state.PlayerCreatures)
        {
            if (bearer == target || bearer.IsDead) continue;
            if (bearer.GetPower<BeaconOfLightPower>() is not { } beacon) continue;
            beacon.Flash();
            await CreatureCmd.Heal(bearer, beacon.Amount);
        }
    }
}
