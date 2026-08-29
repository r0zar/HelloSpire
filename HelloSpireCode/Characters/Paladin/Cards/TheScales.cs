using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using HelloSpire.HelloSpireCode.Characters.PaladinContent.Faith;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;

namespace HelloSpire.HelloSpireCode.Characters.PaladinContent.Cards;

/// <summary>Deal damage equal to twice your Faith in Tyr. The payoff-side scaler.
/// Faith is read at play time; nothing is spent.</summary>
public sealed class TheScales() : PaladinCard(2, CardType.Attack, CardRarity.Rare, TargetType.AnyEnemy)
{
    // A DamageVar keeps the card in the normal damage pipeline (Strength, Vulnerable, etc.).
    // Base value is 0; the real number is computed at play time.
    protected override IEnumerable<DynamicVar> CanonicalVars => [new DamageVar(0m, ValueProp.Move)];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        ArgumentNullException.ThrowIfNull(cardPlay.Target);
        var faith = FaithTracks.Amount(Owner.PlayerCombatState, Deity.Tyr);
        await DamageCmd.Attack(faith * 2).FromCard(this).Targeting(cardPlay.Target)
            .WithHitFx("vfx/vfx_attack_slash")
            .Execute(choiceContext);
    }

    protected override void OnUpgrade() { }
}
