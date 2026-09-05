using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HelloSpire.HelloSpireCode.Alchemist.Potions;
using HelloSpire.HelloSpireCode.Characters;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Context;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Multiplayer;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Potions;
using MegaCrit.Sts2.Core.Factories;
using MegaCrit.Sts2.Core.GameActions;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Potions;
using MegaCrit.Sts2.Core.Multiplayer.Game;
using MegaCrit.Sts2.Core.Nodes.CommonUi;
using MegaCrit.Sts2.Core.Nodes.Multiplayer;
using MegaCrit.Sts2.Core.Runs;

namespace HelloSpire.HelloSpireCode.Alchemist;

/// <summary>
/// The real ILabBridge: every operation the quarantine declared, implemented against the game's
/// actual APIs (PotionCmd/PotionFactory for the belt, CardSelectCmd for choices, CardCmd/
/// CardPileCmd for the piles).
///
/// ChoosePotion and ChoosePotionOption share one real UI (PotionPickerPopup) and the same
/// multiplayer-sync shape as any other player choice here -- see ChoosePotionFrom. Pressure Burst
/// surfaced this: it used to silently take the first held Potion with no popup at all, which read
/// as "the card did nothing" even though it had picked something.
/// </summary>
public sealed class WiredLabBridge : ILabBridge
{
    // -------------------------------------------------------------------------- the belt
    public IReadOnlyList<PotionModel> Held(Player player) => player.Potions.ToList();

    public int SlotCount(Player player) => player.PotionSlots.Count;

    public async Task<PotionModel?> Brew(PlayerChoiceContext ctx, Player player, PotionModel potion)
    {
        if (!player.HasOpenPotionSlots) return null;   // documented Brew-failure: the Potion is lost
        var result = await PotionCmd.TryToProcure(potion, player);
        return result.success ? result.potion : null;
    }

    public Task Discard(PlayerChoiceContext ctx, Player player, PotionModel potion) =>
        PotionCmd.Discard(potion);

    /// <summary>
    /// The real, weaker Volatile potions Common-rarity combat generation actually hands out (see
    /// VolatileCommonPotions.cs). Alchemize deliberately bypasses this -- it asks for
    /// <c>rarity: null</c> specifically to reach the full, unrestricted pool below, since it
    /// Procures a real, non-Volatile Potion.
    ///
    /// The four Draw Potions (Attack/Colorless/Power/Skill) are deliberately NOT in this list --
    /// see <see cref="VolatileDrawPool"/>, their own separate pool.
    /// </summary>
    private static IReadOnlyList<PotionModel> VolatileCommonPool() =>
    [
        ModelDb.Potion<VolatileBlockPotion>(),
        ModelDb.Potion<VolatileDexterityPotion>(),
        ModelDb.Potion<VolatileEnergyPotion>(),
        ModelDb.Potion<VolatileExplosiveAmpoule>(),
        ModelDb.Potion<VolatileFirePotion>(),
        ModelDb.Potion<VolatileFlexPotion>(),
        ModelDb.Potion<VolatilePoisonPotion>(),
        ModelDb.Potion<VolatileSpeedPotion>(),
        ModelDb.Potion<VolatileStrengthPotion>(),
        ModelDb.Potion<VolatileSwiftPotion>(),
        ModelDb.Potion<VolatileVulnerablePotion>(),
        ModelDb.Potion<VolatileWeakPotion>(),
    ];

    /// <summary>
    /// The four Volatile Potions that hand you a free card instead of a stat or damage. Their own
    /// pool, not part of <see cref="VolatileCommonPool"/> -- Spare Flask is the only way into it;
    /// ordinary random Common Brews, Alchemize, and shop offers never reach these four at all.
    /// </summary>
    private static IReadOnlyList<PotionModel> VolatileDrawPool() =>
    [
        ModelDb.Potion<VolatileAttackPotion>(),
        ModelDb.Potion<VolatileColorlessPotion>(),
        ModelDb.Potion<VolatilePowerPotion>(),
        ModelDb.Potion<VolatileSkillPotion>(),
    ];

