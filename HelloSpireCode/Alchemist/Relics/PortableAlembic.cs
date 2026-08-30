using System.Collections.Generic;
using System.Threading.Tasks;
using HelloSpire.HelloSpireCode.Alchemist.Lab;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Merchant;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Potions;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Rooms;

namespace HelloSpire.HelloSpireCode.Alchemist.Relics;

/// <summary>
/// The starter: at the start of each combat, Brew a random Common Combat Potion. Turn one always
/// has something on the belt -- the Cracked Core shape, in glassware.
/// </summary>
public sealed class PortableAlembic : Characters.AlchemistRelic
{
    public override RelicRarity Rarity => RelicRarity.Starter;

    public override async Task BeforeSideTurnStart(PlayerChoiceContext choiceContext, CombatSide side,
        IReadOnlyList<Creature> participants, ICombatState combatState)
    {
        if (!participants.Contains(Owner.Creature) || Owner.PlayerCombatState.TurnNumber > 1) return;
        Flash();
        await Belt.BrewRandom(choiceContext, LabContext.From(Owner));
    }
}
