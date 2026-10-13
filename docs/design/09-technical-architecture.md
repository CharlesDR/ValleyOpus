# 09 · Technical Architecture

Status: **DRAFT v0.3** (2026-10-13). v0.3: requirements from the Part 4 decisions. Platform facts: [research](../research/comparators-and-concepts.md).

## Targets
- Stardew Valley **1.6.15** (latest), SMAPI 4.x. Recipes and tag data are kept isolated so the **1.7** migration (data-model recipes, `Data/ContextTags`) is cheap.
- Windows, macOS and Linux. Android is not a 1.0 target.

## Packaging
| Component | Kind | Contents |
|---|---|---|
| **ValleyOpus** (core) | SMAPI C# mod | decomposition engine, adjacency, codex and discovery UI, staged rituals, SpaceCore skill, multiplayer sync, Automate factory, GMCM config, custom GSQs and `OutputMethod`s |
| **[CP] ValleyOpus** | Content Patcher pack | items, apparatus (`Data/Machines`, `Data/BigCraftables`), buildings (`Data/Buildings`), shops, mail, events, NPCs, maps, strings, placeholder sprites |
| Integration packs | CP, conditional on `HasMod` | SVE etc.: item → source overrides, gated by Nexus popularity ranking |

Unique ID: **`CharlesDR.ValleyOpus`**. All item and machine IDs are prefixed `CharlesDR.ValleyOpus_`, so they never collide with other alchemy mods.

## Dependencies
- Required: SMAPI, Content Patcher, SpaceCore.
- Optional: Automate (first-class), Generic Mod Config Menu, Walk of Life (works without it), SVE and other integration targets.
- Avoid: Producer Framework Mod and Json Assets (1.6 makes them unnecessary). Use Extra Machine Configs only if a feature truly needs it.

## Key systems
- **Decomposition:** a data table of tag → Prime weights, category defaults and overrides; √price yield scaling; computed at runtime in an `OutputMethod` so any modded item works.
- **Machines:** `Data/Machines` with `RequiredTags`, GSQ `Condition`s (weekday, weather, season, location), and flavored outputs (`DROP_IN`, `{0}` names, `CopyPrice`). C# `OutputMethod`s for decomposition, adjacency and staged rituals.
- **Adjacency:** a neighbour scan when input is loaded; a data-driven effect table.
- **Laboratory effects:** a custom GSQ (e.g. `CharlesDR.ValleyOpus_IN_LAB <tier>`) used in machine rules, plus C# modifiers.
- **Codex:** host-owned save data; farmhand → host → broadcast sync via SMAPI multiplayer messages.
- **Automate:** engineered and minor arcane apparatus use Automate's native `Data/Machines` support. Ritual altars and the Ritual Circle use our `IAutomationFactory`: `SetInput` stages materials until the recipe is complete, and `GetState` reports *processing* (not *empty*) while staged-but-untriggered, so Automate stops feeding it. Output is collected normally after the player triggers the ritual.
- **Derelict workshop:** a new indoor location (CP map) with a warp **behind the Community Center** on the Town map. Risk: Town map edits by SVE and other town mods. Use a minimal tile patch and test against SVE early. It must work on both the CC and Joja routes.
- **Reagent Satchel:** a one-slot container tool for ValleyOpus items (doc 03b); hand-loading pulls from it. Needs multiplayer-safe item handling.
- **Ley nodes:** a data table of map + tile + Prime profile; the Ley Tap is a `Data/Machines` machine with a location-based GSQ.
- **Grand rituals:** C# for the multi-tile circle detection, the conditions check and the batch animation.
- **Familiars:** a per-Lab bond (T3 Labs only) stored in the building's `modData`; uniqueness enforced farm-wide.
- **NPCs:** 10 new NPCs (plus 4 Familiars as non-villager spirits). Hesper's visiting schedule is 3–4 days a week with intra-day wandering. Every NPC needs SVE-aware schedules.

## Requirements from the Part 4 decisions (doc 00, 2026-10-13)
- **Uninstall safety (S3, confirmed):** vanilla object types only, no custom item subclasses; state lives in `modData` and mod save data; Satchel contents in a global inventory; a "prepare for removal" command.
  - **Labs without the mod (Charles, 2026-10-13):** they simply **disappear**. Labs are ordinary `Data/Buildings` entries with no save-time swap, so their footprint and interior are free design choices. Anything inside a Lab is lost; "prepare for removal" offers to empty Labs into a chest first. **Test early:** confirm 1.6 drops a building with missing data cleanly on load (no crash, no stuck tiles).
  - **Workshop without the mod:** the entrance comes from our Content Patcher patch to the Town map, so it disappears with the mod and the spot does nothing. Items left inside the workshop are lost.
- **i18n (S4):** every player-facing string goes through `i18n/` from the first build.
- **Recipes (R2):** input count is data (2, 3, later 4+), never a fixed shape.
- **Buff effects (P2):** custom effects are an extensible registry, so new effects are data plus a small handler.
- **Directional chaining (S2):** our own transfer logic between adjacent ValleyOpus machines (West in, East out, South chest second). It runs without Automate; with Automate, our integration takes priority over Automate's group logic. It sits behind a general item-route interface, so 2.0 arcane logistics can replace or extend it.
- **Controller and split-screen (U1):** all custom UI (codex, Satchel grid, Experiment Table, picker) must support gamepads and per-screen state.
- **GMCM (U3):** hints, celebrations and economy knobs.

## Engineering practices
- Tests for the decomposition math, economy caps (including the all-six-professions fixture), and adjacency resolution.
- A balance-sweep script that reads the data files and outputs GPTD/VAR and source-diversity reports (doc 06).
- Docs move into the GitHub repo once it exists. This folder is the staging copy.
