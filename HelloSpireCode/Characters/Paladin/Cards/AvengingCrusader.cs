using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;

namespace HelloSpire.HelloSpireCode.Characters.PaladinContent.Cards;

/// <summary>
/// Whenever you play an Attack, gain 1 Spirit. Whenever you Judge an enemy, deal damage to it
/// equal to your Spirit. The crusade charges; the judgment releases. Solo-legal.
/// </summary>
public sealed class AvengingCrusader() : PaladinCard(2, CardType.Power, CardRarity.Rare, TargetType.Self)
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [new DynamicVar("Spirit", 1m)];
    protected override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.Static(PaladinTips.Judge)];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await CreatureCmd.TriggerAnim(Owner.Creature, "PowerUp", Owner.Character.PowerUpAnimDelay);
        await PowerCmd.Apply<AvengingCrusaderPower>(choiceContext, Owner.Creature,
            DynamicVars["Spirit"].BaseValue, Owner.Creature, this);
    }

    protected override void OnUpgrade() => DynamicVars["Spirit"].UpgradeValueBy(1m);
}
