using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;

namespace HelloSpire.HelloSpireCode.Characters.PaladinContent.Cards;

/// <summary>
/// Gain 1 Plating. Discard a card. The cheapest armor tick, free -- the Prot rhythm's
/// smallest beat, and still a discard enabler.
/// </summary>
public sealed class Alms() : PaladinCard(0, CardType.Skill, CardRarity.Common, TargetType.Self)
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [new DynamicVar("Plating", 1m)];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await CreatureCmd.TriggerAnim(Owner.Creature, "Cast", Owner.Character.CastAnimDelay);
        await PowerCmd.Apply<PlatingPower>(choiceContext, Owner.Creature,
            DynamicVars["Plating"].BaseValue, Owner.Creature, this);
        await PaladinEffects.DiscardChosen(choiceContext, Owner, 1, this);
    }

    protected override void OnUpgrade() => DynamicVars["Plating"].UpgradeValueBy(1m);
}
