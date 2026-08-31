using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HelloSpire.HelloSpireCode.Alchemist.Potions;
using HelloSpire.HelloSpireCode.Characters;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Context;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Gold;
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
/// actual APIs (PotionCmd/PotionFactory for the belt, PlayerCmd for Gold, CreatureCmd.LoseMaxHp
/// for Render, CardSelectCmd for choices, CardCmd/CardPileCmd for the piles).
///
/// One remaining deliberate deviation, documented rather than hidden:
/// - ChoosePotion takes the first held Potion rather than asking. The game has no potion-select
///   UI to borrow; a real picker is the remaining polish item.
/// </summary>
public sealed class WiredLabBridge : ILabBridge
{
    // -------------------------------------------------------------------------- Gold
    public int Gold(Player player) => player.Gold;

    public Task GainGold(Player player, int amount) => PlayerCmd.GainGold(amount, player);

    /// <summary>
    /// Invest is a real Pay/Decline prompt, not a rubber stamp. The affordability check happens
    /// before any UI is created: too poor means an automatic Decline, never a popup asking about
    /// Gold the player doesn't have.
    /// </summary>
    public async Task<bool> OfferInvest(PlayerChoiceContext ctx, Player player, int cost)
    {
        cost = System.Math.Max(1, cost - AlchemistHooks.InvestDiscount(LabContext.From(player)));
        if (player.Gold < cost) return false;

        if (!await Confirm(ctx, player, "HELLOSPIRE-ALCHEMIST_INVEST_PROMPT.header", "HELLOSPIRE-ALCHEMIST_INVEST_PROMPT.body", cost))
            return false;

        await PlayerCmd.LoseGold(cost, player, GoldLossType.Spent);
        return true;
    }

    // -------------------------------------------------------------------------- Max HP
    public async Task<bool> OfferRender(PlayerChoiceContext ctx, Player player, int cost)
    {
        if (player.Creature == null || player.Creature.MaxHp - cost <= 0) return false;

        if (!await Confirm(ctx, player, "HELLOSPIRE-ALCHEMIST_RENDER_PROMPT.header", "HELLOSPIRE-ALCHEMIST_RENDER_PROMPT.body", cost))
            return false;

        await CreatureCmd.LoseMaxHp(ctx, player.Creature, cost, isFromCard: true);
        return true;
    }

    /// <summary>
    /// The Pay/Decline primitive shared by Invest and Render.
    ///
    /// There is no card- or combat-scoped confirmation dialog in the base game — Invest/Render
    /// have no vanilla precedent at all. What does exist, and is what every menu-level confirmation
    /// in the base game (reset settings, delete profile, abandon run, disconnect...) is built on,
    /// is <see cref="NGenericPopup"/> + <see cref="NModalContainer"/>: create the popup, add it to
    /// the single global modal layer, await its Yes/No result. This is the exact call sequence
    /// decompiled from NResetGameplayButton.ResetSettingsAfterConfirmation in sts2.dll. The Yes/No
    /// button labels reuse the base game's own "Confirm"/"Cancel" loc keys (main_menu_ui,
    /// GENERIC_POPUP.confirm/cancel) rather than minting new ones.
    ///
    /// Multiplayer: a card's OnPlay runs on every connected client to keep game state
    /// deterministic, not just the client of the player who played it (confirmed via a real
    /// in-game report of the Invest prompt popping up for every player at once). The fix, mirrored
    /// exactly from CardSelectCmd.FromHand (decompiled from sts2.dll): only the player's own client
    /// shows the popup (gated by LocalContext.IsMe, the same check FromHand uses), and the answer
    /// is broadcast to every other client via PlayerChoiceSynchronizer so their simulation reaches
    /// the same Gold/Max-HP state without ever showing them a popup of their own. ReserveChoiceId
    /// is called unconditionally, before branching, because it also has to stay in lock-step across
    /// clients -- exactly how FromHand calls it.
    /// </summary>
    private static async Task<bool> Confirm(PlayerChoiceContext ctx, Player player, string headerKey, string bodyKey, int cost)
    {
        var isLocal = LocalContext.IsMe(player) && RunManager.Instance.NetService.Type != NetGameType.Replay;
        var choiceId = RunManager.Instance.PlayerChoiceSynchronizer.ReserveChoiceId(player);

        await ctx.SignalPlayerChoiceBegun(PlayerChoiceOptions.None);
        try
        {
            if (!isLocal)
            {
                var remote = await RunManager.Instance.PlayerChoiceSynchronizer.WaitForRemoteChoice(player, choiceId);
                return remote.AsIndex() == 1;
            }

            var answer = await ShowPopup(headerKey, bodyKey, cost);
            RunManager.Instance.PlayerChoiceSynchronizer.SyncLocalChoice(player, choiceId, PlayerChoiceResult.FromIndex(answer ? 1 : 0));
            return answer;
        }
        finally
        {
            await ctx.SignalPlayerChoiceEnded();
        }
    }

