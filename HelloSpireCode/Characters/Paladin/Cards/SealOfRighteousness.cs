using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models.Powers;

namespace HelloSpire.HelloSpireCode.Characters.PaladinContent.Cards;

/// <summary>
/// Gain Amount Strength this turn, bank the seal. Judge: deal 10. The Ret on-ramp -- temp
/// Strength (was permanent: +1 per deck cycle made act 1 trivial); build the turn, then swing.
/// </summary>
public sealed class SealOfRighteousness() : SealCard(1, CardRarity.Common, 2m)
{
    protected override async Task Arm(PlayerChoiceContext ctx, decimal amount)
    {
        await PowerCmd.Apply<SealOfRighteousnessStrengthPower>(ctx, Owner.Creature, amount, Owner.Creature, this);
        await Seals.Grant<SealOfRighteousnessPower>(ctx, Owner, 1m, this);
    }
}
