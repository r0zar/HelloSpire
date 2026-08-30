using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HelloSpire.HelloSpireCode.Powers;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;

namespace HelloSpire.HelloSpireCode.Characters.PaladinContent;

/// <summary>
/// The scaling Seal: Attacks deal +Amount; Judged, gain 1 permanent Strength.
/// </summary>
public sealed class SealOfTheCrusaderPower : SealPower
{
    public override decimal ModifyDamageAdditive(Creature? target, decimal amount, ValueProp props,
        Creature? dealer, CardModel? cardSource)
    {
        if (Owner != dealer || !props.IsPoweredAttack()) return 0m;
        return Amount;
    }

    public override async Task OnJudged(PlayerChoiceContext ctx, Creature target) =>
        await PowerCmd.Apply<StrengthPower>(ctx, Owner, 1m, Owner, null);
}
