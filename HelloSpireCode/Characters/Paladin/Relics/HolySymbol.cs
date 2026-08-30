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
/// The Paladin's starter relic. Your first Faith gain each combat is doubled.
///
/// This is the in-combat half of the Holy Symbol. The design's other half -- consumed for a
/// deity relic once run-total Faith in one deity hits a milestone -- needs run-persistent
/// state and is a later piece. Doubling the first gain is enough to make the relic felt on
/// turn one, which is what a starter relic is for.
///
/// Mirrors Cracked Core's shape: arm on the owner's first turn via BeforeSideTurnStart. The
/// doubling itself lives in FaithTracks so it applies to printed and Oath-triggered gains alike.
/// </summary>
public sealed class HolySymbol : PaladinRelic
{
    public override RelicRarity Rarity => RelicRarity.Starter;

    protected override IEnumerable<DynamicVar> CanonicalVars => [new DynamicVar("Bonus", 2m)];

    public override async Task BeforeSideTurnStart(PlayerChoiceContext choiceContext, CombatSide side,
        IReadOnlyList<Creature> participants, ICombatState combatState)
    {
        if (!participants.Contains(Owner.Creature) || Owner.PlayerCombatState.TurnNumber > 1) return;
        FaithTracks.ArmFirstGainMultiplier(Owner.PlayerCombatState, (int)DynamicVars["Bonus"].BaseValue, Flash);
        await Task.CompletedTask;
    }
}