    /// <summary>The actual local popup, split out so Confirm can gate it behind LocalContext.IsMe.</summary>
    private static async Task<bool> ShowPopup(string headerKey, string bodyKey, int cost)
    {
        // No modal layer, or one is already up: there is no safe way to ask, so Decline. This can
        // only make the bridge more conservative than intended, never less — it can never cause an
        // unasked-for Gold/Max HP spend.
        if (NModalContainer.Instance == null || NModalContainer.Instance.OpenModal != null)
            return false;

        var popup = NGenericPopup.Create();
        if (popup == null) return false; // TestMode, or the popup scene failed to load.

        var header = new LocString("cards", headerKey);
        var body = new LocString("cards", bodyKey);
        body.Add("Cost", (decimal)cost);
        var yes = new LocString("main_menu_ui", "GENERIC_POPUP.confirm");
        var no = new LocString("main_menu_ui", "GENERIC_POPUP.cancel");

        NModalContainer.Instance.Add(popup);
        return await popup.WaitForConfirmation(body, header, no, yes);
    }

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
    /// </summary>
    private static IReadOnlyList<PotionModel> VolatileCommonPool() =>
    [
        ModelDb.Potion<VolatileAttackPotion>(),
        ModelDb.Potion<VolatileBlockPotion>(),
        ModelDb.Potion<VolatileColorlessPotion>(),
        ModelDb.Potion<VolatileDexterityPotion>(),
        ModelDb.Potion<VolatileEnergyPotion>(),
        ModelDb.Potion<VolatileExplosiveAmpoule>(),
        ModelDb.Potion<VolatileFirePotion>(),
        ModelDb.Potion<VolatileFlexPotion>(),
        ModelDb.Potion<VolatilePowerPotion>(),
        ModelDb.Potion<VolatileSkillPotion>(),
        ModelDb.Potion<VolatileSpeedPotion>(),
        ModelDb.Potion<VolatileStrengthPotion>(),
        ModelDb.Potion<VolatileSwiftPotion>(),
        ModelDb.Potion<VolatileVulnerablePotion>(),
        ModelDb.Potion<VolatileWeakPotion>(),
    ];

    public PotionModel? RandomCombatPotion(Player player, PotionRarity? rarity = null)
    {
        if (rarity == PotionRarity.Common)
        {
            var commons = VolatileCommonPool();
            return commons.Count == 0 ? null : player.RunState.Rng.CombatPotionGeneration.NextItem(commons).ToMutable();
        }

        // Curated per design/alchemist.md: nothing whose value outlives the fight. The Stone is
        // non-Brewable by rule; Aurum Tincture makes real Gold.
        var options = PotionFactory.GetPotionOptions(player, [])
            .Where(p => p is not PhilosophersStone and not AurumTincture)
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
            : PotionFactory.GetPotionOptions(player, []).Where(p => p is not PhilosophersStone and not AurumTincture);
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

    public Task GainSlots(Player player, int count) => PlayerCmd.GainMaxPotionCount(count, player);

    public Task LoseSlots(Player player, int count) => PlayerCmd.LoseMaxPotionCount(count, player);

    public PotionModel? NamedPotion(BasePotion which) => which switch
    {
        BasePotion.Block => ModelDb.Potion<VolatileBlockPotion>().ToMutable(),
        BasePotion.ExplosiveAmpoule => ModelDb.Potion<VolatileExplosiveAmpoule>().ToMutable(),
        BasePotion.Energy => ModelDb.Potion<VolatileEnergyPotion>().ToMutable(),
        _ => null,
    };

    // -------------------------------------------------------------------------- choices
    /// <summary>
    /// Pick one of the offered Potions. Same multiplayer shape as Confirm: only the acting
    /// player's client shows UI (LocalContext.IsMe), the chosen index is broadcast via
    /// PlayerChoiceSynchronizer, and every other client resolves the same list entry.
    /// </summary>
    public async Task<PotionModel?> ChoosePotionOption(PlayerChoiceContext ctx, Player player, IReadOnlyList<PotionModel> options)
    {
        if (options.Count == 0) return null;
        if (options.Count == 1) return options[0];

        var isLocal = LocalContext.IsMe(player) && RunManager.Instance.NetService.Type != NetGameType.Replay;
        var choiceId = RunManager.Instance.PlayerChoiceSynchronizer.ReserveChoiceId(player);

        await ctx.SignalPlayerChoiceBegun(PlayerChoiceOptions.None);
        try
        {
            int picked;
            if (!isLocal)
            {
                var remote = await RunManager.Instance.PlayerChoiceSynchronizer.WaitForRemoteChoice(player, choiceId);
                picked = System.Math.Clamp(remote.AsIndex(), 0, options.Count - 1);
            }
            else
            {
                picked = await ShowPotionOptions(options);
                RunManager.Instance.PlayerChoiceSynchronizer.SyncLocalChoice(player, choiceId, PlayerChoiceResult.FromIndex(picked));
            }
            return options[picked];
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
    private static async Task<int> ShowPotionOptions(IReadOnlyList<PotionModel> options)
    {
        var picked = PotionPickerPopup.TryShow(options);
        return picked == null ? 0 : await picked;
    }


    public Task<PotionModel?> ChoosePotion(PlayerChoiceContext ctx, Player player, IReadOnlyList<PotionModel> from) =>
        Task.FromResult<PotionModel?>(from.Count > 0 ? from[0] : null);

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

    public Task Exhaust(PlayerChoiceContext ctx, Player player, CardModel card) =>
        CardCmd.Exhaust(ctx, card);

    public Task DiscardCard(PlayerChoiceContext ctx, Player player, CardModel card) =>
        CardCmd.Discard(ctx, card);

    public async Task BottomOfDraw(PlayerChoiceContext ctx, Player player, CardModel card) =>
        await CardPileCmd.Add(card, PileType.Draw, CardPilePosition.Bottom);

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