    public PotionModel? RandomCombatPotion(Player player, PotionRarity? rarity = null)
    {
        if (rarity == PotionRarity.Common)
        {
            var commons = VolatileCommonPool();
            return commons.Count == 0 ? null : player.RunState.Rng.CombatPotionGeneration.NextItem(commons).ToMutable();
        }

        // Curated per design/alchemist.md: nothing whose value outlives the fight. The Stone is
        // non-Brewable by rule; Aurum Tincture is excluded because its Poison payload assumes it
        // was deliberately bought or found, not handed out for free by a random Brew. Poison
        // Ampoule is non-Brewable the same way the Stone is -- Stabilizing a Volatile Poison
        // Ampoule is its only source. Unstable Concoction is what Infuse Brews (see Belt.Infuse);
        // Residual Reagent is what a card leaves behind by calling Belt.LeaveResidualReagent
        // alongside it. Neither should ever be handed out any other way.
        var options = PotionFactory.GetPotionOptions(player, [])
            .Where(p => p is not PhilosophersStone and not AurumTincture and not PoisonAmpoule and not UnstableConcoction and not ResidualReagent)
            .Where(p => rarity == null || p.Rarity == rarity)
            .ToList();
        if (options.Count == 0) return null;
        return player.RunState.Rng.CombatPotionGeneration.NextItem(options).ToMutable();
    }

    public IReadOnlyList<PotionModel> CombatPotionOptions(Player player, int count, PotionRarity? rarity = null)
    {
        // Same curation as RandomCombatPotion, sampled WITHOUT replacement so the offer is three
        // different labels. Deterministic across clients: the pool order and the synced RNG are.
        IEnumerable<PotionModel> source = rarity == PotionRarity.Common
            ? VolatileCommonPool()
            : PotionFactory.GetPotionOptions(player, [])
                .Where(p => p is not PhilosophersStone and not AurumTincture and not PoisonAmpoule and not UnstableConcoction and not ResidualReagent);
        var pool = source.Where(p => rarity == null || p.Rarity == rarity).ToList();
        var picks = new List<PotionModel>();
        while (picks.Count < count && pool.Count > 0)
        {
            var pick = player.RunState.Rng.CombatPotionGeneration.NextItem(pool);
            pool.Remove(pick);
            picks.Add(pick.ToMutable());
        }
        return picks;
    }

    public PotionModel? RandomDrawPotion(Player player)
    {
        var options = VolatileDrawPool();
        return options.Count == 0 ? null : player.RunState.Rng.CombatPotionGeneration.NextItem(options).ToMutable();
    }

    public Task GainSlots(Player player, int count) => PlayerCmd.GainMaxPotionCount(count, player);

    public Task LoseSlots(Player player, int count) => PlayerCmd.LoseMaxPotionCount(count, player);

    public PotionModel? NamedPotion(BasePotion which) => which switch
    {
        BasePotion.Block => ModelDb.Potion<VolatileBlockPotion>().ToMutable(),
        BasePotion.ExplosiveAmpoule => ModelDb.Potion<VolatileExplosiveAmpoule>().ToMutable(),
        BasePotion.Energy => ModelDb.Potion<VolatileEnergyPotion>().ToMutable(),
        BasePotion.Vulnerable => ModelDb.Potion<VolatileVulnerablePotion>().ToMutable(),
        BasePotion.Weak => ModelDb.Potion<VolatileWeakPotion>().ToMutable(),
        BasePotion.Speed => ModelDb.Potion<VolatileSpeedPotion>().ToMutable(),
        BasePotion.Attack => ModelDb.Potion<VolatileAttackPotion>().ToMutable(),
        BasePotion.Fire => ModelDb.Potion<VolatileFirePotion>().ToMutable(),
        BasePotion.Strength => ModelDb.Potion<VolatileStrengthPotion>().ToMutable(),
        BasePotion.Poison => ModelDb.Potion<VolatilePoisonPotion>().ToMutable(),
        BasePotion.Dexterity => ModelDb.Potion<VolatileDexterityPotion>().ToMutable(),
        BasePotion.PoisonAmpoule => ModelDb.Potion<VolatilePoisonAmpoule>().ToMutable(),
        _ => null,
    };

    // -------------------------------------------------------------------------- choices
    /// <summary>
    /// Pick one of the offered Potions. Same multiplayer shape as Confirm: only the acting
    /// player's client shows UI (LocalContext.IsMe), the chosen index is broadcast via
    /// PlayerChoiceSynchronizer, and every other client resolves the same list entry.
    /// </summary>
    public Task<PotionModel?> ChoosePotionOption(PlayerChoiceContext ctx, Player player, IReadOnlyList<PotionModel> options) =>
        ChoosePotionFrom(ctx, player, options);

