using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System;
namespace HelloSpire.HelloSpireCode.Characters.PaladinContent.Cards;

/// <summary>
/// Aura of Vitality's little sibling: 1 Energy, all players gain a small Regen. Was an unbounded
/// per-turn party heal -- same stall problem, same cure: Regen is self-limiting and the card
/// Exhausts. Circle of Healing owns the party burst; this is the cheap trickle.
/// </summary>
public sealed class AuraOfMercy() : PaladinCard(1, CardType.Skill, CardRarity.Uncommon, TargetType.Self)
{
    public override CardMultiplayerConstraint MultiplayerConstraint => CardMultiplayerConstraint.MultiplayerOnly;
    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];
    protected override IEnumerable<DynamicVar> CanonicalVars => [new DynamicVar("Regen", 2m)];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await CreatureCmd.TriggerAnim(Owner.Creature, "Cast", Owner.Character.CastAnimDelay);
        foreach (var creature in Owner.Creature.CombatState!.PlayerCreatures.Where(c => c.IsAlive))
            await PowerCmd.Apply<RegenPower>(choiceContext, creature,
                DynamicVars["Regen"].BaseValue, Owner.Creature, this);
    }

    protected override void OnUpgrade() => DynamicVars["Regen"].UpgradeValueBy(1m);
}
