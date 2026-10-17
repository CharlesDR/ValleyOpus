# 03b · Item Budget & Inventory

Status: **DRAFT v0.8** (2026-10-16). v0.8: the Satchel auto-collects reagents only, and the census adds doc 15's gift products and décor (doc 15). v0.7: **the item budget is loosened** (Charles, full review): restraint applies only to items that drop in adventure areas (the mines, Skull Cavern, the Volcano and similar). The ~40-item cap and the general forage ban are gone. v0.6: where late-area reagents appear (with or without SVE), no forage in hidden areas, and alchemical trinkets in the census (Charles). v0.5: rule 1 softened from "no" to "few" new monster drops and mine finds, matching Charles's edit to the inventory rule in doc 01; Radicula added to the census. v0.4: Gloam Lanterns added. This responds to Charles's warning: be careful creating new items, especially monster drops and mine/Skull Cavern finds, because inventory space is already tight.

## Rules
1. **Few new monster drops or mine/Skull Cavern finds** (Charles changed "no" to "few" in doc 01). The default is still **existing vanilla loot**; new drops and finds are **high-value specialty reagents found only in late-game areas** (Charles). **With SVE installed** they appear in SVE's late-game locations; **without SVE** they're grouped into fitting vanilla places instead (Skull Cavern, the Volcano, Ginger Island). Combat and mining reagents otherwise come from vanilla loot (Slime, Bug Meat, Bat Wing, Bone Fragment, Solar Essence, Void Essence, Cinder Shard, ores, geodes, minerals) through context-tag decomposition. Ivo's bounties count **kills** or ask for **vanilla** drops.
2. **Forage is not restricted** (loosened 2026-10-14). New forage is fine where it serves the design, though **Ley Taps** (doc 04) stay the main source of location-specific Primes. **Hidden alchemy areas still get no forage of their own** (Charles, 2026-10-13); they host rare ley nodes and lore.
3. **Not every substance is an item.** Each entry in doc 03a is classed as one of:
   - **Item:** a carriable, sellable object (most Tier 2/3 products, the Primes).
   - **Power:** exists only as an adjacency or network effect, never in inventory (Vapor, Fulgur, Flamma Viva as a lamp fuel state).
   - **Process state:** a stage of a batch, shown in the machine and codex but never an item (Cauda Pavonis, the colour stages).
   - **In-machine intermediate:** consumed inside a multi-stage apparatus without being bottled, unless the player chooses to bottle it (many Tier 1 substances).
4. **No overall cap** (loosened 2026-10-14). Restraint applies only to rule 1's adventure-area drops. Every new item still needs a purpose, an outside-the-mod use (player-experience pillar 1) or a role as a required input, and must follow the mod's established naming and behaviour patterns (internal consistency, doc 01).

## Carrying alchemy items: options
| Option | How it works | Pros | Cons |
|---|---|---|---|
| **A. Reagent Satchel** (recommended) | a one-slot tool that holds every ValleyOpus substance; auto-collects on pickup; hand-loading a machine pulls from it | one inventory slot for the whole mod; familiar "bag" pattern; Automate is unaffected because chests still work | C# UI work; must handle multiplayer and item-grab menus |
| **B. Farm Essence Reservoir** | Primes aren't items at all: they're farm-wide fluid quantities (like Alchemy Factory liquids), drawn by apparatus inside Labs | zero inventory cost for Primes; strong flavour | needs a custom Automate integration; less tactile; breaks "item in, item out" |
| **C. Items only, aggressive pruning** | everything is a normal stackable item, kept to the budget | simplest; maximum compatibility | still costs slots in play |

**Decided (Charles, 2026-10-13): A + rules 1–4**, keeping B as a possible later addition for Primes inside Laboratories.

## Reagent Satchel spec (draft)
Inspired by Minecraft's bundle (hover to see contents), with a proper menu.
- **Seeing contents:** hovering the satchel shows a tooltip listing its contents with counts. Right-click (or the use-tool button) opens a chest-style grid with tabs (Essences, Substances, Products). The icon shows a fill state and a count badge.
- **Putting items in:** while the satchel is in the inventory, ValleyOpus **reagents** (Primes and recipe inputs) picked up from machines, drops or Ley Taps go into it automatically; finished products whose main use is outside the mod (food, fertilizer, combat, gifts) go to the main inventory instead (doc 15, default), with a small "+3 Ignis" HUD notice (GMCM toggle). Items can also be dropped onto it, chest-style. It accepts only items tagged `valleyopus_reagent`.
- **Taking items out:** drag from the grid into the inventory or a chest. Clicking a machine while holding the satchel opens a **picker filtered to that machine's valid inputs**.
- **Capacity:** a limited number of item types (≈24 to start), upgradeable through Tobin. It's a progression sink, not infinite storage.
- **Not inside it:** shipping, gifting and selling all require taking the item out first, which keeps vanilla flows untouched.
- **Multiplayer:** each player has their own satchel; contents are saved in the item's `modData`.
- **Automate:** never reads the satchel; chest-fed lines are unaffected.
- **Compatibility to test:** Chests Anywhere, Lookup Anything, Convenient Inventory-style mods, and controller UI.

## Item census (v0.2 estimate, to refine)
- 8 Primes (items)
- ~12 bottled Tier 1 substances; the other ~24 Tier 1 substances are power, process states or in-machine only
- ~15 Tier 2 products with outside uses (elixirs, fertilizers, bombs, coatings, glass); doc 15 lists them by family
- 3 gift products (Rosa Vitrea, Mel Vitae, Phiala Stellarum) and 2–3 glazed décor Furniture pieces (doc 15, proposals)
- 3 planetary metals (Argentum, Stannum, Plumbum)
- 4 Tier 3 capstone items
- 1 residue (Caput Mortuum), which now also buys hints (doc 07)
- 1 Radicula, the plant homunculus (candidate; doc 03a)
- 0–3 late-area specialty reagents under the softened rule 1 (none chosen yet)
- 6 alchemical trinkets (Vial Pendant, Smouldering Censer and Rebis Amulet, weak and strong) and 3 rings (Planetary Band, Ring of the Seven, Ouroboros Ring) (doc 15 §3a)
- 4 effect products and placed items (Midas Draught, Hourglass of Mercury, Azoth Diffuser, Ever-burning Lamp) and 2 infusion produce items (Brimstone Egg, Golden Fleece); the 12 animal infusions themselves (doc 15 §7a)
- 1–2 Gloam Lanterns (base and upgraded; doc 04). These are a **deliberate inventory cost**: they work only from the main inventory, and carrying more fills faster at diminishing returns. They are crafted tools, not monster drops, so rule 1 holds.
- **≈ 75–80** in total. There's no cap now; the census stays as a planning count, and only the late-area reagents are held to rule 1's restraint.
