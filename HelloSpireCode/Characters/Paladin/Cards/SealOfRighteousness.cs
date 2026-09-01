using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models.Powers;

namespace HelloSpire.HelloSpireCode.Characters.PaladinContent.Cards;

/// <summary>Gain Amount Strength now, bank the seal. Judge: deal 10. The Ret on-ramp.</summary>
public sealed class SealOfRighteousness() : SealCard(1, CardRarity.Common, 1m)
{
    protected override async Task Arm(PlayerChoiceContext ctx, decimal amount)
    {
        await PowerCmd.Apply<StrengthPower>(ctx, Owner.Creature, amount, Owner.Creature, this);
        await Seals.Grant<SealOfRighteousnessPower>(ctx, Owner, 1m, this);
    }
}
