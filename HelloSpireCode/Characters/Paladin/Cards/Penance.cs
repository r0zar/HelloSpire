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
/// Lose 4 HP, gain 2 Spirit. Exhaust. Blood for faith -- the aggressive Spirit ramp, and the
/// one class that can heal the toll back. Spirit-gain Exhausts, per the rule.
/// </summary>
public sealed class Penance() : PaladinCard(0, CardType.Skill, CardRarity.Uncommon, TargetType.Self)
{
    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];
    protected override IEnumerable<DynamicVar> CanonicalVars =>
        [new HpLossVar(4m), new DynamicVar("Spirit", 2m)];

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.Static(PaladinTips.Tithe)];

    public override bool HasTithe => true;

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await CreatureCmd.TriggerAnim(Owner.Creature, "Cast", Owner.Character.CastAnimDelay);
        await CreatureCmd.Damage(choiceContext, Owner.Creature, DynamicVars.HpLoss.BaseValue,
            ValueProp.Unblockable | ValueProp.Unpowered | ValueProp.Move, this);
        await Spirit.Gain(choiceContext, Owner, (int)DynamicVars["Spirit"].BaseValue, this);
    }

    protected override async Task OnTithe(PlayerChoiceContext ctx) =>
        await CreatureCmd.GainBlock(Owner.Creature, 2m, ValueProp.Unpowered, null);

    protected override void OnUpgrade() => DynamicVars["Spirit"].UpgradeValueBy(1m);
}
