# 17 · Multiplayer, Saves & Compatibility

Status: **DRAFT v0.2** (2026-10-19) · Readiness: ready for architecture
Depends on: 03b, 05, 07, 09, 11, 12, 13, 14, 15, 18 · Feeds: 09 (the architecture plan), 11 (the slice's test plan), 16 (what the menus must respect), 20

## Purpose
ValleyOpus adds a lot of state to a farm: a shared codex, unlocks, machine trays, Satchels, Familiars, skill data. This doc says, for **every piece of that state**, where it lives, who may change it, how it reaches the other players, what happens when the mod is removed, and how it survives an update. It also sets the compatibility test matrix. It serves the multiplayer-safe and safe-to-uninstall promises (doc 01, S3) and Charles's rule that slice saves carry forward (doc 11). It owns no screens: anything players see here is specified by doc 16.

## Decisions locked
- **Multiplayer:** one shared codex per farm; all discoveries are shared (Charles, 2026-10-13). Controller and split-screen are required for 1.0 (U1).
- **Safe to uninstall** (S3, Charles, 2026-10-13): the save loads and plays without the mod and mod content is lost; Labs simply disappear; the workshop entrance does nothing; a "prepare for removal" command exists. Labs are stripped out of the farm on every save so removal can't crash a save (§6; Charles, 2026-10-19, option C).
- **Slice saves carry forward** with a schema version from day one (Charles, 2026-10-18, doc 11).
- **Item IDs** are stable strings that never change and carry no order; ordering lives in separately spaced sort keys (Charles, 2026-10-18, doc 09).
- **Whose perks apply:** hand loads use the loader's perks; chain and Automate loads use the machine's owner, the player who placed it (Charles, 2026-10-18, doc 05 §6).
- **Hourglass of Mercury** does nothing with farmhands connected, since the clock is shared (doc 15).
- **1.0 tie-ins:** SVE, Alecto the Witch, Ridgeside Village and East Scarp (U2, widened). **SVE installed is a test configuration from the very first build** (Charles, 2026-10-18, doc 11 §6).
- **Satchel storage: one bag per player**, a 1.6 global inventory keyed by the owning player (Charles, 2026-10-19; §4).
- **Testing:** at most four test configurations per build; test 1 is chaining only, without Automate (Charles, 2026-10-18).
- **Host economy settings, version mismatch, newer saves and test rotation** (§3c, §3e, §5d, §8b): proposed here, confirmed by Charles on 2026-10-19 (doc 10 D11–D14).

## Player-facing spec
Most of this doc is invisible when it works. What players notice:
- **Co-op just works.** Anyone can discover, and everyone's codex fills at once. Unlocks, bundles, Labs and Familiars belong to the farm. Each player has their own skill, perks, buffs and Satchel.
- **Celebrations never steal time from others.** Solo, a mandatory animation pauses the clock; with anyone else in the game, it plays as a short overlay instead (doc 07).
- **Updating is safe.** A save from an earlier build loads in a later one, migrating its data once, with a line in the SMAPI log.
- **Removing the mod is safe.** One console command (and a GMCM button) gathers mod items into chests first; then the save loads and plays without the mod. Reinstalling brings back everything that survived (§6).
- **Other mods are respected.** The tie-in mods and the popular utility mods are tested every release, and our map touches avoid the spots they edit.

## Rules & systems

### 1 · The state register
Every piece of ValleyOpus state, in one table. *Host save data* means SMAPI's `WriteSaveData` on the host (only the host can write it; farmhands' entries are stored there keyed by player ID). *modData* means the vanilla `modData` dictionary on a game object, which vanilla saves and syncs with its location.

