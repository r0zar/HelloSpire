// Reading (and writing back) the card set that lives in HelloSpireCode/**/Cards.
//
// A HelloSpire card is a C# class, not a data file: its behaviour is real code
// in OnPlay. What a designer balances, though, is a small and very regular
// surface — the four constructor arguments, the base value of each declared
// DynamicVar, and the per-var upgrade deltas in OnUpgrade. All 263 cards in
// the mod are written in the same shape:
//
//   /// <summary>Fire 1. Draw 1 card.</summary>
//   public sealed class SnapShot() : GunslingerCard(1, CardType.Attack, CardRarity.Common, TargetType.AnyEnemy)
//   {
//       protected override IEnumerable<DynamicVar> CanonicalVars => [new CardsVar(1), new DynamicVar("Deadeye", 0m)];
//       ...
//       protected override void OnUpgrade() => DynamicVars["Deadeye"].UpgradeValueBy(2m);
//   }
//
// This module locates exactly those literals and records their byte ranges.
// Writing replaces the ranges and nothing else, so OnPlay bodies, hover tips,
// comments and formatting survive a save untouched.

import { classToId, classToSlug } from "./ids.ts";
import type { Edit, Span } from "./edits.ts";

export interface CardVar {
  /** How the card indexes it: DynamicVars["Bonus"] → "Bonus". */
  name: string;
  /** Declared constructor, e.g. "DamageVar", "PowerVar<WeakPower>". */
  kind: string;
  /** Base value at level 0. */
  value: number;
  valueSpan: Span;
  /** Set when the value came from a named constant rather than a literal —
   *  the span then points at the constant's own initializer. */
  constName: string | null;
  /** True when the source wrote a decimal literal (6m) rather than an int (6). */
  decimal: boolean;
  /** Amount OnUpgrade adds, or null when the var does not upgrade. */
  upgrade: number | null;
  upgradeSpan: Span | null;
  upgradeDecimal: boolean;
}

export interface ParsedCard {
  className: string;
  /** HELLOSPIRE-SNAP_SHOT */
  id: string;
  /** snap_shot — the art filename stem. */
  slug: string;
  /** Repo-relative source file. */
  file: string;
  /** Base class as written, e.g. GunslingerCard. */
  base: string;
  /** Gunslinger | Alchemist | Paladin, from the base class. */
  character: string;
  /** First line of the /// <summary> above the class, plain text. */
  summary: string;
  line: number;

  // Each span is null when the value is not written in this class: a card built
  // on an intermediate base (SealCard) inherits its type and target from that
  // base, so there is nothing here to rewrite. Same idiom as ParsedRelic.rarity.
  cost: number;
  costSpan: Span | null;
  type: string;
  typeSpan: Span | null;
  rarity: string;
  raritySpan: Span | null;
  target: string;
  targetSpan: Span | null;

  /** The intermediate base this card's constants come from, when it has one. */
  via: string | null;

  /** EnergyCost.UpgradeBy delta in OnUpgrade (e.g. -1), or null when cost doesn't upgrade. */
  costUpgrade: number | null;

  /** "multiplayer" | "singleplayer" | "any" — from the MultiplayerConstraint override. */
  mode: string;

  vars: CardVar[];
}

const DECL =
  /(?:public|internal)\s+sealed\s+class\s+(\w+)\(\)\s*:\s*(?:[\w.]*\.)?(\w*Card)\(([^)]*)\)/g;

const CHARACTERS = ["Gunslinger", "Alchemist", "Paladin"] as const;

/**
 * Intermediate card bases.
 *
 * Most cards name a character base directly and pass all four constructor
 * arguments, which is the shape this module was written for. The Paladin's nine
 * Seals do not: they extend `SealCard(int cost, CardRarity rarity, decimal
 * amount)`, which hardcodes `CardType.Skill` and `TargetType.Self` and forwards
 * the rest. Before this was understood, `parseCtorArgs` found no `CardType.` in
 * `SealCard(1, CardRarity.Common, 2m)` and returned undefined -- so eight
 * shipped cards were dropped from the editor silently, and showed up only as
 * "localized strings with no card behind them".
 *
 * A base records, for each of the four fields, either a constant it bakes in or
 * the position in the subclass's own argument list that supplies it.
 */
