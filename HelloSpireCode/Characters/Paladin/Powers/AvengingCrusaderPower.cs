using System.Threading.Tasks;
using HelloSpire.HelloSpireCode.Powers;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.ValueProps;

namespace HelloSpire.HelloSpireCode.Characters.PaladinContent;

/// <summary>
/// Attacks charge, judgments release: whenever the bearer plays an Attack, gain Amount Spirit;
/// whenever the bearer Judges an enemy, deal their Spirit in damage to it. One stat sizes
/// heals and verdicts both.
/// </summary>
public sealed class AvengingCrusaderPower : HelloSpirePower, IJudgeTrigger
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;

    public override async Task AfterCardPlayed(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (cardPlay.Card.Owner?.Creature != Owner || cardPlay.Card.Type != CardType.Attack) return;
        if (Owner.Player is not { } player) return;
        Flash();
        await Spirit.Gain(choiceContext, player, (int)Amount);
    }

    public async Task OnJudgeInstance(PlayerChoiceContext ctx, Creature target)
    {
        if (Owner.Player is not { } player) return;
        var spirit = Spirit.Of(player);
        if (spirit <= 0) return;
        await CreatureCmd.Damage(ctx, [target], spirit, ValueProp.Unpowered, Owner);
    }
}
