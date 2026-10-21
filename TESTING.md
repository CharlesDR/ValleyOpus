# Testing ValleyOpus

**This build:** `0.1.0-alpha.2` (batch B2, the bench and the line). This is the first build you can play: the Experiment Table, the Satchel, the codex, six apparatus, chaining, Automate support, the first products and the Alchemy skill up to level 4. Item IDs are now fixed, so **farms you start with alpha.2 carry forward** to later builds. alpha.1 farms don't: start new ones.

**Checked with:** Stardew Valley 1.6.15, SMAPI 4.x, Content Patcher 2.4+, SpaceCore 1.25+, Automate 2.x (VO-3 only), Stardew Valley Expanded (VO-2 only). Write the exact versions you used in the spike table's notes.

Contents: [1 · First-time PC setup](#1--first-time-pc-setup) · [2 · Building and installing a test build](#2--building-and-installing-a-test-build) · [3 · Test configurations](#3--test-configurations) · [4 · Logs and console commands](#4--logs-and-console-commands) · [5 · Test scripts by configuration](#5--test-scripts-by-configuration) · [6 · Spike table](#6--spike-table) · [7 · Low-confidence areas](#7--low-confidence-areas) · [8 · Bug report template](#8--bug-report-template)

---

## 1 · First-time PC setup

You did this for alpha.1; skip it unless something changed on your PC. A Remote Control session (Claude Code on your PC) does the building for you (D23), but it needs these tools to work, and you need to know they're there. Every command below is typed into **PowerShell**: press the Windows key, type `PowerShell`, press Enter. A blue or black window opens; type the command, press Enter, and wait for the prompt (`PS C:\Users\you>`) to come back before typing the next one.

### 1a · Check .NET works
```
dotnet --list-sdks
```
**It worked if** you see at least one line starting with `6.` or higher, such as `6.0.428 [C:\Program Files\dotnet\sdk]`.
**If you see** "dotnet is not recognized": close PowerShell, open a new one and try again (the installer only updates new windows). If it still fails, re-run the .NET 6 **SDK** installer (not the "Runtime" one) from https://dotnet.microsoft.com/download/dotnet/6.0 and choose the "x64" Windows installer under "SDK".

### 1b · Tell Git who you are
Git stamps every change with a name and email. Use the email on your GitHub account:
```
git --version
git config --global user.name "Charles"
git config --global user.email "charles.d.rose@gmail.com"
```
**It worked if** the first line prints something like `git version 2.47.0.windows.1` and the other two print nothing (no news is good news).

### 1c · Sign Git in to GitHub
The first time Git talks to GitHub it opens a sign-in window from "Git Credential Manager". Trigger it now by downloading the repository (this also creates the folder the builds use):
```
cd $HOME\Documents
git clone https://github.com/CharlesDR/ValleyOpus.git
```
If a window asks how to sign in, choose **Sign in with your browser**, approve it in the browser, and come back. **It worked if** PowerShell ends with `Resolving deltas: 100%, done.` and a `ValleyOpus` folder now exists in your Documents. You never need to type Git commands again unless you want to; the Remote Control session uses this folder.

### 1d · Check SMAPI and the required mods
1. Start the game through SMAPI once. The black SMAPI window's first lines show `SMAPI 4.x.x with Stardew Valley 1.6.15`.
2. Install, if you don't have them, into the game's `Mods` folder: **Content Patcher** and **SpaceCore** (both from Nexus). For VO-3 also **Automate**; for VO-2 also **Stardew Valley Expanded** and its requirements.
3. Your `Mods` folder is usually `C:\Program Files (x86)\Steam\steamapps\common\Stardew Valley\Mods`. In Steam: right-click Stardew Valley → Manage → Browse local files.

---

## 2 · Building and installing a test build

### 2a · With the Remote Control session (the normal way)
When a test build is ready, Claude asks for a Remote Control session on your PC in the `Documents\ValleyOpus` folder; allow it when the card appears. That session:
1. fetches the build's branch and runs `dotnet build src/ValleyOpus/ValleyOpus.csproj -c Release`, which compiles the mod and copies it straight into your game's `Mods\ValleyOpus` folder;
2. copies the content packs this build needs (`[CP] ValleyOpus` and `[CP] ValleyOpus - Spikes`, which now only holds the spike Lab building) from the repo's `content` folder into `Mods`;
3. writes a zip of the same files with the packaging tool, for keeping or sharing.

**It worked if** your `Mods` folder now holds `ValleyOpus`, `[CP] ValleyOpus` and `[CP] ValleyOpus - Spikes`, and SMAPI's window at launch shows `ValleyOpus 0.1.0-alpha.2` in the loaded mods list with no red lines under it.

### 2b · By hand (only if you ever want to)
From the `Documents\ValleyOpus` folder in PowerShell:
```
git pull
dotnet build src/ValleyOpus/ValleyOpus.csproj -c Release
dotnet run --project src/ValleyOpus.Tools -c Release -- package --mod "<your Mods folder>\ValleyOpus" --content content --out ValleyOpus-alpha.2.zip --with-spikes
```
The build step already put `ValleyOpus` into `Mods`; copy the two `[CP] …` folders from `content` into `Mods` too. The last line is optional: it only writes the zip. (The Tools project needs the .NET 8 SDK; the build step only needs .NET 6.)

### 2c · Removing the mod (for spike S3 and the uninstall test)
Move the `ValleyOpus`, `[CP] ValleyOpus` and `[CP] ValleyOpus - Spikes` folders out of `Mods` (onto the Desktop, say). Moving them back reinstalls.

---

## 3 · Test configurations
At most four per build (doc 11 §6). This build uses VO-1, VO-2 and VO-3, with the same Desktop launchers as alpha.1.

| # | Launcher | Mods | Farm |
|---|---|---|---|
| VO-1 | `VO1` (clean) | ValleyOpus + Content Patcher + SpaceCore | **new** farm named `VO1-a2`, solo |
| VO-2 | your normal launch | your full mod list with **SVE** | **new** farm named `VO2-a2`, solo |
| VO-3 | `VO3` | VO-1 + **Automate** | **new** farm named `VO3-a2`, solo |
| 4 | co-op, split-screen, controller | deferred until you can run them (doc 17 §8b) | none this build |

**Make a new farm for each.** alpha.1 farms won't work with alpha.2 because the item IDs changed, for the last time. From alpha.2 on, keep your farms: later builds load them.

---

## 4 · Logs and console commands

**Where the log is:** `%appdata%\StardewValley\ErrorLogs\SMAPI-latest.txt` (paste `%appdata%` into the File Explorer address bar).
**Sharing it:** open https://smapi.io/log, drop the file in, click upload, and paste the link here. Every ValleyOpus line is marked `[ValleyOpus]`.

**Console commands** are typed into the black SMAPI window while the game runs, then Enter. In commands, item names are the English name with no spaces (`Ignis`, `CaputMortuum`, `RosMaialis`).

| Command | What it does |
|---|---|
| `valleyopus_dev on` | turns on the test helpers below (they're off by default so normal play isn't affected) |
| `valleyopus_dev kit` | gives you the Satchel, the Lens, the six apparatus (and their crafting recipes), 4 Ignis and 10 Coal |
| `valleyopus_dev give <Name> [count]` | gives you any ValleyOpus item, e.g. `valleyopus_dev give Terra 10` |
| `valleyopus_dev discover all` | marks every recipe as discovered (no XP), to look at a full codex |
| `valleyopus_dev xp <amount>` | adds Alchemy XP (levels at 100, 380, 770 and 1300) |
| `valleyopus_dev prices` | prints every item's sell price |
| `valleyopus_inspect` | prints what the machine you face holds: tray, remembered recipe, pin, neighbours, fuel, heat |
| `valleyopus_report` | prints your codex, skill and Satchel numbers |
| `valleyopus_validate` | checks the content and prints problems (you want "No findings.") |
| `valleyopus_verbose` | toggles detailed logging; turn it on before reproducing a bug |
| `valleyopus_spike list`, `valleyopus_spike <id> [args]` | runs a spike (S3, S9 and S15 in this build) |

Spike lines read `[S3] PASS …`, `FAIL …` or `LOOK …`. **LOOK** means only your eyes can judge: do what the line says and write down what you saw.

---

## 5 · Test scripts by configuration
Work through one configuration at a time and tick what passes. For anything odd, fill in the bug template (§8). If a step crashes or prints `ERROR`, share the log and move on.

**How to play it, in short.** Hold the **Satchel** and click an apparatus to choose what to load into it; click empty ground with it to open the Satchel. Hold the **Lens** and click a chest to assay what's in it (learn each item's Prime profile). The **Experiment Table** is where you try combinations and discover recipes. The **codex** shows what you've found: open the Satchel and click its **Codex** button, or set `CodexKey` in `Mods\ValleyOpus\config.json` to a key you like (for example `"K"`).

