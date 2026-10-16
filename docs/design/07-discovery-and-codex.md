# 07 · Discovery & the Codex

Status: **DRAFT v0.7** (2026-10-16) · Readiness: ready for architecture (no `[blocks-arch]` item open)
Depends on: 01, 03a, 04, 12, 13 · Feeds: 05, 06, 14, 16, 17, 20

v0.7: retrofitted to the doc 00 template; adopts doc 12's discovery keys and Charles's "a Table discovery unlocks only that recipe" rule; adds the discovery path for recipes the Table can't make, power and heat at the Table, and the codex data shape. v0.6: the target player and complexity ceiling, "feeling clever" as a design goal, the pace of discovery and hand-made-only entries (Charles's guiding answers, doc 01). v0.5: Experiment Table input counts (doc 00, R2). v0.4 applies Charles's verdicts on the discovery research ([research/inspiration-discovery-moments.md](../research/inspiration-discovery-moments.md)).

## Purpose
**Learning and unlocking through experimentation** (approved principle). The game invites players to try things and celebrates every new discovery with a pleasing animation, popup or toast. Discovery is also the gate on automation, so it serves pillar 4 (wonder first, then scale) and pillar 1 (the codex shows both routes to a result side by side).

## Decisions locked
- **Who it's for** (Charles, doc 01): both cozy players and factory builders; unlocking every secret takes dedication; high complexity is fine because the hint layers make everything reachable in-game; **feeling clever is the goal**.
- **Pace** (Charles, doc 01): a diligent player discovers most entries in Year 1 and all by the end of Year 2; most players take until about Year 4. Nothing is time-gated.
- **Table inputs** (Charles, R2): two, three after a workshop unlock; input count is data.
- **Discovery gates automation** (Charles, doc 04 v0.4): automation can't make discoveries and requires them.
- **A Table discovery unlocks only that recipe** (Charles, 2026-10-16, default confirmed). **Conditional:** revisit in the balance pass if completing the codex turns out too obnoxious (open questions).
- **Celebration tiers, residue divination, codex depth, the per-farm secret and the shared farm codex** (approved v0.4).
- **Hand-made-only entries count only hand-made crafts** (Charles, doc 01).

## Player-facing spec

### Who it's for
- A cozy player can enjoy a partial codex; completing it is a real achievement. Optimizers will probably want a wiki or spreadsheet, and that's acceptable.
- Some recipes are deliberately puzzles: the right pair under the right condition, an adjacency layout, a ritual's catalyst placement. Hints point toward the puzzle; they never hand over the answer.
- Knowledge, not the calendar, is what speeds a replay up. The per-farm secret recipe keeps one surprise even for experts.

### Ways to discover
1. **Experiment Table:** place one to two items or substances, or up to three after a workshop unlock (Charles, R2). A defined combination plays the anticipation beat, reveals the result and records the recipe. An undefined one yields Caput Mortuum, marks the combination "tried" in the codex so the player never repeats it unknowingly, and gives a tiny bit of XP. The Table **respects its neighbours** (doc 13 §5): Ignis + Aqua reveals Vapor anywhere, but Metallum becomes Orichalcum only on a Table beside a charged Lightning Coil.
2. **Assaying:** use the **Assayer's Lens** on any item to reveal its Prime profile (first time only), and on places or objects (ley nodes, the Witch's Hut, the Wizard's tower, the workshop's ruined fixtures) to reveal lore and recipe hints. This is the direct analog of Wizardry's Analyze.
3. **Apparatus behaviour:** the first time a machine produces something new, or an adjacency effect fires, it's recorded, but **only when the player unloads it by hand** (see Discovery gates automation).
4. **Conditions:** running a machine under new conditions (a planetary day, storm, night) can reveal variant outputs.
5. **Research at the workshop:** restoring workshop rooms unlocks blueprints; NPCs working there (Oriel, Tobin, the Curator, Hesper on visiting days) offer research hints.

### Recipes the Table can't make
Rituals, putrefactions and capstones (Tenebrae, Mors, Perditio, Prima Materia, Alkahest, Azoth, Lapis Philosophorum and other recipes without a T mark in doc 03a) are discovered only by **performing them by hand** the first time, a manual first attempt (doc 12 §3).
- **Where the inputs come from:** hint channels, never guesswork alone. Each such recipe has at least one hint that names its inputs: a Cyprian journal page, Oriel's advice once the right workshop room is restored, Hesper's almanac for timing, or residue divination.
- **The Table still helps:** placing such a recipe's inputs on the Table consumes nothing and leaves no "tried" mark; the Table hums and says which apparatus the union needs ("this wants an altar, and the dark"). That one line is the payoff for a player who has worked the pair out.
- **Apparatus also hint:** an apparatus only accepts an item that can lead to a recipe it runs (doc 13 §6), so a vessel taking an item is itself a quiet clue.

