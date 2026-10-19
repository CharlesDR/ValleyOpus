# 10 · Roadmap & Open Questions

Status: **DRAFT v0.10** (2026-10-18). v0.10: doc 17 settles Satchel storage (Charles chose one bag per player) and adds defaults D11–D14. v0.9: this doc now collects **every open item** from all the docs (below), grouped by who or what settles it (cleanup sweep). v0.8: Charles's full-review feedback (item budget loosened, internal consistency principle, integration targets from the top-80 list). v0.7: Charles's answers to the v0.6 questions, and Laboratory design moved before the architecture gate. v0.6: the docs pass against Charles's guiding answers in doc 01, with the questions it raised (below). The path to architecture and every decision on it are in [doc 00](00-critical-path.md).

## Version scope
| | 1.0 | 2.0 |
|---|---|---|
| Primes, decomposition, Tier 1–3 substances | ✓ | expansions |
| Engineered and arcane apparatus, **adjacency**, **grand rituals**, staged altars (animation) | ✓ | **ritual mini-game** |
| Ley nodes and Ley Taps | ✓ | |
| Skill with L5/L10 professions (vanilla-speed curve) | ✓ | **prestige / Ultimate Mastery** perks |
| Laboratory, 3 tiers by interior size, Familiars specialize T3 Labs (max 4; doc 18) | ✓ | more specializations |
| Derelict workshop, storyline, cast of 11 + Familiars (doc 08a) | ✓ | more arcs |
| Shared codex and discovery celebrations | ✓ | |
| Content Patcher extensibility (Primes, substances, apparatus, recipes) | ✓ | |
| Vanilla and SVE villagers integrated (dialogue, gift tastes, events) | ✓ | |
| Mod tie-ins | SVE, Alecto the Witch, Ridgeside Village, East Scarp | others, by Nexus ranking |
| Plant transmutation (seeds, the Radicula) | ✓ (candidates) | |
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
2. ~~Full review of the design docs~~: done (2026-10-14, applied in v0.8).
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
10. Seed transmutation and the Radicula are approved as candidates.
11. Passive combat effects come from buffs and a couple of rings, **plus some trinket-slot items** (doc 03a).
12. New drops are high-value specialty reagents found only in late-game areas. **With SVE installed they appear in SVE's late locations; without it, they're grouped into fitting vanilla places** (doc 03b).
13. No new forage in hidden areas.
14. The late-game gates (Skull Cavern, Ginger Island, Volcano, Mr. Qi) are approved as proposals.
15. Conditions escalate: none or one for Tier 1, one or two for Tier 2, two or more for Tier 3 and capstones.
16. Completing the Great Work is the 1.0 finished state.
17. All the added scope stays in 1.0.
18. The public beta ships as 1.0.0, marked beta on Nexus.

## Earlier decisions
All Part 4 decisions (doc 00) and the earlier pending items were answered by 2026-10-13: cast names (the apprentice is Lark), the Reagent Satchel (doc 03b), the arcane automation model (doc 04), the ritual mini-game in 2.0, the Gloam Lantern collecting Shades (doc 04), and the defaults (mod ID, 36-cell grid, dark twins, perks-only-add, residue → Prima Materia).

