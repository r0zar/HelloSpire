using HelloSpire.HelloSpireCode.Gunslinger.Powers;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.ValueProps;

namespace HelloSpire.HelloSpireCode.Gunslinger.Cylinder;

/// <summary>What one Fire actually did. Cards branch on this constantly.</summary>
public sealed class FireResult
{
    /// <summary>The chamber was empty: no damage, no effect, but the hammer still moved.</summary>
    public bool WasClick { get; init; }

    /// <summary>The Round that was fired, or null on a Click.</summary>
    public Round? Round { get; init; }

    /// <summary>Damage actually dealt, after Deadeye, relics and enemy defences.</summary>
    public int DamageDealt { get; init; }

    public bool Hit => !WasClick;
}

/// <summary>Per-card adjustments to a Fire. Defaults reproduce the plain "Fire 1".</summary>
public sealed class FireOptions
{
    /// <summary>Flat bonus for Rounds fired by this card (Trick Shot, Run the Cylinder+, Last Word+).</summary>
    public int BonusDamage { get; init; }

    /// <summary>Multiplier on the Round's damage (One Bullet Left, Executioner's Calm).</summary>
    public decimal Multiplier { get; init; } = 1m;

    /// <summary>Forces the shot through Block regardless of ammunition (Through the Coat).</summary>
    public bool IgnoreBlock { get; init; }

    /// <summary>Extra repeats of the damage only, skipping the Round's effect (Double-Tap).</summary>
    public int ExtraDamageRepeats { get; init; }

    /// <summary>Suppresses the Round's non-damage effect (No Witnesses' follow-up hits).</summary>
    public bool SkipRoundEffect { get; init; }

    public static readonly FireOptions Default = new();
}

/// <summary>Shorthand for the nine kinds of ammunition.</summary>
public static class Rounds
{
    public static Round Lead() => new LeadRound();
    public static Round Heavy() => new HeavyRound();
    public static Round Crippling() => new CripplingRound();
    public static Round Piercing() => new PiercingRound();
    public static Round Guard() => new GuardRound();
    public static Round Smoke() => new SmokeRound();
    public static Round Rending() => new RendingRound();
    public static Round BlackPowder(int damage = 16) => new BlackPowderRound { PrintedDamage = damage };
    public static Round DeadMans(int damage = 24) => new DeadMansRound { PrintedDamage = damage };

    /// <summary>Bottomless Bandolier's pool: the four special Rounds that are not situational.</summary>
    public static readonly Func<Round>[] Special = [Heavy, Crippling, Piercing, Guard];
}

/// <summary>
/// The rules of the gun: Load, Fire, Cycle, Spin, Click and Self-Fire.
///
/// Every Gunslinger card, relic and potion goes through this class, so the six-chamber rules are
/// stated once. Cards stay short enough to read as their own card text.
/// </summary>
public static class Revolver
{
    /// <summary>The cylinder if combat has already created it, without creating one.</summary>
    public static CylinderPower? Peek(GunContext gun) => gun.Player.Creature?.GetPower<CylinderPower>();

    /// <summary>
    /// The cylinder, creating it if this is the first Gunslinger effect of the combat.
    /// Applied silently — the gun is always there, it is not a buff the player just gained.
    /// </summary>
    public static async Task<CylinderPower?> Get(PlayerChoiceContext ctx, GunContext gun)
    {
        var creature = gun.Player.Creature;
        if (creature == null) return null;

        var existing = creature.GetPower<CylinderPower>();
        if (existing != null) return existing;

        return await PowerCmd.Apply<CylinderPower>(creature, 1m, creature, gun.Card, true);
    }

    // ---------------------------------------------------------------- Load

    /// <summary>
    /// Loads <paramref name="count"/> Rounds from <paramref name="factory"/>, each into the first
    /// empty chamber clockwise from the hammer.
    ///
    /// Loading into a full cylinder is deliberately still legal: it overwrites from the hammer
    /// forwards and discards what was there. That keeps ammo cards from ever being dead draws while
    /// still making over-loading a real cost. Loading never moves the hammer.
    /// </summary>
    public static async Task Load(PlayerChoiceContext ctx, GunContext gun, Func<Round> factory, int count = 1)
    {
        var cylinder = await Get(ctx, gun);
        if (cylinder == null) return;

        var overwriteOffset = 0;
        for (var i = 0; i < count; i++)
        {
            var round = factory();
            var slot = NextLoadSlot(cylinder, gun, ref overwriteOffset);
            cylinder.Chambers[slot] = round;
            cylinder.SyncDisplay();
            await GunslingerHooks.NotifyLoaded(ctx, gun, round);
        }
    }

