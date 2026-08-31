using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using HelloSpire.HelloSpireCode.Characters.PaladinContent;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;

namespace HelloSpire.HelloSpireCode.Characters.PaladinContent.Cards;

/// <summary>
/// Grants Seal of Light: the healer lane's anchor. The passive stays small (1 + Spirit per copy);
/// the upgrade raises the JUDGE -- 2 Spirit per Judgment -- so upgrading buys ramp, not raw heal.
/// JudgeSpirit is a high-water mark on the power: one upgraded copy lifts it, extra copies stack
/// the passive as usual.
/// </summary>
public sealed class SealOfLight() : PaladinCard(1, CardType.Power, CardRarity.Uncommon, TargetType.Self)
{
    protected override IEnumerable<DynamicVar> CanonicalVars =>
        [new DynamicVar("Amount", 1m), new DynamicVar("Spirit", 1m)];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await Seals.Grant<SealOfLightPower>(choiceContext, Owner, DynamicVars["Amount"].BaseValue, this);
        if (Owner.Creature.GetPower<SealOfLightPower>() is { } seal)
            seal.JudgeSpirit = Math.Max(seal.JudgeSpirit, DynamicVars["Spirit"].IntValue);
    }

    protected override void OnUpgrade() => DynamicVars["Spirit"].UpgradeValueBy(1m);
}
