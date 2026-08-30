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
/// Gain 2 Strength and 2 Spirit; shuffle a Geas into your draw pile. Power now, weight later.
/// </summary>
public sealed class SacredOath() : PaladinCard(1, CardType.Skill, CardRarity.Rare, TargetType.Self)
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [new DynamicVar("Amount", 2m)];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await CreatureCmd.TriggerAnim(Owner.Creature, "PowerUp", Owner.Character.PowerUpAnimDelay);
        await PowerCmd.Apply<StrengthPower>(choiceContext, Owner.Creature,
            DynamicVars["Amount"].BaseValue, Owner.Creature, this);
        await Spirit.Gain(choiceContext, Owner, (int)DynamicVars["Amount"].BaseValue, this);
        await CardPileCmd.AddGeneratedCardToCombat(
            CombatState.CreateCard<Geas>(Owner), PileType.Draw, Owner);
    }

    protected override void OnUpgrade() => DynamicVars["Amount"].UpgradeValueBy(1m);
}
