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
/// Mark a player as the Beacon: whenever you heal a player, the Beacon also heals 3. The co-op
/// build-around; rides the Spirit.Heal funnel every Paladin heal already goes through.
/// </summary>
public sealed class BeaconOfLight() : PaladinCard(1, CardType.Power, CardRarity.Rare, TargetType.AnyPlayer)
{
    public override CardMultiplayerConstraint MultiplayerConstraint => CardMultiplayerConstraint.MultiplayerOnly;
    protected override IEnumerable<DynamicVar> CanonicalVars => [new DynamicVar("Heal", 3m)];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        ArgumentNullException.ThrowIfNull(cardPlay.Target);
        await CreatureCmd.TriggerAnim(Owner.Creature, "PowerUp", Owner.Character.PowerUpAnimDelay);
        await PowerCmd.Apply<BeaconOfLightPower>(choiceContext, cardPlay.Target,
            DynamicVars["Heal"].BaseValue, Owner.Creature, this);
    }

    protected override void OnUpgrade() => DynamicVars["Heal"].UpgradeValueBy(1m);
}
