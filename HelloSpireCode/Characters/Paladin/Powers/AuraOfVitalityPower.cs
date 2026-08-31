using System.Linq;
using System.Threading.Tasks;
using HelloSpire.HelloSpireCode.Powers;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;

namespace HelloSpire.HelloSpireCode.Characters.PaladinContent;

/// <summary>
/// Aura: whenever the owner Exhausts a card, heal the most wounded player (lowest HP fraction)
/// Amount plus the owner's Spirit -- through the funnel, so Beacon of Light rides along.
/// Bounded by construction: every trigger consumed a card.
/// </summary>
public sealed class AuraOfVitalityPower : HelloSpirePower
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;

    public override async Task AfterCardExhausted(PlayerChoiceContext choiceContext, CardModel card, bool causedByEthereal)
    {
        if (card.Owner?.Creature != Owner || Owner.Player is not { } player) return;
        var state = Owner.CombatState;
        if (state == null) return;
        var wounded = state.PlayerCreatures.Where(c => c.IsAlive && c.CurrentHp < c.MaxHp)
            .OrderBy(c => (double)c.CurrentHp / c.MaxHp).FirstOrDefault();
        if (wounded == null) return;
        Flash();
        await Spirit.Heal(player, wounded, Amount);
    }
}
