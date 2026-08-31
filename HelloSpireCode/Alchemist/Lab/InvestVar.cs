using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;

namespace HelloSpire.HelloSpireCode.Alchemist.Lab;

/// <summary>
/// An Invest clause's Gold cost. Previews as 0 whenever something has reduced this card's own
/// Energy cost to 0 for this play -- Ledger.Invest charges the same zero, so the printed cost and
/// the real one never disagree. Same UpdateCardPreview pattern PowerVar uses to preview a Power
/// amount instead of a Gold one.
/// </summary>
public sealed class InvestVar : DynamicVar
{
    public InvestVar(decimal cost) : base("Invest", cost) { }

    public override void UpdateCardPreview(CardModel card, CardPreviewMode previewMode, Creature? target, bool runGlobalHooks)
    {
        if (card.EnergyCost.GetAmountToSpend() == 0) PreviewValue = 0;
    }
}
