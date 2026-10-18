# 18 · Laboratory & Familiars

Status: **DRAFT v0.1** (2026-10-18) · Readiness: ready for architecture (no `[blocks-arch]` item open; the remaining questions are content)
Depends on: 01, 02, 03a, 05, 06, 08a, 12, 13, 14, 15 · Feeds: 06, 09, 11, 16, 17, 20, 21

## Purpose
The Laboratory is alchemy's home building: a Shed-like building from Robin that makes ValleyOpus apparatus inside it a little better, gives each tier a floorplan worth designing a layout for, and at Tier 3 takes a bonded **Familiar**, an elemental spirit that turns the Lab into a specialist room. It serves pillar 1 (layouts that reward thought: West-to-East chains, power clusters, a ritual corner), the "feel clever" player-experience pillar (each floorplan is a small puzzle, not an empty rectangle), and doc 01's late-game scale target (one T3 Lab per Familiar is more than enough room for everything). Labs are never required: every apparatus works anywhere (doc 02).

## Decisions locked
- **Three tiers** (Charles, 2026-10-13): T1 has the Shed's floor area, T2 the Big Shed's, and **T3 is 50% larger than T2** and more expensive.
- **Small, escalating bonuses** (Charles, 2026-10-13): T1 and T2 give small bonuses to apparatus inside, enough to choose a Lab over a Shed for alchemy but not to replace Sheds for anything else; T3 gives a modestly stronger version of the same bonus.
- **Familiars at T3 only; one per Lab; a Familiar bonds to one Lab** (Charles, 2026-10-13), so at most four Labs per farm are specialized and further Labs still work without one. Assigning a Familiar **is** the lateral specialization.
- **Unique floorplans and aesthetic touches** (Charles, 2026-10-13): every tier has its own shape (alcoves, pillars, a hearth wall, a dais) that encourages creative automation, and décor that takes no working floor.
- **Without the mod, Labs simply disappear** (Charles, S3); no conversion to Sheds, so sizes are free choices.
- **Unlocks** (doc 14, approved): T1 at Robin's after the Furnace Room, T2 after the Sanctum, T3 and Familiar bonding after the Great Work Chamber.
- **Late-game space** (Charles, doc 01): four T3 Labs, one per Familiar, hold a setup for every item with room to spare, and one Lab does not hold everything.
- **Built-in features count** (Charles, 2026-10-18): hearth tiles give the *Heated* neighbour state and alcove tiles are always dark (§2a).
- **Condensation rule 2 is final** (Charles, 2026-10-18): no modifier blocks; the room and range-2 conditions give those bonuses (§2b).
- **Perk rules apply** (doc 05): Lab and Familiar bonuses are variables in doc 05's combine table; they add, then clamp, and never change which recipe a machine runs.

## Player-facing spec
- **Building one.** Robin's menu lists *Laboratory* once the Furnace Room is restored. It builds like a Shed (same 7×3 footprint, two days), then upgrades in place to T2 and T3 like Shed → Big Shed. Robin's description names the bonus ("Apparatus inside work a little faster").
- **Inside.** Each tier is a room with character: shelves of glassware and star charts on the walls, a **hearth** set into the north wall, a shuttered **dark alcove**, and from T2 a raised **ritual dais** and stub pillars where Robin knocked through the old wall. The décor sits on wall tiles, so all floor is working floor.
- **Seeing the bonus.** An apparatus inside a Lab shows a small Lab mark on hover with its tier bonus ("Laboratory II: 15% faster, 10% chance of a better stage"). The chain overlay (doc 13 §7) tints hearth-heated and dark tiles so the player spots them while planning.
- **Upgrading keeps everything where it is.** Each tier's floorplan contains the previous one at the same coordinates (§2), so an upgrade never moves or drops a machine.
- **Familiars.** Once the Great Work Chamber opens, Cyprian tells of four elemental spirits. Each waits somewhere fitting its element (§4). After meeting one, the player bonds it in a T3 Lab with a hand-made offering ritual at a Ritual Altar there. The Familiar then lives in that Lab, wanders between the machines it favours (which sparkle while boosted), and can be greeted daily and given small gifts. Its name and element show on the Lab's door when hovered from outside.
- **Failure state:** none. A Lab never stops a machine working, a fifth T3 Lab simply has no Familiar, and releasing a Familiar (§4) is free and keeps its friendship.

