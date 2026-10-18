# 11 · First Playable Slice

Status: **DRAFT v0.2** (2026-10-18) · Readiness: not ready (Charles's review pending; no `[blocks-arch]` item open)
Depends on: 00, 02, 03, 03a, 03b, 05, 07, 12, 13, 14, 15, 18 · Feeds: 09 (the architecture plan), 16, 17, execution planning

## Purpose
The first build a player can install and play: **Spring of Year 1, end to end** (S1). It cuts a thin, complete path through every system the merged docs define, so the first build tests the two things that are hardest to fix later: whether the core loop is fun (decompose, experiment, feel clever, set up a first small line), and whether the riskiest architecture holds (the resolver behind `OutputMethod`, the staging tray, discovery locks, the shared codex, the Satchel, Automate and chaining, the workshop map, the Lab building, uninstall safety). Content is thin on purpose; **shape is complete**. Nothing in the slice is a throwaway: every system it ships is the 1.0 system, filled with Spring's rows. It serves pillar 4 (wonder first: Spring is hand work and discovery), the approachability pillar (the first evening must be playable and readable without a wiki) and doc 00's governing rule (prove shape early, fill content later).

## Decisions locked
- **S1 · Slice scope** (Charles, 2026-10-13): Spring Y1 end to end: the intro cutscene, the first workshop room, decomposition, the Experiment Table, 4–6 apparatus (engineered and minor arcane), the codex and the Satchel.
- **Chaining and Automate are in the slice** (Charles, 2026-10-18), so their technical risks surface in the first build (§1). They are **tested separately**: the first test runs chaining only, without Automate (Charles, 2026-10-18; §6).
- **Oriel, Tobin and the Curator are workshop-bound in the slice** (Charles, 2026-10-18): the cutscene, a few hint and reaction lines, time in the workshop; hearts, gift tastes and town schedules come later.
- **Slice saves carry forward** (Charles, 2026-10-18): save data has a schema version from day one and later builds migrate it (§5).
- **Defaults confirmed** (Charles, 2026-10-18): XP banks past level 4 with professions later, Oriel's starter quests (now a chain, §Player-facing spec), a slice for Charles and a few testers (not Nexus), darkness tested through a test pack.
- **Lab T1 is in the slice**, with its hearth and dark alcove (Charles, 2026-10-13; doc 18).
- **Everything already locked in docs 12, 13, 14, 05, 07, 15 and 18 applies unchanged.** The slice adds no rule of its own; where it differs from 1.0, it ships *fewer rows*, never a different behaviour.
- **i18n from the first build** (S4): every string is an i18n key, English only in the slice.
- **Safe to uninstall** (S3) and **multiplayer-safe** from the first build: both are cheaper to prove now than to retrofit.
- **Testing is built in** (Charles, 2026-10-18): at most four test configurations, unit tests and logging from the first commit, and a testing guide for Charles (§6).
- **SVE from the start** (Charles, 2026-10-18): the slice is tested with SVE installed from the very first build; nothing in it may assume vanilla maps or items only (§6).
- **Placeholder art** (Charles, doc 08 scope answers): reused vanilla sprites plus generated tinted icons; licensed or commissioned art is a separate track.

## Player-facing spec

### What the slice is, in one paragraph
A new (or existing) save reaches the soft start, meets Oriel, and leaves the ruined workshop that evening with a Reagent Satchel, an Assayer's Lens, an Experiment Table, an Alembic, a Retort and Oriel's old Athanor. Over Spring the player decomposes their forage, crops, fish and mine finds into the eight Primes, works through the Tier 1 grid at the Table, learns that an Alembic distills beside a kindled Athanor, restores the **Furnace Room** with three vanilla bundles, welcomes **Tobin** and his **Calcinator**, builds **Laboratory T1** with Saxum they made themselves, and runs a first small line of apparatus inside it, by hand, by chaining or with Automate. By the end of Spring they have most non-Aether Tier 1 cells, a first Tier 2 compound or two, about Alchemy level 4, and a board showing the Cistern and Library waiting.

### The finish line
The slice is "played through" when all of these are true. They are the slice's acceptance path, and the build's test plan walks them on both routes:
1. The soft-start cutscene played, and the intro gifts are in the player's hands.
2. The player has decomposed an item and made ten Tier 1 discoveries. *(v0.1 said "filled a Tier 1 row", but every Prime's row includes its Aether cell, so a row needs Aether; §1a.)*
3. The Furnace Room is restored, its scene played and its unlocks granted.
4. Tobin has arrived and the Calcinator recipe is learned.
5. A Laboratory T1 is built and at least one apparatus inside has run a recipe with the Lab bonus.
6. A discovered recipe has run unattended, by chaining or Automate.
7. One Tier 2 compound has been discovered.

### The first evening (the most important ten minutes)
- **The Encounter:** 2 days after the CC opens, the first Town entry between 6:00am and 3:00pm (doc 14). Oriel's short cutscene, the walk behind the CC, the Entrance Hall. The clock is frozen throughout; the scene is skippable after the first view.
- **The gifts:** Satchel, Lens, Experiment Table, one Alembic, one Retort (each with its recipe) and one Athanor (gift only; its recipe comes with the Furnace Room).
- **The starter chain** (Charles, 2026-10-18): a walkthrough, not a to-do list. Each quest appears in the vanilla quest log only when the one before it is done, and its reward is **material for the next step**. Oriel gives the first four, Tobin the last:
  1. *"Take it apart"* (given in the cutscene): decompose any item in the Athanor. Teaches the Satchel filling itself. **Reward:** 4 Ignis from Oriel's own jar, enough to try Ignis + Ignis at the Table and still kindle the Athanor afterwards.
  2. *"Two become one"*: make any discovery at the Experiment Table. Teaches the anticipation beat and the first toast. **Reward:** 4 Aqua, for the next quest's distillation.
  3. *"A living flame"*: kindle the Athanor (Ignis + Ignis) and distill anything in an Alembic beside it, for example Aqua + Aqua into Ros Maialis. Teaches heat and neighbour states, the slice's biggest new idea. **Reward:** 5 Coal, a slot of the Furnace Room's *Kindling* bundle.
  4. *"The Furnace Room"*: restore the room's three bundles at the Curator's board. **Reward:** 10 Refined Quartz toward Laboratory T1.
  5. *"A room of your own"* (Tobin, the day he arrives): build Laboratory T1 and run a recipe inside it. **Reward:** 10 Coal, fuel for his Calcinator and the first line.
  Nothing in the chain gates anything. If a quest's goal is already met when it appears, it completes at once and the next one follows, so a player who rushes ahead never waits on the log. Reward sizes are placeholders for the sweep; none of them may fill a whole bundle on its own.
- **The Curator** appears once, half-seen, at the board. The mystery starts here (doc 08); he says almost nothing in the slice.

### The Spring loop
| Step | What the player does | Systems it proves |
|---|---|---|
| Gather | play Spring as usual: forage, crops, fish, the Mines to about 40 | nothing new; the mod rides on ordinary play |
| Assay | point the Lens at items, the board, the workshop's ruined fixtures | `assay:` keys, profiles in tooltips, place hints |
| Decompose | feed items to the Athanor (slow, top two Primes, little residue), later the Calcinator (fast, coal fuel, more residue) | the decomposition engine, route modifiers, residue, the Satchel |
| Experiment | try pairs at the Table; dead ends give residue and a "tried" mark | the resolver, Table outcomes, `tried:` keys, celebrations, XP |
| Arrange | kindle the Athanor (Ignis + Ignis) and set an Alembic beside it, or use the Lab's hearth; put Spring flowers within range 2 of an Athanor or Retort for a quality Bonus | neighbour states (*Cold* / *Heated*), power state, Bonus conditions, `effect:` keys |
| Run | load apparatus with the Satchel picker; set a remembered recipe; place a second machine to the East | the staging tray, recipe memory and pinning, locked outputs, chaining, Automate |
| Restore | fill the Furnace Room's bundles; watch the scene; Tobin arrives the next day | bundles, unlocks, mail flags, restoration scenes, NPC arrival |
| Build | buy Laboratory T1 from Robin (with 10 Saxum) and move a line inside | `Data/Buildings`, the Lab interior, hearth and alcove tiles, Lab Bonus |
| Use | drink Sal Volatile or Vigor for energy, Ros Maialis for Undine's Gift; spread Caput Mortuum or Terra Viva as fertilizer; ship Calx and Smaltum; dye with Smaltum | the effect registry (vanilla and one custom handler), fertilizer queries, derived prices |

### What the player sees at the edges of the slice
- **The bundle board** shows all seven rooms. Rooms past the Furnace Room show their name and a sealed door with the line *"Sealed. (Not in this build.)"*, so the board's final layout is tested now.
- **The codex Unlocks page** lists Tier II unlocks as silhouettes with their sources, marked the same way.
- **Aether** is rare, as doc 14 §7 intends, and its yield follows how common each source is (Charles, 2026-10-18). **Solar and Void Essence** are common monster drops, so they decompose mostly into their own Primes with only a trace of Aether (placeholder: about 1 Aether per essence). **Prismatic Shards and legendary fish** are rare, so they decompose mostly into Aether, and their price (1,000g and up, against 40–50g for an essence) already gives them several times the units under the √price yield. Placeholder ratio: one Prismatic Shard gives about as much Aether as 15–20 essences. That is enough to discover an Aether pair or make the odd Vigor or Terra Viva in Spring, not to run them. The weights are `Decomposition/TagWeights` rows (doc 12 §2), so the sweep tunes them without a shape change.
- **Alchemy level 5:** the slice ships no professions. XP keeps banking past level 4; a player who reaches the level 5 threshold sees "Your next level arrives in a later build", and the level and its choice apply when the build with professions is installed.

### Failure states
As in 1.0: an undefined Table pair gives residue, a little XP and a "tried" mark; apparatus refuse items that can't lead to a recipe they run; locked outputs sparkle and wait. Nothing in the slice can destroy an item without giving residue or XP.

## Rules & systems

### 1 · In and out
| System | In the slice | Out (later builds) |
|---|---|---|
| **Primes** (doc 03a) | all 8, as items | — |
| **Decomposition** (docs 03, 12 §2) | the full engine: tag weights for every Spring-obtainable vanilla item, category defaults for everything else, overrides, the equivalent-exchange clamp, the Aether source list weighted by rarity (above). **SVE items decompose from day one** through their context tags and category defaults, with no SVE-specific rows yet | SVE, RSV, East Scarp and Alecto **integration overrides** (hand-tuned profiles and late-area reagents); the SVE integration pack is out, SVE itself is a test configuration from the start (§6) |
| **Tier 1 grid** | **all 36 rows** *(default)*: 33 items plus 3 Power substances (Flamma Viva runs the Athanor; Vapor and Fulgur are Table reveals with a puff, since no Boiler or Coil exists yet) | — |
| **Tier 2** | 4 rows: **Vitrum**, **Terra Viva**, **Vigor**, **Smaltum** | the other ~21; Pulvis Pyrius and anything needing three inputs |
| **Dark twins, metals, Tier 3** | none | Undercroft, Quarry Crucible, Great Work |
| **Experiment Table** | 2 slots; Table outcomes 1–3 (doc 07); neighbours respected | the third slot (Library) |
| **Apparatus** | **6**: Experiment Table; Athanor, Alembic, Retort, Hermetic Vessel (minor arcane ✦); Calcinator (engineered ⚙︎) | Process Vat, Press, Boiler, Lightning Coil, Pelican, Ritual Altar, Circle Sigil, Quarry Crucible, Ley Tap; the Furnace and Keg appended rules |
| **Neighbour states** | *Cold* / *Heated* on the Alembic (heat from a kindled Athanor or a Lab hearth) | *Vapor*, *Fulgur*, *Crossed* (no sources yet) |
| **Conditions** | Required power (Flamma Viva, for kindling and heat), one Bonus adjacency (flowers by tag within range 2, on the Athanor, Retort and Hermetic Vessel), the Lab-tier Bonus, the darkness GSQ (built and tested, see §3) | weekday, weather, moon, ley node, catalysts, Shades |
| **Directional chaining** | **in** (Charles, 2026-10-18): West in, East out, South chest second, behind the item-route interface; a minimal chain overlay | the full overlay and teaching visuals (doc 16), Tobin's chaining lesson (Cistern) |
| **Automate** | **in** (Charles, 2026-10-18): our `IAutomationFactory` for every apparatus, recipe memory, refusal of locked outputs | — |
| **Codex** | pages: Primes, Tier 1 grid, Tier 2, Apparatus, Unlocks; silhouettes, "tried" marks, warm hints, lore lines at 1/10/50 | Dark twins, Metals, Great Work and Places pages (they exist but stay empty) |
| **Hints** | layer 1 (flavour, acceptance), layer 2 (grid warmth), layer 4 (Oriel, Tobin and the Curator, a few lines each), layer 5 (a handful of Cyprian journal pages in the Entrance Hall) | residue divination (Library), Lunar Dream, Hesper, Finch, Lark |
| **Celebrations** | all three tiers, the anticipation beat, clock freeze, feedback styles (arcane and mechanical) | Cauda Pavonis (first stage advance needs an altar) |
| **Satchel** (doc 03b) | full spec: auto-fill, tabs, picker, ≈24 types; the level 2 extra tab | Tobin's capacity upgrades |
| **Assayer's Lens** | items, places, fixtures; the level 4 whole-chest assay | — |
| **Skill** (doc 05) | SpaceCore skill, every XP source, the vanilla curve, levels 1–4 and their unlocks | professions (L5, L10) and their recipes; XP banks past L4 |
| **Workshop** (doc 14) | the Town warp behind the CC, the Entrance Hall, the Furnace Room (ruined and restored states), the bundle board with all seven rooms laid out | the other six rooms' bundles, scenes and map states |
| **Bundles** | the Furnace Room's three: *Kindling*, *Mine Salts*, *Spring Gleanings* (all vanilla) | the other 22 |
| **NPCs** | **Oriel**, **Tobin**, **the Curator** as workshop-bound characters (Charles, 2026-10-18): the cutscene, short daily presence in the workshop, a few hint and reaction lines; no hearts, gifts or schedules outside the workshop | full villager treatment for all three; Finch, Ivo and every other arrival |
| **Unlocks** (doc 14 §1) | the Intro and Furnace Room rows, with every grant; Finch's and Ivo's unlock flags are set but nothing appears yet | every later row |
| **Laboratory** (doc 18) | T1 at Robin's: cost, interior map, hearth and alcove, Speed +10% and the 5% step chance | T2, T3, Familiars |
| **Products** (doc 15) | energy (Sal Volatile, Vigor), **Undine's Gift** weak tier (Ros Maialis), fertilizers (Caput Mortuum, Terra Viva), the blue dye (Smaltum), derived prices and the Artisan category; every other slice item is sellable only | Second Wind, Cleanse, Glow and the rest of §3a; trinkets, rings, combat items, gifts, contracts, the décor set |
| **Villager integration** | none | dialogue, gift tastes and events for vanilla and SVE villagers |
| **GMCM** | toast frequency, the Satchel HUD notice, celebration skip, and the `Decomposition/Settings` economy knobs | hint and economy knobs for later systems |
| **Console commands** | "prepare for removal"; the slice report (§4); developer commands (§4) | — |

**Why these six apparatus.** They are exactly what doc 14 gives a Spring player (intro plus Furnace Room), and together they offer every operation but Ritual: Calcination and decomposition (Athanor, Calcinator), Dissolution and Distillation (Alembic), Separation and Coagulation (Retort), Conjunction and Fermentation (Hermetic Vessel), Combination (the Table). Every Item-class Tier 1 recipe except Quinta Essentia and Mercurius Philosophorum (Table and altar only, doc 03a) therefore runs on a slice apparatus, so the slice tests the full minor-arcane class. The Calcinator is the one engineered machine, which is enough to test the engineered feedback style, fuel as an `AdditionalConsumedItems`, and a mixed line.

**Why chaining and Automate are in.** Neither is gated by an unlock in doc 13 or doc 14: Tobin's lesson teaches chaining, it doesn't switch it on. Leaving them out would make Spring play differ from 1.0, and they carry the resolver's biggest technical risks (doc 13 §6: Automate claiming our machines, polling cost, locked-output refusal). They ship in the slice so those risks surface in the first build.

### 1a · Critical-path check (Charles, 2026-10-18)
Each finish-line step, checked against what the table above actually gives a Spring player (doc 01's internal-consistency rule, doc 14's critical-path rule):
| Step | Reachable with | Result |
|---|---|---|
| 1 · Cutscene and gifts | the soft-start trigger (doc 14) | ✓ |
| 2 · Ten Tier 1 discoveries | the 28 non-Aether cells, all Table-attemptable with two slots; Primes from Spring forage, crops, fish, stone, clay and Mines loot (Sal from minerals, Sulphur from sap and mushrooms, Mercurius from slime) | ✓ **fixed:** v0.1 asked for a full Tier 1 row, which needs Aether; the codex flourish "a Tier 1 row complete" stays as a milestone but is no longer on the finish line |
| 3 · Furnace Room | *Kindling* (Hardwood needs the copper axe, Coal, Sap), *Mine Salts* (Copper Bars, Frozen Tear and Earth Crystal by Mines 40), *Spring Gleanings*: all vanilla, all Spring | ✓ |
| 4 · Tobin and the Calcinator | the day after step 3 | ✓ **requirement:** the Calcinator's crafting cost must use only Spring materials (Stone, Copper Bars, Saxum) |
| 5 · Lab T1 with a recipe run inside | 25,000g, 400 Wood, 20 Refined Quartz, 10 Copper Bars and 10 Saxum (Terra + Terra Coagulation on the intro Retort) | ✓ for a diligent player, but **tight:** 25,000g in late Spring Y1 comes from ordinary farming, since mod products sit at or below vanilla. Logged as a pace risk; the slice report measures it, and the cost stays as doc 18 set it unless playtests show it out of reach |
| 6 · An unattended run | a discovered recipe, one hand load to set recipe memory, then two adjacent apparatus with a South chest (for example Athanor decomposing into an East Retort making Saxum); Automate in its own configuration | ✓ |
| 7 · A Tier 2 discovery | Vitrum (Pulvis + Calx) and Smaltum (Caput Mortuum + Calx) need no Aether and fit the two-slot Table | ✓ |
| The starter chain | Ignis, Aqua and Coal rewards cover quests 2–4; the Alembic's *Heated* state from a kindled Athanor | ✓ |
| Skill pace | about 75 XP a day from about day 7 gives level 4 by the end of Spring | ✓ **fixed:** v0.1 said "level 3–4" in one place and "near level 5" in another; level 4 at the end of Spring, 5 in early Summer (doc 05 §2) |

### 2 · The slice's recipe rows
Content for the build, listed here so the scope is countable:
- **Tier 1:** the 36 rows of doc 03a, each on the Table plus its listed ✦ and ⚙︎ kinds. Cells with no slice apparatus are made at the Table only: Vapor and Fulgur (⚙︎ plus Table) reveal with a puff and wait for the Boiler and Coil; Quinta Essentia and Mercurius Philosophorum (✦A plus Table) come out of the Table at Nigredo and wait for the altar.
- **Tier 2:** Vitrum (Pulvis + Calx, Calcination), Terra Viva (Limus + Vitae, Fermentation), Vigor (Spiritus + Ros Maialis, Distillation, so it needs heat), Smaltum (Caput Mortuum + Calx, Calcination). Vitrum and Smaltum need no Aether, so every player sees a Tier 2 discovery in Spring; Terra Viva and Vigor reward a player who found an Aether source.
- **Kindling:** Ignis + Ignis on the Athanor sets its permanent Flamma Viva state (doc 13 §3).
- **Neighbour-state pairs on the Alembic:** every Dissolution recipe lists *Cold*, every Distillation recipe lists *Heated*, and no row lists both unless it means it (doc 13 §1a). This is the slice's test of the uniqueness validator.

Every row follows doc 12's shape exactly; the validator runs on the slice data with zero errors as a build requirement.

### 3 · Darkness in the slice
No slice recipe needs darkness: putrefaction makes only the dark twins, which are out. The darkness GSQ and the Lab alcove tiles still ship, because doc 18's alcove is part of the T1 map, and the condition is tested by a **test content pack** (§4) that adds one fixture recipe needing it. Where darkness holds outside a Lab is an open doc 12 follow-up (todo list); the slice only needs the alcove.

### 4 · Build tooling the slice needs
- **The slice report** (console command): discoveries per day, XP per day by source, Primes made per source, residue made and spent, gold earned from mod items, hand actions per day (loads, collections, Table attempts). It reads the save and writes a small CSV, so playtests feed doc 06's sweep and doc 05's pace targets (about 75 XP a day in Spring).
- **Developer commands**, off by default: grant an unlock, mark a recipe discovered, reset the codex, spawn Primes, jump to the soft start. They exist for testing and for players recovering a broken save.
- **A test content pack**, never shipped to players: fixture recipes that exercise rules no slice row uses (darkness, a three-input recipe on a two-slot Table, a deliberate collision the validator must reject, a pack-added Prime). It proves the content-pack promise (doc 01) in the first build.

### 5 · Edge cases
- **Multiplayer:** the slice must run in two-player co-op and in split-screen. Unlocks, bundles and the codex are farm-wide (host save data, mirrored mail flags); each player has their own Satchel; a farmhand's discovery reaches the host and is broadcast; toasts show to the discoverer and a quieter notice to others; clock freeze becomes a non-blocking overlay when farmhands are connected (doc 07; verified in the slice).
- **Joja route:** the soft start fires before any route choice; the workshop stays reachable behind the warehouse. The test plan walks the finish line on both routes.
- **Late install:** adding the slice to an existing save fires the soft start on the next eligible Town entry, whatever the season (doc 14 §9). No fast start.
- **SVE:** no integration pack in the slice, but **SVE is a test configuration from the first build** (§6), so nothing in the slice may assume vanilla-only maps or items: the Town warp behind the CC must stay reachable and must not overlap SVE's Town edits, SVE items must decompose through tags and category defaults without errors, and SVE's own events must not collide with the soft start (doc 17 test items, brought forward).
- **Controller:** every slice menu (Table, Satchel grid, picker, codex, bundle board) is playable with a gamepad (U1).
- **Uninstall (S3):** the slice test plan removes the mod from a played save and loads it: the Lab disappears cleanly, the workshop entrance does nothing, apparatus become inert items, no crash. "Prepare for removal" empties Satchels and staging trays into a chest and offers to empty Labs first. This is doc 09's "test early" item for building removal.
- **Save carry-forward** (Charles, 2026-10-18): slice save data carries a schema version from day one, and later builds migrate it, so a slice save continues into the next build. That tests migration early (doc 17 owns the mechanism).
- **Festival days:** a soft start, restoration scene or Tobin's arrival due on a festival moves to the next day (doc 14 §9).

### 6 · Testing (Charles, 2026-10-18)
**Test configurations.** No more than four, each covering the combination that matters most:
| # | Mods | Play | Route | What it covers first |
|---|---|---|---|---|
| 1 | slice only | solo, new save | CC | the core loop, **chaining only** (no Automate), the finish line, then remove the mod and reload (uninstall) |
| 2 | slice + **SVE** | solo, new save | CC | maps (Town warp), SVE items decomposing, event collisions; runs from the first build alongside test 1 |
| 3 | slice + **Automate** | solo, late install on an existing Spring save | Joja | Automate claiming our machines, locked outputs, polling cost, late-install soft start, the Joja route |
| 4 | slice + SVE + Automate | two-player co-op, plus a split-screen session with a controller | CC | codex sync, farmhand discoveries, clock freeze with farmhands, per-screen menus, carry-forward into the next build |

**Unit tests and CI.** The engine's pure logic is written without game types where practical, so it runs in a plain C# test project: decomposition (profiles, √price yield, largest-remainder split, the exchange clamp, Aether weights), recipe matching and the uniqueness validator, neighbour states, quality modes and clamps, discovery keys and `tried:` signatures, and save-schema migration. The tests run on every push (GitHub Actions) and on the slice's data files, so a bad content row fails before anyone plays it. Doc 09's architecture plan must keep these pieces separable.

**Logging.** One `[ValleyOpus]` prefix in the SMAPI log. *Info*: discoveries, unlocks, bundle completions, schema migrations. *Warn*: disabled content rows and validator findings. *Error*: anything caught, with the machine, tile, recipe and save day. A **verbose mode** (GMCM or console toggle, off by default) traces every resolver decision, chaining transfer and Automate call. An **inspect** console command prints the hovered machine's tray, recipe memory, pin, neighbour state, lock and power state.

**The testing guide** (`TESTING.md` in the repo, shipped with every test build) tells Charles:
- how to set up each configuration, and which build and mod versions it was checked with;
- **where the logs are:** Windows `%appdata%\StardewValley\ErrorLogs\SMAPI-latest.txt`, macOS and Linux `~/.config/StardewValley/ErrorLogs/SMAPI-latest.txt`; how to share one through smapi.io/log; where the slice report CSV is written;
- the console commands (report, inspect, verbose, developer commands) and when to use them;
- the **known low-confidence areas** to watch in game, updated each build: machines under Automate (does anything ever take a locked output?), many machines in one Lab (frame time), the Town warp with SVE, clock freeze with a farmhand connected, controller navigation in the Table and Satchel menus, removing the mod from a save with a Lab, and loading a slice save in the next build;
- a short bug-report template: configuration, save day, what happened, the log link.

## Data shape
The slice adds **no new tables**. It fills the assets other docs define with Spring's rows:

| Asset (owner) | Rows in the slice |
|---|---|
| `Primes` (12) | 8 |
| `Substances` (12) | 36 Tier 1 (33 items, 3 Power), 4 Tier 2, Caput Mortuum |
| `Recipes` (12) | 36 Tier 1 plus kindling, 4 Tier 2; their `NeighbourStates` (13) |
| `Conditions` (12) | Flamma Viva power, *Heated*/*Cold*, flowers within range 2, Lab tier ≥ 1, darkness |
| `Decomposition/*` (12) | tag weights for Spring's vanilla items and every category default; overrides only as playtests need |
| `Apparatus`, `Data/Machines`, `Data/BigCraftables` (13) | 6 apparatus |
| `Unlocks`, `WorkshopRooms`, `WorkshopBundles` (14) | Intro and Furnace Room unlocks; all 7 rooms plus the Great Work Chamber as rows (later rooms with no bundles, which the board shows as sealed); 3 bundles |
| `Effects` and fertilizer rows (15) | energy edibility on Sal Volatile and Vigor, Undine's Gift (weak), Caput Mortuum and Terra Viva fertilizer profiles, Smaltum dye |
| `Codex/Pages`, `Codex/Lore`, `Hints`, `Milestones` (07) | 5 pages; lore lines for slice items; about 40 hints; about 6 milestones (first discovery, first Tier 1 row (a milestone only), first Tier 2, first kindling, Furnace Room restored, first Lab) |
| `Data/Buildings` (18) | Laboratory T1 |
| Skill (05) | the SpaceCore skill, levels 1–4 unlocks |
| Save data (C#, host) | farm codex, unlocks, bundle progress, and a **schema version** (§5) |

**Inventory sort order** (Charles, 2026-10-18): the slice's 46 objects must sort, under vanilla Organize and the common sort mods, as the eight Primes first, then substances by tier (Tier 1, then Tier 2, with Caput Mortuum among the substances), then endgame Elements last (none in the slice yet), alphabetical within each group. Before the slice assigns categories, item IDs or tags, the build verifies what those sorters key on (category number, display name, item ID or context tags) and picks the field that yields this order in every language. This may conflict with doc 15's Artisan Goods category for sellable compounds; the item-data pass resolves it (todo list).

**Room in the ID sequence** (Charles, 2026-10-18): whatever ordering key the sort uses (an ID segment, a sort number or a category value), it leaves gaps between groups and subgroups, so later builds can add an item anywhere without reassigning existing IDs. Stardew 1.6 item IDs are strings (`CharlesDR.ValleyOpus_Calx`), and renaming one breaks saves, so if the order comes from the ID itself the scheme needs a gapped prefix (for example `10_`, `20_`), otherwise from a separate gapped sort field. The slice is the first build to fix IDs, and carry-forward saves (§5) make them permanent from then on.

**Out-of-build rows** are simply absent, never present-and-disabled, except the seven `WorkshopRooms` rows, which the board needs for its layout.

## Content inventory
- **Items:** 8 Primes, 33 Tier 1 items, 4 Tier 2 items and Caput Mortuum (46 objects), 2 tools (Satchel, Lens), 6 BigCraftables.
- **Recipes:** 41 resolver recipes; 6 apparatus and 2 tool crafting recipes (Table, Alembic, Retort, Athanor, Hermetic Vessel, Calcinator; Satchel and Lens recipes for replacements).
- **Locations:** the workshop (Entrance Hall, Furnace Room in two states, the board's sealed doors), the Town warp patch, the Lab T1 interior.
- **Building:** Laboratory T1.
- **NPCs:** Oriel, Tobin, the Curator (workshop-bound, with portraits and sprites reused or tinted from vanilla).
- **Events and mail:** the soft-start cutscene, the Furnace Room restoration scene, Tobin's arrival, the Lab blueprint mail; the five-quest starter chain. Scripts are drafts written for the slice; doc 20 replaces them later.
- **Readables:** about 5 Cyprian journal pages.
- **Strings:** everything above through i18n, English only.
- **Art:** reused vanilla sprites and generated tinted icons (one tint per Prime colour from `Primes.Color`, applied to a small set of base shapes: vial, powder, crystal, flame). The apparatus show West intake and East spout even as placeholders (doc 13 §7).

## Tech hooks
The slice is where doc 09's architecture plan is first proven. The risks it must surface, each a test in the build's plan:
1. **The staging layer calling the vanilla output path** with `probe`, and `OutputMethod` returning the resolver's match (doc 13 §6.1–2).
2. **Automate never routing our machines to its generic handler** once our factory claims them; locked outputs refused (doc 13 §6.3).
3. **Resolver caching under polling:** a target of 50 apparatus under Automate in one location with no visible frame-time cost (placeholder; the slice report logs resolver time per day).
4. **Codex sync** host → farmhands, including late joiners.
5. **Clock freeze** in single player, and its non-blocking form with farmhands (doc 07).
6. **A building with missing data** removed cleanly on load (S3).
7. **The Town warp** with and without SVE.
8. **SpaceCore skill** registration, level-up screen, and banking XP past level 4.
9. **Custom buff handler** (Undine's Gift) on the vanilla `Data/Buffs` path, and the **fertilizer queries** for our item IDs (doc 15).
10. **Controller support** in every custom menu and per-screen state in split-screen.

Framework choices still open at the gate (doc 09): the bundle menu (Unlockable Bundles or our own), the Satchel's storage (global inventory or item `modData`; doc 17 settles it). The slice uses whatever the gate picks; it is the first build after the gate, not before it.

**1.7:** nothing in the slice depends on 1.7.

## Balance hooks
The slice is the first data the sweep sees. Placeholders it tunes or checks:
- **XP pace:** about 75 a day in Spring, so a diligent player ends Spring at level 4 and reaches 5 in early Summer (doc 05 §2).
- **Aether by source:** the trace from Solar and Void Essence against the bulk from Prismatic Shards and legendary fish (about 1 : 15–20 per item), checked against how often a Spring player meets each.
- **Discovery pace:** most non-Aether Tier 1 cells by the end of Spring (doc 14 §8).
- **Yield constants** `k`, `e`, `QualityBonus`, `Threshold`, `ExchangeRatio`, and the Athanor's and Calcinator's yield, residue and concentration (docs 12, 13).
- **Gold per tile-day** of the best Spring route against what a Spring player has (no kegs yet; doc 14 §3's state table), at or below vanilla (power baseline).
- **Hand actions per day** for a player running a small line: Spring is hand work, but the slice records the number doc 16's quarter-day budget will be measured against.
- **Bundle stretch:** the Furnace Room's three bundles completable within about a week of Spring play (doc 14 §3).
- **Lab T1 cost** reachable in Spring, including the 10 Saxum; 25,000g is the tight part (§1a).
- **Starter-chain rewards:** useful for the next step, never a whole bundle.

## Open questions
- ~~`[blocks-content]` **Chaining and Automate in the slice.**~~ **In** (Charles, 2026-10-18).
- ~~`[blocks-content]` **Oriel, Tobin and the Curator.**~~ **Workshop-bound** (Charles, 2026-10-18).
- ~~`[blocks-content]` **Save carry-forward.**~~ **Carry forward** (Charles, 2026-10-18).
- ~~`[blocks-content]` **Level 5 in the slice.**~~ XP banks past level 4 and applies later (default confirmed by Charles, 2026-10-18).
- ~~`[blocks-content]` **Oriel's starter quests.**~~ Kept (Charles, 2026-10-18), and made a five-step chain whose rewards feed the next step (Charles review, v0.2).
- ~~`[blocks-content]` **Who plays the slice.**~~ Charles and a few testers, not on Nexus (default confirmed by Charles, 2026-10-18).
- `[blocks-content]` **Draft scripts** for the cutscene, restoration scene and starter quests. *Owner: doc 20 (slice drafts written during execution).*
- *For other docs:* doc 13's content-inventory line ("plus the Hermetic Vessel if room allows") is settled here: all six are in. Doc 03 and doc 14 §7 should note that Aether yield follows source rarity (§Player-facing spec). Doc 09's plan must keep the engine's pure logic testable without the game (§6). Doc 16 must spec the slice's menus first (Table, Satchel grid and picker, codex, bundle board, minimal chain overlay); doc 17 must spec the schema version and migration before the build starts.

## Change log
- **v0.2 (2026-10-18):** Charles's review: the starter quests become a five-step chain whose rewards feed the next step; Aether yield follows source rarity (Solar and Void Essence give a trace, Prismatic Shards and legendary fish the bulk); a critical-path check of every finish-line step (§1a), which replaced "a Tier 1 row" with ten Tier 1 discoveries, fixed the end-of-Spring level, required a Spring-only Calcinator cost and flagged Lab T1's 25,000g as tight; a testing section (§6) with four configurations (chaining-only first, SVE from the start), unit tests, logging and a testing guide.
- **v0.1 (2026-10-18):** first draft: the finish line, the first evening and Oriel's starter quests, the Spring loop, the in/out table (six apparatus, all 36 Tier 1 rows, four Tier 2 rows, chaining and Automate in, professions out), recipe rows, darkness via a test pack, build tooling (the slice report, developer commands, a test content pack), edge cases including save carry-forward, the data rows per asset, and the ten technical risks the slice must surface.