    /// <summary>
    /// Shared by ChoosePotionOption (offered Potions, e.g. Buy Ingredients) and ChoosePotion
    /// (held Potions, e.g. Distill, Stabilize, Pressure Burst) -- same PotionPickerPopup UI and
    /// the same multiplayer sync shape as Confirm: only the acting player's client shows it,
    /// the chosen index is broadcast via PlayerChoiceSynchronizer, and every other client
    /// resolves the same list entry.
    ///
    /// <paramref name="allowStop"/> offers a "Done" button alongside the Potions and skips the
    /// options.Count == 1 auto-pick shortcut, so the player can stop even with exactly one Potion
    /// left -- Grand Combustion's "distill any number" needs that; a mandatory single-Potion pick
    /// (Distill, Stabilize, Pressure Burst) does not, and keeps the shortcut.
    /// PlayerChoiceResult.FromIndex/AsIndexOrNull already represent "no index" over the network
    /// (see CardReward's skip-the-reward case in sts2.dll), so a stop needs no sentinel value here.
    /// </summary>
    private async Task<PotionModel?> ChoosePotionFrom(PlayerChoiceContext ctx, Player player, IReadOnlyList<PotionModel> options, LocString? header = null, bool allowStop = false)
    {
        if (options.Count == 0) return null;
        if (!allowStop && options.Count == 1) return options[0];

        var isLocal = LocalContext.IsMe(player) && RunManager.Instance.NetService.Type != NetGameType.Replay;
        var choiceId = RunManager.Instance.PlayerChoiceSynchronizer.ReserveChoiceId(player);

        await ctx.SignalPlayerChoiceBegun(PlayerChoiceOptions.None);
        try
        {
            int? picked;
            if (!isLocal)
            {
                var remote = await RunManager.Instance.PlayerChoiceSynchronizer.WaitForRemoteChoice(player, choiceId);
                picked = remote.AsIndexOrNull();
                if (picked.HasValue) picked = System.Math.Clamp(picked.Value, 0, options.Count - 1);
            }
            else
            {
                picked = await ShowPotionOptions(options, header, allowStop);
                RunManager.Instance.PlayerChoiceSynchronizer.SyncLocalChoice(player, choiceId, PlayerChoiceResult.FromIndex(picked));
            }
            return picked.HasValue ? options[picked.Value] : null;
        }
        finally
        {
            await ctx.SignalPlayerChoiceEnded();
        }
    }

    /// <summary>
    /// One popup, every option with its icon, click one -- PotionPickerPopup. No UI host
    /// (TestMode, headless) means no way to ask: take the first option, never hang.
    /// </summary>
    private static async Task<int?> ShowPotionOptions(IReadOnlyList<PotionModel> options, LocString? header = null, bool allowStop = false)
    {
        var picked = PotionPickerPopup.TryShow(options, header, allowStop);
        return picked == null ? 0 : await picked;
    }


    public Task<PotionModel?> ChoosePotion(PlayerChoiceContext ctx, Player player, IReadOnlyList<PotionModel> from, LocString? prompt = null, bool allowStop = false) =>
        ChoosePotionFrom(ctx, player, from, prompt, allowStop);

    public async Task<CardModel?> ChooseCard(PlayerChoiceContext ctx, Player player, IReadOnlyList<CardModel> from, CardModel? source, LocString? prompt = null)
    {
        // FromChooseACardScreen only supports 3 or fewer cards (throws otherwise -- confirmed via
        // a real in-game AggregateException on Salvage Reagents with a bigger hand). FromHand is
        // the documented "simple hand selection, no bespoke UI" primitive and has no such limit.
        //
        // source cannot be null: FromHand forwards it straight into NCombatRoom's hand-selection
        // UI (decompiled from sts2.dll), which dereferences it -- confirmed by a real
        // NullReferenceException on Transmute when this was passed as null!. Every current caller
        // has a real source card; the from.FirstOrDefault() fallback only exists so a future
        // relic/potion-driven caller with no natural source can't reintroduce that crash.
        // A default-constructed CardSelectorPrefs is a trap, not a "no preferences" value: its
        // Prompt is null (the hand UI formats it unconditionally -> NRE) and MaxSelect is 0 (no
        // card can ever be picked), so the selection never resolves and, in multiplayer, every
        // other client hangs in WaitForRemoteChoice. The real constructor gives min = max = 1:
        // pick one card and the selection completes; with exactly one candidate the game skips
        // the screen entirely, same as vanilla.
        var prefs = new CardSelectorPrefs(prompt ?? CardSelectorPrefs.ExhaustSelectionPrompt, 1);
        var chosen = await CardSelectCmd.FromHand(ctx, player, prefs, card => from.Contains(card),
            source ?? from.FirstOrDefault());
        return chosen.FirstOrDefault();
    }

