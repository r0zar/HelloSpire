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

    public static Task Gain(PlayerChoiceContext ctx, Player p, int n, CardModel? source = null) =>
        PowerCmd.Apply<SpiritPower>(ctx, p.Creature, n, p.Creature, source);

    /// <summary>A heal boosted by the healer's Spirit. All Paladin heal cards route through this.</summary>
    public static Task Heal(Player healer, decimal baseAmount) =>
        Heal(healer, healer.Creature, baseAmount);

    /// <summary>Heal any target with the caster's Spirit added -- the ally-heal form.</summary>
    public static Task Heal(Player healer, Creature target, decimal baseAmount) =>
        CreatureCmd.Heal(target, baseAmount + Of(healer));
}
