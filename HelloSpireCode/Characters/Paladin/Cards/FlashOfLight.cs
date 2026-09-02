using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;

namespace HelloSpire.HelloSpireCode.Characters.PaladinContent.Cards;

/// <summary>
/// Heal a player 5 + Spirit, free. Exhaust. Tithe: apply 1 Weak to ALL enemies.
/// The candle: a real heal once, a recurring defensive flicker forever.
/// </summary>
public sealed class FlashOfLight() : PaladinCard(0, CardType.Skill, CardRarity.Common, TargetType.AnyPlayer), IHealingCard
{
    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];
    protected override IEnumerable<DynamicVar> CanonicalVars => [new SpiritHealVar(5m)];
    protected override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.Static(PaladinTips.Tithe)];

    public override bool HasTithe => true;

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        var target = TargetOrOwner(cardPlay);
        await CreatureCmd.TriggerAnim(Owner.Creature, "Cast", Owner.Character.CastAnimDelay);
        await Spirit.Heal(Owner, target, DynamicVars.Heal.BaseValue);
    }

    protected override async Task OnTithe(PlayerChoiceContext ctx)
    {
        var state = Owner.Creature.CombatState;
        foreach (var enemy in state.HittableEnemies.ToList())
            await PowerCmd.Apply<WeakPower>(ctx, enemy, 1m, Owner.Creature, this);
    }

    protected override void OnUpgrade() => DynamicVars.Heal.UpgradeValueBy(3m);
}
