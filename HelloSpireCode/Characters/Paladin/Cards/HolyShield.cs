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
/// Whenever a card gains you Block, gain 3 more. Tithe: apply Holy Shield. The Holy blocking
/// engine -- the lane leans on Block cards and this makes every one bigger. The face is Sly
/// in all but name: discarding it still installs the engine, worded as the class's own tithe.
/// </summary>
public sealed class HolyShield() : PaladinCard(1, CardType.Power, CardRarity.Uncommon, TargetType.Self)
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [new DynamicVar("Block", 3m)];
    protected override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.Static(PaladinTips.Tithe)];

    public override bool HasTithe => true;

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await CreatureCmd.TriggerAnim(Owner.Creature, "PowerUp", Owner.Character.PowerUpAnimDelay);
        await PowerCmd.Apply<HolyShieldPower>(choiceContext, Owner.Creature,
            DynamicVars["Block"].BaseValue, Owner.Creature, this);
    }

    protected override async Task OnTithe(PlayerChoiceContext ctx) =>
        await PowerCmd.Apply<HolyShieldPower>(ctx, Owner.Creature,
            DynamicVars["Block"].BaseValue, Owner.Creature, this);

    protected override void OnUpgrade() => DynamicVars["Block"].UpgradeValueBy(1m);
}
