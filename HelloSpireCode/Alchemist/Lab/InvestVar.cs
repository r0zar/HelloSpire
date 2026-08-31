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

    /// <summary>
    /// True when an effect has cut this card's Energy cost to 0 for this specific play -- not when
    /// the card is simply printed at 0, like Gold Standard. Compares the resting cost
    /// (CostModifiers.None: upgrades count, temporary reductions don't) against what would actually
    /// be spent right now; the two only differ while a temporary effect is doing the work.
    /// </summary>
    public static bool IsForcedFree(CardModel card) =>
        card.EnergyCost.GetWithModifiers(CostModifiers.None) > 0 && card.EnergyCost.GetAmountToSpend() == 0;

    public override void UpdateCardPreview(CardModel card, CardPreviewMode previewMode, Creature? target, bool runGlobalHooks)
    {
        if (IsForcedFree(card)) PreviewValue = 0;
    }
}
