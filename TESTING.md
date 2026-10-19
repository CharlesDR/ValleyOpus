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
| 1 | ValleyOpus + Content Patcher + SpaceCore (no Automate, no SVE) | new save, solo | S1, S19, S3, S5, S11, S12, S14, S16, S13 |
| 2 | configuration 1 + **SVE** | new save, solo | S13, S17, S11 (SVE adds sort mods' lists of items), S16 |
| 3 | configuration 1 + **Automate** | an existing Spring save, solo | S2, then S1 again |
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

## 5 · The spike scripts
Each script lists where to stand and what to type. Run them in this order; the whole set takes about an hour across the three configurations. If a command prints `ERROR` or `crashed`, share the log and move on to the next spike.

**Wave A (blocks item IDs and the machine path)**

- **S11 · Sort keys** (configs 1 and 2). Anywhere: `valleyopus_spike S11`. Note which scheme says PASS (or "exact reverse"). Then face an empty tile and `valleyopus_spike S11 chest`, open the chest, press vanilla's Organize button and every installed sort mod's button, and note whether each kept the order in the LOOK line. Then switch the game's language to French (title screen → options), load, run `valleyopus_spike S11` again and note which scheme passes now.
- **S16 · Categories and dye** (config 1). Anywhere: `valleyopus_spike S16`. Then take a "Spike Dye (blue)" (`debug item CharlesDR.ValleyOpus_Spike_S16_Dye` gives one) to Emily's dye pots and note whether it counts as blue.
- **S1 · Staging and output, S19 · speed modifiers** (configs 1 and 3). Stand on the farm with an open area in front of you: `valleyopus_spike S1`. It places two Spike Apparatus and runs both checks. Then do the by-hand check from the LOOK line with real Wood and Stone (`debug item 388 5`, `debug item 390 5`).
- **S2 · Automate** (config 3 only). Place a Spike Apparatus (`debug item (BC)CharlesDR.ValleyOpus_Spike_Apparatus`) with a chest right next to it holding Wood and Stone. Wait one in-game hour, then `valleyopus_spike S2`. The desk check expects FAIL (see the spike table). Then `valleyopus_spike S2 patch`, type `automate reset`, wait another hour by the machine and chest, and run `valleyopus_spike S2` again. Last: face the apparatus, `valleyopus_spike S2 lock`, and watch whether Automate empties it when it's ready (it shouldn't).

**Wave B (blocks world, saves and storage)**

- **S3 · Removing the mod** (config 1). On the farm in an open area: `valleyopus_spike S3 build`. Sleep (this saves), quit to the desktop, remove the mod (§2c), start the game and load the farm. Note whether it loads with no crash or red errors. Sleep once (saves without the mod), quit, put the mod back, load, and run `valleyopus_spike S3 check`.
- **S12 · Satchel storage** (config 1). `valleyopus_spike S12 setup`, sleep, quit to title, load, `valleyopus_spike S12 check`.
- **S13 · Town warp** (configs 1 and 2). In Town: `valleyopus_spike S13`. If it FAILs, run `valleyopus_spike S13 scan` and share the line. Then walk behind the Community Center onto the warp; `valleyopus_spike S13 back` brings you back.
- **S17 · Robin's menu** (config 2, and 1 for comparison). `valleyopus_spike S17`, then open Robin's building menu and note where "Spike Lab" sits.
- **S5 · Clock during the hold-up** (config 1, solo). Outdoors during the day: `valleyopus_spike S5`. The farmer holds up a piece of coal; wait two seconds for the result line.
- **S14 · SpaceCore skill** (config 1). `valleyopus_spike S14`, then sleep and note the level-up screens; open the skills page and check the Spike skill shows level 4.

S15 (custom buffs and fertilizer) needs real products and runs in alpha.2.

---

## 6 · Spike table
Desk results come from reading public sources before the build (doc 22 §4). In-game results are filled in from your runs.

| Spike | Build | Desk result | In-game result | Fallback taken |
|---|---|---|---|---|
| S11 sort keys | alpha.1 | Can't be settled from public sources (the game's sort code isn't published). The spike tries all four candidate fields at once. | | |
| S16 categories, professions, dye tags | alpha.1 | Expected: Artisan ×1.4 on category −26, Gemologist ×1.3 on gems and minerals; Blacksmith on category −15 to be confirmed; dye reads `color_*` tags. | | |
| S1 staging → output path; `OutputMethod` | alpha.1 | Likely PASS. The `OutputMethod` signature (machine, input, probe, output data, player, out minutes) matches the published machine docs. | | |
| S19 `ReadyTimeModifiers` over our minutes | alpha.1 | Unknown: the docs don't say whether the modifiers apply when `OutputMethod` overrides the minutes. | | |
| S2 Automate claims our machines | alpha.1 | **Expected FAIL.** Automate's source asks its own default factory first, and it claims every object with a `Data/Machines` entry; a claimed machine never reaches a mod's factory ("if Automate already has an automatable for an entity, it won't call your factory"). Automate documents no option to skip a machine. Its generic handler loads through the vanilla drop-in path, so our staging prefix still sees every item, but it would collect locked outputs. Candidate fallbacks are put to Charles before alpha.2. | | |
| S3 missing building; Error Items | alpha.1 | Expected: the building is dropped on a save made without the mod; BigCraftables keep their item ID and work again when the data returns. | | |
| S12 global inventory per player | alpha.1 | Expected PASS (1.6 global inventories are saved with the team). Chest mods: to be seen. Co-op part deferred. | | |
| S13 Town warp with and without SVE | alpha.1 | Can't be settled from public sources (SVE's Town map isn't readable here). The spike checks the tile and can list open tiles. | | |
| S17 Robin's menu with SVE | alpha.1 | Order follows building data order; SVE's buildings may come first. Accept and note it. | | |
| S5 hold-up pauses the clock | alpha.1 | Unknown from public sources. | | |
| S14 SpaceCore skill and banking | alpha.1 | Likely PASS. SpaceCore's source: a skill's level is the highest step of its experience curve reached, and XP is never capped, so a four-step curve banks XP past level 4 with no code of ours. Its skill code has no Mastery hook, so our XP shouldn't count toward 1.6 Mastery (the documented default). Walk of Life: to be seen. | | |
| S15 custom buffs, fertilizer | alpha.2 | — | | |

---

## 7 · Low-confidence areas
What to keep an eye on in this build:
- Anything Automate does with a Spike Apparatus (configuration 3): does it ever take an output after `S2 lock`?
- Loading the farm with the mod removed (S3): any crash or red error.
- The warp tile behind the Community Center with SVE (S13).
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
