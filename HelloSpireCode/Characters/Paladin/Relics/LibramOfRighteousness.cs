using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;

namespace HelloSpire.HelloSpireCode.Characters.PaladinContent.Relics;

/// <summary>
/// The Paladin's starter relic: at the start of each combat, gain Seal of Righteousness.
/// The Defect's Cracked Core, translated: you start every fight with your engine primed and the
/// starter Judgment has something to consume. (Librams are WoW's paladin relic slot.)
/// </summary>
public sealed class LibramOfRighteousness : PaladinRelic
{
    public override RelicRarity Rarity => RelicRarity.Starter;

    public override async Task BeforeSideTurnStart(PlayerChoiceContext choiceContext, CombatSide side,
        IReadOnlyList<Creature> participants, ICombatState combatState)
    {
        if (!participants.Contains(Owner.Creature) || Owner.PlayerCombatState.TurnNumber > 1) return;
        Flash();
        await Seals.Grant<SealOfRighteousnessPower>(choiceContext, Owner, 2m);
    }
}
