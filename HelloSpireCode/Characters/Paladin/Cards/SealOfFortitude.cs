using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;

namespace HelloSpire.HelloSpireCode.Characters.PaladinContent.Cards;

/// <summary>Seal. While held: Amount Plating per turn. Judge: 10 Block. The Prot seal.</summary>
public sealed class SealOfFortitude() : SealCard(1, CardRarity.Common, 1m)
{
    protected override Task Arm(PlayerChoiceContext ctx, decimal amount) =>
        Seals.Grant<SealOfFortitudePower>(ctx, Owner, amount, this);
}
