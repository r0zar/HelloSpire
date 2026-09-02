// =============================================================================
// HELLOSPIRE CARD EDITOR — LOCAL DEVELOPMENT TOOL ONLY.
//
// This server rewrites C# source files, the English string table and PNG art
// anywhere under the repo's content trees, with NO AUTHENTICATION. Anyone who
// can reach it can rewrite the mod. Three guards keep that local:
//
//   1. Refuses to start under NODE_ENV=production.
//   2. Binds 127.0.0.1 — the host is a literal, deliberately not configurable.
//   3. CORS allowlist holds only the Vite dev origin and its own port.
//
// If you ever want this reachable from elsewhere, write a different program.
// Do not loosen these.
// =============================================================================

import { existsSync, mkdirSync, readFileSync, statSync, writeFileSync } from "node:fs";
import { join } from "node:path";
import cors from "cors";
import express from "express";

import { balanceStats } from "./src/balance.ts";
import { CARD_RARITIES, CARD_TYPES, cardEdits, parseCards } from "./src/cs-cards.ts";
import type { CardPatch, ParsedCard } from "./src/cs-cards.ts";
import { RELIC_RARITIES, parseRelics, relicEdits } from "./src/cs-relics.ts";
import type { ParsedRelic, RelicPatch } from "./src/cs-relics.ts";
import { applyEdits } from "./src/edits.ts";
import {
  CARD_FIELDS,
  RELIC_FIELDS,
  locFor,
  readTable,
  saveLoc,
  type StringTable,
} from "./src/localization.ts";
import {
  CARD_ART_SIZES,
  RELIC_ART_SIZES,
  REPO_ROOT,
  allCardFiles,
  allRelicFiles,
  assertSlug,
  readAllCards,
  readCardBases,
  safeJoin,
  type ArtSize,
} from "./src/repo.ts";

if (process.env.NODE_ENV === "production") {
  console.error("[card-editor] refusing to start under NODE_ENV=production — dev-only tool.");
  process.exit(1);
}

const PORT = 2580;
const ALLOWED_ORIGINS = new Set([
  "http://localhost:5180",
  "http://127.0.0.1:5180",
  "http://localhost:2580",
  "http://127.0.0.1:2580",
]);

const app = express();