export interface CardBase {
  name: string;
  /** The character base it ultimately extends, e.g. "PaladinCard". */
  base: string;
  cost: Slot;
  type: Slot;
  rarity: Slot;
  target: Slot;
  /** Vars the base declares for its subclasses, valued from a forwarded arg. */
  vars: { name: string; kind: string; argIndex: number }[];
  /** Upgrade deltas the base applies. Inherited, so not editable per card. */
  upgrades: Map<string, number>;
}

/** Where one constructor field's value comes from. */
export type Slot =
  | { from: "literal"; text: string }
  | { from: "arg"; index: number };

const BASE_DECL =
  /public\s+abstract\s+class\s+(\w+)\s*\(([^)]*)\)\s*:\s*(?:[\w.]*\.)?(\w*Card)\(([^)]*)\)/g;

/**
 * Find the intermediate card bases declared in one file, so `parseCards` can be
 * given them. A base whose own arguments are all forwarded parameters (the three
 * character bases) is skipped: those are the plain shape and need no help.
 */
export function parseCardBases(source: string): CardBase[] {
  const out: CardBase[] = [];

  for (const m of source.matchAll(BASE_DECL)) {
    const [, name, paramText, base, argText] = m;
    if (!name || !base || paramText === undefined || argText === undefined) continue;

    const params = splitArgs(paramText).map((p) => p.trim().split(/\s+/).pop() ?? "");
    const args = splitArgs(argText).map((a) => a.trim());
    if (args.length < 4) continue;

    const slot = (raw: string): Slot => {
      const i = params.indexOf(raw);
      return i === -1 ? { from: "literal", text: raw } : { from: "arg", index: i };
    };
    const cost = slot(args[0]!);
    const type = slot(args[1]!);
    const rarity = slot(args[2]!);
    const target = slot(args[3]!);

    // All four forwarded: an ordinary character base, nothing to record.
    if ([cost, type, rarity, target].every((x) => x.from === "arg")) continue;

    const body = source.slice(m.index);
    out.push({
      name,
      base,
      cost,
      type,
      rarity,
      target,
      vars: parseBaseVars(body, params),
      upgrades: new Map([...parseUpgrades(body, 0)].map(([k, v]) => [k, v.value])),
    });
  }
  return out;
}

/** Vars an intermediate base declares whose value is a forwarded parameter. */
function parseBaseVars(
  body: string,
  params: string[],
): { name: string; kind: string; argIndex: number }[] {
  const out: { name: string; kind: string; argIndex: number }[] = [];
  const region = canonicalVarsRegion(body);
  if (!region) return out;

  for (const m of region.text.matchAll(NEW_VAR)) {
    const kind = m[1]!;
    const argsOpen = m.index + m[0].length - 1;
    const argsClose = matchBracket(region.text, argsOpen, "(", ")");
    if (argsClose === -1) continue;
    const args = region.text.slice(argsOpen + 1, argsClose);

    const named = /^\s*"([^"]+)"/.exec(args);
    const argIndex = splitArgs(args)
      .map((a) => params.indexOf(a.trim()))
      .find((i) => i !== -1);
    if (argIndex === undefined) continue;

    out.push({ name: named?.[1] ?? defaultVarName(kind), kind, argIndex });
  }
  return out;
}

