using System.Linq;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Potions;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;

using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Models;
using HelloSpire.HelloSpireCode.Alchemist;
using HelloSpire.HelloSpireCode.Alchemist.Potions;
namespace HelloSpire.HelloSpireCode.Alchemist.Lab;

/// <summary>What a Distill actually consumed. Essence Distillation branches on the rarity.</summary>
public sealed class DistillResult
{
    public bool Distilled { get; init; }
    public PotionRarity Rarity { get; init; }
    public bool WasVolatile { get; init; }

    public static readonly DistillResult Nothing = new();
}

/// <summary>
/// The rules of the belt: Brew, Volatile, Distill and Potency.
///
/// Every Alchemist card, relic and potion goes through this class, so the Potion rules are stated
/// once and the cards stay short enough to read as their own card text. The base-game calls it
/// needs all live behind <see cref="LabBridge"/>; what lives here is the character's rules on top
/// of them.
/// </summary>
public static class Belt
{
    /// <summary>
    /// Brew a Potion into the first empty slot.
    ///
    /// Volatile unless the caller says otherwise. The Great Work's Philosopher's Stone is the one
    /// exception in the whole class — a trophy meant to survive the fight. Every other Brew,
    /// Alchemize and Gilded Execution's real Rare Potions included, vanishes at combat end.
    ///
    /// A full belt is not an error. The card still resolved; the Potion is simply lost. That is a
    /// real cost the Brew-heavy hand is supposed to feel.
    /// </summary>
    public static async Task<PotionModel?> Brew(PlayerChoiceContext ctx, LabContext lab,
        PotionModel? potion, bool volatilePotion = true)
    {
        if (potion == null) return null;

        // Marked Volatile BEFORE calling the bridge, not after: PotionCmd.TryToProcure (confirmed
        // via sts2.dll) places this exact instance into the belt and creates its NPotion visual
        // node synchronously, so VolatilePotionOutlinePatch's Reload() postfix fires and checks
        // this HashSet before the line below would otherwise have added anything to it. Marking
        // afterward means every freshly-Brewed Volatile Potion renders one frame too late to ever
        // show its outline, since Reload() isn't called again just because Volatile state changes.
        var bench = await AlchemistEffects.Bench(ctx, lab);
        if (volatilePotion && bench != null) bench.Volatile.Add(potion);

        var placed = await LabBridge.Current.Brew(ctx, lab.Player, potion);
        if (placed == null)
        {
            bench?.Volatile.Remove(potion);
            return null;
        }

        if (bench != null)
        {
            bench.BrewedThisTurn++;
            bench.BrewedThisCombat++;

            // Distillation Mastery's payload: the next successful Brew's whole effect is scaled,
            // then the multiplier resets. Every declared DynamicVar counts, not just Damage/Block --
            // "the Potion's effect increased by 50%" is meant to mean all of it.
            if (bench.BrewBonusMultiplier != 1m)
            {
                foreach (var key in placed.DynamicVars.Keys.Cast<string>().ToList())
                    placed.DynamicVars[key].BaseValue *= bench.BrewBonusMultiplier;
                bench.BrewBonusMultiplier = 1m;
            }
        }

        // Any Attack that Brews leaves a Volatile Residue behind in the discard pile -- a passive
        // tax on the Brew-and-hit cards, not something any one card grants itself.
        if (lab.Card?.Type == CardType.Attack)
            await Alchemy.CreateVolatileResidue(ctx, lab, PileType.Discard);

        await AlchemistHooks.NotifyBrewed(ctx, lab, placed);
        return placed;
    }

    /// <summary>Offer a choice of distinct random Combat Potions and Brew the pick. Buy Ingredients.</summary>
    public static async Task<PotionModel?> BrewChoice(PlayerChoiceContext ctx, LabContext lab,
        int count = 3, PotionRarity? rarity = PotionRarity.Common)
    {
        var options = LabBridge.Current.CombatPotionOptions(lab.Player, count, rarity);
        var chosen = await LabBridge.Current.ChoosePotionOption(ctx, lab.Player, options);
        return await Brew(ctx, lab, chosen);
    }

