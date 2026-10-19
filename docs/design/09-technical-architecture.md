# 09 · Technical Architecture

Status: **DRAFT v1.0** (2026-10-19) · Readiness: architecture plan, awaiting Charles's review (one decision, D22 in §1a, awaits Charles)
Depends on: 00, 01, 03, 03a, 03b, 04, 05, 07, 11, 12, 13, 14, 15, 16, 17, 18 · Feeds: execution planning, every build, 19, 20, 21

## Purpose
This is the architecture plan the critical path (doc 00) gates on. It turns the shapes fixed in docs 03–18 into one buildable design: which projects and packs exist, which code runs where and when, how data flows from Content Patcher rows into the game, how state is saved and synced, which framework and Harmony touchpoints we take on, and which risks the first build must prove. It serves the multiplayer-safe, safe-to-uninstall and extensible promises (doc 01) and Charles's rule of testing from the first commit (doc 11 §6). **It adds no gameplay rules.** Where this doc and an owning doc disagree on a rule, the owning doc wins and this doc is corrected; where they disagree on how something is built, this doc wins.

## Decisions locked
Carried from earlier versions of this doc and from the owning docs; nothing here is reopened without a change-log entry.
- **Targets:** Stardew Valley 1.6.15, SMAPI 4.x; Windows, macOS and Linux; Android is not a 1.0 target. Recipe and tag reading is isolated so the 1.7 migration is cheap (doc 12).
- **Unique ID** `CharlesDR.ValleyOpus`; every ID is prefixed `CharlesDR.ValleyOpus_` (Charles, 2026-10-13).
- **IDs are stable names that carry no order**; ordering lives in separately spaced sort keys (Charles, 2026-10-18; §11).
- **Required dependencies:** SMAPI, Content Patcher, SpaceCore. **Optional:** Automate (first-class, never required), GMCM, Better Crafting, the four 1.0 tie-ins. **Avoid:** Producer Framework Mod, Json Assets (1.6 makes them unnecessary).
- **Safe to uninstall** (S3): vanilla object types only, no custom item or object subclasses; state in `modData`, host save data and per-player global inventories; a "prepare for removal" command (doc 17 §6).
- **One C# resolver** behind every apparatus's `Data/Machines` `OutputMethod`, fed by a staging tray (Charles, 2026-10-16; doc 13 §6).
- **One Automate factory** claims every ValleyOpus apparatus (doc 13 §6.3).
- **Directional chaining is ours**, runs without Automate, and sits behind an item-route interface that 2.0 arcane logistics can replace or extend (S2).
- **i18n from the first build** (S4); **controller and split-screen** required for 1.0, with groundwork and code review every build and in-game tests deferred until Charles can run them (U1; doc 17 §8b).
- **Testing from the first commit:** pure engine logic in a plain C# test project on every push, one `[ValleyOpus]` log prefix, at most four test configurations per build with SVE among them, `TESTING.md` in every test build (Charles, 2026-10-18; doc 11 §6).
- **Satchel contents** in a 1.6 global inventory per player (Charles, 2026-10-19; doc 17 §4).
- **Vanilla first, then widely used frameworks** (Charles, 2026-10-19): where the base game or a widely used framework (such as SpaceCore) already has a system, we use it instead of building our own, so other mods that change that system also change our content (§1a).

## Player-facing spec
None of its own. Players meet this architecture only through what docs 07, 13, 16 and 17 already specify: machines that behave like vanilla machines, co-op that just works, saves that survive updates and removal, and other mods left intact.

## Rules & systems

### 1 · Shape in one picture
```
 Content Patcher packs                 C# mod (SMAPI)                               Game
 ─────────────────────                 ─────────────────────────────────────        ─────────────────
 [CP] ValleyOpus        ─EditData─►   our registries ─► Validator ─► Engine index     Data/Objects
 integration packs                     (CharlesDR.ValleyOpus/…)          │            Data/BigCraftables
 other modders' packs                        │                           ▼            Data/Machines
                                             └─► Data compiler ─────────────────────► Data/CraftingRecipes
                                                                                      Data/Buffs
 players' actions ─► Game adapter (staging layer, neighbour scan, GSQs, menus) ─► Engine (pure) ─► results
 host save data  ◄─► Farm state services (codex, unlocks, Familiars) ◄─► sync messages ◄─► farmhands
```
Three rules hold everywhere:
1. **The engine decides, the adapter acts.** Every rule of docs 03, 05, 07, 12, 13 and 17 that can be stated without game types lives in the engine, so it runs in the test project. The adapter only reads the game into plain records, calls the engine, and writes the answer back.
2. **One entry point per question.** "Can this item go in here?", "what does this tray make?", "is this discovered?", "what is this perk's total?" each have exactly one implementation, used by hand loading, the Satchel picker, chaining, Automate, the codex and tooltips alike (doc 12 §12).
3. **Vanilla shapes at rest.** Everything saved is a vanilla object, a `modData` string, a vanilla global inventory or our host save envelope. Nothing we save needs our code to deserialize except the envelope, which is plain JSON.

