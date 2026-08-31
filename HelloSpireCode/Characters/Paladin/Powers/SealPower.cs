using System.Linq;
using System.Threading.Tasks;
using HelloSpire.HelloSpireCode.Powers;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;

namespace HelloSpire.HelloSpireCode.Characters.PaladinContent;

/// <summary>
/// A Seal is an ordinary buff power with one extra face: a Judgment effect, fired when the
/// player Judges (Judgment, Exorcism, Divine Purpose, Shield of the Righteous). Stack as many
/// Seals as you draft -- each is deliberately small, and Judgment triggers them ALL, so seal
/// count is the scaling axis and Judgment is the payoff. Replaying a Seal stacks its Amount.
/// </summary>
public abstract class SealPower : HelloSpirePower
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;

    /// <summary>This Seal's Judgment effect. The Seal persists.</summary>
    public abstract Task OnJudged(PlayerChoiceContext ctx, Creature target);

    /// <summary>Chained Gauntlet: passives off, Judgment effects untouched.</summary>
    protected bool PassivesDisabled =>
        Owner.Player != null && Owner.Player.Relics.OfType<Relics.ChainedGauntlet>().Any();
}

/// <summary>Grant and Judge Seals. All Seal logic funnels through here.</summary>
public static class Seals
{
    public static SealPower? Active(Creature creature) =>
        creature.GetPowerInstances<SealPower>().FirstOrDefault();

    public static Task Grant<T>(PlayerChoiceContext ctx, Player p, decimal amount, CardModel? source = null)
        where T : SealPower =>
        PowerCmd.Apply<T>(ctx, p.Creature, amount, p.Creature, source);

    /// <summary>
    /// Judge: trigger every active Seal's effect (twice with Avenging Wrath). Seals persist.
    /// </summary>
    public static async Task Judge(PlayerChoiceContext ctx, Player p, Creature target)
    {
        var seals = p.Creature.GetPowerInstances<SealPower>().ToList();
        if (seals.Count == 0) return;
        var triggers = p.Creature.HasPower<AvengingWrathPower>() ? 2 : 1;
        for (var i = 0; i < triggers; i++)
            foreach (var seal in seals)
            {
                seal.Flash();
                await seal.OnJudged(ctx, target);
            }
        if (p.Creature.GetPower<ZealotryPower>() is { } zealotry)
        {
            zealotry.Flash();
            // Avenging Wrath doubles everything a Judgment does, Zealotry's Strength included.
            await PowerCmd.Apply<StrengthPower>(ctx, p.Creature, zealotry.Amount * triggers, p.Creature, null);
        }
    }
}
