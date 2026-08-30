import { readFileSync } from "node:fs";
import { describe, expect, it } from "vitest";
import { cardEdits, parseCards } from "../src/cs-cards.ts";
import { relicEdits, parseRelics } from "../src/cs-relics.ts";
import { applyEdits } from "../src/edits.ts";
import { classToEntry, classToId } from "../src/ids.ts";
import { allCardFiles, allRelicFiles, REPO_ROOT } from "../src/repo.ts";
import { join } from "node:path";

const cards = allCardFiles().flatMap((f) =>
  parseCards(f, readFileSync(join(REPO_ROOT, f), "utf8")),
);
const relics = allRelicFiles().flatMap((f) =>
  parseRelics(f, readFileSync(join(REPO_ROOT, f), "utf8")),
);

describe("id derivation", () => {
  it("matches the localization keys the game ships", () => {
    const loc = JSON.parse(
      readFileSync(join(REPO_ROOT, "HelloSpire/localization/eng/cards.json"), "utf8"),
    ) as Record<string, string>;
    const titles = new Set(
      Object.keys(loc)
        .filter((k) => k.endsWith(".title"))
        .map((k) => k.slice(0, -".title".length)),
    );
    const ids = new Set(cards.map((c) => c.id));
    // Every localized title belongs to a card we parsed. (The reverse does not
    // hold: the Alchemist set is scaffolded and not localized yet.)
    for (const t of titles) expect(ids.has(t), `${t} has no card class`).toBe(true);
  });

  it("splits PascalCase on every capital", () => {
    expect(classToEntry("SnapShot")).toBe("SNAP_SHOT");
    expect(classToEntry("Reload")).toBe("RELOAD");
    expect(classToId("FanTheHammer")).toBe("HELLOSPIRE-FAN_THE_HAMMER");
  });
});

describe("card parsing", () => {
  it("finds the whole card set", () => {
    expect(cards.length).toBeGreaterThan(250);
  });

  it("reads a known card exactly", () => {
    const snap = cards.find((c) => c.className === "SnapShot")!;
    expect(snap).toBeDefined();
    expect(snap.cost).toBe(1);
    expect(snap.type).toBe("Attack");
    expect(snap.rarity).toBe("Common");
    expect(snap.target).toBe("AnyEnemy");
    expect(snap.character).toBe("Gunslinger");
    expect(snap.vars.map((v) => v.name).sort()).toEqual(["Cards", "Deadeye"]);
    const deadeye = snap.vars.find((v) => v.name === "Deadeye")!;
    expect(deadeye.value).toBe(0);
    expect(deadeye.upgrade).toBe(2);
  });

  it("names vars the way the card indexes them", () => {
    for (const c of cards) {
      for (const v of c.vars) expect(v.name).toMatch(/^\w+$/);
    }
  });

  it("gives every card all four constructor arguments", () => {
    for (const c of cards) {
      expect(Number.isInteger(c.cost), `${c.className} cost`).toBe(true);
      expect(c.type, `${c.className} type`).toMatch(/^\w+$/);
      expect(c.rarity, `${c.className} rarity`).toMatch(/^\w+$/);
      expect(c.target, `${c.className} target`).toMatch(/^\w+$/);
    }
  });

  it("points every recorded span at the text it claims", () => {
    for (const f of allCardFiles()) {
      const src = readFileSync(join(REPO_ROOT, f), "utf8");
      for (const c of parseCards(f, src)) {
        expect(src.slice(c.costSpan.start, c.costSpan.end)).toBe(String(c.cost));
        expect(src.slice(c.typeSpan.start, c.typeSpan.end)).toBe(c.type);
        expect(src.slice(c.raritySpan.start, c.raritySpan.end)).toBe(c.rarity);
        expect(src.slice(c.targetSpan.start, c.targetSpan.end)).toBe(c.target);
        for (const v of c.vars) {
          expect(parseFloat(src.slice(v.valueSpan.start, v.valueSpan.end))).toBe(v.value);
          if (v.upgradeSpan) {
            expect(parseFloat(src.slice(v.upgradeSpan.start, v.upgradeSpan.end))).toBe(v.upgrade);
          }
        }
      }
    }
  });
});