    // -------------------------------------------------------------------------- hand and piles
    public IReadOnlyList<CardModel> Hand(Player player) =>
        PileType.Hand.GetPile(player).Cards.ToList();

    public IReadOnlyList<CardModel> DiscardPile(Player player) =>
        PileType.Discard.GetPile(player).Cards.ToList();

    public IReadOnlyList<CardModel> ExhaustPile(Player player) =>
        PileType.Exhaust.GetPile(player).Cards.ToList();

    public Task Exhaust(PlayerChoiceContext ctx, Player player, CardModel card) =>
        CardCmd.Exhaust(ctx, card);

    public Task DiscardCard(PlayerChoiceContext ctx, Player player, CardModel card) =>
        CardCmd.Discard(ctx, card);

    public async Task BottomOfDraw(PlayerChoiceContext ctx, Player player, CardModel card) =>
        await CardPileCmd.Add(card, PileType.Draw, CardPilePosition.Bottom);

    public async Task ReturnToHand(PlayerChoiceContext ctx, Player player, CardModel card) =>
        await CardPileCmd.Add(card, PileType.Hand);

    public CardModel? RandomCard(Player player, CardRarity? rarity = null, CardType? type = null)
    {
        var options = ModelDb.CardPool<AlchemistCardPool>()
            .GetUnlockedCards(player.UnlockState, player.RunState.CardMultiplayerConstraint)
            .Where(c => c.Rarity is not CardRarity.Basic and not CardRarity.Status and not CardRarity.Curse)
            .Where(c => rarity == null || c.Rarity == rarity)
            .Where(c => type == null || c.Type == type)
            .ToList();
        if (options.Count == 0) return null;

        // player.RunState.CreateCard makes a card scoped to the RUN (deck-level) -- fine for a
        // shop or reward screen, but this card is about to be dropped straight into Hand mid-combat.
        // Confirmed via sts2.dll: the base game's own combat-generation helpers
        // (CardFactory.GetDistinctForCombat/GetForCombat, used by e.g. Infernal Blade) create
        // through player.Creature.CombatState.CreateCard instead, which is what registers the card
        // with the active CombatState. A Run-scoped card added to Hand isn't in that registry, so it
        // plays fine visually but throws "must be added to a CombatState before playing it" the
        // moment it's actually played -- and that exception corrupts the action queue for the rest
        // of the combat, so every card played after it fails the same way. Confirmed in a real
        // session: Homunculus Assault created a Pressure Burst this way, which failed to play, and
        // every subsequent card that combat (including an ordinary drawn Auric Needle) failed too.
        var model = player.RunState.Rng.CombatTargets.NextItem(options);
        return player.Creature?.CombatState?.CreateCard(model, player);
    }

    public async Task CreateInHand(PlayerChoiceContext ctx, Player player, CardModel card, bool costsZeroThisTurn)
    {
        await CardPileCmd.AddGeneratedCardToCombat(card, PileType.Hand, player);
        if (costsZeroThisTurn) card.EnergyCost.SetThisTurnOrUntilPlayed(0);
    }

    public async Task CreateStatusInPile(PlayerChoiceContext ctx, Player player, CardModel canonicalCard, PileType pile)
    {
        // Same CombatState.CreateCard instantiation RandomCard uses above, and for the same reason:
        // a Run-scoped card isn't registered with the active combat and throws the moment it's
        // touched (Exhausted, played, etc.).
        var card = player.Creature?.CombatState?.CreateCard(canonicalCard, player);
        if (card != null) await CardPileCmd.AddGeneratedCardToCombat(card, pile, player);
    }

    public Task UpgradeForCombat(PlayerChoiceContext ctx, Player player, CardModel card)
    {
        if (card.IsUpgradable) CardCmd.Upgrade(card, CardPreviewStyle.None);
        return Task.CompletedTask;
    }

    public Task UpgradePermanently(Player player, CardModel card)
    {
        // Combat cards are clones; the deck original is what survives the fight.
        if (card.IsUpgradable) CardCmd.Upgrade(card, CardPreviewStyle.None);
        if (card.CloneOf is { IsUpgradable: true } original) CardCmd.Upgrade(original, CardPreviewStyle.None);
        return Task.CompletedTask;
    }
}
