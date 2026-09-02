using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.ValueProps;

namespace HelloSpire.HelloSpireCode.Characters.PaladinContent.Cards;

/// <summary>
/// Your Block cards gain extra Block equal to your Spirit -- Dexterity that prays.
/// Tithe: gain 4 Block, a bounded flicker (plain Block this turn; nothing survives the
/// discard pile, unlike the rejected apply-itself face). Upgrade: costs 1.
/// </summary>
public sealed class HolyShield() : PaladinCard(2, CardType.Power, CardRarity.Uncommon, TargetType.Self)
{
    protected override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.Static(PaladinTips.Tithe)];

    public override bool HasTithe => true;

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await CreatureCmd.TriggerAnim(Owner.Creature, "PowerUp", Owner.Character.PowerUpAnimDelay);
        await PowerCmd.Apply<HolyShieldPower>(choiceContext, Owner.Creature, 1m, Owner.Creature, this);
    }

    protected override async Task OnTithe(PlayerChoiceContext ctx) =>
        await CreatureCmd.GainBlock(Owner.Creature, 4m, ValueProp.Unpowered, null);

    protected override void OnUpgrade() => EnergyCost.UpgradeBy(-1);
}
