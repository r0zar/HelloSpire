using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models.Powers;

namespace HelloSpire.HelloSpireCode.Characters.PaladinContent.Cards;

/// <summary>
/// Gain Amount Strength this turn, and bank the seal. Judge: deal 20. The Ret rare -- temp
/// Strength (was permanent 2/cycle, the Inflame-every-shuffle problem); 4 for the burst turn
/// where the banked verdicts unleash.
/// </summary>
public sealed class SealOfTheCrusader() : SealCard(1, CardRarity.Rare, 4m)
{
    protected override async Task Arm(PlayerChoiceContext ctx, decimal amount)
    {
        await PowerCmd.Apply<SealOfTheCrusaderStrengthPower>(ctx, Owner.Creature, amount, Owner.Creature, this);
        await Seals.Grant<SealOfTheCrusaderPower>(ctx, Owner, 1m, this);
    }
}
