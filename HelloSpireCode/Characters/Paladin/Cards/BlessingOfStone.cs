using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;

namespace HelloSpire.HelloSpireCode.Characters.PaladinContent.Cards;

/// <summary>A player gains 3 Plating. Prot support that travels -- solo-legal, co-op gift.</summary>
public sealed class BlessingOfStone() : PaladinCard(1, CardType.Skill, CardRarity.Common, TargetType.AnyPlayer)
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [new DynamicVar("Plating", 3m)];

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.Static(PaladinTips.Tithe)];

    public override bool HasTithe => true;

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        var target = TargetOrOwner(cardPlay);
        await CreatureCmd.TriggerAnim(Owner.Creature, "Cast", Owner.Character.CastAnimDelay);
        await PowerCmd.Apply<PlatingPower>(choiceContext, target,
            DynamicVars["Plating"].BaseValue, Owner.Creature, this);
    }

    protected override async Task OnTithe(PlayerChoiceContext ctx) =>
        await PowerCmd.Apply<PlatingPower>(ctx, Owner.Creature, 1m, Owner.Creature, this);

    protected override void OnUpgrade() => DynamicVars["Plating"].UpgradeValueBy(1m);
}
