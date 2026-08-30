using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;

namespace HelloSpire.HelloSpireCode.Characters.PaladinContent.Cards;

/// <summary>Gain 1 Spirit at the start of each turn. The Spirit engine; Mend grows every turn.</summary>
public sealed class Devotion() : PaladinCard(1, CardType.Power, CardRarity.Common, TargetType.Self)
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [new DynamicVar("Spirit", 1m)];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay) =>
        await PowerCmd.Apply<DevotionPower>(choiceContext, Owner.Creature,
            DynamicVars["Spirit"].BaseValue, Owner.Creature, this);

    protected override void OnUpgrade() => DynamicVars["Spirit"].UpgradeValueBy(1m);
}
