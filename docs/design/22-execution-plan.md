# 22 · Execution Plan

Status: **DRAFT v0.4a** (2026-10-28) · Readiness: the plan for build 1, approved (PR #20 merged); B1 and B2 are done, B3 waits for Charles's yes
Depends on: 00, 09, 10, 11, 17 · Feeds: every build batch, `TESTING.md`, the to-do list and ideas backlog

## Purpose
The architecture gate is passed (doc 09 v1.0). This doc turns doc 09's plan and doc 11's slice into work that can be released one batch at a time under Charles's build gate: the milestones up to **build 1** (the first playable slice, doc 11), what each batch contains and how it is tested, the order the doc 09 §14 spikes run in and what happens when one fails, and how items from the to-do list and the ideas backlog get pulled into a batch. It adds **no gameplay rules and no architecture**: where it disagrees with an owning doc, the owning doc wins and this doc is corrected.

## Decisions locked
- **Build gate** (Charles, 2026-10-13): no branch, commit or PR for a build without Charles's explicit yes for that build. Bug reports and requests are diagnosed and answered only; any change they need becomes a to-do item. Builds are released in batches.
- **Plan first** (Charles, 2026-10-19): this plan is written and reviewed before any code or scaffolding.
- **Architecture** is doc 09 v1.0: the game-free `ValleyOpus.Engine`, the SMAPI mod as the adapter, `ValleyOpus.Tests` and `ValleyOpus.Tools`; CI builds and tests only the game-free projects, because the game's assemblies can't be redistributed (doc 09 §2).
- **Vanilla first, then widely used frameworks** (Charles, 2026-10-19; doc 09 §1a): a batch that adds a subsystem adds its doc 09 §1a row first.
- **Testing rules** (Charles, 2026-10-18; doc 11 §6): unit tests and logging from the first commit; at most four test configurations per build; configuration 1 runs chaining only, with no Automate; SVE installed is a test configuration from the first test build; every test build ships `TESTING.md`.
- **Co-op, split-screen and controller** (Charles, 2026-10-19; doc 17 §8b): groundwork built and code-reviewed in every batch; in-game runs deferred until Charles can run them; still 1.0 requirements.
- **Slice saves carry forward** (Charles, 2026-10-18): from the first build with fixed IDs, every save migrates into the next build (doc 17 §5).
- **D23 · Compiling** (Charles, 2026-10-19): a Remote Control session on Charles's computer compiles and zips each test build.
- **D24 · Three batches to build 1** (Charles, 2026-10-19): foundation and spikes, the bench and the line, the workshop (§3).
- **Spikes run before the code that depends on them**, and S11 (sort keys) runs before any item ID is fixed (doc 09 §11, §14).

## Player-facing spec
None. Players meet the result as builds; testers meet it through each build's `TESTING.md`.

## Rules & systems

### 1 · Terms
- **Batch:** one unit of work Charles releases with one yes. It ends in one PR (two when review size calls for it, for example engine and content) and, in one **test build**.
- **Test build:** one zip Charles installs and plays from `TESTING.md`, holding the C# mod and `[CP] ValleyOpus`, made on his PC after compiling (Charles, 2026-10-20). Named `0.1.0-alpha.N`; build 1 itself ships as `0.1.0`. Each test build is a git tag on `main`. Sink-or-swim trial builds (§3a) are named `1.0.0-sos.N` and stay on their own branch.
- **Milestone:** the state a batch reaches, stated as what Charles can do in game at the end of it.
- **Build 1** is the first playable slice of doc 11: Spring Y1 end to end, walked through its finish line.

### 2 · Who does what
| Step | Cloud thread | Charles |
|---|---|---|
| Propose a batch (scope, test configs, spikes, to-do and backlog items pulled) | posts it to the coordinator, who puts it on a card | says yes, trims or defers |
| Engine, tests, tools, content packs, CI | writes them; CI green before review | reviews the PR |
| The SMAPI mod project | writes it; compile-checks it in the cloud against game DLLs copied from Charles's PC (since 2026-10-22; never committed) | a Remote Control session on his computer compiles and packages it (**D23**, below) |
| Spikes | desk-checks every spike it can from public sources first (SMAPI, Automate, SpaceCore and the wiki; §4) | runs the in-game half from `TESTING.md` and shares the SMAPI log |
| Playtest findings | diagnoses and answers; adds items to `todo.md` | plays, reports |
| Design changes found in play | a docs PR first, then the build | approves the docs PR |

**D23 · How the SMAPI mod gets compiled** (Charles, 2026-10-19): a Remote Control session on Charles's computer, in his checkout, runs `dotnet build` against his game install and writes the test-build zip, so he never builds by hand. Alternatives: Charles builds from a short checklist in `TESTING.md`; or reference stubs of the game API in the repo (rejected: fragile, and a licensing risk). **Since 2026-10-22** the cloud thread can also compile the mod against the game DLLs copied from his PC (first done for `sos.3`), which catches compile errors before the hand-off; the DLLs stay out of the repo and the zip, and the test build itself is still compiled and zipped on his PC.

The cloud environment also needs the .NET SDK so engine tests run before each push; batch B1 adds that to the environment's setup script (a setting Charles changes, with the steps in the PR).

### 3 · Milestones and batches to build 1
Three batches (Charles, 2026-10-19, **D24**). Every batch also carries: the doc 09 §12 code-review checklist (controller neighbour IDs, `PerScreen<T>` state, host-only farm-state writes, every Harmony patch on the doc 09 §14 list), i18n for every new string, `[ValleyOpus]` logging, and updated `TESTING.md`. A batch may land as two PRs when review size calls for it; one yes covers both.

| Batch | Milestone (what Charles can do at the end) | Test build | Test configs (doc 11 §6) | Spikes |
|---|---|---|---|---|
| **B1 · Foundation and spikes** | review a green CI run where the engine decomposes, matches recipes and validates fixture data; then install a tiny mod and run each spike from `TESTING.md` (nothing playable yet) | `alpha.1` | 1, 2, 3 | desk checks, then waves A and B in game (§4) |
| **B2 · The bench and the line** | decompose in the Athanor, fill the Satchel, try pairs at the Table, watch the codex fill, kindle an Athanor, run all six apparatus, chain a line, run it under Automate, use the slice's products, level the skill to 4; items come from developer commands | `alpha.2` | 1, 2, 3 | S15; S9 partial (50 apparatus under Automate); S5 and S14 if B1 left them open |
| **B3 · The workshop = build 1** | play Spring Y1 from the cutscene to the finish line, then remove the mod and reload | `0.1.0` | 1, 2, 3 (4 deferred) | S3 and S13 re-checked on real content |

**B1 · Foundation and spikes** (`alpha.1`; saves are throwaway)
*Foundation (no game code runs; usually the first PR):*
- Repository layout of doc 09 §2: the solution, `ValleyOpus.Engine`, `ValleyOpus.Tests`, `ValleyOpus.Tools`, the SMAPI project with its manifest (`CharlesDR.ValleyOpus`, SMAPI, Content Patcher and SpaceCore as dependencies), `content/` folders, `TESTING.md` skeleton.
- GitHub Actions: build the three game-free projects, run the tests, validate every shipped JSON file on every push.
- Engine first pass with tests over **fixture IDs only** (no real ID is fixed before S11): `Model`, `Registry` (with `FormatVersion` upgrades), `Validation` (the uniqueness rule with tag overlap, `NeighbourStates` checks), `Decomposition` (profile, √price units, largest-remainder split, exchange clamp, residue), `Resolver` (index, "does this item extend the tray?", exact match), `Conditions`, `Quality`, `Random` (the stable hash), `Codex` keys and `tried:` signatures, `State` (the envelope with a schema version, the migration runner, round-trip tests).
- The content-pack reader that resolves `{{ModId}}` and `{{i18n:…}}`, so CI reads packs exactly as the game will (doc 09 §4d).
- The desk checks of §4, written into `TESTING.md`'s spike table as "desk result".

*Spike build (the second PR, compiled on Charles's PC per D23):*
- A minimal SMAPI mod and a spike content pack with placeholder items, one console command per spike (`valleyopus_spike <id>`) that sets the case up and logs `PASS`, `FAIL` or `LOOK` (when only Charles's eyes can judge), and a step-by-step script per spike in `TESTING.md`.
- Spikes from waves A and B (§4). Each result goes into the spike table (doc 09 data shape: spike ID, build, result, fallback taken).
- **Exit:** every wave A spike has a result, so B2 can fix IDs and build the machine path. A failed spike takes its doc 09 fallback; if the fallback changes a rule, a docs PR lands before B2 is proposed.

**B2 · The bench and the line** (`alpha.2`; **saves carry forward from here**, schema version 1)
- **Log:** B2 started 2026-10-19 on Charles's yes and merged as PR #22 on 2026-10-21 (`alpha.2`). B1's results: every wave A and B spike passed except S3 (fixed in B2: Labs are stripped into save data on every save and restored after, doc 17 §6) and S16's Blacksmith bonus (a Harmony postfix on `valleyopus_metal_bar`, Charles's option A, doc 15 §6); S13 passed at Town 53,9 after failing at 52,7 with SVE.
*The bench:*
- **IDs and sort keys fixed** per S11's result (doc 09 §11), together with categories per S16. From this build on, every ID is permanent.
- The base CP pack with every slice registry row of doc 11 (8 Primes, 46 objects, 41 resolver recipes, 6 apparatus rows, conditions, tag weights and category defaults for Spring's items, Aether weights by source rarity) and their authored `Data/Objects` and `Data/BigCraftables` rows; the data compiler (doc 09 §4c); the data test fails CI on any validator finding.
- Adapter: `Content`, `Catalog`, `Machines` (the staging layer and `OutputMethod`), `Satchel` (global inventory, auto-collect, picker, the guard patch), the Lens, `Codex` pages (Primes, Tier 1, Tier 2, Apparatus, Unlocks) with silhouettes and `tried` marks, celebrations with clock freeze, `FarmState` and `Saves` with the envelope, `Net` messages for codex sync, `Diagnostics` (inspect, verbose, developer commands).
- Placeholder art per the art notes: the recoloured vial with its rune for Element icons, base-game or open-license sprites for apparatus and tools (doc 11 Content inventory).

*The line:*
- All six apparatus: the Experiment Table, Athanor, Alembic, Retort, Hermetic Vessel and Calcinator; kindling and *Heated*/*Cold*; the flower Bonus; the darkness GSQ with the test fixture pack (doc 11 §3, §4); recipe memory, pin and locked outputs; quality.
- `Routes`: directional chaining behind `IItemRoute` and the minimal chain overlay; Automate needs no code (D25); it is simply absent in configuration 1.
- `Skill`: the SpaceCore skill, every XP source, levels 1–4, banking past 4.
- `Effects`: Sal Volatile and Vigor energy, Undine's Gift (the one custom buff handler), Caput Mortuum and Terra Viva fertilizer, Smaltum dye, derived prices.
- Performance: the resolver timing line in the log and a developer command that places 50 apparatus under Automate (doc 09 §8).

**B3 · The workshop** (`0.1.0`, build 1)
- `World`: the Town warp behind the CC (vanilla and SVE), the one-screen workshop map with the Entrance Hall and the Furnace Room's area in both states (doc 14 §2), Laboratory T1 at Robin's with hearth, alcove and the Lab Bonus.
- The bundle board (our own, D20) in vanilla's bundle style, showing only open rooms' bundles (doc 16 §10), the Furnace Room's three bundles, `Unlocks` with mail flags and recipe grants.
- Oriel, Tobin and the Curator, workshop-bound; the soft-start cutscene, the Furnace Room scene, Tobin's arrival, the Lab blueprint mail, the five-quest starter chain, the Cyprian journal pages and the slice hints. **Draft scripts** are written for this batch (doc 11's open question; doc 20 replaces them later).
- GMCM page, the slice report CSV, "prepare for removal", the full `TESTING.md` (setup per configuration, log locations, commands, low-confidence areas, bug template).
- **Done when:** the doc 11 finish line is walked on configurations 1, 2 and 3 (CC route on 1 and 2, Joja late install on 3); the uninstall test passes on configuration 1; an `alpha.2` save carries into `0.1.0` (the first migration check); CI is green and the validator reports zero findings; the spike table has every build-1 spike filled.

**D24 · How many batches before build 1** (Charles, 2026-10-19): three, as above. The five-batch alternative (foundation, spikes, bench, line and workshop each on their own) gave smaller test builds but more yeses and test sessions.

### 3a · Sink-or-swim trial builds (Charles, 2026-10-20)
Beside the three batches, Charles asked for a **full 1.0 trial build** played sink-or-swim: every system at once, built from the B2 code on its own branch (draft PR #23, `1.0.0-sos.N`), and rewound to `alpha.2` if it goes poorly. Each trial build still needs Charles's yes. Its findings go to the to-do list like any playtest's, and design changes reach the owning docs through a docs PR (§5 rule 4). B3 is unchanged and still waits for its own yes.
- **Log:** `sos.1` (draft PR #23, 2026-10-20) was played; `sos.2` (same branch, 2026-10-20) builds in Charles's feedback from that round and was installed from its zip on his PC. The design findings were filed in the 2026-10-21 docs sweep. `sos.3` (draft PR #25, stacked on #23, 2026-10-22) is an art and look-and-feel pass over `sos.2`'s gameplay, waiting for Charles's play-through.

### 4 · Spike order and fallbacks
Every spike gets a **desk check** first (public docs and the open-source code of SMAPI, Content Patcher, Automate and SpaceCore), which often settles it or narrows what Charles has to look at. The **in-game check** then runs in the earliest test build that can hold it. Fallbacks are doc 09 §14's; none forces a redesign.

| Wave | Spike | Desk check (B1) | In game | If it fails |
|---|---|---|---|---|
| **A** · blocks IDs and the machine path | **S11** sort keys | the open-source sort mods' code (what they compare) | B1: placeholder items in each candidate scheme, sorted by vanilla Organize and the sort mods in the test configs; checked in English and one other language | a spaced prefix in the display name's sort, or a custom category per group |
| | **S16** categories and their professions; dye tag names | wiki item and category data | B1, with S11 (they decide the same fields) | categories chosen by the result; tags corrected in data |
| | **S1** staging calls the output path; `OutputMethod` returns our match | the `Data/Machines` docs | B1: one placeholder apparatus | set the held object and minutes ourselves |
| | **S19** vanilla `ReadyTimeModifiers` on top of our minutes | the `Data/Machines` docs | B1, same apparatus, with a test modifier | apply the rule's modifiers ourselves |
| | **S2** Automate and our machines: **decided as D25** (its generic handler runs them); in game, its loads reach the staging layer and locked outputs never reach its chests | Automate's source | B1 in config 3 | move locked outputs out at `OutputMethod` time; chaining-only multi-input lines |
| **B** · blocks world, saves and storage | **S3** missing building dropped cleanly; Error Items become machines on reinstall | SMAPI and wiki notes on missing data | B1: a placeholder building, removed and restored; re-checked in B3 on the real Lab | failed in B1; replaced by stripping Labs into save data on every save (doc 17 §6) |
| | **S12** global inventories sync per player; hidden from chest mods | SMAPI multiplayer docs; Chests Anywhere's and Convenient Inventory's source | B1 (solo part); co-op part deferred (doc 17 §8b) | guard the key through those mods' APIs or config |
| | **S13** the Town warp with and without SVE | SVE's Town map edits, CC-opening event ID, Quarry bridge and bus flags (doc 10 upcoming tasks) | B1 in configs 1 and 2; re-checked in B3 | a second entrance on the workshop's own map edge |
| | **S17** Robin's menu order with SVE | SVE's building list | B1 in config 2 | accept the order; note it in the guide |
| | **S5** the hold-up pauses the clock in single player | wiki | B1 | a single-player-only postfix (Harmony patch 6) |
| | **S14** SpaceCore registration, level-up, banking past L4, Mastery, Walk of Life | SpaceCore's source | B1 | our own cap on displayed levels |
| | **S15** custom buff handler on `Data/Buffs`; fertilizer queries; other fertilizer mods | wiki; popular fertilizer mods' patches | B2 with real products | per-tick buff checks; patch priority and a note |
| **C** · measured on real content | **S9** overlay and tooltip frame time | — | B2 (50 apparatus); a full T3 Lab waits for that build | lower the particle cap; arrows for the hovered chain only |
| | **S6, S7** codex snapping, smallest split-screen viewport | layout reviewed in code | deferred with the controller and split-screen runs (doc 17 §8b) | explicit neighbour IDs; a compact layout |
| **After build 1** | **S20** special-orders board (Sorenson, Cistern) | vanilla special-orders code paths in the wiki | the Cistern build | our own small board over the same data |
| | **S4** Lab upgrade keeps placed objects (T2) | — | the build with Lab T2 | move objects across at upgrade |
| | **S10** rugs under BigCraftables (Circle Sigil) | — | the build with the Crystal Vault | a floor-placed Sigil beside the altar |
| | **S18** hover cost in large Labs | — | the build with T3 Labs | hovered-tile only (already the plan) |
| | **S8** | settled by D20 | — | — |

### 5 · Pulling to-do and backlog items into a batch
The to-do list (`todo.md`: fixes and tweaks) and the ideas backlog (`ideas/backlog.md`) are read fresh before every batch proposal.
1. **Fixes come first.** Every open to-do fix for a system already built is in the next batch by default. A fix for a system not yet built waits for the batch that builds it.
2. **Tweaks ride along when small.** A tweak joins the next batch if it touches only that batch's systems or ones already built; a bigger one is offered on the batch card as its own line.
3. **Backlog ideas need three things** before a batch can take them: Charles approved the idea, its owning doc already carries it (design before build), and the systems it needs exist in or before that batch. Ideas that pass are listed on the batch card; the rest stay where they are.
4. **Design changes go through the docs first.** A playtest finding that changes a rule becomes a docs PR, which Charles approves, before the build that implements it.
5. **The batch card** (posted by the coordinator, with an "Other / comments" option) lists: the milestone, the test configurations, the spikes, the to-do and backlog items pulled in, and anything deliberately left for later. Charles's yes on that card is the build gate for the batch.
6. **After merge,** each pulled to-do item is marked done with its PR; backlog items are only marked when their doc changes.
7. **Hotfixes.** A crash or save-loss bug in a test build may be fixed in a small out-of-order build (`alpha.N.1`), still with Charles's yes.

### 6 · Each batch, start to finish
1. The batch card gets Charles's yes.
2. One branch fresh from `main`; one PR (two at most) with the doc 09 §12 checklist and the attribution block.
3. CI green; the engine's tests and the data test pass; the PR description lists what changed in `TESTING.md`.
4. Charles reviews and merges; the test build is compiled and zipped (D23) and tagged.
5. Charles plays the configurations in `TESTING.md` and shares logs; findings go to `todo.md`, spike results into the spike table, rule changes into a docs PR.
6. The next batch card is drafted from this doc, the to-do list and the backlog (§5).

### 7 · After build 1 (sketch)
Planned in full once build 1's playtest closes. The order follows doc 14's room tiers, so each build is one season of a diligent Year 1:

| Build | Content | New spikes |
|---|---|---|
| 2 · Summer | Tier II rooms (Cistern, Library): Vat, Press, Boiler, Coil, the Table's third slot, Sorenson's contracts, Hesper; professions at level 5; the SVE integration pack | S20 |
| 3 · Fall | Tier III (Sanctum, Undercroft): Ritual Altar, Gloam Lantern, dark twins, Lab T2, the Witch | S4 |
| 4 · Winter | Tier IV (Observatory, Crystal Vault): Circle Sigil, ley nodes, Lab T3 and the Familiars | S10, S18, S9 in full |
| 5 · The Great Work | capstones (doc 19), the remaining tie-ins, villager integration (doc 20), commissioned art (doc 21), the balance sweep | — |

Villager dialogue and the balance sweep's tooling grow a little in every build rather than all waiting for build 5.

## Data shape
No registry or saved state. Two build-level records:

| Record | Where | Fields |
|---|---|---|
| Spike table | `TESTING.md`, per build | spike ID, build, desk result, in-game result, fallback taken (doc 09 data shape) |
| Batch log | this doc's change log | batch, PR, test build tag, to-do and backlog items pulled |

## Content inventory
None of its own; each batch's rows are doc 11's.

## Tech hooks
Doc 09 throughout. New here only: the `valleyopus_spike <id>` console command in the spike build, and the .NET SDK in the cloud environment's setup script.

## Balance hooks
The slice report (doc 11 §4) starts writing data in B2 and is complete in B3; the sweep's tooling grows from B1's engine (doc 09 §12).

## Open questions
Every open item across the docs is also collected in [doc 10](10-roadmap-and-open-questions.md#open-questions).
None open. D23 and D24 are decided (Decisions locked).

## Change log
- **v0.4a (2026-10-28):** docs cleanup sweep: the cloud now compile-checks the mod against game DLLs copied from Charles's PC (since 2026-10-22); D23 is unchanged, the test build is still compiled and zipped on his PC (§2); the sink-or-swim log adds `sos.2` and `sos.3` (§3a).
- **v0.4 (2026-10-21):** docs cleanup sweep: marked approved (PR #20 merged); the batch log adds B1 (PR #21, `alpha.1`, merged 2026-10-19) and B2 (PR #22, `alpha.2`, merged 2026-10-21); one zip per build (Charles, 2026-10-20); the sink-or-swim trial builds (§3a); B3's workshop and board follow Charles's one-screen workshop and vanilla-style board (docs 14, 16).
- **v0.3 (2026-10-19):** B2 docs update: B2 started on Charles's yes; B1's spike results logged (all wave A/B passed except S3 and S16's Blacksmith bonus).
- **v0.2a (2026-10-19):** S2 settled as D25 (Automate's generic handler runs our machines).
- **v0.2 (2026-10-19):** Charles decided D23 (a Remote Control session on his PC compiles each test build) and D24 (three batches: foundation and spikes, the bench and the line, the workshop); §3 and the spike table follow.
- **v0.1 (2026-10-19):** first draft after the architecture gate: five batches to build 1 (foundation, spike build, the bench, the line, the workshop), who does what, the spike order in three waves with desk checks first, the rules for pulling to-do and backlog items into a batch, and a sketch of builds 2–5. D23 and D24 await Charles.