| State | Owning doc | Lives in | Written by | Reaches other players by | Without the mod |
|---|---|---|---|---|---|
| Farm codex: `recipe:`, `item:`, `tried:`, `assay:`, `effect:`, `count:`, `hint:`, `milestone:` keys | 07, 12 | host save data | host only; farmhands send requests | codex messages (§3a) | ignored; returns on reinstall |
| Secret picks and re-roll count | 12 §11 | host save data (codex) | host | codex messages | ignored |
| Unlocks, bundle progress, rooms restored | 14 | host save data | host | unlock messages, then each client mirrors mail flags | mail flags stay, harmless |
| Unlock mail flags `CharlesDR.ValleyOpus_Unlock_<Id>` | 14 | each farmer's `mailReceived` | each client, for its own player, from the synced unlock state | vanilla | harmless strings |
| Crafting recipes granted by unlocks | 14 | each farmer's `craftingRecipes` | each client, for its own player, on connect and day start | vanilla | recipes for missing items are ignored (test item, §8) |
| Skill XP, levels, professions | 05 | SpaceCore's per-farmer data | the acting player's client | SpaceCore | ignored by SpaceCore (test item) |
| One-time XP keys | 05 | the farmer's own `modData` `CharlesDR.ValleyOpus/XpKeys`, so they travel with the farmhand | host, on the acting player's request | vanilla (farmer sync) | ignored |
| Familiar state (`Met`, `BondedLab`, `BondPoints`, `GreetedToday`) | 18 | host save data, mirrored in the Lab's `modData` | host | Familiar messages; the Lab mirror syncs with the building | ignored; Labs gone |
| Machine state: recipe ID, condition snapshot, locked flag, stage, power state, kindled flag, `RecipeMemory`, `Pinned`, **`Owner`**, tray guid `CharlesDR.ValleyOpus/Tray` | 12, 13 | the object's `modData` | the client acting on it (§3d) | vanilla location sync | inert BigCraftable or Error Item |
| Staging tray contents | 12, 13 | a per-machine global inventory `CharlesDR.ValleyOpus_Tray_<guid>` (full item fidelity: flavour, quality, `modData`) | the client acting on the machine, under the inventory's mutex | vanilla team sync | items stay in the save, unreachable; return on reinstall; gathered by §6 |
| Locked outputs taken from machines | 13 §6 | the global inventory `CharlesDR.ValleyOpus_LockedOutputs`, so they never reach Automate's chests | host | vanilla team sync | items stay in the save, unreachable |
| Staged altar inputs | 04, 13 | the altar's tray global inventory, as other apparatus | as machine state | vanilla team sync | unreachable unless gathered |
| Gloam Lantern fill | 04 | the lantern item's `modData` | its carrier's client | vanilla (item in an inventory or chest) | inert |
| Animal infusion | 15 §7a | the animal's `modData` | the client using the item | vanilla | inert; produce returns to vanilla |
| Buffs and custom effects | 15 | vanilla buffs on each farmer | each client | vanilla | unknown buff IDs dropped (test item) |
| Worn trinkets and rings | 15 | vanilla equipment slots | each client | vanilla | Error Items in a slot (gathered by §6) |
| Fertilizer on tiles | 15 | vanilla `HoeDirt.fertilizer` (our item ID) | the client applying it | vanilla | unknown ID must do nothing (test item; §6 can clear it) |
| Radicula, placed helpers | 15 | the object's `modData` | the acting client | vanilla | inert |
| **Satchel contents** | 03b | a 1.6 **global inventory per player** (§4) | the owning player's client, under the inventory's mutex | vanilla team sync | items stay in the save, unreachable; return on reinstall |
| Satchel capacity upgrades | 03b | the same per-player record in host save data | host | Satchel messages | ignored |
| Labs | 18 | vanilla `Data/Buildings` entries on the farm; on each save, stripped into the envelope's `StrippedBuildings` and restored after saving and on load (§6) | anyone, as with Sheds | vanilla | absent from the saved farm, so no crash or Error building; contents return to chests on reinstall (§6) |
| The workshop | 08, 14 | a Content Patcher location | nobody places items there (§2f) | vanilla | location gone; its entrance does nothing |
| Schema version, migration markers, backups | 17 §5 | host save data envelope | host | not synced | ignored |
| Slice report counters | 11 | host save data | host | not synced | ignored |
| Personal preferences (toasts, HUD notices, celebration skip) | 07 | GMCM config file per install | the local player | never synced | the file stays on disk |
| Economy settings (prices, yields, caps) | 06 | GMCM config, **host's values rule** (§3c) | host | config message on connect | n/a |

