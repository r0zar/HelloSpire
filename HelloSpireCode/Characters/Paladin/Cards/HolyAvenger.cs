using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;

namespace HelloSpire.HelloSpireCode.Characters.PaladinContent.Cards;

/// <summary>Double your Spirit. Exhaust. Tithe: draw a card. The Spirit crescendo.</summary>
public sealed class HolyAvenger() : PaladinCard(0, CardType.Skill, CardRarity.Rare, TargetType.Self)
{
    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];
    protected override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.Static(PaladinTips.Tithe)];

    public override bool HasTithe => true;

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await CreatureCmd.TriggerAnim(Owner.Creature, "PowerUp", Owner.Character.PowerUpAnimDelay);
        var current = Spirit.Of(Owner);
        if (current > 0) await Spirit.Gain(choiceContext, Owner, current, this);
    }

    protected override async Task OnTithe(PlayerChoiceContext ctx) =>
        await CardPileCmd.Draw(ctx, 1, Owner);

    protected override void OnUpgrade() => EnergyCost.UpgradeBy(0);
}
