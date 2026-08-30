// Typed wrappers over the dev API. Everything the UI knows about content comes
// through here; nothing is cached across a save, because the server returns the
// re-parsed record and that is the authority.

import type { ParsedCard } from "./cs-cards.ts";
import type { ParsedRelic } from "./cs-relics.ts";
import type { StringTable } from "./localization.ts";
import type { BalanceStats } from "./balance.ts";

/** mtimeMs per size, or null when that file does not exist. */
export interface ArtState {
  small: number | null;
  big: number | null;
}

export interface CardRecord extends ParsedCard {
  loc: StringTable;
  balance: BalanceStats;
  art: ArtState;
}

export interface RelicRecord extends ParsedRelic {
  loc: StringTable;
  art: ArtState;
}

export interface Warning {
  kind: "collision" | "no-loc" | "no-art";
  className: string;
  message: string;
}

export interface Schema {
  cardTypes: string[];
  cardRarities: string[];
  relicRarities: string[];
  cardFields: string[];
  relicFields: string[];
  cardArt: Record<"small" | "big", { w: number; h: number }>;
  relicArt: Record<"small" | "big", { w: number; h: number }>;
}

async function getJson<T>(path: string): Promise<T> {
  const res = await fetch(path);
  if (!res.ok) throw new Error(`GET ${path}: HTTP ${res.status}`);
  return (await res.json()) as T;
}

export const fetchCards = (): Promise<CardRecord[]> => getJson("/api/cards");
export const fetchRelics = (): Promise<RelicRecord[]> => getJson("/api/relics");
export const fetchWarnings = (): Promise<Warning[]> => getJson("/api/warnings");
export const fetchSchema = (): Promise<Schema> => getJson("/api/schema");

async function put<T>(path: string, body: unknown): Promise<T> {
  const res = await fetch(path, {
    method: "PUT",
    headers: { "content-type": "application/json" },
    body: JSON.stringify(body),
  });
  const parsed = (await res.json()) as T & { error?: string };
  if (!res.ok) throw new Error(parsed.error ?? `HTTP ${res.status}`);
  return parsed;
}

export interface SaveBody {
  patch?: Record<string, unknown>;
  loc?: StringTable;
}

export const saveCard = (className: string, body: SaveBody): Promise<CardRecord> =>
  put(`/api/cards/${encodeURIComponent(className)}`, body);

export const saveRelic = (className: string, body: SaveBody): Promise<RelicRecord> =>
  put(`/api/relics/${encodeURIComponent(className)}`, body);

/** Cache-busted so a re-upload shows immediately rather than the stale PNG. */
export function artUrl(
  kind: "card" | "relic",
  slug: string,
  size: "small" | "big",
  mtime: number | null,
): string | null {
  return mtime === null ? null : `/api/art/${kind}/${size}/${slug}.png?v=${Math.floor(mtime)}`;
}

export interface ArtUploadResult {
  written: string[];
  /** Files with no sibling .png.import yet — Godot generates those on rescan. */
  needsImport: string[];
}

export async function uploadArt(
  kind: "card" | "relic",
  slug: string,
  art: { small?: string; big?: string },
): Promise<ArtUploadResult> {
  const res = await fetch(`/api/art/${kind}/${encodeURIComponent(slug)}`, {
    method: "POST",
    headers: { "content-type": "application/json" },
    body: JSON.stringify(art),
  });
  const body = (await res.json()) as ArtUploadResult & { error?: string };
  if (!res.ok) throw new Error(body.error ?? `HTTP ${res.status}`);
  return body;
}
