# 12 · Recipe & Process Model

Status: **DRAFT v0.5a** (2026-10-21) · Readiness: ready for architecture (the resolver details are settled in doc 13 §6)
Depends on: 01, 03, 03a, 03b, 04, 07 · Feeds: 03/03a (data shape, apparatus column), 05, 06, 07, 09, 13, 14, 15, 17

## Purpose
One model for everything that turns items into other items: decomposition into Primes, combination at the Experiment Table, apparatus and ritual recipes, quality stages, conditions, residue, and the discovery state that gates automation. Every other system reads or writes it, so this doc fixes its **shape**; names, numbers and exact recipes stay content. It serves pillar 1 (mechanical and arcane routes are two kinds of recipe over the same data), pillar 2 (everything decomposes by context tag), pillar 4 (discovery is keyed per recipe, and automation only runs discovered ones), and the extensibility promise in doc 01 (content packs add Primes, substances and recipes as rows, never code).

## Decisions locked
- **R1 · Quality is vanilla quality** (Charles, 2026-10-13). Nigredo, Albedo, Citrinitas and Rubedo are quality 0, 1, 2 and 4. No parallel quality system. The stage names are internal design terms: in game, quality is shown as "base quality" and the vanilla silver, gold and iridium stars (Charles, 2026-10-18; doc 16 §2).
- **R2 · Input count is data** (Charles, 2026-10-13). The Experiment Table holds two inputs, then three after a workshop unlock; a recipe's inputs are a list of any length, so four or more can be added later without rework.
- **R3 · Recipes are fixed for every farm**, except the per-farm secret recipe (Charles, 2026-10-13).
- **R4 · Primes are carried items**, kept in the Reagent Satchel (Charles, 2026-10-13).
- **A1 · Recipes name operations, not machines** (Charles, 2026-10-13). A recipe says which *operation* and *apparatus kind* it needs; doc 13 decides which BigCraftable or arrangement (a vessel plus a heat source, catalyst or neighbour) provides that operation. That is what lets the machine count drop while every operation stays reachable.
- **A2 · Condition variants** (Charles, 2026-10-13): a variant that makes a **named product** is its own recipe and codex entry; any other variant is a quality or yield bonus on the same recipe.
- **Automation only runs discovered recipes** and never makes discoveries (Charles, v0.4 of doc 04). Undiscovered output is locked in the machine until unloaded by hand.
- **Hand-made-only flag** (Charles, doc 01 and doc 04 v0.7): such recipes refuse Automate and chaining entirely, staging included.
- **Latin-first naming** (Charles, 2026-10-14, doc 03a): every material record carries a Latin display name and an English gloss as separate i18n keys.
- **Content packs add Primes, substances and recipes** through Content Patcher in 1.0 (Charles, doc 01). The Tier 1 grid is derived from the Prime list, never fixed at eight.
- **Condition counts grow by tier, with no upper bound** (Charles, doc 01); stacking is a balance risk the sweep reports (doc 06).
- **Primes carry no quality** (Charles, 2026-10-16, default confirmed). An input's quality raises decomposition yield instead.
- **A Table discovery unlocks only that recipe** on apparatus, not other routes to the same item (Charles, 2026-10-16, default confirmed). **Conditional:** revisit in the balance pass if completing the codex turns out too obnoxious.
- **One C# resolver behind `Data/Machines` `OutputMethod`** (Charles, 2026-10-16, default confirmed). The details (caching, Automate and Lookup Anything visibility, locked outputs) settle in doc 13.

## Player-facing spec
The model is invisible; the player meets it through four touchpoints:
1. **Decomposing.** Any item goes into a decomposing apparatus and comes out as a few Prime items plus a little Caput Mortuum. The same item always gives the same profile; better apparatus or routes shift the yield and residue, not the profile.
2. **Experimenting.** Two (later three) items or substances go on the Experiment Table. A defined combination plays the anticipation beat, reveals the result and records the recipe. An undefined one gives residue, a tiny bit of XP, and marks that combination "tried" so it's never repeated by accident (doc 07).
3. **Running apparatus.** A discovered recipe runs on any apparatus that offers its operation, by hand, by directional chaining or by Automate. Conditions (weekday, weather, moon, darkness, neighbours, catalysts) either gate a recipe or improve its quality and yield. When conditions are wrong, a ritual altar says which ones are missing and what would help.
4. **The codex.** Every recipe and every item has a codex entry. Silhouettes show what's still to find; "tried" marks show dead ends; twin routes show side by side (⚙︎ / ✦).

