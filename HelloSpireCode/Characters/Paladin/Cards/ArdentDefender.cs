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
/// Whenever an enemy attack hits you, gain 2 Plating (3 upgraded). Hardens under fire:
/// multi-hit enemies feed it, and every stack feeds Shield Bash. Nerfed from 3/5 per hit --
/// the flurry snowball outgrew the rare-power band.
/// </summary>
public sealed class ArdentDefender() : PaladinCard(2, CardType.Power, CardRarity.Rare, TargetType.Self)
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [new DynamicVar("Plating", 2m)];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await CreatureCmd.TriggerAnim(Owner.Creature, "PowerUp", Owner.Character.PowerUpAnimDelay);
        await PowerCmd.Apply<ArdentDefenderPower>(choiceContext, Owner.Creature,
            DynamicVars["Plating"].BaseValue, Owner.Creature, this);
    }

    protected override void OnUpgrade() => DynamicVars["Plating"].UpgradeValueBy(1m);
}
