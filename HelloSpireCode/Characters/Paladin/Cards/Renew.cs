using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;

namespace HelloSpire.HelloSpireCode.Characters.PaladinContent.Cards;

/// <summary>
/// Heal a player 3 + Spirit now, and the same again at the start of their next turn.
/// Exhaust. Tithe: gain 1 Energy. Was flat Regen (the set's one heal that ignored Spirit);
/// now the simple two-beat Spirit heal -- the echo snapshots the cast amount.
/// </summary>
public sealed class Renew() : PaladinCard(1, CardType.Skill, CardRarity.Uncommon, TargetType.AnyPlayer), IHealingCard
{
    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];
    protected override IEnumerable<DynamicVar> CanonicalVars => [new SpiritHealVar(3m)];
    protected override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.Static(PaladinTips.Tithe)];

    public override bool HasTithe => true;

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        var target = TargetOrOwner(cardPlay);
        await CreatureCmd.TriggerAnim(Owner.Creature, "Cast", Owner.Character.CastAnimDelay);
        await Spirit.Heal(Owner, target, DynamicVars.Heal.BaseValue);
        var echo = Math.Max(0m, DynamicVars.Heal.BaseValue + Spirit.Of(Owner));
        if (echo > 0)
            await PowerCmd.Apply<RenewPower>(choiceContext, target, echo, Owner.Creature, this);
    }

    protected override async Task OnTithe(PlayerChoiceContext ctx) =>
        await PlayerCmd.GainEnergy(1m, Owner);

    protected override void OnUpgrade() => DynamicVars.Heal.UpgradeValueBy(2m);
}
