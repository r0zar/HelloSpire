using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;

namespace HelloSpire.HelloSpireCode.Characters.PaladinContent.Cards;

/// <summary>
/// Grants Seal of the Crusader: Attacks deal +1; Judged, gain 1 permanent Strength. The rare
/// Seal that turns trigger density into scaling.
/// </summary>
public sealed class SealOfTheCrusader() : PaladinCard(1, CardType.Power, CardRarity.Rare, TargetType.Self)
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [new DynamicVar("Amount", 1m)];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay) =>
        await Seals.Grant<SealOfTheCrusaderPower>(choiceContext, Owner, DynamicVars["Amount"].BaseValue, this);

    protected override void OnUpgrade() => DynamicVars["Amount"].UpgradeValueBy(1m);
}
