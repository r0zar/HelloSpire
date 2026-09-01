using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;

namespace HelloSpire.HelloSpireCode.Characters.PaladinContent.Cards;

/// <summary>Seal. While held: Attacks +2. Judge: deal 10. The Ret on-ramp.</summary>
public sealed class SealOfRighteousness() : SealCard(1, CardRarity.Common, 2m)
{
    protected override Task Arm(PlayerChoiceContext ctx, decimal amount) =>
        Seals.Grant<SealOfRighteousnessPower>(ctx, Owner, amount, this);
}
