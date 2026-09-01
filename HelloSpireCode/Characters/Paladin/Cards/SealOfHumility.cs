using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;

namespace HelloSpire.HelloSpireCode.Characters.PaladinContent.Cards;

/// <summary>Seal. While held: heals restore +Amount. Judge: -3 Str for one turn. The Holy anchor.</summary>
public sealed class SealOfHumility() : SealCard(1, CardRarity.Common, 3m)
{
    protected override Task Arm(PlayerChoiceContext ctx, decimal amount) =>
        Seals.Grant<SealOfHumilityPower>(ctx, Owner, amount, this);
}
