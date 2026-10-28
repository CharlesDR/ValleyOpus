# 14 · Unlock Map & Workshop Bundles

Status: **DRAFT v0.6** (2026-10-21) · Readiness: ready for architecture (Charles approved every default on 2026-10-17; no `[blocks-arch]` item is left open)
Depends on: 01, 02, 03a, 05, 07, 08, 08a, 12, 13, 15 · Feeds: 05, 06, 09, 11, 16, 17, 18, 19, 20

## Purpose
When and how each part of ValleyOpus opens up: every apparatus, tool, Table upgrade, Lab tier, NPC, hint channel, ley node and story beat, mapped to its source (a workshop room, an NPC, a skill level, a base-game milestone or a discovery), on both the Community Center and Joja routes. It also defines the **derelict workshop's rooms and bundles**. The map serves pillar 4 (wonder first, then scale: Spring is hand work and discovery, Summer brings Tobin's lines), the discovery pillar (unlocks open *means*, never recipes, so knowledge still comes from experimenting), and doc 02's pacing targets: a diligent player sees most of the mod in Year 1 and all of it by the end of Year 2, with no calendar gate anywhere.

## Decisions locked
- **Soft start** (Charles, doc 02): 2 days after the Community Center opens, the first entry into Town between 6:00am and 3:00pm; Oriel walks the player to the workshop behind the CC. It never fires straight after the CC-opening cutscene; it waits for the next Town entry with the other conditions met (Charles, 2026-10-20).
- **Letters arrive overnight** (Charles, 2026-10-20): as vanilla mail does, a letter comes the next morning. What a scene's follow-up letter would say belongs in the scene itself, so the intro sends no welcome letter.
- **One-screen workshop** (Charles, 2026-10-20): the workshop is one map that fits on one screen; its rooms are areas of it, not separate maps (§2).
- **No hard calendar gates** (Charles, doc 01): every unlock is gated by progress (a bundle, a skill level, friendship, a base-game milestone, a discovery). Recurring conditions (weekday, weather, season, moon) are allowed because they come round again. Seasons in this doc are the **expected pace for a diligent player**, never a requirement.
- **Discovery over shopping** (Charles, doc 01): automation only runs discovered recipes, and **no unlock grants a recipe**. Unlocks give apparatus, tools, places, people and hints; the recipes themselves are learned by experimenting (doc 07). *One exception (Charles, 2026-10-18): Alchemy professions and skill levels may grant new recipes that change no existing setup, and nothing on the critical path may depend on one (doc 05 §3, §5).*
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
2. **Restoring the workshop.** The Curator's bundle board works like the CC's: each room has three bundles, which appear only once the room is open; restoring a room plays a short scene and grants its unlocks. Rooms open in four tiers, and each tier after the first opens when **any one** room of the tier before it is restored, so a player can lean toward Tobin's or Oriel's side and come back for the other.
3. **People arrive** as the workshop comes back: Tobin with the first restored room, Hesper and Dr. Sorenson with the second tier, the Wizard and the Witch with the third. Agnes, Bennet, Delta, Ivo, Finch and Lark each have their own trigger (below).
4. **The Great Work.** Once all seven rooms are restored, the sealed chamber opens: the Curator's identity, the Great Work quest, Laboratory T3 and the Familiars.

### What the player sees
- **The bundle board** shows each room's bundles with vanilla items as icons and Elements as **silhouettes with a one-line hint** until discovered. Asking for an Element counts as an `Existence` hint (doc 07): a bundle never asks for something the player can't find out how to make.
- **Restoration scenes** are short (clock frozen, skippable after the first view) and end on a hold-up of what was unlocked, or a letter the next morning.
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

**Contract rewards.** A Sorenson contract can grant an unlock as its non-monetary reward (doc 15 §6). Only **optional extras** are given this way (a décor set, a second Satchel picker pin, a Lab cosmetic, a contract-only curio), never an apparatus, Lab tier or story step on the main path, so a player who skips contracts loses nothing the rest of the map needs (Charles, 2026-10-17).