## Rules & systems

### 1 · Tiers, costs and bonuses
Numbers are placeholders for the sweep (doc 06); the shape is the proposal.

| Tier | Floor (§2) | Source (doc 14) | Robin's cost (placeholder) | Build | Bonus to ValleyOpus apparatus inside |
|---|---|---|---|---|---|
| **T1 · Still Room** | 11×9 + 2×2 alcove (≈103 tiles) | Furnace Room (Spring) | 17,500g, 300 Wood, 20 Refined Quartz, 5 Copper Bar | 2 days, new building | Speed +10%; 5% chance of +1 quality step |
| **T2 · Long Bench** | 17×12 + alcove (≈206) | Sanctum (Fall) | 25,000g, 550 Wood, 300 Stone, 20 Vitrum, 10 Saxum | 2 days, upgrade of T1 | Speed +15%; 10% chance of +1 step |
| **T3 · Great Hall** | 17×18 + two alcoves (≈309) | Great Work Chamber (Year 2) | 60,000g, 200 Hardwood, 30 Saxum, 10 Iridium Bar, 1 Quinta Essentia | 3 days, upgrade of T2 | Speed +20%; 15% chance of +1 step; **one Familiar** (§3) |

- **Costs.** T1 is a Shed plus a little glass and copper, all vanilla, so it is buildable in Spring. T2 asks for Tier 1 and 2 Elements the player makes by Fall (Saxum is already "construction material for Lab upgrades" in doc 03a). T3 asks for a Quinta Essentia, so it follows the Aether supply.
- **What the bonus touches.** Only ValleyOpus apparatus of the Engineered, Minor arcane and ritual kinds (doc 13 §3). Specialists (Experiment Table, Quarry Crucible, Ley Tap) take no Lab bonus, as they take no perks (doc 05). Vanilla machines inside a Lab (kegs, jars) get nothing, which is what keeps a Lab from replacing a Shed.
- **Speed** joins doc 05's Speed variable (add, total at most +50%). **The quality chance** joins the Quality steps variable: rolled once at load, in the condition snapshot (doc 12 §3), then clamped at the recipe's cap like every other step. No Lab bonus raises a cap.
- **Several Labs.** Players may build as many as they like (doc 02); each gives its own bonus to the apparatus inside it. Nothing stacks across buildings.
- **Mid-game target.** Doc 06 sets "Lab T2 + L5 profession" as roughly par with vanilla on alchemy's best routes, so the T2 numbers are the ones the sweep tunes first.

### 1a · What a Lab never does
- **It never selects a recipe.** In ValleyOpus's own data, Lab tier and Familiar are **Bonus conditions only**: never Required, never a Variant *(default; Charles to confirm, see Open questions)*. That keeps doc 02's promise that machines work anywhere, and it means moving a line from a Shed into a Lab only ever speeds it up. Content packs may still use the Lab GSQs (§Tech hooks) as they like; the validator does not forbid it.
- **It never changes a neighbour state when its tier or Familiar changes.** Upgrading, bonding or releasing never flips a machine from one recipe to another (doc 13 §1a). The only built-in features that touch neighbour states are fixed parts of the floorplan (§2), present from the moment the tier is built.

### 2 · Floorplans
Each tier contains the previous tier's floor **at the same coordinates**: T2 knocks through the east and south walls of T1, and T3 adds a hall to the south of T2. Nothing that exists in a lower tier ever becomes a wall, so upgrading in place is always safe.

Legend: `.` floor · `#` wall · `+` door · `H` hearth in the wall (the floor tile beside it is **heated**) · `D` shuttered alcove (always **dark**) · `O` stub pillar (blocks a tile) · `~` raised ritual dais (ordinary floor; exactly fits a 5×5 Circle Sigil) · `S` Familiar's shrine niche (in the wall, no floor)

