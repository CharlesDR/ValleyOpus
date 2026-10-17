# 14 · Unlock Map & Workshop Bundles

Status: **DRAFT v0.3** (2026-10-17) · Readiness: ready for architecture (Charles approved every default on 2026-10-17; no `[blocks-arch]` item is left open)
Depends on: 01, 02, 03a, 05, 07, 08, 08a, 12, 13, 15 · Feeds: 05, 06, 09, 11, 16, 17, 18, 19, 20

## Purpose
When and how each part of ValleyOpus opens up: every apparatus, tool, Table upgrade, Lab tier, NPC, hint channel, ley node and story beat, mapped to its source (a workshop room, an NPC, a skill level, a base-game milestone or a discovery), on both the Community Center and Joja routes. It also defines the **derelict workshop's rooms and bundles**. The map serves pillar 4 (wonder first, then scale: Spring is hand work and discovery, Summer brings Tobin's lines), the discovery pillar (unlocks open *means*, never recipes, so knowledge still comes from experimenting), and doc 02's pacing targets: a diligent player sees most of the mod in Year 1 and all of it by the end of Year 2, with no calendar gate anywhere.

## Decisions locked
- **Soft start** (Charles, doc 02): 2 days after the Community Center opens, the first entry into Town between 6:00am and 3:00pm; Oriel walks the player to the workshop behind the CC.
- **No hard calendar gates** (Charles, doc 01): every unlock is gated by progress (a bundle, a skill level, friendship, a base-game milestone, a discovery). Recurring conditions (weekday, weather, season, moon) are allowed because they come round again. Seasons in this doc are the **expected pace for a diligent player**, never a requirement.
- **Discovery over shopping** (Charles, doc 01): automation only runs discovered recipes, and **no unlock grants a recipe**. Unlocks give apparatus, tools, places, people and hints; the recipes themselves are learned by experimenting (doc 07).
- **Workshop bundles** (P3, Charles): they take vanilla items and mod substances; early bundles are all vanilla; they never ask for a ✋ item (doc 15 §10).
- **Both routes** (doc 08): the workshop sits under the CC on both routes; no unlock names a CC-only flag (docs 12 and 13 already rely on this).
- **Power baselines** (Charles, 2026-10-16): early and mid-game unlocks are paced so products stay at or below vanilla; the late tier is benchmarked against SVE (doc 15).
- **The v0.1 defaults** (Charles, 2026-10-17): the seven rooms with the "any one room" tier rule, the Athanor as an intro gift, the bundle currency by tier, Tier 3 recipes requiring the Great Work, product crafting recipes learned from their defining substance, the Lightning Coil letter after a storm (granted with the room as the fallback if Winter proves a problem), and no automatic fast start on a late install.
- **Recipe uniqueness and the environment rule** (Charles, 2026-10-16; doc 13 §1a): unlocks never add a recipe, so they can't create a collision; an unlock that is itself gated by weather (the Lightning Coil letter) only delays, it never changes what a line makes. A gated automated recipe **waits idle** on days its condition isn't met (Charles, 2026-10-17; doc 13 §1a), so an unlock schedule never has to plan around wasted inputs.
- **Selling and contracts** (Charles, 2026-10-17; doc 06 v0.6, doc 15 §6): every shop pays the shipping price, so an NPC arrival opens a **place to sell**, never a better price. Dr. Sorenson's contracts pay more than shipping **or** give a significant non-monetary reward, and an unlock may be that reward (§4).
- **Element** (Charles, 2026-10-17; doc 03a): the umbrella term for Primes, substances and anything derived from them; this doc uses it wherever a rule covers all of them.

## Player-facing spec

