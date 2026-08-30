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
/// A Seal: the Paladin's answer to the Defect's orbs, with a one-slot rule. A Seal is a passive
/// buff while it is active (each subclass hooks whatever it modifies), and Judgment triggers its
/// <see cref="OnJudged"/> effect. Seals persist -- Judging does not consume them.
///
/// One at a time is the whole tension: a new Seal replaces the old, so which Seal is up decides
/// both your passive and what every Judgment does.
/// </summary>
public abstract class SealPower : HelloSpirePower
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Single;

    /// <summary>What happens when this Seal is Judged. The Seal persists.</summary>
    public abstract Task OnJudged(PlayerChoiceContext ctx, Creature target);
}

/// <summary>Grant, find, and Judge Seals. All Seal logic funnels through here.</summary>
public static class Seals
{
    public static SealPower? Active(Creature creature) =>
        creature.GetPowerInstances<SealPower>().FirstOrDefault();

    /// <summary>Grant a Seal, replacing whatever Seal is already up (one-slot rule).</summary>
    public static async Task Grant<T>(PlayerChoiceContext ctx, Player p, decimal amount, CardModel? source = null)
        where T : SealPower
    {
        foreach (var old in p.Creature.GetPowerInstances<SealPower>().Where(s => s is not T).ToList())
            await PowerCmd.Remove(old);
        await PowerCmd.Apply<T>(ctx, p.Creature, amount, p.Creature, source);
    }

    /// <summary>Judge: trigger the active Seal's effect. The Seal stays. No Seal, no effect.</summary>
    public static async Task Judge(PlayerChoiceContext ctx, Player p, Creature target)
    {
        if (Active(p.Creature) is not { } seal) return;
        seal.Flash();
        await seal.OnJudged(ctx, target);
    }
}
