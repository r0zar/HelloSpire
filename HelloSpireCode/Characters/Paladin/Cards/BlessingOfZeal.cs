using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;

namespace HelloSpire.HelloSpireCode.Characters.PaladinContent.Cards;

/// <summary>
/// Next turn: gain 2 Energy and 1 Strength. The wind-up blessing -- invests this turn into a
/// bigger one. Might as zeal, at parity with Invoke.
/// </summary>
public sealed class BlessingOfZeal() : PaladinCard(1, CardType.Skill, CardRarity.Uncommon, TargetType.Self)
{
    protected override IEnumerable<DynamicVar> CanonicalVars =>
        [new DynamicVar("Energy", 2m), new DynamicVar("Strength", 1m)];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await CreatureCmd.TriggerAnim(Owner.Creature, "Cast", Owner.Character.CastAnimDelay);
        await PowerCmd.Apply<EnergyNextTurnPower>(choiceContext, Owner.Creature,
            DynamicVars["Energy"].BaseValue, Owner.Creature, this);
        await PowerCmd.Apply<ZealNextTurnPower>(choiceContext, Owner.Creature,
            DynamicVars["Strength"].BaseValue, Owner.Creature, this);
    }

    protected override void OnUpgrade() => DynamicVars["Strength"].UpgradeValueBy(1m);
}