### The shape of a playthrough
1. **The Encounter** (Spring, a few days in). Oriel's cutscene, the walk behind the CC, the ruined workshop. The player leaves with a Reagent Satchel, an Assayer's Lens, an Experiment Table, an Alembic, a Retort and Oriel's own old **Athanor**, so they can decompose and experiment on the first evening.
2. **Restoring the workshop.** The Curator's bundle board in the entrance hall works like the CC's: each room has three bundles; restoring a room plays a short scene and grants its unlocks. Rooms open in four tiers, and each tier after the first opens when **any one** room of the tier before it is restored, so a player can lean toward Tobin's or Oriel's side and come back for the other.
3. **People arrive** as the workshop comes back: Tobin with the first restored room, Hesper and Dr. Sorenson with the second tier, the Wizard and the Witch with the third. Agnes, Bennet, Delta, Ivo, Finch and Lark each have their own trigger (below).
4. **The Great Work.** Once all seven rooms are restored, the sealed chamber opens: the Curator's identity, the Great Work quest, Laboratory T3 and the Familiars.

### What the player sees
- **The bundle board** shows each room's bundles with vanilla items as icons and Elements as **silhouettes with a one-line hint** until discovered. Asking for an Element counts as an `Existence` hint (doc 07): a bundle never asks for something the player can't find out how to make.
- **Restoration scenes** are short (clock frozen, skippable after the first view) and end on a mail or a hold-up of what was unlocked.
- **New crafting recipes** appear in the vanilla crafting menu with a "learned" notice, as vanilla skill recipes do. Robin's menu gains each Lab tier when it unlocks.
- **The codex has an Unlocks page** listing every unlock as a silhouette with its source ("Restore the Cistern", "Hesper, 2 hearts", "Repair the Quarry bridge"), so the path forward is always readable in-game.
- **Failure state:** none. An unlock never expires or closes again; missing a weekday or a storm only waits for the next one.

## Rules & systems

### 1 · What an unlock is
An **unlock** is one farm-wide flag plus the things it grants. Kinds:
| Kind | Grants | Example |
|---|---|---|
| **Apparatus** | a crafting recipe for a BigCraftable (plus, for the intro, a free copy) | Process Vat |
| **Tool** | a tool or its crafting recipe | Assayer's Lens, Gloam Lantern |
| **Capacity** | a data flag on an existing system | Experiment Table third slot (R2) |
| **Building** | a Robin blueprint | Laboratory T1, T2, T3 |
| **NPC** | an NPC's arrival, schedule or shop | Hesper's visiting days |
| **Hint channel** | a source of doc 07 hints | Cyprian's journal pages, Hesper's almanac, residue divination |
| **Place** | ley nodes revealed, a ritual site usable | the first Aether-bearing ley node |
| **Story** | an event or quest line | the Great Work |
| **Feature** | a mechanic switched on | Sorenson's contracts, Qi orders |

**Contract rewards.** A Sorenson contract can grant an unlock as its non-monetary reward (doc 15 §6). Only **optional extras** are given this way (a décor set, a second Satchel picker pin, a Lab cosmetic, a contract-only curio), never an apparatus, Lab tier or story step on the main path, so a player who skips contracts loses nothing the rest of the map needs *(proposal)*.

**Recipes are never unlocked.** A recipe becomes *reachable* when (a) an apparatus kind that runs it is unlocked, (b) its inputs can be obtained and (c) its Required conditions can be met; it becomes *known* only through discovery (doc 07), and *automatable* only through `recipe:` (doc 12 §10). The one exception is story order: Tier 3 recipes carry a `RequiresUnlock` of **the Great Work** *(approved)*, so the capstones can't be stumbled on before the story reaches them. No other recipe carries an unlock requirement.

**Crafting recipes for products** (trinkets, rings, décor, the alternate Bomb and Rain Totem recipes; doc 12 §9 `Craft`) are learned when their **defining substance is discovered** (its `item:` key): discovering Pulvis Pyrius teaches the Bomb and Mega Bomb recipes, discovering Smaltum teaches the glazed décor set *(approved)*. Apparatus and tool recipes are the only crafting recipes that come from unlocks.

### 2 · The workshop's rooms (proposal)
Seven rooms, one for each operation of the Magnum Opus in doc 02's seasonal order, plus the sealed Great Work chamber. Each room's motif is the operation the matching season introduces; it doesn't mean the operation was unavailable before (the Alembic dissolves from day one).