**Rule for new state:** a system that adds state adds a row here first. A row must name all six columns, or the system isn't ready for architecture.

### 2 · Multiplayer rules
**a. The host decides farm-wide facts.** The codex, secret seed, unlocks, bundles, Familiars and one-time XP keys are written only by the host. A farmhand's client sends a request ("I discovered `recipe:X`", "I filled bundle slot Y"); the host checks it, records it with the day and player, and broadcasts the change. Two requests for the same key in one tick: both players get their toast, and the first one recorded wins attribution (doc 07).

**b. Clients act on what is in front of them.** Hand loading, collecting, Table attempts, using items and equipping run on the acting player's own client, as vanilla does, and read the synced codex. Results agree on every screen because residue and other rolls use doc 12's deterministic seed (day, location, tile, recipe).

**c. Automation runs on the host only.** Automate already works only for the main player; our directional chaining and power ticks run on the host too, so nothing moves twice. Farmhands see the results through vanilla's location sync.

**d. Concurrent hands.** Two players at one machine is rare but possible:
- The **Experiment Table** and the **Satchel picker on a machine** take a short **use lock**: the client asks the host, the host grants it to one player, and it's released when the menu closes or the player disconnects. The other player sees the machine as busy (doc 16 shows how).
- A **hand load racing a host chain transfer** into the same tray is the same race vanilla has with Automate and kegs. The resolver re-validates the tray when it completes a match. If the tray no longer matches any recipe, the extra item goes back to its loader's Satchel or inventory, or drops at the machine; **it is never deleted**. Verbose logging records every such case (test item, §8).