**Curio** (term): a collectible keepsake with **no gameplay effect**, placed as Furniture or kept on a shelf, whose value is story and display, like vanilla's Prize Machine trophies. Each Sorenson curio is a piece of her Joja past (an early "synthetic essence" vial, a redacted lab notebook) with a line of lore on hover. A curio is not a hint item: Finch's weekly **curiosity** (doc 07) is a different thing, a stock item whose flavour text hints at a recipe.

**Recipes are never unlocked.** A recipe becomes *reachable* when (a) an apparatus kind that runs it is unlocked, (b) its inputs can be obtained and (c) its Required conditions can be met; it becomes *known* only through discovery (doc 07), and *automatable* only through `recipe:` (doc 12 §10). The one exception is story order: Tier 3 recipes carry a `RequiresUnlock` of **the Great Work** *(approved)*, so the capstones can't be stumbled on before the story reaches them. No other recipe carries an unlock requirement.

**Crafting recipes for products** (trinkets, rings, décor, the alternate Bomb and Rain Totem recipes; doc 12 §9 `Craft`) are learned when their **defining substance is discovered** (its `item:` key): discovering Pulvis Pyrius teaches the Bomb and Mega Bomb recipes, discovering Smaltum teaches the glazed décor set *(approved)*. Apparatus and tool recipes are the only crafting recipes that come from unlocks.

### 2 · The workshop's rooms
Seven rooms, one for each operation of the Magnum Opus in doc 02's seasonal order, plus the sealed Great Work chamber. Each room's motif is the operation the matching season introduces; it doesn't mean the operation was unavailable before (the Alembic dissolves from day one).

**One screen** (Charles, 2026-10-20): the workshop is one map that fits on one screen, and each room is an area of it. Like the CC's rooms, a room's bundles appear in the workshop only once the room is open, so neither the map nor the board spells out the order (doc 16 §10). For now the map is one big rectangle; a more interesting floor plan comes later.

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
| ★ | **Great Work Chamber** | the Opus | the Curator → Cyprian | all seven rooms | late Winter Y1 → Year 2 | the Curator revealed as **Cyprian** in a cutscene, never a letter (Charles, 2026-10-20); the **Great Work** quest (Tier 3 recipes reachable); **Laboratory T3** at Robin's; **Familiar** bonding (doc 18) |

**Why "any one" between tiers:** it keeps pillar 1's choice real. A player drawn to rituals can restore the Library, then the Sanctum, and leave the Cistern for later; a factory builder can go Cistern then Undercroft. The finale still needs all seven, so nobody skips a side for good.

**Why the Athanor is a gift at the intro** *(Charles, 2026-10-17; doc 13 updated)*: decomposition is the first verb of the mod, and without a decomposer the Table has nothing to combine. Oriel's own old Athanor makes the first evening playable; its crafting recipe still comes with the Furnace Room, so a second one is the room's reward.

### 3 · Workshop bundles
Three bundles per room, four in the Great Work Chamber, CC-style: each slot is an ingredient spec from doc 12 §3, so tags work ("any Spring forage", "any Tier 1 Ignis substance"), and a bundle may need N of M slots. That makes 25 bundles. Contents are placeholders for doc 20 and the sweep; what is fixed here is the **stretch rule** and the **currency rule** per tier.

