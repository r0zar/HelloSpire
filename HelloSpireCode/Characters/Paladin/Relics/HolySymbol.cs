using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;

namespace HelloSpire.HelloSpireCode.Characters.PaladinContent.Relics;

/// <summary>
/// The Paladin's starter relic: at the start of each combat, gain 3 holy Faith.
///
/// This is the whole holy "home polarity": everyone starts holy because everyone holds this
/// relic, not because of a rule. A draftable Fallen counterpart that replaces it -- starting
/// unholy instead -- is how an unholy deck commits, and is a later piece.
///
/// Mirrors Divine Right (the Regent's starter grants Stars the same way): fires on the owner's
/// first turn via BeforeSideTurnStart.
/// </summary>
public sealed class HolySymbol : PaladinRelic
{
    public override RelicRarity Rarity => RelicRarity.Starter;

    protected override IEnumerable<DynamicVar> CanonicalVars => [new DynamicVar("Faith", 3m)];

    public override async Task BeforeSideTurnStart(PlayerChoiceContext choiceContext, CombatSide side,
        IReadOnlyList<Creature> participants, ICombatState combatState)
    {
        if (!participants.Contains(Owner.Creature) || Owner.PlayerCombatState.TurnNumber > 1) return;
        Flash();
        Faith.Gain(Owner, (int)DynamicVars["Faith"].BaseValue);
        await Task.CompletedTask;
    }
}
