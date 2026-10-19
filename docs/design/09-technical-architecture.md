# 09 · Technical Architecture

Status: **DRAFT v0.6b** (2026-10-18). v0.6b: Satchel storage, multiplayer sync, saves and the test matrix follow doc 17. This doc stays a sketch until the architecture gate, when it is rewritten as the architecture plan; where it and docs 12–18 differ, those docs win. v0.6: cleanup sweep: machines, Automate and Lookup Anything follow doc 13 §6; Familiar state follows doc 18; NPC count follows doc 08a; Charles's item ID and sort-key rules added. v0.5: integration and compatibility targets chosen from Charles's export of the 80 most-downloaded mods. v0.4: content-pack extensibility in 1.0, three more 1.0 tie-in mods, villager integration, the hand-made-only flag and compatibility-safe world changes (Charles's guiding answers, doc 01). v0.3: requirements from the Part 4 decisions. Platform facts: [research](../research/comparators-and-concepts.md).

## Targets
- Stardew Valley **1.6.15** (latest), SMAPI 4.x. Recipes and tag data are kept isolated so the **1.7** migration (data-model recipes, `Data/ContextTags`) is cheap.
- Windows, macOS and Linux. Android is not a 1.0 target.

## Packaging
| Component | Kind | Contents |
|---|---|---|
| **ValleyOpus** (core) | SMAPI C# mod | decomposition engine, adjacency, codex and discovery UI, staged rituals, SpaceCore skill, multiplayer sync, Automate factory, GMCM config, custom GSQs and `OutputMethod`s |
| **[CP] ValleyOpus** | Content Patcher pack | items, apparatus (`Data/Machines`, `Data/BigCraftables`), buildings (`Data/Buildings`), shops, mail, events, NPCs, maps, strings, placeholder sprites |
| Integration packs | CP, conditional on `HasMod` | 1.0: **SVE, Alecto the Witch, Ridgeside Village, East Scarp** (item → source overrides, gift tastes, dialogue, small events). Others in 2.0; see "Integration and compatibility targets" |

Unique ID: **`CharlesDR.ValleyOpus`**. All item and machine IDs are prefixed `CharlesDR.ValleyOpus_`, so they never collide with other alchemy mods.

**Room to grow in IDs and sort keys** (Charles, 2026-10-18): new items must fit into any category or subcategory without breaking a sequence or reassigning an existing ID. 1.6 item IDs are strings, so every ID is a **stable name** (`CharlesDR.ValleyOpus_<Name>`) that never changes (renumbering breaks saves) and carries **no order**. Any ordering number (category values, sort keys) lives separately and is **spaced with gaps at every level** (for example groups by 1000, tiers by 100, items by 10). The inventory sort order these keys serve is in doc 03b.

## Dependencies
- Required: SMAPI, Content Patcher, SpaceCore.
- Optional: Automate (expected and first-class, but never required: every feature works by hand plus directional chaining), Generic Mod Config Menu, Walk of Life (works without it), and the 1.0 tie-in mods above.
- Avoid: Producer Framework Mod and Json Assets (1.6 makes them unnecessary). Use Extra Machine Configs only if a feature truly needs it.

## Key systems
- **Decomposition:** a data table of tag → Prime weights, category defaults and overrides; √price yield scaling; computed at runtime in an `OutputMethod` so any modded item works.
- **Machines:** one `Data/Machines` output rule per apparatus whose `OutputMethod` calls **one C# resolver** fed by a staging tray (doc 13 §6); the resolver handles decomposition, recipes, neighbour states, conditions and staged rituals. Plain `Data/Machines` rules (with our `VALLEYOPUS_DISCOVERED` GSQ) are used only for the appended Furnace and Keg rules.
- **Adjacency:** a neighbour scan when input is loaded, giving each apparatus one of its mutually exclusive neighbour states (doc 13 §1a); neighbour tests read map tiles (Lab hearths and alcoves) as well as objects (doc 18 §2a).
- **Laboratory effects:** Lab tier and Familiar are Bonus effects applied by the resolver at load (doc 18 §1a); a custom GSQ (e.g. `CharlesDR.ValleyOpus_IN_LAB <tier>`) stays available to content packs.
- **Codex:** host-owned save data; farmhand → host → broadcast sync via SMAPI multiplayer messages.
- **Automate:** one `IAutomationFactory` claims **every** ValleyOpus apparatus, so Automate's generic `Data/Machines` handler never sees them (doc 13 §6, superseding the earlier native-support plan). `SetInput` loads only a discovered, non-hand-made recipe chosen by the machine's recipe memory (doc 13 §6a); `GetOutput` refuses locked outputs; `GetState` reports *processing* for staged-but-untriggered altars, so Automate stops feeding them. The appended Furnace and Keg rules stay on Automate's native support.
- **Derelict workshop:** a new indoor location (CP map) with a warp **behind the Community Center** on the Town map. Risk: Town map edits by SVE and other town mods. Use a minimal tile patch and test against SVE early. It must work on both the CC and Joja routes.
- **Reagent Satchel:** a one-slot container tool for ValleyOpus items (doc 03b); hand-loading pulls from it. Needs multiplayer-safe item handling.
- **Ley nodes:** a data table of map + tile + Prime profile; the Ley Tap is a `Data/Machines` machine with a location-based GSQ.
- **Grand rituals:** C# for the multi-tile circle detection, the conditions check and the batch animation.
- **Familiars:** bonds and hearts are farm-wide C# state in host save data, mirrored in the Lab building's `modData` for fast lookup; one per T3 Lab, uniqueness enforced farm-wide (doc 18).
- **NPCs:** 11 new NPCs (doc 08a; plus 4 Familiars as non-villager spirits). Hesper's visiting schedule is 3–4 days a week with intra-day wandering. Every NPC needs SVE-aware schedules.
- **Vanilla and SVE villagers (Charles):** every villager gets alchemy dialogue, gift tastes and, where it fits, events. All of it is Content Patcher data (`Characters/Dialogue/*`, `Data/NPCGiftTastes`, `Data/Events/*`) conditioned on our flags, appended rather than replacing, and fully i18n. The Witch's content switches to Alecto's NPC when that mod is installed.
- **Hand-made-only recipes:** a flag in recipe data. Our Automate factory and the chaining logic refuse such inputs, so gifts, quest items and capstones are only ever made by the player (doc 04).
- **World changes from capstones:** delivered as conditional Content Patcher patches on locations we own, decorations on tiles no common mod edits, and dialogue, event or appearance changes for NPCs. No edits to shared map areas such as Town. Each one is checked against SVE, RSV and East Scarp (doc 17).
- **Hidden alchemy areas:** new small locations reached by a warp added on a minimal tile patch, chosen where the 1.0 tie-in mods don't edit.

## Requirements from the Part 4 decisions (doc 00, 2026-10-13)
- **Uninstall safety (S3, confirmed):** vanilla object types only, no custom item subclasses; state lives in `modData` and mod save data; Satchel contents in a 1.6 global inventory per player (doc 17 §4); a "prepare for removal" command.
  - **Labs without the mod (Charles, 2026-10-13):** they simply **disappear**. Labs are ordinary `Data/Buildings` entries with no save-time swap, so their footprint and interior are free design choices. Anything inside a Lab is lost; "prepare for removal" offers to empty Labs into a chest first. **Test early:** confirm 1.6 drops a building with missing data cleanly on load (no crash, no stuck tiles).
  - **Workshop without the mod:** the entrance comes from our Content Patcher patch to the Town map, so it disappears with the mod and the spot does nothing. Items left inside the workshop are lost.
- **i18n (S4):** every player-facing string goes through `i18n/` from the first build.
- **Recipes (R2):** input count is data (2, 3, later 4+), never a fixed shape.
- **Buff effects (P2):** custom effects are an extensible registry, so new effects are data plus a small handler.
- **Directional chaining (S2):** our own transfer logic between adjacent ValleyOpus machines (West in, East out, South chest second). It runs without Automate; with Automate, our integration takes priority over Automate's group logic. It sits behind a general item-route interface, so 2.0 arcane logistics can replace or extend it.
- **Controller and split-screen (U1):** all custom UI (codex, Satchel grid, Experiment Table, picker) must support gamepads and per-screen state.
- **GMCM (U3):** hints, celebrations and economy knobs.

## Integration and compatibility targets (2026-10-14)
Chosen from Charles's export of the 80 most-downloaded Stardew mods on Nexus (2026-10-13). Most of that list is frameworks, cosmetics or cheats that need nothing from us. These are the few that matter for an alchemy-and-automation mod.

**Content tie-ins (1.0):** each gets a CP integration pack.
| Mod | Why it matters |
|---|---|
| **Stardew Valley Expanded** | the biggest expansion: new NPCs, maps and items to map as Prime sources; late-game locations for specialty reagents (doc 03b); Town map edits near the workshop entrance |
| **Ridgeside Village** | 50+ NPCs and new items; a new location for a ley node or tie-in event |
| **East Scarp** | a new area and NPCs; tie-in lines and sources |
| **Alecto the Witch** | Charles's pick for the Witch's storyline (doc 08). Not in the top 80, so it's chosen on design grounds, not popularity |

**Integrations (1.0):** we call or support their APIs.
| Mod | What we do |
|---|---|
| **Automate** | first-class: our factory for every apparatus, with chaining priority, recipe memory and the discovery gate (above, doc 13 §6) |
| **Generic Mod Config Menu** | our config UI (U3) |
| **Lookup Anything** | it shows our machines' processing state with no extra work (doc 13 §6); in 1.0 the codex is the recipe browser, and display-only rules listing our recipes are `[2.0]` (Charles, 2026-10-16) |
| **Better Crafting** | let its crafting menu draw ingredients from the Reagent Satchel, and make sure hand-made-only recipes appear correctly |

**Compatibility to test (1.0):** no integration code planned, but each is in the doc 17 test matrix (§8).
| Mod | Risk |
|---|---|
| **Romanceable Rasmodia** | replaces the Wizard with a female version. The Wizard teaches the bright substances and enforces the ban (doc 08), so our Wizard content must work with this NPC: no hard-coded pronouns or appearance in our lines |
| **Canon-Friendly Dialogue Expansion** and **Miss Coriel's Unique Courtship Responses** | large dialogue mods; our villager lines must use our own keys or conditions so neither side overwrites the other |
| **Chests Anywhere**, **Convenient Inventory** | chest access and quick-stack; the Satchel and staged altars must behave with both |
| **UI Info Suite 2**, **Experience Bars** | HUD info; our SpaceCore skill and machines should display sensibly |
| **Wear More Rings** | our alchemical rings stack with extra ring slots (combat cap, doc 03a) |
| **Skull Cavern Elevator** | changes how fast players reach late-area reagents |
| **Item Extensions** | its custom resources and mixed seeds must decompose through context tags like any item |
| **Producer Framework Mod**, **Json Assets** | legacy frameworks still widely installed; their items must decompose too, and we never depend on them |

**Framework candidates (decide at the architecture gate):** **Unlockable Bundles** for the workshop's restoration bundles, and **Custom Companions** for the Familiars. Either would save C# work at the cost of an extra dependency.

## Extensibility for other modders (Charles, 1.0)
Other modders can add **Primes, substances, apparatus and recipes** through Content Patcher in 1.0. That makes these registries public data, not internal tables:
- **Primes** are a data asset; the Tier 1 grid, codex grid, tag-weight table and sweep are all derived from it (n Primes give n(n+1)/2 Tier 1 cells), never fixed at eight.
- **Substances and recipes** use the doc 12 recipe model (variable input count, apparatus class, conditions, quality rule, residue rule, hand-made-only flag) as a documented CP asset.
- **Apparatus** are `Data/Machines` entries plus our own data for adjacency, chaining and automation class.
- **Discovery state** is keyed by namespaced IDs, so removing a pack orphans its codex entries cleanly instead of breaking the save.
- We ship a short **content-pack guide** and an example pack. The format is versioned, so later changes don't break packs.

## Engineering practices
- Tests for the decomposition math, economy caps (including the all-six-professions fixture), and adjacency resolution.
- A balance-sweep script that reads the data files and outputs GPTD/VAR and source-diversity reports (doc 06).
- The design docs live in this repo under `docs/design/` and `docs/research/` (since 2026-10-13); the repo is their source of truth.
