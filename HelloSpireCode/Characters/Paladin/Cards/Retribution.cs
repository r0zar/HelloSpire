using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;

namespace HelloSpire.HelloSpireCode.Characters.PaladinContent.Cards;

/// <summary>
/// Lose 2 Spirit. At the start of your turn, gain 1 Energy. The zeal engine: trade the
/// light for fury. Upgrade: lose only 1 Spirit. (Spirit can't go below 0 -- the loss is
/// capped at what you have.)
/// </summary>
public sealed class Retribution() : PaladinCard(1, CardType.Power, CardRarity.Uncommon, TargetType.Self)
{
    protected override IEnumerable<DynamicVar> CanonicalVars =>
        [new DynamicVar("Spirit", 2m), new DynamicVar("Energy", 1m)];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await CreatureCmd.TriggerAnim(Owner.Creature, "PowerUp", Owner.Character.PowerUpAnimDelay);
        var loss = Math.Min(Spirit.Of(Owner), (int)DynamicVars["Spirit"].BaseValue);
        if (loss > 0)
            await PowerCmd.Apply<SpiritPower>(choiceContext, Owner.Creature, -loss, Owner.Creature, this);
        await PowerCmd.Apply<RetributionPower>(choiceContext, Owner.Creature,
            DynamicVars["Energy"].BaseValue, Owner.Creature, this);
    }

    protected override void OnUpgrade() => DynamicVars["Spirit"].UpgradeValueBy(-1m);
}
