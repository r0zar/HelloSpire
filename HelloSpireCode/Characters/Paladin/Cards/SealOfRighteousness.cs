using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;

namespace HelloSpire.HelloSpireCode.Characters.PaladinContent.Cards;

/// <summary>
/// Grants Seal of Righteousness: the damage-lane tempter, now drafted rather than given.
/// Amount is the attack bonus (+2, +3 upgraded); the judge is a flat 10.
/// </summary>
public sealed class SealOfRighteousness() : PaladinCard(1, CardType.Skill, CardRarity.Uncommon, TargetType.Self)
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [new DynamicVar("Amount", 2m)];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay) =>
        await Seals.Grant<SealOfRighteousnessPower>(choiceContext, Owner, DynamicVars["Amount"].BaseValue, this);

    protected override void OnUpgrade() => DynamicVars["Amount"].UpgradeValueBy(1m);
}
