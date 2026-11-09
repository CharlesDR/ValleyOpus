# 16 · UX & Interface

Status: **DRAFT v0.2** (2026-10-21) · Readiness: ready for architecture (Charles reviewed and merged it as PR #17; no `[blocks-arch]` item open)
Depends on: 01, 02, 03b, 04, 05, 07, 11, 12, 13, 14, 15, 18 · Feeds: 09 (the architecture plan), 11 (the slice's menus), 17, 20, 21

## Purpose
Every screen, prompt, tooltip and in-world indicator the player meets, and the input model that ties them together. The goal is a mod that is **learnable by looking**: the first evening is playable without a wiki, chaining is understood from the machines themselves, and a heavily invested mid-game day stays near a quarter of the player's time with no busywork. It serves the approachability pillar (gentle failure, readable hints, clock freeze), discovery over shopping (the codex as the recipe browser and the record of every find), internal consistency (one look and one set of verbs for every apparatus) and U1 (controller and split-screen). Multiplayer sync, save storage and the compatibility test matrix are doc 17's; this doc only says what each player sees.

## Decisions locked
Everything below was already decided elsewhere; this doc gathers it and adds no rule that contradicts it.
- **Controller and split-screen are required for 1.0** (Charles, U1): every custom menu is fully playable on a gamepad, with per-screen state. **Groundwork first, in-game tests later** (Charles, 2026-10-18, widened to online co-op on 2026-10-19): Charles can't test controller, split-screen or online co-op play for a while, so every build lays the groundwork (snapping IDs, per-screen state, no mouse-only actions) from the first menu and code review checks it, while their in-game tests are deferred until he can run them (§16; doc 17 §8b owns the rotation). All three stay 1.0 requirements.
- **GMCM holds hints, celebrations and economy knobs** (Charles, U3). Balance targets apply at the defaults.
- **Clock freeze** (Charles, doc 01): any mandatory animation that stops the player acting freezes the in-game clock; optional ones skip with one press. With farmhands connected, celebrations become non-blocking overlays or stay under about one second (doc 07).
- **Three celebration tiers** (toast, hold-up, codex flourish), the anticipation beat, feedback styles by apparatus kind and the toast-frequency option (approved, doc 07).
- **Chaining is learnable by looking** (Charles, 2026-10-16; doc 13 §7): direction on the sprites, live links, a chain overlay, tooltips naming links, and Tobin's lesson.
- **Recipe memory is visible** on the machine and in its tooltip, with a pin mark (doc 13 §6a).
- **The codex is the recipe browser in 1.0**; Lookup Anything recipe rules are 2.0 (Charles, 2026-10-16).
- **The Table's crowded line**: a three-item load containing a defined pair fails as usual and adds "something here is feeling crowded" (Charles, 2026-10-18; doc 07).
- **Finch's curiosity hint never shows in his shop tooltip**, only once the item is owned (Charles, 2026-10-17; doc 07 layer 4). This doc enforces it (§9).
- **Inventory sort order**: base Primes first, then substances by tier, then endgame Elements, alphabetical within groups (Charles, 2026-10-18; doc 03b rule 5). Every list in our own menus uses the same order.
- **Quarter-day time share** without busywork (Charles, doc 02): this doc sets the budget and the levers that meet it (§17).
- **No late-install fast start** (Charles, 2026-10-18): GMCM offers no option to skip ahead; every save, however late it installs the mod, plays from the soft start (doc 14 §9).
- **Quality uses vanilla words and stars** (Charles, 2026-10-18): screens say "base quality" and show the silver, gold and iridium star icons, never the Latin stage names (§2).
- **This doc's defaults, confirmed** (Charles, 2026-10-18): level 2's extra Satchel tab adds capacity, a second page of type slots (§4); staged items can be taken back from the tray until the recipe completes (§5); recipe-memory icons draw only with the overlay by default (§5, §15); the codex keeps every seen hint on a Notes page (§7); the overlay and codex keys start unbound, since the Satchel covers both (§15).
- **i18n from the first build** (S4): no player-facing string is hard-coded or baked into a sprite.
- **Reagent Satchel** behaviour (doc 03b, Charles 2026-10-13 and 2026-10-18): tooltip contents, chest-style grid with tabs, auto-collect of reagents only, the picker filtered to a machine's valid inputs.

## Player-facing spec

### The principles every screen follows
1. **Looks vanilla.** Menus use the game's own frames, fonts, scroll and button sprites, so the mod reads as part of Stardew. New art is limited to icons, machine sprites and a few overlays (doc 21).
2. **One verb, one meaning.** The same button does the same kind of thing on every apparatus (the interaction model below). A new apparatus never introduces a new control.
3. **Nothing needs a mouse.** Every action has a controller path; nothing depends on hover alone, since every hover tooltip also appears for the controller's selected tile or slot.
4. **Silhouette means "not yet known", red means "broken".** Undiscovered things are never shown in red, and a red mark always names what is wrong.
5. **Shape and colour together.** Every state that uses colour (link working, idle, broken; Element identity) also has a shape or icon, for colour-blind players.
6. **Say why.** When something refuses (a machine rejects an item, an altar won't trigger, the Table can't make a pair), one line says why and, where the rules allow, what would help.
7. **No busywork.** No task needs the same menu twice, a walk between buildings to move intermediates, or a trip to check whether something is ready (§17).

### The interaction model
| Input (keyboard and mouse / controller) | On an apparatus | Elsewhere |
|---|---|---|
| **Action** (right-click / A), holding an item | hand-load it (doc 13 §6.1); if the item can't extend the tray toward any recipe here, the machine shakes and says "This doesn't belong here" | vanilla behaviour |
| **Action**, holding the **Satchel** | open the **picker** (§4) | open the Satchel grid |
| **Action**, empty-handed or holding a tool | collect a ready or locked output; on a loading machine, open the **tray view** (§5); on a staged altar, the **trigger prompt** (§12); on the Experiment Table, the **Table menu** (§3) | vanilla behaviour |
| **Use tool** (left-click / X), holding the Satchel | open the Satchel grid | open the Satchel grid |
| **Use tool**, holding the **Assayer's Lens** | assay the apparatus (its codex entry and known neighbour effects) | assay the item, place or fixture (§11) |
| **Hover** (mouse) / **selected tile** (controller) | the machine tooltip (§5) | item tooltips show the item's Elements (§11) |
| **Codex key** (GMCM keybind) / the codex button in the Satchel grid | open the codex (§7) | same |
| **Overlay key** (GMCM keybind) / the overlay toggle in the Satchel grid; holding the Satchel | full / light chain overlay (§6) | — |

The Satchel is the controller player's home base: holding it gives the light overlay, its grid has a codex button and a full-overlay toggle, and its picker loads machines. A controller player never needs a keybind they don't have.

### The first evening, as the player sees it
The slice's first ten minutes (doc 11) touch, in order: the cutscene (clock frozen, skippable after the first view); the quest log entry for *Take it apart*; the Satchel's tooltip and its first "+2 Ignis" HUD notice; the Athanor tooltip ("Decomposes: any item"); the Table menu and its anticipation beat; the first toast; the codex opening once to the Tier 1 grid (skippable). Every one of these screens is a slice screen (§2) and must work on a controller in the first build.

## Rules & systems

### 1 · Screen inventory
| Screen | Kind | Slice | Owner of its rules |
|---|---|---|---|
| Experiment Table menu | custom menu | ✓ | doc 07, doc 12 §3 |
| Satchel tooltip, grid and picker | tooltip + custom menus | ✓ | doc 03b |
| Machine tooltip, tray view, in-world indicators | tooltip, small panel, sprites | ✓ | doc 13 |
| Chain overlay | world overlay | ✓ (minimal: arrows and broken links) | doc 13 §7 |
| Codex | custom menu | ✓ (Primes, Tier 1, Tier 2, Apparatus, Unlocks, Notes) | doc 07 |
| Toasts, hold-ups, flourishes, HUD notices | HUD and vanilla hold-up | ✓ (no Cauda Pavonis) | doc 07 |
| Bundle board | bundle menu | ✓ (the Furnace Room only) | doc 14 |
| Assayer's Lens results | small panel | ✓ | doc 07 |
| Skill page and level-up | SpaceCore | ✓ (levels 1–4) | doc 05 |
| GMCM page | GMCM | ✓ (slice options, §15) | this doc |
| Lab mark and overlay tints | tooltip line, overlay | ✓ (T1) | doc 18 |
| Finch's shop | vanilla shop menu | — | doc 07 |
| Altar trigger prompt and staged notice | dialogue box, HUD | — | doc 04 |
| Residue divination | bundle-board panel | — | doc 07 |
| Familiar door hover, greeting, gifts | tooltip, vanilla dialogue | — | doc 18 |

### 2 · Shared building blocks
- **Element icons and colours.** Every Element has one icon and one colour (`Primes.Color` for Primes); every list, grid, profile bar and overlay tint uses them. The icon carries the Element's rune, so identity never rests on colour alone (placeholder art per the art-sources notes; commissioned art per doc 21).
- **Kind marks.** ⚙︎ engineered, ✦ arcane, both marks for blended machines; shown after every apparatus name in tooltips and the codex, matching doc 13's `FeedbackStyle`.
- **Silhouettes.** An unknown item, recipe or unlock is its icon filled flat in a dark tint, with "?" for its name. A silhouette may carry one hint line when a hint channel has revealed it (`hint:` set).
- **Quality is shown the vanilla way** (Charles, 2026-10-18): in game, quality is always "base quality" and the vanilla silver, gold and iridium star icons, on items, in tooltips, the codex, recipe rows and bonus lines ("chance of a higher quality"). The design docs' stage names (Nigredo, Albedo, Citrinitas, Rubedo = quality 0, 1, 2, 4; doc 12 R1) are internal terms only and never label quality on screen.
- **Status icons** (small, top-right of a machine sprite, never more than one at a time, in this priority): locked "something new inside" sparkle → ready bubble (vanilla) → staged glow (altars) → waiting icon (a gated recipe waiting idle for its day, weather or moon; doc 12) → no-fuel icon → nothing. The sparkle and the ready bubble look clearly different, since one means "collect by hand to discover".
- **The one-line reason.** Refusals, waits and failures use one i18n line drawn from a small fixed set (missing apparatus, missing neighbour state, wrong day, wrong weather, wrong moon, no fuel, output locked, hand-made only, not discovered yet), so the wording is consistent everywhere.

### 3 · The Experiment Table menu
**Layout.** A vessel in the middle with **two input slots** around it (three once the Library is restored: the third slot fades in with a short flourish the first time). Below it, the player's inventory with a Satchel tab beside it, so both sources are one press away. To the right, a **result panel** and a **Combine** button. Above the vessel, a **neighbour line** naming the Table's current neighbour state ("Beside: a charged Lightning Coil (East)"), since the Table respects neighbours (doc 13 §5).

**Before Combine**, the result panel already tells the player what the press will do, so nothing is wasted by accident:
| Slots hold | Result panel shows | Combine |
|---|---|---|
| a known recipe (`recipe:` set) | its output icon and name, "Known" | makes it at base quality (doc 07) |
| a signature already tried here (`tried:` set) | "Tried before: residue" with the residue icon | allowed, but asks once ("You tried this. Try again?") |
| anything else | a question-mark vessel | plays the attempt |

The panel never reveals an unknown recipe before Combine; it only reflects what the codex already knows.

**After Combine**, in doc 07's outcome order:
1. **A defined, Table-attemptable recipe with its Required conditions met:** the anticipation beat (1–2 seconds of simmer and glow, clock frozen because a menu is open), then the result rises from the vessel into the result panel, the toast (or hold-up, for firsts that matter) plays and the codex records it. A Power or Process-state result plays a puff and a reveal card instead of an item.
2. **The inputs of a defined recipe that isn't Table-attemptable, or whose Required conditions fail:** nothing is consumed; the vessel hums and the reason line names the missing apparatus or condition kind ("This wants an altar, and the dark").
3. **Otherwise:** a comic puff, residue into the result panel, a small XP number, and the signature marked tried. With three inputs containing a defined pair, the crowded line follows: *"Something here is feeling crowded."*

**Convenience.** After a result, the slots keep the same items if the player still holds them, so trying a variant is one swap. Results go to the Satchel (reagents) or the inventory (products) by doc 03b's rule; if both are full, the result stays in the result panel until there is room.

**Controller.** The cursor starts on the first empty slot; A on an inventory item places it in the next empty slot; A on a filled slot returns it; Y is Combine; the shoulder buttons switch between inventory and Satchel.

### 4 · The Reagent Satchel
**Tooltip** (hovering the Satchel in the toolbar or inventory): a compact grid of the contents with counts, in the sort order of doc 03b rule 5, capped at three rows with "+12 more" when longer. The icon shows a fill state and a count badge (types held / capacity).

**Grid** (Action or Use tool while holding it): a chest-style menu with doc 03b's three category tabs (**Essences**, **Substances**, **Products**) along the top, the player inventory below, a **codex button** and a **full-overlay toggle** in the corner (§6). Items move by click or drag exactly as in a chest; shift-click (or the controller's Y) moves a whole stack. Capacity counts **item types** across all tabs (≈24 at the start). A full Satchel shows its badge in red and the "+3 Ignis" notice says "Satchel full: to your inventory" instead.
- **Level 2's extra tab** (doc 05) is read as **more capacity**, not a fourth category: it adds a second page of type slots to the grid (placeholder +12 types) (Charles, 2026-10-18).
- **Tobin's capacity upgrades** (later builds) add slots the same way.

**Picker** (Action on an apparatus while holding the Satchel): a small panel anchored beside the machine.
- **Recipes row:** the discovered recipes this apparatus can run in its **current neighbour state**, as output icons. Selecting one loads its whole input multiset from the Satchel and inventory in one press and sets the machine's **recipe memory** (doc 13 §6a). Recipes whose inputs the player doesn't hold appear dimmed with the missing input named on hover. Undiscovered recipes never appear here.
- **Items grid:** every carried item that can extend this machine's tray toward a recipe it can run now, discovered or not, for manual building and first attempts. This filter is the same deliberate gentle hint as hand-loading acceptance (doc 13 §6.7).
- **Tray strip:** what is already staged, with a take-back button per item (§5).
- **Pin toggle:** a pin icon; a pinned machine asks before a different recipe is loaded (doc 13 §6a).
- **Controller:** opens with the cursor on the remembered recipe, so "refill this machine" is two presses (A, A).

**HUD notice.** "+3 Ignis" with the item icon, merged for pickups within about two seconds ("+3 Ignis, +1 Aqua"), toggle in GMCM (doc 03b).

### 5 · Machines: tooltip, tray view and in-world indicators
**Tooltip**, in a fixed order so every apparatus reads alike; lines that don't apply are omitted:
1. Name and kind mark ("Alembic ✦").
2. **Status:** Empty · Loading (1 of 2 inputs) · Processing (2h 10m) · Ready · Something new inside (collect by hand) · Staged (trigger by hand) · Waiting: the recipe runs on Sundays (or another one-line reason, §2).
3. **Recipe memory:** the remembered output's icon and name, with the pin mark when pinned; "Not set: load a recipe by hand once" on a fresh machine (doc 13 §6a; the D4 default).
4. **Neighbour state:** "Heated by: Athanor (West)" · "Cold" · "Powered by: Boiler (North), 4h left".
5. **Links** (doc 13 §7): "Takes from: Calcinator (West), chest (South) · Sends to: Process Vat (East)". A broken link reads in red with its reason: "Sends to: Retort (East): can't use Calx".
6. **Room bonus:** "Laboratory I: 10% faster, 5% chance of a higher quality" (doc 18), and a Familiar's boost when it applies.
7. For power sources: what they emit, charges or time left, and which machines they feed, numbered in their fixed N, E, S, W order (doc 13 §5).

**Tray view** (Action, empty-handed, on a loading machine): a small panel showing the staged items and the recipes they are heading toward as silhouettes or icons, with a **take-back** button per item until the recipe completes. Taking back returns items to the Satchel or inventory. This undoes a mis-load without a removal command (Charles, 2026-10-18).

**In-world indicators.**
- The **status icon** (§2) above the sprite.
- The **recipe-memory icon**: a small output icon on the machine's front, with a pin dot when pinned. GMCM chooses when it is drawn: always, only with the overlay (default), or never. The default keeps a large Lab from looking cluttered (Charles, 2026-10-18).
- **Sprites show direction**: an intake on the West face and a spout on the East face of every ValleyOpus machine, in both art styles and in placeholder art (doc 13 §7, doc 11).
- **Feedback styles** when a cycle completes: arcane glow and shimmer, engineered clunk and steam puff, blended machines both (doc 07).

### 6 · Chaining visual aids (doc 13 §7)
**Live links.** While an item passes between two machines, or between a machine and its South chest, a short flowing-particle line runs along the link for about a second, in the moving item's Element colour. GMCM can turn the particles off.

**The chain overlay** has two levels:
| | Light (holding the Satchel) | Full (the overlay key, or the toggle in the Satchel grid; stays on until toggled off or the player leaves the location) |
|---|---|---|
| Links | arrows for active links: machine → machine (West to East) and chest → machine (South) | the same, plus idle links as dashed arrows |
| Machines | none | machines that receive nothing are greyed; broken links get a red ✕ with the reason icon |
| Power | none | each power source's reach, numbered in its N, E, S, W order, with "full" when `MaxConsumers` is reached |
| Room | none | hearth-heated tiles tinted warm, dark tiles shaded (doc 18), the Familiar's boosted machines outlined |
| Recipe memory | none | every machine's remembered-output icon and pin |
| Layout guide (Labs only) | none | optional faint marks of the default floorplan's line rows and walkways (doc 18 §2d), GMCM toggle, off by default |

Link states use shape as well as colour: solid arrow (working), dashed (idle), ✕ (broken).

**Placement preview.** While placing a ValleyOpus machine, its ghost shows the arrows it would form with its West, East and South neighbours and marks a link that could never work (for example, an East neighbour with no recipe taking this machine's outputs) with a faint ✕. This teaches the West-in, East-out rule at the moment the player decides where things go.

**The slice's minimal overlay** (doc 11): the light level plus broken-link marks, and the tooltip links line. Particles, power reach, room tints, the layout guide and the placement preview arrive with the full overlay before Tobin's chaining lesson (Cistern).

**Teaching moment** (doc 02, doc 13): Tobin's first-line lesson turns the full overlay on for the scene, has the player place three machines in a row, and ends with a one-line card naming the overlay key and "or hold your Satchel".

### 7 · The codex
**Opening it.** The codex key, the button in the Satchel grid, or the bookstand in the Entrance Hall. It opens on the last page viewed (per player), except for the one-time auto-open on a tier's first discovery (doc 07).

**Pages**, in this order (doc 07's `Codex/Pages` groups; pack pages slot in by `SortOrder`): Primes · Tier 1 grid · Tier 2 · Dark twins · Metals · Great Work · Apparatus · Places · **Unlocks** (doc 14) · **Notes**. The shoulder buttons (or tabs on the left edge) move between pages; pages with nothing discovered yet still show, filled with silhouettes, so the scope is visible from day one.

**The Tier 1 grid** is a triangle with the Primes along both edges (n(n+1)/2 cells, derived from the `Primes` asset, so a pack-added Prime grows it). Each cell is one of: **discovered** (the output icon), **warm** (a shimmering silhouette, for a pair adjacent to known ones; doc 07 layer 2), **tried** (a small residue mark: this pair failed at the Table), or **unknown** (blank). The grid is the codex's front door and the Tier 1 row-complete flourish plays on it.

**An entry** (selecting any cell or item):
- Icon, name, class (Item, Power, Process state) and flavour text.
- **Prime profile** bars, once assayed.
- **Routes:** every discovered recipe that makes it, twin routes side by side (⚙︎ / ✦), each with inputs, apparatus kinds, neighbour state, and Required and Bonus conditions. Undiscovered routes of a discovered item show as "???" rows (doc 04's "?" convention, written the same way as **Used in** below), so the player knows another way exists without learning what it is.
- **Uses** line (doc 15): "Drink: Speed +1, 4 min" · "Dye: strong blue" · "Sold at Agnes's".
- **Used in:** discovered recipes that take it as an input, plus one row marked **"???"** for each undiscovered recipe it is used in, so the entry shows how much is left to learn about it (Charles, 2026-10-18). A "???" row shows nothing else: not the other inputs, the output or the apparatus.
- **Lore:** lines unlocked at 1, 10 and 50 crafted, with a progress pip toward the next (doc 07); hand-made-only entries say "hand-made crafts" beside the count.
- **Discovered by** a player and day, in multiplayer (doc 07).

**Notes page.** Every hint the farm has seen (`hint:` keys), grouped by channel (Oriel, Tobin, Hesper's almanac, Finch's curiosities, Lark, the Curator, divination, dreams, readables), each linked to its target's silhouette. Hints are never lost to a closed dialogue box (Charles, 2026-10-18).

**Finding things.** A search box (vanilla text box; the on-screen keyboard on a controller) matches names in the current language. Filters: by Prime, by apparatus kind, "I can make this now" (discovered recipes whose inputs are in the Satchel and inventory), and "not yet automated" (discovered items with no discovered automatable route). Results list in doc 03b rule 5's order.

**Controller.** The d-pad moves between cells, A opens an entry, B goes back, X opens search, the shoulders change page.

### 8 · Celebrations and notices
**Toasts** use the vanilla HUD message area (bottom left): icon, name, one flavour line. Discoveries within about two seconds merge into one "3 new discoveries" toast that opens the codex Notes or grid on click. Other players see a quieter one-line notice ("Alex discovered Vapor") without the sound (doc 07).

**Hold-up pose and sound** use vanilla's hold-up-item path for firsts that matter (first Tier 2, first dark twin, capstone items).

**Codex flourish** for milestones: the codex opens on the right page and plays a short page-turn with a sound; one press skips it.

**Cauda Pavonis**: an iridescent shimmer on the machine and the item icon the first time an item is raised from base quality to silver (the design docs' Nigredo → Albedo).

**Clock freeze, precisely.** In single player the clock already stops while a menu is open, so menu-borne celebrations (the Table, the codex flourish) freeze it for free. World-borne ones (the hold-up, a restoration scene) pause the clock for their duration; to verify that the vanilla hold-up path does so. With farmhands connected, nothing pauses the shared clock: the hold-up plays as a non-blocking overlay and world animations stay under about one second (doc 07; the multiplayer test is doc 17's).

**HUD notices** (non-celebratory) share one style and merge like toasts: the Satchel pickup notice, "Conjunction Altar is staged" (doc 04), "Learned: Calcinator" for crafting recipes (doc 14), "Your next level arrives in a later build" (slice, doc 11). Never more than three notices stack; older ones fold into "+2 more".

### 9 · Finch's shop (enforces doc 07 layer 4)
Finch's shop is the vanilla shop menu, so buying feels like any travelling merchant.
- A curiosity's item **Description** is a generic line ("A curiosity from the road. Its secret is for its owner."). The **hint text** is a separate i18n key, never in the item data.
- Our tooltip code appends the hint text only when the item is **owned**: in the player's inventory, the Satchel, a chest, or as its codex Notes entry. A shop's for-sale list is never "owned", so neither vanilla's shop tooltip nor a mod that reads the item's description (Lookup Anything on the shop stock) can show it.
- **Never a hint for something already known** (Charles, 2026-10-18): Finch stocks a curiosity only if its hint's target is still undiscovered (no `recipe:` key for a recipe target, no `item:` key for an item target). If the farm discovers the target while the curiosity is still on sale, it leaves his stock and an undiscovered one takes its place, so he never sells a hint that points at something the player already knows.
- Buying one sets its `hint:` key, so it joins the Notes page at once and is never sold as new again by divination.

### 10 · The bundle board (doc 14)
- **Layout** (Charles, 2026-10-20): the board looks like vanilla's bundle menu: an overview of the open rooms' bundles, then a page per bundle with its slots. Like the CC's, a room's bundles appear only once the room is open (doc 14 §2), so the board never shows rooms that aren't open yet and never spells out the order. In the slice only the Furnace Room's bundles appear (doc 11).
- **Rewards stay hidden** until a bundle is complete, as in vanilla (Charles, 2026-10-20).
- **A shining particle effect** on the board, so it's obvious the player should look at it (Charles, 2026-10-20).
- **Bundles:** vanilla items as icons; Elements as silhouettes with a one-line hint until discovered (doc 14). A slot's hover names the item, how many are needed and, for a discovered Element, its codex link.
- **Residue divination** (from the Library): a panel on the board lists the hints divination can buy, each as its target's silhouette with its residue cost, plus "Surprise me" at the lowest cost. **Bulk surprise** (Charles, 2026-10-18): once the farm has divined 5 times, "Surprise me" gets a count selector from 1 to 10, priced at that many times the single cost, and each hint is drawn without repeats from the hints not yet shown (`hint:` unset). If fewer hints are left than the count asked for, the selector stops at what is left. Buying plays a small smoke flourish and adds the hint to the Notes page.
- **Framework:** the board is our own menu, not Unlockable Bundles (Charles, 2026-10-19; doc 09 §10), so the Element silhouettes and the divination panel are all part of it.

### 11 · The Assayer's Lens
- **On an item:** a small panel with the item's Prime profile as coloured bars with Element icons, and the codex entry it opened. The first time only, it sets `assay:` (doc 12 §10).
- **Elements in tooltips** (Charles, 2026-10-20 and 2026-10-22): once the intro has played, an item's tooltip shows a row of Element icons for every Element in it that the farm has discovered, as **icons only, with no amounts**, plus one **"?"** icon in the same style when at least one of its Elements is still undiscovered. **Assaying** the item adds the units and clears the "?", so the row becomes its full profile (doc 12 §10). Key items get no row: tools, quest items and irreplaceable items. GMCM chooses always (default), only while holding the Lens, or never.
- **On a place or fixture:** a lore box in the vanilla dialogue style, and a toast if it revealed a hint.
- **Level 4, a whole chest** (doc 05): one panel listing every newly assayed item in the chest with its profile, scrollable.

### 12 · Altars and rituals
- **Staging:** a staged altar shows its ready glow and, if enabled, the HUD notice (doc 04).
- **Trigger prompt** (Action on a staged altar): a vanilla question box listing the recipe and each condition with ✓ or ✕ ("Full moon ✓ · Thunderstorm ✕"), then Trigger / Not yet. If a Required condition fails it says what would help ("Hesper's almanac lists the next storm") and the materials stay staged (doc 04). If the recipe allows a partly wrong trigger, the prompt says it will give a lower stage plus residue.
- **Grand rituals** run for a day or more with a world animation; only their start (under a second) can't be skipped.
- **The Mystic perk** that triggers nearby staged altars together (doc 05) uses the same prompt, with one row per altar.

### 13 · Skill, Lab and Familiars
- **Skill:** the SpaceCore skills page and level-up screen (doc 05); each level-up lists what it opened. Professions use SpaceCore's two-option menu.
- **Lab:** the hover Lab mark on apparatus (§5) and the overlay tints (§6). Robin's menu text names the tier's bonus (doc 18).
- **Familiars:** the Lab door's hover names the bonded Familiar and its element; greeting and gifts use the vanilla talk and gift verbs; a boosted machine sparkles faintly while the Familiar is near (doc 18).

### 14 · Products and buffs
Products follow doc 15 with no new UI beyond the **Uses** line in tooltips and the codex, and the **Apply** prompt for non-edible products, which reuses vanilla's eat prompt. Buffs, including the custom ones, use the vanilla buff bar with an icon, timer and tooltip.

### 15 · GMCM options
Grouped as the GMCM page shows them. **Per player** options are saved locally; **farm** options are the host's (their sync is doc 17's).
| Group | Option | Default | Scope | Slice |
|---|---|---|---|---|
| Notifications | Toast frequency: all / milestones only | all | player | ✓ |
| | Satchel pickup notice | on | player | ✓ |
| | Staged-altar notice | on | player | — |
| | Other players' discovery notices | on | player | ✓ |
| Celebrations | Skip celebrations (hold-ups and flourishes become toasts) | off | player | ✓ |
| | Auto-open the codex on a tier's first discovery | on | player | ✓ |
| Hints | Prime profiles in tooltips: always / holding the Lens / never | always | player | ✓ |
| | Warm hints on the codex grid | on | player | ✓ |
| Overlay | Overlay key | unbound *(placeholder; checked against vanilla and common mods' binds)* | player | ✓ |
| | Holding the Satchel shows the light overlay | on | player | ✓ |
| | Recipe-memory icons on machines: always / with the overlay / never | with the overlay | player | ✓ |
| | Link particles | on | player | — |
| | Lab layout guide | off | player | — |
| Controls | Codex key | unbound *(placeholder)* | player | ✓ |
| Accessibility | Reduced motion (no shimmer, shake or particles; still images instead) | off | player | ✓ |
| Economy | `Decomposition/Settings` knobs: `k`, `e`, `QualityBonus`, `Threshold`, `ExchangeRatio`, default residue (doc 12) | the data's values | farm | ✓ |
| | Later knobs from the sweep (prices, yields; doc 06) | the data's values | farm | — |
| Debug | Verbose logging (doc 11 §6) | off | player | ✓ |

### 16 · Controller and split-screen
- **Menus** are built on the vanilla menu base with clickable components and neighbour IDs on every control, so the game's own snapping moves the cursor; every menu sets a default component when it opens. Text entry uses the vanilla on-screen keyboard.
- **Button map** inside our menus: A select or place, B back, X search (codex) or take back (tray), Y Combine (Table) or move stack (Satchel), shoulders switch tab or page, Start closes. Outside menus, nothing new is bound on the controller by default: the Satchel covers the overlay and codex (§ interaction model).
- **Selected-tile tooltips:** the machine tooltip follows the controller's grab tile as well as the mouse.
- **Split-screen:** every menu, overlay, toast and tooltip is per screen (per-screen state, sized to that screen's viewport); one player's overlay or celebration never draws on the other's screen. Menus are laid out to fit the smallest split-screen viewport at the default zoom.
- **Testing:** until Charles can test in game, the controller and split-screen rules above are kept by construction and by code review (every control has neighbour IDs, every menu a default component, nothing reads the mouse directly); in-game checks join the test plan once he can run them (Charles, 2026-10-18; the same deferral covers online co-op, doc 17 §8b).
- **Two players at one Table** (co-op or split-screen): the Table opens for one player at a time and says "Alex is using this", like a chest. How that is synced is doc 17's.

### 17 · The hand-action budget (doc 02, doc 06)
**The target.** A Stardew day runs 6:00am to 2:00am, about 14 real minutes. A heavily invested mid-game player spends **about a quarter of it, roughly 5 in-game hours, on alchemy**, counting walking to and inside Labs and the workshop, loading, collecting, triggering, Table attempts and menu time. **"Heavily invested" means the top tenth of players** (Charles, 2026-10-18): someone who strongly prioritizes alchemy over other activities, at about the 90th percentile of investment, not a middle-ground player. The reference setup is sized to match (placeholder: Summer to Fall of Year 1, the largest Labs and lines a diligent alchemy-first player has by then, about 30 apparatus and an altar or two; the sweep sets the real size). A typical player spends well under a quarter of the day.

**How it is measured.** The slice report (doc 11 §4) logs per day: hand loads, collections, Table attempts, altar triggers, menus opened, and in-game minutes spent in Labs and the workshop. The sweep (doc 06) compares a representative setup against the target. Spring is hand work and may run higher for a player who chooses it; the target is for the mid-game.

**The levers**, each a rule for the screens above:
| Chore | Without the lever | With it |
|---|---|---|
| Load a recipe by hand | one press per input | one press from the picker's Recipes row (§4) |
| Refill a line | visit every machine | recipe memory + chaining or Automate: refill only the first machine's South chest (doc 13) |
| Collect a line's output | visit every machine | the last machine sends East to nothing, so its output lands in its South chest: one chest to empty |
| Check what is ready | walk to look | status icons, staged-altar notices and the waiting reason in tooltips (§2, §5, §8) |
| Find a broken link | trial and error | the full overlay's red ✕ with its reason (§6) |
| Trigger several altars | one prompt each | the Mystic perk's grouped prompt (§12) |
| Look up a recipe | walk to the workshop | the codex anywhere, from the Satchel (§7) |
| Retry an experiment | rebuild the slots | slots keep the last items (§3) |
| Undo a mis-load | a removal command | the tray view's take-back (§5) |

**Busywork test** for any new screen or mechanic: if a routine daily task needs the same menu twice, a walk between buildings to move intermediates, or a trip just to check a state, it fails and needs a lever.

### 18 · Strings and readability
- Every string is an i18n key (S4). English sets the length; layouts leave about 40% spare width for longer languages, and long lines wrap rather than shrink.
- No text in sprites; numbers use the game's own number font and the player's locale.
- Item names, flavour and reason lines follow doc 03a's naming rule (Latin first, English gloss second) and doc 01's lore rules.

### 19 · Edge cases
- **Multiplayer:** all our menus are local to a player; the codex, unlocks and bundle board read the host's farm state (doc 17). Toasts go to the discoverer, quieter notices to everyone else. Clock freeze becomes non-blocking with farmhands (§8).
- **Joja route:** the bundle board and Unlocks page read doc 14's flags, set on both routes; no screen names the CC.
- **SVE and tie-ins:** our screens list pack and tie-in content by the same rules (codex pages by `SortOrder`, sort order by doc 03b rule 5); no screen assumes eight Primes or vanilla items only.
- **Automate:** machine tooltips show the same status and links whether Automate is installed or not; the links line reads "Automate" for the third priority (doc 13 §7).
- **Walk of Life:** a respec changes no menu; level-granted recipes stay visible in the codex (doc 05).
- **Uninstall (S3):** only GMCM config and per-player codex view state are ours; nothing in this doc is stored in the save beyond doc 07's codex keys.
- **Other UI mods:** UI Info Suite 2 and Experience Bars show our skill through SpaceCore; Lookup Anything shows machine state and generic descriptions only (§9); Chests Anywhere does not treat the Satchel as a chest; Better Crafting draws from the Satchel (doc 09). Each is a doc 17 test item.

## Data shape
| Record | Kind | Fields |
|---|---|---|
| Player UI config | GMCM config, per player, local file | the per-player rows of §15, keybinds |
| Farm config | GMCM config, host (sync by doc 17) | the economy knobs of §15 |
| Codex view state | C# state, per player, local mod data | last page, search text, filters |
| Per-screen UI state | C# state, not saved | open menu, overlay level, notice queue, cached link graph for the current location |
| Reason lines | i18n | one key per reason in §2's fixed set |
| `Hints` (doc 07) | CP data | unchanged; Finch's curiosities use `Channel` = Finch, and the curiosity item's `Description` is the generic line (§9) |
| Machine display | reads doc 13's `modData` | no new fields: status, recipe memory, pin, neighbour state and links all come from doc 13's machine state |

The UI adds no save data and no CP asset of its own; it reads the assets of docs 07, 12, 13, 14 and 15.

## Content inventory
- **Custom menus:** 5 (Experiment Table, Satchel grid, picker, codex, tray view), plus the divination panel and the Lens panel.
- **World overlays:** the chain overlay (two levels), live link particles, placement preview, the Lab layout guide.
- **Icons:** one per Element (with rune), the kind marks, six status icons, the link shapes (solid, dashed, ✕), the pin, about ten reason icons.
- **Sounds:** vanilla cues where one fits (page turn, hold-up, puff, clunk); a few new ones for the anticipation beat and Cauda Pavonis (doc 21).
- **Strings:** about 200 UI keys (menus, reasons, notices, GMCM), separate from item and hint text.

## Tech hooks
- **Menus:** subclasses of the vanilla menu base with clickable components for controller snapping; these are UI classes, never saved, so uninstall safety is unaffected.
- **Tooltips:** our machine tooltip draws on hover through a SMAPI render event; item tooltip lines (profiles, the Finch hint) are appended through a Harmony postfix on the item tooltip path. Link and power lines are computed when the hovered tile changes, from a per-location link graph cached and rebuilt on object placement, removal and location change (doc 13 and doc 18 both list hover cost as a risk).
- **Overlay:** drawn on the rendered-world event from the cached link graph; particles are temporary sprites, capped per screen.
- **HUD:** toasts and notices extend vanilla HUD messages; the hold-up uses vanilla's hold-up path.
- **SpaceCore** for the skill page; **GMCM** API for §15.
- **Per-screen state** with SMAPI's per-screen wrapper for everything in the per-screen record.
- **Risks to verify (doc 09 §14):** controller snapping in a triangular grid and the smallest split-screen viewport (S6, S7, deferred with the in-game controller and split-screen runs); tooltip postfixes alongside UI Info Suite 2 and Lookup Anything; overlay frame time in a full T3 Lab (S9). The hold-up pausing the clock passed in B1 (S5); the bundle menu is settled (D20).
- **1.7:** nothing here depends on 1.7; menu base classes are stable across 1.6.

## Balance hooks
- **Hand actions per day** and minutes in alchemy locations (§17), from the slice report, against the quarter-day target.
- **Divination costs** are shown, not set, here (doc 07, doc 06).
- Placeholders: the notice merge window (about 2 seconds), the Satchel level 2 increment (+12 types), the stack cap of three notices, particle caps.

## Open questions
Every open item across the docs is also collected in [doc 10](10-roadmap-and-open-questions.md#open-questions).
- `[blocks-content]` **Art and sound** for icons, overlay shapes and the anticipation beat. *Owner: doc 21.*
- `[2.0]` Lookup Anything recipe rules (doc 13 §6.6).

## Change log
- **v0.2 (2026-10-21):** docs cleanup sweep: Charles's sink-or-swim feedback (2026-10-20) moved here from the to-do list: the board looks like vanilla's bundle menu, shows a room's bundles only once the room is open (no sealed rooms), hides rewards until a bundle is done and shines so players notice it; item tooltips show an item's farm-discovered Elements as icons plus a "?" from the end of the intro, never on key items, and assaying adds the units and clears the "?" (Charles's card, 2026-10-22). Risks follow the B1 spike results.
- **v0.1a (2026-10-19):** pre-gate cleanup sweep: marked ready (merged as PR #17); the in-game test deferral now also covers online co-op (doc 17 §8b).
- **v0.1 (2026-10-18), Charles's review:** the quarter-day budget's "heavily invested" player is the 90th percentile of alchemy investment; bulk surprise divination (up to 10 at once, proportional residue) after 5 divinations; Finch never sells a curiosity whose hint points at something already discovered; quality shows on screen as "base quality" and the vanilla stars, never the Latin stage names; a codex entry's Used in list shows a "???" row for each undiscovered recipe that uses the item, and undiscovered routes use the same "???" mark; controller and split-screen groundwork from the first build, with in-game tests deferred while Charles can't test.
- **v0.1 (2026-10-18), Charles's answers:** no late-install fast start (the GMCM row and open question are removed); defaults D15–D19 confirmed.
- **v0.1 (2026-10-18):** first draft: principles, the interaction model, the screen inventory with slice flags, the Experiment Table menu with its before-Combine result panel, the Satchel tooltip, grid and picker, the machine tooltip order and tray view, the chain overlay in two levels with the placement preview, the codex pages, entries, Notes page and search, celebrations and clock freeze, Finch's shop hiding the curiosity hint, the bundle board and divination panel, the Lens, altar prompts, GMCM options, controller and split-screen rules, and the quarter-day hand-action budget with its levers.
