using System.Collections.Generic;
using System.Threading.Tasks;
using HelloSpire.HelloSpireCode.Alchemist.Lab;
using HelloSpire.HelloSpireCode.Alchemist.Potions;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;

namespace HelloSpire.HelloSpireCode.Alchemist.Relics;

/// <summary>
/// The starter: at the start of each combat, Brew a Unstable Concoction -- empty, ready for the turn's
/// cards to Infuse.
///
/// Same shape as PortableAlembic's own turn-1 Brew, just of a fixed Potion instead of a random
/// one. Unstable Concoction is Volatile, so exactly one exists per combat: it's gone by the time this
/// fires again next fight, never two at once.
/// </summary>
public sealed class AlchemicalSatchel : Characters.AlchemistRelic
{
    public override RelicRarity Rarity => RelicRarity.Starter;

    public override async Task BeforeSideTurnStart(PlayerChoiceContext choiceContext, CombatSide side,
        IReadOnlyList<Creature> participants, ICombatState combatState)
    {
        if (!participants.Contains(Owner.Creature) || Owner.PlayerCombatState.TurnNumber > 1) return;
        Flash();
        await Belt.Brew(choiceContext, LabContext.From(Owner), ModelDb.Potion<UnstableConcoction>().ToMutable());
    }
}
