#!/usr/bin/env python3
"""
Scaffold and check HelloSpire/localization/eng/ancients.json against the base game's own.

Ancient dialogue is keyed by the base game's Ancient IDs, and **a key the game does not
recognise is silently ignored** -- no error, no log line, just an Ancient that says nothing.
That makes guessing the ID names the one thing you must never do here, and it is why this
file is still one Ancient deep: `THE_ARCHITECT` is the only ID the mod template ever
handed us.

So this tool does the mechanical half, and leaves only the writing:

    # once, pointing at the base game's extracted localization (see ART.md)
    python tools/gen_ancient_dialogue.py scaffold --base <dir>/localization/eng/ancients.json

It reads every `<ANCIENT>.talk.<CHARACTER>.<slot>` key in the base file, works out the slot
shape each Ancient actually uses, and writes our ancients.json with one empty string per
missing line for all three of our characters -- real IDs, real slots, nothing invented.
Lines already written are never touched. Then you fill in the blanks.

    python tools/gen_ancient_dialogue.py check [--base <...>]

Reports lines still empty, and -- with --base -- any key of ours whose Ancient ID or slot
does not exist in the base game, which is the failure mode that otherwise stays invisible.

    python tools/gen_ancient_dialogue.py rename NEOW THE_NEOW

Moves every line written under one Ancient ID to another, text untouched. This is the repair
for the one thing the roster does not tell us: the eight Ancients past the Architect were
keyed from their DISPLAY names (wiki.gg), on the single precedent that "The Architect" is
`THE_ARCHITECT`. If the game spells one of them differently, `check --base` names it and this
fixes it in one line -- the writing never has to be touched.

The writing itself, and what each character sounds like, lives in design/ancient-dialogue.md.

Stdlib only. Does not need the mod to build, or Godot.
"""
import argparse
import json
import os
import re
import sys

ROOT = os.path.dirname(os.path.dirname(os.path.abspath(__file__)))
OURS = os.path.join(ROOT, "HelloSpire", "localization", "eng", "ancients.json")

#: Our three characters, in the order the file should read.
CHARACTERS = ["HELLOSPIRE-PALADIN", "HELLOSPIRE-ALCHEMIST", "HELLOSPIRE-GUNSLINGER"]

#: `<ANCIENT>.talk.<CHARACTER>.<slot>` -- the shape every line in this file has.
KEY = re.compile(r"^(?P<ancient>[A-Z0-9_]+)\.talk\.(?P<char>[A-Za-z0-9_-]+)\.(?P<slot>.+)$")


def load(path):
    with open(path, encoding="utf-8") as handle:
        return json.load(handle)


def parse(table):
    """{ancient: {slot: [characters that use it]}} plus the slot order first seen."""
    ancients = {}
    for key in table:
        m = KEY.match(key)
        if not m:
            continue
        slots = ancients.setdefault(m["ancient"], {})
        slots.setdefault(m["slot"], []).append(m["char"])
    return ancients


def scaffold(base_path, dry_run):
    base = parse(load(base_path))
    if not base:
        sys.exit(f"no `<ANCIENT>.talk.<CHARACTER>.<slot>` keys found in {base_path} -- "
                 "is that the base game's ancients.json?")

    ours = load(OURS) if os.path.exists(OURS) else {}
    out, added, kept = {}, 0, 0

    for ancient, slots in base.items():
        for character in CHARACTERS:
            for slot in slots:
                key = f"{ancient}.talk.{character}.{slot}"
                if ours.get(key):
                    out[key] = ours[key]
                    kept += 1
                else:
                    out[key] = ""
                    added += 1

    # Anything we already wrote that the base file does not describe is kept, but called out:
    # it is either a slot only our characters use, or a key the game silently ignores.
    orphans = [k for k, v in ours.items() if k not in out and v]
    for key in orphans:
        out[key] = ours[key]

    print(f"{len(base)} ancients x {len(CHARACTERS)} characters -> {len(out)} keys")
    print(f"  {kept} already written, {added} blank")
    if orphans:
        print(f"  {len(orphans)} key(s) not present in the base game's shape -- CHECK THESE, "
              f"the game ignores unknown keys silently:")
        for key in orphans:
            print(f"      {key}")

    if dry_run:
        print("(dry run; nothing written)")
        return

    with open(OURS, "w", encoding="utf-8") as handle:
        json.dump(out, handle, indent=2, ensure_ascii=False)
        handle.write("\n")
    print(f"wrote {OURS}")


def check(base_path):
    ours = load(OURS)
    blank = [k for k, v in ours.items() if not v]
    print(f"{len(ours)} keys, {len(blank)} still blank")
    for key in blank:
        print(f"  blank   {key}")

    if not base_path:
        return 1 if blank else 0

    base = parse(load(base_path))
    unknown = []
    for key in ours:
        m = KEY.match(key)
        if not m:
            unknown.append((key, "not `<ANCIENT>.talk.<CHARACTER>.<slot>`"))
        elif m["ancient"] not in base:
            unknown.append((key, f"no such Ancient: {m['ancient']}"))
        elif m["slot"] not in base[m["ancient"]]:
            unknown.append((key, f"{m['ancient']} has no slot {m['slot']}"))
    for key, why in unknown:
        print(f"  IGNORED {key}  ({why})")
    if unknown:
        print(f"{len(unknown)} key(s) the game will silently ignore.")
    return 1 if (blank or unknown) else 0


def rename(old, new):
    ours = load(OURS)
    moved = {}
    hits = 0
    for key, value in ours.items():
        m = KEY.match(key)
        if m and m["ancient"] == old:
            moved[f"{new}.talk.{m['char']}.{m['slot']}"] = value
            hits += 1
        else:
            moved[key] = value
    if not hits:
        sys.exit(f"no keys under {old}")

    with open(OURS, "w", encoding="utf-8") as handle:
        json.dump(moved, handle, indent=2, ensure_ascii=False)
        handle.write("\n")
    print(f"{old} -> {new}: {hits} keys moved")


def main():
    parser = argparse.ArgumentParser(description=__doc__,
                                     formatter_class=argparse.RawDescriptionHelpFormatter)
    sub = parser.add_subparsers(dest="command", required=True)

    s = sub.add_parser("scaffold", help="write real keys for every Ancient, blank where unwritten")
    s.add_argument("--base", required=True, help="the base game's localization/eng/ancients.json")
    s.add_argument("--dry-run", action="store_true")

    c = sub.add_parser("check", help="report blank lines, and keys the game would ignore")
    c.add_argument("--base", default=None, help="the base game's localization/eng/ancients.json")

    r = sub.add_parser("rename", help="move every line from one Ancient ID to another")
    r.add_argument("old")
    r.add_argument("new")

    args = parser.parse_args()
    if args.command == "scaffold":
        scaffold(args.base, args.dry_run)
        return 0
    if args.command == "rename":
        rename(args.old, args.new)
        return 0
    return check(args.base)


if __name__ == "__main__":
    raise SystemExit(main())
