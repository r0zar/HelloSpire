using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models.Powers;

namespace HelloSpire.HelloSpireCode.Characters.PaladinContent.Cards;

/// <summary>
/// Apply Amount Weak and Amount Vulnerable to ALL enemies now, bank the seal.
/// Judge: 2 Weak + 2 Vulnerable. The debuff seal commands the field on the way in.
/// </summary>
public sealed class SealOfCommand() : SealCard(1, CardRarity.Uncommon, 1m)
{
    protected override async Task Arm(PlayerChoiceContext ctx, decimal amount)
    {
        var state = Owner.Creature.CombatState;
        foreach (var enemy in state.HittableEnemies.ToList())
        {
            await PowerCmd.Apply<WeakPower>(ctx, enemy, amount, Owner.Creature, this);
            await PowerCmd.Apply<VulnerablePower>(ctx, enemy, amount, Owner.Creature, this);
        }
        await Seals.Grant<SealOfCommandPower>(ctx, Owner, amount, this);
    }
}