| Tier | Room | Motif | Host | Opens when | Pace (diligent) | Unlocks on restoration |
|---|---|---|---|---|---|---|
| — | **Entrance Hall** | — | Oriel, the Curator | the soft-start cutscene | Spring | Reagent Satchel, Assayer's Lens, Experiment Table (2 slots), Alembic and Retort (recipes + one each), one Athanor (gift only), the bundle board, the codex |
| I | **Furnace Room** | Calcination | the Curator | the cutscene | Spring | Athanor and Hermetic Vessel recipes; **Tobin arrives** next day with the Calcinator recipe; **Laboratory T1** at Robin's; Finch starts his weekly route; Ivo appears at the Guild |
| II | **Cistern** | Dissolution | Tobin | Furnace Room | Summer | Process Vat, Press and Boiler recipes; Tobin's **chaining lesson** (doc 13 §7); **Dr. Sorenson** arrives (contracts); the **Lightning Coil** recipe by Tobin's letter the morning after the next storm |
| II | **Library** | Separation | the Curator, Oriel | Furnace Room | Summer | **Experiment Table third slot** (R2); Cyprian's journal series as a hint channel; **Hesper** starts her visiting days and the weekly almanac; **residue divination** at the bundle board |
| III | **Sanctum** | Conjunction | Oriel, the Wizard | any Tier II room | Fall | **Ritual Altar** recipe; the Wizard's bright-substance lesson, which reveals the **first Aether-bearing ley node**; **Laboratory T2** at Robin's |
| III | **Undercroft** | Fermentation | the Witch | any Tier II room | Fall | the Witch's letter and hut visit; **Gloam Lantern** recipe; Shades catalysts in recipes become usable; her dark-twin hints |
| IV | **Observatory** | Distillation | Hesper | any Tier III room | Winter | Hesper's star rituals and celestial-event hints; the **farm-secret** almanac thread (doc 07); the remaining valley ley nodes revealed |
| IV | **Crystal Vault** | Coagulation | the Curator | any Tier III room | Winter | **Circle Sigil** recipe; Robin mentions a larger Lab (T3 groundwork, flavour only) |
| ★ | **Great Work Chamber** | the Opus | the Curator → Cyprian | all seven rooms | late Winter Y1 → Year 2 | the Curator revealed as **Cyprian**; the **Great Work** quest (Tier 3 recipes reachable); **Laboratory T3** at Robin's; **Familiar** bonding (doc 18) |

**Why "any one" between tiers:** it keeps pillar 1's choice real. A player drawn to rituals can restore the Library, then the Sanctum, and leave the Cistern for later; a factory builder can go Cistern then Undercroft. The finale still needs all seven, so nobody skips a side for good.

**Why the Athanor is a gift at the intro** *(Charles, 2026-10-17; doc 13 updated)*: decomposition is the first verb of the mod, and without a decomposer the Table has nothing to combine. Oriel's own old Athanor makes the first evening playable; its crafting recipe still comes with the Furnace Room, so a second one is the room's reward.

### 3 · Workshop bundles (proposal)
Three bundles per room, CC-style (each slot an ingredient spec from doc 12 §3, so tags work: "any Spring forage", "any Tier 1 Ignis substance"; a bundle may need N of M slots). 24 bundles in all. Contents below are placeholders for doc 20 and the sweep; what is fixed here is the **currency rule** per tier:

