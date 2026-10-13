# 03b · Item Budget & Inventory

Status: **DRAFT v0.4** (2026-10-13). v0.4: Gloam Lanterns added. This responds to Charles's warning: be careful creating new items, especially monster drops and mine/Skull Cavern finds, because inventory space is already tight.

## Rules
1. **No new monster drops or mine/Skull Cavern finds.** Combat and mining reagents come from **existing vanilla loot** (Slime, Bug Meat, Bat Wing, Bone Fragment, Solar Essence, Void Essence, Cinder Shard, ores, geodes, minerals) through context-tag decomposition. Ivo's bounties count **kills** or ask for **vanilla** drops.
2. **No new forage for alchemy's sake.** Location-specific essences come from **Ley Taps**, which produce Primes directly (doc 04), not from new forage items.
3. **Not every substance is an item.** Each entry in doc 03a is classed as one of:
   - **Item:** a carriable, sellable object (most Tier 2/3 products, the Primes).
   - **Power:** exists only as an adjacency or network effect, never in inventory (Steam, Fulgur, Living Flame as a lamp fuel state).
   - **Process state:** a stage of a batch, shown in the machine and codex but never an item (Cauda Pavonis, the colour stages).
   - **In-machine intermediate:** consumed inside a multi-stage apparatus without being bottled, unless the player chooses to bottle it (many Tier 1 substances).
4. **A budget.** Aim for at most **~40 new carriable items in 1.0**, excluding flavored variants. Every new item must justify itself with an outside-the-mod use (player-experience pillar 1) or as a required input.

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
- **Putting items in:** while the satchel is in the inventory, ValleyOpus items picked up from machines, drops or Ley Taps go into it automatically, with a small "+3 Fire" HUD notice (GMCM toggle). Items can also be dropped onto it, chest-style. It accepts only items tagged `valleyopus_reagent`.
- **Taking items out:** drag from the grid into the inventory or a chest. Clicking a machine while holding the satchel opens a **picker filtered to that machine's valid inputs**.
- **Capacity:** a limited number of item types (≈24 to start), upgradeable through Tobin. It's a progression sink, not infinite storage.
- **Not inside it:** shipping, gifting and selling all require taking the item out first, which keeps vanilla flows untouched.
- **Multiplayer:** each player has their own satchel; contents are saved in the item's `modData`.
- **Automate:** never reads the satchel; chest-fed lines are unaffected.
- **Compatibility to test:** Chests Anywhere, Lookup Anything, Convenient Inventory-style mods, and controller UI.

## Item census (v0.2 estimate, to refine)
- 8 Primes (items)
- ~12 bottled Tier 1 substances; the other ~24 Tier 1 substances are power, process states or in-machine only
- ~15 Tier 2 products with outside uses (elixirs, fertilizers, bombs, coatings, glass)
- 3 planetary metals (Silver, Tin, Lead)
- 4 Tier 3 capstone items
- 1 residue (Caput Mortuum), which now also buys hints (doc 07)
- 1–2 Gloam Lanterns (base and upgraded; doc 04). These are a **deliberate inventory cost**: they work only from the main inventory, and carrying more fills faster at diminishing returns. They are crafted tools, not monster drops, so rule 1 holds.
- **≈ 45**, over budget; pruning targets are listed in doc 03a's class column (to be added in the balance pass).
