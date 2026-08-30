// The English string table, HelloSpire/localization/eng/*.json.
//
// Flat key→string maps, keyed by content id: "HELLOSPIRE-SNAP_SHOT.title",
// ".description", and for cards ".upgrade.description" (only present when the
// upgraded card reads differently). Relics add ".flavor".
//
// Key ORDER in these files is meaningful to reviewers even though it is not to
// the game: the sets were written character by character, and a save that
// reshuffled them would turn a one-line balance change into an unreadable
// diff. So edits update in place and new keys append.

import { readFileSync, writeFileSync } from "node:fs";
import { join } from "node:path";
import { LOC_DIR } from "./repo.ts";

export type StringTable = Record<string, string>;

/** Which fields each content kind carries in the table. */
export const CARD_FIELDS = ["title", "description", "upgrade.description"] as const;
export const RELIC_FIELDS = ["title", "description", "flavor"] as const;

export type LocKind = "cards" | "relics";

export type CardLoc = Partial<Record<(typeof CARD_FIELDS)[number], string>>;
export type RelicLoc = Partial<Record<(typeof RELIC_FIELDS)[number], string>>;

function file(kind: LocKind): string {
  return join(LOC_DIR, `${kind}.json`);
}

export function readTable(kind: LocKind): StringTable {
  return JSON.parse(readFileSync(file(kind), "utf8")) as StringTable;
}

function writeTable(kind: LocKind, table: StringTable): void {
  writeFileSync(file(kind), JSON.stringify(table, null, 2) + "\n", "utf8");
}

/** The fields recorded for one content id. */
export function locFor(table: StringTable, id: string, fields: readonly string[]): StringTable {
  const out: StringTable = {};
  for (const f of fields) {
    const v = table[`${id}.${f}`];
    if (v !== undefined) out[f] = v;
  }
  return out;
}

/**
 * Apply a field patch for one id and persist.
 *
 * An empty string removes the key rather than storing "": a card with an empty
 * `upgrade.description` would render a blank line in game, whereas an absent
 * one correctly falls back to the base description.
 */
export function saveLoc(
  kind: LocKind,
  id: string,
  patch: StringTable,
  fields: readonly string[],
): StringTable {
  const table = readTable(kind);
  for (const [field, value] of Object.entries(patch)) {
    if (!fields.includes(field)) throw new Error(`unknown ${kind} field: ${field}`);
    const key = `${id}.${field}`;
    if (value === "") delete table[key];
    else table[key] = value;
  }
  writeTable(kind, table);
  return locFor(table, id, fields);
}
