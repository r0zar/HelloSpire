using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HelloSpire.HelloSpireCode.Powers;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;

namespace HelloSpire.HelloSpireCode.Characters.PaladinContent;

/// <summary>Whenever the owner plays an Attack, heal the most wounded player (lowest HP fraction).</summary>
public sealed class AvengingCrusaderPower : HelloSpirePower
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;

    public override async Task AfterCardPlayed(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (cardPlay.Card.Owner?.Creature != Owner || cardPlay.Card.Type != CardType.Attack) return;
        var state = Owner.CombatState;
        if (state == null) return;
        var wounded = state.PlayerCreatures.Where(c => c.IsAlive && c.CurrentHp < c.MaxHp)
            .OrderBy(c => (double)c.CurrentHp / c.MaxHp).FirstOrDefault();
        if (wounded == null) return;
        Flash();
        await PowerCmd.Apply<RegenPower>(choiceContext, wounded, Amount, Owner, null);
    }
}