## First playable slice (S1)
Spring Y1 end to end: the intro cutscene, the first workshop room, decomposition, the Experiment Table, 4–6 apparatus (engineered and minor arcane), the codex and the Satchel. Specced in doc 11 (v0.2, in review as PR #14): six apparatus, a five-quest starter chain, four test configurations (chaining-only first, SVE from the first build), unit tests, logging and a testing guide.

## Open questions
The one list of everything still open, across all docs (collected in the 2026-10-18 cleanup sweep). Each doc's own Open questions section keeps its items with a tag and owner; when an item is settled, it is written into its owning doc and removed from both lists.

### Awaiting Charles: defaults already applied in the docs
Each is written into the docs as the default; confirming it just removes the *(default)* mark. **Confirmed by Charles on 2026-10-18:** D1 power is never a recipe input (Perpetuum Mobile is an altar ritual beside a Boiler), D2 Electrum and Rebis are a counterpart pair, D3 Ectoplasm is hand-made only, D7 a re-roll replaces the discovered secret, D8 ValleyOpus items decompose at a loss, D9 the Satchel auto-collects reagents only, D5 exact Table matching with a "something here is feeling crowded" line for a valid subset, D6 Table outputs at Nigredo. **Confirmed on 2026-10-19:** D10 Satchel contents live in one global inventory per player (doc 17 §4).
| # | Default | Where |
|---|---|---|
| D4 | **A fresh machine takes nothing from automation** until set once by hand (the alternative: first satisfiable in codex order). *Kept for now; Charles will playtest how it feels.* | doc 13 §6a |
| D11 | **The host's economy settings rule** while farmhands are connected; personal preferences stay local. | doc 17 §3c |
| D12 | **Version mismatch:** a farmhand without the mod or on another schema version gets a notice, and the host ignores their codex, bundle and unlock requests until versions match. | doc 17 §3e |
| D13 | **A save from a newer build** loads read-only (no writes, no migration) with a HUD notice. | doc 17 §5d |
| D14 | **Test rotation:** every build includes an SVE configuration and the remove-and-reload check; every build after the slice includes online co-op; a release candidate covers all eight configurations across its last two builds. *Controller and split-screen: groundwork every build, checked by code review; in-game tests deferred (Charles, provisional, 2026-10-19).* | doc 17 §8b |

### Design questions still open
- `[blocks-content]` **Where darkness holds outside a Lab** (night outdoors, caves, unlit rooms), since darkness is Required for every putrefaction recipe. *Owner: doc 12.*
- `[blocks-content]` **Inventory sort keys:** verify what vanilla Organize and the common sort mods key on before tags, categories and IDs are assigned (doc 03b rule 5, doc 09). *Owner: the data layer.*
- `[blocks-content]` **Late-install fast start** as an optional GMCM setting. *Owner: Charles / doc 16 (doc 14).*
- `[blocks-content]` **Effect count:** the 2–3 cap stays suspended; doc 15 §3a's catalogue is the 1.0 list for now. *Owner: Charles (doc 15).*

### For the Phase 4 docs (16 UX, 17 Multiplayer, Saves & Compat)
- **Doc 16:** the slice's menus first (Table, Satchel grid and picker, codex, bundle board, minimal chain overlay; doc 11); chaining visual aids and the recipe-memory icon (doc 13 §6a, §7); Finch's curiosity hint hidden in his shop tooltip (doc 07); the quarter-day hand-action budget (doc 02); the Satchel UI (doc 03b); codex, toasts and flourishes (doc 07).
- **Doc 17:** drafted (v0.1). Satchel storage is decided (D10, one bag per player); its other defaults are D11–D14 above; its test items are carried into each build's test plan (doc 17 §8c). New touchpoints (Familiar meeting places, ley-node sites, hidden-area entrances, Hesper's spot) join its register when chosen.

### Content pass (rows, docs 19 and 20)
- The six profession signature recipes and the odd-level conveniences (doc 05).
- Effect and infusion carriers, Giant's Catalyst and Divining Rod rows, gift tastes for the three gift products (doc 15).
- Capstone effects (doc 19, from doc 15).
- Hint wording and counts (doc 20, from doc 07); bundle contents, rewards and arrival scenes; the contract-reward list (doc 20, from doc 14).
- Slice draft scripts: the soft-start cutscene, the Furnace Room scene and the starter chain (doc 11).
- Familiar meeting places, gift lists and heart events (doc 20, from doc 18).

### Balance pass (the sweep, doc 06)
- Table unlock scope: "only that recipe" is revisited if codex completion is too obnoxious (docs 07, 12).
- The Lightning Coil letter in Winter; grant it with the room if it feels too slow (doc 14).
- The 150% late-game cap is to be revisited later (Charles, doc 06); condition stacking (docs 02, 06).
- Every Lab, Familiar, perk and price number (docs 05, 15, 18).
- Lab T1's 25,000g in late Spring Y1 is tight; the slice report measures it (doc 11 §1a).
- Aether yield by source rarity: a trace from Solar and Void Essence, the bulk from Prismatic Shards and legendary fish (placeholder 1 : 15–20 per item; doc 11, for docs 03 and 14 §7).