**e. Ownership.** Each apparatus records `Owner` (the placing player's ID) in its `modData` when placed; vanilla's own owner field isn't relied on. The owner's perks apply to chain and Automate loads (doc 05 §6) **whether or not the owner is online**, read from their saved farmer data, so a line's output doesn't change when someone logs off. **XP** from automated output goes to the owner only while they're in the game and is never banked (doc 05). A machine picked up and placed again takes the new placer as owner.

**f. Places and buildings.** Labs are built, upgraded and used by anyone, like Sheds. The Lab bonus a machine gets is the Lab it stands in, whoever loaded it (doc 18). The workshop is a story location: players can't place objects or chests in it, so it never holds items that could be lost on uninstall.

**g. Late joiners and reconnects.** On connect the host sends the full codex, unlock and Familiar state as one **snapshot**, followed by live deltas. The joining client then grants its player's unlocked crafting recipes and mail flags. A farmhand who was offline when something was unlocked gets it on next connect.

**h. Time.** The clock is shared in co-op and in split-screen. Solo, mandatory celebrations freeze it (doc 07); with anyone else in the game, they play as non-blocking overlays of about one second, and **the mod never pauses the game for others**. The Hourglass of Mercury is inert whenever another player is in the game, split-screen included. Power and other durations use in-game time, never per-screen timers.

**i. Split-screen.** Every piece of per-player runtime state (open menus, the chain overlay, hover tooltips, toast queues, input bindings, the Satchel picker) is held per screen (`PerScreen<T>`), never in a static field. Events, toasts and celebrations go to the screen of the player they're about. Both local players share one GMCM config file, so personal preferences are shared on that machine (acceptable; noted for doc 16).

### 3 · Sync protocol
All messages go through SMAPI's multiplayer API, filtered to our mod ID, and carry the **schema version** (§5) so mismatched clients can be detected.

**a. Codex.** `CodexRequest` (farmhand → host: a key, the day, the player) → `CodexDelta` (host → all: the recorded key with attribution). `CodexSnapshot` on connect. The codex UI and resolver read only the local mirror, so a slow host never blocks a menu.

**b. Unlocks, bundles, Familiars, Satchel capacity.** The same request → delta → snapshot pattern, one message type per subject. Bundle slots accept a deposit only once the host confirms it, so two players can't fill the same slot twice.

**c. Economy settings.** Prices, yields and the caps behind doc 06's balance targets must be identical for everyone, so **the host's economy values rule** while farmhands are connected (sent in a `ConfigSync` message on connect and when the host changes them). A farmhand's own economy values are kept but unused until they host. Personal preferences stay local. *(Charles, 2026-10-19, D11.)*

**d. Use locks.** `LockRequest`, `LockGrant`, `LockRelease`, keyed by location and tile; a lock dies with its holder's connection.

**e. Version handshake.** On connect, the farmhand's client sends its mod version and schema version. *(Charles, 2026-10-19, D12.)*
- **Same schema version:** everything works; a different patch version is only logged.
- **Farmhand without ValleyOpus:** the host's chat shows a one-line notice that this player sees mod items as Error Items and can't use them; nothing else changes. Both players' saves stay safe, because a farmhand's client never writes host save data.
- **Different schema version:** both players get the notice, and the host ignores that farmhand's requests (no discoveries, bundles or unlocks recorded from them) until versions match. Their hand actions still work on objects, as vanilla allows.

### 4 · Satchel storage (decided by Charles, 2026-10-19)
Each player's Satchel contents live in a **1.6 global inventory keyed by the owning player**: `CharlesDR.ValleyOpus_Satchel_<UniqueMultiplayerID>`. Charles chose this over the alternatives below.
- **Why a global inventory, not the item's `modData`:** vanilla saves the real items (stack, quality, their own `modData`) with no custom serializer, syncs them team-wide, and offers a mutex for safe access. `modData` holds only strings, so every item would need our own serializer, and a bug in it loses the whole bag.
- **Why keyed by player, not by satchel item:** the contents can't be orphaned. If a Satchel is trashed, lost or replaced through doc 11's replacement recipe, the next Satchel the same player holds opens the same contents. A second Satchel in one inventory opens the same bag, so duplicates hold nothing extra. Satchels can't be trashed or shipped while their bag holds anything.
- **Uninstall:** the global inventory stays in the save as vanilla data. Without the mod its items are unreachable but intact, and **reinstalling restores them**. "Prepare for removal" moves every bag, offline farmhands' included, into chests first (§6).
- **Multiplayer:** a player's bag is opened only by its owner's client, under its mutex. The host can still reach every bag for "prepare for removal". Capacity upgrades are a per-player record in host save data (§1).
- **To verify in the slice:** that 1.6 global inventories sync to farmhands as expected and accept a per-player key, and that Chests Anywhere and Convenient Inventory don't list or quick-stack into them.

| Option | Pros | Cons |
|---|---|---|
| **Global inventory per player** (chosen) | vanilla serialization and sync; never orphaned; survives uninstall intact | one bag per player, so a second Satchel adds nothing; needs the trash and ship guard |
| Global inventory per satchel item | each Satchel is its own bag | trashing one orphans its contents; needs a recovery path |
| The item's `modData` (doc 03b's earlier text) | contents travel with the item, including into chests | our own serializer; contents vanish on uninstall; a large string synced on every inventory change |

### 5 · Saves, schema and migration
**a. One versioned envelope.** All host save data is one record, `CharlesDR.ValleyOpus/Farm`, with `SchemaVersion` (an integer, starting at 1 in the slice), `ModVersion`, and one section per subject in §1. Every `modData` record we own uses the `CharlesDR.ValleyOpus/` key prefix and carries its own `CharlesDR.ValleyOpus/Schema` entry on each object.