    /// <summary>Loads a single, already-built Round.</summary>
    public static Task Load(PlayerChoiceContext ctx, GunContext gun, Round round) =>
        Load(ctx, gun, () => round, 1);

    /// <summary>
    /// Where the next Round goes. Stacked Chamber redirects a single Load under the hammer; failing
    /// that it is the first empty chamber clockwise, and failing that an overwrite from the hammer.
    /// </summary>
    private static int NextLoadSlot(CylinderPower cylinder, GunContext gun, ref int overwriteOffset)
    {
        var stacked = gun.Player.Creature?.GetPower<StackedChamberPower>();
        if (stacked != null)
        {
            stacked.Consume();
            return cylinder.Hammer;
        }

        for (var step = 0; step < CylinderPower.ChamberCount; step++)
        {
            var index = cylinder.Offset(step);
            if (cylinder.Chambers[index] == null) return index;
        }

        var overwrite = cylinder.Offset(overwriteOffset);
        overwriteOffset++;
        return overwrite;
    }

    /// <summary>Fills every empty chamber. Speedloader, Perfect Reload, True Iron, Speedloader Flask.</summary>
    public static async Task FillEmpty(PlayerChoiceContext ctx, GunContext gun, Func<Round> factory)
    {
        var cylinder = await Get(ctx, gun);
        if (cylinder == null) return;

        for (var step = 0; step < CylinderPower.ChamberCount; step++)
        {
            var index = cylinder.Offset(step);
            if (cylinder.Chambers[index] != null) continue;

            var round = factory();
            cylinder.Chambers[index] = round;
            cylinder.SyncDisplay();
            await GunslingerHooks.NotifyLoaded(ctx, gun, round);
        }
    }

    /// <summary>Loads into a random empty chamber. Russian Roulette's Dead Man's Round.</summary>
    public static async Task LoadRandomEmpty(PlayerChoiceContext ctx, GunContext gun, Round round)
    {
        var cylinder = await Get(ctx, gun);
        if (cylinder == null) return;

        var empty = new List<int>();
        for (var index = 0; index < CylinderPower.ChamberCount; index++)
            if (cylinder.Chambers[index] == null) empty.Add(index);

        if (empty.Count == 0)
        {
            await Load(ctx, gun, round);
            return;
        }

        var pick = empty[Math.Clamp(gun.Player.RunState.Rng.Chaotic.NextInt(0, empty.Count - 1), 0, empty.Count - 1)];
        cylinder.Chambers[pick] = round;
        cylinder.SyncDisplay();
        await GunslingerHooks.NotifyLoaded(ctx, gun, round);
    }

    // ---------------------------------------------------------------- Fire

    /// <summary>
    /// Resolves the chamber under the hammer, then advances the hammer — whether or not it was loaded.
    ///
    /// A loaded chamber deals its damage as an Attack from the source card, so Strength, Weak and
    /// Vulnerable all apply; Deadeye is consumed by the first successful Round only. An empty
    /// chamber Clicks: nothing happens, the hammer still moves.
    /// </summary>
    public static async Task<FireResult> Fire(PlayerChoiceContext ctx, GunContext gun, Creature? target,
        FireOptions? options = null)
    {
        options ??= FireOptions.Default;

        var cylinder = await Get(ctx, gun);
        if (cylinder == null) return new FireResult { WasClick = true };

        var round = cylinder.UnderHammer;
        cylinder.Chambers[cylinder.Hammer] = null;
        cylinder.Advance();
        cylinder.FiredThisTurn = true;
        cylinder.SyncDisplay();

        if (round == null)
        {
            var click = new FireResult { WasClick = true };
            await GunslingerHooks.NotifyFired(ctx, gun, click);
            return click;
        }

        var damage = round.Damage
                     + options.BonusDamage
                     + GunslingerHooks.RoundDamageBonus(round, gun)
                     + await ConsumeDeadeye(gun);

        if (options.Multiplier != 1m) damage = (int)Math.Floor(damage * options.Multiplier);
        if (damage < 0) damage = 0;

        var props = options.IgnoreBlock ? ValueProp.Unblockable : round.Props;
        var hits = 1 + Math.Max(0, options.ExtraDamageRepeats);

        var dealt = 0;
        if (target != null && damage > 0 && gun.Card != null)
        {
            var attack = await DamageCmd.Attack(damage)
                .WithHitCount(hits)
                .WithValueProp(props)
                .FromCard(gun.Card)
                .Targeting(target)
                .WithHitFx("vfx/vfx_attack_blunt", null, "blunt_attack.mp3")
                .Execute(ctx);

            dealt = attack.Results.Sum(result => result.TotalDamage);
        }

        cylinder.RoundsFiredThisCombat++;

        if (!options.SkipRoundEffect) await round.Resolve(ctx, gun, target);

        var result = new FireResult { Round = round, DamageDealt = dealt };
        await GunslingerHooks.NotifyFired(ctx, gun, result);

        if (cylinder.IsEmpty) await GunslingerHooks.NotifyCylinderEmptied(ctx, gun);

        return result;
    }

