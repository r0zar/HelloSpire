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
/// A Seal is a banked charge: casting one adds it to what you hold (seals STACK -- casting a
/// second copy of the same seal strengthens its aura). Some seals pay an effect up front, some
/// carry an aura while armed. Judging unleashes EVERY armed seal's payoff, then consumes them
/// all -- the bank-versus-cash decision is the whole game. A seal-less Judge still counts as
/// judging (IJudgeTrigger powers fire); it simply has no bank to cash.
/// </summary>
public abstract class SealPower : HelloSpirePower
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;

    /// <summary>This Seal's judge payoff. Runs once per judge instance; the seal is consumed after.</summary>
    public abstract Task OnJudged(PlayerChoiceContext ctx, Creature target);

    /// <summary>Chained Gauntlet: armed auras off, judge payoffs untouched.</summary>
    protected bool PassivesDisabled =>
        Owner.Player != null && Owner.Player.Relics.OfType<Relics.ChainedGauntlet>().Any();
}

/// <summary>
/// A power that reacts to the Judge verb itself (Zealotry, Sanctified Wrath, Vow of Enmity,
/// Avenging Crusader). Fires once per judge instance, seals banked or not.
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
    /// Bank a seal. Seals stack: a new seal joins the bank; a second copy of the same seal
    /// merges into its power (Counter stacking) and strengthens the aura.
    /// </summary>
    public static Task Grant<T>(PlayerChoiceContext ctx, Player p, decimal amount, CardModel? source = null)
        where T : SealPower =>
        PowerCmd.Apply<T>(ctx, p.Creature, amount, p.Creature, source);

    /// <summary>
    /// Judge a target N times. Avenging Wrath doubles the instance count. Each instance fires
    /// EVERY banked seal's payoff and every IJudgeTrigger power; the whole bank is consumed at
    /// the end.
    /// </summary>
    public static async Task Judge(PlayerChoiceContext ctx, Player p, Creature target, int times = 1)
    {
        var creature = p.Creature;
        var instances = times * (creature.HasPower<AvengingWrathPower>() ? 2 : 1);
        var bank = creature.GetPowerInstances<SealPower>().ToList();

        for (var i = 0; i < instances; i++)
        {
            foreach (var seal in bank)
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

        foreach (var seal in bank)
            await PowerCmd.Remove(seal);
    }

    /// <summary>
    /// Judge EACH of the given targets once (Tribunal, Thunderous Smite): every banked seal's
    /// payoff and every IJudgeTrigger fire per target; the bank is consumed once at the end.
    /// </summary>
    public static async Task JudgeEach(PlayerChoiceContext ctx, Player p, IReadOnlyList<Creature> targets)
    {
        var creature = p.Creature;
        var passes = creature.HasPower<AvengingWrathPower>() ? 2 : 1;
        var bank = creature.GetPowerInstances<SealPower>().ToList();

        for (var i = 0; i < passes; i++)
            foreach (var target in targets)
            {
                foreach (var seal in bank)
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

        foreach (var seal in bank)
            await PowerCmd.Remove(seal);
    }
}
