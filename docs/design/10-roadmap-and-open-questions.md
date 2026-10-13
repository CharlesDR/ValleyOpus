# 10 · Roadmap & Open Questions

Status: **DRAFT v0.5** (2026-10-13). The path to architecture and every decision on it are in [doc 00](00-critical-path.md).

## Version scope
| | 1.0 | 2.0 |
|---|---|---|
| Primes, decomposition, Tier 1–3 substances | ✓ | expansions |
| Engineered and arcane apparatus, **adjacency**, **grand rituals**, staged altars (animation) | ✓ | **ritual mini-game** |
| Ley nodes and Ley Taps | ✓ | |
| Skill with L5/L10 professions (vanilla-speed curve) | ✓ | **prestige / Ultimate Mastery** perks |
| Laboratory, 3 tiers by interior size, Familiars augment T3 (max 4) | ✓ | more specializations |
| Derelict workshop, storyline, cast of 10 + Familiars | ✓ | more arcs |
| Shared codex and discovery celebrations | ✓ | |
| Reagent Satchel and the item budget | ✓ | maybe an Essence Reservoir |
| Directional chaining (West → East, South chests) | ✓ | |
| Arcane logistics | ✗ | ✓ |
| Tool and weapon forging (Orichalcum) | ✗ | ✓ |
| Potion Craft-style **brew map** | ✗ | ✓ (needs more design before it's scoped) |
| Commissioned art | before the 1.0 publish | |

## Charles's action items
1. **Nexus popularity export** for integration targets (manual, since Nexus blocks cloud sessions). 1.0 integrates SVE only, so this mainly sets 2.0 priorities.
2. **Full review of the staged design docs** before the next stage (Phase 1, doc 12).

## Waiting on Charles
- Part 4 decisions: all answered and confirmed 2026-10-13 (doc 00).
- ~~Soul-stone name~~: decided, **Gloam Lantern** collecting **Shades** (doc 04).

Earlier items, all decided as of 2026-10-13 17:39, and the defaults (mod ID, 36-cell grid, dark twins, perks-only-add, residue → Prima Materia) are confirmed.
1. ~~Cast names~~: approved; the apprentice is Lark.
2. ~~Inventory approach~~: decided, Reagent Satchel (doc 03b).
3. ~~Arcane automation default~~: decided. Minor arcane apparatus are fully automatable; ritual altars are staged by Automate and triggered by hand (doc 04).
4. ~~Ritual mini-game~~: decided. 1.0 is animation only; the mini-game moves to 2.0.

## First playable slice (S1)
Spring Y1 end to end: the intro cutscene, the first workshop room, decomposition, the Experiment Table, 4–6 apparatus (engineered and minor arcane), the codex and the Satchel. Specced in doc 11.

## Deferred design passes (agreed to come later)
- Opening cutscene spec (doc 02).
- Laboratory detail (direction set: T1 = Shed size, T2 = Big Shed size, T3 larger; escalating apparatus bonuses; Familiar assignment at T3 is the specialization), after the first slice.
- Capstones, once progression is coherent.
- Skill display name (working name: Alchemy).
- Lark's family (Jodi's or Pam's) and Hesper's outdoor spot.

## Upcoming tasks
- Move these docs into the GitHub repo (`CharlesDR/ValleyOpus` exists; waiting on Charles's go).
- Balance sweep tooling (doc 06), including the item census (doc 03b).
- Nexus popularity ranking for integration targets. This needs a manual export or the browser, because Nexus blocks automated access from cloud sessions.
- Verify the CC-opening event ID, the Quarry bridge and bus repair flags (both CC and Joja routes), and the SVE Town map behind the CC.

## Change log
- **v0.5 (2026-10-13):** Charles's Part 4 decisions (doc 00): Spring Y1 slice, directional chaining in 1.0 with arcane logistics moved to 2.0, i18n, Golem Clay dropped, three Experiment Table inputs, vanilla-speed skill, economy knobs in GMCM, SVE-only integration, Lab tiers by size with Familiars at T3. The Charles action items list was added.
- **v0.4 (2026-10-13):** from Charles's review of the inspiration research: Lore & Worldbuilding pillar and the "boost underwhelming base-game activities" guiding star (doc 01); automation can't make discoveries and undiscovered outputs lock in the machine (docs 04, 07); three celebration tiers, clock freeze, residue hints, codex depth and the per-farm secret recipe with re-roll (doc 07); flower-fed arcane apparatus, Gloam Lanterns and late-game blended machines (doc 04).
- **v0.3 (2026-10-13):** automation model: minor arcane apparatus are fully automatable, ritual altars are staged by Automate and triggered by hand, and attunement decay is removed; profession perks revised to match.
- **v0.2 (2026-10-13):** cast approved and renamed (6F/5M, random assignment); the workshop is the hub under the CC; Oriel/Tobin roles revised; grand rituals, ley nodes, Ley Taps, Ivo/Agnes/Bennet gates and the item budget added; #3 merged into Hesper.
- **v0.1 (2026-10-13):** first draft.
