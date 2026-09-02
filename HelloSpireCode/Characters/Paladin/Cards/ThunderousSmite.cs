using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;

namespace HelloSpire.HelloSpireCode.Characters.PaladinContent.Cards;

/// <summary>Deal 12 to ALL enemies. Judge each enemy. Tribunal grown up -- the mass-judge finisher.</summary>
public sealed class ThunderousSmite() : PaladinCard(2, CardType.Attack, CardRarity.Rare, TargetType.AllEnemies)
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [new DamageVar(12m, ValueProp.Move)];
    protected override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.Static(PaladinTips.Judge)];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await DamageCmd.Attack(DynamicVars.Damage.BaseValue).FromCard(this).WithAttackerAnim(HeavyAttackAnim, HeavyAttackDelay)
            .TargetingAllOpponents(CombatState)
            .WithHitFx("vfx/vfx_heavy_blunt")
            .SpawningHitVfxOnEachCreature()
            .Execute(choiceContext);
        await Seals.JudgeEach(choiceContext, Owner, CombatState.HittableEnemies.ToList());
    }

    protected override void OnUpgrade() => DynamicVars.Damage.UpgradeValueBy(4m);
}
