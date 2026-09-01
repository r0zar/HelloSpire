using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;

namespace HelloSpire.HelloSpireCode.Characters.PaladinContent.Cards;

/// <summary>Seal. While held: your debuffs get +Amount stacks. Judge: 2 Weak + 2 Vulnerable.</summary>
public sealed class SealOfCommand() : SealCard(1, CardRarity.Uncommon, 1m)
{
    protected override Task Arm(PlayerChoiceContext ctx, decimal amount) =>
        Seals.Grant<SealOfCommandPower>(ctx, Owner, amount, this);
}
