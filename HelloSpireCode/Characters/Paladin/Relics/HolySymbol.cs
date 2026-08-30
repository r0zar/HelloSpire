using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;

namespace HelloSpire.HelloSpireCode.Characters.PaladinContent.Relics;

/// <summary>
/// Paladin pool relic: at the start of each combat, gain 3 Spirit. Currently the stat's only
/// source -- drafting this is what turns Mend from a flat heal into a scaling one. Was the
/// starter relic before the Holy Book took that slot.
///
/// Fires on the owner's first turn via BeforeSideTurnStart, like the Regent's Divine Right.
/// </summary>
public sealed class HolySymbol : PaladinRelic
{
    public override RelicRarity Rarity => RelicRarity.Common;

    protected override IEnumerable<DynamicVar> CanonicalVars => [new DynamicVar("Spirit", 3m)];

    public override async Task BeforeSideTurnStart(PlayerChoiceContext choiceContext, CombatSide side,
        IReadOnlyList<Creature> participants, ICombatState combatState)
    {
        if (!participants.Contains(Owner.Creature) || Owner.PlayerCombatState.TurnNumber > 1) return;
        Flash();
        await Spirit.Gain(choiceContext, Owner, (int)DynamicVars["Spirit"].BaseValue);
    }
}