### VO-1 · clean (farm `VO1-a2`): the full walk-through
Start the farm and sleep once so you're on Day 2. Then, in the SMAPI window, type `valleyopus_dev on` and then `valleyopus_dev kit`.
- [ ] **Load check.** `valleyopus_validate` prints "No findings."
- [ ] **Satchel collects reagents.** `valleyopus_dev give Terra 5`: the Terra goes into the Satchel, not your backpack. Chop some Wood: it goes into the backpack as usual.
- [ ] **Satchel screen.** Hold the Satchel and click empty ground: the Satchel opens and shows the Terra, and you can take it back out.
- [ ] **Lens.** Put a Wood and a crop in a chest. Hold the Lens and click the chest: each item's Prime profile appears, and the items show as assayed in the codex.
- [ ] **Decomposition.** Place the Calcinator and put Wood in it. When it's done it gives Primes (and sometimes Caput Mortuum), and you get Alchemy XP.
- [ ] **Experiment Table.** Place it, put two Primes on it and try them. A new recipe shows a discovery message, gives XP and appears in the codex. A combination you already tried says so.
- [ ] **Athanor kindling.** Place the Athanor and load 2 Ignis: it's kindled (`valleyopus_inspect` says so). Feed it Coal as fuel.
- [ ] **Heat.** Place the Alembic right next to the kindled Athanor (north, east, south or west). Recipes that need heat now run there. Move the Alembic away and they're refused with a reason.
- [ ] **Remembered recipe and pin.** After a machine makes something, it remembers the recipe. Pin it from its panel: it then refuses a different recipe.
- [ ] **Chaining.** Put a chest south of a machine and a second machine east of it. With its recipe remembered, the first machine pulls inputs from the chest to its south and pushes its output east, into the next machine or a chest.
- [ ] **Products.** `valleyopus_dev give RosMaialis`, then drink it: a buff appears. Water crops: about half the waterings use no water. Hold Caput Mortuum and click tilled soil: it's placed like fertilizer.
- [ ] **Skill.** `valleyopus_dev xp 1300`: Alchemy reaches level 4, with level-up screens, and the Satchel grows to 36 slots at level 2. XP past level 4 keeps counting.
- [ ] **Codex.** Open it: the Tier 1 grid shows "???" for recipes you haven't found, and finishing your first recipe of a tier shows a milestone message.
- [ ] **Saves carry over.** Sleep, quit to title and load the farm again: the Satchel's contents, machine trays, remembered recipes, codex and XP are all still there.
- [ ] **S15 spike.** Drink Ros Maialis, then type `valleyopus_spike S15`. Face tilled empty soil and run it again. You want PASS both times; then do the LOOK step it prints.
- [ ] **S3 spike, part 1.** On the farm, in an open area, type `valleyopus_spike S3 build`. Sleep. Next morning, `valleyopus_spike S3 check` should PASS: the Lab is still there with its Athanor and chest.
- [ ] **S3 spike, part 2 (removal).** Quit the game. Move the three ValleyOpus folders out of `Mods` (§2c). Load the farm: you want **no red errors** and no Lab. Sleep to save, then quit. Move the folders back, load the farm and type `valleyopus_spike S3 check`. You want PASS, with the Lab's contents in chests beside the farmhouse.

