using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;

namespace HelloSpire.HelloSpireCode.Characters.PaladinContent.Cards;

/// <summary>
/// A player gains 2 Strength and 2 Dexterity; you gain 2 Spirit. Exhaust.
/// The full coronation, once -- permanent stats compound, so the true cast is bounded.
/// Spirit lands on the caster, per the Blessing of Faith precedent. No face: not needed.
/// </summary>
public sealed class BlessingOfKings() : PaladinCard(2, CardType.Skill, CardRarity.Rare, TargetType.AnyPlayer)
{
    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];
    protected override IEnumerable<DynamicVar> CanonicalVars =>
        [new DynamicVar("Amount", 2m), new DynamicVar("Spirit", 2m)];
    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        var target = TargetOrOwner(cardPlay);
        await CreatureCmd.TriggerAnim(Owner.Creature, "Cast", Owner.Character.CastAnimDelay);
        await PowerCmd.Apply<StrengthPower>(choiceContext, target,
            DynamicVars["Amount"].BaseValue, Owner.Creature, this);
        await PowerCmd.Apply<DexterityPower>(choiceContext, target,
            DynamicVars["Amount"].BaseValue, Owner.Creature, this);
        await Spirit.Gain(choiceContext, Owner, (int)DynamicVars["Spirit"].BaseValue, this);
    }

    protected override void OnUpgrade()
    {
        DynamicVars["Amount"].UpgradeValueBy(1m);
        DynamicVars["Spirit"].UpgradeValueBy(1m);
    }
}
