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

/// <summary>Helpers so cards read as design language: Spirit.Of, Spirit.Gain, Spirit.Heal.</summary>
public static class Spirit
{
    public static int Of(Player p) => p.Creature.GetPowerAmount<SpiritPower>();

    public static async Task Gain(PlayerChoiceContext ctx, Player p, int n, CardModel? source = null)
    {
        // Libram of Wrath: fuel bought with the heal identity -- no Spirit while held.
        if (p.Relics.OfType<Relics.LibramOfWrath>().Any()) return;
        await PowerCmd.Apply<SpiritPower>(ctx, p.Creature, n, p.Creature, source);

        // Aura of Mercy: the bearer's Spirit gains pulse the whole team, flat -- adding Spirit on
        // top here would double-dip the very stat being gained.
        if (n > 0 && p.Creature.GetPower<AuraOfMercyPower>() is { } aura && p.Creature.CombatState is { } state)
        {
            aura.Flash();
            foreach (var ally in state.PlayerCreatures.Where(c => c.IsAlive))
                await CreatureCmd.Heal(ally, n);
        }
    }

    /// <summary>A heal boosted by the healer's Spirit. All Paladin heal cards route through this.</summary>
    public static Task Heal(Player healer, decimal baseAmount) =>
        Heal(healer, healer.Creature, baseAmount);

    /// <summary>
    /// Heal any target with the caster's Spirit added -- the ally-heal form. Also the funnel
    /// where Beacon of Light rides: any other player bearing the Beacon heals its Amount too.
    /// </summary>
    public static async Task Heal(Player healer, Creature target, decimal baseAmount)
    {
        // Seal of Humility: while held, every heal restores Amount more -- the Holy analog of
        // Righteousness's attack bonus, read here because this funnel is where heals live.
        var humility = Seals.Active(healer.Creature) is SealOfHumilityPower h ? h.HealBonus : 0m;
        await CreatureCmd.Heal(target, baseAmount + Of(healer) + humility);
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
