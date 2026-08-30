using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;

namespace HelloSpire.HelloSpireCode.Characters.PaladinContent.Cards;

/// <summary>
/// Costs 1 Faith. Heal 6. The spender: with the Holy Symbol granting 3 Faith a combat, this is
/// three heals a fight unless the deck earns more -- which is the entire anti-stall economy.
/// Unplayable at zero Faith.
/// </summary>
public sealed class Mend() : PaladinCard(1, CardType.Skill, CardRarity.Basic, TargetType.Self)
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [new HealVar(6m)];

    // Outside combat the card reads as playable, like the base class.
    protected override bool IsPlayable => Owner?.PlayerCombatState == null || Faith.Has(Owner, 1);

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        Faith.Spend(Owner, 1);
        await CreatureCmd.TriggerAnim(Owner.Creature, "Cast", Owner.Character.CastAnimDelay);
        await CreatureCmd.Heal(Owner.Creature, DynamicVars.Heal.BaseValue);
    }

    protected override void OnUpgrade() => DynamicVars.Heal.UpgradeValueBy(3m);
}