// Art uploads carry two base64 PNGs (a 1000×760 portrait is comfortably over a
// megabyte); every other body is a small JSON patch. One path-aware middleware,
// because a route-scoped parser cannot raise a limit a global parser already
// rejected with a 413.
const jsonSmall = express.json({ limit: "256kb" });
const jsonArt = express.json({ limit: "16mb" });
app.use((req, res, next) => {
  (/^\/api\/art\//.test(req.path) ? jsonArt : jsonSmall)(req, res, next);
});
app.use(
  cors({
    origin: (origin, cb) => {
      if (!origin || ALLOWED_ORIGINS.has(origin)) cb(null, true);
      else cb(new Error(`origin not allowed: ${origin}`));
    },
  }),
);

// ── Reading content ─────────────────────────────────────────────────────────
//
// Every request re-reads and re-parses from disk. Source offsets are never
// cached across requests: a PUT re-parses the file it is about to edit, so an
// edit made in another window (or in an IDE) can't be clobbered by stale spans.

function readCards(): ParsedCard[] {
  return readAllCards();
}

function readRelics(): ParsedRelic[] {
  return allRelicFiles().flatMap((f) => parseRelics(f, readFileSync(join(REPO_ROOT, f), "utf8")));
}

/** mtime of an art file, or null when the slug has no art at that size. */
function artMtime(dir: string, slug: string): number | null {
  const p = join(dir, `${slug}.png`);
  return existsSync(p) ? statSync(p).mtimeMs : null;
}

function cardPayload(card: ParsedCard, loc: StringTable) {
  return {
    ...card,
    loc: locFor(loc, card.id, CARD_FIELDS),
    balance: balanceStats(card),
    art: {
      small: artMtime(CARD_ART_SIZES.small.dir, card.slug),
      big: artMtime(CARD_ART_SIZES.big.dir, card.slug),
    },
  };
}

function relicPayload(relic: ParsedRelic, loc: StringTable) {
  return {
    ...relic,
    loc: locFor(loc, relic.id, RELIC_FIELDS),
    art: {
      small: artMtime(RELIC_ART_SIZES.small.dir, relic.slug),
      big: artMtime(RELIC_ART_SIZES.big.dir, relic.slug),
    },
  };
}

app.get("/api/cards", (_req, res) => {
  try {
    const loc = readTable("cards");
    res.json(readCards().map((c) => cardPayload(c, loc)));
  } catch (err) {
    res.status(500).json({ error: (err as Error).message });
  }
});

app.get("/api/relics", (_req, res) => {
  try {
    const loc = readTable("relics");
    res.json(readRelics().map((r) => relicPayload(r, loc)));
  } catch (err) {
    res.status(500).json({ error: (err as Error).message });
  }
});

/** Enum choices the UI offers, so the two never drift apart. */
app.get("/api/schema", (_req, res) => {
  res.json({
    cardTypes: CARD_TYPES,
    cardRarities: CARD_RARITIES,
    relicRarities: RELIC_RARITIES,
    cardFields: CARD_FIELDS,
    relicFields: RELIC_FIELDS,
    cardArt: { small: CARD_ART_SIZES.small, big: CARD_ART_SIZES.big },
    relicArt: { small: RELIC_ART_SIZES.small, big: RELIC_ART_SIZES.big },
  });
});

// ── Writing ─────────────────────────────────────────────────────────────────

interface SaveBody {
  patch?: CardPatch & RelicPatch;
  loc?: StringTable;
}

app.put("/api/cards/:className", (req, res) => {
  try {
    const { className } = req.params;
    const body = req.body as SaveBody;

    const card = readCards().find((c) => c.className === className);
    if (!card) {
      res.status(404).json({ error: `no card class ${className}` });
      return;
    }

    if (body.patch) {
      // Re-parse the single file so the spans are current, then splice.
      const full = join(REPO_ROOT, card.file);
      const src = readFileSync(full, "utf8");
      // Bases are re-read too: a card on an intermediate base (SealCard) is
      // unparseable without them, and would look like it had vanished.
      const fresh = parseCards(card.file, src, readCardBases()).find(
        (c) => c.className === className,
      );
      if (!fresh) throw new Error(`${className} vanished from ${card.file}`);
      const edits = cardEdits(fresh, body.patch);
      if (edits.length > 0) writeFileSync(full, applyEdits(src, edits), "utf8");
    }

    if (body.loc) saveLoc("cards", card.id, body.loc, CARD_FIELDS);

    const loc = readTable("cards");
    const after = readCards().find((c) => c.className === className)!;
    res.json(cardPayload(after, loc));
  } catch (err) {
    res.status(400).json({ error: (err as Error).message });
  }
});

app.put("/api/relics/:className", (req, res) => {
  try {
    const { className } = req.params;
    const body = req.body as SaveBody;

    const relic = readRelics().find((r) => r.className === className);
    if (!relic) {
      res.status(404).json({ error: `no relic class ${className}` });
      return;
    }

    if (body.patch) {
      const full = join(REPO_ROOT, relic.file);
      const src = readFileSync(full, "utf8");
      const fresh = parseRelics(relic.file, src).find((r) => r.className === className);
      if (!fresh) throw new Error(`${className} vanished from ${relic.file}`);
      const edits = relicEdits(fresh, body.patch);
      if (edits.length > 0) writeFileSync(full, applyEdits(src, edits), "utf8");
    }

    if (body.loc) saveLoc("relics", relic.id, body.loc, RELIC_FIELDS);

    const loc = readTable("relics");
    const after = readRelics().find((r) => r.className === className)!;
    res.json(relicPayload(after, loc));
  } catch (err) {
    res.status(400).json({ error: (err as Error).message });
  }
});

// ── Art ─────────────────────────────────────────────────────────────────────

const ART_DIRS = { card: CARD_ART_SIZES, relic: RELIC_ART_SIZES } as const;
type ArtKind = keyof typeof ART_DIRS;

function artDir(kind: string, size: string): string {
  const sizes = ART_DIRS[kind as ArtKind];
  if (!sizes) throw new Error(`bad art kind: ${kind}`);
  const entry = sizes[size as ArtSize];
  if (!entry) throw new Error(`bad art size: ${size}`);
  return entry.dir;
}

app.get("/api/art/:kind/:size/:slug.png", (req, res) => {
  try {
    const slug = assertSlug(req.params.slug!);
    const file = safeJoin(artDir(req.params.kind!, req.params.size!), `${slug}.png`);
    if (!existsSync(file)) {
      res.status(404).end();
      return;
    }
    res.type("png").send(readFileSync(file));
  } catch (err) {
    res.status(400).json({ error: (err as Error).message });
  }
});

/** Strict data-URL decode — a PNG and nothing else reaches the filesystem. */
function decodePng(dataUrl: string): Buffer {
  const m = /^data:image\/png;base64,([A-Za-z0-9+/=]+)$/.exec(dataUrl);
  if (!m?.[1]) throw new Error("expected a base64 image/png data URL");
  const buf = Buffer.from(m[1], "base64");
  // PNG magic — belt and braces against a mislabelled payload.
  if (buf.subarray(0, 8).toString("hex") !== "89504e470d0a1a0a") throw new Error("not a PNG");
  return buf;
}

app.post("/api/art/:kind/:slug", (req, res) => {
  try {
    const kind = req.params.kind!;
    const slug = assertSlug(req.params.slug!);
    const body = req.body as Partial<Record<ArtSize | "outline", string>>;

    const written: string[] = [];
    for (const size of ["small", "big"] as const) {
      const dataUrl = body[size];
      if (!dataUrl) continue;
      const dir = artDir(kind, size);
      mkdirSync(dir, { recursive: true });
      const file = safeJoin(dir, `${slug}.png`);
      writeFileSync(file, decodePng(dataUrl));
      written.push(file.slice(REPO_ROOT.length + 1));
    }
    // Relics also ship a white silhouette beside the icon. Only relics have
    // one, so an outline sent for a card is a bug in the caller, not a file.
    if (body.outline !== undefined) {
      if (kind !== "relic") throw new Error("only relics have an outline");
      const dir = artDir(kind, "small");
      mkdirSync(dir, { recursive: true });
      const file = safeJoin(dir, `${slug}_outline.png`);
      writeFileSync(file, decodePng(body.outline));
      written.push(file.slice(REPO_ROOT.length + 1));
    }
    if (written.length === 0) throw new Error("no art in body");

    // Godot writes the sibling .png.import on its next project scan. Saying so
    // here is the only warning a designer gets that a brand-new art file is not
    // yet visible in game.
    const needsImport = written.filter((f) => !existsSync(join(REPO_ROOT, `${f}.import`)));
    res.json({ ok: true, written, needsImport });
  } catch (err) {
    res.status(400).json({ error: (err as Error).message });
  }
});

// ── Content warnings ────────────────────────────────────────────────────────
//
// The checks a compiler cannot make: art and localization resolve by class
// name at runtime, so a missing string or a name collision is silent until
// someone opens the card in game.

app.get("/api/warnings", (_req, res) => {
  try {
    const cards = readCards();
    const relics = readRelics();
    const cardLoc = readTable("cards");
    const relicLoc = readTable("relics");
    const warnings: { kind: string; className: string; message: string }[] = [];

    const byClass = new Map<string, string[]>();
    for (const c of [...cards, ...relics]) {
      byClass.set(c.className, [...(byClass.get(c.className) ?? []), c.file]);
    }
    for (const [className, files] of byClass) {
      if (files.length > 1) {
        warnings.push({
          kind: "collision",
          className,
          // Art and strings key off the class name alone, so two classes with
          // one name share a portrait and a title no matter which characters
          // they belong to.
          message: `declared in ${files.length} files (${files.join(", ")}) — they share art and strings`,
        });
      }
    }

    for (const c of cards) {
      if (cardLoc[`${c.id}.title`] === undefined) {
        warnings.push({ kind: "no-loc", className: c.className, message: "no localized title" });
      }
      if (artMtime(CARD_ART_SIZES.small.dir, c.slug) === null) {
        warnings.push({ kind: "no-art", className: c.className, message: "no portrait art" });
      }
    }
    for (const r of relics) {
      if (relicLoc[`${r.id}.title`] === undefined) {
        warnings.push({ kind: "no-loc", className: r.className, message: "no localized title" });
      }
      if (artMtime(RELIC_ART_SIZES.small.dir, r.slug) === null) {
        warnings.push({ kind: "no-art", className: r.className, message: "no relic icon" });
      }
    }

    res.json(warnings);
  } catch (err) {
    res.status(500).json({ error: (err as Error).message });
  }
});

app.listen(PORT, "127.0.0.1", () => {
  console.log(`[card-editor] api on http://127.0.0.1:${PORT} — repo ${REPO_ROOT}`);
});
