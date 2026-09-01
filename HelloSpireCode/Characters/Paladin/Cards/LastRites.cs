using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;

namespace HelloSpire.HelloSpireCode.Characters.PaladinContent.Cards;

/// <summary>
/// X-cost: deal (4 + Spirit + 2 per healing card Exhausted this combat) damage, X times.
/// The Holy finisher: every candle spent comes due, X times over. Counts only true casts --
/// the Exhaust pile -- never Tithe faces.
/// </summary>
public sealed class LastRites() : PaladinCard(0, CardType.Attack, CardRarity.Rare, TargetType.AnyEnemy)
{
    protected override bool HasEnergyCostX => true;
    protected override IEnumerable<DynamicVar> CanonicalVars => [new DamageVar(4m, ValueProp.Move)];

    private int CandlesSpent =>
        PileType.Exhaust.GetPile(Owner).Cards.Count(c => c is IHealingCard);

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        ArgumentNullException.ThrowIfNull(cardPlay.Target);
        var times = ResolveEnergyXValue();
        if (times <= 0) return;
        var perHit = DynamicVars.Damage.BaseValue + Spirit.Of(Owner) + 2m * CandlesSpent;
        await DamageCmd.Attack(perHit).FromCard(this).Targeting(cardPlay.Target)
            .WithHitCount(times)
            .WithHitFx("vfx/vfx_heavy_blunt")
            .Execute(choiceContext);
    }

    protected override void OnUpgrade() => DynamicVars.Damage.UpgradeValueBy(3m);
}
