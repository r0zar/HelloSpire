using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization;

namespace HelloSpire.HelloSpireCode.Characters.PaladinContent.Cards;

/// <summary>
/// Put a card from your discard pile into your hand. The fisher closes the Tithe loop:
/// pitch a card for its face, then consult it back for the true cast -- or retrieve the
/// seal you spent. Any card, no restriction. Upgrade: costs 0.
/// </summary>
public sealed class ConsultTheScriptures() : PaladinCard(1, CardType.Skill, CardRarity.Uncommon, TargetType.Self)
{
    protected override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.Static(PaladinTips.Tithe)];

    public override bool HasTithe => true;

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await CreatureCmd.TriggerAnim(Owner.Creature, "Cast", Owner.Character.CastAnimDelay);
        var discard = PileType.Discard.GetPile(Owner);
        if (discard.Cards.Count == 0) return;
        var chosen = (await CardSelectCmd.FromCombatPile(choiceContext, discard, Owner,
            new CardSelectorPrefs(new LocString("card_selection", "HELLOSPIRE-TO_FETCH"), 1))).FirstOrDefault();
        if (chosen == null) return;
        await CardPileCmd.Add(chosen, PileType.Hand.GetPile(Owner));
    }

    protected override async Task OnTithe(PlayerChoiceContext ctx) =>
        await CardPileCmd.Draw(ctx, 1, Owner);

    protected override void OnUpgrade() => EnergyCost.UpgradeBy(-1);
}
