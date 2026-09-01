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
using MegaCrit.Sts2.Core.ValueProps;

namespace HelloSpire.HelloSpireCode.Characters.PaladinContent.Cards;

/// <summary>
/// Deal 12. Apply 2 Weak. Tithe: ALL enemies lose 1 Strength until end of turn. Retuned to
/// the 2E-common band (Flatten 12, Predator 15, Cinder 18); the face is a recurring flicker
/// of the class's Strength-down signature.
/// </summary>
public sealed class Chastise() : PaladinCard(2, CardType.Attack, CardRarity.Common, TargetType.AnyEnemy)
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [new DamageVar(12m, ValueProp.Move)];
    protected override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.Static(PaladinTips.Tithe)];

    public override bool HasTithe => true;

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        ArgumentNullException.ThrowIfNull(cardPlay.Target);
        await DamageCmd.Attack(DynamicVars.Damage.BaseValue).FromCard(this).Targeting(cardPlay.Target)
            .WithHitFx("vfx/vfx_attack_blunt")
            .Execute(choiceContext);
        await PowerCmd.Apply<WeakPower>(choiceContext, cardPlay.Target, 2m, Owner.Creature, this);
    }

    protected override async Task OnTithe(PlayerChoiceContext ctx)
    {
        if (Owner.Creature.CombatState is not { } state) return;
        foreach (var enemy in state.HittableEnemies.ToList())
            await PowerCmd.Apply<HumblingShacklesPower>(ctx, enemy, 1m, Owner.Creature, null);
    }

    protected override void OnUpgrade() => DynamicVars.Damage.UpgradeValueBy(4m);
}