/** Split an argument list on top-level commas, ignoring generics and strings. */
function splitArgs(text: string): string[] {
  const out: string[] = [];
  let depth = 0;
  let start = 0;
  for (let i = 0; i < text.length; i++) {
    const ch = text[i];
    if (ch === '"') {
      i = text.indexOf('"', i + 1);
      if (i === -1) break;
    } else if (ch === "(" || ch === "[" || ch === "<") depth++;
    else if (ch === ")" || ch === "]" || ch === ">") depth--;
    else if (ch === "," && depth === 0) {
      out.push(text.slice(start, i));
      start = i + 1;
    }
  }
  out.push(text.slice(start));
  return out.filter((a) => a.trim() !== "");
}

/** Numeric literal, with the C# decimal suffix when present. */
const NUMBER = /(-?\d+(?:\.\d+)?)(m?)/;

/**
 * Parse every card class in one source file.
 *
 * `file` is only carried through onto the results — parsing reads `source`.
 */
export function parseCards(file: string, source: string, bases: CardBase[] = []): ParsedCard[] {
  const decls = [...source.matchAll(DECL)];
  const cards: ParsedCard[] = [];

  for (const [i, m] of decls.entries()) {
    const [full, className, base, argText] = m;
    if (!className || !base || argText === undefined) continue;
    const declStart = m.index;
    const argsStart = declStart + full.lastIndexOf(argText);

    const via = bases.find((b) => b.name === base) ?? null;
    const ctor = via
      ? resolveViaBase(via, argText, argsStart)
      : parseCtorArgs(argText, argsStart);
    if (!ctor) continue;

    // The class body runs to the next declaration, or to end of file.
    const bodyEnd = i + 1 < decls.length ? decls[i + 1]!.index : source.length;
    const body = source.slice(declStart, bodyEnd);

    cards.push({
      className,
      id: classToId(className),
      slug: classToSlug(className),
      file,
      base,
      character:
        CHARACTERS.find((c) => base.startsWith(c)) ??
        (via ? (CHARACTERS.find((c) => via.base.startsWith(c)) ?? "Unknown") : "Unknown"),
      summary: summaryAbove(source, declStart),
      line: source.slice(0, declStart).split("\n").length,
      ...ctor,
      via: via?.name ?? null,
      costUpgrade: parseCostUpgrade(body),
      mode: parseMode(body),
      vars: via
        ? inheritedVars(via, argText, argsStart)
        : parseVars(body, declStart),
    });
  }
  return cards;
}

/** EnergyCost.UpgradeBy(n) in OnUpgrade — how the cost changes when upgraded. */
function parseCostUpgrade(body: string): number | null {
  const m = /EnergyCost\.UpgradeBy\((-?\d+)\)/.exec(body);
  return m?.[1] !== undefined ? Number(m[1]) : null;
}

/** The MultiplayerConstraint override, when the class body has one. */
function parseMode(body: string): string {
  const m = /CardMultiplayerConstraint\.(\w+)/.exec(body);
  if (m?.[1] === "MultiplayerOnly") return "multiplayer";
  if (m?.[1] === "SingleplayerOnly") return "singleplayer";
  return "any";
}

/** The four positional constructor arguments, with their spans. */
function parseCtorArgs(
  argText: string,
  base: number,
):
  | Pick<
      ParsedCard,
      "cost" | "costSpan" | "type" | "typeSpan" | "rarity" | "raritySpan" | "target" | "targetSpan"
    >
  | undefined {
  const cost = spanOf(argText, /(-?\d+)/, base);
  const type = spanOf(argText, /CardType\.(\w+)/, base);
  const rarity = spanOf(argText, /CardRarity\.(\w+)/, base);
  const target = spanOf(argText, /TargetType\.(\w+)/, base);
  if (!cost || !type || !rarity || !target) return undefined;

  return {
    cost: Number(cost.text),
    costSpan: cost.span,
    type: type.text,
    typeSpan: type.span,
    rarity: rarity.text,
    raritySpan: rarity.span,
    target: target.text,
    targetSpan: target.span,
  };
}

/**
 * The four constructor fields for a card built on an intermediate base: each is
 * either a constant the base bakes in (no span — nothing in this file to edit)
 * or an argument this class passes, resolved positionally.
 */
