// The UI is plain DOM over these three pure modules, so this is where UI bugs
// are actually catchable without a browser: what the card text renders as, and
// what the controls bar selects.

import { readFileSync } from "node:fs";
import { join } from "node:path";
import { describe, expect, it } from "vitest";
import { parseCards } from "../src/cs-cards.ts";
import type { CardVar } from "../src/cs-cards.ts";
import { balanceStats } from "../src/balance.ts";
import { danglingVars, renderText } from "../src/text.ts";
import { emptyFilters, facets, filterCards, toggle } from "../src/filters.ts";
import { locFor, readTable } from "../src/localization.ts";
import { CARD_FIELDS } from "../src/localization.ts";
import { allCardFiles, REPO_ROOT } from "../src/repo.ts";
import type { CardRecord } from "../src/api.ts";

const loc = readTable("cards");
const records: CardRecord[] = allCardFiles()
  .flatMap((f) => parseCards(f, readFileSync(join(REPO_ROOT, f), "utf8")))
  .map((c) => ({
    ...c,
    loc: locFor(loc, c.id, CARD_FIELDS),
    balance: balanceStats(c),
    art: { small: null, big: null },
  }));

const vars: CardVar[] = [
  {
    name: "Damage",
    kind: "DamageVar",
    value: 7,
    valueSpan: { start: 0, end: 1 },
    constName: null,
    decimal: true,
    upgrade: 3,
    upgradeSpan: { start: 0, end: 1 },
    upgradeDecimal: true,
  },
  {
    name: "Cards",
    kind: "CardsVar",
    value: 1,
    valueSpan: { start: 0, end: 1 },
    constName: null,
    decimal: false,
    upgrade: null,
    upgradeSpan: null,
    upgradeDecimal: true,
  },
];

describe("card text", () => {
  it("interpolates vars and strips the colour markup", () => {
    expect(renderText("Deal {Damage:diff()} damage.", vars, { upgraded: false })).toBe(
      "Deal 7 damage.",
    );
    expect(renderText("Gain {Block} [gold]Block[/gold].", [], { upgraded: false })).toBe(
      "Gain {Block} Block.",
    );
  });

  it("shows upgraded values when asked", () => {
    expect(renderText("Deal {Damage} damage, draw {Cards}.", vars, { upgraded: true })).toBe(
      "Deal 10 damage, draw 1.",
    );
  });

  it("flags placeholders no var satisfies", () => {
    expect(danglingVars("Deal {Damage}, gain {Block}.", vars)).toEqual(["Block"]);
    expect(danglingVars("Deal {Damage}.", vars)).toEqual([]);
  });

  it("leaves every shipped description fully resolvable", () => {
    // A dangling placeholder renders as a literal "{Foo}" in game. None of the
    // localized cards should have one.
    const broken = records
      .filter((c) => c.loc["description"] !== undefined)
      .map((c) => ({
        className: c.className,
        dangling: danglingVars(
          `${c.loc["description"]}${c.loc["upgrade.description"] ?? ""}`,
          c.vars,
        ),
      }))
      .filter((x) => x.dangling.length > 0);
    // The one legitimate hit is the Bandolier name collision: two classes share
    // HELLOSPIRE-BANDOLIER, so the Gunslinger's text reaches the Alchemist's
    // card, which declares no {Lead}. That is the content bug /api/warnings
    // reports, not a parser gap.
    expect(broken.map((b) => b.className)).toEqual(["Bandolier"]);
  });
});

describe("filters", () => {
  it("offers only facets that exist in the data", () => {
    const fx = facets(records, []);
    expect(fx.characters).toContain("Gunslinger");
    expect(fx.types).toContain("Attack");
    expect(fx.costs[0]).toBeLessThanOrEqual(1);
  });

  it("narrows by character, type and rarity together", () => {
    const f = emptyFilters();
    toggle(f.characters, "Gunslinger");
    toggle(f.types, "Attack");
    toggle(f.rarities, "Common");
    const out = filterCards(records, f);
    expect(out.length).toBeGreaterThan(0);
    for (const c of out) {
      expect(c.character).toBe("Gunslinger");
      expect(c.type).toBe("Attack");
      expect(c.rarity).toBe("Common");
    }
  });

  it("searches titles, class names and card text", () => {
    const f = emptyFilters();
    f.search = "snap shot";
    expect(filterCards(records, f).map((c) => c.className)).toContain("SnapShot");

    f.search = "FanTheHammer";
    expect(filterCards(records, f).map((c) => c.className)).toEqual(["FanTheHammer"]);
  });

  it("finds the content that still needs text", () => {
    const f = emptyFilters();
    f.missingText = true;
    const out = filterCards(records, f);
    expect(out.length).toBeGreaterThan(0);
    for (const c of out) expect(c.loc["title"]).toBeUndefined();
  });

  it("sorts damage descending, with no filter dropping cards silently", () => {
    const f = emptyFilters();
    f.sort = "damage";
    const out = filterCards(records, f);
    expect(out.length).toBe(records.length);
    for (let i = 1; i < out.length; i++) {
      expect(out[i - 1]!.balance.damage).toBeGreaterThanOrEqual(out[i]!.balance.damage);
    }
  });
});

describe("balance stats", () => {
  it("divides by cost, and declines to for a 0-cost card", () => {
    const strike = records.find((c) => c.className === "StrikeGunslinger");
    expect(strike).toBeDefined();
    expect(strike!.balance.damage).toBeGreaterThan(0);

    const free = records.find((c) => c.cost === 0 && c.balance.damage > 0);
    if (free) expect(free.balance.damagePerEnergy).toBeNull();
  });

  it("calls a card with no damage or block utility", () => {
    const util = records.find((c) => c.balance.utility)!;
    expect(util.balance.damage).toBe(0);
    expect(util.balance.block).toBe(0);
  });
});
