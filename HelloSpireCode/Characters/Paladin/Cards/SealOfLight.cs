using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;

namespace HelloSpire.HelloSpireCode.Characters.PaladinContent.Cards;

/// <summary>Seal. While held: Amount Spirit per turn. Judge: draw 3, discard 2. The Holy engine.</summary>
public sealed class SealOfLight() : SealCard(2, CardRarity.Uncommon, 1m)
{
    protected override Task Arm(PlayerChoiceContext ctx, decimal amount) =>
        Seals.Grant<SealOfLightPower>(ctx, Owner, amount, this);
}
