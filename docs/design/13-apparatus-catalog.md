# 13 · Apparatus Catalog

Status: **DRAFT v0.5** (2026-10-17) · Readiness: ready for architecture once Charles confirms the defaults marked *(default)* below; no `[blocks-arch]` item is left open
Depends on: 01, 02, 03, 03a, 04, 12 · Feeds: 05, 06, 07, 09, 14, 15, 16, 17, 18

## Purpose
Every machine the player places, what operations it offers, how it is fed and automated, and how arrangements of a few machines replace the ~25 single-purpose stations proposed in doc 04. The catalog serves pillar 1 (Tobin's engineered line and Oriel's arcane line offer the same operations by different means), pillar 4 (every apparatus asks doc 12's one resolver, so discovery gates automation in one place) and Charles's A1 goal: **fewer BigCraftables, every operation still reachable, and layouts that reward thought**. It also settles doc 12's open `[blocks-arch]` resolver details.

## Decisions locked
- **A1 · Operations stay, BigCraftables shrink** (Charles, 2026-10-13). Doc 04's tables are a list of operations; this doc is the machine list.
- **Recipes name operations and apparatus kinds, never machines** (doc 12, A1). Specialists and vanilla machines are the only exceptions.
- **One C# resolver behind `Data/Machines` `OutputMethod`** (Charles, 2026-10-16). Details settled below (§6).
- **Automation classes** (doc 04 v0.7): engineered and minor arcane run unattended; altars and the circle are staged by Automate and triggered by hand; hand-made-only recipes are fully manual, staging included.
- **Directional chaining** (S2): West in, East out, South chest second, works without Automate, built behind an item-route interface.
- **Aether stays arcane** (doc 03a v0.7): no engineered apparatus runs a recipe with Aether among its direct inputs.

## Player-facing spec
- **A small set of vessels that change with their neighbours.** A player learns about a dozen machines, then learns what each does beside a Boiler, a Lightning Coil, a kindled Athanor or in darkness. Hover tooltips list the operations an apparatus currently offers and which neighbour enables each one; undiscovered neighbour effects show "?" (doc 07).
- **Loading.** Clicking an apparatus while holding the Satchel opens a picker of the discovered recipes it can run now; picking one loads all of that recipe's inputs at once. Holding a single item and clicking adds it to the apparatus's **staging tray**. The apparatus starts as soon as the tray exactly matches a recipe it can run; if a longer recipe on the same apparatus could still follow, it waits for an empty-hand click to seal it.
- **Power.** Boilers, Lightning Coils and a kindled Athanor glow while they supply power, and a faint line links them to the machines they feed. A machine that needs power and lacks it says so on hover.
- **Rituals.** A staged altar glows and the HUD says "Ritual Altar is staged". Laying a Circle Sigil rug around it turns it into a Ritual Circle for one big batch.
- **Failure state.** An apparatus never accepts an item that can't lead to a recipe it can run, so nothing is wasted. Locked discoveries sparkle and wait for the player (doc 07).

## Rules & systems

### 1 · How condensation works
An apparatus offers a **set of operations**. Some are always on; others are **enabled by a neighbour** (power, heat, darkness or a pattern). The resolver already matches recipes by their inputs, so one vessel can offer several operations at once: the inputs and the neighbour state decide which recipe runs (§1a). A **mode switch** in doc 04's sense is the same inputs giving different recipes when a different neighbour is present (an Alembic dissolves cold and distills beside heat).

### 1a · One valid recipe, never a priority check (Charles, 2026-10-16)
A player who controls the arrangement and the inputs must be able to **guarantee** the output. So the resolver never ranks candidates: by construction, at most one recipe can match.
- **Neighbour state.** Each apparatus has a small, fixed set of **neighbour states**, evaluated once in the load-time snapshot and mutually exclusive. For example, the Process Vat's states are *No power*, *Vapor*, *Fulgur* and *Crossed* (both kinds adjacent). The Alembic's are *Cold* and *Heated*. Every recipe **names the states it runs in**. "Any neighbour is fine" is never allowed: a recipe that doesn't care lists every state explicitly, so adding a new state later forces a decision.
- **Uniqueness.** No two recipes may share an apparatus kind, a neighbour state and an input set. Two recipes with the same inputs on the same apparatus are fine **only** when their state lists don't overlap. An example is Dissolution in *Cold* and Distillation in *Heated*.
- **Overlap includes tags.** Two recipes overlap if any single set of real items could satisfy both, so `category_fish` and Tuna collide. The load-time validator checks every pair against the loaded item list and disables the later pack's row with a SMAPI error, so a collision never reaches play.
- **Other selecting conditions follow the same rule.** A Variant (doc 12 §4) is a partition too: the named-product recipe requires its condition, and the base recipe requires the negation. Bonus conditions only change quality, yield or speed, so they never select.
- **The environment never switches an automated line's output** (Charles, 2026-10-16). For automatable recipes (everything except hand-triggered rituals and ✋), conditions the player doesn't control, such as weekday, weather, season, moon and time, may only **gate** a recipe (Required: it runs or it doesn't; storm-only and storm-excluded recipes are both fine) or add a **Bonus**. They may never be a Variant: two automatable recipes may not be identical apart from such a condition (Charles, 2026-10-16). A setup valid only in a storm does nothing in other weather; it never makes something else, so lines built on its output don't break. On those days the machine **waits idle** (Charles, 2026-10-17): it refuses inputs, consumes nothing, makes no residue, and resumes when the condition returns. "Residue instead of failure" covers failed experiments and hand rituals, not a machine that is simply waiting. Variants on automatable recipes use player-controlled factors only: neighbour states, catalysts, location (a ley node; a Lab tier is Bonus only, doc 18 §1a). Environmental Variants stay allowed on **hand-triggered** rituals and hand-made recipes, where the player chooses the moment.
- **Neighbour state is arrangement, not charge.** A Boiler or Lightning Coil puts its neighbours in the *Vapor* or *Fulgur* state whenever it is adjacent, charged or not. An empty source only pauses recipes that need its power. So a storm charging a Coil never flips a Vat from one recipe to another.
- **Crossed power** runs nothing, and the machine says "crossed power" on hover and in the chain overlay (§7). That gives a clear failure state, not a hidden precedence rule.
- **Overclock as a state.** The Lightning Coil's overclock is a recipe that lists the *Fulgur* state with a speed and residue effect. It isn't a blanket bonus, so a Coil never silently changes which recipe a machine runs.

Three rules keep it readable:
1. **At most two always-on operations per vessel**, plus at most two neighbour-enabled ones.
2. **Neighbours are other apparatus, never decorations.** No item exists only to sit beside another one (the 03b purpose rule), so the doc 04 Salt Lamp, Bellows and Cooling Coil are cut (§4). **Final** (Charles, 2026-10-18): doc 18's draft layouts showed every neighbour slot of an automated machine is already a chaining, chest or power slot, so the room (Lab tier, Familiar, hearths, alcoves) and range-2 conditions carry those bonuses instead.
3. **Power and heat are the only neighbours that make up neighbour states.** Everything else (darkness, flowers, ley nodes, the Lab tier) gives Bonus effects or satisfies a recipe's Required condition, as doc 12 §4 conditions, under the same partition rule (§1a) whenever it selects between recipes.

### 2 · The behaviour classes
These are what architecture needs; the rows in §3 are content.

| Class | What it is | Built on | Automation |
|---|---|---|---|
| **Resolver apparatus** | a staging tray, the resolver picks the recipe, timed processing | `Data/Machines` entry, one output rule calling our `OutputMethod`; C# staging layer | full (engineered, minor arcane) |
| **Power source** | a resolver apparatus whose recipe output is a **power state** held in the machine, not an item | same, plus a power-state record in `modData` | full: Automate and chaining refuel it like any input |
| **Staged ritual** | an altar that fills to a complete recipe, then waits for a hand trigger | resolver apparatus with `Trigger: Hand` | staging only; output collected after the rite |
| **Arrangement** | a multi-tile pattern that upgrades a centre apparatus to another kind (the Ritual Circle) | a walkable rug-type Furniture plus the centre apparatus | as its centre |
| **World tap** | produces from its location each day, with no input (Ley Tap) | `Data/Machines` with a `DayUpdate` rule and a location condition | output only |
| **Discovery station** | the Experiment Table: instant, never automated | a BigCraftable with a custom menu | none |
| **Appended vanilla rule** | Furnace and Keg rules for discovered recipes | plain `Data/Machines` rules with the `VALLEYOPUS_DISCOVERED` GSQ; no C# | Automate's native support |

Tools (Assayer's Lens, Gloam Lantern, Reagent Satchel) are not apparatus; docs 07, 04 and 03b own them.

### 3 · The 1.0 machine list (14 BigCraftables)
Kinds and caps follow doc 12 §5. Names are working names; unlock sources are set by doc 14 (§2, §4), summarized here.

**Engineered ⚙︎ (Tobin)**
| Apparatus | Always on | Enabled by a neighbour | Notes | Unlock (doc 14) |
|---|---|---|---|---|
| **Calcinator** | Calcination; **decomposition** | — | fast bulk decomposition; burns a fuel (coal or Bitumen) as an additional consumed item | Tobin, who arrives after the Furnace Room |
| **Process Vat** | Dissolution; Fermentation | **Distillation** with Vapor; **Separation** with Fulgur (electrolysis) | the shared engineered vessel; most of Tobin's line is Vats in a row | Cistern |
| **Press** | Conjunction; Coagulation | — | alloys and moulds; Electrum, Pulvis Pyrius, Saxum | Cistern |
| **Boiler** | Distillation of its own fuel recipe (Ignis + Aqua → Vapor) | — | power source: **Vapor** to up to N adjacent machines (placeholder 4) for a set time per charge | Cistern |
| **Lightning Coil** | Conjunction of its own fuel recipe (Ignis + Aer → Fulgur) | storms charge it free, like a lightning rod | power source: **Fulgur**, stored as charges; also **overclocks**: recipes that list the *Fulgur* state may run faster with more residue (§1a), which replaces doc 04's Bellows | Tobin's letter after the first storm once the Cistern is restored |

**Minor arcane ✦ (Oriel)**
| Apparatus | Always on | Enabled by a neighbour | Notes | Unlock (proposal) |
|---|---|---|---|---|
| **Athanor** | Calcination; **decomposition** | — | slow, better profile (top two Primes), little residue. Fed Ignis + Ignis it **kindles** (Flamma Viva) and stays lit for good, and is then an arcane **heat** source for up to 2 neighbours | one given by Oriel at the intro; recipe from the Furnace Room |
| **Alembic** | Dissolution | **Distillation** with heat (a kindled Athanor or a Boiler) | starter glassware | intro (recipe + one) |
| **Retort** | Separation; Coagulation | — | starter glassware; the neighbour-sensitive one (most adjacency Bonus effects target it) | intro (recipe + one) |
| **Hermetic Vessel** (Vas Hermeticum) | Conjunction; Fermentation | — | the sealed egg of the Work: unions in the light, putrefaction in darkness (darkness is the Required condition of every putrefaction recipe) | Furnace Room |
| **Pelican** | Distillation | — | Bennet's advanced shape: runs `Advance` recipes (doc 12 §5) up to gold, a slow arcane route to quality | Bennet, after the bus repair |

**Ritual ✦A / ✦C**
| Apparatus | Offers | Notes | Unlock (proposal) |
|---|---|---|---|
| **Ritual Altar** | every operation, for recipes marked ✦A, plus Ritual | merges doc 04's Conjunction and Fixing Altars; catalysts are staged on it; Required planetary day, hour or moon conditions live on the recipes | Sanctum |
| **Circle Sigil** *(Furniture, not a BigCraftable)* | turns the altar at its centre into a **Ritual Circle** (✦C): one batch of up to a stack, catalysts at the four points | a 5×5 walkable rug, so it costs floor but no machine slot | Crystal Vault |

**Specialists and discovery**
| Apparatus | Offers | Notes | Unlock (proposal) |
|---|---|---|---|
| **Experiment Table** | Combination (every recipe marked T) | two slots, three after a workshop unlock (a flag, not a new item); respects neighbours, so power and heat recipes can be discovered here (§5) | intro; third slot from the Library |
| **Quarry Crucible** | Conjunction for metals (S) | planetary metals on their weekday; Orichalcum with Fulgur | Agnes, after the Quarry bridge |
| **Ley Tap** | none (world tap) | placeable only on a ley-node tile; yields that node's Primes daily | Hesper, 2 hearts |

**Appended vanilla rules:** Furnace (Gold Bar + Argentum → Electrum; Metallum rules), Keg (Vitae + Ros Maialis → Aqua Vitae). They only accept discovered recipes; their first discovery comes from the Table or our apparatus.

**Every operation stays reachable on both lines** where Aether allows:
| Operation | Engineered ⚙︎ | Minor arcane ✦ | Ritual ✦A |
|---|---|---|---|
| Calcination (+ decomposition) | Calcinator | Athanor | Altar |
| Dissolution | Process Vat | Alembic | Altar |
| Separation | Process Vat + Fulgur | Retort | Altar |
| Conjunction | Press | Hermetic Vessel | Altar |
| Fermentation (incl. Putrefaction) | Process Vat | Hermetic Vessel (dark) | Altar |
| Distillation | Process Vat + Vapor | Alembic + heat; Pelican | Altar |
| Coagulation | Press | Retort | Altar |
| Combination | Experiment Table | Experiment Table | — |
| Ritual | — | — | Altar, Circle |

Doc 04 proposed about 25 machines (7 engineered, 2 support, 5 minor arcane, 2 altars, a circle, 4 specialists, a Cooling Coil, a Salt Lamp, a Bellows and unnamed flower-fed apparatus). This list has **14 BigCraftables plus one rug**.

### 4 · What was cut, and where its job went
| Doc 04 proposal | Now |
|---|---|
| Leaching Vat, Centrifuge, Digester, Steam Still | the **Process Vat**, with Vapor or Fulgur for Distillation and Separation |
| Alloy Press, Mould Press | one **Press** |
| Putrefaction Vessel, minor Conjunction | one **Hermetic Vessel** (light: Conjunction; dark: putrefaction) |
| Conjunction Altar, Fixing Altar | one **Ritual Altar**; Fixing is Coagulation with a Sal Fixum or Mercurius Fixus catalyst |
| Ritual Circle as a separate machine | the **Circle Sigil** arrangement around the altar |
| Cooling Coil (condensing mode switch) | cut: condensing products are Coagulation recipes on the Retort or Press |
| Salt Lamp (quality stabilizer) | cut: Sal Fixum as a catalyst does this per recipe (doc 12 §5) |
| Bellows (speed for residue) | the **Lightning Coil's overclock** |
| Flower-fed apparatus | an **adjacency Bonus condition** (crops or flowers with a tag within range 2) on the Athanor, Hermetic Vessel and Retort; flowers wilt as they feed, so it stays a real crop. No new machine |

### 5 · Power, heat and the "power inputs" rule
Vapor, Fulgur and Flamma Viva are **Power class** (doc 03b): never items. Three recipes in doc 03a listed them as inputs (consistency check 1). **Rule (default):** a Power substance is never a recipe input; a recipe that needs one carries a **Required `power` condition** instead (doc 12 §4 already has the kind), met when a supplying source is adjacent or, for Flamma Viva, when the apparatus itself is a kindled Athanor.
| Recipe | Was | Now |
|---|---|---|
| **Orichalcum** | Metallum + Fulgur | Metallum, Required Fulgur, at the Quarry Crucible (or the Table beside a charged Coil) |
| **Cinis Phoenicis** | Flamma Viva + Vitae | Vitae, Required Flamma Viva: calcined in a kindled Athanor (or on the Table beside one) |
| **Perpetuum Mobile** | Motus + Vapor + Ordo, crafted | Motus + Ordo + Orichalcum, Required Vapor, as a hand-made **ritual at the Altar with a Boiler beside it**. Crafting can't read neighbours, and an altar fed by Tobin's steam is the first of doc 04's **blended** moments, fitting a Mechanist capstone. Orichalcum keeps it late (proposal; doc 03a v0.8) |

**Supplying power.** Adjacency sets the neighbour state; charge only decides whether a powered recipe can run now (§1a). A source holds a power state in `modData`: Vapor and kindled Flamma Viva run for a duration (Vapor) or for good (Flamma Viva); Fulgur holds charges, and each powered cycle it feeds spends one. A source feeds at most `MaxConsumers` adjacent machines, chosen in a fixed order (N, E, S, W) so the result never depends on load order. Power is checked in the **load-time snapshot** (doc 12 §3), so a Boiler going cold mid-cycle never ruins a batch.

### 6 · The resolver behind `OutputMethod` (settles doc 12's `[blocks-arch]` item)
1. **Input never goes through vanilla's one-item drop-in alone.** Every ValleyOpus apparatus keeps a **staging tray** in `modData`. All four loaders write to it through one C# entry point: hand loading (a Harmony prefix on drop-in for our machines), the Satchel picker, directional chaining, and our Automate machine. Each asks the resolver "can this item extend the tray toward a recipe this apparatus can run now?" before moving anything.
2. **When the tray exactly matches**, the C# layer calls the vanilla machine output path with the last item as the trigger. The single `Data/Machines` output rule's `OutputMethod` asks the resolver for the match, returns the output item, sets `overrideMinutesUntilReady` from the recipe, and writes recipe ID, condition snapshot and the locked flag to `modData`. Vanilla then handles the processing timer, ready bubble, sprite frames and hand collection, which is why Lookup Anything and most HUD mods show our machines' state with no extra work.
3. **Automate: one factory for all our apparatus.** Our `IAutomationFactory` claims every ValleyOpus apparatus (not only altars, as doc 09 v0.2 assumed), so Automate's generic `Data/Machines` handler never sees them. Our machine wrapper's `SetInput` loads a **discovered**, non-hand-made recipe that the connected chests can fill, chosen by the machine's **recipe memory** (§6a), and pulls that whole multiset at once; `GetOutput` returns nothing while the output is locked; `GetState` reports *processing* for staged-but-untriggered altars. Chaining priority, the discovery lock and hand-made-only refusal therefore live in one class. Appended vanilla rules (Furnace, Keg) stay on Automate's native support.
4. **Caching under Automate polling.** The resolver keeps, per apparatus kind and enabled-operation set, an **index of runnable recipes keyed by input item ID and tag**, built at load and rebuilt on discovery events, asset reloads and day start. `SetInput` looks up only recipes whose first input appears in the connected chests' item set, and remembers a "nothing loadable" result per machine until a connected chest's contents change. Condition checks run once per successful match, not per poll.
5. **Locked outputs.** The output is a real item from the start (so saves and uninstall stay vanilla-shaped); the lock is a `modData` flag. Hand collection clears it and fires discovery; our Automate machine and chaining refuse it. Third-party extractors that bypass Automate (rare; a doc 17 test item) could take a locked output, which would at worst skip a celebration: the `item:` key is still set on first hand pickup elsewhere.
6. **Lookup Anything and recipe browsing.** Lookup Anything sees each apparatus as a machine with one C#-driven rule, so it can't list our recipes. In 1.0 the **codex is the recipe browser** (doc 07). Display-only output rules that mirror our recipes for Lookup Anything go on the 2.0 list (Charles, 2026-10-16).
7. **Hand-made-only and the acceptance hint.** Hand loading accepts an item only if it extends the tray toward some recipe, discovered or not (a manual first attempt must be possible). So an apparatus that accepts an item tells the player that item belongs in some recipe here: a deliberate, gentle hint (doc 07 layer 0).

### 6a · Which recipe an automated machine runs
"Best" is never decided by the mod, so a line can make exactly the output the player wants even when a more advanced recipe is unlocked (Charles, 2026-10-16). Recipe memory is an **input filter**: it says which multiset Automate pulls from a mixed chest. Once inputs are loaded, the recipe is fixed by inputs plus neighbour state (§1a), so memory never chooses between recipes.
- **Recipe memory:** every apparatus remembers the **last recipe loaded by hand** (or picked in the Satchel picker). Automate and chaining only ever load that recipe. A machine set to Calx stays on Calx even if its chests could also feed Vitrum.
- **Pinning:** the picker has a pin toggle. A pinned machine never changes recipe, even when the player hand-loads something else by accident; hand-loading a different recipe asks first.
- **A fresh machine** with no memory takes nothing from automation until it's set once by hand, so a new line never starts on a surprise recipe. *(Default; the alternative is "first satisfiable in codex order".)*
- **Visible:** the remembered recipe's output shows as a small icon on the machine and in its hover tooltip, with a pin mark when pinned.
- Stored in `modData` (`RecipeMemory`, `Pinned`), so it syncs in multiplayer and is inert without the mod.

### 7 · Directional chaining with multi-input recipes
- A machine offers its finished output East first. The East machine accepts it if it extends that machine's tray toward a discovered recipe; the rest of the recipe's inputs come from its South chest (priority 2) or Automate (priority 3).
- Power sources chain like any machine: a Boiler with a South chest of Ignis and Aqua refuels itself.
- Locked outputs, hand-made-only recipes and untriggered altars never move.
- **Readable in play (Charles, 2026-10-16):** chaining must be learnable by looking, not by reading a wiki. Requirements for doc 16:
  - **Sprites show direction:** every ValleyOpus machine has an intake on its West face and a spout on its East face, in both art styles.
  - **Live links:** a short flowing-particle line between two machines while an item passes, and between a machine and its South chest.
  - **Chain overlay:** a toggle (key, controller button, or holding the Satchel) draws arrows for every active link in the room, greys out machines that receive nothing, and marks broken links (an East neighbour that can't accept the output) in red.
  - **Tooltips:** hover says "Takes from: Calcinator (West), chest (South) · Sends to: Process Vat (East)".
  - **Teaching moment:** Tobin's first-line lesson (doc 02, Summer) walks the player through placing three machines in a row, with the overlay on.

### 8 · Edge cases
- **Multiplayer:** machine state lives on the object's `modData`, so it syncs with the location; the resolver reads the host-synced codex. Power-state expiry uses in-game time, not a per-screen timer.
- **Joja route:** unlock flags come from doc 14, set on both routes; no apparatus names a CC flag.
- **SVE and tie-in mods:** they add recipes and tag weights, never machines, in 1.0. A content pack that adds an apparatus adds an `Apparatus` row plus its BigCraftable (data only).
- **Walk of Life and perks:** perks modify speed, yield, residue and caps through doc 12's additive-then-clamp rule; no perk adds an operation to an apparatus.
- **Uninstall (S3):** apparatus are vanilla BigCraftables with `modData`; without the mod they become inert. Staged trays are emptied into a chest by the "prepare for removal" command.

### 9 · Late-game scale check (doc 00, doc 02)
Target: one T3 Lab per Familiar (four) is **more than enough** for a setup producing every item. Rough count with placeholder numbers, to re-check against real floorplans in doc 18:
- About 75 recipes in 1.0 (36 Tier 1, ~25 Tier 2, 3 dark twins, 3 metals, ~4 stage advances, 4 Tier 3). Recipes sharing an apparatus kind can share a machine over a week, but a "produce everything at once" setup needs about one machine each: **~75 machines**.
- Each needs its share of a South chest, a power source and a walkway: about **5 tiles per machine**, so ~375 tiles; plus 2–3 Ritual Circles at 25 tiles each: **~450 tiles**.
- Taking the Big Shed interior as roughly 200 usable tiles (to verify) and T3 as 1.5× that (~300), four T3 Labs hold ~1,200 tiles. The full setup fits in two, leaving room for scale and decoration. ✓

## Data shape
All assets are public Content Patcher data under `CharlesDR.ValleyOpus/…`, with `FormatVersion`, namespaced IDs and the doc 12 "disable the row, never crash" rule.

| Asset | Kind | Key fields |
|---|---|---|
| `Apparatus` | CP data | `Id` (= BigCraftable ID), `Kind` (Engineered / MinorArcane / RitualAltar / Table / Specialist), `NeighbourStates` [{`Id`, `Test` (power kinds or heat adjacent, or none; mutually exclusive)}], `Operations` [{`Operation`, `States` [state IDs]}], `Decompose` {`YieldMultiplier`, `ResidueRate`, `Concentration`, `Minutes`} or null, `QualityCap` (null = the kind's default), `TrayCapacity`, `Trigger` (Auto / Hand), `Automation` (Full / StageOnly / None), `Chaining` (bool), `Power` {`Emits` (substance ID), `MaxConsumers`, `Mode` (Duration / Charges / Permanent), `Amount`, `StormCharge`} or null, `FeedbackStyle` (Mechanical / Arcane / Blended), `UnlockFlag` (doc 14), `NameKey`, `HintKey` |
| `Arrangements` | CP data | `Id`, `FurnitureId` (the rug), `Centre` (apparatus kind it upgrades), `BecomesKind`, `Points` [tile offsets for catalysts], `BatchSize` |
| `LeyNodes` | CP data | `Location`, `Tile`, `Profile` {Prime → weight}, `UnitsPerDay`, `RevealFlag` |
| `Data/Machines` entries | CP data (generated pattern) | one `OutputRule` per apparatus: trigger `ItemPlacedInMachine`, `OutputMethod` = our resolver; Ley Tap uses `DayUpdate`; vanilla Furnace and Keg get appended rules |
| Machine state | C# state, object `modData` | tray contents, recipe ID, condition snapshot, locked flag, stage, power state (`Emits`, `Until` or `Charges`), kindled flag, `RecipeMemory`, `Pinned` |

Recipes keep `ApparatusKinds` and `ApparatusIds` as doc 12 defines; an apparatus runs a recipe when its `Kind` is listed (or its `Id`, for specialists and vanilla) and the recipe's `Operation` is offered in the current neighbour state. Recipes gain one field for this: `NeighbourStates` [state IDs], which is required and never empty (§1a). The validator checks it against each listed apparatus kind's states.

## Content inventory
- 14 BigCraftables (§3) and 1 Furniture rug (Circle Sigil); 2 appended vanilla machines.
- 3 power sources with their fuel recipes (already Tier 1 rows in 03a: Vapor, Fulgur, Flamma Viva).
- About a dozen adjacency conditions (power, heat, darkness, flowers by tag, ley node, Lab tier, overclock), sharing doc 12's `Conditions` asset.
- 6–10 ley nodes (doc 04's list).
- Crafting recipes for every apparatus (doc 14 sets their unlocks; Vitrum for glassware, Saxum and metals for Tobin's machines).
- Slice (S1): all six of Experiment Table, Athanor, Alembic, Retort, Hermetic Vessel and Calcinator (doc 11 §1).

## Tech hooks
- **`Data/Machines`:** one rule per apparatus with `OutputMethod`; `AdditionalConsumedItems` for the Calcinator's fuel; `ReadyTimeModifiers` left empty (the resolver sets minutes); `DayUpdate` for the Ley Tap. Appended vanilla rules use our `VALLEYOPUS_DISCOVERED` GSQ.
- **C#:** the staging layer and drop-in prefix; the resolver index; power-state ticking; the Automate factory and machine wrapper for every apparatus; the Circle Sigil arrangement check (rug under the altar with the centre tile matching); custom GSQs from doc 12.
- **Risks to verify (doc 09):** that calling the vanilla output path from our staging layer behaves with `probe`; that Automate never routes our machines to its generic handler once our factory claims them; Furniture rugs under a BigCraftable (placement order); hover-tooltip cost for power links in large Labs.
- **1.7:** nothing here depends on the crafting or context-tag changes beyond doc 12's notes.

## Balance hooks
Doc 06 metrics per apparatus: GPTD per tile including its share of power and chest tiles; Boiler `MaxConsumers` and duration (the Factorio ratio); Coil storm charge rate and overclock size; Athanor heat range; decomposition `YieldMultiplier`, `ResidueRate` and `Concentration` per decomposer; Ley Tap units per day; Circle batch size. All placeholders. The sweep also reports machines per endgame setup against §9.

## Open questions
- ~~`[blocks-content]` **Environment rule vs "residue instead of failure"**~~ **settled** (Charles, 2026-10-17): the machine waits idle (§1a).
- `[blocks-content]` **Power-inputs rule** (§5): Power substances become Required power conditions, and Perpetuum Mobile becomes an altar ritual beside a Boiler with Orichalcum as a third input. *Owner: Charles (default applied in 03a v0.8).*
- `[blocks-content]` **Electrum and Rebis** (doc 12, owner 03a): recommended default is that a **twin route** always shares its output, and Electrum (⚙︎ Press or Furnace) and Rebis (✦A Altar) are instead a **counterpart pair** shown side by side through the same `TwinGroup`. No machine changes either way. *Owner: Charles.*
- `[blocks-content]` **Ectoplasm hand-made only** (03a v0.7): if it stays ✋, the Hermetic Vessel runs it only from a hand load; if not, it is ordinary minor-arcane output. Default: keep ✋. *Owner: Charles.*
- ~~`[blocks-content]` **Condensation rule 2 (no modifier blocks)**~~ **final** (Charles, 2026-10-18, after doc 18 §2b's draft layouts).
- `[blocks-content]` **Chaining visual aids** (§7) and the recipe-memory icon (§6a): exact art and controls. *Owner: doc 16.*
- ~~`[blocks-content]` **Lab footprint numbers** in §9 are estimates.~~ **Re-checked** in doc 18 §2c on real floorplans (T3 ≈330 usable tiles): one Lab can't hold everything, two can, four are more than enough.
- *For other docs:* doc 04's proposal tables and doc 09's "native Automate support for engineered and minor arcane" are superseded by §3 and §6; both should point here when next touched.
- `[2.0]` Display-only `Data/Machines` rules that mirror our recipes, so Lookup Anything can list them (Charles, 2026-10-16).
- `[2.0]` A flower-fed **trickle** (a Prime from a planted bed, no input) would be a world tap placed in a garden; arcane logistics replaces chaining through the item-route interface; blended late-game machines are designed with the capstones.

## Change log
- **v0.5 (2026-10-17):** the unlock column follows doc 14 (Charles approved): Oriel gives one Athanor at the intro, rooms supply the rest.
- **v0.4 (2026-10-16):** Charles's extension: uncontrollable conditions only gate or bonus automatable recipes, never vary their output. Neighbour state comes from arrangement, not charge. The tension with gentle failure is logged as a follow-up.
- **v0.3 (2026-10-16):** Charles's uniqueness rule (§1a): neighbour states are mutually exclusive and named by every recipe, there is no "any adjacency", and no two recipes share apparatus, state and inputs. Crossed power runs nothing, and overclock is a state, not a blanket bonus.
- **v0.2 (2026-10-16):** Charles's review: rule 2 marked provisional; automated machines run their remembered or pinned recipe, never a "best" one (§6a); Lookup Anything display rules moved to 2.0; chaining visual-aid requirements (§7).
- **v0.1 (2026-10-16):** first draft: condensation rules, seven behaviour classes, 14 BigCraftables plus the Circle Sigil, power and heat as conditions, the resolver details for `OutputMethod`, Automate and caching, chaining with multi-input recipes, and the four-T3-Lab check.
