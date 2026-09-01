using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models.Powers;

namespace HelloSpire.HelloSpireCode.Characters.PaladinContent.Cards;

/// <summary>
/// Gain Amount Strength now, and bank the seal. Judge: deal 20. The Ret rare pays its ramp
/// up front and keeps the hammer blow banked.
/// </summary>
public sealed class SealOfTheCrusader() : SealCard(1, CardRarity.Rare, 2m)
{
    protected override async Task Arm(PlayerChoiceContext ctx, decimal amount)
    {
        await PowerCmd.Apply<StrengthPower>(ctx, Owner.Creature, amount, Owner.Creature, this);
        await Seals.Grant<SealOfTheCrusaderPower>(ctx, Owner, 1m, this);
    }
}