function resolveViaBase(
  via: CardBase,
  argText: string,
  base: number,
):
  | Pick<
      ParsedCard,
      "cost" | "costSpan" | "type" | "typeSpan" | "rarity" | "raritySpan" | "target" | "targetSpan"
    >
  | undefined {
  const args = argSpans(argText, base);

  const read = (slot: Slot, strip: RegExp): { text: string; span: Span | null } | undefined => {
    if (slot.from === "literal") return { text: slot.text.replace(strip, ""), span: null };
    const a = args[slot.index];
    if (!a) return undefined;
    const m = strip.exec(a.text);
    const text = a.text.replace(strip, "");
    const offset = m ? m[0].length : 0;
    return { text, span: { start: a.span.start + offset, end: a.span.start + offset + text.length } };
  };

  const cost = read(via.cost, /^/);
  const type = read(via.type, /^CardType\./);
  const rarity = read(via.rarity, /^CardRarity\./);
  const target = read(via.target, /^TargetType\./);
  if (!cost || !type || !rarity || !target) return undefined;
  if (!Number.isInteger(Number(cost.text))) return undefined;

  return {
    cost: Number(cost.text),
    costSpan: cost.span,
    type: type.text,
    typeSpan: type.span,
    rarity: rarity.text,
    raritySpan: rarity.span,
    target: target.text,
    targetSpan: target.span,
  };
}

/**
 * The vars an intermediate base declares on this card's behalf. The value lives
 * in this class's argument list, so it stays editable; the upgrade delta lives
 * in the base's OnUpgrade and is shared by every subclass, so it is reported
 * with a null span and `cardEdits` refuses to change it here.
 */
function inheritedVars(via: CardBase, argText: string, base: number): CardVar[] {
  const args = argSpans(argText, base);
  const out: CardVar[] = [];

  for (const v of via.vars) {
    const a = args[v.argIndex];
    if (!a) continue;
    const num = NUMBER.exec(blankStrings(a.text));
    if (!num) continue;

    const start = a.span.start + num.index;
    out.push({
      name: v.name,
      kind: v.kind,
      value: Number(num[1]),
      valueSpan: { start, end: start + num[0].length },
      constName: null,
      decimal: num[2] === "m",
      upgrade: via.upgrades.get(v.name) ?? null,
      upgradeSpan: null,
      upgradeDecimal: true,
    });
  }
  return out;
}

/** Each top-level argument with its absolute span, trimmed of surrounding space. */
function argSpans(argText: string, base: number): { text: string; span: Span }[] {
  const out: { text: string; span: Span }[] = [];
  let at = 0;
  for (const raw of splitArgs(argText)) {
    const idx = argText.indexOf(raw, at);
    at = idx + raw.length;
    const lead = raw.length - raw.trimStart().length;
    const text = raw.trim();
    out.push({
      text,
      span: { start: base + idx + lead, end: base + idx + lead + text.length },
    });
  }
  return out;
}

/** Match `re` in `text` and return capture 1 plus its absolute span. */
function spanOf(
  text: string,
  re: RegExp,
  base: number,
): { text: string; span: Span } | undefined {
  const m = re.exec(text);
  if (!m || m[1] === undefined) return undefined;
  const start = base + m.index + m[0].indexOf(m[1]);
  return { text: m[1], span: { start, end: start + m[1].length } };
}

/**
 * The vars a card declares, joined to the upgrade deltas that mention them.
 * `body` is the class body; `base` is its absolute offset in the file.
 */
function parseVars(body: string, base: number): CardVar[] {
  const vars = parseCanonicalVars(body, base);
  const upgrades = parseUpgrades(body, base);
  for (const v of vars) {
    const up = upgrades.get(v.name);
    if (!up) continue;
    v.upgrade = up.value;
    v.upgradeSpan = up.span;
    v.upgradeDecimal = up.decimal;
  }
  return vars;
}

