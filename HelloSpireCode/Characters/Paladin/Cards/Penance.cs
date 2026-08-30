using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;

namespace HelloSpire.HelloSpireCode.Characters.PaladinContent.Cards;

/// <summary>
/// Lose 4 HP, gain 2 Energy. The Bloodletting slot, as self-mortification: HP is fuel, and this
/// is the one class that can heal the toll back. Upgrade: 3 Energy.
/// </summary>
public sealed class Penance() : PaladinCard(0, CardType.Skill, CardRarity.Common, TargetType.Self)
{
    protected override IEnumerable<DynamicVar> CanonicalVars =>
        [new HpLossVar(4m), new EnergyVar(2)];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await CreatureCmd.TriggerAnim(Owner.Creature, "Cast", Owner.Character.CastAnimDelay);
        await CreatureCmd.Damage(choiceContext, Owner.Creature, DynamicVars.HpLoss.BaseValue,
            ValueProp.Unblockable | ValueProp.Unpowered | ValueProp.Move, this);
        await PlayerCmd.GainEnergy(DynamicVars["Energy"].BaseValue, Owner);
    }

    protected override void OnUpgrade() => DynamicVars["Energy"].UpgradeValueBy(1m);
}