### VO-2 · your full mod list with SVE (farm `VO2-a2`)
Same start: sleep once, then `valleyopus_dev on` and `valleyopus_dev kit`.
- [ ] **Load check.** `valleyopus_validate` prints "No findings.", and there are no red ValleyOpus lines in the log at launch.
- [ ] **A quick pass of VO-1.** Satchel, Lens, Calcinator, Table, and the Athanor with the Alembic beside it. Watch especially for clashes with your other mods: inventory or chest mods with the Satchel, sorting mods with our items.
- [ ] **Workshop warp.** Behind the Community Center at 53,9: it still leads to the workshop.
- [ ] **Saves carry over.** Sleep, quit, reload: everything is still there.

### VO-3 · with Automate (farm `VO3-a2`)
Same start: sleep once, then `valleyopus_dev on` and `valleyopus_dev kit`.
- [ ] **Automate feeds a machine.** Place a Calcinator with a chest next to it holding Wood. Within a couple of in-game hours the Wood goes in and the Primes come out into the chest.
- [ ] **Automate with a remembered recipe.** Make Saxum once by hand at the Retort. Then put a chest of Terra next to it (`valleyopus_dev give Terra 20`): Automate keeps it making Saxum.
- [ ] **Locked output.** A run whose result is locked (from a flower bonus) doesn't go into the chest: clicking the machine yourself hands it to you.
- [ ] **S9 spike (performance).** On the farm, in a big open area, type `valleyopus_spike S9 place` (it places 50 Retorts with chests). Hold the Satchel and walk around for an in-game hour, then type `valleyopus_spike S9 measure`. Note whether the game felt slower.
- [ ] **Saves carry over.** Sleep, quit, reload.