const NEW_VAR = /new\s+(\w+Var(?:<\s*\w+\s*>)?)\s*\(/g;

/** The `[...]` collection expression after CanonicalVars, with its offset. */
function canonicalVarsRegion(body: string): { text: string; open: number } | null {
  const at = body.indexOf("CanonicalVars");
  if (at === -1) return null;
  const open = body.indexOf("[", at);
  if (open === -1) return null;
  const close = matchBracket(body, open, "[", "]");
  if (close === -1) return null;
  return { text: body.slice(open, close), open };
}

function parseCanonicalVars(body: string, base: number): CardVar[] {
  const found = canonicalVarsRegion(body);
  if (!found) return [];
  const { text: region, open } = found;
  const out: CardVar[] = [];

  for (const m of region.matchAll(NEW_VAR)) {
    const kind = m[1]!;
    const argsOpen = m.index + m[0].length - 1;
    const argsClose = matchBracket(region, argsOpen, "(", ")");
    if (argsClose === -1) continue;
    const args = region.slice(argsOpen + 1, argsClose);

    // Usually the value is a literal in the argument list. One card writes it
    // as a named constant instead (`new DynamicVar("Threshold", ArmorThreshold)`);
    // resolving that keeps the var visible and editable rather than dropping it
    // silently, which would also make its {Threshold} placeholder look dangling.
    //
    // Searched with string literals blanked out, so a var whose NAME contains a
    // digit cannot have that digit mistaken for its value. Blanking preserves
    // offsets, so the recorded span still points at the real source text.
    const scannable = blankStrings(args);
    const num = NUMBER.exec(scannable);
    const literal = num
      ? {
          value: Number(num[1]),
          span: {
            start: base + open + argsOpen + 1 + num.index,
            end: base + open + argsOpen + 1 + num.index + num[0].length,
          },
          decimal: num[2] === "m",
          constName: null as string | null,
        }
      : resolveConst(args, body, base);
    if (!literal) continue;

    // A var is named by a string literal only in FIRST position — that is the
    // shape of every naming constructor in the set (`DynamicVar("Deadeye", 0m)`,
    // `BlockVar("Bonus", 3m, ...)`). A string anywhere else is an ordinary
    // argument: `SpiritHealVar(5m, "Spirit")` names no var, it points at a
    // sibling one. Taking any string literal used to read that card as
    // declaring a second "Spirit" var, which collided with its real one and
    // made a save write the wrong number into the wrong span.
    const named = /^\s*"([^"]+)"/.exec(args);
    out.push({
      name: named?.[1] ?? defaultVarName(kind),
      kind,
      value: literal.value,
      valueSpan: literal.span,
      constName: literal.constName,
      decimal: literal.decimal,
      upgrade: null,
      upgradeSpan: null,
      upgradeDecimal: true,
    });
  }
  return out;
}

/**
 * An argument that names a `const` declared in the same class: return the
 * constant's value and the span of ITS initializer, so an edit rewrites the
 * constant — the one place the number actually lives.
 */
function resolveConst(
  args: string,
  body: string,
  base: number,
): { value: number; span: Span; decimal: boolean; constName: string } | null {
  for (const m of args.matchAll(/(?:^|,)\s*([A-Za-z_]\w*)\s*(?=,|$)/g)) {
    const ident = m[1]!;
    const decl = new RegExp(`const\\s+\\w+\\s+${ident}\\s*=\\s*(-?\\d+(?:\\.\\d+)?)(m?)`).exec(body);
    if (!decl?.[1]) continue;
    const start = base + decl.index + decl[0].lastIndexOf(decl[1]);
    return {
      value: Number(decl[1]),
      span: { start, end: start + decl[1].length + decl[2]!.length },
      decimal: decl[2] === "m",
      constName: ident,
    };
  }
  return null;
}

