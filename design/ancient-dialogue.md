# Ancient dialogue

Status: **written — all nine Ancients, all three characters, 108 lines.** One thing is still
unverified, and it is the ID spelling, not the text. See *The one open risk* below.

## What exists

`HelloSpire/localization/eng/ancients.json` holds 108 lines: nine Ancients × three characters ×
four beats each.

| Ancient | Act | What they are, and what the exchange turns on |
|---|---|---|
| The Architect | — | Condescending builder. The original twelve lines, from the mod template; the tonal benchmark for everything else. |
| Neow | 1 | Remakes arrivals. Halting, elliptical. Each character asks what was kept and what was not. |
| Orobas | 2 | Childlike, doubles words, calls you *puppet*. Collides with three characters who are each, in their way, extremely serious. |
| Pael | 2 | Exhausted, worried about "Father". The only Ancient who sounds like the Paladin feels. |
| Tezcatara | 2 | Hospitable, third person, calls you *dear*. Hospitality as pressure. |
| Darv | 2–3 | Forgetful collector, rambling. Loses the thread mid-line, including mid-attack. |
| Nonupeipe | 3 | Gracious, blessing-giver. Each character refuses the blessing in their own idiom. |
| Tanx | 3 | ALL CAPS. Reads every character as a stat and approves of the wrong one. |
| Vakuu | 3 | Offers power for submission. Lands on what each character will not give up. |

The roster and the personalities come from
[wiki.gg's Ancients page](https://slaythespire.wiki.gg/wiki/Slay_the_Spire_2:Ancients).

## The one open risk

The wiki gives **display names**, not the internal localization IDs. The eight new Ancients are
keyed from those names — `NEOW`, `OROBAS`, `PAEL`, `TEZCATARA`, `DARV`, `NONUPEIPE`, `TANX`,
`VAKUU` — on a single precedent: "The Architect" is `THE_ARCHITECT`. That precedent covers the
`THE_` prefix on a name that starts with "The", and nothing else.

If the game spells one differently, **that Ancient is silently mute** — no error, no log line.
Nothing crashes and nothing else is affected, so the blast radius is one Ancient's twelve lines.

The repair costs one command and never touches the writing:

```
python tools/gen_ancient_dialogue.py check --base <dir>/localization/eng/ancients.json
python tools/gen_ancient_dialogue.py rename NEOW <whatever it really is>
```

`check --base` prints `IGNORED <key> (no such Ancient: X)` for every ID the game does not have,
and `rename` moves all twelve of that Ancient's keys in one go. Run `check --base` once against
an extracted base-game localization (ART.md, *Reference: extracting the base game's assets*) and
this stops being a risk.

Two smaller unknowns the same check settles:

- **Slots.** Every Ancient here uses the Architect's four (`0-0r.char`, `0-0r.next`,
  `0-1r.ancient`, `0-attack`). Most of these Ancients hand out relics rather than fight, so
  `0-attack` may be dead for them. A dead slot is ignored the same way a wrong ID is.
- **Conversations.** The leading `0` is a conversation index. If the game reads `1-*` too, every
  Ancient is a larger writing surface than four lines.

## The key grammar

Read off the Architect's four keys:

```
THE_ARCHITECT.talk.HELLOSPIRE-PALADIN.0-0r.char      the character's line
THE_ARCHITECT.talk.HELLOSPIRE-PALADIN.0-0r.next      the stage direction beside it
THE_ARCHITECT.talk.HELLOSPIRE-PALADIN.0-1r.ancient   the Ancient's reply
THE_ARCHITECT.talk.HELLOSPIRE-PALADIN.0-attack       the Ancient's line as the fight starts
```

`<ANCIENT>.talk.<CHARACTER>.<slot>`. Only `.char` is our character's own voice; `.next`,
`.ancient` and `.attack` are the Ancient's. Do not assume every Ancient uses these four slots —
the scaffold reads each Ancient's real slot list out of the base file rather than assuming the
Architect's.

## The exchange, as the Architect's twelve lines set it

Four beats, in order: **the character sizes the Ancient up → the Ancient sizes them up → the
Ancient answers in kind → the Ancient starts the fight.**

- The character's line is a *read of that specific Ancient*, in one sentence, delivered flat.
  "I have climbed worse than you" only works against something that is a structure.
- The stage direction is third-person, present tense, and notices one concrete object: armor,
  instruments, the cylinder. Never an emotion.
- The Ancient's reply is condescension with a grain of respect in it, and it lands on the
  character's *mechanic*, not their outfit: faith, dissection, counting deaths in advance.
- The attack line resolves the metaphor the exchange opened. "Then let us test the shield."

No line exceeds one sentence. Nothing rhymes. No Ancient says the character's name.

## The three voices

| | Reads as | Says | Never says |
|---|---|---|---|
| **Paladin** | Sworn to a light that has not answered in a long time. Holds the line anyway. | Duty, weight, the wall. Short declaratives. | Anything hopeful about the light. It stopped answering; they kept going. |
| **Alchemist** | Treats the Spire as a very large, very hostile experiment. Takes notes between fights. | Curiosity with no manners. Questions as statements. | Fear. An Ancient is a specimen first. |
| **Gunslinger** | A weathered shooter off an endless road. Counts what is coming. | Arithmetic. Six of anything. The road. | More words than the count needs. |

Each character's line against an Ancient should be a collision between that voice and what that
Ancient *is* — the Paladin's wall against a builder, the Alchemist's scalpel against something
that does not bleed, the Gunslinger's count against something that cannot be counted.

## When it is finished

- [x] Every Ancient on the wiki roster, × three characters, × four beats. **108 lines.**
- [ ] `gen_ancient_dialogue.py check --base <extracted ancients.json>` exits clean — the one
      step that turns the inferred IDs into verified ones.
- [ ] Decide whether `0-attack` belongs on the relic-giving Ancients, once the base file says
      which slots they actually declare.