    /// <summary>Brew a random Potion from the curated Combat Potion pool.</summary>
    public static Task<PotionModel?> BrewRandom(PlayerChoiceContext ctx, LabContext lab,
        PotionRarity? rarity = PotionRarity.Common) =>
        Brew(ctx, lab, LabBridge.Current.RandomCombatPotion(lab.Player, rarity));

    /// <summary>Fill every empty slot with random Combat Potions. Magnum Opus and Panacea of Plenty.</summary>
    public static async Task<int> FillEmpty(PlayerChoiceContext ctx, LabContext lab)
    {
        var brewed = 0;

        // Bounded by slot count rather than by "until Brew fails", so a bridge that cannot report
        // a full belt still terminates.
        var attempts = EmptySlots(lab);
        for (var i = 0; i < attempts; i++)
        {
            if (await BrewRandom(ctx, lab) == null) break;
            brewed++;
        }

        return brewed;
    }

    // ------------------------------------------------------------------ Distill

    /// <summary>
    /// Discard a held Potion without resolving it.
    ///
    /// Distill is deliberately not "using" a Potion: effects keyed off using one do not fire,
    /// effects keyed off a slot becoming empty do. That split is what lets the Distillation
    /// archetype and the Potion-use archetype be different decks.
    /// </summary>
    public static async Task<DistillResult> Distill(PlayerChoiceContext ctx, LabContext lab)
    {
        var held = Held(lab);
        if (held.Count == 0) return DistillResult.Nothing;

        var chosen = held.Count == 1
            ? held[0]
            : await LabBridge.Current.ChoosePotion(ctx, lab.Player, held,
                new LocString("cards", "HELLOSPIRE-ALCHEMIST_DISTILL_CHOICE.header"));

        if (chosen == null) return DistillResult.Nothing;
        return await Distill(ctx, lab, chosen);
    }

    /// <summary>Distill one named Potion, for the cards that have already picked.</summary>
    public static async Task<DistillResult> Distill(PlayerChoiceContext ctx, LabContext lab, PotionModel potion)
    {
        var bench = await AlchemistEffects.Bench(ctx, lab);
        var wasVolatile = bench?.Volatile.Contains(potion) ?? false;

        await LabBridge.Current.Discard(ctx, lab.Player, potion);

        if (bench != null)
        {
            bench.Volatile.Remove(potion);
            bench.DistilledThisTurn++;
            bench.DistilledThisCombat++;
        }

        await NotifySlotEmptied(ctx, lab);
        await AlchemistHooks.NotifyDistilled(ctx, lab, potion);

        return new DistillResult { Distilled = true, Rarity = potion.Rarity, WasVolatile = wasVolatile };
    }

    /// <summary>
    /// Distill as many as the player is willing to, one choice at a time -- they can stop with
    /// Potions still held. Grand Combustion.
    /// </summary>
    public static async Task<int> DistillAny(PlayerChoiceContext ctx, LabContext lab, int max)
    {
        var count = 0;
        while (count < max && Held(lab).Count > 0)
        {
            var chosen = await LabBridge.Current.ChoosePotion(ctx, lab.Player, Held(lab),
                new LocString("cards", "HELLOSPIRE-ALCHEMIST_DISTILL_CHOICE.header"), allowStop: true);
            if (chosen == null) break;

            var result = await Distill(ctx, lab, chosen);
            if (!result.Distilled) break;
            count++;
        }

        return count;
    }

    // ------------------------------------------------------------------ reading the belt

    public static IReadOnlyList<PotionModel> Held(LabContext lab) => LabBridge.Current.Held(lab.Player);

    /// <summary>Total slots, including this combat's temporary Volatile-only ones.</summary>
    public static int Slots(LabContext lab)
    {
        var bench = AlchemistEffects.Peek(lab);
        return LabBridge.Current.SlotCount(lab.Player) + (bench?.TemporarySlots ?? 0);
    }

    public static int EmptySlots(LabContext lab) => Math.Max(0, Slots(lab) - Held(lab).Count);

    public static bool IsFull(LabContext lab) => EmptySlots(lab) == 0;

    public static bool IsEmpty(LabContext lab) => Held(lab).Count == 0;