### Deferred design passes (agreed to come later)
- Opening cutscene spec (doc 02).
- Capstones, once progression is coherent (doc 19).
- Skill display name (working name: Alchemy).
- Lark's family (Jodi's or Pam's) and Hesper's outdoor spot.

### 2.0
Collected in the version-scope table above and tagged `[2.0]` in each doc: arcane logistics, forging, the brew map, the ritual mini-game, prestige perks, Lookup Anything recipe rules, flower-fed trickles, tie-in Familiars and more Lab specializations, custom-effect bombs.

## Upcoming tasks
- Balance sweep tooling (doc 06), including the item census (doc 03b).
- Lore check: read the vanilla and SVE wiki entries for every villager with a planned alchemy angle (doc 08), and the Alecto the Witch, Ridgeside Village and East Scarp pages for tie-in spots and map edits to avoid.
- Verify the CC-opening event ID, the Quarry bridge and bus repair flags (both CC and Joja routes), and the SVE Town map behind the CC.

## Change log
- **v0.10 (2026-10-18):** doc 17 drafted: Satchel storage moves from the design questions to D10, which Charles confirmed on 2026-10-19 (one bag per player); D11–D14 added (host economy settings, version mismatch, newer saves, test rotation); the doc 17 Phase 4 list is replaced by a pointer.
- **v0.9 (2026-10-18):** cleanup sweep: an Open questions hub collects every open item across the docs (defaults awaiting Charles, design questions, Phase 4, content and balance passes, deferred passes, 2.0); the earlier-decisions list is collapsed; the full-review action item and the Nexus ranking task are marked done; the cast count follows doc 08a (11).
- **v0.8b (2026-10-14):** material names swept to Latin first, English gloss second (Charles's choice); the rule is in doc 03a.
- **v0.8 (2026-10-14):** Charles's full review: the item budget is loosened to adventure-area drops only (doc 03b), internal consistency becomes a core principle (doc 01), and integration and compatibility targets are chosen from his top-80 export (doc 09). 
- **v0.7 (2026-10-13):** Charles answered all 18 v0.6 questions (capstone rituals fully manual, Evelyn as a secret alchemist, Andy as the skeptic, trinket-slot items, late-area reagents placed by SVE presence), and set the Lab direction: T3 is 50% larger than T2, every tier gets aesthetic touches and a unique floorplan, and Lab design now comes before the architecture gate (doc 00).
- **v0.6 (2026-10-13):** a pass over docs 02–09 against Charles's guiding answers in doc 01: pacing targets and no calendar gates, escalating conditions, the 1.0 finished state, hand-made-only gifts, quest items and capstones, Automate expected but optional, late-game Lab sizing, the combat cap, plants-only living transmutation and the Wizard's ban, tone and reputation, full villager integration, 1.0 tie-ins with Alecto the Witch, Ridgeside Village and East Scarp, and Content Patcher extensibility. 18 questions for Charles.
- **v0.5 (2026-10-13):** Charles's Part 4 decisions (doc 00): Spring Y1 slice, directional chaining in 1.0 with arcane logistics moved to 2.0, i18n, Golem Clay dropped, three Experiment Table inputs, vanilla-speed skill, economy knobs in GMCM, SVE-only integration, Lab tiers by size with Familiars at T3. The Charles action items list was added.
- **v0.4 (2026-10-13):** from Charles's review of the inspiration research: Lore & Worldbuilding pillar and the "boost underwhelming base-game activities" guiding star (doc 01); automation can't make discoveries and undiscovered outputs lock in the machine (docs 04, 07); three celebration tiers, clock freeze, residue hints, codex depth and the per-farm secret recipe with re-roll (doc 07); flower-fed arcane apparatus, Gloam Lanterns and late-game blended machines (doc 04).
- **v0.3 (2026-10-13):** automation model: minor arcane apparatus are fully automatable, ritual altars are staged by Automate and triggered by hand, and attunement decay is removed; profession perks revised to match.
- **v0.2 (2026-10-13):** cast approved and renamed (6F/5M, random assignment); the workshop is the hub under the CC; Oriel/Tobin roles revised; grand rituals, ley nodes, Ley Taps, Ivo/Agnes/Bennet gates and the item budget added; #3 merged into Hesper.
- **v0.1 (2026-10-13):** first draft.
