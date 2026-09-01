using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;

namespace HelloSpire.HelloSpireCode.Characters.PaladinContent.Cards;

/// <summary>Seal. While held: discards ping Amount at a random enemy. Judge: 6 to ALL. The Tithe seal.</summary>
public sealed class SealOfThePenitent() : SealCard(1, CardRarity.Common, 2m)
{
    protected override Task Arm(PlayerChoiceContext ctx, decimal amount) =>
        Seals.Grant<SealOfThePenitentPower>(ctx, Owner, amount, this);
}
