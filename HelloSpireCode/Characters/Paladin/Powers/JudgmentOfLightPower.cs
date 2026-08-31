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

/// <summary>
/// Sits on an ENEMY: whenever a player attacks it, the attacker heals Amount, flat.
///
/// Displayed as "Mark of Light": Judgment is the seal-trigger keyword and this card never judges,
/// so the WoW name was actively misleading here. The class keeps its old name because the model
/// id derives from it -- renaming the class would orphan the card in existing run decks.
/// </summary>
public sealed class JudgmentOfLightPower : HelloSpirePower
{
    public override PowerType Type => PowerType.Debuff;
    public override PowerStackType StackType => PowerStackType.Counter;

    public override async Task AfterDamageReceived(PlayerChoiceContext choiceContext, Creature target,
        DamageResult result, ValueProp props, Creature? dealer, CardModel? cardSource)
    {
        if (target != Owner || dealer == null || !dealer.IsPlayer || dealer.IsDead || !props.IsPoweredAttack()) return;
        Flash();
        // Flat by design: the healer would be whoever attacked -- possibly a teammate with a
        // different Spirit -- so no number on the card could honestly preview a scaled heal.
        await CreatureCmd.Heal(dealer, Amount);
    }
}
