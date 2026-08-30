// Surgical source editing.
//
// Card behaviour lives in the same file as its balance numbers, so the editor
// NEVER regenerates C#. It records the byte range of each editable literal at
// parse time and splices replacements back in. Anything the parser did not
// claim — OnPlay bodies, comments, usings, formatting — is preserved exactly.

export interface Span {
  /** Byte offset of the first character of the literal. */
  start: number;
  /** Byte offset one past the last character. */
  end: number;
}

export interface Edit extends Span {
  /** Replacement text for [start, end). */
  text: string;
}

/**
 * Apply edits to `source`. Edits are applied right-to-left so earlier offsets
 * stay valid. Overlapping edits are a programming error and throw rather than
 * silently corrupting a source file.
 */
export function applyEdits(source: string, edits: readonly Edit[]): string {
  const sorted = [...edits].sort((a, b) => b.start - a.start);
  let prevStart = Number.POSITIVE_INFINITY;
  let out = source;
  for (const e of sorted) {
    if (e.start < 0 || e.end > source.length || e.start > e.end) {
      throw new Error(`edit out of range: [${e.start}, ${e.end}) of ${source.length}`);
    }
    if (e.end > prevStart) {
      throw new Error(`overlapping edits at [${e.start}, ${e.end})`);
    }
    out = out.slice(0, e.start) + e.text + out.slice(e.end);
    prevStart = e.start;
  }
  return out;
}

/** Format a number the way the C# sources do: decimal literals carry `m`. */
export function decimalLiteral(value: number): string {
  return `${trimNumber(value)}m`;
}

/** Format a plain int literal (cost, and anything else without a suffix). */
export function intLiteral(value: number): string {
  if (!Number.isInteger(value)) throw new Error(`not an integer: ${value}`);
  return String(value);
}

function trimNumber(value: number): string {
  if (!Number.isFinite(value)) throw new Error(`not a finite number: ${value}`);
  // Avoid exponent notation and trailing ".0" — the sources write 6m, not 6.0m.
  const s = Number.isInteger(value) ? String(value) : value.toFixed(4).replace(/0+$/, "");
  return s.endsWith(".") ? s.slice(0, -1) : s;
}