/**
 * The var kinds that carry a name of their own, as the game declares them:
 * `DynamicVars.Damage`, `.Block`, `.Heal`, `.Cards`, `.Energy`. A subclass
 * inherits its base's name rather than coining one from its own class name,
 * so these are matched as a suffix.
 */
const BASE_VAR_NAMES = ["Damage", "Block", "Heal", "Cards", "Energy"] as const;

/**
 * `DamageVar` → "Damage"; `PowerVar<WeakPower>` → "WeakPower";
 * `SpiritHealVar` → "Heal".
 *
 * The last case is why this is a suffix match rather than `kind.replace(/Var$/,
 * "")`. SpiritHealVar extends HealVar and passes the amount straight to its
 * base, so the card indexes it as `DynamicVars.Heal` — stripping the suffix off
 * the derived class name instead produced "SpiritHeal", which matched nothing,
 * and every one of the Paladin's fourteen healing cards looked like it had a
 * dangling {Heal} placeholder.
 */
function defaultVarName(kind: string): string {
  const generic = /<\s*(\w+)\s*>/.exec(kind);
  if (generic?.[1]) return generic[1];

  const bare = kind.replace(/Var$/, "");
  return BASE_VAR_NAMES.find((n) => bare === n || bare.endsWith(n)) ?? bare;
}

/** Replace the contents of every string literal with spaces, keeping offsets. */
function blankStrings(text: string): string {
  return text.replace(/"[^"]*"/g, (s) => " ".repeat(s.length));
}

const UPGRADE =
  /DynamicVars(?:\.(\w+)|\[\s*"([^"]+)"\s*\])\s*\.\s*UpgradeValueBy\(\s*(-?\d+(?:\.\d+)?)(m?)/g;

/** var name → the delta OnUpgrade adds to it. */
function parseUpgrades(
  body: string,
  base: number,
): Map<string, { value: number; span: Span; decimal: boolean }> {
  const out = new Map<string, { value: number; span: Span; decimal: boolean }>();
  const at = body.indexOf("OnUpgrade");
  if (at === -1) return out;

  // Everything after OnUpgrade in the class body. Nothing else in these classes
  // calls UpgradeValueBy, so the region does not need a tighter end.
  const region = body.slice(at);
  for (const m of region.matchAll(UPGRADE)) {
    const name = m[1] ?? m[2];
    const digits = m[3]!;
    if (!name) continue;
    const start = base + at + m.index + m[0].lastIndexOf(digits);
    out.set(name, {
      value: Number(digits),
      span: { start, end: start + digits.length + m[4]!.length },
      decimal: m[4] === "m",
    });
  }
  return out;
}

/** Index of the bracket closing the one at `open`, or -1. */
function matchBracket(text: string, open: number, o: string, c: string): number {
  let depth = 0;
  for (let i = open; i < text.length; i++) {
    const ch = text[i];
    if (ch === '"') {
      i = text.indexOf('"', i + 1);
      if (i === -1) return -1;
      continue;
    }
    if (ch === o) depth++;
    else if (ch === c && --depth === 0) return i;
  }
  return -1;
}

/** Plain text of the /// <summary> block immediately above `declStart`. */
function summaryAbove(source: string, declStart: number): string {
  const before = source.slice(0, declStart).split("\n");
  const doc: string[] = [];
  for (let i = before.length - 2; i >= 0; i--) {
    const line = before[i]!.trim();
    if (line.startsWith("///")) doc.unshift(line.replace(/^\/{3}\s?/, ""));
    else if (line === "") continue;
    else break;
  }
  const text = doc.join(" ");
  const inner = /<summary>([\s\S]*?)<\/summary>/.exec(text);
  return (inner?.[1] ?? text)
    .replace(/<[^>]+>/g, "")
    .replace(/\s+/g, " ")
    .trim();
}

// ── Writing ─────────────────────────────────────────────────────────────────

