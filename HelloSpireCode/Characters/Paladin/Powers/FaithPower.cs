using System.Threading.Tasks;
using HelloSpire.HelloSpireCode.Powers;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;

namespace HelloSpire.HelloSpireCode.Characters.PaladinContent;

/// <summary>
/// Faith, the Paladin's stat: what Strength is to attacks and Dexterity is to Block, Faith is to
/// healing. Sits in the power bar next to Strength ("power" is just the engine's word for any
/// per-combat stat on a creature; the player sees a gold icon titled Faith).
///
/// The game has no combat-heal modify hook (only rest-site heals have one), so this class holds
/// no logic: Paladin heal cards add the owner's Faith at heal time via <see cref="Faith.Heal"/>.
/// </summary>
public sealed class FaithPower : HelloSpirePower
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;
}

/// <summary>Helpers so cards read as design language: Faith.Of, Faith.Gain, Faith.Heal.</summary>
public static class Faith
{
    public static int Of(Player p) => p.Creature.GetPowerAmount<FaithPower>();

    public static Task Gain(PlayerChoiceContext ctx, Player p, int n, CardModel? source = null) =>
        PowerCmd.Apply<FaithPower>(ctx, p.Creature, n, p.Creature, source);

    /// <summary>A heal boosted by the healer's Faith. All Paladin heal cards route through this.</summary>
    public static Task Heal(Player healer, decimal baseAmount) =>
        CreatureCmd.Heal(healer.Creature, baseAmount + Of(healer));
}
