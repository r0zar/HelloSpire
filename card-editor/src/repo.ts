// Where the mod keeps its content, and the guards that keep the dev server
// writing only inside it.

import { existsSync, readdirSync, statSync } from "node:fs";
import { dirname, join, relative, resolve, sep } from "node:path";
import { fileURLToPath } from "node:url";

export const REPO_ROOT = resolve(dirname(fileURLToPath(import.meta.url)), "..", "..");

export const CODE_DIR = join(REPO_ROOT, "HelloSpireCode");
export const LOC_DIR = join(REPO_ROOT, "HelloSpire", "localization", "eng");
export const CARD_ART_DIR = join(REPO_ROOT, "HelloSpire", "images", "card_portraits");
export const RELIC_ART_DIR = join(REPO_ROOT, "HelloSpire", "images", "relics");

/** Card art ships at two sizes; the game picks by context (see StringExtensions). */
export const CARD_ART_SIZES = {
  /** images/card_portraits/<slug>.png — the in-hand portrait. */
  small: { dir: CARD_ART_DIR, w: 250, h: 190 },
  /** images/card_portraits/big/<slug>.png — the inspected card. */
  big: { dir: join(CARD_ART_DIR, "big"), w: 1000, h: 760 },
} as const;

// Measured from the art already in the tree: 128² icon, 256² inspected.
// Relics carry a third file, <slug>_outline.png, a white silhouette of the
// icon that the game tints for the relic bar; it sits beside the small icon.
export const RELIC_ART_SIZES = {
  small: { dir: RELIC_ART_DIR, w: 128, h: 128 },
  big: { dir: join(RELIC_ART_DIR, "big"), w: 256, h: 256 },
} as const;

export type ArtSize = keyof typeof CARD_ART_SIZES;

/**
 * Resolve `rel` under `root` and refuse anything that escapes it. Every path
 * the HTTP layer derives from a request goes through here.
 */
export function safeJoin(root: string, rel: string): string {
  const full = resolve(root, rel);
  if (full !== root && !full.startsWith(root + sep)) {
    throw new Error(`path escapes ${root}: ${rel}`);
  }
  return full;
}

/** A content slug is the lowercase snake stem the art files are named by. */
export function assertSlug(slug: string): string {
  if (!/^[a-z0-9_]{1,64}$/.test(slug)) throw new Error(`bad slug: ${slug}`);
  return slug;
}

function walk(dir: string): string[] {
  if (!existsSync(dir)) return [];
  const out: string[] = [];
  for (const name of readdirSync(dir)) {
    const full = join(dir, name);
    if (statSync(full).isDirectory()) out.push(...walk(full));
    else out.push(full);
  }
  return out;
}

/** Repo-relative .cs files under HelloSpireCode whose path names a Cards dir. */
export function allCardFiles(): string[] {
  return walk(CODE_DIR)
    .filter((f) => f.endsWith(".cs") && f.includes(`${sep}Cards${sep}`))
    .map((f) => relative(REPO_ROOT, f))
    .sort();
}

/** Repo-relative .cs files that can hold relic classes. */
export function allRelicFiles(): string[] {
  return walk(CODE_DIR)
    .filter((f) => f.endsWith(".cs") && (f.includes(`${sep}Relics${sep}`) || f.endsWith("Relic.cs")))
    .map((f) => relative(REPO_ROOT, f))
    .sort();
}