    /// <summary>
    /// Extra slots for the rest of this combat. The slots are REAL -- grown on the player via the
    /// bridge, so the game's own belt UI and Procure checks all see them -- and the bench records
    /// the count so <see cref="LabPower.AfterCombatEnd"/> can take them back.
    /// </summary>
    public static async Task GrantTemporarySlots(PlayerChoiceContext ctx, LabContext lab, int count)
    {
        if (count <= 0) return;

        var bench = await AlchemistEffects.Bench(ctx, lab);
        if (bench == null) return;

        await LabBridge.Current.GainSlots(lab.Player, count);
        bench.TemporarySlots += count;
    }

    // ------------------------------------------------------------------ using a Potion

    /// <summary>
    /// Called when the player uses a Potion, from wherever the game announces that.
    ///
    /// Fired by <see cref="PotionUsePatch"/>, a Harmony patch on
    /// <see cref="MegaCrit.Sts2.Core.Models.PotionModel.OnUseWrapper"/> — see that class for why
    /// that method and not one of the base game's own potion-use hooks. Potency is applied by the
    /// same patch's prefix, which bumps the Potion's damage and Block vars before OnUse computes
    /// with them; Bottled Time's save-a-Potion is handled there too, after this method runs.
    /// </summary>
    public static async Task OnPotionUsed(PlayerChoiceContext ctx, LabContext lab, PotionModel potion, Creature? target)
    {
        var bench = await AlchemistEffects.Bench(ctx, lab);
        if (bench != null)
        {
            bench.PotionsUsedThisTurn++;
            bench.UsedThisCombat.Add(potion);
            bench.Volatile.Remove(potion);
        }

        await NotifySlotEmptied(ctx, lab);
        await AlchemistHooks.NotifyPotionUsed(ctx, lab, potion, target);
    }

    /// <summary>
    /// Potency to add to a Volatile Potion's damage and Block values.
    ///
    /// Zero for anything the player found, bought or Procured. That restriction is the entire
    /// reason this class is allowed an exception to "Potions ignore stat scaling" — without it,
    /// Potency becomes a blanket buff to whatever Rare Potion a shop happened to sell, which no
    /// part of the Combat Potion curation covers. Applied by PotionUsePatch's prefix, before the
    /// Potion's own OnUse runs.
    /// </summary>
    public static int PotencyBonus(LabContext lab, PotionModel potion)
    {
        var bench = AlchemistEffects.Peek(lab);
        if (bench == null || !bench.Volatile.Contains(potion)) return 0;

        return AlchemistEffects.Potency(lab);
    }

    /// <summary>Volatile Potions do not survive the fight. Called when combat ends.</summary>
    public static async Task ClearVolatile(PlayerChoiceContext ctx, LabContext lab)
    {
        var bench = AlchemistEffects.Peek(lab);
        if (bench == null) return;

        foreach (var potion in bench.Volatile.ToList())
            await LabBridge.Current.Discard(ctx, lab.Player, potion);

        bench.Volatile.Clear();
    }

    // ------------------------------------------------------------------ Unstable Concoction

    /// <summary>
    /// Add to whatever Unstable Concoction is currently held, if any. Silent no-op otherwise -- the belt
    /// was full when Alchemical Satchel tried to Brew one, or it's already been used this combat --
    /// same "a full belt is not an error" rule Brew itself follows.
    /// </summary>
    public static async Task Infuse(PlayerChoiceContext ctx, LabContext lab, decimal damage = 0,
        decimal block = 0, decimal poison = 0, decimal energy = 0)
    {
        if (Held(lab).OfType<UnstableConcoction>().FirstOrDefault() is not { } mixture) return;

        if (damage > 0) mixture.DynamicVars.Damage.BaseValue += damage;
        if (block > 0) mixture.DynamicVars.Block.BaseValue += block;
        if (poison > 0) mixture.DynamicVars["Poison"].BaseValue += poison;
        if (energy > 0) mixture.DynamicVars["Energy"].BaseValue += energy;

        var total = damage + block + poison + energy;
        var bench = AlchemistEffects.Peek(lab);
        if (bench != null) bench.InfusedThisTurn += total;

        await AlchemistHooks.NotifyInfused(ctx, lab, total);
    }

    private static async Task NotifySlotEmptied(PlayerChoiceContext ctx, LabContext lab)
    {
        var bench = AlchemistEffects.Peek(lab);
        if (bench != null) bench.SlotsEmptiedThisTurn++;

        await AlchemistHooks.NotifySlotEmptied(ctx, lab);
    }
}
