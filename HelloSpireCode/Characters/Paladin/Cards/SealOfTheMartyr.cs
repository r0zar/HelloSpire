using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models.Powers;

namespace HelloSpire.HelloSpireCode.Characters.PaladinContent.Cards;

/// <summary>
/// Gain Amount Thorns this turn, bank the seal. Judge: ALL enemies lose 5 Strength this turn.
/// The Prot emergency -- temp Thorns now (permanent Thorns never ticked down: the same
/// cycle-snowball as the old Strength seals); a parry you time against the multi-hit turn.
/// </summary>
public sealed class SealOfTheMartyr() : SealCard(1, CardRarity.Rare, 3m)
{
    protected override async Task Arm(PlayerChoiceContext ctx, decimal amount)
    {
        await PowerCmd.Apply<SealOfTheMartyrThornsPower>(ctx, Owner.Creature, amount, Owner.Creature, this);
        await Seals.Grant<SealOfTheMartyrPower>(ctx, Owner, 1m, this);
    }
}
