using System.Collections.Generic;
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
/// Gain 1 Buffer. Exhaust, Retain. Tithe: gain 8 Block.
/// The bubble, retuned: 1E for one true negate (was 2E for 2), and the face went from a
/// token 3 Block to a real emergency parry -- hold it for the killing blow, or pitch it
/// when the wall needs to be NOW. Upgrade: costs 0. The set's only Buffer spell.
/// </summary>
public sealed class DivineShield() : PaladinCard(1, CardType.Skill, CardRarity.Rare, TargetType.Self)
{
    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust, CardKeyword.Retain];
    protected override IEnumerable<DynamicVar> CanonicalVars => [new DynamicVar("Buffer", 1m)];
    protected override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.Static(PaladinTips.Tithe)];

    public override bool HasTithe => true;

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await CreatureCmd.TriggerAnim(Owner.Creature, "PowerUp", Owner.Character.PowerUpAnimDelay);
        await PowerCmd.Apply<BufferPower>(choiceContext, Owner.Creature,
            DynamicVars["Buffer"].BaseValue, Owner.Creature, this);
    }

    protected override async Task OnTithe(PlayerChoiceContext ctx) =>
        await CreatureCmd.GainBlock(Owner.Creature, 8m, ValueProp.Unpowered, null);

    protected override void OnUpgrade() => EnergyCost.UpgradeBy(-1);
}