**Failure state:** never a total loss. An undefined experiment gives residue. A ritual triggered under partly wrong conditions (where its recipe allows it) gives a lower stage plus extra residue. A machine never accepts inputs for a recipe it can't run, so automation can't waste materials.

## Rules & systems

### 1 · The things in the model
| Record | What it is | Is it an item? |
|---|---|---|
| **Prime** | one of the base essences (eight by default) | always (R4), held in the Satchel |
| **Substance** | anything made from Primes or other substances: Tier 1–3, dark twins, metals, residue | depends on its **class** (doc 03b): Item, Power, Process state, or In-machine intermediate |
| **Product** | an Item-class substance with outside uses (food, buffs, fertilizer, combat, sale, gifts) | yes; its uses are owned by doc 15, not here |
| **Operation** | a kind of transformation: Calcination, Dissolution, Separation, Conjunction, Fermentation (incl. Putrefaction), Distillation, Coagulation, Combination (the Table), Ritual, Craft | no; a label recipes and apparatus share |
| **Apparatus kind** | Engineered ⚙︎, Minor arcane ✦, Ritual altar ✦, Ritual circle ✦, Experiment Table, Vanilla machine, Specialist, Crafting | no; sets the default quality cap and automation class |
| **Recipe** | inputs + operation + kinds + conditions → output, with quality, residue and discovery rules | no |
| **Condition** | a named, reusable test (GSQ, adjacency, location, catalyst, power) | no |

Vanilla items are never redefined. They take part as **inputs** (by item ID, context tag or category) and occasionally as **outputs** (e.g. a recipe that yields a vanilla gem).

