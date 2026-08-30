using HelloSpire.HelloSpireCode.Gunslinger.Cylinder;
using HelloSpire.HelloSpireCode.Gunslinger.Powers;
using HelloSpire.HelloSpireCode.Characters;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Rooms;

namespace HelloSpire.HelloSpireCode.Gunslinger.Relics;

/// <summary>
/// Shared plumbing for the Gunslinger's relics: a <see cref="GunContext"/>, a first-turn-of-combat
/// signal, and per-turn / per-combat latches.
///
/// The first-turn signal stands in for a combat-start hook deliberately. Loading on the player's
/// first turn rather than during setup is indistinguishable in play — the cards are already in hand
/// either way — and it keeps every relic on hook signatures this mod has seen working code for.
/// </summary>
public abstract class GunslingerRelic : Characters.GunslingerRelic
{
    private bool _combatStarted;

    protected GunContext Gun => GunContext.From(Owner);

    /// <summary>Reset at the start of each of the owner's turns.</summary>
    protected bool UsedThisTurn { get; set; }

    /// <summary>Reset when combat ends.</summary>
    protected bool UsedThisCombat { get; set; }

    public override async Task BeforeSideTurnStart(PlayerChoiceContext ctx, CombatSide side, IReadOnlyList<Creature> participants, ICombatState state)
    {
        if (side != Owner.Creature.Side) return;

        UsedThisTurn = false;

        if (_combatStarted) return;
        _combatStarted = true;

        await OnCombatOpening(ctx, state);
    }

    /// <summary>Runs once, at the top of the player's first turn of a combat.</summary>
    protected virtual Task OnCombatOpening(PlayerChoiceContext ctx, ICombatState state) => Task.CompletedTask;

    public override Task AfterCombatEnd(CombatRoom room)
    {
        _combatStarted = false;
        UsedThisTurn = false;
        UsedThisCombat = false;
        return Task.CompletedTask;
    }
}

/// <summary>
/// Starter relic. Loads three Lead Rounds and one Round of whatever else was in the coat, at the
/// top of each combat — so Quick Draw in the opening hand is a real card rather than a dead one,
/// without removing the need to draft Loading cards.
///
/// The fourth Round is rolled. It is worth more than a fourth Lead on average, but the reason it
/// is there is that it makes the first turn of every fight a slightly different puzzle, which is
/// the character the rest of the set is written for.
/// </summary>
public sealed class OldIron : GunslingerRelic
{
    public override RelicRarity Rarity => RelicRarity.Starter;

    public override RelicModel? GetUpgradeReplacement() => ModelDb.Relic<TrueIron>();

    protected override async Task OnCombatOpening(PlayerChoiceContext ctx, ICombatState state)
    {
        Flash();
        await Revolver.Load(ctx, Gun, Rounds.Lead, 3);
        await Revolver.Load(ctx, Gun, Rounds.RandomSpecial(Gun), 1);
    }
}

/// <summary>
/// The Ancient upgrade to <see cref="OldIron"/>. Every chamber starts loaded, and Lead hits harder.
/// Six prepared Rounds is a lot of stored damage, but it still takes Fire cards to spend it, and the
/// Lead bonus is narrow enough that special ammunition stays worth drafting.
/// </summary>
public sealed class TrueIron : GunslingerRelic, IRoundDamageModifier
{
    public override RelicRarity Rarity => RelicRarity.Starter;

    protected override async Task OnCombatOpening(PlayerChoiceContext ctx, ICombatState state)
    {
        Flash();
        await Revolver.FillEmpty(ctx, Gun, Rounds.Lead);
    }

    public int ModifyRoundDamage(Round round, GunContext gun) => round.IsLead ? 2 : 0;
}

/// <summary>
/// The first Load each combat brings a spare Round with it — and never the one you asked for.
///
/// A common relic that reads "and one more Lead" is a rounding error. Rolling it instead makes the
/// same slot occasionally hand you a Heavy or a Piercing Round in Act 1, which is a real turn.
/// </summary>
public sealed class OiledRag : GunslingerRelic, ILoadListener
{
    public override RelicRarity Rarity => RelicRarity.Common;

    public async Task OnLoaded(PlayerChoiceContext ctx, GunContext gun, Round round)
    {
        if (UsedThisCombat) return;
        UsedThisCombat = true;

        Flash();
        await Revolver.Load(ctx, gun, Rounds.RandomOrdinary(gun));
    }
}

/// <summary>The first Weak each turn also buys a little Block — the bridge between control and defence.</summary>
public sealed class TinBadge : GunslingerRelic, IWeakListener
{
    public override RelicRarity Rarity => RelicRarity.Uncommon;

    public async Task OnWeakApplied(PlayerChoiceContext ctx, GunContext gun, Creature target, int amount)
    {
        if (UsedThisTurn) return;
        UsedThisTurn = true;

        Flash();
        await GunslingerEffects.GainBlock(gun, 3);
    }
}

/// <summary>Catches the one failure state the character really has: an empty gun and no ammo cards.</summary>
public sealed class SpareSpeedloader : GunslingerRelic, ICylinderEmptiedListener
{
    public override RelicRarity Rarity => RelicRarity.Uncommon;

    public async Task OnCylinderEmptied(PlayerChoiceContext ctx, GunContext gun)
    {
        if (UsedThisCombat) return;
        UsedThisCombat = true;

        Flash();
        await Revolver.LoadBetween(ctx, gun, Rounds.Lead, 3, 5);
    }
}

/// <summary>Start each combat with Armor.</summary>
public sealed class LongcoatPlates : GunslingerRelic
{
    public override RelicRarity Rarity => RelicRarity.Rare;

    protected override async Task OnCombatOpening(PlayerChoiceContext ctx, ICombatState state)
    {
        Flash();
        await GunslingerEffects.GainArmor(ctx, Gun, 3);
    }
}

/// <summary>
/// The first Spin each turn pays out either way: a card if it landed on a Round, Block if it did not.
/// It makes Spinning productive without making it safe.
/// </summary>
public sealed class LuckyCoin : GunslingerRelic, ISpinListener
{
    public override RelicRarity Rarity => RelicRarity.Rare;

    public async Task OnSpun(PlayerChoiceContext ctx, GunContext gun)
    {
        if (UsedThisTurn) return;
        UsedThisTurn = true;

        Flash();

        if (Revolver.Peek(gun) is { UnderHammer: not null })
        {
            await GunslingerEffects.Draw(ctx, gun, 1);
            return;
        }

        await GunslingerEffects.GainBlock(gun, 4);
    }
}

/// <summary>The first Round to land each turn hits harder. Consistent, and it rewards aiming.</summary>
public sealed class EngravedHammer : GunslingerRelic, IRoundDamageModifier, IFireListener
{
    public override RelicRarity Rarity => RelicRarity.Rare;

    public int ModifyRoundDamage(Round round, GunContext gun) => UsedThisTurn ? 0 : 4;

    public Task OnFired(PlayerChoiceContext ctx, GunContext gun, FireResult result)
    {
        if (result.Hit && !UsedThisTurn)
        {
            UsedThisTurn = true;
            Flash();
        }
        return Task.CompletedTask;
    }
}

/// <summary>Every Round but Lead hits for more. The reward for building around special ammunition.</summary>
public sealed class IvoryHandle : GunslingerRelic, IRoundDamageModifier
{
    public override RelicRarity Rarity => RelicRarity.Shop;

    public int ModifyRoundDamage(Round round, GunContext gun) => round.IsLead ? 0 : 3;
}
