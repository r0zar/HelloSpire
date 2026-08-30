using System;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;

namespace HelloSpire.HelloSpireCode.Characters.PaladinContent.Cards;

/// <summary>
/// Trigger your Seal's effect. Unplayable without a Seal: the card is nothing but the trigger,
/// so what Judgment does is entirely defined by which Seal is up. Upgrade: costs 0.
/// </summary>
public sealed class Judgment() : PaladinCard(1, CardType.Attack, CardRarity.Basic, TargetType.AnyEnemy)
{
    // Outside combat the card reads as playable, like the base class.
    protected override bool IsPlayable =>
        Owner?.PlayerCombatState == null || Seals.Active(Owner.Creature) != null;

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        ArgumentNullException.ThrowIfNull(cardPlay.Target);
        await CreatureCmd.TriggerAnim(Owner.Creature, "Attack", Owner.Character.AttackAnimDelay);
        await Seals.Judge(choiceContext, Owner, cardPlay.Target);
    }

    protected override void OnUpgrade() => EnergyCost.UpgradeBy(-1);
}
