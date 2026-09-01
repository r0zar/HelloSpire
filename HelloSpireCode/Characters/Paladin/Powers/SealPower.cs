using System.Collections.Generic;
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
/// A Seal is a held stance: exactly ONE at a time. Casting any seal (a new one or a second copy
/// of the held one) replaces what is held -- re-arm, never stack. Judging fires the held seal's
/// payoff once per judge instance and then CONSUMES the seal. A seal-less Judge still counts as
/// judging (IJudgeTrigger powers fire); it simply has no seal payoff to cash.
/// </summary>
public abstract class SealPower : HelloSpirePower
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;

    /// <summary>This Seal's judge payoff. Runs once per judge instance; the seal is consumed after.</summary>
    public abstract Task OnJudged(PlayerChoiceContext ctx, Creature target);

    /// <summary>Chained Gauntlet: passives off, judge payoffs untouched.</summary>
    protected bool PassivesDisabled =>
        Owner.Player != null && Owner.Player.Relics.OfType<Relics.ChainedGauntlet>().Any();
}

/// <summary>
/// A power that reacts to the Judge verb itself (Zealotry, Sanctified Wrath, Vow of Enmity,
/// Avenging Crusader). Fires once per judge instance, seal or no seal.
/// </summary>
public interface IJudgeTrigger
{
    Task OnJudgeInstance(PlayerChoiceContext ctx, Creature target);
}

/// <summary>Grant and Judge Seals. All Seal logic funnels through here.</summary>
public static class Seals
{
    public static SealPower? Active(Creature creature) =>
        creature.GetPowerInstances<SealPower>().FirstOrDefault();

    /// <summary>
    /// Hold a seal. Any seal already held is removed first -- one seal, re-armed fresh, never
    /// stacked. Removing before applying means recasting the held seal resets it to full.
    /// </summary>
    public static async Task Grant<T>(PlayerChoiceContext ctx, Player p, decimal amount, CardModel? source = null)
        where T : SealPower
    {
        foreach (var held in p.Creature.GetPowerInstances<SealPower>().ToList())
            await PowerCmd.Remove(held);
        await PowerCmd.Apply<T>(ctx, p.Creature, amount, p.Creature, source);
    }

    /// <summary>
    /// Judge a target N times. Avenging Wrath doubles the instance count. Each instance fires the
    /// held seal's payoff (if any) and every IJudgeTrigger power; the seal is consumed at the end.
    /// </summary>
    public static async Task Judge(PlayerChoiceContext ctx, Player p, Creature target, int times = 1)
    {
        var creature = p.Creature;
        var instances = times * (creature.HasPower<AvengingWrathPower>() ? 2 : 1);
        var seal = Active(creature);

        for (var i = 0; i < instances; i++)
        {
            if (seal != null)
            {
                seal.Flash();
                await seal.OnJudged(ctx, target);
            }
            foreach (var trigger in creature.Powers.OfType<IJudgeTrigger>().ToList())
            {
                (trigger as HelloSpirePower)?.Flash();
                await trigger.OnJudgeInstance(ctx, target);
            }
        }

        if (seal != null)
            await PowerCmd.Remove(seal);
    }

    /// <summary>
    /// Judge EACH of the given targets once (Tribunal, Thunderous Smite): the held seal's payoff
    /// and every IJudgeTrigger fire per target, and the seal is consumed once at the end.
    /// </summary>
    public static async Task JudgeEach(PlayerChoiceContext ctx, Player p, IReadOnlyList<Creature> targets)
    {
        var creature = p.Creature;
        var passes = creature.HasPower<AvengingWrathPower>() ? 2 : 1;
        var seal = Active(creature);

        for (var i = 0; i < passes; i++)
            foreach (var target in targets)
            {
                if (seal != null)
                {
                    seal.Flash();
                    await seal.OnJudged(ctx, target);
                }
                foreach (var trigger in creature.Powers.OfType<IJudgeTrigger>().ToList())
                {
                    (trigger as HelloSpirePower)?.Flash();
                    await trigger.OnJudgeInstance(ctx, target);
                }
            }

        if (seal != null)
            await PowerCmd.Remove(seal);
    }
}