| Tier | Currency rule | Room | Bundles (themes, example slots) |
|---|---|---|---|
| I | **all vanilla** (P3) | Furnace Room | *Kindling* (Wood, Sap, Coal, Hardwood) · *Spring Gleanings* (any 4 Spring forage) · *Mine Salts* (Quartz, Earth Crystal, Copper Ore, Stone) |
| II | vanilla, plus **one** bundle of Tier 1 substances | Cistern | *River and Lake* (4 freshwater fish) · *Fittings* (Copper Bar, Iron Bar, Refined Quartz) · *First Waters* (Ros Maialis, Limus, Sal Marinum) |
| II | same | Library | *Bindings* (Cloth, Wool, Duck Feather) · *Curios* (any 3 artifacts) · *First Fires* (Calx, Magma, Pulvis) |
| III | one-third mod products (Tier 1 and 2, automatable) | Sanctum | *Summer Light* (Sunflower, Starfruit, Solar Essence) · *Offerings* (Honey, any 3 flowers) · *Unions* (Vitrum, Electrum, any Tier 2 at silver) |
| III | same | Undercroft | *Night Harvest* (Void Essence, Bat Wing, Purple Mushroom) · *Dark Things* (any 3 monster loot) · *Rot and Return* (Mors, Venenum, Caput Mortuum ×20) |
| IV | two-thirds mod products, some at a quality stage | Observatory | *Star Chart* (Luna, Sol, Spiritus) · *Clear Spirits* (Aqua Vitae, Vigor, Elixir at silver) · *Winter Sky* (any 4 Winter forage) |
| IV | same | Crystal Vault | *Fixed Salts* (Sal Fixum, Mercurius Fixus, Gelum) · *Gems* (Diamond, Emerald, Vitreus ×5) · *Metals of the Week* (Argentum or Stannum or Plumbum, Gold Bar) |
| ★ | mostly mod products at gold | Great Work Chamber | *Nigredo* (dark twins) · *Albedo* (Tier 2 at silver ×3) · *Citrinitas* (Tier 2 at gold ×3) |