### Discovery gates automation (Charles, v0.4)
- A machine accepts inputs from Automate or chaining only for recipes the farm has already discovered. A player can still load an undiscovered recipe **by hand**: that is the manual first attempt.
- If a machine produces something undiscovered (a hand-loaded first attempt, a condition variant on a stormy day, a new adjacency effect), the output is **locked in the machine**: Automate, chaining and other extractors can't take it. The machine shows a distinct "something new inside" sparkle. When the player unloads it by hand, the discovery plays and the recipe unlocks for automation.
- **Discovering the item another manual way** (the Table, assaying) **releases a locked output of that item**. It does not unlock other recipes or routes that make the same item: each recipe is learned on its own (doc 12 §10, Charles 2026-10-16).
- Inspired by Kingdom Come: Deliverance II, where you can only auto-brew potions you've brewed by hand.

### Celebration (approved requirement; tiers approved v0.4)
Three tiers, from small to large:
- **Toast** for every new entry: icon, name and one line of flavour. Several at once collapse into one "3 new discoveries" toast.
- **Hold-up pose + sound** for firsts that matter (first Tier 2, first dark twin, capstone items), using vanilla's hold-up-item pose.
- **Codex flourish** for milestones (a Tier 1 row complete, first Tier 2, first dark twin): a short page-turn animation with a sound. The first discovery of each Tier opens the codex to its page once (skippable).
- **Anticipation beat:** a 1–2 second simmer/glow on the Experiment Table before the result.
- **Clock freeze:** every mandatory animation above pauses the in-game clock so it never eats daylight; optional ones can be skipped with one press. In multiplayer the clock is shared and can't pause for one player (to verify), so there these play as non-blocking overlays or stay under ~1 second.
- **Feedback styles:** arcane apparatus get glow and shimmer, engineered ones a crisp mechanical clunk; **blended machines mix the two** to show the disciplines approaching harmony (doc 04; doc 13's `FeedbackStyle`).
- **Cauda Pavonis** iridescent shimmer for the first time an item passes nigredo → albedo.
- Settings in GMCM: toast frequency (all / milestones only) for players who find it noisy.

### Hints (approachability pillar)
A layered hint system, from subtle to explicit:
1. Item flavour text ("It smells faintly of brimstone…"), and apparatus that accept an item only if it belongs in one of their recipes.
2. The codex grid shows silhouettes and "warmer" hints for pairs adjacent to known ones. A three-item Table load that contains a defined pair fails as usual but adds a "something here wants a partner" line *(default)*.
3. **Residue divination (Charles, v0.4):** spend Caput Mortuum to divine a hint (cost may scale with how far the target is from what's known). Every pile of residue is a choice: fertilizer, a tiny sale, saving it for late recipes that turn residue into valuable materials, or hints. Working things out yourself stays optimal; savings get you unstuck.
4. NPC channels, one per owner (vanilla and SVE villagers add flavour reactions, not new hint channels; doc 08): **Oriel** (ritual and concept advice), **Tobin** (machine and layout tips), **Hesper** (the weekly almanac letter: planetary days, weather, celestial events), **Finch** (curiosity items), **Lark** (gentle comic attempts that point at untried pairs), **the Curator** (workshop lore).
5. In-world readables in the derelict workshop under the CC (Cyprian's journals, half-burned notes, the wordless Silent Pages).

Hints never cost real money and are never gated outside the game.

### Failure state
Gentle: residue (low value, minor uses: fertilizer, Smaltum glaze, ballast, hint divination, and later Prima Materia), the combination marked tried, and a little XP. No explosions that destroy machines. Comic "puffs" are fine.

### Codex depth (approved v0.4)
Entries gain a lore line at 1, 10 and 50 crafted, with a progress pip toward the next. Automated output counts after the manual first discovery, which gives players who automate a reason to come back to the codex. Hand-made-only entries (gifts, quest items, capstones; doc 04) count only hand-made crafts.

### The farm's secret recipe (approved v0.4)
- One or two late-game **secret recipes are seeded per farm** from a candidate pool, so guides can't spoil them. They're revealed through in-world readables and Hesper's almanac, which hints early at *which* secret this farm holds.
- **Re-roll:** an expensive, time-consuming and difficult way to re-seed it (e.g. a multi-day grand ritual needing Quinta Essentia and rare catalysts). Restarting a Stardew save is a huge undertaking, so players aiming for a specific late recipe must never be stuck without it.
- Multiplayer-safe: stored with the farm's shared codex.

### The shared farm codex (approved)
- **One codex per farm; every discovery is shared** across all players.
- Toasts show for the discovering player; others get a quieter "Alex discovered Vapor" notice.

## Rules & systems
- **Keys.** Discovery state is doc 12 §10's keys (`recipe:`, `item:`, `tried:`, `assay:`, `effect:`, `count:`), plus two owned here: `hint:<HintId>` (a hint was shown, so divination never sells it twice) and `milestone:<MilestoneId>` (a flourish played).
- **What sets `recipe:`.** A Table success, or unloading a locked output by hand. Nothing else: assaying, hints and discovering the item elsewhere never set it.
- **What sets `item:`.** Any manual route that produces or reveals the item. It releases locked outputs of that item and shows its codex entry; it doesn't make any recipe automatable.
- **The Table.** Accepts 1 to capacity inputs. Matching is exact (doc 12 §3). Outcomes, in order: (1) a defined, Table-attemptable recipe whose Required conditions hold → its output (Item class) or a reveal and a puff (Power or Process-state class), at Nigredo; (2) the inputs of a defined recipe that isn't Table-attemptable, or whose Required conditions fail → nothing consumed, a line naming the missing apparatus or condition kind; (3) otherwise → residue, a little XP and a `tried:` key. A disabled secret candidate counts as undefined.
- **Tried signatures** are the sorted input item IDs, so "A + B" and "B + A" are one entry; a Table condition doesn't change the signature (the codex's "tried" mark means "tried in these conditions" only in its tooltip).
- **Edge cases.** Multiplayer: a farmhand's discovery is sent to the host, recorded, then broadcast; two players discovering the same thing in one tick both get a toast, and the first recorded wins attribution. Joja route: hints tied to workshop rooms use doc 14's unlock flags, set on both routes. Removed packs: their keys stay hidden, not deleted (doc 12). Uninstall: codex data is mod save data and is simply ignored.

## Data shape
| Asset | Kind | Key fields |
|---|---|---|
| `Codex/Pages` | CP data | `Id`, `Group` (Primes / Tier 1 grid / Tier 2 / Dark twins / Metals / Great Work / Apparatus / Places / pack-defined), `SortOrder`, `TitleKey`; entries are **derived** from `Primes`, `Substances` and `Recipes` (doc 12) and placed by their tier and tags, so packs get pages without extra rows |
| `Codex/Lore` | CP data | `TargetId` (item or recipe), `Threshold` (1, 10, 50), `TextKey` |
| `Hints` | CP data | `Id`, `Target` (recipe, item or condition ID), `Channel` (Flavour / Grid / Divination / NPC name / Readable), `Reveals` (Inputs / Apparatus / Condition / Existence), `Requires` (GSQ or unlock flag), `DivinationCost` (residue count), `TextKey` |
| `Milestones` | CP data | `Id`, `Trigger` (first of a tier, a row complete, a process state), `Celebration` (Toast / HoldUp / Flourish), `TextKey` |
| Farm codex | C# state, host save data | doc 12 §10 keys plus `hint:` and `milestone:`, each with day and player; secret picks and re-roll count; Table capacity flag |
| Player prefs | GMCM config, per player | toast frequency, staged-altar HUD notice, celebration skip |

## Content inventory
- One codex entry per Prime, substance, product, apparatus and ley node: about 65 substances (doc 03a), 14 apparatus (doc 13), 6–10 ley nodes.
- About 3 lore lines per craftable entry; about 150 hints (at least one input-naming hint for every non-Table recipe); about 15 milestones.
- Cyprian's journal pages and the Silent Pages as readables (scripts in doc 20).

## Tech hooks
- The farm codex lives in host save data, synced through SMAPI multiplayer messages.
- Our `VALLEYOPUS_DISCOVERED` GSQ lets plain `Data/Machines` rules (Furnace, Keg) respect discovery.
- Locked outputs are enforced by doc 13's staging layer, Automate machine wrapper and chaining (doc 13 §6), so the gate lives in one place.
- The codex UI, toasts and flourishes are doc 16's; clock freeze in multiplayer is a doc 17 test item.

## Balance hooks
Doc 06 reads discovery pace from this data: hints per recipe, divination costs, XP per discovery and per tried combination, and the number of recipes reachable before each unlock. The sweep reports how many recipes a player must make separately for codex completion (the Table-unlock revisit) and how many non-Table recipes have only one hint.

## Open questions
- `[blocks-content]` **Table unlock scope, balance revisit** (from doc 12): only that recipe for now; if codex completion is too obnoxious, options are letting a Table discovery also unlock the same output's twin route, or a cheaper divination for already-known items. *Owner: doc 06 balance pass / Charles.*
- `[blocks-content]` **Partial Table matches:** exact matching only, with the "wants a partner" line on a three-item load containing a defined pair *(default)*. *Owner: Charles.*
- `[blocks-content]` **Table outputs at Nigredo** *(default)*, so the Table discovers and apparatus produce quality. *Owner: Charles.*
- `[blocks-content]` **Secret re-roll:** a discovered secret is replaced on re-roll, its codex entry kept as lore *(default, doc 12)*. *Owner: Charles.*
- `[blocks-content]` **Hint wording and counts** per recipe. *Owner: doc 20.*

## Change log
- **v0.7 (2026-10-16):** template retrofit; doc 12 keys and the "only that recipe" rule; discovery path for non-Table recipes; neighbours at the Table; apparatus acceptance as a hint; codex data shape.
- **v0.6 (2026-10-13):** target player, complexity, feeling clever, pace, hand-made-only entries.
- **v0.5:** Table input counts. **v0.4:** Charles's verdicts on the discovery research.
