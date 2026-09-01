using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;

namespace HelloSpire.HelloSpireCode.Characters.PaladinContent.Cards;

/// <summary>
/// Gain 1 Spirit. Discard a card. Free -- the Holy tick: give a little, and the light grows.
/// (Was a Plating tick, too close to Prayer after its rework. Recurring Spirit at 0E is a
/// deliberate law amendment, Blessing of Might precedent: one per deck cycle for a card slot.)
/// </summary>
public sealed class Alms() : PaladinCard(0, CardType.Skill, CardRarity.Common, TargetType.Self)
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [new DynamicVar("Spirit", 1m)];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await CreatureCmd.TriggerAnim(Owner.Creature, "Cast", Owner.Character.CastAnimDelay);
        await Spirit.Gain(choiceContext, Owner, (int)DynamicVars["Spirit"].BaseValue, this);
        await PaladinEffects.DiscardChosen(choiceContext, Owner, 1, this);
    }

    protected override void OnUpgrade() => DynamicVars["Spirit"].UpgradeValueBy(1m);
}
