import { readFileSync } from "node:fs";
import { describe, expect, it } from "vitest";
import { cardEdits, parseCards } from "../src/cs-cards.ts";
import { relicEdits, parseRelics } from "../src/cs-relics.ts";
import { applyEdits } from "../src/edits.ts";
import { classToEntry, classToId } from "../src/ids.ts";
import { allCardFiles, allRelicFiles, readAllCards, readCardBases, REPO_ROOT } from "../src/repo.ts";
import { join } from "node:path";

const cards = readAllCards();
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

  it("has no localized card without a class behind it", () => {
    // The reverse of the check above. A retired card that leaves its strings in
    // cards.json is dead weight the compiler never sees: Mend was cut in the
    // Paladin rework and its two keys sat there for months.
    const loc = JSON.parse(
      readFileSync(join(REPO_ROOT, "HelloSpire/localization/eng/cards.json"), "utf8"),
    ) as Record<string, string>;
    const ids = new Set(cards.map((c) => c.id));
    const orphans = Object.keys(loc)
      .filter((k) => k.endsWith(".title"))
      .map((k) => k.slice(0, -".title".length))
      .filter((id) => !ids.has(id));
    expect(orphans).toEqual([]);
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

  it("gives a var subclass the name of the base var it extends", () => {
    // SpiritHealVar extends HealVar and hands the amount to its base, so the
    // card indexes it as DynamicVars.Heal — not "SpiritHeal".
    const lay = cards.find((c) => c.className === "LayOnHands")!;
    expect(lay).toBeDefined();
    expect(lay.vars.map((v) => v.name)).toContain("Heal");
  });

  it("takes a var name only from a leading string literal", () => {
    // `new SpiritHealVar(5m, "Spirit")` names no var: the string points at a
    // sibling var whose value is gained as Spirit first. Reading it as a name
    // gave the card two vars called "Spirit" and made an unrelated save write
    // the wrong number into the heal amount.
    const blessed = cards.find((c) => c.className === "BlessedRecovery")!;
    expect(blessed).toBeDefined();
    expect(blessed.vars.map((v) => v.name).sort()).toEqual(["Heal", "Spirit"]);
    expect(blessed.vars.find((v) => v.name === "Heal")!.value).toBe(5);
    expect(blessed.vars.find((v) => v.name === "Spirit")!.value).toBe(3);
  });

  it("reads a card built on an intermediate base", () => {
    // SealCard(int cost, CardRarity rarity, decimal amount) hardcodes
    // CardType.Skill and TargetType.Self and forwards the rest. All eight Seals
    // used to be dropped on the floor here: parseCtorArgs found no CardType. in
    // their argument list and gave up, so they were invisible to the editor and
    // showed up only as localized strings with no card behind them.
    const seal = cards.find((c) => c.className === "SealOfTheCrusader")!;
    expect(seal).toBeDefined();
    expect(seal.via).toBe("SealCard");
    expect(seal.character).toBe("Paladin");
    expect(seal.cost).toBe(1);
    expect(seal.rarity).toBe("Rare");
    // Inherited from the base, so reported but not editable on this card.
    expect(seal.type).toBe("Skill");
    expect(seal.typeSpan).toBeNull();
    expect(seal.target).toBe("Self");
    expect(seal.targetSpan).toBeNull();
    // The base declares Amount; its value is this card's third argument.
    const amount = seal.vars.find((v) => v.name === "Amount")!;
    expect(amount).toBeDefined();
    expect(amount.value).toBe(4);
    expect(amount.upgrade).toBe(1);
    expect(amount.upgradeSpan).toBeNull();
  });

  it("finds every Seal, not just the ones with a plain constructor", () => {
    const seals = cards.filter((c) => c.via === "SealCard").map((c) => c.className);
    expect(seals).toHaveLength(8);
    expect(seals).toContain("SealOfLight");
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
    const bases = readCardBases();
    for (const f of allCardFiles()) {
      const src = readFileSync(join(REPO_ROOT, f), "utf8");
      for (const c of parseCards(f, src, bases)) {
        // A null span means the value is inherited from an intermediate base
        // and is not written in this file — there is nothing here to point at.
        const at = (s: { start: number; end: number } | null) =>
          s === null ? null : src.slice(s.start, s.end);
        if (c.costSpan) expect(at(c.costSpan)).toBe(String(c.cost));
        if (c.typeSpan) expect(at(c.typeSpan)).toBe(c.type);
        if (c.raritySpan) expect(at(c.raritySpan)).toBe(c.rarity);
        if (c.targetSpan) expect(at(c.targetSpan)).toBe(c.target);
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
    const bases = readCardBases();
    for (const f of allCardFiles()) {
      const src = readFileSync(join(REPO_ROOT, f), "utf8");
      const parsed = parseCards(f, src, bases);
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

  it("rewrites what an inherited card does own, and refuses what it does not", () => {
    const seal = cards.find((c) => c.className === "SealOfTheCrusader")!;
    const src = readFileSync(join(REPO_ROOT, seal.file), "utf8");

    // Cost and rarity are this card's own arguments.
    const out = applyEdits(src, cardEdits(seal, { cost: 2, rarity: "Uncommon" }));
    const after = parseCards(seal.file, out, readCardBases()).find(
      (c) => c.className === "SealOfTheCrusader",
    )!;
    expect(after.cost).toBe(2);
    expect(after.rarity).toBe("Uncommon");

    // Type and target belong to SealCard, and the error says where to go.
    expect(() => cardEdits(seal, { type: "Attack" })).toThrow(/inherits its type from SealCard/);
    expect(() => cardEdits(seal, { target: "AnyEnemy" })).toThrow(/SealCard/);
    expect(() => cardEdits(seal, { upgrades: { Amount: 2 } })).toThrow(/SealCard/);

    // Echoing an inherited value back is not a change, so it is not an error.
    expect(cardEdits(seal, { type: "Skill", upgrades: { Amount: 1 } })).toEqual([]);
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
