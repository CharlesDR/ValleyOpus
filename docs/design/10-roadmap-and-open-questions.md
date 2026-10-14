# 10 · Roadmap & Open Questions

Status: **DRAFT v0.8** (2026-10-14). v0.8: Charles's full-review feedback (item budget loosened, internal consistency principle, integration targets from the top-80 list). v0.7: Charles's answers to the v0.6 questions, and Laboratory design moved before the architecture gate. v0.6: the docs pass against Charles's guiding answers in doc 01, with the questions it raised (below). The path to architecture and every decision on it are in [doc 00](00-critical-path.md).

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
| Content Patcher extensibility (Primes, substances, apparatus, recipes) | ✓ | |
| Vanilla and SVE villagers integrated (dialogue, gift tastes, events) | ✓ | |
| Mod tie-ins | SVE, Alecto the Witch, Ridgeside Village, East Scarp | others, by Nexus ranking |
| Plant transmutation (seeds, the Rootling) | ✓ (candidates) | |
| Ending | a definitive finished state (the Great Work) | an endless long tail |
| Reagent Satchel (item restraint for adventure-area drops only) | ✓ | maybe an Essence Reservoir |
| Directional chaining (West → East, South chests) | ✓ | |
| Arcane logistics | ✗ | ✓ |
| Tool and weapon forging (Orichalcum) | ✗ | ✓ |
| Potion Craft-style **brew map** | ✗ | ✓ (needs more design before it's scoped) |
| Commissioned art | before the 1.0 publish | |

**Release plan (Charles, doc 01):** what we call 1.0 is a **public beta**, released after a lot of polish. Success means technical polish, satisfying loops and a fun experience. After 1.0 the mod is maintained: game updates, bug fixes, new content and translations.

## Charles's action items
1. ~~**Nexus popularity export**~~: done (2026-10-14). Charles supplied the top-80 list; the targets are in doc 09.
2. **Full review of the design docs** before the next stage (Phase 1, doc 12).
3. ~~Answer the questions from the v0.6 pass~~: done (below).

## Answers to the v0.6 questions (Charles, 2026-10-13 21:47)
All 18 answered and applied in v0.7.
1. **Capstone rituals are fully manual.** Automate can't stage them; the player loads and triggers them by hand, to keep their gravitas. The same applies to every hand-made-only recipe (doc 04).
2. Quest items are story-quest deliveries only; contracts, bounties and workshop bundles accept automated output.
3. Products made mainly for gifting are hand-made only; any alchemy item can still be gifted.
4. Four T3 Labs, one per Familiar, hold a setup for every item with room to spare.
5. Slapstick: Finch and Tobin, with Lark kept restrained; Oriel, Hesper and Cyprian stay dry.
6. The dark-alchemy past is a named fictional incident, elsewhere and decades ago, not the cause of the workshop's ruin.
7. Without Alecto the Witch, the Witch stays the unnamed vanilla Witch.
8. "Sofia" is SVE's Sophia.
9. The villager angles stand. **New:** **Evelyn** is a secret alchemist, and **Andy** (SVE), a gruff hillbilly type, is the resident skeptic (doc 08).
10. Seed transmutation and the Rootling are approved as candidates.
11. Passive combat effects come from buffs and a couple of rings, **plus some trinket-slot items** (doc 03a).
12. New drops are high-value specialty reagents found only in late-game areas. **With SVE installed they appear in SVE's late locations; without it, they're grouped into fitting vanilla places** (doc 03b).
13. No new forage in hidden areas.
14. The late-game gates (Skull Cavern, Ginger Island, Volcano, Mr. Qi) are approved as proposals.
15. Conditions escalate: none or one for Tier 1, one or two for Tier 2, two or more for Tier 3 and capstones.
16. Completing the Great Work is the 1.0 finished state.
17. All the added scope stays in 1.0.
18. The public beta ships as 1.0.0, marked beta on Nexus.

## Waiting on Charles (earlier)
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
- Capstones, once progression is coherent.
- Skill display name (working name: Alchemy).
- Lark's family (Jodi's or Pam's) and Hesper's outdoor spot.

## Upcoming tasks
- Balance sweep tooling (doc 06), including the item census (doc 03b).
- Nexus popularity ranking for integration targets. This needs a manual export or the browser, because Nexus blocks automated access from cloud sessions.
- Lore check: read the vanilla and SVE wiki entries for every villager with a planned alchemy angle (doc 08), and the Alecto the Witch, Ridgeside Village and East Scarp pages for tie-in spots and map edits to avoid.
- Verify the CC-opening event ID, the Quarry bridge and bus repair flags (both CC and Joja routes), and the SVE Town map behind the CC.

## Change log
- **v0.8 (2026-10-14):** Charles's full review: the item budget is loosened to adventure-area drops only (doc 03b), internal consistency becomes a core principle (doc 01), and integration and compatibility targets are chosen from his top-80 export (doc 09). The material-name sweep waits on his choice of language order.
- **v0.7 (2026-10-13):** Charles answered all 18 v0.6 questions (capstone rituals fully manual, Evelyn as a secret alchemist, Andy as the skeptic, trinket-slot items, late-area reagents placed by SVE presence), and set the Lab direction: T3 is 50% larger than T2, every tier gets aesthetic touches and a unique floorplan, and Lab design now comes before the architecture gate (doc 00).
- **v0.6 (2026-10-13):** a pass over docs 02–09 against Charles's guiding answers in doc 01: pacing targets and no calendar gates, escalating conditions, the 1.0 finished state, hand-made-only gifts, quest items and capstones, Automate expected but optional, late-game Lab sizing, the combat cap, plants-only living transmutation and the Wizard's ban, tone and reputation, full villager integration, 1.0 tie-ins with Alecto the Witch, Ridgeside Village and East Scarp, and Content Patcher extensibility. 18 questions for Charles.
- **v0.5 (2026-10-13):** Charles's Part 4 decisions (doc 00): Spring Y1 slice, directional chaining in 1.0 with arcane logistics moved to 2.0, i18n, Golem Clay dropped, three Experiment Table inputs, vanilla-speed skill, economy knobs in GMCM, SVE-only integration, Lab tiers by size with Familiars at T3. The Charles action items list was added.
- **v0.4 (2026-10-13):** from Charles's review of the inspiration research: Lore & Worldbuilding pillar and the "boost underwhelming base-game activities" guiding star (doc 01); automation can't make discoveries and undiscovered outputs lock in the machine (docs 04, 07); three celebration tiers, clock freeze, residue hints, codex depth and the per-farm secret recipe with re-roll (doc 07); flower-fed arcane apparatus, Gloam Lanterns and late-game blended machines (doc 04).
- **v0.3 (2026-10-13):** automation model: minor arcane apparatus are fully automatable, ritual altars are staged by Automate and triggered by hand, and attunement decay is removed; profession perks revised to match.
- **v0.2 (2026-10-13):** cast approved and renamed (6F/5M, random assignment); the workshop is the hub under the CC; Oriel/Tobin roles revised; grand rituals, ley nodes, Ley Taps, Ivo/Agnes/Bennet gates and the item budget added; #3 merged into Hesper.
- **v0.1 (2026-10-13):** first draft.
