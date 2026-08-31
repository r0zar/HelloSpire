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
/// Aura: whenever you Exhaust a card, heal the most wounded player. Every Paladin heal Exhausts,
/// so the healer deck chains -- and it cannot run continuously: each trigger costs a real card.
/// Back to a Power card, restoring the auras-are-MP-Powers rule the Regen rework bent.
/// </summary>
public sealed class AuraOfVitality() : PaladinCard(2, CardType.Power, CardRarity.Uncommon, TargetType.Self)
{
    public override CardMultiplayerConstraint MultiplayerConstraint => CardMultiplayerConstraint.MultiplayerOnly;
    protected override IEnumerable<DynamicVar> CanonicalVars => [new HealVar(2m)];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await CreatureCmd.TriggerAnim(Owner.Creature, "PowerUp", Owner.Character.PowerUpAnimDelay);
        await PowerCmd.Apply<AuraOfVitalityPower>(choiceContext, Owner.Creature,
            DynamicVars.Heal.BaseValue, Owner.Creature, this);
    }

    protected override void OnUpgrade() => DynamicVars.Heal.UpgradeValueBy(1m);
}
