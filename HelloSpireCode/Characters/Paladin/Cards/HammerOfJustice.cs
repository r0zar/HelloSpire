using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.MonsterMoves.Intents;
using MegaCrit.Sts2.Core.ValueProps;

namespace HelloSpire.HelloSpireCode.Characters.PaladinContent.Cards;

/// <summary>
/// Deal damage equal to your Faith. Stun the enemy. 3-cost, upgrade brings it to 2.
///
/// The starter payoff for banked Faith: every point not spent on Mend is damage here, so the
/// heal-or-hit tension lives on one number. Faith is read at play time; nothing is spent.
/// </summary>
public sealed class HammerOfJustice() : PaladinCard(3, CardType.Attack, CardRarity.Basic, TargetType.AnyEnemy)
{
    protected override IEnumerable<IHoverTip> ExtraHoverTips => [StunIntent.GetStaticHoverTip()];
    // A zero-base DamageVar keeps the card in the normal damage pipeline (Strength, Vulnerable).
    protected override IEnumerable<DynamicVar> CanonicalVars => [new DamageVar(0m, ValueProp.Move)];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        ArgumentNullException.ThrowIfNull(cardPlay.Target);
        await DamageCmd.Attack(Faith.Of(Owner)).FromCard(this).Targeting(cardPlay.Target)
            .WithHitFx("vfx/vfx_attack_blunt")
            .Execute(choiceContext);
        await CreatureCmd.Stun(cardPlay.Target);
    }

    protected override void OnUpgrade() => EnergyCost.UpgradeBy(-1);
}