    /// <summary>
    /// Fires the chamber under the hammer at every enemy at once, resolving the Round's non-damage
    /// effect only once. No Witnesses turns one good chamber into a room-clear.
    /// </summary>
    public static async Task<FireResult> FireAtAll(PlayerChoiceContext ctx, GunContext gun,
        FireOptions? options = null)
    {
        options ??= FireOptions.Default;

        var cylinder = await Get(ctx, gun);
        if (cylinder == null) return new FireResult { WasClick = true };

        var round = cylinder.UnderHammer;
        cylinder.Chambers[cylinder.Hammer] = null;
        cylinder.Advance();
        cylinder.FiredThisTurn = true;
        cylinder.SyncDisplay();

        if (round == null)
        {
            var click = new FireResult { WasClick = true };
            await GunslingerHooks.NotifyFired(ctx, gun, click);
            return click;
        }

        var damage = round.Damage
                     + options.BonusDamage
                     + GunslingerHooks.RoundDamageBonus(round, gun)
                     + await ConsumeDeadeye(gun);

        if (damage < 0) damage = 0;

        var enemies = GunslingerEffects.Enemies(gun);
        var dealt = 0;

        if (enemies.Count > 0 && damage > 0 && gun.Card != null)
        {
            var attack = await DamageCmd.Attack(damage)
                .WithValueProp(options.IgnoreBlock ? ValueProp.Unblockable : round.Props)
                .FromCard(gun.Card)
                .TargetingFiltered(enemies)
                .Execute(ctx);

            dealt = attack.Results.Sum(result => result.TotalDamage);
        }

        cylinder.RoundsFiredThisCombat++;
        await round.Resolve(ctx, gun, enemies.FirstOrDefault());

        var result = new FireResult { Round = round, DamageDealt = dealt };
        await GunslingerHooks.NotifyFired(ctx, gun, result);
        return result;
    }

    /// <summary>Fire X: the same Fire, X times, at the same target.</summary>
    public static async Task<List<FireResult>> FireTimes(PlayerChoiceContext ctx, GunContext gun, Creature? target,
        int times, FireOptions? options = null)
    {
        var results = new List<FireResult>();
        for (var i = 0; i < times; i++) results.Add(await Fire(ctx, gun, target, options));
        return results;
    }

    /// <summary>Fire X, choosing a fresh random enemy for each shot. Trick Shot.</summary>
    public static async Task<List<FireResult>> FireAtRandom(PlayerChoiceContext ctx, GunContext gun, int times,
        FireOptions? options = null)
    {
        var results = new List<FireResult>();
        for (var i = 0; i < times; i++)
            results.Add(await Fire(ctx, gun, GunslingerEffects.RandomEnemy(gun), options));
        return results;
    }

    /// <summary>Fire until a chamber Clicks, up to <paramref name="max"/> shots. Run the Cylinder.</summary>
    public static async Task<List<FireResult>> FireUntilClick(PlayerChoiceContext ctx, GunContext gun,
        Creature? target, int max, FireOptions? options = null)
    {
        var results = new List<FireResult>();
        for (var i = 0; i < max; i++)
        {
            var result = await Fire(ctx, gun, target, options);
            results.Add(result);
            if (result.WasClick) break;
        }
        return results;
    }

    /// <summary>
    /// Deadeye is all-or-nothing: the first successful Round takes the whole stack and the rest of a
    /// salvo gets nothing. That is what stops Fire 6 from multiplying it six times over.
    /// </summary>
    private static async Task<int> ConsumeDeadeye(GunContext gun)
    {
        var deadeye = gun.Player.Creature?.GetPower<DeadeyePower>();
        if (deadeye == null) return 0;

        var amount = (int)deadeye.Amount;
        if (amount <= 0) return 0;

        await PowerCmd.Remove(deadeye);
        return amount;
    }

    // ------------------------------------------------------- Cycle and Spin

    /// <summary>Advances the hammer without firing. The Gunslinger's deterministic setup tool.</summary>
    public static async Task Cycle(PlayerChoiceContext ctx, GunContext gun, int steps = 1)
    {
        var cylinder = await Get(ctx, gun);
        if (cylinder == null) return;

        cylinder.Advance(steps);
        cylinder.SyncDisplay();
    }