## 6 · Spike table
Desk results come from reading public sources before the build (doc 22 §4). In-game results are filled in from your runs. The alpha.1 rows stay as the record; alpha.2 adds rows for the S3 and S16 fixes and runs S9 and S15.

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

## 7 · Low-confidence areas
This build was written in the cloud and compiles for the first time on your PC, so that first compile may need fixes (the Remote Control session makes them and reports what changed). After that, watch these:
- **S3 Lab strip on save.** It uses the game's own building serializer in a way that isn't documented. Watch for red errors around saving or loading, and for a Lab that loses its contents.
- **Satchel auto-collect.** It hooks the game's "add item to inventory" step. Watch for reagents vanishing, doubling or skipping the Satchel, and check that other items (Wood, crops) still go to your backpack.
- **Undine's Gift (Ros Maialis).** About half your waterings should use no water. Watch the watering can's level.
- **Automate (VO-3).** Locked outputs, and pinned machines with Automate feeding them.
- **Menus.** This is the first version of the Table, apparatus panel, Satchel, codex and assay screens. Anything cut off, overlapping or unreadable is worth a screenshot.
- **Deliberate differences from the design docs in this build:** a pinned machine refuses a different recipe instead of asking first; a sealed run ignores speed-up modifiers; codex lore text is a placeholder; Flamma Viva is one recipe, made on the Table or the Athanor.

---

## 8 · Bug report template
Paste this into the thread, filled in:
```
Configuration: VO-1 / VO-2 / VO-3
Build: 0.1.0-alpha.2
Save day: (e.g. Spring 3, Year 1)
What I did:
What happened:
What I expected:
Log: (smapi.io/log link)
Screenshot: (if it's a menu or something you saw)
```
