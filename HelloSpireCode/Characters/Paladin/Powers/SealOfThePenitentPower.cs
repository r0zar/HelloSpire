using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;

namespace HelloSpire.HelloSpireCode.Characters.PaladinContent;

/// <summary>
/// The Tithe seal. While held: whenever you discard a card, deal Amount to a random enemy --
/// every Tithe face, every Benediction cycle, every Zealous Offering pings. Judge: deal 6 to ALL.
/// </summary>
public sealed class SealOfThePenitentPower : SealPower
{
    public const decimal JudgeDamage = 6m;

    public override async Task AfterCardDiscarded(PlayerChoiceContext choiceContext, CardModel card)
    {
        if (PassivesDisabled || card.Owner?.Creature != Owner) return;
        var enemy = Owner.Player is { } p ? PaladinEffects.RandomEnemy(p) : null;
        if (enemy == null) return;
        Flash();
        await CreatureCmd.Damage(choiceContext, [enemy], Amount, ValueProp.Unpowered, Owner);
    }

    public override async Task OnJudged(PlayerChoiceContext ctx, Creature target)
    {
        if (Owner.CombatState is not { } state || state.HittableEnemies.Count == 0) return;
        await CreatureCmd.Damage(ctx, state.HittableEnemies, JudgeDamage, ValueProp.Unpowered, Owner);
    }
}