/** The editable surface of a card, as the UI sends it back. */
export interface CardPatch {
  cost?: number;
  type?: string;
  rarity?: string;
  target?: string;
  /** var name → new base value. */
  values?: Record<string, number>;
  /** var name → new upgrade delta. Only vars that already upgrade. */
  upgrades?: Record<string, number>;
}

export const CARD_TYPES = ["Attack", "Skill", "Power", "Status", "Curse"] as const;
export const CARD_RARITIES = ["Starter", "Common", "Uncommon", "Rare", "Special"] as const;

/**
 * Turn a patch into source edits against the file `card` was parsed from.
 * Fields absent from the patch produce no edit, so a save only ever touches
 * what the designer actually changed.
 */
export function cardEdits(card: ParsedCard, patch: CardPatch): Edit[] {
  const edits: Edit[] = [];

  // Every field below is a no-op when the patch matches what the source already
  // says, and that check comes FIRST -- a card can legitimately report a value
  // it does not own (a constant inherited from SealCard), and echoing that value
  // straight back must not be an error.
  if (patch.cost !== undefined && patch.cost !== card.cost) {
    if (!Number.isInteger(patch.cost) || patch.cost < -1) {
      throw new Error(`cost must be an integer >= -1, got ${patch.cost}`);
    }
    edits.push({ ...requireSpan(card, card.costSpan, "cost"), text: String(patch.cost) });
  }
  if (patch.type !== undefined && patch.type !== card.type) {
    requireEnum("type", patch.type, CARD_TYPES);
    edits.push({ ...requireSpan(card, card.typeSpan, "type"), text: patch.type });
  }
  if (patch.rarity !== undefined && patch.rarity !== card.rarity) {
    requireEnum("rarity", patch.rarity, CARD_RARITIES);
    edits.push({ ...requireSpan(card, card.raritySpan, "rarity"), text: patch.rarity });
  }
  if (patch.target !== undefined && patch.target !== card.target) {
    if (!/^\w+$/.test(patch.target)) throw new Error(`bad target: ${patch.target}`);
    edits.push({ ...requireSpan(card, card.targetSpan, "target"), text: patch.target });
  }

  for (const [name, value] of Object.entries(patch.values ?? {})) {
    const v = card.vars.find((x) => x.name === name);
    if (!v) throw new Error(`${card.className} has no var "${name}"`);
    if (value === v.value) continue;
    edits.push({ ...v.valueSpan, text: numberLiteral(value, v.decimal) });
  }

  for (const [name, value] of Object.entries(patch.upgrades ?? {})) {
    const v = card.vars.find((x) => x.name === name);
    if (!v) throw new Error(`${card.className} has no var "${name}"`);
    if (value === v.upgrade) continue;
    if (!v.upgradeSpan) {
      throw new Error(
        card.via
          ? `${card.className}.${name} upgrades in ${card.via}, which every card on that base shares`
          : `${card.className}.${name} has no upgrade to change`,
      );
    }
    edits.push({ ...v.upgradeSpan, text: numberLiteral(value, v.upgradeDecimal) });
  }

  return edits;
}

/** A span that exists, or an error naming where the value actually lives. */
function requireSpan(card: ParsedCard, span: Span | null, field: string): Span {
  if (span) return span;
  throw new Error(
    `${card.className} inherits its ${field} from ${card.via ?? "its base"}; ` +
      `change it there, or give the card its own constructor argument`,
  );
}

function requireEnum(field: string, value: string, allowed: readonly string[]): void {
  if (!allowed.includes(value)) {
    throw new Error(`${field} must be one of ${allowed.join(", ")}, got ${value}`);
  }
}

/** Re-emit a number in the same style the source used for that literal. */
function numberLiteral(value: number, decimal: boolean): string {
  if (!Number.isFinite(value)) throw new Error(`not a finite number: ${value}`);
  const s = Number.isInteger(value) ? String(value) : String(Number(value.toFixed(4)));
  return decimal ? `${s}m` : s;
}
