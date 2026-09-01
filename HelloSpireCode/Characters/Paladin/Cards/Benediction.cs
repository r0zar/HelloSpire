using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;

namespace HelloSpire.HelloSpireCode.Characters.PaladinContent.Cards;

/// <summary>
/// Gain 2 Plating. Draw a card, then discard a card. The starter teacher: armor-always plus
/// the draw-discard rhythm every lane speaks. Upgrade: 3 Plating.
/// </summary>
public sealed class Benediction() : PaladinCard(1, CardType.Skill, CardRarity.Basic, TargetType.Self)
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [new DynamicVar("Plating", 2m)];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await CreatureCmd.TriggerAnim(Owner.Creature, "Cast", Owner.Character.CastAnimDelay);
        await PowerCmd.Apply<PlatingPower>(choiceContext, Owner.Creature,
            DynamicVars["Plating"].BaseValue, Owner.Creature, this);
        await CardPileCmd.Draw(choiceContext, 1, Owner);
        await PaladinEffects.DiscardChosen(choiceContext, Owner, 1, this);
    }

    protected override void OnUpgrade() => DynamicVars["Plating"].UpgradeValueBy(1m);
}
