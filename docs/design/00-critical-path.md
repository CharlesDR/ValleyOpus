# 00 · Design Critical Path to Architecture

Status: **DRAFT v0.5** (2026-10-18). v0.5: docs cleanup sweep: the doc inventory shows each doc's current state, the path shows progress, superseded S3 details are marked, and every open item now lives in doc 10. v0.4: Laboratory design (doc 18) moves before the architecture gate (Charles). v0.3: the guiding answers in doc 01 add requirements for docs 12, 13, 16 and 17 (below), and U2 widens to four tie-in mods. v0.2: Charles answered every Part 4 decision. This doc says which design docs must be "ready" before architecture planning starts, in what order, and which decisions Charles still owns along the way.

## The governing rule
Architecture needs the **shape** of every system, not its **content**. Shape means what kinds of things exist, the rules connecting them, and which of them need C# rather than Content Patcher data. Content means names, numbers, dialogue, art and exact recipes. Content can be filled in during execution as long as it is data-driven, so it never blocks architecture. Rework comes from changing shape late, so the path front-loads shape decisions and pushes content behind the gate.

A doc is **ready for architecture** when every section of the template below is filled, its "Open questions" list has no item tagged `[blocks-arch]`, and it contradicts no other ready doc. **Internal consistency is a core principle (doc 01):** each doc also checks that its names, elemental affinities and machine behaviours follow the established patterns, and records any deliberate deviation with its reason.

---

## Part 1 · Template for every design doc

New docs use this skeleton. Existing docs (01–09) get retrofitted with the missing sections when they come up on the path, not all at once.

```markdown
# NN · Title

Status: **DRAFT vX.Y** (date) · Readiness: not ready | ready for architecture | content complete
Depends on: [docs] · Feeds: [docs]

## Purpose
One paragraph: what player experience this system delivers and which pillars it serves (doc 01).

## Decisions locked
Bullets, each dated and attributed (Charles / default confirmed). Nothing here is reopened without a change-log entry.

## Player-facing spec
What the player sees and does: the loop, the unlock moment, the failure state, the UI touchpoints.

## Rules & systems
The mechanics as rules: inputs, outputs, conditions, interactions with other systems, edge cases (multiplayer, Joja route, SVE, Automate, Walk of Life).

## Data shape
The tables or records this system needs, with their fields (not their rows). Mark each as CP data or C# state, and note save-data and multiplayer ownership.

## Content inventory
The rows: items, machines, recipes, events, NPC lines. Counts and placeholders are fine before the gate. Every new item has a stated purpose, and adventure-area drops follow the restraint rule in doc 03b.

## Tech hooks
Vanilla or framework features used (Data/Machines, GSQs, SpaceCore, Automate), what needs custom C#, known risks, and 1.7 migration notes.

## Balance hooks
Which doc 06 metrics apply, and which numbers are placeholders to tune in the sweep.

## Open questions
Each tagged `[blocks-arch]`, `[blocks-content]` or `[2.0]`, with an owner.

## Change log
- **vX.Y (date):** what changed and why.
```

---

## Part 2 · Doc inventory and gaps

State as of the 2026-10-18 cleanup sweep. Each doc's own status line is authoritative; open items for every doc are collected in [doc 10](10-roadmap-and-open-questions.md#open-questions).

