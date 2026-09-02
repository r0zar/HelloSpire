using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;

namespace HelloSpire.HelloSpireCode.Characters.PaladinContent.Cards;

/// <summary>
/// Base for the nine seal cards: a Skill that arms a held stance. One seal at a time --
/// Seals.Grant replaces whatever is held. The card's "Amount" var sizes the seal's passive;
/// upgrades raise it by 1 unless a card overrides.
/// </summary>
public abstract class SealCard(int cost, CardRarity rarity, decimal amount) :
    PaladinCard(cost, CardType.Skill, rarity, TargetType.Self)
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [new DynamicVar("Amount", amount)];

    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
        [HoverTipFactory.Static(PaladinTips.Seal), HoverTipFactory.Static(PaladinTips.Judge)];

    /// <summary>Arm this card's seal power at the given amount.</summary>
    protected abstract Task Arm(PlayerChoiceContext ctx, decimal amount);

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await CreatureCmd.TriggerAnim(Owner.Creature, "PowerUp", Owner.Character.PowerUpAnimDelay);
        await Arm(choiceContext, DynamicVars["Amount"].BaseValue);
    }

    protected override void OnUpgrade() => DynamicVars["Amount"].UpgradeValueBy(1m);
}
