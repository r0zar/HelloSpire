using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models.Powers;

namespace HelloSpire.HelloSpireCode.Characters.PaladinContent.Relics;

/// <summary>
/// Pool relic. Whenever you play a card that heals, gain 1 Strength. Once per turn. Now that
/// healing scales with Spirit, this pays you for doing it.
///
/// Turns the character's least-valued verb into its most-valued stat: healing is weak in Slay
/// the Spire and Strength is the strongest scaling there is. A heal-heavy deck becomes a
/// scaling deck, which makes Ilmater cards attractive to a Tyr build -- the cross-deity blending
/// the design wants -- without touching Spirit itself.
///
/// Trigger is "played a card that heals", not "HP went up": countable, credits ally heals to the
/// Paladin, and does not fire on potions, Regen or end-of-combat heals. Once per turn caps a
/// Mend + Salve turn at +1 rather than +2, which is what keeps it starter-scale rather than a
/// turn-one Demon Form. Strength resets per combat on its own.
/// </summary>
public sealed class HolyFervor : PaladinRelic
{
    public override RelicRarity Rarity => RelicRarity.Common;

    private bool _usedThisTurn;

    public override async Task AfterPlayerTurnStart(PlayerChoiceContext choiceContext, Player player)
    {
        if (player == Owner) _usedThisTurn = false;
        await Task.CompletedTask;
    }

    public override async Task AfterCardPlayed(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (_usedThisTurn || cardPlay.Card.Owner != Owner) return;
        if (!cardPlay.Card.DynamicVars.Keys.Cast<string>().Contains("Heal")) return;

        _usedThisTurn = true;
        Flash();
        await PowerCmd.Apply<StrengthPower>(choiceContext, Owner.Creature, 1m, Owner.Creature, null);
    }
}