**T1 · Still Room** (11×9 and a 2×2 alcove)
```
  ######HH#####
  #...........#
  #...........#
  #...........#
  #...........#
###...........#
#DD...........#
#DD...........#
###...........#
  #...........#
  ######+######
```
A plain room on purpose, for Spring: the hearth lets two Alembics distill without a kindled Athanor or Boiler, and the alcove is the farm's first guaranteed place to putrefy in darkness.

**T2 · Long Bench** (17×12)
```
  ######HH###########
  #............~~~~~#
  #............~~~~~#
  #...........O~~~~~#
  #............~~~~~#
###............~~~~~#
#DD.................#
#DD...........O.....#
###.................#
  #.................#
  #.................#
  #.................#
  #.................#
  #########+#########
```
The south rows run the **full 17 tiles** West to East: the "long bench" a Tobin line wants. The two stub pillars mark the old east wall and break the north half into a hearth bay, an alcove bay and the **ritual dais** in the north-east corner, a ritual corner ready for the Circle Sigil (Crystal Vault, Winter).

**T3 · Great Hall** (17×18; two alcoves, two hearths)
```
  ######HH###########
  #............~~~~~#
  #............~~~~~#
  #...........O~~~~~#
  #............~~~~~#
###............~~~~~#
#DD.................#
#DD...........O.....#
###.................#
  #.................#
  #.................#
  #.................#
  #.................#
  #.................#
  H.................#
  H....O...O...O....###
  #.................DD#
  #.................DD#
  #.................###
  #####+#######SS####
```
The new hall adds a **colonnade** of three pillars that splits it into four bays (each a natural cluster around one Boiler or Athanor), a second hearth on the west wall, a second dark alcove on the east, and the **Familiar's shrine** in the south wall, where the bonded spirit rests. Floor grows from 204 to 306 tiles before features (the 50% rule).

**Exterior.** All tiers keep the Shed's 7×3 footprint; the sprite grows taller and grander per tier, as the Big Shed's does *(default)*. 1.6 building skins give each tier one or two exterior styles.

**Aesthetic touches** (doc 21 lists the art): wall shelves of jars and glassware, star charts, hanging herbs, a stained-glass window with seasonal light, a brass orrery in T3, and the shrine's element-coloured glow. Floors and wallpaper can be changed as in a Shed. Dr. Sorenson's optional contract reward (doc 14) is an alternate interior set, a sterile white-tile "clean room", as a cosmetic only.

### 2a · Built-in features
These are part of the map, not items, so they don't break doc 03b's purpose rule.
- **Hearth** (2 per hearth wall): a heat source for the floor tiles directly beside it, so a machine there is in its *Heated* state (doc 13 §1a), exactly as beside a kindled Athanor. It is plain heat only: never Vapor, Fulgur or Flamma Viva, so it can't power a Vat or calcine Cinis Phoenicis. Theurgist's "near a kindled Athanor" perk doesn't count it.
- **Dark alcove**: its tiles always meet the darkness condition, even with lamps elsewhere in the room. Putrefaction recipes (doc 12 §7) can therefore run there, by hand or automated, without the Lab being *required* for them: the darkness condition is still met elsewhere, as doc 12 defines it.
- **Pillars and the shrine** only block tiles. **The dais** is ordinary floor; it just shows where a Circle fits.

### 2b · Draft process layouts (revisiting doc 13's condensation rule 2)
Doc 13 left "neighbours are other apparatus, never decorations" provisional until draft layouts existed. Three drafts, using doc 13's rules (West in, East out, South chest, power by adjacency):

**(a) Vapor spine**, the basic Tobin line (any tier):
```
B  B  B  B      Boilers (Vapor), one north of each Vat
V→ V→ V→ V→     Process Vats, chained West to East
c  c  c  c      South chests (inputs and the Boilers' Ignis + Aqua)
```
Every side of every Vat is already spoken for: West and East by the chain, South by its chest, North by its power. A modifier block would have to displace a chest or a Boiler. Sharing one Boiler between two rows (Vat rows north and south of a Boiler row) frees tiles but costs the north row its South chests, so it runs from the chain and Automate only: a real trade-off, which is the kind of puzzle the floorplans are for.

**(b) Hearth corner** (T1, Spring): two Alembics on the hearth tiles distill with no fuel, each chaining East into a Retort, with chests South. The only free heat in the building, so it is the obvious first layout.

