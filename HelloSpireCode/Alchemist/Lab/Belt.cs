using MegaCrit.Sts2.Core.Entities.Potions;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;

using MegaCrit.Sts2.Core.Models;
using HelloSpire.HelloSpireCode.Alchemist;
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
    /// Volatile unless the caller says otherwise — Alchemize Procures a real Potion, and The Great
    /// Work's Philosopher's Stone is a trophy meant to survive the fight. Everything else vanishes
    /// at combat end.
    ///
    /// A full belt is not an error. The card still resolved; the Potion is simply lost. That is a
    /// real cost the Brew-heavy hand is supposed to feel.
    /// </summary>
    public static async Task<PotionModel?> Brew(PlayerChoiceContext ctx, LabContext lab,
        PotionModel? potion, bool volatilePotion = true)
    {
        if (potion == null) return null;

        var placed = await LabBridge.Current.Brew(ctx, lab.Player, potion);
        if (placed == null) return null;

        var bench = await AlchemistEffects.Bench(ctx, lab);
        if (bench != null)
        {
            if (volatilePotion) bench.Volatile.Add(placed);
            bench.BrewedThisTurn++;
        }

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
            : await LabBridge.Current.ChoosePotion(ctx, lab.Player, held);

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
        }

        await NotifySlotEmptied(ctx, lab);
        await AlchemistHooks.NotifyDistilled(ctx, lab, potion);

        return new DistillResult { Distilled = true, Rarity = potion.Rarity, WasVolatile = wasVolatile };
    }

    /// <summary>Distill as many as the player is willing to. Grand Combustion.</summary>
    public static async Task<int> DistillAny(PlayerChoiceContext ctx, LabContext lab, int max)
    {
        var count = 0;
        while (count < max && Held(lab).Count > 0)
        {
            var result = await Distill(ctx, lab);
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

    /// <summary>Extra slots for the rest of this combat. Volatile-only, which is what makes them temporary.</summary>
    public static async Task GrantTemporarySlots(PlayerChoiceContext ctx, LabContext lab, int count)
    {
        if (count <= 0) return;

        var bench = await AlchemistEffects.Bench(ctx, lab);
        if (bench != null) bench.TemporarySlots += count;
    }

    // ------------------------------------------------------------------ using a Potion

    /// <summary>
    /// Called when the player uses a Potion, from wherever the game announces that.
    ///
    /// Fired by <see cref="PotionUsePatch"/>, a Harmony patch on
    /// <see cref="MegaCrit.Sts2.Core.Models.PotionModel.OnUseWrapper"/> — see that class for why
    /// that method and not one of the base game's own potion-use hooks. Potency (buffing a Volatile
    /// Potion's own damage/Block) is a separate, still-unaddressed integration point: it needs to
    /// change the values the potion's own OnUse computes with, which means a patch that runs before
    /// OnUse, not a notification that runs after it.
    /// </summary>
    public static async Task OnPotionUsed(PlayerChoiceContext ctx, LabContext lab, PotionModel potion)
    {
        var bench = await AlchemistEffects.Bench(ctx, lab);
        if (bench != null)
        {
            bench.PotionsUsedThisTurn++;
            bench.UsedThisCombat.Add(potion);
            bench.Volatile.Remove(potion);
        }

        await NotifySlotEmptied(ctx, lab);
        await AlchemistHooks.NotifyPotionUsed(ctx, lab, potion);
    }

    /// <summary>
    /// Potency to add to a Volatile Potion's damage and Block values.
    ///
    /// Zero for anything the player found, bought or Procured. That restriction is the entire
    /// reason this class is allowed an exception to "Potions ignore stat scaling" — without it,
    /// Potency becomes a blanket buff to whatever Rare Potion a shop happened to sell, which no
    /// part of the Combat Potion curation covers.
    ///
    /// TODO(Phase 3): the same potion-resolution patch that drives OnPotionUsed must call this and
    /// add the result to the Potion's damage and Block.
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

    private static async Task NotifySlotEmptied(PlayerChoiceContext ctx, LabContext lab)
    {
        var bench = AlchemistEffects.Peek(lab);
        if (bench != null) bench.SlotsEmptiedThisTurn++;

        await AlchemistHooks.NotifySlotEmptied(ctx, lab);
    }
}