    /// <summary>
    /// Moves the hammer to a random chamber. The result is only revealed once it resolves, and a
    /// card that Spins then acts gives the player no window in between — that is the gamble.
    /// </summary>
    public static async Task Spin(PlayerChoiceContext ctx, GunContext gun)
    {
        var cylinder = await Get(ctx, gun);
        if (cylinder == null) return;

        var rng = gun.Player.RunState.Rng.Chaotic;
        cylinder.Hammer = Math.Clamp(rng.NextInt(0, CylinderPower.ChamberCount - 1),
            0, CylinderPower.ChamberCount - 1);
        cylinder.SyncDisplay();

        await GunslingerHooks.NotifySpun(ctx, gun);
    }

    // ------------------------------------------------------------ Self-Fire

    /// <summary>
    /// Points the gun the wrong way. A loaded chamber costs the Round's printed damage as HP loss —
    /// no Strength, no Weak, no Deadeye, and no Block, Armor or Dodge to hide behind. The Round's
    /// other effects do not happen. An empty chamber just Clicks.
    /// </summary>
    public static async Task<FireResult> SelfFire(PlayerChoiceContext ctx, GunContext gun)
    {
        var cylinder = await Get(ctx, gun);
        if (cylinder == null) return new FireResult { WasClick = true };

        var round = cylinder.UnderHammer;
        cylinder.Chambers[cylinder.Hammer] = null;
        cylinder.Advance();
        cylinder.SyncDisplay();

        if (round == null)
        {
            var click = new FireResult { WasClick = true };
            await GunslingerHooks.NotifyFired(ctx, gun, click);
            return click;
        }

        await GunslingerEffects.LoseHp(ctx, gun, round.Damage);

        var result = new FireResult { Round = round, DamageDealt = round.Damage };
        await GunslingerHooks.NotifyFired(ctx, gun, result);
        return result;
    }

    // --------------------------------------------------------- Chamber picks

    /// <summary>
    /// Moves the most valuable loaded chamber under the hammer.
    ///
    /// The design asks the player to choose a chamber here. There is no base-game selection screen
    /// for "pick one of six chambers", so until a cylinder UI exists the card picks the highest
    /// printed damage — which is the choice a player makes almost every time anyway.
    /// </summary>
    public static async Task<bool> MoveBestLoadedUnderHammer(PlayerChoiceContext ctx, GunContext gun)
    {
        var cylinder = await Get(ctx, gun);
        if (cylinder == null || cylinder.IsEmpty) return false;

        var bestIndex = -1;
        var bestDamage = int.MinValue;
        for (var index = 0; index < CylinderPower.ChamberCount; index++)
        {
            var round = cylinder.Chambers[index];
            if (round == null || round.Damage <= bestDamage) continue;
            bestDamage = round.Damage;
            bestIndex = index;
        }

        if (bestIndex < 0) return false;

        (cylinder.Chambers[cylinder.Hammer], cylinder.Chambers[bestIndex]) =
            (cylinder.Chambers[bestIndex], cylinder.Chambers[cylinder.Hammer]);
        cylinder.SyncDisplay();
        return true;
    }

    /// <summary>
    /// Packs every loaded Round into consecutive chambers starting under the hammer, biggest first.
    /// Stack the Cylinder's "rearrange in any order and choose the hammer position", resolved to the
    /// arrangement a player would pick, for the same reason as above.
    /// </summary>
    public static async Task StackForBurst(PlayerChoiceContext ctx, GunContext gun)
    {
        var cylinder = await Get(ctx, gun);
        if (cylinder == null) return;

        var rounds = cylinder.Chambers
            .Where(round => round != null)
            .OrderByDescending(round => round!.Damage)
            .ToList();

        for (var index = 0; index < CylinderPower.ChamberCount; index++) cylinder.Chambers[index] = null;

        cylinder.Hammer = 0;
        for (var index = 0; index < rounds.Count; index++) cylinder.Chambers[index] = rounds[index];
        cylinder.SyncDisplay();
    }

    /// <summary>Replaces the chamber under the hammer outright. Black Powder.</summary>
    public static async Task ReplaceUnderHammer(PlayerChoiceContext ctx, GunContext gun, Round round)
    {
        var cylinder = await Get(ctx, gun);
        if (cylinder == null) return;

        cylinder.Chambers[cylinder.Hammer] = round;
        cylinder.SyncDisplay();
        await GunslingerHooks.NotifyLoaded(ctx, gun, round);
    }
}