**(c) Ritual corner** (T2 dais): the Circle Sigil on the dais, the altar at its centre, catalysts on its four points, and a Boiler beside the altar for Perpetuum Mobile (doc 13 §5). The pillar beside the dais leaves a one-tile walkway for hand-triggering.

**Finding:** the crowding Charles foresaw is real. Every neighbour slot of an automated machine is already a chaining, chest or power slot, so modifier blocks would mostly be in the way. The bonuses a modifier block would have given now come from the **room** (Lab tier, Familiar, hearth, alcove) and from **range-2 conditions** such as flowers in Garden Pots, which use no adjacent side. **Decided** (Charles, 2026-10-18): rule 2 is final, so there are no modifier blocks.

### 2c · Space check (doc 13 §9, re-run on real floorplans)
- Floor per tier: T1 ≈103, T2 ≈206, T3 ≈309 tiles (vanilla Shed 11×9 and Big Shed 17×12, per the Stardew wiki; pillars and features counted). Allowing a walkway, a T3 Lab works out to about **280 usable tiles**.
- Doc 13's "everything at once" estimate is ~75 machines at ~5 tiles each plus 2–3 Circles: **~450 tiles**.
- **One T3 Lab can't hold everything** (280 < 450) ✓. **Two hold it** (560), and **four** (1,120) are more than enough, with room for scale and decoration ✓.
- **Per Familiar** (§3): the operations split roughly Ignis ~15 machines, Aqua ~20, Aer ~20, Terra ~20 plus the altars. At ~5 tiles each plus one Circle, the largest share is about 125–150 tiles, half a T3 Lab ✓.

### 3 · Familiars
Four elemental spirits from Paracelsus (doc 08a): no gender, not villagers, one per T3 Lab, farm-wide unique.

**Specialization by operation** *(default; Charles to confirm)*. Each Familiar favours the operations of its element, the classical reading of the seven operations of the Magnum Opus. In its Lab, every recipe whose operation is one of these (on any apparatus kind, altars included; the Experiment Table excepted) gains **Speed +10%** and **+10% chance of +1 quality step**, on top of the T3 bonus and inside doc 05's clamps. Each also has one **signature** that lowers a cost or adds a convenience, never a new neighbour state.

| Familiar | Element | Favoured operations | Signature (in its Lab) |
|---|---|---|---|
| **Salamander** | Ignis | Calcination, decomposition | Calcinators burn 50% less fuel (multiplies with Engineer; still at least one fuel per stated interval) |
| **Undine** | Aqua | Dissolution, Fermentation (putrefaction included) | Boiler charges last 50% longer |
| **Sylph** | Aer | Separation, Distillation | Lightning Coils inside charge in storms as if under open sky, and store one more charge |
| **Gnome** | Terra | Conjunction, Coagulation | catalysts (Sal Fixum, Mercurius Fixus, Shades) have a 25% chance not to be consumed |

So a Salamander Lab is the decomposition house, an Undine Lab the cellar of vats and putrefaction, a Sylph Lab the still-room of vapours and lightning, and a Gnome Lab the forge of unions and salts. Ritual Altars and Circles benefit in whichever Lab they stand, by their recipe's operation.

*Alternative considered:* boosting recipes whose output contains the Familiar's Prime. It reads closer to "machines of its element" (doc 08a), but most substances are pairs of Primes, so every Lab would boost a scattered quarter of everything and specialization would blur. By operation, a player can plan a Lab around one Familiar.

### 4 · Meeting, bonding and release
1. **Opening.** The Great Work Chamber scene (doc 14 ★) has Cyprian name the four spirits. Each then appears at its place once the player has a T3 Lab *(proposal; places are content, doc 20)*:
   - Salamander: the Mines' lava floors (80+) or the Volcano.
   - Undine: the Mountain lake on a rainy morning.
   - Sylph: the Mountain on a windy day (falling leaves or petals) or in a storm.
   - Gnome: the Quarry (after the bridge) or the Mines' 40–79 floors.
   All are recurring conditions, never calendar gates (doc 14). Meeting is a short event and a codex page.
