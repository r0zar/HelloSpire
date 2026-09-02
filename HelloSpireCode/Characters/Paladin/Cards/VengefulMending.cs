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
/// Deal 4. Heal the most wounded player 2 + Spirit. Exhaust. Tithe: deal 3 to a random enemy.
/// The Holy attack: smite and mend in one motion; deterministic heal target per the one-target rule.
/// </summary>
public sealed class VengefulMending() : PaladinCard(0, CardType.Attack, CardRarity.Common, TargetType.AnyEnemy), IHealingCard
{
    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];
    protected override IEnumerable<DynamicVar> CanonicalVars =>
        [new DamageVar(4m, ValueProp.Move), new SpiritHealVar(2m)];
    protected override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.Static(PaladinTips.Tithe)];

    public override bool HasTithe => true;

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        ArgumentNullException.ThrowIfNull(cardPlay.Target);
        await DamageCmd.Attack(DynamicVars.Damage.BaseValue).FromCard(this).Targeting(cardPlay.Target)
            .WithHitFx("vfx/vfx_attack_slash")
            .Execute(choiceContext);
        await Spirit.Heal(Owner, PaladinEffects.MostWounded(Owner.Creature), DynamicVars.Heal.BaseValue);
    }

    protected override async Task OnTithe(PlayerChoiceContext ctx)
    {
        var enemy = PaladinEffects.RandomEnemy(Owner);
        if (enemy == null) return;
        await CreatureCmd.Damage(ctx, [enemy], 3m, ValueProp.Unpowered, Owner.Creature);
    }

    protected override void OnUpgrade() => DynamicVars.Damage.UpgradeValueBy(3m);
}
