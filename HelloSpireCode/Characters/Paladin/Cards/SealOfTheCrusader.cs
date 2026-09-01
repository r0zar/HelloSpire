using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;

namespace HelloSpire.HelloSpireCode.Characters.PaladinContent.Cards;

/// <summary>Seal. While held: Amount Strength per turn. Judge: deal 20. The Ret rare.</summary>
public sealed class SealOfTheCrusader() : SealCard(1, CardRarity.Rare, 1m)
{
    protected override Task Arm(PlayerChoiceContext ctx, decimal amount) =>
        Seals.Grant<SealOfTheCrusaderPower>(ctx, Owner, amount, this);
}