- **Rewards** per bundle are small and useful, like the CC's: a stack of a Prime-rich vanilla item, an extra apparatus, a fertilizer stack. The room's unlocks are the main reward.
- **Never ✋.** Bundles ask only for automatable items, and accept automated output (doc 10 #2).
- **Never a single source.** A slot names a tag or "any of" wherever possible, so one bad harvest or one undiscovered route can't stall a room.
- **Metals of the Week** depends on the Quarry bridge (Quarry Crucible). It gives a "2 of 3" choice with a vanilla fallback slot *(approved)*, so a player who hasn't repaired the bridge can still finish the Crystal Vault.
- **The final room** asks for the colour stages themselves, a quiet rehearsal of the Great Work's four stages.

### 4 · NPC arrivals and their unlocks
| NPC | Arrives | Then unlocks |
|---|---|---|
| **Oriel** | soft-start cutscene | ritual and concept advice (hint channel); spends part of her week at the workshop from the Furnace Room on |
| **The Curator** | in the workshop from the start | the bundle board; workshop-lore hints; revealed as Cyprian at the Great Work Chamber |
| **Tobin** | the day after the Furnace Room | Calcinator; the Cistern's machines; machine and layout tips (hint channel); the Lightning Coil letter |
| **Finch** | the week after the Furnace Room | his weekly route: sells rare reagents, buys surplus, stocks a curiosity hint (doc 07) |
| **Ivo** | the Guild, once the Furnace Room is restored and the Guild is open | bounties (vanilla loot); his Pulvis Pyrius hints wait for the Table's third slot |
| **Hesper** | the Library | visiting days, the weekly almanac; **Ley Tap** recipe at 2 hearts; Observatory content |
| **Dr. Sorenson** | the Cistern | bulk contracts that pay above shipping or reward an optional unlock (doc 15 §6, §1 above); her Joja arc runs through the Great Work |
| **Agnes** | in Clint's shop, the first visit after any Tier II room | assaying; a place to sell metals (at the shipping price); the **Quarry Crucible** once the **Quarry bridge** is repaired |
| **Delta** | at the beach, once the Furnace Room is restored and the player has Fishing level 2 | fish requests; sea-substance hints (Sal Marinum, Luna, Hydragenum) |
| **Bennet** | after the **bus repair**, once the Furnace Room is restored | the **Pelican** recipe; glassware commissions (apparatus upgrades) |
| **Lark** | after the farm's 10th discovery | the comic hint channel |
| **The Wizard** | the Sanctum (vanilla friendship gates still apply to his heart content) | bright-substance teaching; the first Aether ley node; the ban scene the first time Anima is made |
| **The Witch** (or Alecto) | the Undercroft | the Gloam Lantern; dark-twin hints; Shades |
| **The Familiars** | the Great Work Chamber and a T3 Lab | bonding, one per T3 Lab (doc 18 designs the bonding; proposal: a hand-made offering ritual of Anima plus a gift of the Familiar's element) |

### 5 · Base-game gates (both routes)
Each gate reads the vanilla milestone, whichever route reached it. Flag names are to verify (doc 10).
| Gate | CC route | Joja route | Opens |
|---|---|---|---|
| **CC opens** | Lewis's CC event | the same event (it fires before any route choice) | the soft start |
| **Quarry bridge** | Crafts Room | the Joja bridge project | the Quarry Crucible (with Agnes met): planetary metals, Orichalcum, advanced transmutation |
| **Bus repair** | Vault | the Joja bus project | Bennet: the Pelican and glassware commissions; desert ley node |
| **Adventurer's Guild** | open after the Mines | same | Ivo |
| **Skull Cavern** | Skull Key | same | Ivo's deeper bounties; Shade-rich hunting |
| **Ginger Island** | Willy's boat | same | the island ley nodes: the Volcano node (Ignis-heavy) and the jungle node (Terra and Aether) |
| **Volcano forge** | reached | same | an Ignis ritual site (Required location for a late ritual); forging itself is 2.0 |
| **Mr. Qi** | Qi's Walnut Room | same | a few Qi special orders for alchemical feats; the hard Qi Bean / Radioactive Ore recipe becomes reachable through its inputs |

### 6 · Skill-level unlocks (proposal for doc 05)
Vanilla skills hand out crafting recipes; ours mostly can't, because recipes come from discovery. Levels give **capacity and tool upgrades** instead:
| Level | Unlock |
|---|---|
| 2 | Reagent Satchel: one more tab of capacity |
| 4 | Assayer's Lens assays a whole chest's contents at once (codex profiles only) |
| 5 | profession choice (doc 05) |
| 6 | upgraded Gloam Lantern recipe (with the Undercroft restored) |
| 8 | Satchel picker can pin a recipe on several machines at once (doc 13 §6a) |
| 10 | profession choice (doc 05) |
Level 10 never means everything is unlocked (P4): rooms, NPCs and the Great Work are separate tracks.

### 7 · Aether is the main pacing valve
Every bright substance, every dark twin and most of Tier 2's arcane side need **Aether**. Its sources open in a fixed order of effort, which paces the arcane mid-game without a gate on any recipe:
1. **Spring:** rare vanilla sources only (Prismatic Shard, Solar and Void Essence, legendary fish), enough to discover a few Aether pairs at the Table, not to run them.
2. **Summer–Fall:** the Ley Tap (Hesper, 2 hearts) on ordinary nodes gives elemental Primes; the **Sanctum** reveals the first Aether-bearing node, which turns Aether into a daily trickle.
3. **Winter onward:** the Observatory reveals the remaining valley nodes; Ginger Island adds richer ones.
The sweep checks that the Aether a diligent player can collect by each season covers what that season's bundles and featured products ask for (balance hooks).

### 8 · Year 1 at a diligent pace (what's reachable)
| Season | Apparatus | Recipes reachable | Products that matter |
|---|---|---|---|
| **Spring** | Table (2), Athanor, Alembic, Retort, Hermetic Vessel, Calcinator; Lab T1 | the Tier 1 grid (Aether cells only rarely); first Tier 2 by hand (Vitrum, Terra Viva, Vigor) | Sal Volatile, Ros Maialis, Caput Mortuum fertilizer, Calx; first gifts by hand |
| **Summer** | Process Vat, Press, Boiler, Lightning Coil; Table (3); Ley Tap | Tier 1 automated; most Tier 2 without Aether; Pulvis Pyrius; Electrum (Furnace) | bombs, Terra Viva fertilizer, Sorenson's contracts, Mors weed control |
| **Fall** | Ritual Altar, Gloam Lantern; Lab T2; Quarry Crucible (if the bridge is repaired) | bright substances, dark twins, Rebis, planetary metals, Panacea, animal infusions | Elixir, Panacea, Venenum coatings, the first trinkets and the Planetary Band, infusions |
| **Winter** | Circle Sigil; Pelican (if the bus is repaired) | ritual batches, stage advances to gold, Gelum, Quinta Essentia, Mercurius Philosophorum | gold-quality products, the strong-tier effects, the secret recipe's first hints |
| **Year 2** | Lab T3, Familiars | Tier 3 (Prima Materia, Alkahest, Azoth, Lapis Philosophorum), Perpetuum Mobile, the farm's secret | capstones, the Great Work's ending (doc 02's finished state) |

A typical player covers the same map more slowly, finishing around Year 4 (doc 02).

### 9 · Edge cases
- **Multiplayer:** unlocks are **farm-wide**, stored with the shared farm codex in host save data and mirrored to every player as mail flags for Content Patcher conditions; a farmhand who joins later receives them on connect. Crafting recipes from unlocks are granted to every player, including late joiners. Bundles are shared, like the CC's.
- **Joja route:** nothing in this doc reads a CC-only flag; the workshop and its bundles work the same; Sorenson's arc carries more weight (doc 08).
- **Late install** (the mod added to an existing save): the soft start fires on the next eligible Town entry, however late; base-game gates already met apply at once. Nothing is skipped automatically (approved; an optional GMCM "fast start" is an open question).
- **SVE and tie-ins:** our gates read vanilla milestone flags, not CC bundle contents, so SVE's remixed bundles don't matter. SVE's Town map behind the CC must leave room for the workshop warp (doc 17). Tie-in mods add ley nodes and hints, never rooms, in 1.0.
- **Festivals:** a restoration scene or NPC arrival due on a festival day moves to the next day, like the soft start.
- **Removed packs:** an unlock row from a removed pack is kept but hidden, as discovery keys are (doc 12).
- **Uninstall (S3):** unlock state is mod save data and is simply ignored; learned crafting recipes for missing items are dropped by vanilla (doc 17 test item).

## Data shape
All assets are public Content Patcher data under `CharlesDR.ValleyOpus/…`, with `FormatVersion`, namespaced IDs and doc 12's "disable the row, never crash" rule.

| Asset | Kind | Key fields |
|---|---|---|
| `Unlocks` | CP data | `Id`, `Kind` (Apparatus / Tool / Capacity / Building / NPC / HintChannel / Place / Story / Feature / pack-defined), `Grants` [{`Type` (CraftingRecipe / Item / Flag / Blueprint / Event / Mail), `Ref`, `Count`}], `Source` {`Type` (Intro / WorkshopRoom / NPC / Friendship / SkillLevel / BaseGame / Discovery / Contract / Unlock), `Ref`}, `Requires` [unlock IDs or GSQs; all must hold], `Announce` (mail, event or toast key), `Pace` (Spring / Summer / Fall / Winter / Year2; **informational only**, read by the sweep, never by the game), `NameKey`, `SourceKey` (the codex "how to unlock" line) |
| `WorkshopRooms` | CP data | `Id`, `Tier`, `Motif` (operation), `Host`, `Opens` {`AnyOf` or `AllOf` room IDs}, `Bundles` [bundle IDs], `Unlocks` [unlock IDs], `RestoreEvent`, `MapArea` |
| `WorkshopBundles` | CP data | `Id`, `Room`, `Slots` [doc 12 ingredient specs: item, tag or category, `Count`, `MinQuality`], `Required` (N of M), `Reward` {item, count}, `NameKey`, `HintKeys` (shown for undiscovered Elements) |
| Recipe field | CP data (doc 12 `Recipes`) | `RequiresUnlock` (optional; only Tier 3 uses it) |
| Craft-recipe learning | CP data (doc 12 `Craft` recipes) | `LearnedBy` {`Item` (an `item:` key) or `Unlock` (an unlock ID)} |
| Farm unlock state | C# state, host save data | unlocked IDs with day and player; bundle slot progress; rooms restored; mirrored as mail flags `CharlesDR.ValleyOpus_Unlock_<Id>` on every player |

**Derived, never stored:** recipe reachability per season (from apparatus unlocks, input sources and conditions), the codex Unlocks page, and the sweep's pace report.

## Content inventory
- About **40 unlocks**: 14 apparatus and the Circle Sigil, 4 tools and upgrades, 1 capacity, 3 Lab tiers, 12 NPC arrivals, 6 hint channels, 4 ley-node reveals, the Great Work and a few features.
- **8 workshop areas** (the Entrance Hall plus 7 rooms) and the Great Work Chamber; **24 bundles** (placeholder contents above).
- **9 restoration scenes** (one per room plus the hall and the finale) and about 12 arrival scenes or letters (doc 20 writes them).
- Crafting recipes for every apparatus and tool, with costs: early apparatus cost only vanilla items (Clay, Refined Quartz, Copper Bar, Stone); later ones use Saxum, Vitrum and metals (doc 13).

## Tech hooks
- **Flags and conditions:** our `VALLEYOPUS_UNLOCKED <Id>` GSQ, plus the mirrored mail flags, so plain CP data (Robin's `Data/Buildings` `BuildCondition`, shop entries, `Data/Characters` `UnlockConditions`, events, dialogue) can test an unlock without C#.
- **Bundles:** Unlockable Bundles or our own bundle menu; doc 09 picks at the gate (already listed there). The `WorkshopBundles` shape works with either.
- **Crafting recipes** are per player in vanilla: a C# day-start and on-connect pass grants every unlocked recipe to every player.
- **Base-game gates:** vanilla mail flags and world state for the CC and Joja milestones (to verify, doc 10), wrapped in one GSQ per gate so routes never leak into data.
- **The workshop map:** the rooms are areas of one map that change on restoration (CP map patches keyed on our flags), like the CC's rooms.
- **1.7:** nothing here depends on 1.7 changes.

## Balance hooks
Doc 06 reads the map as a **pace report**: for each season at a diligent pace, which recipes are reachable, the gold per tile-day of the best reachable route against the vanilla machines a player has by then (kegs and jars early), the Aether a player can gather against what that season's content asks for, and the number of hand actions per day the season's setups need (doc 16's quarter-day budget). Placeholders to tune: bundle contents and rewards, apparatus crafting costs, Hesper's heart threshold for the Ley Tap, Lark's discovery count, and skill-level unlocks.

## Open questions
- `[blocks-content]` **Late-install fast start:** nothing is skipped automatically (approved); whether GMCM later offers an optional "fast start" that restores the Furnace Room is still open. *Owner: Charles / doc 16.*
- `[blocks-content]` **Lightning Coil in Winter:** the Coil comes by letter after the next storm (approved). If a Cistern restored in Winter feels too slow in play, the fallback is to grant it with the room. *Owner: balance pass.*
- `[blocks-content]` **Skill-level unlocks** (§6) are proposals for doc 05. *Owner: doc 05.*
- `[blocks-content]` **Contract-reward unlocks:** optional extras only (proposal); the list is content. *Owner: Charles / doc 20.*
- `[blocks-content]` **Familiar bonding** trigger and cost. *Owner: doc 18.*
- `[blocks-content]` **Bundle contents, rewards and NPC arrival scenes.** *Owner: doc 20 and the sweep.*
- *For other docs:* doc 07's hint channels gain their unlock sources from §4 when it is next touched.
- `[2.0]` Arcane logistics (Hesper's teaching) and forging unlocks; prestige perks.

## Change log
- **v0.3 (2026-10-17):** follows doc 15 v0.6 (PR #11): gated automated recipes wait idle; shops pay the shipping price, so NPCs open places to sell, not prices; Sorenson's contracts may reward an optional unlock (new `Contract` source); "Element" used for the umbrella term.
- **v0.2 (2026-10-17):** Charles approved every default; doc 02's jungle ley node, doc 13's unlock column and the README updated to match.
- **v0.1 (2026-10-17):** first draft: unlock kinds, "unlocks open means, never recipes", seven workshop rooms in four tiers plus the Great Work Chamber, 24 bundles with a currency rule per tier, NPC arrivals, base-game gates on both routes, skill-level proposals, Aether as the pacing valve, the Year 1 reachability table and the data shape.