**Stretch rule** (Charles, 2026-10-17): every bundle is completable with what is unlocked when its room opens (the critical-path rule below), but it should be a **challenge at that point**, pushing the player to use the tools they already have to their limit rather than wait for the next ones. Each room's three bundles therefore follow one pattern:
1. **A tools bundle** that asks the newest apparatus for something at its edge: a discovery puzzle (an arrangement the player hasn't tried), volume that rewards a small automated line, or a quality stage just inside the apparatus's cap.
2. **A progress bundle** tied to base-game progress expected at that point: the next band of the Mines, a skill level's tool, the season's harder crops or fish.
3. **A gathering bundle** of the room's theme, the easiest of the three, so every room starts with a foothold.

**What a diligent player likely has when each room opens** (the stretch is measured against this):
| Room opens | Base game | Alchemy |
|---|---|---|
| Furnace Room (Spring, about day 7) | copper tools coming, Mines to about 20, Spring crops, no animals | Athanor, Alembic, Retort, Table (2), a handful of Tier 1 discoveries |
| Tier II (late Spring) | copper tools, Mines to about 40, a coop or the first sprinklers | the full Spring kit (Hermetic Vessel, Calcinator, Lab T1), most non-Aether Tier 1 cells, residue piling up |
| Tier III (Summer) | steel tools, Mines to about 80, bee houses, first kegs | Tobin's line (Vat, Press, Boiler, Coil) **or** Table (3), Hesper and the Ley Tap; first Tier 2 by hand; Aether still rare |
| Tier IV (Fall) | Mines bottom or close, Quarry bridge likely, Skull Cavern not yet | the Ritual Altar **or** the Gloam Lantern; Lab T2; Aether as a daily trickle if the Sanctum is done |
| Great Work Chamber (Winter) | Mines done, the desert open, Skull Cavern started | all seven rooms: altars, the Circle, the Pelican if the bus is repaired, quality stages up to iridium |

| Tier | Currency | Room | Tools bundle (stretch) | Progress bundle | Gathering bundle |
|---|---|---|---|---|---|
| I | **all vanilla** (P3) | Furnace Room | *Kindling*: Hardwood ×10 (a copper axe), Coal ×5, Sap ×20 | *Mine Salts*: Copper Bar ×5, Frozen Tear, Earth Crystal, Quartz ×3 (Mines 40) | *Spring Gleanings*: any 4 different Spring forage |
| II | vanilla plus **one** bundle of Tier 1 substances | Cistern | *First Waters*: Ros Maialis ×5 (needs heat: an Alembic beside a kindled Athanor, the first arrangement puzzle), Sal Marinum ×10, Limus ×10 | *Fittings*: Iron Bar ×5, Refined Quartz ×5, a Sprinkler (Farming 2) | *River and Lake*: any 4 different freshwater fish |
| II | same | Library | *First Fires*: Calx ×10 at silver (an Athanor with a flower Bonus), Magma ×5, Vitrum (the first Tier 2) | *Curios*: any 3 different artifacts, one of them donated (Gunther) | *Bindings*: Fiber ×50, Clay ×10, any 2 different Spring flowers |
| III | one-third mod products (Tier 1 and 2, automatable) | Sanctum | *Unions*: Vitrum ×20 (volume: a small automated line or a patient week), Vigor (Aether), any 3 different Tier 2 at silver | *Offerings*: Honey ×5 (bee houses), any 3 different flowers, a gold-quality crop | *Summer Light*: Sunflower, Melon, Solar Essence ×5 |
| III | same | Undercroft | *Rot and Return*: Caput Mortuum ×50, Miasma ×10, Bitumen ×10 (volume with no Aether, since this room can open before the Sanctum's ley node) | *Night Harvest*: Purple Mushroom (Mines 81+), Void Essence ×5, Bat Wing ×10 | *Dark Things*: any 3 different monster loot |
| IV | two-thirds mod products, some at a quality stage | Observatory | *Star Chart*: any 2 of Luna, Sol and Spiritus, ×3 each (an Aether supply: the Sanctum's ley node, or Void and Solar Essence decomposed) | *Clear Spirits*: Aqua Vitae ×5, Elixir at gold (arcane quality), any Wine or Mead | *Winter Sky*: any 4 different Winter forage |
| IV | same | Crystal Vault | *Fixed Salts*: Sal Fixum ×10, Mercurius Fixus at silver, Gelum ×5 | *Gems*: Diamond, Emerald, Vitreus ×10 | *Metals of the Week*: 2 of 3: Argentum or Stannum or Plumbum, Gold Bar ×5, Iron Bar ×10 |
| ★ | mostly mod products at a stage | Great Work Chamber | *Nigredo*: each dark twin ×3 · *Albedo*: any 5 different Tier 2 at silver · *Citrinitas*: any 3 different Tier 2 at gold · *Rubedo*: one Tier 2 at iridium (altar) | | |

- **Rewards** per bundle are small and useful, like the CC's: a stack of a Prime-rich vanilla item, an extra apparatus, a fertilizer stack. The room's unlocks are the main reward. A bundle's reward stays hidden until the bundle is complete, as in vanilla (Charles, 2026-10-20; doc 16 §10).
- **Never ✋.** Bundles ask only for automatable items, and accept automated output (doc 10 #2).
- **Critical-path rule** (Charles, 2026-10-17): every slot must be makeable with what is unlocked **when its room opens**, never with something the room itself (or a later room) unlocks. Checked against §2 and §4, the v0.3 list broke this twice, now fixed: the Sanctum asked for Electrum, which needs Argentum from the Quarry Crucible (a bridge-gated specialist), and the Undercroft asked for Mors and Venenum, the dark twins the Witch teaches only once the Undercroft is restored. A bundle may ask for something whose source is a base-game gate only through an "N of M" choice with a vanilla fallback (Metals of the Week). The sweep re-runs this check whenever bundles or unlocks change (balance hooks).
- **Never a single source.** A slot names a tag or "any of" wherever possible, so one bad harvest or one undiscovered route can't stall a room.
- **Metals of the Week** depends on the Quarry bridge (Quarry Crucible). It gives a "2 of 3" choice with a vanilla fallback slot *(approved)*, so a player who hasn't repaired the bridge can still finish the Crystal Vault.
- **The final room** asks for the four colour stages themselves, a rehearsal of the Great Work: the Rubedo bundle is the first time most players push a product to iridium. The four bundle titles keep the stage names as lore names (Charles, 2026-10-19: they name stages of the Work, not a quality); their slot text shows quality the vanilla way, "base quality" and the silver, gold and iridium stars (doc 16 §2).
- **Stretch, not grind.** Counts stay in the range a diligent player reaches in about a week of the season's normal play (placeholders for the sweep). A slot that needs more than that is a design bug, not a challenge.

### 4 · NPC arrivals and their unlocks
| NPC | Arrives | Then unlocks |
|---|---|---|
| **Oriel** | soft-start cutscene | ritual and concept advice (hint channel); spends part of her week at the workshop from the Furnace Room on |
| **The Curator** | in the workshop from the start | the bundle board; workshop-lore hints; revealed as Cyprian at the Great Work Chamber, in a cutscene |
| **Tobin** | the day after the Furnace Room | Calcinator; the Cistern's machines; machine and layout tips (hint channel); the Lightning Coil letter |
| **Finch** | the week after the Furnace Room | his weekly route: sells rare reagents, buys surplus, stocks a curiosity hint (doc 07). His wagon stands out in the world, never in the workshop, at a spot that is open with and without SVE (Charles, 2026-10-20; doc 17 §7a) |
| **Ivo** | the Guild, once the Furnace Room is restored and the Guild is open | bounties (vanilla loot); his Pulvis Pyrius hints wait for the Table's third slot |
| **Hesper** | the Library | visiting days, the weekly almanac; **Ley Tap** recipe at 2 hearts; Observatory content |
| **Dr. Sorenson** | the Cistern | bulk contracts that pay above shipping or reward an optional unlock (doc 15 §6, §1 above); her Joja arc runs through the Great Work |
| **Agnes** | in Clint's shop, the first visit after any Tier II room | assaying; a place to sell metals (at the shipping price); the **Quarry Crucible** once the **Quarry bridge** is repaired |
| **Delta** | at the beach, once the Furnace Room is restored and the player has Fishing level 2 | fish requests; sea-substance hints (Sal Marinum, Luna, Hydragenum) |
| **Bennet** | after the **bus repair**, once the Furnace Room is restored | the **Pelican** recipe; glassware commissions (apparatus upgrades) |
| **Lark** | after the farm's 10th discovery | the comic hint channel |
| **The Wizard** | the Sanctum (vanilla friendship gates still apply to his heart content) | bright-substance teaching; the first Aether ley node; the ban scene the first time Anima is made |
| **The Witch** (or Alecto) | the Undercroft | the Gloam Lantern; dark-twin hints; Shades |
| **The Familiars** | the Great Work Chamber and a T3 Lab | met at their places, then wait in the workshop; bonding, one per T3 Lab, is a hand-made rite of Anima plus the element's pure pair at a Ritual Altar in that Lab (doc 18 §4) |

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

### 6 · Skill-level unlocks
Adopted and owned by **doc 05 §3**: levels give capacity and tool upgrades (Satchel tab, chest-wide assaying, the upgraded Gloam Lantern, multi-machine pinning), professions at 5 and 10, and may also grant recipes under doc 05's critical-path guard (the exception in Decisions locked). No bundle requires an Alchemy-level unlock. Level 10 never means everything is unlocked (P4): rooms, NPCs and the Great Work are separate tracks.

### 7 · Aether is the main pacing valve
Every bright substance, every dark twin and most of Tier 2's arcane side need **Aether**. Its sources open in a fixed order of effort, which paces the arcane mid-game without a gate on any recipe:
1. **Spring:** rare vanilla sources only (Prismatic Shard, Solar and Void Essence, legendary fish), enough to discover a few Aether pairs at the Table, not to run them. Yield follows source rarity: the essences give a trace, Prismatic Shards and legendary fish the bulk (doc 03).
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
- **Late install** (the mod added to an existing save): the soft start fires on the next eligible Town entry, however late; base-game gates already met apply at once. Nothing is skipped automatically, and there is no optional GMCM fast start either (Charles, 2026-10-18; doc 16).
- **SVE and tie-ins:** our gates read vanilla milestone flags, not CC bundle contents, so SVE's remixed bundles don't matter. The workshop entrance warp is at Town tile **53,9**, behind the CC; spike S13 passed there for both vanilla and SVE (doc 17 §7a). Charles's rule: the entrance is behind the CC or a side stairwell, never the CC's front door. Tie-in mods add ley nodes and hints, never rooms, in 1.0.
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
- **8 workshop areas** (the Entrance Hall plus 7 rooms) and the Great Work Chamber; **25 bundles** (placeholder contents above).
- **9 restoration scenes** (one per room plus the hall and the finale) and about 12 arrival scenes or letters (doc 20 writes them).
- Crafting recipes for every apparatus and tool, with costs: early apparatus cost only vanilla items (Clay, Refined Quartz, Copper Bar, Stone); later ones use Saxum, Vitrum and metals (doc 13).

## Tech hooks
- **Flags and conditions:** our `VALLEYOPUS_UNLOCKED <Id>` GSQ, plus the mirrored mail flags, so plain CP data (Robin's `Data/Buildings` `BuildCondition`, shop entries, `Data/Characters` `UnlockConditions`, events, dialogue) can test an unlock without C#.
- **Bundles:** our own bundle menu, not Unlockable Bundles (Charles, 2026-10-19; doc 09 §10). Its progress lives in host save data with the rest of farm state.
- **Crafting recipes** are per player in vanilla: a C# day-start and on-connect pass grants every unlocked recipe to every player.
- **Base-game gates:** vanilla mail flags and world state for the CC and Joja milestones (to verify, doc 10), wrapped in one GSQ per gate so routes never leak into data.
- **The workshop map:** the rooms are areas of one screen-sized map that change on restoration (CP map patches keyed on our flags), like the CC's rooms (§2).
- **1.7:** nothing here depends on 1.7 changes.

## Balance hooks
Doc 06 reads the map as a **pace report**, and the sweep also runs the **bundle critical-path check** (§3): for each bundle slot, the earliest unlock set that makes it reachable must be available when the slot's room opens. The pace report covers, for each season at a diligent pace, which recipes are reachable, the gold per tile-day of the best reachable route against the vanilla machines a player has by then (kegs and jars early), the Aether a player can gather against what that season's content asks for, and the number of hand actions per day the season's setups need (doc 16's quarter-day budget). Placeholders to tune: bundle contents and rewards, apparatus crafting costs, Hesper's heart threshold for the Ley Tap, Lark's discovery count, and skill-level unlocks.

## Open questions
Every open item across the docs is also collected in [doc 10](10-roadmap-and-open-questions.md#open-questions). Settled items are removed from this list; their outcomes are in the sections above and the change log.
- `[blocks-content]` **Lightning Coil in Winter:** the Coil comes by letter after the next storm (approved). If a Cistern restored in Winter feels too slow in play, the fallback is to grant it with the room. *Owner: balance pass.*
- `[blocks-content]` **Contract-reward list:** optional extras only (approved); which extras and curios is content. *Owner: doc 20.*
- `[blocks-content]` **Bundle contents, rewards and NPC arrival scenes.** *Owner: doc 20 and the sweep.*
- `[2.0]` Arcane logistics (Hesper's teaching) and forging unlocks; prestige perks.

## Change log
- **v0.6 (2026-10-21):** docs cleanup sweep: Charles's sink-or-swim feedback (2026-10-20) moved here from the to-do list: the soft start never fires straight after the CC-opening cutscene; letters arrive overnight, so the intro sends no welcome letter; the workshop is one screen whose rooms are areas, with a room's bundles appearing only once it opens; bundle rewards stay hidden until done; the Curator's name is revealed in a cutscene, never a letter; Finch's wagon stands out in the world, never in the workshop.
- **v0.5 (2026-10-19):** B2 docs update: workshop entrance warp at Town 53,9 behind the CC (spike S13 passed, vanilla and SVE); never the CC's front door (Charles).
- **v0.4a (2026-10-19):** pre-gate cleanup sweep: the Great Work Chamber keeps stage names as bundle titles only (Charles, 2026-10-19); Spring Aether yield follows source rarity (doc 03).
- **v0.4a (2026-10-18), Charles's answer:** no late-install fast start, not even as a GMCM option; the open question is closed.
- **v0.4 (2026-10-18):** cleanup sweep: "(proposal)" dropped from §2 and §3 (Charles approved them in v0.2); §6's skill table now points to doc 05 §3, which owns it; the Familiar row follows doc 18 §4; doc 07's hint channels now carry their unlock sources; settled open questions removed.
- **v0.3 (2026-10-17):** follows doc 15 v0.6 (PR #11): gated automated recipes wait idle; shops pay the shipping price, so NPCs open places to sell, not prices; Sorenson's contracts may reward an optional unlock (new `Contract` source); "Element" used for the umbrella term. Charles approved the contract-reward rule; "curio" defined; bundles checked against the critical path (Electrum and the dark twins replaced); bundles rebuilt on Charles's stretch rule (a tools, a progress and a gathering bundle per room, against the player's likely state), and the Great Work Chamber gains a Rubedo bundle (25 in all).
- **v0.2 (2026-10-17):** Charles approved every default; doc 02's jungle ley node, doc 13's unlock column and the README updated to match.
- **v0.1 (2026-10-17):** first draft: unlock kinds, "unlocks open means, never recipes", seven workshop rooms in four tiers plus the Great Work Chamber, 24 bundles with a currency rule per tier, NPC arrivals, base-game gates on both routes, skill-level proposals, Aether as the pacing valve, the Year 1 reachability table and the data shape.
