using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;

namespace HelloSpire.HelloSpireCode.Characters.PaladinContent.Cards;

/// <summary>
/// Heal a player 6 + Spirit. Exhaust. Tithe: deal 6 + Spirit to a random enemy.
/// The flex card -- mend or smite, Spirit sizes both.
/// </summary>
public sealed class HolyShock() : PaladinCard(1, CardType.Skill, CardRarity.Common, TargetType.AnyPlayer), IHealingCard
{
    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];
    protected override IEnumerable<DynamicVar> CanonicalVars => [new SpiritHealVar(6m)];
    protected override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.Static(PaladinTips.Tithe)];

    public override bool HasTithe => true;

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        ArgumentNullException.ThrowIfNull(cardPlay.Target);
        await CreatureCmd.TriggerAnim(Owner.Creature, "Cast", Owner.Character.CastAnimDelay);
        await Spirit.Heal(Owner, cardPlay.Target, DynamicVars.Heal.BaseValue);
    }

    protected override async Task OnTithe(PlayerChoiceContext ctx)
    {
        var enemy = PaladinEffects.RandomEnemy(Owner);
        if (enemy == null) return;
        await CreatureCmd.Damage(ctx, [enemy], DynamicVars.Heal.BaseValue + Spirit.Of(Owner),
            ValueProp.Unpowered, Owner.Creature);
    }

    protected override void OnUpgrade() => DynamicVars.Heal.UpgradeValueBy(3m);
}