### 2 · Decomposition
Decomposition is a single built-in rule rather than a recipe per item, because it has to work on every vanilla and modded item.
1. **Profile.** Sum the Prime weights of every context tag and the category the item carries (tag-weight table). If nothing matches, use the category default; if the category has none, the global default. Normalize to 1. Drop Primes below a readability threshold (placeholder 10%) and renormalize, so profiles stay short.
2. **Overrides win.** An item override pins the profile, the yield, or both. Integration packs (SVE, RSV, East Scarp, Alecto) live here.
3. **Yield.** `units = max(1, floor(k · price^e · (1 + q_bonus · quality)))`, with `e ≈ 0.5` (sub-linear, doc 03). Units are split across the profile by largest remainder, so the total is exact and deterministic.
4. **Route modifiers.** The decomposing apparatus applies a yield multiplier, a residue rate and optionally a *concentration* (the Athanor's "better profile" keeps only the top two Primes). The Perditio route raises yield and residue together. These are data on the apparatus (doc 13), not on the item.
5. **Equivalent exchange.** The summed value of the Primes produced must stay below `ratio · sell price` (placeholder 0.8). The loader clamps units to enforce it and the sweep reports every clamp, so no item decomposes for profit.
6. **Aether is gated.** Only tags on the explicit Aether source list (prismatic shard, solar and void essence, legendary fish, Mr. Qi items, ley nodes) carry Aether weight; the category defaults never do.
7. **ValleyOpus items** decompose by a pinned override at a loss (Charles, 2026-10-18); Caput Mortuum refuses decomposition (doc 15 §6). Hand-made-only items and capstones refuse decomposition.

### 3 · Recipes
A recipe is one row. Its inputs are a **multiset**: order never matters, and each input is an **ingredient spec**:
- an exact item ID (a Prime, a substance, or a vanilla item), or
- a context tag or category (`category_fish`, `color_red`, `flower_item`), so modded items work without patches,
- each with a count and an optional minimum quality.

**Matching.** Given the apparatus (its operation, kind and modifiers), the loaded inputs and the current context, the resolver:
1. keeps recipes whose operation and kind the apparatus offers, and whose input specs can be satisfied one-to-one by the loaded items, with **exact count of inputs** (no partial matches: a three-item Table load containing a defined pair is not that pair);
2. drops recipes that are disabled on this farm (unselected secret candidates, broken pack references) or not discovered, unless this is a manual first attempt (below);
3. drops recipes whose **Required** conditions fail;
4. returns the **single** survivor. By construction there is never more than one (Charles, 2026-10-16): no two recipes may share an apparatus kind, a neighbour state and an input set, where tags overlap if any real item satisfies both. Selecting conditions (neighbour states, Variants) are partitions, and no recipe matches "any" state. A pair that could both match is a **load-time validation error** for the pack that added the later one, so the resolver never ranks candidates and ambiguity never reaches play (doc 13 §1a).

Condition snapshots are taken **when inputs are loaded** for apparatus (matching doc 04's adjacency rule) and **at trigger time** for rituals. The snapshot is stored with the machine, so reloads, chaining and multiplayer all see the same result.

**Manual first attempts.** A recipe the farm hasn't discovered can still run when the player loads it by hand (or at the Table). The output is then **locked**: Automate, chaining and other extractors can't take it, and the machine shows the "something new inside" sparkle. Unloading by hand discovers the recipe. Automate and chaining only ever load inputs for **discovered** recipes, so they can't start an undiscovered one.

**One recipe, many places.** A recipe lists every apparatus kind it can run on, the Experiment Table included. Discovering Ignis + Aqua → Vapor at the Table is therefore the same discovery that lets a Boiler arrangement run it. Output on the Table follows the output's class: an Item is produced; a Power or Process-state output is revealed in the codex and leaves a harmless puff instead of an item.

**Undefined combinations.** Only the Experiment Table (and Combination-capable apparatus) accept inputs that match no recipe; every other apparatus rejects them, so automation can't waste materials. An undefined Table attempt yields residue, a little XP, and records a **tried** key.

**Input counts.** The Table's capacity is data (2, then 3 after the workshop unlock). Recipes with more inputs than the current capacity are not attemptable; the codex shows them as "needs a larger table" once hinted.

### 4 · Conditions
A condition is a named record with a **kind** and a **use**:
- **Kinds:** a GSQ (weekday, season, weather, time, location, festival); a ValleyOpus GSQ we register in C# (moon phase, planetary day or hour, darkness, ley node, inside a Lab of tier ≥ N, discovered); **adjacency** (a neighbour with a tag, in a direction set, within the 4 or 8 tile setting); **catalyst** (extra consumed items, or Shades from a Gloam Lantern); **power** (adjacent Vapor, Fulgur or a Flamma Viva lamp state).
- **Uses:** **Required** (the recipe won't run without it), **Bonus** (adds quality steps, yield or speed when met), or **Variant** (selects a different recipe; per A2 that's only for named products, which are then separate recipe rows sharing inputs, with the Variant condition as their extra Required condition, while the base row requires its negation, so the two never both match). **For automatable recipes, a Variant may only use player-controlled conditions** (neighbour state, catalyst, location). Weekday, weather, season, moon and time may only gate or add a Bonus there, so an automated line never changes output with the environment (Charles, 2026-10-16; doc 13 §1a). Environmental Variants are for hand-triggered and hand-made recipes. On a day a gate isn't met, an automated machine **waits idle**: it refuses inputs and makes nothing, not even residue (Charles, 2026-10-17).

Conditions are referenced by ID, so the same "Monday" or "beside a heat source" record is reused everywhere and the codex can show "?" for an undiscovered neighbour effect. Doc 04's **mode switches** (an Alembic dissolves *Cold* and distills *Heated*) are two recipes on the same apparatus whose neighbour states don't overlap (doc 13 §1a).

### 5 · Quality stages
Each recipe has a **quality rule**:
- **Base mode:** `Fixed` (a set stage), `InheritMin` (the lowest input quality, the default for engineered lines), `InheritAverage` (rounded down), or `Advance` (input quality + N; for stage-advancing rituals).
- **Modifiers**, all additive with one clamp: met Bonus conditions, catalysts (Sal Fixum prevents a drop; Mercurius Fixus locks the current stage), the Lab tier bonus (doc 18), and perks (doc 05's rule: each perk a distinct variable, shared variables add then clamp).
- **Cap:** from the apparatus kind unless the recipe sets a lower one. Defaults (doc 04, doc 13 §3): Engineered = silver (gold with perks), Minor arcane = gold, Ritual altar and circle = iridium. Perks may raise a cap only where doc 05 says so, and never past iridium.
- **Floor:** the recipe's minimum, so a degraded ritual drops at most to its floor.

Vanilla quality has no stage 3, so `Advance` steps over it (gold → iridium is one step).

**Primes carry no quality** (Charles, 2026-10-16). An item's quality raises its decomposition yield instead. Quality enters at the substance stage through apparatus, conditions and rituals, which keeps the Satchel to one stack per Prime and makes the arcane route the way to high stages.

### 6 · Processes (multi-stage work)
The Great Work is a chain of ordinary recipes, not a special system: a **stage-advance recipe** takes item X at stage *s* and returns X at stage *s+1* (Mode `Advance`). Lapis Philosophorum is Prima Materia advanced through all four stages, then a final hand-made-only ritual whose input spec requires Rubedo. **Process states** (Cauda Pavonis, and the colour stage names) are Process-state substances: codex entries shown in the machine and fired as discovery events (e.g. "first nigredo → albedo advance"), never items. Player-facing text for a stage advance names qualities the vanilla way ("raised to gold quality"), per R1.

### 7 · Twin routes and dark twins
- **Twin routes:** recipes can share a `TwinGroup` ID. The codex shows a group's mechanical and arcane members side by side, and the sweep compares them. Usually the members share an output and differ in cost, conditions and quality cap; a twin route always shares its output. A group may instead mark a **counterpart pair**: different outputs from different inputs shown side by side, like Electrum (⚙︎) and Rebis (✦A) (Charles, 2026-10-18).
- **Dark twins:** a substance may name its `Polarity` twin. Putrefaction recipes (bright → dark) are ordinary Fermentation recipes with a darkness condition and, for higher tiers, a Shades catalyst.

### 8 · Residue
Each recipe has a **residue rule** with three amounts (count and chance): on success, on a partial ritual (wrong Bonus conditions, or a Required condition the recipe marks as *soft*), and on failure. The Table's undefined-combination residue is a global default. Residue chances use a deterministic seed (day, location, tile, recipe), so reloading or a farmhand's view never changes the result.

### 9 · Hand-made only, gifts and crafting
- `HandMadeOnly` on a recipe: Automate and chaining can neither stage nor load it, nor collect its output; only the player loads, crafts or triggers it. Codex counts only hand-made crafts for it (doc 07).
- Recipes with operation `Craft` compile into vanilla crafting recipes (so Better Crafting and the crafting menu see them), still discovery-gated by our codex.

### 10 · Discovery state
Discovery is keyed by **namespaced IDs**, stored once per farm in host save data, and synced to farmhands (doc 07). Keys:
| Key | Set when | Unlocks |
|---|---|---|
| `recipe:<RecipeId>` | the recipe is completed at the Table, or its locked output is unloaded by hand | automation of that recipe |
| `item:<ItemId>` | the item is first obtained or revealed by any manual route | its codex entry and silhouette; releases any locked output of that item |
| `tried:<signature>` | an undefined Table attempt | the "tried" mark; signature = the sorted list of input item IDs |
| `assay:<ItemId>` | first Assayer's Lens use on it | its tooltip row gains the units and loses the "?", becoming the full profile (doc 16 §11) |
| `effect:<ConditionId>` | a Bonus condition or adjacency effect first fires on a hand-unloaded batch | its "?" replaced in the codex |
| `count:<ItemId>` (+ hand-made count) | each craft | codex depth lines at 1, 10 and 50 |

Each key records the day and the discovering player for the shared-codex notice. A key whose ID no longer resolves (a removed pack) is kept but hidden, so reinstalling the pack restores it.

### 11 · The per-farm secret recipe
- Recipes flagged `SecretCandidate` belong to a named **pool**. On a new save, the farm seed (`hash(game unique ID, reroll count)`) picks the pool's set number of recipes (1–2) for this farm. Unpicked candidates are **disabled**: the resolver ignores them and the Table treats their inputs as undefined.
- The pick and the re-roll count live in the farm's codex data, so every player shares it.
- Hesper's almanac and readables reference the picked recipe by ID, so hints follow the seed automatically.
- **Re-roll:** the re-roll ritual (doc 07) increments the count and re-picks. A discovered secret is **replaced** on re-roll, and its codex entry stays as lore (Charles, 2026-10-18).

### 12 · Edge cases
- **Multiplayer:** the host is authoritative for discovery and the secret seed; machines resolve recipes where they are, from the same synced data. Seeded residue makes results identical on every screen.
- **Joja route:** no recipe references a CC-only flag directly; it references unlock flags that doc 14 sets on both routes.
- **SVE and tie-in mods:** they add tag weights, overrides and, where needed, recipes as pack rows. Tag-based input specs mean most of their items need nothing.
- **Automate and chaining:** both ask the resolver "is this a valid input here?" before moving an item, so discovery gating, hand-made-only refusal and condition checks live in one place.
- **Walk of Life:** perks only touch quality, speed, residue and yield modifiers through the additive-then-clamp rule; no perk adds or removes recipes.
- **Uninstall (S3):** recipes are data; machines keep only a recipe ID and a snapshot in `modData`, which is inert without the mod.

## Data shape
All registries are **public Content Patcher assets** under `CharlesDR.ValleyOpus/…`, edited with `EditData`. Each carries a `FormatVersion`. IDs are namespaced (`{{ModId}}_Name`). In our own base CP pack, `{{ModId}}` resolves to the content pack's ID (`CharlesDR.ValleyOpus.CP`), not the C# mod's, so base-pack rows write the `CharlesDR.ValleyOpus_` prefix literally; third-party packs use their own `{{ModId}}_` prefix. Pack references that don't resolve disable the row with a SMAPI warning, never a crash.

| Asset | Kind | Key fields |
|---|---|---|
| `Primes` | CP data | `Id`, `ItemId`, `Group` (Element / Principle / Rare / pack-defined), `Color`, `NameKey`, `GlossKey`, `SortOrder` |
| `Substances` | CP data | `Id`, `ItemId` (null unless Item class), `Class` (Item / Power / ProcessState / Intermediate), `Bottleable`, `Tier`, `Polarity` twin ID, `Tags`, `NameKey`, `GlossKey`, `FlavorKey` |
| `Recipes` | CP data | `Id`, `Inputs` [{`Item` or `Tag` or `Category`, `Count`, `MinQuality`}], `Operation`, `ApparatusKinds` [], `ApparatusIds` [] (specialists and vanilla machines only), `Conditions` [{`ConditionId`, `Use`, `Soft`, `Effect`}], `Output` {`Id`, `Count`, `Flavor` (optional `DROP_IN`-style)}, `Quality` {`Mode`, `Steps`, `Cap`, `Floor`}, `Residue` {`Success`, `Partial`, `Failure`}, `Minutes` or `Days`, `HandMadeOnly`, `TwinGroup`, `Discovery` {`TableAttemptable`, `Hidden`, `HintKeys`}, `SecretPool` |
| `Conditions` | CP data | `Id`, `Kind`, `Query` (GSQ string) or `Adjacency` {`Tag`, `Directions`, `Range`} or `Catalyst` {`Item`/`Shades`, `Count`} or `Power`, `NameKey`, `HintKey` |
| `Operations` | CP data | `Id`, `NameKey` (packs may add operations; doc 13 maps them to apparatus) |
| `Decomposition/TagWeights` | CP data | `Tag` or `Category` → {`Weights` {`Prime`: weight}, `AetherSource`} (doc 03 has the full shape) |
| `Decomposition/CategoryDefaults` | CP data | `Category` → profile |
| `Decomposition/Overrides` | CP data | `ItemId` → {`Profile`, `Units`, `Refuse`} |
| `Decomposition/Settings` | CP data (GMCM economy knobs read it) | `k`, `e`, `QualityBonus`, `Threshold`, `ExchangeRatio`, default residue |
| `SecretPools` | CP data | `Id`, `PickCount` |
| Farm codex | C# state, host save data | discovery keys (section 10), secret picks, re-roll count, Table capacity flag |
| Machine state | C# state, object `modData` | recipe ID, condition snapshot, locked flag, stage, staged inputs |

**Derived, never stored:** the Tier 1 grid (n Primes give n(n+1)/2 cells; missing pairings are undefined combinations and flagged by the validator), the codex layout, per-recipe "days a year it can run", and the sweep tables.

## Content inventory
Placeholder counts for 1.0 (rows, not final names):
- 8 Primes; 36 Tier 1 recipes (one per cell); about 25 Tier 2 recipes plus their twin routes; 4 Tier 3 routes; 3 putrefaction recipes (dark twins); 3 planetary-metal recipes; about 4 stage-advance recipes (per stage, generic by tag where possible).
- About 20–30 conditions (weekdays, weather, moon phases, darkness, ley nodes, a dozen adjacency effects, catalysts).
- Appended vanilla-machine recipes (Furnace accepts Metallum, Keg accepts Vitae for Aqua Vitae; doc 04).
- One secret pool with about 6–8 candidates, 1–2 picked per farm.
- A tag-weight table of roughly 60–100 rows plus a default per vanilla category; overrides only where the sweep shows a need.
No new items are created by this doc; every row's output already exists in doc 03a or 03b.

## Tech hooks
- **Resolver in C#.** One recipe resolver serves the Table, every apparatus, chaining, Automate input acceptance, the codex and Lookup Anything. Approved (Charles, 2026-10-16; details in doc 13): apparatus are `Data/Machines` entries with a C# `OutputMethod` that calls the resolver, and Automate's own generic handler runs them (Charles, 2026-10-19, D25; doc 13 §6.3), so the discovery lock, hand-made-only refusal and ritual staging live in our staging layer, which every loader goes through (doc 09 §5a, §9).
- **Custom GSQs** registered in C# (`VALLEYOPUS_DISCOVERED`, moon phase, planetary hour, darkness, ley node, Lab tier) so conditions also work inside plain `Data/Machines` rules and other packs' data.
- **Crafting** recipes compile into `Data/CraftingRecipes`; vanilla-machine rules are appended to `Data/Machines`, never replacing.
- **Validation at load:** unresolved references, ambiguous matches, missing Tier 1 cells, equivalent-exchange clamps and hand-made-only recipes on automatable-only apparatus are reported in the SMAPI log; a console command dumps the report.
- **Risks:** resolver cost when Automate polls many machines (cache matches per apparatus and input signature; invalidate on discovery or data reload); `OutputMethod` plus Automate behaviour to verify for locked outputs.
- **1.7 migration:** 1.7's `Data/ContextTags` and data-model crafting recipes affect only the tag reader and the crafting compiler; the recipe asset itself is ours and stays.

## Balance hooks
Doc 06 metrics read straight from this data: GPTD and value per tile and per ingredient use recipe inputs, outputs, times and quality caps; arcane recipes are averaged over a week of conditions. Placeholders to tune in the sweep: `k`, `e`, `QualityBonus`, `Threshold`, `ExchangeRatio`, per-apparatus yield and residue rates, quality caps, Bonus condition sizes, ready times, and residue amounts. The sweep also reports days-per-year per recipe (condition stacking), Prime source diversity from the tag table, and the all-six-professions quality clamp.

## Open questions
Every open item across the docs is also collected in [doc 10](10-roadmap-and-open-questions.md#open-questions). Settled items are removed from this list; their outcomes are in the sections above and the change log.
- `[blocks-content]` **Table unlock scope, balance revisit:** Charles approved "only that recipe" on condition it's revisited in the balance pass if codex completion feels too obnoxious (options in doc 07). *Owner: doc 06 balance pass.*
- `[blocks-content]` **Where darkness holds outside a Lab** (raised by doc 18, 2026-10-18): darkness is the Required condition of every putrefaction recipe (§4, doc 13 §3), and doc 18 only guarantees a Lab's dark alcove tiles. Night outdoors, caves and unlit rooms need a definition. *Owner: doc 12.*
- `[2.0]` Four-plus Table inputs need no model change. A Potion Craft–style brew map would add a new operation with its own resolver, plugged in beside this one.

## Change log
- **v0.5a (2026-10-21):** docs cleanup sweep: the resolver tech hook follows D25 (Automate's generic handler, no factory); the `assay:` row follows Charles's tooltip decision (2026-10-22: farm-discovered Elements as icons, assaying adds units).
- **v0.5 (2026-10-19):** B2 docs update: base-pack rows write the `CharlesDR.ValleyOpus_` prefix literally, since `{{ModId}}` there resolves to the content pack's ID.
- **v0.4a (2026-10-19):** pre-gate cleanup sweep: R1 and §6 note that the stage names are internal; player-facing quality uses vanilla words and stars (doc 16 §2).
- **v0.4 (2026-10-18), Charles's answers:** ValleyOpus items decompose at a loss; a re-roll replaces the discovered secret; Electrum and Rebis are a counterpart pair, and twin routes always share their output.
- **v0.4 (2026-10-18):** cleanup sweep: settled open questions removed (doc 07 already adopted the "releases a locked output" wording); darkness outside a Lab added as an open question from doc 18; the Satchel-storage conflict tagged for doc 17; the Cooling Coil example replaced (doc 13 cut it).
- **v0.3 (2026-10-17):** the environment rule vs "residue instead of failure" is settled: an automated machine waits idle on days its gate isn't met (Charles). Caput Mortuum refuses decomposition (doc 15).
- **v0.2 (2026-10-16):** Charles confirmed the three `[blocks-arch]` defaults (no quality on Primes; a Table discovery unlocks only that recipe, revisit in the balance pass; one resolver behind `OutputMethod`, details to doc 13). `TagWeights` gains `AetherSource` to match doc 03's data shape.
- **v0.1 (2026-10-15):** first draft: records, decomposition, recipe matching, conditions, quality, processes, residue, discovery keys, the secret recipe and the content-pack data shape.
