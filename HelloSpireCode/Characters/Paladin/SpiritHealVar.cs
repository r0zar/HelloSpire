using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;

namespace HelloSpire.HelloSpireCode.Characters.PaladinContent;

/// <summary>
/// A HealVar that previews the number the heal will actually restore: base plus the owner's
/// current Spirit -- the same live-calculated display attacks get from Strength via DamageVar.
/// The highlight logic comes free: PreviewValue above base renders green, exactly like damage.
///
/// Cards that GAIN Spirit before healing (Comfort, Blessed Recovery) name their gain var, and the
/// preview reads it at display time so upgrades to the gain stay honest.
///
/// Display only: the actual heal math still lives in Spirit.Heal, the one funnel.
/// </summary>
public sealed class SpiritHealVar : HealVar
{
    /// <summary>Name of a sibling var whose value is gained as Spirit before the heal resolves.</summary>
    private readonly string? _gainedSpiritVar;

    public SpiritHealVar(decimal healAmount, string? gainedSpiritVar = null)
        : base(healAmount)
    {
        _gainedSpiritVar = gainedSpiritVar;
    }

    public override void UpdateCardPreview(CardModel card, CardPreviewMode previewMode, Creature? target, bool runGlobalHooks)
    {
        if (card.Owner == null) return; // card library / compendium: no owner, show base

        var value = BaseValue + Spirit.Of(card.Owner);
        if (_gainedSpiritVar != null) value += card.DynamicVars[_gainedSpiritVar].BaseValue;
        PreviewValue = value;
    }
}