2. **Bonding** is a **hand-made ritual** (✋, doc 12 §9) on a Ritual Altar **inside the T3 Lab** that will host the spirit: **Anima** (the offering doc 03a already names) plus the element's pure pair: Ros Maialis (Undine), Saxum (Gnome), Pneuma (Sylph). Flamma Viva is power, never an item (doc 13 §5), so the Salamander's rite is Anima + Fire Quartz with a **Required Flamma Viva**: a kindled Athanor beside the altar. Triggering it binds the Familiar to that Lab. This is the "attuned in Mystic rituals" of doc 08a; any player can perform it, Mystic or not (perks never forbid).
3. **Release** is free: interact with the shrine and confirm. The Familiar returns to its place and can be bonded again elsewhere with a new offering. Demolishing a Lab releases its Familiar. Moving a Lab with Robin keeps the bond.
4. **Bond hearts** (0–5): +1 point per daily greeting and per liked gift (Tier 1 substances of its element). Heart events at 1, 3 and 5 (doc 20); 5 hearts adds a shrine décor variant and a lore page. **Hearts give no power**: the full Lab bonus applies from the day of bonding, so no one grinds friendship for throughput.

Because the bonding rite is hand-made only, an environmental Variant may decorate it (a stronger first-day glow on the element's planetary day), as doc 12 allows for hand-triggered recipes.

### 5 · Edge cases
- **Multiplayer:** Labs are farm buildings, built and upgraded by anyone as with Sheds. Familiar bonds and hearts are **farm-wide** and stored by the host; anyone can greet, gift, bond or release. A machine's Lab bonus uses the Lab it stands in, whoever loaded it; perks still follow doc 05 §6. Split-screen needs nothing special.
- **Joja route:** Robin and the workshop exist on both routes (doc 14), so nothing changes.
- **SVE and tie-in mods:** farm maps change, but buildings place normally. Tie-in packs may add Familiars only in 2.0 (the four are the 1.0 set; the data allows more, uniqueness is per Familiar).
- **Automate:** a Lab is an ordinary location; Automate links chests and machines inside it as in a Shed. Bonuses are applied in the resolver at load (doc 13 §6), so automated loads get them too.
- **Walk of Life:** no profession touches Labs or Familiars, so resets change nothing here.
- **Uninstall (S3):** Labs disappear with their contents. "Prepare for removal" offers to empty every Lab into chests outside first (doc 09). Familiar state is mod save data and simply goes unused.

## Data shape
| Table | Kind | Fields |
|---|---|---|
| `Data/Buildings` entries | CP data | three buildings: `Builder` Robin, `BuildCost`, `BuildMaterials`, `BuildDays`, `BuildingToUpgrade` (T2 ← T1, T3 ← T2), `BuildCondition` (`VALLEYOPUS_UNLOCKED <flag>`, doc 14), `IndoorMap`, `Skins`, `CustomFields` {`CharlesDR.ValleyOpus_LabTier`} |
| `LabTiers` | CP data | `Tier`, `BuildingId`, `Perks` [same shape as doc 05: `Variable`, `Scope` (apparatus kinds), `Op`, `Value`], `FamiliarSlots` (0 or 1) |
| Interior maps | CP map assets | one per tier, nested coordinates (§2); tile properties `ValleyOpus_Heat` (on hearth wall tiles, with the tiles they heat), `ValleyOpus_Dark` (alcove tiles), `ValleyOpus_Shrine` |
| `Familiars` | CP data | `Id`, `Element` (Prime ID), `Operations` [operation IDs], `Perks` [doc 05 shape, scoped to those operations], `Signature` [doc 05 perk rows, e.g. `FuelCost`, `ChargeLength`, `CatalystSaveChance`, `IndoorStormCharge`], `MeetEvent`, `BondRecipe` (a ✋ recipe ID), `LikedGifts` [ingredient specs], `HeartEvents` [3 IDs], `NameKey`, `Sprite` |
| Familiar state | C# state, host save data | per Familiar: `Met`, `BondedLab` (building GUID or null), `BondPoints`, `GreetedToday`; mirrored in the Lab building's `modData` (`FamiliarId`) for fast lookup |
| New perk variables | CP data (doc 05 `PerkVariables`) | `CatalystSaveChance` (add, max 50%), `IndoorStormCharge` (bool, max) |

## Content inventory
- 3 buildings (Data/Buildings), 3 interior maps, 3 exterior sprites (plus skins), built-in décor tiles.
- 4 Familiars: sprites with idle and wander frames, a shrine glow each, 4 meeting events, 4 bonding rites (✋ recipes), 12 heart events (3 each, doc 20), liked-gift lists, 4 codex pages.
- 2 new perk variables (above).
- 1 cosmetic interior set as a Sorenson contract reward (doc 14).
- Slice (doc 11): T1 only, with the hearth and alcove.

## Tech hooks
- **Buildings:** 1.6 `Data/Buildings` with `BuildingToUpgrade` and `IndoorMap`; upgrading swaps the map on the same indoor location, so objects keep their tiles (nested floorplans make that safe). **Verify early:** that an upgrade with a different indoor map keeps all placed objects, as Shed → Big Shed does.
- **Lab GSQs:** `CharlesDR.ValleyOpus_IN_LAB <minTier>` and `CharlesDR.ValleyOpus_LAB_FAMILIAR <id>` (docs 09, 12), so CP data and other packs can read them.
- **Resolver:** at load, reads the location's building tier (`CustomFields`) and Familiar, and adds their perk rows to the perk service (doc 05) along with the player's perks. Neighbour-state tests (doc 13) also read map tile properties, so a hearth tile counts as a heat source and an alcove tile as dark. **This is the one architecture change** this doc asks of doc 13: neighbour sources can be map tiles as well as objects.
- **Familiars:** a light C# renderer (a non-colliding sprite that wanders between favoured machines) or Custom Companions (doc 09's candidate, decided at the gate). Greeting and gifting are interaction handlers, not a villager friendship record.
- **Sylph signature:** a storm-day hook that charges Coils in a Sylph Lab as the outdoor rule does. Assumed: Coils indoors don't storm-charge otherwise (a lightning rod needs open sky); doc 13 to confirm.
- **Risks:** hover cost of the Lab mark in large Labs (shared with doc 13's power-link tooltip); building removal on uninstall (doc 09's early test); Robin's menu ordering with SVE's extra buildings.
- **1.7:** `Data/Buildings` is stable in the 1.7 notes we track; no other migration expected.

## Balance hooks
Doc 06 reads: per-tier speed and quality chance against the phase targets (T2 + L5 at par; T3 + Familiar + L10 + all six professions under the 150% ceiling), Familiar signatures as cost reductions in GPTD, Robin's costs against the Shed and Big Shed, and the §2c space check with real machine counts once content exists. Placeholders: every bonus number, every cost, the Familiar numbers.

## Open questions
- ~~`[blocks-arch]` **Built-in features as neighbours** (§2a)~~ **settled** (Charles, 2026-10-18): both count; neighbour tests read map tiles as well as objects.
- ~~`[blocks-content]` **Condensation rule 2** (doc 13)~~ **settled** (Charles, 2026-10-18): "no modifier blocks" is final.
- `[blocks-content]` **Lab tier in recipes** (§1a): Bonus only in our own data (default), or allow Lab- or Familiar-only named Variants. *Owner: Charles.*
- `[blocks-content]` **Familiar specialization by operation** (§3) rather than by output Prime. *Owner: Charles. Default: by operation.*
- `[blocks-content]` Familiar meeting places, gift lists and heart events. *Owner: doc 20.*
- `[blocks-content]` Every bonus, cost and Familiar number. *Owner: the sweep.*
- *For doc 12:* darkness outside a Lab (night outdoors, caves, unlit rooms) needs a definition; this doc only guarantees the alcove. *Owner: doc 12 follow-up.*
- `[2.0]` Familiars from tie-in packs; Familiar heart perks; more Lab specializations (doc 10).

## Change log
- **v0.1 (2026-10-18):** first draft: three tiers with costs and bonuses, nested floorplans with hearths, alcoves, a dais and a colonnade, draft process layouts that test doc 13's rule 2, the space check on real floor areas, four Familiars specialized by operation, meeting, bonding by hand-made rite, release and bond hearts, data shape and tech hooks.
