using BaseLib.Abstracts;
using BaseLib.Extensions;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models.CardPools;

namespace HelloSpire.HelloSpireCode.Alchemist.Cards;

/// <summary>
/// A free Colorless card born from an unstable Brew. Gain 1 Energy, no strings attached -- Pooled
/// as Colorless rather than as an Alchemist card, since gaining Energy has nothing to do with the
/// Belt. Exhausts on play, and Ethereal besides, so one left sitting in Hand doesn't linger past
/// the turn it arrived either.
/// </summary>
[Pool(typeof(ColorlessCardPool))]
public sealed class VolatileReagent() : AlchemistCard(0, CardType.Skill, CardRarity.Common, TargetType.Self)
{
    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust, CardKeyword.Ethereal];

    protected override async Task OnPlay(PlayerChoiceContext ctx, CardPlay play) =>
        await AlchemistEffects.GainEnergy(Lab, 1m);

    /// <summary>
    /// Upgrade: drop Ethereal, so a Reagent that arrives on a turn you cannot spend it survives
    /// to the turn you can. The Energy stays at 1 -- a card that Brews itself into your hand for
    /// free should not also scale, and Exhaust stays for the same reason.
    /// </summary>
    protected override void OnUpgrade() => RemoveKeyword(CardKeyword.Ethereal);
}