### 1a · Vanilla first, then widely used frameworks (Charles, 2026-10-19)
**Rule.** Before any subsystem builds its own mechanism, it uses, in this order: (1) a base-game system or data asset, (2) a widely used framework we already depend on (SpaceCore, Content Patcher), (3) a widely used optional framework, and only then (4) our own code. The goal is compatibility: a mod that changes friendship gains, gift tastes, machine speed, prices, buffs, quests or skill XP should change ours too. Our own code is allowed only where nothing standard fits, and the reason is recorded in the table below. A new subsystem adds its row here first.

| Area | What we use | Status |
|---|---|---|
| Villager friendship, gifts, heart events | vanilla `Data/Characters`, `Data/NPCGiftTastes`, events with friendship preconditions | vanilla (docs 08, 08a, 20) |
| Familiar bonds and hearts | own bond points today (doc 18 §4) | **D22, to Charles:** move them onto vanilla friendship |
| Skill, XP, professions, level-up | SpaceCore | framework |
| Machines, timers, ready bubble, collection | `Data/Machines` with `OutputMethod` | vanilla; our minutes are the base, and vanilla `ReadyTimeModifiers` (ours empty, other mods' kept) apply on top (spike S19) |
| Crafting recipes and learning | `Data/CraftingRecipes`, each farmer's `craftingRecipes` | vanilla |
| Buffs and effects | `Data/Buffs`, the `Buffs` field on objects | vanilla; custom handlers only for effects vanilla lacks |
| Prices and profession bonuses | vanilla prices and categories (Artisan for sellables) | vanilla, so price and profession mods apply |
| Fertilizer | vanilla `HoeDirt` fertilizer, with postfixes only for our IDs | vanilla |
| Rings and trinkets | vanilla ring and `Data/Trinkets` slots | vanilla |
| Shops | `Data/Shops` | vanilla |
| Starter quests and story quests | vanilla quest log (`Data/Quests`) | vanilla (doc 11) |
| **Sorenson's contracts** | vanilla **special orders** (`Data/SpecialOrders`), with money or a mail-flag unlock as the reward | vanilla (changed here; doc 15 §6) |
| Unlock flags | mail flags plus our GSQ | vanilla |
| Mail, events, dialogue, schedules, maps | Content Patcher data | framework |
| Celebrations | the vanilla hold-up and HUD messages | vanilla |
| Satchel storage | 1.6 global inventories | vanilla |
| Labs | `Data/Buildings` | vanilla |
| Config | GMCM | optional framework |
| Workshop bundle board | our own (D20) | own: vanilla bundles can't add rooms, and the board needs sealed doors, silhouettes and divination; Unlockable Bundles is the alternative (§10) |
| Codex, hints, discovery | our own | own: no base-game equivalent |
| Recipe resolver, adjacency, chaining, rituals, ley nodes, Shades | our own | own: these are the mod's new mechanics |

### 2 · Repository and packaging
```
ValleyOpus/
  src/
    ValleyOpus.Engine/     net6.0 class library; references nothing from the game, SMAPI or Harmony
    ValleyOpus/            the SMAPI mod: game adapter, UI, integrations; references Engine
    ValleyOpus.Tests/      unit and data tests over Engine and the shipped JSON
    ValleyOpus.Tools/      console tools over Engine: validator, balance sweep, item census, slice-report reader
  content/
    [CP] ValleyOpus/                 the base content pack
    [CP] ValleyOpus - SVE/           one integration pack per 1.0 tie-in (SVE, Alecto, RSV, East Scarp)
    [CP] ValleyOpus - Test Fixtures/ the test content pack (doc 11 §4); never shipped to players
  docs/  TESTING.md  .github/workflows/
```
- **Why the engine is its own project:** the game's assemblies can't be redistributed, so CI can't build the SMAPI mod. CI builds `Engine`, `Tests` and `Tools`, runs the tests, and validates every shipped JSON file on every push (doc 11 §6). The mod project builds locally with the standard SMAPI build package.
- **One download, several folders:** the release zip holds the C# mod, the base CP pack and the integration packs. Each integration pack is gated by `HasMod`, so installing them all is always safe.
- **Shipped split:**

| Component | Kind | Contents |
|---|---|---|
| **ValleyOpus** | SMAPI C# mod | engine, adapter, menus and overlay, SpaceCore skill, sync, save envelope, Automate factory, GMCM page, custom GSQs, Harmony patches (§13), console commands, UI strings in `i18n/` |
| **[CP] ValleyOpus** | Content Patcher pack | registry rows (docs 03, 05, 07, 12–15, 18), authored vanilla rows (`Data/Objects`, `Data/BigCraftables`, `Data/Buildings`, `Data/Furniture`, `Data/Trinkets`, shops, mail, events, NPCs, schedules, dialogue, gift tastes), maps, sprites, content strings in `i18n/` |
| **Integration packs** | CP, `HasMod`-gated | tag weights and overrides for the tie-in's items, gift tastes, dialogue, small events, ley nodes in the tie-in's own areas (doc 17 §7a) |

### 3 · Layers and modules
**The engine** (`ValleyOpus.Engine`, pure C#):

| Module | Owns | Owning doc |
|---|---|---|
| `Model` | immutable records for every registry row (Prime, Substance, Recipe, Condition, Apparatus, Arrangement, Unlock, Room, Bundle, Product, Effect, Profession, Perk, LabTier, Familiar, Hint, Milestone, LeyNode, IdAlias) | 03, 05, 07, 12–15, 18 |
| `Registry` | loads rows, upgrades older `FormatVersion`s in memory, resolves references, disables broken rows with a reason | 12 |
| `Validation` | the uniqueness rule with tag overlap, `NeighbourStates` checks, missing Tier 1 cells, Aether weights outside the source list, hand-made-only on automatable-only apparatus, unknown Primes; one report | 12 §3, 13 §1a |
| `Decomposition` | profile, √price units, largest-remainder split, exchange clamp, route modifiers, residue | 03, 12 §2 |
| `Resolver` | the runnable-recipe index per apparatus kind and operation set; "does this item extend the tray?"; the exact match; manual first attempts and locked outputs; the "nothing loadable" memo | 12 §3, 13 §6 |
| `Conditions` | evaluates condition records against a context snapshot the adapter supplies; Required, Bonus and Variant uses; waits idle | 12 §4 |
| `Quality` | base modes, additive modifiers with one clamp, caps and floors, the stage-3 skip | 12 §5 |
| `Perks` | the perk service: sums perk rows from the player, Lab tier and Familiar by the combine table, clamped | 05, 18 |
| `Codex` | discovery keys, attribution, `tried:` signatures, counts, hints, milestones, the secret pick and re-roll | 07, 12 §10–11 |
| `Unlocks` | unlock evaluation, bundle slot progress, room opening rules | 14 |
| `Routes` | the item-route interface and the directional chaining planner (§5d) | 04, 13 §7 |
| `Rituals` | staging completeness, trigger checks with their reason line, arrangement matching against a tile grid | 04, 13 |
| `Effects` | effect tiers, day schedules and the Shades split (`0.75^log₂(n)`) | 04, 15 |
| `Xp` | XP sources, one-time keys, automated XP rules | 05 |
| `State` | the save envelope, ordered migration steps, alias sweeps, snapshot and delta merge, use locks, the version handshake | 17 |
| `Random` | the deterministic seed: a stable hash (never .NET's per-process `string.GetHashCode`) over day, location, tile and recipe, and over game ID and re-roll count for the secret pick | 12 §8, §11 |

**The game adapter** (`ValleyOpus`, SMAPI):

| Module | Does | Owning doc |
|---|---|---|
| `Content` | provides our registry assets, the data compiler (§4c), asset invalidation | 12 |
| `Catalog` | snapshots every item's ID, price, category and base context tags for the engine; rebuilt after data reloads | 03, 12 |
| `Machines` | the staging layer and drop-in prefix, the `OutputMethod`, the per-location machine index, power ticking, locked-output handling, recipe memory and pin | 13 |
| `Neighbours` | builds the neighbour-state and condition snapshot from objects **and map tile properties** (`ValleyOpus_Heat`, `ValleyOpus_Dark`, `ValleyOpus_Shrine`) | 13, 18 |
| `Queries` | registers our GSQs (`VALLEYOPUS_DISCOVERED`, `VALLEYOPUS_UNLOCKED`, moon phase, planetary day and hour, darkness, ley node, `CharlesDR.ValleyOpus_IN_LAB`, `CharlesDR.ValleyOpus_LAB_FAMILIAR`, one per base-game gate) | 12, 14, 18 |
| `Routes` | runs the chaining planner on the host and moves the items | 13 §7 |
| `FarmState` | host-authoritative services for codex, unlocks, Familiars and per-player records; the local mirror every screen reads | 07, 14, 17, 18 |
| `Net` | the message router: request → delta → snapshot, locks, `ConfigSync`, `VersionHello` | 17 §3 |
| `Saves` | reads and writes the envelope, runs migrations, the one-time `modData` sweep, read-only mode | 17 §5 |
| `Satchel` | per-player global inventory, auto-collect of reagents, the trash and ship guard, the picker | 03b, 16 §4, 17 §4 |
| `Effects` | the effect-handler registry, buff handlers, fertilizer patches, trinket effect class, ring hooks, use actions | 15 |
| `Rituals` | altar trigger, Circle Sigil detection, the batch animation, clock freeze in single player | 04, 07, 13 |
| `World` | the workshop location and Town warp, Labs, Familiar sprites and interactions, ley nodes, hidden areas, the Gloam Lantern | 08, 14, 18 |
| `Skill` | the SpaceCore skill, professions, level unlocks | 05 |
| `UI` | every custom menu, tooltip, overlay and HUD notice of doc 16, with `PerScreen<T>` state | 16 |
| `Integrations` | Automate, GMCM, Better Crafting; each optional and isolated | §9 |
| `Diagnostics` | logging, verbose mode, console commands, the slice report | 11 §4, §6 |

Dependencies run one way: `UI` → adapter services → `Engine`. No engine type references the adapter, and no UI code writes state except through a service.

### 4 · The data pipeline
**a. Our registries are game assets.** Each registry (`CharlesDR.ValleyOpus/Primes`, `/Substances`, `/Recipes`, `/Apparatus`, … as the owning docs list) is a `Dictionary<string, Row>` asset that the C# mod provides empty at `AssetLoadPriority.Exclusive`. Every pack, our own base pack included, fills it with Content Patcher `EditData`. So the base content goes through the same door as every other modder's, and the content-pack promise is tested by our own content every day.

**b. Load, validate, index.** On the first read after launch, after a save loads and whenever a registry asset is invalidated: the adapter reads all registries and the item catalog → `Registry` upgrades and links rows → `Validation` disables any row that breaks a rule (Warn line with the pack and the reason; the later pack's row loses a collision, doc 13 §1a) → `Resolver` rebuilds its indexes → derived prices and profiles are recomputed. A broken row is **disabled, never a crash** (doc 12).

**c. The data compiler.** Vanilla rows our items need are **authored** in the CP pack (names, descriptions, textures, categories, context tags), so vanilla tools and other mods read normal data. The compiler only adds what is computed from our registries, editing at `AssetEditPriority.Early` so other packs can still override:

| Vanilla asset | What the compiler writes | Source |
|---|---|---|
| `Data/Objects` | derived `Price`; `Buffs` and edibility from `Effects` and `Uses`; `valleyopus_reagent` / `valleyopus_product` tags | docs 12, 15 |
| `Data/Machines` | one `ItemPlacedInMachine` rule per `Apparatus` row with our `OutputMethod`; `DayUpdate` for the Ley Tap; `AdditionalConsumedItems` for fuel; appended Furnace and Keg rules with `VALLEYOPUS_DISCOVERED` | doc 13 |
| `Data/CraftingRecipes` | one entry per `Craft` recipe | doc 12 §9 |
| `Data/Buffs` | one row per effect; custom ones name their handler in `CustomFields` | doc 15 |

When a registry is invalidated, the compiler invalidates exactly the vanilla assets it writes. The validator warns when an Item-class substance has no authored `Data/Objects` row.

**d. Pack rules that keep data testable.** Base-pack registry rows use only the `{{ModId}}` and `{{i18n:…}}` tokens; anything conditional (`HasMod`, config) goes in an integration pack. The test project reads our packs' `content.json` with a small reader that resolves just those two tokens, so CI validates the same rows the game loads.

**e. Strings.** Content strings (item names, registry text fields such as `NameKey`, dialogue) are translated by Content Patcher's `{{i18n:…}}` token in each pack's own `i18n/` folder, so every pack translates its own rows; the registry field then holds the resolved text. UI strings (menus, the fixed reason lines of doc 16 §2) live in the C# mod's `i18n/`. No player-facing string is hard-coded.

### 5 · Subsystems
**a. The resolver and the staging layer** (doc 13 §6). All four loaders (hand drop-in, the Satchel picker, chaining, Automate) call one method, `TryStage(machine, item, who, source)`:
```
TryStage ─► engine: does item extend the tray toward a recipe runnable here, now?
            (index lookup by item ID and tags → neighbour state → discovered? → Required conditions)
   no  ─► refuse with one reason line (doc 16 §2)
   yes ─► move the item into the tray (modData)
          tray exact match? ─► call the vanilla output path with the last item as trigger
                               └► our OutputMethod asks the engine for the match, returns the output,
                                  sets minutes from the recipe and perks, writes recipe ID, snapshot,
                                  locked flag and owner to modData
```
Vanilla then runs the timer, ready bubble, sprite frames and collection. The resolver re-validates the tray at match time, which settles the hand-load and chain race (doc 17 §2d): an item that no longer fits goes back to its loader or drops at the machine, never deleted.

**b. Neighbours and conditions.** At load time (and at trigger time for rituals) the adapter builds one **context snapshot**: the apparatus's neighbour state from adjacent power and heat sources and heat tiles, condition facts (darkness tiles, flowers within range 2, ley node, Lab tier and Familiar, weekday, weather, season, moon, time), and the loading player. The engine evaluates every condition from that snapshot only, so the same snapshot gives the same answer on every screen and in tests. Where darkness holds outside a Lab is still open (doc 10); its definition will be a condition record plus at most one new snapshot fact, not a new subsystem.

**c. Machine runtime.** Each location keeps an index of its ValleyOpus objects, built at save load and kept current through SMAPI's object-list events. Power sources tick on the host on time change, in game time (Vapor durations, Fulgur charges, the Sylph's day-start roll). A gated machine on a wrong day **waits idle**: its `TryStage` refuses everything with the waiting reason (doc 12 §4).

**d. Item routes: chaining and Automate.** The `Routes` module defines one interface:
```
IItemRoute
  Offer(machine)  → the item this machine can send, if any
  Accept(machine, item) → TryStage(...)
  Plan(location)  → ordered transfers for this pass
```
Directional chaining is its 1.0 implementation: output East first, then the South chest; input West first, then the South chest (doc 04). It runs **on the host only** (doc 17 §2c), on a once-a-second pass over a queue of dirty machines (a finished output, a tray change, a placement), with a per-pass budget so a large farm spreads its work. When Automate is installed, our factory (§9) gives our route priority before Automate's group logic, and both go through `TryStage`. 2.0 arcane logistics is a second `IItemRoute`, not a rewrite.

**e. Farm state.** Codex, unlocks, bundle progress, Familiars and per-player records live in one host-owned envelope (doc 17 §5). Each is a service with a local mirror: the host writes and broadcasts; farmhands send requests and read their mirror, so no menu waits on the network. Unlock state is mirrored into mail flags and crafting recipes for every player on connect and at day start (doc 14). Bundle deposits wait for the host's confirmation (doc 17 §3b).

**f. Perks, effects and XP.** The perk service is called by the resolver for speed, quality, fuel, batch and catalyst chances, and by ritual triggers. It reads the loader's perks for hand loads and the owner's for chain and Automate loads, from saved farmer data even when the owner is offline (doc 17 §2e). Effects are a registry of handlers keyed by ID with a small public C# API, so new effects are a data row plus a handler (P2). XP goes through SpaceCore; one-time keys sit in the envelope.

**g. Rituals and arrangements.** Altars are resolver apparatus with `Trigger: Hand`. The trigger checks Required conditions on a fresh snapshot, says why it can't run if it can't, and otherwise plays the batch animation. The Circle Sigil check reads the rug under the altar and the catalyst tiles of its `Arrangements` row; a matched arrangement changes the centre's apparatus kind for that run only.

**h. World.** The workshop is a CP location whose rooms change through map patches keyed on our unlock flags, entered by a warp on a minimal Town tile patch (doc 17 §7a). Labs are `Data/Buildings` entries with nested interiors; their tier is read from `CustomFields` and their Familiar from the building's `modData` mirror. Ley nodes are `LeyNodes` rows read by the Ley Tap's `DayUpdate` rule. The Gloam Lantern listens for monster kills near its carrier and fills its own `modData`.

**i. UI.** Menus subclass the vanilla menu base with clickable components and neighbour IDs, so the game's own snapping drives the controller (doc 16 §16). All runtime UI state is `PerScreen<T>`, never static. The chain overlay and link tooltips read one **per-location link graph**, rebuilt only on placement, removal and location change, never per frame.

### 6 · State, sync and saves
Doc 17 owns the rules; this is how they are built.
- **The state register is the checklist.** Every row of doc 17 §1 maps to exactly one owner module above; a system that adds state adds its row there first.
- **The envelope** `CharlesDR.ValleyOpus/Farm` is read on the host at save load and written on `Saving`, with the previous version kept as `Backup_v<N>`. Migrations are engine functions over plain JSON, unit-tested on fixtures from the previous build. A save from a newer build switches the mod to **read-only mode** (doc 17 §5d): every `FarmState` write is refused at one gate.
- **Object `modData`** uses the `CharlesDR.ValleyOpus/` prefix and a per-object schema entry; one accessor class reads and writes it, migrating lazily, plus the one-time sweep at the first day start after an upgrade.
- **Messages** are small records in the engine (so the merge logic is tested), sent by `Net` through SMAPI's multiplayer API and filtered to our mod ID. Bursts of `tried:` keys are batched per tick (doc 17 risks).
- **Who runs what in multiplayer:** the host runs farm state, chaining, power ticks, Automate and migrations; every client runs its own player's hand actions, menus, buffs, Satchel and mail-flag mirroring.

### 7 · Runtime model: when code runs
| SMAPI event | Host | Every client |
|---|---|---|
| `GameLaunched` | register GSQs, Harmony patches, SpaceCore skill, optional APIs (each switches off with one log line if missing or too old) | same |
| `SaveLoaded` | read envelope, migrate or enter read-only mode, load registries, build indexes and machine index | load registries, build indexes; farmhands ask for snapshots |
| `PeerConnected` | `VersionHello` check, send snapshots and `ConfigSync` | n/a |
| `DayStarted` | one-time `modData` sweep after an upgrade, rebuild resolver indexes, day-start rolls, re-grant unlock recipes | mirror mail flags and crafting recipes |
| `TimeChanged` | power ticks, waits idle re-checked | none |
| `OneSecondUpdateTicked` | the chaining pass (budgeted) | none |
| object-list and location-change events | update machine index, mark link graph dirty | same |
| `AssetsInvalidated` (our registries) | rebuild the pipeline (§4b), invalidate compiled assets | same |
| `Saving` | write envelope and backup | none |
| `RenderedWorld`, `RenderedHud` | overlay, tooltips, notices for each screen | same |

### 8 · Performance budgets
Placeholders until the slice measures them (doc 11 §4's report logs resolver time per day).
- **Resolver:** an indexed lookup per staging call; condition checks once per successful match, not per poll; "nothing loadable" remembered per machine until a connected chest changes (doc 13 §6.4). Target: 50 apparatus under Automate in one location with no visible frame-time cost (doc 11).
- **Chaining pass:** at most about 1 ms of work per pass; the rest of the queue waits for the next second.
- **Overlay and tooltips:** drawn from the cached link graph; particles capped per screen; target no visible cost with about 75 machines in a full T3 Lab (doc 13 §9, doc 16).
- **Decomposition profiles:** cached per item ID, cleared on data reload.
- **Network:** deltas only after the snapshot; per-tick batching.

### 9 · Integrations
| Mod | How | If absent or too old |
|---|---|---|
| **SpaceCore** (required) | `Skills.RegisterSkill`; per-farmer skill data and its sync | SMAPI won't load us; the manifest names the minimum version |
| **Content Patcher** (required) | our packs and every content pack | same |
| **Automate** | `IAutomateAPI.AddFactory` with one factory for every ValleyOpus apparatus; its machine wrapper's `SetInput` calls `TryStage` for the recipe memory's multiset, `GetOutput` refuses locked outputs, `GetState` reports *processing* for staged altars (doc 13 §6.3). Isolated in its own folder and only touched after the mod registry reports Automate | chaining and hand work cover everything (doc 04) |
| **GMCM** | API: per-player options, host economy knobs, the "prepare for removal" button (doc 16 §15) | config file only |
| **Better Crafting** | its public API: the Satchel as an extra ingredient source; hand-made-only recipes listed normally | vanilla crafting menu |
| **Lookup Anything** | nothing: it reads our machines as `Data/Machines` machines (doc 13 §6.6) | n/a |
| **Tie-in mods** | CP integration packs only, `HasMod`-gated | the pack does nothing |

Every optional integration is looked up once at launch, version-checked, and isolated so its absence costs one log line (doc 17 §7d).

### 10 · Framework choices made at the gate
| Choice | Default or decision | Alternative | Why the default |
|---|---|---|---|
| **Bundle menu** (doc 14, doc 16 §10) | **our own bundle board** (chosen by Charles, 2026-10-19, D20) | Unlockable Bundles | the board needs sealed doors, Element silhouettes with hints, the divination panel and host-confirmed deposits, and bundle progress belongs in our envelope with the rest of farm state. UB would save the slot UI but bring its own state, sync and layout, plus a dependency. |
| **Familiar sprites** (doc 18) | **our own light renderer** (chosen by Charles, 2026-10-19, D21): a non-colliding sprite that wanders between favoured machines in its Lab or the workshop | Custom Companions | four sprites in known rooms is little code; greeting, gifts and bonding are our own handlers in either case, so the framework would add a dependency for the drawing alone. |
| Ring effects (doc 15) | our own equip and unequip hooks (§13) | a ring framework | only a couple of rings in 1.0 |
| Extra Machine Configs | not used | — | the resolver covers every case doc 13 lists |

### 11 · IDs, sort keys and naming
- Every item, recipe, unlock, Prime and row ID is a **stable name** (`CharlesDR.ValleyOpus_<Name>`) that never changes and carries no order; renames go through `IdAliases` only as a last resort (doc 17 §5e).
- **Ordering numbers live separately and are spaced at every level** (Charles, 2026-10-18): for example groups by 1000, tiers by 100, items by 10, so a new item fits anywhere without reassigning anything.
- **Inventory sort order** (doc 03b rule 5, doc 11): Primes first, then substances by tier, then endgame Elements, alphabetical within groups. What vanilla Organize and the common sort mods actually key on (category, display name, internal name or ID) is spike S11 below. Its result picks which field carries the spaced sort key before the slice fixes any ID. It may also settle doc 15's Artisan category question.
- `modData` keys, mail flags, global-inventory keys and message types all use the `CharlesDR.ValleyOpus` prefix (doc 17).

### 12 · Testing, logging and tooling
Doc 11 §6 and doc 17 §8 own the test plan; this is where the pieces live.
- **`ValleyOpus.Tests`:** decomposition math, recipe matching and the uniqueness validator, neighbour states, quality modes and clamps, the perk combine table (including the all-six-professions fixture), discovery keys and `tried:` signatures, the secret pick, migrations and the envelope round trip, snapshot and delta merge, use locks, the version handshake, and a **data test** that loads every shipped pack's rows and fails on any validator finding. Runs on every push in GitHub Actions.
- **`ValleyOpus.Tools`:** the validator report, the balance sweep (GPTD/VAR and source diversity, doc 06), the item census (doc 03b), and a reader for the slice report CSV. Written in C# over the engine, so the sweep's numbers come from the same code as the game's.
- **The test content pack** exercises rules no shipped row uses (darkness, a three-input recipe on a two-slot Table, a collision the validator must reject, a pack-added Prime; doc 11 §4).
- **Logging:** one `[ValleyOpus]` prefix; Info, Warn and Error as doc 11 §6 sets; verbose mode traces every resolver decision, chaining transfer and Automate call.
- **Console commands:** `valleyopus_report`, `valleyopus_inspect`, `valleyopus_verbose`, `valleyopus_validate`, `valleyopus_prepare_removal`, and developer commands off by default (doc 11 §4).
- **Code review checklist** every build: controller neighbour IDs and default components in every menu, no static per-player state, host-only writes to farm state, every Harmony patch in the §13 list.

### 13 · Extensibility for other modders
- **Content packs** add Primes, substances, apparatus, recipes and every other registry row through Content Patcher in 1.0 (Charles). Registries are documented public assets with a `FormatVersion`; the Tier 1 grid and codex layout are derived, so a new Prime needs no code (doc 12).
- **C# mods** get a small public API: register an effect handler, query discovery and unlock state, and ask the resolver what an item makes on an apparatus. Content-pack professions are 2.0 (doc 05).
- **Discovery keys** are namespaced, so removing a pack hides its entries and reinstalling restores them (doc 12 §10).
- We ship a short content-pack guide and an example pack, which is the test fixture pack minus its deliberate errors.

### 14 · Risk register and spikes
The gate can't run the game, so every platform question below becomes a spike in the first build, run before the code that depends on it. Each has a fallback, so no answer forces a redesign. Items doc 10 asked to "verify at the gate" are S5–S9.

| # | Risk or question | Raised by | Fallback if the spike fails |
|---|---|---|---|
| S1 | Our staging layer calling the vanilla output path with `probe`, and `OutputMethod` returning the resolver's match | 11, 13 | set the held object and minutes ourselves; vanilla still draws and collects |
| S2 | Automate never routing our machines to its generic handler once our factory claims them; locked outputs refused | 11, 13 | mark our `Data/Machines` rules so Automate's generic handler skips them, per its documented options |
| S3 | A building with missing data dropped cleanly on load (S3); Error Items of our BigCraftables turning back into machines on reinstall | 09, 17 | "prepare for removal" also removes emptied Labs, and the release notes say to run it |
| S4 | A Lab upgrade with a different indoor map keeping every placed object | 18 | nested floorplans keep coordinates; else move objects across in code at upgrade |
| S5 | The vanilla hold-up pausing the clock in single player | 07, 16 | a single-player-only postfix on the time-pass check |
| S6 | Controller snapping in the codex's triangular grid | 16 | explicit neighbour IDs per cell instead of auto-snapping |
| S7 | The smallest split-screen viewport at default zoom | 16 | a compact layout for small viewports |
| S8 | The bundle board fitting our needs (decided by §10) | 14, 16 | n/a once §10 is answered |
| S9 | Overlay and tooltip frame time in a full T3 Lab | 13, 16, 18 | lower the particle cap; draw arrows only for the hovered chain |
| S10 | Rugs (Furniture) under a BigCraftable, in either placement order | 13 | the Sigil becomes a floor-placed object the altar sits beside |
| S11 | What vanilla Organize and the common sort mods key on | 03b, 11 | a spaced prefix in the display name's sort, or a custom category per group |
| S12 | Global inventories per player syncing to farmhands, and staying out of chest mods' lists | 17 | guard the key from those mods through their APIs or config |
| S13 | The Town warp with and without SVE | 11, 17 | a second entrance tile on the workshop's own map edge |
| S14 | SpaceCore registration, level-up, banking XP past level 4; SpaceCore XP and 1.6 Mastery; Walk of Life seeing SpaceCore professions | 05, 11 | our own cap on displayed levels; the documented default (no Mastery XP) |
| S15 | A custom buff handler on the vanilla `Data/Buffs` path; the fertilizer queries for our IDs; other fertilizer mods patching the same methods | 15 | per-tick buff checks; patch priority and a compatibility note |
| S16 | Category numbers and the professions they trigger (Artisan, Blacksmith for bars); dye context-tag names | 15 | categories chosen by the result; tags corrected in data |
| S17 | Robin's menu ordering with SVE's extra buildings | 18 | accept the order; mention it in the guide |
| S18 | Hover cost of power links and the Lab mark in large Labs | 13, 18 | computed on hovered-tile change only (already the plan) |
| S19 | Vanilla `ReadyTimeModifiers` (other mods' machine-speed edits) applying on top of our `OutputMethod` minutes | 09 §1a | apply the rule's modifiers ourselves after setting minutes |

**Harmony patches** (the compatibility surface). Each is registered in one place with an ID and a guard that logs and disables the patch on error, and each is checked against the configuration-6 and 8 mods (doc 17 §8a):
1. a prefix on object drop-in, for our machines only (the staging layer);
2. postfixes on the three fertilizer queries, for our item IDs only (doc 15);
3. a postfix on the item tooltip path for profile and hint lines (doc 16);
4. ring equip and unequip hooks for our rings (doc 15);
5. the Satchel guard: our Satchel can't be trashed or shipped while its bag holds anything, and reagents go to the bag even when the main inventory is full (doc 17 §4);
6. S5's fallback, only if the spike needs it.
A new patch is added to this list before it is written.

### 15 · Integration and compatibility targets
Chosen from Charles's export of the 80 most-downloaded Stardew mods (2026-10-14). The test configurations they join are doc 17 §8's.

**Content tie-ins (1.0), one CP integration pack each:** **Stardew Valley Expanded** (new items to map as sources, late-game locations for specialty reagents, Town edits near the workshop entrance), **Ridgeside Village** (NPCs, items, a location for a ley node or tie-in event), **East Scarp** (an area, NPCs, tie-in lines and sources), **Alecto the Witch** (Charles's pick for the Witch's storyline; the Witch's content switches to Alecto when installed, and stays the unnamed vanilla Witch otherwise).

**Integrations (1.0):** Automate, GMCM, Lookup Anything and Better Crafting (§9).

**Compatibility to test (1.0), no integration code planned:**
| Mod | Risk |
|---|---|
| **Romanceable Rasmodia** | replaces the Wizard; our Wizard lines have no hard-coded pronouns or appearance |
| **Canon-Friendly Dialogue Expansion**, **Miss Coriel's Unique Courtship Responses** | our villager lines use our own keys or conditions, so neither side overwrites the other |
| **Chests Anywhere**, **Convenient Inventory** | the Satchel's global inventory and staged altars behave with both (S12) |
| **UI Info Suite 2**, **Experience Bars** | our skill and machines display sensibly; tooltip postfixes coexist |
| **Wear More Rings** | our rings stack with extra slots (combat cap, doc 03a) |
| **Skull Cavern Elevator** | changes how fast players reach late-area reagents |
| **Item Extensions**, **Producer Framework Mod**, **Json Assets** | their items decompose through tags and category defaults; we never depend on them |

**Villagers (Charles):** every vanilla and SVE villager gets alchemy dialogue, gift tastes and, where it fits, events, all as appended CP data conditioned on our flags and fully translated (doc 20). New NPCs (11, doc 08a) get SVE-aware schedules. **World changes** from capstones go only on places we own or tiles no common mod edits (doc 17 §7a).

## Data shape
This doc adds **no registry and no saved state**; every asset and state row belongs to the doc named in §3 and doc 17 §1. It adds three build-level records:

| Record | Kind | Fields |
|---|---|---|
| Harmony patch list | code registry | patch ID, target, owning module, guard (§14) |
| Optional integration table | code registry | mod ID, minimum version, isolated entry point (§9) |
| Spike results | design table in `TESTING.md` per build | spike ID, build, result, fallback taken |

Clarification for every owning doc: registry fields named `…Key` (such as `NameKey`) carry text resolved by Content Patcher's `{{i18n:…}}` token in the pack that adds the row (§4e).

## Content inventory
None. Rows belong to the owning docs; the slice's are listed in doc 11.

## Tech hooks
This whole doc. In short: `Data/Machines` with `OutputMethod`, custom GSQs, `Data/Buildings` with `CustomFields`, global inventories, `modData`, SMAPI save data, multiplayer messages and `PerScreen<T>`, SpaceCore skills, the Automate factory API, the GMCM API, and the Harmony patches listed in §14.
**1.7:** `Data/ContextTags` and data-model crafting recipes touch only `Catalog` (tag reading) and the data compiler's crafting step; our registries, the engine and the save envelope don't change. Any 1.7 change to saved state ships as a normal migration step (doc 17).

## Balance hooks
Every number lives in data the sweep can rewrite (doc 00, "what deliberately waits"), and the sweep runs on the engine itself (§12), so its numbers always match the game's. The GMCM economy knobs edit `Decomposition/Settings` and the other economy rows, with the host's values ruling in multiplayer (doc 17 §3c).

## Open questions
Every open item across the docs is also collected in [doc 10](10-roadmap-and-open-questions.md#open-questions).
- `[blocks-arch]` **D22 · Familiar bonds on vanilla friendship** (§1a): make the four Familiars vanilla non-villager NPCs so greeting, gifts and hearts use vanilla friendship, or keep doc 18's own bond points. *Owner: Charles.*
- `[blocks-content]` **Inventory sort keys:** settled by spike S11 before the slice fixes IDs (§11). *Owner: the first build.*
- `[2.0]` Arcane logistics as a second `IItemRoute`; content-pack professions; Lookup Anything recipe rules (doc 13 §6.6).

## Change log
- **v1.0a (2026-10-19):** Charles's vanilla-first rule (§1a) with an audit of every area; Sorenson's contracts become vanilla special orders; machine timing keeps vanilla `ReadyTimeModifiers` (spike S19); D22 (Familiar bonds on vanilla friendship) raised.
- **v1.0 (2026-10-19):** rewritten as the architecture plan at the gate: the engine and adapter split with one module per owning doc, the repository layout (a game-free engine so CI can test it), the data pipeline (our registries as CP-filled assets, a validator, a compiler that adds computed fields to authored vanilla rows), the staging, neighbour, routing, farm-state and UI designs, the runtime event table, performance budgets, integrations, the framework choices (our own bundle board, chosen by Charles as D20; our own Familiar renderer as D21, both chosen by Charles), the risk register as first-build spikes with fallbacks, and the Harmony patch list. Integration targets and Charles's ID rules carried over.
- **v0.6c (2026-10-19):** pre-gate cleanup sweep: testing from the first commit (doc 11 §6) and the deferred in-game co-op, split-screen and controller tests (doc 17 §8b).
- **v0.6b (2026-10-19):** Satchel storage, multiplayer sync, saves and the test matrix follow doc 17.
- **v0.6 (2026-10-18):** machines, Automate and Lookup Anything follow doc 13 §6; Familiar state follows doc 18; NPC count follows doc 08a; Charles's item ID and sort-key rules.
- **v0.5 (2026-10-14):** integration and compatibility targets from Charles's top-80 export.
- **v0.4 (2026-10-13):** content-pack extensibility, three more 1.0 tie-ins, villager integration, the hand-made-only flag and compatibility-safe world changes (doc 01).
- **v0.3 (2026-10-13):** requirements from the Part 4 decisions.