| # | Doc | State (2026-10-18) | What's still missing before the gate |
|---|---|---|---|
| 01 | Vision & Pillars | v0.7 | nothing; the reference all other docs check against |
| 02 | Progression & Unlocks | v0.7, the narrative of the Year 1 curve | nothing; the unlock map is doc 14 and the Lab is doc 18 |
| 03 | Essences & Decomposition | v0.4, data shape done | nothing |
| 03a | Substances Catalog | v0.11, apparatus column done | nothing; recipe rows are content |
| 03b | Items & Inventory | v0.9 | the census refreshed once doc 11's rows land |
| 04 | Machines & Automation | v0.8, philosophy only | nothing; the machine list is doc 13 |
| 05 | Skill & Professions | v0.7, ready | nothing |
| 06 | Economy & Balance | v0.6 | policy is done; numbers come from the sweep (execution) |
| 07 | Discovery & Codex | v0.9, ready | nothing |
| 08 / 08a | Narrative & Cast | v0.4 / approved | only the hooks architecture needs (triggers, flags, locations); scripts come later |
| 09 | Technical Architecture | v0.6, a sketch | rewritten as the architecture plan at the gate |
| 10 | Roadmap & Open Questions | v0.9 | nothing; it collects every open item |
| **11** | **First Playable Slice** | v0.1 in review (PR #14) | Charles's review |
| **12** | **Recipe & Process Model** | v0.4, ready | nothing |
| **13** | **Apparatus Catalog** | v0.6, ready | nothing (the fresh-machine default waits on playtesting) |
| **14** | **Unlock Map & Workshop Bundles** | v0.4, ready | nothing |
| **15** | **Products & Outside Uses** | v0.7, ready | nothing |
| **16** | **UX & Interface** | v0.1 in review | Charles's review; defaults D15–D19 and the late-install fast start (doc 10) |
| **17** | **Multiplayer, Saves & Compatibility** | not started | ownership of every piece of state, save migration, uninstall behaviour, SVE and Automate test matrix |
| **18** | **Laboratory & Familiars** | v0.2, ready | nothing |
| 19 | Capstones *(deferred)* | — | after progression is coherent; hook only |
| 20 | Content Script *(after gate)* | — | events, mail, dialogue, schedules |
| 21 | Art & Audio List *(after gate)* | — | sprites, animations, sounds; placeholders until commissioned |

---

## Part 3 · The critical path

Each phase only starts once the decisions it depends on are made. Inside a phase, docs can be written in parallel.

**Progress (2026-10-18):** Phases 0–3 are done except doc 11, which is in review (PR #14). Phase 4 (docs 16 and 17) is next, then the gate. No code before the gate (Charles).

```
Phase 0  Scope lock ─────────────► decisions S1–S5
              │
Phase 1  12 Recipe & Process Model (keystone) + 03/03a updated
              │
Phase 2  13 Apparatus Catalog ◄──► 07 Discovery (updated)
              │
Phase 3  15 Products ─► 14 Unlock Map ─► 05 Skill (updated) ─► 18 Laboratory ─► 11 Slice
              │
Phase 4  16 UX  ║  17 Multiplayer, Saves & Compat
              │
        ══ ARCHITECTURE GATE ══  → architecture plan (doc 09 rewritten), then execution planning
              │
After    19 Capstones · 20 Content Script · 21 Art List · sweep numbers
```

### Phase 0 · Scope lock
**Why first:** each answer adds or removes whole C# subsystems. Changing them after the architecture exists is the most expensive kind of rework.
**Output:** decisions S1–S5 recorded in doc 10.

### Phase 1 · Recipe & Process Model (doc 12)
**Why next:** every other system (machines, discovery, codex, automation gating, quality, economy, the sweep) reads or writes this model. It is the one doc whose shape, if wrong, forces rework everywhere.
**Covers:** what a Prime, substance and product are as data; how decomposition yields are computed; how a recipe is defined (inputs, apparatus class, conditions, output, quality stage rule, residue rule); how one recipe can have twin routes; how discovery state is keyed (per recipe, per pair tried, per condition variant); the per-farm secret recipe seed; how content packs add rows.
**Also:** doc 03a gets an "apparatus class" column, and doc 03 gets its data shape.

### Phase 2 · Apparatus Catalog (doc 13) with Discovery (doc 07)
**Why together:** "discovery gates automation" ties each machine to recipe discovery state, so the two must agree on the same model from Phase 1.
**Covers:** the behaviour classes (plain Data/Machines, C# OutputMethod, staged ritual, multi-tile circle, Ley Tap, Experiment Table), and which machine falls in which class. The classes, not the individual machines, are what architecture needs.

### Phase 3 · Products, Unlock Map, Skill, Slice
**Why here:** once the recipe and machine shapes are fixed, you can say what the player gets (15), how and when they get it (14), and how the skill paces it (05). Doc 11 then cuts a thin end-to-end slice from those three.
**Order inside:** 15 before 14, because unlocks are paced around the products players care about. **Doc 18 (Laboratory)** comes after 05 and before the slice: its floorplans need doc 13's machine footprints and adjacency rules, and the slice includes Lab T1 (Charles moved it before the gate on 2026-10-13).

### Phase 4 · UX and Multiplayer/Saves/Compat
**Why last before the gate:** both are cross-cutting. They can only list every piece of state and every screen once the systems above have stopped moving. Doc 17 is where hidden architecture costs surface (sync, uninstall safety, SVE map edits).

### The gate
Docs 01, 03, 03a, 03b, 04, 05, 07, 11–18 are "ready for architecture". Then doc 09 is rewritten as the architecture plan, followed by execution planning (milestones, build batches under the build gate).

### New requirements from the guiding answers (doc 01, 2026-10-13)
- **Doc 12** must support content packs adding **Primes**, substances and recipes (the Tier 1 grid is derived from the Prime list), a **hand-made-only** flag, and condition counts that grow by tier.
- **Doc 13** checks footprints against the late-game target: every endgame setup fits in four T3 Labs with room to spare.
- **Doc 16** budgets hand actions so a heavily invested mid-game day stays near a quarter of the player's time.
- **Doc 17** adds Alecto the Witch, Ridgeside Village and East Scarp to the test matrix, plus a compatibility check for every visible world change and hidden area.
- **Doc 20** (after the gate) grows to cover every vanilla and SVE villager's alchemy dialogue, gift tastes and events.

### What deliberately waits
Capstones, the cutscene and content script, the art list, and all balance numbers. Architecture reserves hooks for them: a capstone flag table, event triggers as data, and numbers in data files the sweep can rewrite.

---

## Part 4 · Decisions (answered by Charles, 2026-10-13)

All Part 4 items are answered. Each is recorded below and applied to the doc it affects.

### Phase 0 · Scope lock
- **S1 · First playable slice:** ✅ Spring Y1 end to end: the intro cutscene, the first workshop room, decomposition, the Experiment Table, 4–6 apparatus (engineered and minor arcane), the codex and the Satchel.
- **S2 · Arcane logistics:** ✅ moved to 2.0, and chaining is built so arcane logistics can **replace or extend** it later. 1.0 gets **directional chaining** between adjacent ValleyOpus machines instead (doc 04): inputs come from the West machine first, then a South chest, then Automate; outputs go to the East machine first, then a South chest, then Automate.
- **S3 · Uninstall safety:** ✅ confirmed (2026-10-13): the save loads and plays without the mod, and mod content is lost, plus a "prepare for removal" command. Without the mod, **Labs simply disappear** (no Shed conversion, so Lab sizes are a free design choice), and the **workshop entrance behind the CC simply does nothing**.
- **S4 · Localization:** ✅ i18n from the first build.
- **S5 · Docs location:** ✅ done (2026-10-13): the docs live in the `CharlesDR/ValleyOpus` repo under `docs/`, which is now their source of truth.

#### S3 · "Safe to uninstall": pros and cons
"Safe" here means a save made with ValleyOpus still loads and plays after the mod is removed. Mod items and machines are lost (or show as Error Items), but nothing crashes or corrupts.

**Pros**
- Players can try the mod without risking their farm, which lowers the barrier to installing it and protects the mod's reputation on Nexus.
- Items stay vanilla object types (`Data/Objects`, `Data/BigCraftables`), so Automate, chest sorters, Lookup Anything and CJB tools all handle them with no extra work.
- Saves stay plain vanilla data plus `modData` and mod-owned save files. That is simpler to sync in multiplayer and cheaper to migrate to 1.7.
- It forces clean state boundaries (codex, discovery, bonds and staged rituals in mod save data), which also makes save migration between ValleyOpus versions easier.

**Cons**
- No custom C# item or object subclasses. Behaviour that would naturally live in a subclass (the Satchel, staged altars, the Ritual Circle) has to sit in C# handlers keyed off `modData`. That is somewhat more code and more care.
- The **Satchel's contents** must live outside the item, for example in a 1.6 global inventory keyed by the satchel's ID. Without the mod, those contents are orphaned, so an uninstall needs a helper that empties satchels into a chest first. *(Doc 03b v0.8 says the item's `modData` instead; doc 17 settles it, see doc 10.)*
- **Custom buildings and locations** (the Laboratory, the workshop under the CC) need testing to confirm how 1.6 handles them once their data is gone. Mitigation is part of the same uninstall helper. *(Superseded for Labs by the S3 answer above: they simply disappear, with no conversion to Sheds.)*
- Some rich features get harder: anything that stores complex state on a placed object has to serialize into `modData` strings.
- Ongoing test cost: every release adds a "remove the mod and load the save" check.

**Recommendation:** go with safe-to-uninstall, defined as "the save loads and plays, mod content is lost", plus a console command (and a GMCM button) that prepares a save for removal by emptying satchels, unstaging altars and emptying Labs into chests (Labs then disappear; no conversion, per the S3 answer). That costs a little architecture now and is very hard to add later.

### Phase 1 · Recipe & Process Model
- **R1 · Quality model:** ✅ colour stages are exactly vanilla quality.
- **R2 · Experiment Table inputs:** ✅ two at first, three after a workshop unlock. **Architecture requirement:** a recipe's input count is data, not a fixed shape, so four or more inputs can be added later without rework.
- **R3 · Recipes:** ✅ fixed for every farm, except the per-farm secret recipe.
- **R4 · Primes:** ✅ carried items, kept in the Satchel.
- **R5 · Golem Clay:** ✅ dropped. It was only an "automation capstone candidate" with no spec. That niche is already covered by Perpetuum Mobile (the Mechanist capstone candidate) and the late-game blended machines, so it needs no replacement.

### Phase 2 · Apparatus and Discovery
- **A1 · Machine count:** ✅ keep the proposed set of *operations*, but cut down the number of **BigCraftables** behind them. Doc 13 should route operations through combinations and arrangements of a smaller machine set (a shared vessel plus a heat source, catalyst or neighbour) rather than one machine per step. **Doc 13 goal:** a 1.0 machine count well below the ~20 proposed, with every operation still reachable.
- **A2 · Condition variants:** ✅ separate codex entries only for named products; otherwise a quality or yield bonus.

### Phase 3 · Products, Unlocks, Skill, Slice
- **P1 · Product families:** ✅ consumables, fertilizers, combat preparations, sellable compounds and gifts in 1.0; tool and weapon forging in 2.0.
- **P2 · Buffs:** ✅ vanilla buff stats plus 2–3 custom effects. The effect system must be **extensible**, since more custom effects will come up during the build. *Update (Charles, 2026-10-16): the 2–3 cap is suspended until the effect list's scale is settled; doc 15 §3a's approved catalogue is the 1.0 list.*
- **P3 · Bundle currency:** ✅ both vanilla items and mod substances, with early bundles all vanilla.
- **P4 · Skill curve:** ✅ vanilla speed. **Level 10 does not mean the player has discovered everything.** Recipe discovery, the codex, workshop restoration and NPC hearts are progression tracks separate from XP.

### Phase 4 · UX, Multiplayer, Compatibility
- **U1 · Controller and split-screen:** ✅ required for 1.0.
- **U2 · Integration targets:** ✅ widened (Charles, doc 01): **SVE, Alecto the Witch, Ridgeside Village and East Scarp** in 1.0; others in 2.0, ordered by the Nexus export (doc 10).
- **U3 · Config:** ✅ hints, celebrations **and economy knobs**. Player choice matters more than preventing exploits through options. Balance targets (doc 06) apply at default settings.

### Deferred items with new direction
- **Laboratory tiers (Charles, 2026-10-13):** three buildings. *Update 21:47: T3 is 50% larger than T2; every tier gets aesthetic touches and a unique floorplan that encourages creative automation; Lab design moves before the gate as doc 18.*
  - **T1** matches the base Shed's footprint and interior, and **T2** matches the Big Shed's. **T3** is larger again, and more expensive.
  - T1 and T2 give **small but escalating bonuses** to apparatus inside. That is enough reason to choose a Lab over a Shed for alchemy, but not enough to replace Sheds for anything else.
  - T3 gives a modestly stronger version of the same bonus, **plus Familiar assignment**. Assigning a Familiar is the lateral specialization, so three buildings give the variety of more. One Familiar per Lab; a Familiar can be assigned to only one Lab.
  - *Update (Charles, 2026-10-18):* T1 and T2 are each 3 tiles wider than the Shed and Big Shed, for a central aisle. Doc 18 (v0.2) owns the Lab design.
- Still deferred: capstones, the cutscene spec, the skill's display name, Lark's family, Hesper's outdoor spot, and all balance numbers (listed in doc 10).

---


## Change log
- **v0.5 (2026-10-18):** docs cleanup sweep: doc inventory shows each doc's current state; progress line on the path; S3's Lab conversion and Satchel storage notes marked as superseded or open; P2's suspended cap and the wider Lab tiers noted; doc 00's own v0.3 and v0.4 change-log entries moved out of the template example.
- **v0.4 (2026-10-13):** doc 18 (Laboratory) moved before the gate, with Charles's size and floorplan direction.
- **v0.3 (2026-10-13):** requirements from Charles's guiding answers in doc 01; S5 done; U2 widened to four tie-in mods.
- **v0.2 (2026-10-13):** Charles's answers to Part 4 recorded, with the S3 pros and cons and the new Lab tier direction.
- **v0.1 (2026-10-13):** first draft: template, doc inventory, critical path and decision list.