describe("writing back", () => {
  it("is a no-op when nothing changed", () => {
    for (const f of allCardFiles()) {
      const src = readFileSync(join(REPO_ROOT, f), "utf8");
      const parsed = parseCards(f, src);
      const edits = parsed.flatMap((c) =>
        cardEdits(c, {
          cost: c.cost,
          type: c.type,
          rarity: c.rarity,
          target: c.target,
          values: Object.fromEntries(c.vars.map((v) => [v.name, v.value])),
          upgrades: Object.fromEntries(
            c.vars.filter((v) => v.upgrade !== null).map((v) => [v.name, v.upgrade!]),
          ),
        }),
      );
      expect(edits, `${f} should need no edits`).toEqual([]);
      expect(applyEdits(src, edits)).toBe(src);
    }
  });

  it("changes only the literal it targets", () => {
    const f = "HelloSpireCode/Gunslinger/Cards/Commons.cs";
    const src = readFileSync(join(REPO_ROOT, f), "utf8");
    const snap = parseCards(f, src).find((c) => c.className === "SnapShot")!;
    const out = applyEdits(src, cardEdits(snap, { cost: 2, values: { Deadeye: 3 } }));

    const after = parseCards(f, out).find((c) => c.className === "SnapShot")!;
    expect(after.cost).toBe(2);
    expect(after.vars.find((v) => v.name === "Deadeye")!.value).toBe(3);
    // Same file length ± the digits that moved, and every other card untouched.
    expect(parseCards(f, out).filter((c) => c.className !== "SnapShot")).toEqual(
      parseCards(f, src)
        .filter((c) => c.className !== "SnapShot")
        .map((c) => expect.objectContaining({ className: c.className, cost: c.cost })),
    );
  });

  it("preserves the decimal suffix style of each literal", () => {
    const f = "HelloSpireCode/Gunslinger/Cards/Commons.cs";
    const src = readFileSync(join(REPO_ROOT, f), "utf8");
    const snap = parseCards(f, src).find((c) => c.className === "SnapShot")!;
    // Cards is written `new CardsVar(1)` — a bare int; Deadeye is `0m`.
    const out = applyEdits(src, cardEdits(snap, { values: { Cards: 2, Deadeye: 5 } }));
    expect(out).toContain("new CardsVar(2)");
    expect(out).toContain('new DynamicVar("Deadeye", 5m)');
  });

  it("refuses a var the card does not declare", () => {
    const snap = cards.find((c) => c.className === "SnapShot")!;
    expect(() => cardEdits(snap, { values: { Nope: 1 } })).toThrow(/no var/);
  });

  it("refuses an unknown rarity", () => {
    const snap = cards.find((c) => c.className === "SnapShot")!;
    expect(() => cardEdits(snap, { rarity: "Mythic" })).toThrow(/rarity/);
  });
});

describe("relic parsing", () => {
  it("finds the relic set and its rarities", () => {
    expect(relics.length).toBeGreaterThan(10);
    const oldIron = relics.find((r) => r.className === "OldIron")!;
    expect(oldIron).toBeDefined();
    expect(oldIron.rarity).toBe("Starter");
    expect(oldIron.id).toBe("HELLOSPIRE-OLD_IRON");
  });

  it("round-trips with no edits", () => {
    for (const f of allRelicFiles()) {
      const src = readFileSync(join(REPO_ROOT, f), "utf8");
      const edits = parseRelics(f, src).flatMap((r) => relicEdits(r, { rarity: r.rarity }));
      expect(edits, `${f}`).toEqual([]);
    }
  });

  it("rewrites a rarity in place", () => {
    const f = relics.find((r) => r.className === "OldIron")!.file;
    const src = readFileSync(join(REPO_ROOT, f), "utf8");
    const oldIron = parseRelics(f, src).find((r) => r.className === "OldIron")!;
    const out = applyEdits(src, relicEdits(oldIron, { rarity: "Boss" }));
    expect(parseRelics(f, out).find((r) => r.className === "OldIron")!.rarity).toBe("Boss");
  });
});
