# Testing ValleyOpus

**This build:** `1.0.0-sos.2`, the **sink-or-swim** build. It holds everything slated for 1.0 at once: all 14 apparatus, power (Boiler, Lightning Coil, kindled Athanor), rituals and the Circle, every Tier 1–3 substance and product, the workshop under the Community Center (one big space; each room's corner and bundle board appear as it opens) with 25 bundles, Oriel's five starter quests, the unlock chain, 10 NPCs with shops, Dr. Sorenson's contracts, hints and divination, Labs T1–T3, the four Familiars, Shades and the Gloam Lantern, Ley Taps, the full Alchemy skill (10 levels, 6 professions), worn and applied products, and a config menu (Generic Mod Config Menu, if you have it). Art is placeholder from open-license packs (see `CREDITS.md`).

**What "sink or swim" means here:** nothing in this build has run in the game before. Expect the first compile to need fixes and expect things to break. The goal is to find out how far it gets: play, note what breaks, share the log, and keep going with the dev commands when something blocks you. **Use new farms**; don't load a farm you care about. sos.2 changed the workshop map, so **start fresh farms** for this round too.

**New in sos.2 (your Pass A/B feedback):** the workshop is one screen with no room doors; each room's bundle board appears (with a sparkle) only once that room is open, and the board looks like the vanilla bundle screen with rewards hidden. Oriel's intro waits until your next walk into Town after the Community Center opening scene, and covers what the welcome letter used to say (the letter is gone). Oriel gives five starter quests in a row. Finch's wagon stands in Cindersap Forest, just east of the travelling cart's spot. Item tooltips list the Elements you've discovered inside them, with a ? if some are still unknown. The Curator's name is revealed in a cutscene once every room is restored. Breaking items down is faster (Athanor 30 minutes, Calcinator 10).

**Checked with:** Stardew Valley 1.6.15, SMAPI 4.x, Content Patcher 2.4+, SpaceCore 1.25+, Automate 2.x (VO-3 only), Stardew Valley Expanded (VO-2 only). Generic Mod Config Menu is optional.

Contents: [1 · PC setup](#1--pc-setup) · [2 · Building and installing](#2--building-and-installing) · [3 · Test configurations](#3--test-configurations) · [4 · Logs and console commands](#4--logs-and-console-commands) · [5 · Test scripts](#5--test-scripts) · [6 · Spike table](#6--spike-table) · [7 · Low-confidence areas](#7--low-confidence-areas) · [8 · Bug report template](#8--bug-report-template)

---

## 1 · PC setup

Already done for alpha.1 and alpha.2 (.NET 6 SDK, Git signed in, the repo in `Documents\ValleyOpus`, SMAPI, Content Patcher and SpaceCore). Nothing new to install. If you'd like the in-game settings page, install **Generic Mod Config Menu** from Nexus into `Mods`; without it, settings stay in `Mods\ValleyOpus\config.json`.

---

## 2 · Building and installing

### 2a · With the Remote Control session (the normal way)
Claude asks the Remote Control session on your PC to build this branch (`claude/full-1-0-sink-or-swim-kztlhb`). Allow it if a card asks. That session:
1. fetches the branch and runs `dotnet build src/ValleyOpus/ValleyOpus.csproj -c Release`, which compiles the mod and copies it into `Mods\ValleyOpus`;
2. **replaces** `Mods\[CP] ValleyOpus` with the repo's `content\[CP] ValleyOpus` (delete the old folder first: this build's pack has many new files, and leftovers from alpha.2 would confuse it). `[CP] ValleyOpus - Spikes` isn't needed any more; remove it from `Mods`. Never copy `[CP] ValleyOpus - Test Fixtures`;
3. writes a zip with the packaging tool, for keeping.

**It worked if** SMAPI's window at launch lists `ValleyOpus 1.0.0-sos.2` and `[CP] ValleyOpus 1.0.0-sos.1` with no red lines under them.

### 2b · By hand (only if you ever want to)
From the `Documents\ValleyOpus` folder in PowerShell:
```
git fetch origin
git checkout claude/full-1-0-sink-or-swim-kztlhb
git pull
dotnet build src/ValleyOpus/ValleyOpus.csproj -c Release
```
Then delete `Mods\[CP] ValleyOpus` and copy `content\[CP] ValleyOpus` into `Mods` in its place.

### 2c · Going back to alpha.2
Ask in the thread: the Remote Control session rebuilds the `claude/batch-2-bench-line-l6zmax` branch. Alpha.2 farms that loaded this build may not load cleanly in alpha.2 again, which is another reason to use new farms here.

---

## 3 · Test configurations
At most four per build (doc 11 §6). Same Desktop launchers as before.

| # | Launcher | Mods | Farm |
|---|---|---|---|
| VO-1 | `VO1` (clean) | ValleyOpus + Content Patcher + SpaceCore | **new** farm `VO1-sos`, solo |
| VO-2 | your normal launch | your full mod list with **SVE** | **new** farm `VO2-sos`, solo |
| VO-3 | `VO3` | VO-1 + **Automate** | **new** farm `VO3-sos`, solo |
| 4 | co-op, split-screen, controller | deferred until you can run them (doc 17 §8b) | none this build |

Start with **VO-1**. Only move on to VO-2 and VO-3 once VO-1 gets reasonably far.

---

## 4 · Logs and console commands

**Where the log is:** `%appdata%\StardewValley\ErrorLogs\SMAPI-latest.txt` (paste `%appdata%` into the File Explorer address bar).
**Sharing it:** open https://smapi.io/log, drop the file in, click upload, and paste the link in the thread. Every ValleyOpus line is marked `[ValleyOpus]`.

**Logging for this build:** turn on `valleyopus_verbose` at the start of every session (or set `"Verbose": true` in `config.json` so it's always on). It logs every machine decision, chaining move, Automate load, power charge, unlock grant, bundle donation, contract, hint and Familiar step. The log gets long; that's expected.

**Console commands** are typed into the black SMAPI window while the game runs, then Enter. Item names are the English name with no spaces (`Ignis`, `CaputMortuum`, `RosMaialis`); a vanilla item takes its full ID, e.g. `(O)72` for a Diamond. Unlock, room and Familiar names are the ones in the tables below.

| Command | What it does |
|---|---|
| `valleyopus_dev on` | turns on the dev commands below (off by default) |
| `valleyopus_dev kit` | Satchel, Lens, six starter apparatus and their recipes, 4 Ignis, 10 Coal |
| `valleyopus_dev give <Name> [count]` | any ValleyOpus item, e.g. `valleyopus_dev give Terra 10`; the apparatus need `(BC)` IDs, e.g. `(BC)CharlesDR.ValleyOpus_Boiler` |
| `valleyopus_dev discover all` | marks every recipe discovered (no XP) |
| `valleyopus_dev xp <amount>` | adds Alchemy XP (levels at 100, 380, 770, 1300, 2150, 3300, 4800, 6900, 10000, 15000) |
| `valleyopus_dev unlock <Id\|all>` | grants an unlock as if earned (host only). IDs: Intro, FurnaceRoom, TobinArrives, FinchRoute, IvoGuild, DeltaArrives, BennetArrives, Cistern, SorensonArrives, TobinCoilLetter, Library, HesperArrives, LeyTap, AgnesArrives, AgnesCrucible, Sanctum, Undercroft, Observatory, CrystalVault, GreatWork, LarkArrives, LanternUpgrade |
| `valleyopus_dev room <Id\|all>` | restores a workshop room as if its bundles were done. Rooms: FurnaceRoom, Cistern, Library, Sanctum, Undercroft, Observatory, CrystalVault, GreatWorkChamber |
| `valleyopus_dev workshop` | grants the intro and warps you into the workshop |
| `valleyopus_dev meet <Familiar\|all>` | marks a Familiar met: Salamander, Undine, Gnome, Sylph |
| `valleyopus_dev shades <n>` | sets your Gloam Lantern's Shades (gives you a lantern if you have none) |
| `valleyopus_dev charge` | fully charges the Boiler or Lightning Coil you face |
| `valleyopus_dev place50` | places 50 Retorts with chests (performance check) |
| `valleyopus_dev prices` | prints every item's sell price |
| `valleyopus_dev worldreset` | clears world progress (unlocks, bundles, contracts, Familiars); items stay |
| `valleyopus_inspect` | what the machine you face holds: tray, recipe, pin, neighbours, power, fuel, heat, bonuses |
| `valleyopus_report` | your codex, skill, Satchel and world progress |
| `valleyopus_validate` | checks the content and prints problems (you want "No findings.") |
| `valleyopus_verbose` | toggles detailed logging |

---

## 5 · Test scripts

Two passes on VO-1: first **play it straight** for a few in-game days to see whether the opening works as a player would meet it, then **jump ahead** with dev commands to reach each later system. Tick what works; for anything odd, write a line in the bug template (§8). If something crashes or prints red `ERROR` lines, share the log and keep going.

### VO-1 · pass A: play it straight (farm `VO1-sos`)
Don't use dev commands in this pass, except `valleyopus_verbose`.
- [ ] **Load check.** At launch there are no red ValleyOpus lines; `valleyopus_validate` prints "No findings."
- [ ] **The intro waits.** Watch the Community Center opening scene. Oriel's event does **not** play straight after it. Leave Town, come back (6am–3pm): now it plays behind the Community Center. Her lines cover the bundle board, the Athanor and the Table. Afterwards you have the Satchel and the Lens, and **no** welcome letter. The Table, Alembic, Retort and Athanor are waiting in the workshop: pick each up with your pickaxe and take it home.
- [ ] **Starter quests.** Oriel's first quest ("Take it apart") is in your journal. Each one finishes as soon as you've done its step and pays the material for the next: break something down in the Athanor, discover a recipe at the Table, run the Alembic, restore the Furnace Room, then (after Tobin arrives) run a machine inside your own Lab.
- [ ] **The door.** Behind the Community Center (tile 53,9) there's a wooden door. Face it and press the action button (right-click): it takes you down into the workshop. Walking past it does nothing. The Curator is inside.
- [ ] **One space.** The workshop is a single wide room with no inner doors. Only the Furnace Room's corner has a bundle board at first, with a sparkle over it; other corners are bare until their room opens.
- [ ] **Tooltips.** Hover a Prime-bearing item (e.g. a crop) after the intro: it lists the Elements you've found in it, with a ? if there are more. Key items (Satchel, Lens) show none.
- [ ] **Bundles.** The Furnace Room's board looks like the vanilla bundle screen (rewards hidden) and shows three bundles (Kindling, Mine Salts, Spring Gleanings). **For this test round every bundle slot asks for just 1 Stone, Wood, Fiber or Sap**, so bring a few of each. Donating works; finishing one pays its reward; finishing all three restores the room, with a message and new recipes (Athanor, Hermetic Vessel, the Lab T1 blueprint at Robin's).
- [ ] **The next day.** Tobin arrives (letter the next morning, Calcinator recipe) and stands in the workshop. A new board (the Cistern's) appears. His shop opens when you talk to him once his lines run out.
- [ ] **The core loop** from alpha.2 still works: Satchel, Lens, Table discoveries, Athanor kindling and heat, chaining, products, codex. (The Calcinator isn't part of the intro: Tobin brings its recipe the day after the Furnace Room is restored.)
- [ ] **Table refuses non-Elements.** Click Wood or Stone in the Table screen: it says only Elements go on the Table, and nothing is used. Non-Elements are greyed out.
- [ ] **Menus fit.** The Satchel, Table, Lens and bundle screens hold your whole inventory grid (all three rows) inside their panels.

### VO-1 · pass B: jump ahead (same farm, or a fresh one)
Type `valleyopus_dev on`. Work down the list; each line says how to get there quickly.
- [ ] **Every room opens.** `valleyopus_dev room all`, then `valleyopus_dev workshop`. Every corner along the back wall has its board, and NPCs stand in their room's corner (Oriel by the Library, Tobin by the Cistern, Hesper by the Observatory, the Curator in the middle).
- [ ] **The Curator's name.** With every room restored (`valleyopus_dev unlock GreatWork` if needed), enter the workshop: a cutscene plays where the Curator remembers who they were. There's no letter for it.
- [ ] **Finch's wagon.** `valleyopus_dev unlock FinchRoute`, sleep, then visit Cindersap Forest: Finch stands by a purple-tinted wagon just east of where the travelling cart parks. Check this with SVE too (VO-2).
- [ ] **Faster breakdown.** The Athanor breaks an item down in 30 in-game minutes, the Calcinator in 10.
- [ ] **All unlocks.** `valleyopus_dev unlock all`. Letters arrive over the next mornings; the crafting menu shows all 14 apparatus.
- [ ] **Power.** Craft or give a Boiler. Load it (Ignis and Aqua), then put a Process Vat beside it: the Vat runs its steam recipes, and `valleyopus_inspect` shows the Boiler's time left. A Lightning Coil charges in a storm (or `valleyopus_dev charge`) and each run beside it spends one charge.
- [ ] **Rituals.** Place the Ritual Altar, load a ritual's inputs and catalysts, and use it. Then craft the Circle Sigil rug, put the altar on it: it becomes the Ritual Circle (bigger batches).
- [ ] **Shades.** `valleyopus_dev shades 10`, then run a recipe that uses Shades: it spends them from the lantern. In the mines, kills add Shades to a carried lantern.
- [ ] **Ley Taps.** Craft a Ley Tap and place it on a ley node (one in the Sanctum, the rest after the Observatory). Next morning it holds Aether.
- [ ] **Labs.** At Robin's, build a Lab (T1), then upgrade it to T2 and T3. Inside: hearths count as heat and alcoves as dark. Machines inside run faster (`valleyopus_inspect` shows the bonus).
- [ ] **Familiars.** `valleyopus_dev meet all`. In the Great Work Chamber the four spirits wait. In a T3 Lab, use the Familiar ritual at the altar: the spirit moves into the Lab and floats there. Matching machines in that Lab show its bonus. The Lab's shrine releases it.
- [ ] **Skill and professions.** `valleyopus_dev xp 15000`: level-up screens through level 10, a profession choice at 5 and at 10 (SpaceCore's screen), and the chosen profession's bonus shows in `valleyopus_inspect`.
- [ ] **Products.** `valleyopus_dev give` a few products and use them: drinks give buffs, applied ones work on what you face, rings and trinkets apply their effect while worn. Try at least Ros Maialis, a Fixative, an Hourglass, a Censer, a Vial Pendant and the Azoth Diffuser.
- [ ] **NPCs.** Talk to each new NPC. Once their lines for the day run out, talking again opens their shop (Finch, Tobin, Agnes, Bennet, Ivo and Hesper), Dr. Sorenson's contracts, or a daily hint.
- [ ] **Contracts.** Accept and deliver one of Dr. Sorenson's contracts: the reward arrives.
- [ ] **Hints and divination.** At a bundle board, divine with Caput Mortuum: you get a hint for something you haven't found. Hints land in the codex's Notes page.
- [ ] **Config menu.** If GMCM is installed, its page lists ValleyOpus's settings; a change takes effect without restarting.
- [ ] **Saves carry over.** Sleep, quit to title, reload: rooms, bundles, unlocks, Labs, Familiars, lantern Shades, Boiler/Coil power and machine trays are all still there.

### VO-2 · your full mod list with SVE (farm `VO2-sos`)
- [ ] **Load check** as in VO-1. Watch especially for clashes with SVE's Town map near the CC, with SVE NPCs and schedules, and with inventory or sorting mods.
- [ ] **Intro and door** as in pass A: the event plays and the door at 53,9 works with SVE's Town.
- [ ] **Finch in SVE's Forest**: his wagon sits on open ground near the travelling cart, not inside a tree or fence.
- [ ] **A quick pass B**: rooms, a Lab at Robin's (her menu is long with SVE), two NPC shops, a Boiler line.

### VO-3 · with Automate (farm `VO3-sos`)
- [ ] **Automate runs the new apparatus**: a Boiler with a chest of Ignis and Aqua, a Process Vat beside it with a chest of inputs. Automate keeps both going.
- [ ] **Ritual Altar**: note whether Automate feeds it. Rituals are meant to be done by hand, so it should be left alone.
- [ ] **Ley Tap**: Automate collects its Aether into a chest.
- [ ] **Performance**: `valleyopus_dev place50`, walk around for an in-game hour, and note whether it felt slower.

---

## 6 · Spike table
Desk results come from reading public sources before the build (doc 22 §4). In-game results are filled in from your runs. The alpha.1 rows stay as the record; alpha.2 adds rows for the S3 and S16 fixes and runs S9 and S15. The sink-or-swim build runs no new spikes.

| Spike | Build | Desk result | In-game result | Fallback taken |
|---|---|---|---|---|
| S11 sort keys | alpha.1 | Can't be settled from public sources (the game's sort code isn't published). The spike tries all four candidate fields at once. | **PASS** on the full mod list and again on a clean configuration 1 (2026-10-19): the internal name is the key; item ID and display name sort in reverse; category doesn't sort. The chest order after vanilla Organize also passed. The chest order also passed by eye in configuration 2 (SVE, full mod list). The French rerun also passed. Each sort mod's button wasn't checked one by one. | none; the spaced sort key goes in the internal name (doc 09 §11, B2 docs PR) |
| S16 categories, professions, dye tags | alpha.1 | Expected: Artisan ×1.4 on category −26, Gemologist ×1.3 on gems and minerals; Blacksmith on category −15 to be confirmed; dye reads `color_*` tags. | **PASS except Blacksmith** (2026-10-19, Charles's full mod list with SVE, not a clean configuration): Artisan and Gemologist apply; Blacksmith gives ×1.0 on our Resource items (expected ×1.5), so it seems to apply only to specific vanilla bars. The dye pot check passed (Charles, by eye). | metal-type items need another route to Blacksmith (design item, to-do list) |
| S1 staging → output path; `OutputMethod` | alpha.1 | Likely PASS. The `OutputMethod` signature (machine, input, probe, output data, player, out minutes) matches the published machine docs. | **PASS** (2026-10-19, Charles's full mod list with SVE), and again in a clean configuration 3 with Automate (120 minutes). | none |
| S19 `ReadyTimeModifiers` over our minutes | alpha.1 | Unknown: the docs don't say whether the modifiers apply when `OutputMethod` overrides the minutes. | **PASS** (2026-10-19, Charles's full mod list with SVE), and again in a clean configuration 3 (60 minutes with the ×0.5 modifier). Runs inside S1. | none |
| S2 Automate runs our machines | alpha.1 | **Decided (D25, Charles chose option B).** Automate's own default factory claims every object with a `Data/Machines` entry before any mod's factory is asked, so we let its generic handler run our machines. It loads through vanilla's auto-load, one item per pass, into our staging tray. Locked outputs move to a hidden store the moment they finish. The in-game run checks both. | **PASS** in a clean configuration 3 (2026-10-19, Automate 2.6.1): Automate loaded the apparatus one item per pass, 4 loads reached the tray, and `OutputMethod` made Coal in 120 minutes. **Lock PASS:** the locked Coal was held out of the machine, the next Coal started, and no Coal reached the chest. | none (D25 as designed) |
| S3 missing building; Error Items | alpha.1 | Expected: the building is dropped on a save made without the mod; BigCraftables keep their item ID and work again when the data returns. | **FAIL (building), PASS (Error Items)**, 2026-10-19, run with Charles's full mod list, not a clean configuration 1. Without the mod the save loaded but threw an error (`RequireLayer` from `updateDoors`; the Lab's indoor map can't load once its data is gone) and left a ghost Lab footprint; after reinstalling, 1 Lab was still on the farm (expected 0). All 4 Spike Apparatus (Farm, FarmHouse ×2, Lab interior) were machines again. | Not doc 09's fallback: on save, our buildings and their interiors move into our own save data and are restored after the save (to-do list). Charles chose that on reinstall the Lab stays gone and its contents go to chests beside the farmhouse (doc 17 §6 changes in the B2 docs PR). |
| S12 global inventory per player | alpha.1 | Expected PASS (1.6 global inventories are saved with the team). Chest mods: to be seen. Co-op part deferred. | **PASS** (solo) (2026-10-19, Charles's full mod list with SVE, not a clean configuration). Co-op part deferred. | none |
| S13 Town warp with and without SVE | alpha.1 | Can't be settled from public sources (SVE's Town map isn't readable here). The spike checks the tile and can list open tiles. | **PASS at 53,9, with and without SVE** after a FAIL at 52,7 with SVE (2026-10-19, full mod list): the tile behind the CC isn't walkable. A test warp at 52,20 (the CC's front door) worked with SVE, but the front door was for testing only. Fixed in the installed build: with SVE the warp sits at **53,9**, behind the CC on a tile SVE leaves empty (chosen from SVE's map files on Charles's PC); without SVE it stayed at 52,7. **With SVE: PASS at 53,9** (2026-10-19; 52,9 turned out to be a bush). Then 52,7 turned out to be blocked without SVE too, so the build now uses **53,9 for both** (open in Charles's VO-1 scan). **Without SVE: PASS at 53,9** too. | a SVE-conditional tile behind the CC (53,9 with SVE, 52,7 without); the B2 docs record the spot or a side stairwell |
| S17 Robin's menu with SVE | alpha.1 | Order follows building data order; SVE's buildings may come first. Accept and note it. | Spike Lab is **last** in Robin's menu (22 of 22) (2026-10-19, Charles's full mod list with SVE, not a clean configuration). Clean configuration 1: **PASS**, building 13, last of those available to build. | accept the order; note it in the guide |
| S5 hold-up pauses the clock | alpha.1 | Unknown from public sources. | **PASS** (2026-10-19, Charles's full mod list with SVE, not a clean configuration). | none |
| S14 SpaceCore skill and banking | alpha.1 | Likely PASS. SpaceCore's source: a skill's level is the highest step of its experience curve reached, and XP is never capped, so a four-step curve banks XP past level 4 with no code of ours. Its skill code has no Mastery hook, so our XP shouldn't count toward 1.6 Mastery (the documented default). Walk of Life: to be seen. | **PASS** (2026-10-19, Charles's full mod list with SVE, not a clean configuration). The level-up screens and the unmoved Mastery bar passed (Charles, by eye). | none |
| S3 Lab removal (fix) | alpha.2 | On save, our buildings and their interiors move into our own save data and come back right after the save, so a save never holds a building the game can't load. After a save played without the mod, reinstalling leaves the Lab gone and puts its contents in chests beside the farmhouse (option C). | | |
| S16 Blacksmith on our metal bars (fix) | alpha.2 | Decided (option A): a small patch gives ×1.5 to items tagged `valleyopus_metal_bar` when the seller has Blacksmith. No metal bars are in this slice yet, so it's first testable when they arrive. | | |
| S9 performance, 50 machines | alpha.2 | Expected fine: chaining runs once a second with a budget (200 moves). | | |
| S15 custom buffs, fertilizer | alpha.2 | Expected PASS: the Data/Buffs row names our handler; fertilizer reads our numbers through three small patches. | | |

---


---

## 7 · Low-confidence areas
Everything in this build is a first run. These are the parts most likely to break, roughly from most to least likely:
- **The first compile.** Written in the cloud against decompiled game code; expect a few compile fixes on your PC.
- **The intro event.** Spawning Oriel in an event before she's a "real" villager, and the event's positions behind the CC.
- **sos.2's new parts:** the one-screen workshop map and boards that appear on the map, the starter quests, Finch's spot in a modded Forest, and the two new cutscenes.
- **Workshop and Lab maps.** Our own map files (exits, tile properties for heat and dark, the shrine and bundle tiles). Watch for black screens, being stuck in a wall, or doors that do nothing.
- **NPCs.** Home tiles, schedules, gift tastes and portraits. An NPC standing in a wall or missing entirely is worth a screenshot.
- **Labs.** Building data, interiors and the warp in and out; the T1→T2→T3 upgrades; the strip-on-save fix from alpha.2 now covers three tiers.
- **Buff effects with code behind them.** Volatile, Second Wind, Savour, Dissolution (tool use), Good Cheer and the Azoth Diffuser all hook game code; watch for crashes when they trigger.
- **Context tags in bundles.** Spring Gleanings, Curios and Clear Spirits accept items by tag (`season_spring`, `item_type_arch`, `alcohol_item`); if an item you expect is refused, note which.
- **Familiar bonuses** match machines to the Familiar's operations approximately. Note any machine that gets (or misses) a bonus it shouldn't.
- **Professions not wired yet:** Mystic's fewer catalysts and altar triggering, Hermeticist's double bonus conditions and bigger circle batches show in the skill screen but don't act yet.
- **Multiplayer** (deferred): world progress is built host-owned, but co-op isn't tested.
- **Balance** isn't tuned at all: prices, XP, bundle counts and timings are first drafts.

---

## 8 · Bug report template
Paste this into the thread, filled in. One line per bug is fine for a sink-or-swim run.
```
Configuration: VO-1 / VO-2 / VO-3
Build: 1.0.0-sos.2
Save day: (e.g. Spring 7, Year 1)
What I did:
What happened:
What I expected:
Log: (smapi.io/log link)
Screenshot: (if it's a menu, map or sprite)
```
