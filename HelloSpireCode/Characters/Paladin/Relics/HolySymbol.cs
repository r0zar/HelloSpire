using System.Collections.Generic;
using System.Threading.Tasks;
using HelloSpire.HelloSpireCode.Characters.PaladinContent.Faith;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;

namespace HelloSpire.HelloSpireCode.Characters.PaladinContent.Relics;

/// <summary>
/// The Paladin's starter relic. At the start of each combat, gain 1 Faith in your highest deity.
///
/// With Faith scarce (no common generates it), this is the one guaranteed source in the game:
/// the mechanic is always faintly alive, and devotion snowballs slowly through the relic. On a
/// fresh combat every track is 0, so "highest" defaults to Torm.
///
/// The design's other half -- consumed for a deity relic once run-total Faith in one deity hits
/// a milestone -- needs run-persistent state and is a later piece.
///
/// Mirrors Cracked Core's shape: fires on the owner's first turn via BeforeSideTurnStart.
/// </summary>
public sealed class HolySymbol : PaladinRelic
{
    public override RelicRarity Rarity => RelicRarity.Starter;

    protected override IEnumerable<DynamicVar> CanonicalVars => [new DynamicVar("Faith", 1m)];

    public override async Task BeforeSideTurnStart(PlayerChoiceContext choiceContext, CombatSide side,
        IReadOnlyList<Creature> participants, ICombatState combatState)
    {
        if (!participants.Contains(Owner.Creature) || Owner.PlayerCombatState.TurnNumber > 1) return;
        Flash();
        var (deity, _) = FaithTracks.Highest(Owner.PlayerCombatState);
        FaithTracks.Gain(Owner.PlayerCombatState, deity, (int)DynamicVars["Faith"].BaseValue);
        await Task.CompletedTask;
    }
}