**b. Migrations are ordered steps.** Each schema change ships one step from version N to N+1, written as a pure function over plain data with no game types, and unit-tested on fixtures saved from the previous build (doc 11 §6). On load, the host applies every step from the save's version to the current one, in order.
- **Before migrating**, the host keeps the old envelope under `CharlesDR.ValleyOpus/Backup_v<N>` until the next successful save, alongside SMAPI's own save backups.
- **Object `modData`** is migrated lazily the first time our code reads an object, and by one full sweep of every location at the first day start after an upgrade. A marker in the envelope records the sweep, so it runs once. Farmhands' inventories are swept when they connect.
- **Each migration logs** an Info line: from and to versions, records changed (doc 11's logging rule).

**c. What forces a schema bump:** renaming or removing a field, changing a key's meaning, or changing how a record is keyed. Adding an optional field with a default doesn't.

**d. A save from a newer build** than the installed mod knows (a downgrade): the mod loads in **read-only mode**: it reads what it understands, never writes host save data or migrates objects, and shows one HUD line saying this save was made with a newer ValleyOpus. *(Charles, 2026-10-19, D13.)* The player is never left with a half-migrated save.

**e. IDs never change.** Items, recipes, unlocks, Primes and every other row keep their string IDs for good (doc 09). If a rename is ever unavoidable, an `IdAliases` asset maps the old ID to the new one; the one-time sweep in §5b rewrites items in inventories, chests and machines, and the codex keys. Aliases are a last resort, not a tool for tidying names.

**f. Content packs.** Pack rows carry the pack's `FormatVersion` (doc 09); our loader upgrades older formats in memory and never writes pack files. Keys from a removed pack are kept but hidden (doc 12), so reinstalling it restores them.

**g. The slice.** Schema version 1 is the slice's envelope. Build 2 must load a played slice save from one of the build's solo configurations (doc 11 §6); the migration fixtures come from that save.

### 6 · Uninstall and "prepare for removal"
**The command** `valleyopus_prepare_removal` (and a GMCM button) runs on the host only, after a confirmation, and logs every step. It never deletes anything; it moves items into **labelled chests** placed beside the farmhouse (or the player's position if those tiles are taken).
1. **Satchels:** every player's bag, offline farmhands' included, into one chest per player.
2. **Staging trays and staged altars:** their items into a chest beside each machine.
3. **Labs:** offers to empty each Lab into chests outside it (doc 18). The Lab itself is already absent from the saved farm (below).
4. **Worn trinkets and rings:** each online player's, into their own inventory (offline farmhands' are listed in the log, since their slots can't be changed while they're away).
5. **Our fertilizer:** offers to clear it from tiles, only if the test in §8 shows an unknown fertilizer ID misbehaves.
6. **A report** in the log and on screen: what was moved where, and what stays (machines become Error Items, Furniture becomes Error Items, codex and unlock data are ignored).

**Labs are stripped on every save** (Charles, 2026-10-19, option C). Before each save the host takes every ValleyOpus building (Labs) off the farm and serializes it, contents included, into the envelope's `StrippedBuildings` with `StrippedAtDay`; it restores them right after saving and on load. The saved farm never holds a building the game can't load, so removing the mod can't crash a save or leave an Error building. This replaces spike S3's original fallback ("prepare for removal also removes emptied Labs").

**Without the command**, removal is still safe, only lossier: worn items become Error Items; tray, Satchel and locked-output contents stay in the save, unreachable.

**Reinstalled after playing without the mod:** if the save was played (saved) without ValleyOpus, `StrippedAtDay` is older than the save, so the Lab stays gone, but everything that was inside it (machines, chests and their contents, items) is put into labelled chests beside the farmhouse.

**Reinstalling** restores: the codex, unlocks and Familiars (host save data), XP keys (farmer `modData`), Satchel bags, tray contents, machine `modData`, and any Error Items vanilla kept. Labs come back as they were if the save wasn't played without the mod; otherwise their contents come back in chests (above). *(To verify: whether 1.6 keeps Error Items of a missing BigCraftable as the original ID, so they turn back into machines.)*

**Every release** repeats the remove-and-reload check (doc 00, S3 cons) on a save with a Lab, a staged altar, a full Satchel and fertilizer down.

### 7 · Compatibility rules
**a. Map touchpoints.** Every place where ValleyOpus edits or warps from a shared map is a row in the **touchpoint register** below, checked against SVE, Ridgeside Village and East Scarp map edits (and Alecto the Witch's, if it edits maps) before it's built. A touchpoint is a minimal tile patch or a warp, never a redraw of a shared area (doc 02, doc 09).

| Touchpoint | Map | Owning doc | Check |
|---|---|---|---|
| Workshop entrance behind the Community Center: warp at Town tile **53,9** (spike S13 passed, vanilla and SVE). Rule (Charles): behind the CC or a side stairwell, never the CC's front door | Town | 08, 14 | SVE's Town edits (slice test 2, passed in B1); RSV and East Scarp Town edits, if any |
| Undine's meeting spot at the forest pond | Forest | 18 | SVE's Forest edits |
| Other Familiars' meeting places | to come | 18, 20 | when chosen |
| Ley-node sites (6–10) | various | 04, 14 | each site when placed; tie-in nodes go in their own mod's areas |
| Hidden alchemy areas and their entrances | various | 04 | each entrance when placed |
| Hesper's outdoor spot and visiting schedule | to come | 08 | when chosen; schedules SVE-aware (doc 09) |
| Capstone world changes | owned places only | 19 | each change |
| Laboratory in Robin's build menu | menu | 18 | ordering with SVE's extra buildings |

**b. Content keys.** Our dialogue, events, mail and gift tastes use our own keys or are appended under our conditions, so the large dialogue mods (Canon-Friendly Dialogue Expansion, Miss Coriel's Unique Courtship Responses) and our lines never overwrite each other. Wizard lines have no hard-coded pronouns or appearance (Romanceable Rasmodia, doc 09).

**c. Items from other mods** decompose through tags and category defaults, never through hard-coded IDs, so SVE, Item Extensions, Producer Framework Mod and Json Assets items need no rows (doc 12). An item with no usable data yields nothing and logs a Warn once.

**d. Dependencies are declared with minimum versions** in the manifest (SMAPI, Content Patcher, SpaceCore required; Automate, GMCM and the tie-ins optional). An optional API is looked up once at launch; if it's missing or too old, that integration switches off with one log line and the rest of the mod works.

### 8 · The test matrix
**a. The configuration pool.** Each build's test plan picks **at most four** configurations from this pool (Charles's rule). Configurations 1–4 are the slice's (doc 11 §6).

| # | Mods | Play | What it covers |
|---|---|---|---|
| 1 | ValleyOpus only | solo, new save, CC | the core loop with **chaining only**; remove and reload |
| 2 | + SVE | solo, new save, CC | Town warp, SVE items decomposing, event collisions, SVE-aware schedules |
| 3 | + Automate | solo, late install, Joja | our factory, locked outputs, polling cost, late-install soft start |
| 4 | + SVE + Automate | online co-op and split-screen with a controller (**in-game runs deferred**, §8b) | codex sync, late joiners, use locks, clock freeze with farmhands, per-screen menus, controller play |
| 5 | + SVE + Ridgeside Village + East Scarp + Alecto the Witch | solo | the four tie-ins together: map touchpoints, the Witch switching to Alecto, tie-in nodes and lines |
| 6 | + the utility stack: Chests Anywhere, Convenient Inventory, Better Crafting, Lookup Anything, UI Info Suite 2, Experience Bars, Wear More Rings, Walk of Life | solo | Satchel and global-inventory handling, quick-stack, crafting from the Satchel, HUD display, ring stacking, profession resets |
| 7 | + the dialogue and NPC stack: Canon-Friendly Dialogue Expansion, Miss Coriel's Unique Courtship Responses, Romanceable Rasmodia | solo | dialogue keys, Wizard content |
| 8 | + legacy and resource mods: Producer Framework Mod, Json Assets, Item Extensions, Skull Cavern Elevator, a popular fertilizer mod, a machine-harvesting mod | solo | decomposition of their items, fertilizer patches, third-party extractors and locked outputs |

**b. Rotation.** *(Charles, 2026-10-19, D14.)*
- **Every build** includes an SVE configuration (2, 4 or 5) and repeats the remove-and-reload check in one of its configurations.
- **Every build after the slice** loads a played save from the previous build (the remove-and-reload save or any solo configuration's), so save carry-forward is tested each time.
- **Online co-op, split-screen and controller** (Charles, 2026-10-19: he can't test them for a while): every build lays the groundwork (host authority and the sync protocol §2–3, gamepad navigation in every custom menu, `PerScreen<T>` state, per-screen toasts and overlays §2i), and code review checks it on every build, with unit tests where the logic allows (snapshot/delta merge, lock handling, the version handshake). **In-game runs of configuration 4 are deferred** for the first few builds and start once Charles can run them. All three stay 1.0 requirements (U1, doc 01).
- A configuration whose area the build changed is picked first; one untouched for three builds is picked next.
- **A release candidate** (1.0 beta and each update after it) covers all eight across its last two builds.

**c. Standing test items** (each owned by the doc that raised it):
- 1.6 drops a building with missing data cleanly on load: no crash, no stuck tiles (doc 09; slice test 1).
- The workshop location missing: the save loads, the entrance tile does nothing (S3).
- Learned crafting recipes for missing items are ignored (doc 14).
- A buff with a missing ID is dropped; fertilizer with an unknown ID does nothing (doc 15).
- SpaceCore ignores a skill that's no longer registered; its XP returns on reinstall (doc 05).
- Error Items of our BigCraftables turn back into machines on reinstall (§6).
- Clock freeze solo, and its overlay form with a farmhand and in split-screen (doc 07).
- The hand-load and chain race keeps every item (§2d).
- Third-party machine harvesters taking a locked output: at worst a skipped celebration (doc 13 §6.5).
- Global inventories per player sync and stay out of chest mods' lists (§4).
- Other fertilizer mods patching the same fertilizer methods (doc 15).

## Data shape
| Record | Kind | Fields |
|---|---|---|
| Farm envelope `CharlesDR.ValleyOpus/Farm` | C# state, host save data | `SchemaVersion`, `ModVersion`, `ObjectsMigratedTo`, sections: `Codex` (doc 07), `Unlocks` (doc 14), `Familiars` (doc 18), `Players` {player ID → `SatchelCapacity`}, `SliceStats` (doc 11), `StrippedBuildings` (serialized Labs, §6), `StrippedAtDay` |
| Backup `CharlesDR.ValleyOpus/Backup_v<N>` | C# state, host save data | the previous envelope, kept until the next successful save |
| Object records | C# state, `modData` | `CharlesDR.ValleyOpus/Schema` plus each owning doc's keys; apparatus add `CharlesDR.ValleyOpus/Owner` and `CharlesDR.ValleyOpus/Tray` (the tray guid); farmers carry `CharlesDR.ValleyOpus/XpKeys` |
| Satchel bags | vanilla global inventory | `CharlesDR.ValleyOpus_Satchel_<UniqueMultiplayerID>` |
| Staging trays | vanilla global inventory | `CharlesDR.ValleyOpus_Tray_<guid>`, one per apparatus |
| Locked outputs | vanilla global inventory | `CharlesDR.ValleyOpus_LockedOutputs` |
| `IdAliases` | CP data | `OldId`, `NewId`, `Kind` (Item / Recipe / Unlock / Prime / pack-defined), `Since` (schema version); expected to stay empty |
| Sync messages | C# | `CodexRequest`, `CodexDelta`, `CodexSnapshot`; the same three for Unlocks, Familiars and Satchel capacity; `LockRequest`, `LockGrant`, `LockRelease`; `ConfigSync`; `VersionHello`; each carries `SchemaVersion` |
| Touchpoint register | design table (§7a) | not shipped |

## Content inventory
- One save envelope, one backup, about 14 message types, one console command and its GMCM button.
- Migration fixtures: one saved envelope and one set of object records per shipped schema version, taken from the previous build's played save (§8b).
- The eight test configurations and the standing test items above, carried into each build's `TESTING.md` (doc 11 §6).

## Tech hooks
- **SMAPI:** `Helper.Data.ReadSaveData`/`WriteSaveData` (host only), `Helper.Multiplayer.SendMessage` and `ModMessageReceived`, `PeerConnected` (the version handshake), `GetConnectedPlayer(id).GetMod` (peer mod info), `PerScreen<T>` for split-screen, `Saving`/`SaveLoaded`/`DayStarted` for migration and mirroring.
- **Vanilla 1.6:** `modData` on objects, buildings, animals and items; `FarmerTeam` global inventories and their mutexes for the Satchel; `mailReceived` and `craftingRecipes` per farmer; the shared clock (`Game1.shouldTimePass`, never forced for others).
- **Automate:** runs on the host. Its generic machine handler runs our machines through `Data/Machines`, so ValleyOpus ships no Automate integration code (Charles, D25); a patched `AttemptAutoLoad` marks loads as automated so our staging layer still sees every one, and locked outputs move to `CharlesDR.ValleyOpus_LockedOutputs` (doc 13 §6).
- **SpaceCore:** per-farmer skill data and its own sync.
- **Unit tests:** every migration step, the envelope round trip, the snapshot/delta merge, and the alias sweep run in the plain C# test project (doc 11 §6).
- **Risks:** global-inventory behaviour with farmhands and chest mods; the building-removal test; Error Item round trips on reinstall; the hand-load and chain race; message volume when many `tried:` keys arrive at once (batch them per tick).
- **1.7:** nothing here depends on 1.7. When `Data/ContextTags` and data-model crafting recipes arrive, the change is to data and loaders, not to the save envelope; if a 1.7 change does touch saved state, it ships as a normal migration step.

## Balance hooks
None directly. Two settings touch balance: the host's economy values rule in multiplayer (§3c), so doc 06's targets hold for everyone; and owner perks on automated loads stay on when the owner is offline (§2e), so the late-game cap is measured with perks always on.

## Open questions
None `[blocks-arch]`; every default is confirmed (doc 10). Test items above are verified in the builds, not here.
- `[blocks-content]` **Familiar meeting places, ley-node sites, hidden-area entrances and Hesper's spot** join the touchpoint register when they're chosen. *Owner: docs 04, 08, 18, 20.*
- `[blocks-content]` **Which fertilizer mod and which machine-harvesting mod** stand in for their kind in configuration 8; picked from the Nexus ranking when the configuration is first used. *Owner: the test plan.*
- `[2.0]` Integration packs beyond the four tie-ins join the pool as configurations of their own.

## Change log
- **v0.2 (2026-10-19):** B2 docs update: Labs stripped into save data on every save and restored after (Charles, option C), replacing S3's fallback, with Lab contents returned in chests on a reinstall after play without the mod; workshop warp at Town 53,9 (S13); trays and locked outputs as global inventories; XP keys in the farmer's `modData`; Automate per D25.
- **v0.1e (2026-10-19):** Charles widened the deferral to online co-op: co-op, split-screen and controller get groundwork checked by code review every build; configuration 4's in-game runs are deferred; carry-forward uses a solo save.
- **v0.1d (2026-10-19):** Charles confirmed D11–D14; they move to Decisions locked.
- **v0.1c (2026-10-19):** controller and split-screen get groundwork checked by code review in every build; their in-game tests are deferred (Charles, provisional); configuration 4 tests online co-op.
- **v0.1b (2026-10-19):** Charles chose one Satchel bag per player (§4); it moves from default D10 to Decisions locked.
- **v0.1 (2026-10-18):** first draft: the state register, multiplayer rules, sync protocol, Satchel storage (global inventory per player, default), save envelope and migration, uninstall command, compatibility rules with the touchpoint register, and the eight-configuration test pool with its rotation.
