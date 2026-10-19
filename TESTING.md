# Testing ValleyOpus

**This build:** `0.1.0-alpha.1` (batch B1, foundation and spikes). Nothing is playable yet. This build is a small mod that runs one test per platform question ("spike") and writes the answer to the SMAPI log. Saves made with it are throwaway.

**Checked with:** Stardew Valley 1.6.15, SMAPI 4.x, Content Patcher 2.4+, SpaceCore 1.25+, Automate 2.x (configuration 3 only), Stardew Valley Expanded (configuration 2 only). Write the exact versions you used in the spike table's notes.

Contents: [1 · First-time PC setup](#1--first-time-pc-setup) · [2 · Building and installing a test build](#2--building-and-installing-a-test-build) · [3 · Test configurations](#3--test-configurations) · [4 · Logs and console commands](#4--logs-and-console-commands) · [5 · The spike scripts](#5--the-spike-scripts) · [6 · Spike table](#6--spike-table) · [7 · Low-confidence areas](#7--low-confidence-areas) · [8 · Bug report template](#8--bug-report-template)

---

## 1 · First-time PC setup

You only do this once. A Remote Control session (Claude Code on your PC) does the building for you (D23), but it needs these tools to work, and you need to know they're there. Every command below is typed into **PowerShell**: press the Windows key, type `PowerShell`, press Enter. A blue or black window opens; type the command, press Enter, and wait for the prompt (`PS C:\Users\you>`) to come back before typing the next one.

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
2. Install, if you don't have them, into the game's `Mods` folder: **Content Patcher** and **SpaceCore** (both from Nexus). For configuration 3 also **Automate**; for configuration 2 also **Stardew Valley Expanded** and its requirements.
3. Your `Mods` folder is usually `C:\Program Files (x86)\Steam\steamapps\common\Stardew Valley\Mods`. In Steam: right-click Stardew Valley → Manage → Browse local files.

---

## 2 · Building and installing a test build

### 2a · With the Remote Control session (the normal way)
When a test build is ready, Claude asks for a Remote Control session on your PC in the `Documents\ValleyOpus` folder; allow it when the card appears. That session:
1. fetches the build's branch and runs `dotnet build src/ValleyOpus/ValleyOpus.csproj -c Release`, which compiles the mod and copies it straight into your game's `Mods\ValleyOpus` folder;
2. copies the content packs this build needs (`[CP] ValleyOpus`, and for this build `[CP] ValleyOpus - Spikes`) from the repo's `content` folder into `Mods`;
3. writes a zip of the same files with the packaging tool, for keeping or sharing.

**It worked if** your `Mods` folder now holds `ValleyOpus`, `[CP] ValleyOpus` and `[CP] ValleyOpus - Spikes`, and SMAPI's window at launch shows `ValleyOpus 0.1.0-alpha.1` in the loaded mods list with no red lines under it.

### 2b · By hand (only if you ever want to)
From the `Documents\ValleyOpus` folder in PowerShell:
```
git pull
dotnet build src/ValleyOpus/ValleyOpus.csproj -c Release
dotnet run --project src/ValleyOpus.Tools -c Release -- package --mod "<your Mods folder>\ValleyOpus" --content content --out ValleyOpus-alpha.1.zip --with-spikes
```
The build step already put `ValleyOpus` into `Mods`; copy the two `[CP] …` folders from `content` into `Mods` too. The last line is optional: it only writes the zip. (The Tools project needs the .NET 8 SDK; the build step only needs .NET 6.)

### 2c · Removing the mod (for spike S3 and the uninstall test)
Move the `ValleyOpus`, `[CP] ValleyOpus` and `[CP] ValleyOpus - Spikes` folders out of `Mods` (onto the Desktop, say). Moving them back reinstalls.

---

## 3 · Test configurations
At most four per build (doc 11 §6). This build uses 1, 2 and 3.

| # | Mods | Save | Run these spikes |
|---|---|---|---|
| 1 | ValleyOpus + Content Patcher + SpaceCore (no Automate, no SVE) | new save, solo | §5, configuration 1 |
| 2 | configuration 1 + **SVE** | new save, solo | §5, configuration 2 |
| 3 | configuration 1 + **Automate** | new save, solo | §5, configuration 3 |
| 4 | co-op, split-screen, controller | deferred until you can run them (doc 17 §8b) | none this build |

Make a **new farm for each configuration**, named after it (`VO-1`, `VO-2`, `VO-3`), so you can tell them apart. Use a throwaway save: alpha.1 saves don't carry forward.

---

## 4 · Logs and console commands

**Where the log is:** `%appdata%\StardewValley\ErrorLogs\SMAPI-latest.txt` (paste `%appdata%` into the File Explorer address bar). On macOS and Linux: `~/.config/StardewValley/ErrorLogs/SMAPI-latest.txt`.
**Sharing it:** open https://smapi.io/log, drop the file in, click upload, and paste the link here. Every ValleyOpus line is marked `[ValleyOpus]`.

**Console commands** are typed into the black SMAPI window while the game runs, then Enter:

| Command | What it does |
|---|---|
| `valleyopus_spike list` | lists every spike in this build |
| `valleyopus_spike <id> [args]` | runs one spike, e.g. `valleyopus_spike S11` (scripts below) |
| `valleyopus_validate` | reads every ValleyOpus registry from the installed packs and prints the validator's report (this build: "No findings.") |
| `valleyopus_verbose` | toggles verbose logging |

Each spike writes lines like `[S11] PASS …`, `[S11] FAIL …` or `[S11] LOOK …`. **PASS** and **FAIL** are the mod's own verdict. **LOOK** means only your eyes can judge: do what the line says and write down what you saw.

---

## 5 · The spike scripts, by configuration
Run one section at a time: set up that configuration's mods (§3), make its farm, then work down its list. Your first run (2026-10-19, with your full mod list and SVE) already answered most spikes; those are marked **done**, and the spike table (§6) has the results. If a command prints `ERROR` or `crashed`, share the log and move on to the next step.

### Configuration 1 · ValleyOpus + Content Patcher + SpaceCore (farm `VO-1`): mostly done
Clean run on 2026-10-19 (SMAPI 4.5.2, Stardew 1.6.15, Content Patcher 2.9.1, SpaceCore 1.28.4). S11 passed there; the rest of the results come from your first run with the full mod list. Still open:
- [x] **S11 · Sort keys.** PASS on the clean run: the internal name is the sort key (item ID and display name sort in reverse; category doesn't sort).
- [ ] **S11 · French rerun.** Switch the game's language to French (title screen → options), load, run `valleyopus_spike S11` again and note which scheme passes now.
- [x] **S11 · Chest order.** PASS: after vanilla Organize, the Name-scheme items came out in the wanted order (P a, P b, T1 a, T1 b, T2 a, E a).
- [ ] **S13 · Warp behind the CC, without SVE.** In Town: `valleyopus_spike S13` should say PASS for tile 52,7. Walk behind the Community Center onto that tile: you should land in a shed-like room; `valleyopus_spike S13 back` brings you back.
- [ ] **S17 · Robin's menu, without SVE.** `valleyopus_spike S17`, then open Robin's building menu and note where "Spike Lab" sits.
- [ ] **S16 · Dye, by eye.** `debug item CharlesDR.ValleyOpus_Spike_S16_Dye` gives a "Spike Dye (blue)". Take it to Emily's dye pots and note whether it counts as blue.
- [ ] **S14 · Level-up screens, by eye.** `valleyopus_spike S14`, then sleep. You should see level-up screens for levels 1 to 4 and none past 4; the skills page should show the Spike skill at level 4, and the Mastery bar shouldn't move.
- [ ] **S1 · By hand.** Place a Spike Apparatus, hold Wood and click it, then Stone (`debug item 388 5`, `debug item 390 5`). It should start making Coal (2 hours) and look like any vanilla machine.

Done on your first run (rerun only if you have time): the S1 and S19 commands (`valleyopus_spike S1` on the farm, facing an open area), S5 (`valleyopus_spike S5` outdoors in the day), S12 (`valleyopus_spike S12 setup`, sleep, quit to title, load, `valleyopus_spike S12 check`).

Known fails, fixed in alpha.2: **S3** (removing the mod with a Lab) and the **S16** Blacksmith bonus on our metal items. Skip them here.

### Configuration 2 · configuration 1 + SVE (farm `VO-2`)
Your first run had SVE installed, so S17 (Lab last, 22 of 22) and S11 are **done**. Still to do:
- [x] **S13 · Warp behind the CC, with SVE.** PASS (2026-10-19): the warp at 53,9 (52,9 turned out to be a bush) reached the workshop.
- [x] **S11 · Chest sort, with SVE.** PASS (Charles, by eye after Organize on the chest, full mod list). The main `S11` command wasn't rerun here, and the sort mods' buttons weren't checked one by one.

### Configuration 3 · configuration 1 + Automate (farm `VO-3`): done
Clean run on 2026-10-19 (SMAPI 4.5.2, Stardew 1.6.15, Content Patcher 2.9.1, SpaceCore 1.28.4, Automate 2.6.1): every step passed.
- [x] **S2 · Automate runs our machine.** On the farm, place a Spike Apparatus (`debug item (BC)CharlesDR.ValleyOpus_Spike_Apparatus`, typed with no space after `(BC)`) with a chest right next to it holding a few Wood (`debug item 388 5`) and Stone (`debug item 390 5`). Wait two in-game hours (Automate puts in one item per pass, so a two-item recipe takes two passes), then type `valleyopus_spike S2`. You want **PASS**, and the chest should have lost exactly one Wood and one Stone per Coal.
- [x] **S2 · Locked output.** Face the apparatus and type `valleyopus_spike S2 lock`, then let it make another Coal. When it's done, the machine should look empty, no Coal should appear in the chest, and clicking the machine yourself should hand you the Coal.
- [x] **S1 again, with Automate installed.** On the farm, facing an open area: `valleyopus_spike S1`. Both lines should still say PASS.

### Later builds
S15 (custom buffs and fertilizer) needs real products and runs in alpha.2. Co-op, split-screen and controller (configuration 4) are deferred until you can run them.

---

## 6 · Spike table
Desk results come from reading public sources before the build (doc 22 §4). In-game results are filled in from your runs.

| Spike | Build | Desk result | In-game result | Fallback taken |
|---|---|---|---|---|
| S11 sort keys | alpha.1 | Can't be settled from public sources (the game's sort code isn't published). The spike tries all four candidate fields at once. | **PASS** on the full mod list and again on a clean configuration 1 (2026-10-19): the internal name is the key; item ID and display name sort in reverse; category doesn't sort. The chest order after vanilla Organize also passed. The chest order also passed by eye in configuration 2 (SVE, full mod list). Still to check: the French rerun and each sort mod's button. | none; the spaced sort key goes in the internal name (doc 09 §11, B2 docs PR) |
| S16 categories, professions, dye tags | alpha.1 | Expected: Artisan ×1.4 on category −26, Gemologist ×1.3 on gems and minerals; Blacksmith on category −15 to be confirmed; dye reads `color_*` tags. | **PASS except Blacksmith** (2026-10-19, Charles's full mod list with SVE, not a clean configuration): Artisan and Gemologist apply; Blacksmith gives ×1.0 on our Resource items (expected ×1.5), so it seems to apply only to specific vanilla bars. Dye not yet checked by eye. | metal-type items need another route to Blacksmith (design item, to-do list) |
| S1 staging → output path; `OutputMethod` | alpha.1 | Likely PASS. The `OutputMethod` signature (machine, input, probe, output data, player, out minutes) matches the published machine docs. | **PASS** (2026-10-19, Charles's full mod list with SVE), and again in a clean configuration 3 with Automate (120 minutes). | none |
| S19 `ReadyTimeModifiers` over our minutes | alpha.1 | Unknown: the docs don't say whether the modifiers apply when `OutputMethod` overrides the minutes. | **PASS** (2026-10-19, Charles's full mod list with SVE), and again in a clean configuration 3 (60 minutes with the ×0.5 modifier). Runs inside S1. | none |
| S2 Automate runs our machines | alpha.1 | **Decided (D25, Charles chose option B).** Automate's own default factory claims every object with a `Data/Machines` entry before any mod's factory is asked, so we let its generic handler run our machines. It loads through vanilla's auto-load, one item per pass, into our staging tray. Locked outputs move to a hidden store the moment they finish. The in-game run checks both. | **PASS** in a clean configuration 3 (2026-10-19, Automate 2.6.1): Automate loaded the apparatus one item per pass, 4 loads reached the tray, and `OutputMethod` made Coal in 120 minutes. **Lock PASS:** the locked Coal was held out of the machine, the next Coal started, and no Coal reached the chest. | none (D25 as designed) |
| S3 missing building; Error Items | alpha.1 | Expected: the building is dropped on a save made without the mod; BigCraftables keep their item ID and work again when the data returns. | **FAIL (building), PASS (Error Items)**, 2026-10-19, run with Charles's full mod list, not a clean configuration 1. Without the mod the save loaded but threw an error (`RequireLayer` from `updateDoors`; the Lab's indoor map can't load once its data is gone) and left a ghost Lab footprint; after reinstalling, 1 Lab was still on the farm (expected 0). All 4 Spike Apparatus (Farm, FarmHouse ×2, Lab interior) were machines again. | Not doc 09's fallback: on save, our buildings and their interiors move into our own save data and are restored after the save (to-do list). Charles chose that on reinstall the Lab stays gone and its contents go to chests beside the farmhouse (doc 17 §6 changes in the B2 docs PR). |
| S12 global inventory per player | alpha.1 | Expected PASS (1.6 global inventories are saved with the team). Chest mods: to be seen. Co-op part deferred. | **PASS** (solo) (2026-10-19, Charles's full mod list with SVE, not a clean configuration). Co-op part deferred. | none |
| S13 Town warp with and without SVE | alpha.1 | Can't be settled from public sources (SVE's Town map isn't readable here). The spike checks the tile and can list open tiles. | **FAIL at 52,7 with SVE** (2026-10-19, full mod list): the tile behind the CC isn't walkable. A test warp at 52,20 (the CC's front door) worked with SVE, but the front door was for testing only. Fixed in the installed build: with SVE the warp sits at **53,9**, behind the CC on a tile SVE leaves empty (chosen from SVE's map files on Charles's PC); without SVE it stays at 52,7. **With SVE: PASS at 53,9** (2026-10-19; 52,9 turned out to be a bush). Without SVE (52,7): still to run. | a SVE-conditional tile behind the CC (53,9 with SVE, 52,7 without); the B2 docs record the spot or a side stairwell |
| S17 Robin's menu with SVE | alpha.1 | Order follows building data order; SVE's buildings may come first. Accept and note it. | Spike Lab is **last** in Robin's menu (22 of 22) (2026-10-19, Charles's full mod list with SVE, not a clean configuration). A clean configuration 1 run is still to do. | accept the order; note it in the guide |
| S5 hold-up pauses the clock | alpha.1 | Unknown from public sources. | **PASS** (2026-10-19, Charles's full mod list with SVE, not a clean configuration). | none |
| S14 SpaceCore skill and banking | alpha.1 | Likely PASS. SpaceCore's source: a skill's level is the highest step of its experience curve reached, and XP is never capped, so a four-step curve banks XP past level 4 with no code of ours. Its skill code has no Mastery hook, so our XP shouldn't count toward 1.6 Mastery (the documented default). Walk of Life: to be seen. | **PASS** (2026-10-19, Charles's full mod list with SVE, not a clean configuration). The level-up screens for levels 1 to 4 are still to be checked by eye. | none |
| S15 custom buffs, fertilizer | alpha.2 | — | | |

---

## 7 · Low-confidence areas
What to keep an eye on in this build:
- Loading the farm with the mod removed (S3): any crash or red error.
- The workshop warp behind the Community Center without SVE (52,7) (S13).
- First compile on your PC: if the build fails, the Remote Control session fixes the code and tells Claude what changed.

---

## 8 · Bug report template
Paste this into the thread, filled in:
```
Configuration: 1 / 2 / 3
Build: 0.1.0-alpha.1
Save day: (e.g. Spring 3, Year 1)
What I did:
What happened:
What I expected:
Log: (smapi.io/log link)
```
