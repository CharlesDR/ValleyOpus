# 05 · Skill & Professions

Status: **DRAFT v0.7** (2026-10-18) · Readiness: ready for architecture (no `[blocks-arch]` item open; Charles answered every question on 2026-10-18)
Depends on: 01, 02, 04, 06, 12, 13, 14, 15 · Feeds: 06, 09, 11, 16, 17, 18

## Purpose
The Alchemy skill rewards the player for doing alchemy by hand and paces the two disciplines: level 5 picks a side (Tobin's engineered lines or Oriel's grand rituals) and level 10 deepens it or bridges to the other side. It serves pillar 1 (two separate axes, with the interesting choices where they meet), discovery over shopping (levels open means, never recipes), and the safety rules for Walk of Life resets. XP is one progression track among several: discovery, the codex, workshop restoration and NPC hearts keep going after level 10.

## Decisions locked
- **SpaceCore custom skill**, working name **Alchemy**; internal ID `CharlesDR.ValleyOpus_Alchemy`; the display name is still deferred (doc 10).
- **Vanilla-speed curve** (Charles, P4): level 10 arrives around late Year 1, and level 10 never means everything is discovered.
- **Perks only add, never forbid**, and every combination of the six professions is safe (doc 01, Walk of Life).
- **Combat cap** (Charles, doc 01): no perk pushes a combat preparation past a Combat Mastery trinket.
- **Skill levels give capacity, tools and conveniences** (doc 14 §6; odd levels picked in the content pass, approved 2026-10-18), **and may also grant recipes** (Charles, 2026-10-18), under the critical-path guard in §3.
- **Perks may lower costs and grant new recipes, but never change an existing setup** (Charles, 2026-10-18). Profession and level recipes are the one exception to doc 14's "no unlock grants a recipe".
- **Automated output gives reduced XP, not none** (Charles, 2026-10-18), as vanilla crab pots still give Fishing XP.
- **Recipes granted by a profession give no discovery XP** (Charles, 2026-10-18).
- **Each of the six professions grants one signature recipe** (Charles, 2026-10-18), on its own inputs and adjacency so no existing setup changes.
- **No perk duplicates a vanilla profession** (Charles, doc 15 §3a): no sell-price perk, because our products are Artisan Goods and Artisan already applies (doc 15 §6).
- **No prestige in 1.0**; "Ultimate Mastery" perks are 2.0.

## Player-facing spec
- The skill appears in the skills page like a vanilla skill, with its own icon and level-up screen. Each level-up names what it opened (a Satchel tab, a Lens upgrade).
- **XP comes mostly from hand alchemy:** discovering things, collecting by hand, triggering rituals, assaying and failed experiments. Automated setups still give some XP, but less than doing the work by hand. A failed Table attempt still gives a little XP, so experimenting never feels wasted.
- **Level 5:** choose **Mechanist** (engineered) or **Mystic** (arcane). **Level 10:** choose a deepening perk or a bridge perk under the level 5 choice.
- **Bridges are physical:** each bridge perk rewards putting the two disciplines side by side on the farm (a Boiler beside an altar, engineered machines around a kindled Athanor), so mixing shows up in layouts, not only in numbers. That feeds doc 18's floorplans and doc 04's blended moments.

## Rules & systems

### 1 · XP sources
| Source | Amount | Notes |
|---|---|---|
| First discovery of a recipe | big (placeholder 100, scaled by tier) | Table, apparatus or ritual reveal; once per recipe per player; **none for a recipe a profession granted** |
| First discovery of a neighbour or Bonus effect (`effect:` key, doc 12) | medium (placeholder 25) | once per effect |
| Triggering a ritual (altar or circle) | medium (placeholder 20, scaled by tier) | per trigger, to the triggering player |
| Collecting apparatus output **by hand** | small (placeholder 2 per item, capped per collection) | to the collecting player |
| Output moved by **directional chaining or Automate** | reduced (placeholder 25% of the hand amount) | to the machine's owner, only while they are in the game; like vanilla crab pots, automation still trains the skill, just slower |
| Assaying a new item with the Lens | small (placeholder 5) | once per item |
| Failed Table attempt (undefined combination) | tiny (placeholder 3) | once per tried key, so it can't be farmed |

Hand collection and rituals are the full-rate repeatable sources, and both cost player time against doc 02's quarter-of-the-day budget. Automated output earns a quarter of that, so a large setup helps the skill along without replacing hand work. The level cap of 10 bounds any surplus.

### 2 · Curve and pace
Vanilla thresholds (100, 380, 770, 1300, 2150, 3300, 4800, 6900, 10000, 15000). Pace targets for a diligent player (doc 02): **level 5 in early Summer** and **level 10 by late Winter of Year 1**. That is about 75 XP a day in Spring and about 160 a day afterwards, a rise that comes naturally as rituals and Tier 2 discoveries open. The sweep tunes the amounts to hit those targets.

### 3 · Skill-level unlocks (adopted from doc 14 §6)
| Level | Unlock |
|---|---|
| 1 | *odd-level convenience, to fill (see Open questions)* |
| 2 | Reagent Satchel: one more tab of capacity |
| 3 | *odd-level convenience, to fill* |
| 4 | Assayer's Lens assays a whole chest at once (codex profiles only) |
| 5 | profession choice |
| 6 | upgraded Gloam Lantern recipe (needs the Undercroft restored) |
| 7 | *odd-level convenience, to fill* |
| 8 | Satchel picker pins a recipe on several machines at once (doc 13 §6a) |
| 9 | *odd-level convenience, to fill* |
| 10 | profession choice |

**No bundle requires an Alchemy-level unlock**, so the skill and the workshop stay separate tracks and the bundle critical-path check (doc 14 §3) never depends on XP.

**Recipes granted at a level-up** (Charles, 2026-10-18). Any level may also grant a recipe. It is recorded as known and gives no discovery XP. Three guards keep the critical path intact:
1. **Nothing important is locked behind a level.** No bundle, workshop room, story beat, Great Work step or other unlock may need a recipe that *only* a level grants. A level recipe is either optional (a convenience or an alternate route) or also reachable through discovery.
2. **Order of operations.** A level recipe's apparatus, inputs and conditions must already be reachable at that level's expected pace (§2). A recipe granted at L3 can't need a machine from a Tier III room, or it would sit unusable.
3. **The same rules as any recipe.** It overlaps no existing recipe (doc 13 §1a), so it changes no existing setup, and it follows the wait-idle rule.

The sweep's pace report (doc 14, Balance hooks) checks guards 1 and 2 for every level and profession recipe, alongside the bundle critical-path check.

### 4 · Professions
Numbers are placeholders. ⚙︎ = Engineered kind, ✦ = Minor arcane kind, ✦A / ✦C = altar and circle (doc 12, doc 13 §3). Specialists (Table, Quarry Crucible, Ley Tap) take no perks.

| L5 | L10 A (deepen) | L10 B (bridge) |
|---|---|---|
| **Mechanist**: ⚙︎ apparatus work 25% faster; each Boiler powers one more neighbour and each Lightning Coil stores one more charge | **Engineer**: ⚙︎ quality cap rises from silver to gold; a ⚙︎ apparatus that is fed by another machine rather than by hand burns 25% less fuel (Calcinator fuel), and a Boiler or Lightning Coil fed that way makes each charge last 25% longer | **Artificer**: ✦ apparatus also get the Mechanist speed bonus; an altar or circle with a powered Boiler beside it has a 25% chance of +1 quality step when triggered |
| **Mystic**: rituals use one fewer catalyst (never below one); triggering an altar also triggers every other staged altar of yours within 3 tiles | **Hermeticist**: met Bonus conditions on ✦, ✦A and ✦C apparatus count twice for quality; circle batches hold 25% more | **Theurgist**: ⚙︎ apparatus within 2 tiles of a kindled Athanor get +1 quality step (still capped at silver, or gold with Engineer) |

Changes from v0.5, and why:
- **Engineer's second perk** was "Automate groups with ≥4 machines make less residue". Less residue is no reward, because residue is the consolation for a failure (Charles, 2026-10-18), and Automate is optional (doc 04). It now lowers fuel cost for any ⚙︎ apparatus fed by another machine (Charles chose fuel saving over a bonus-output chance, 2026-10-18). A chain neighbour, a chest or Automate all count, so the wording never has to name Automate.
- **Artificer's altar perk** read "altars staged by Automate". It now reads a Boiler beside the altar, the same blended picture as Perpetuum Mobile (doc 13 §3), and works without Automate.
- **Theurgist** read "⚙︎-made inputs". Tagging an item by where it was made splits stacks (items with different mod data don't stack), so it now reads a kindled Athanor nearby.
- **Hermeticist** read "the planet's day or matching weather". Bonus conditions are already defined per recipe (doc 12 §4), so the perk doubles those instead of adding its own calendar. The grand-ritual batch bonus moved here from Mystic so each L5 has two perks.

### 5 · Combination-safety rules
A player may own **all six** professions through Walk of Life resets. To keep that safe:
1. **Perks only add, never forbid.** No perk removes a capability or blocks a machine. Caps and automation classes are properties of apparatus kinds, never of professions.
2. **Perks never change an existing setup** (Charles, 2026-10-18). A perk may improve the result of a recipe that already matched (speed, quality, yield, batch size, power reach), lower a cost (catalysts, fuel), or **grant a new recipe**. A granted recipe obeys the uniqueness rule like any other (doc 13 §1a): its inputs plus apparatus plus adjacency overlap no existing recipe, so no machine already running switches output when a profession is taken. Perks read neighbours only as bonuses, the way adjacency Bonus conditions do, and the wait-idle rule (doc 12) holds under any set of professions.
   - A granted recipe stays known even if a Walk of Life reset removes the profession, so a setup built on it never stops working.
3. **One variable per perk, one combine rule per variable** (table below). Shared variables add, then clamp.
4. **All-six test:** a fixture grants every profession and checks that doc 06's caps hold, including the 150% ceiling, after Lab bonuses and indirect boosters.
5. **Combat cap:** no perk changes an effect's strength; perks touch production only, so combat items stay under the trinket benchmark by construction.
6. **No Walk of Life dependency.** Everything works without it.

| Variable | Perks | Other sources | Combine and clamp |
|---|---|---|---|
| Speed (per kind) | Mechanist, Artificer | Lab tier (doc 18); indirect boosters such as the Azoth Diffuser (strongest one only, doc 15) | add; total at most +50% (placeholder) |
| Quality cap (⚙︎) | Engineer | the recipe's own lower cap | Engineer raises the kind's cap to gold; a recipe's lower cap still wins |
| Quality steps | Artificer, Hermeticist, Theurgist | Bonus conditions, catalysts, Lab tier | add, then clamp at the cap (doc 12 §5) |
| Fuel cost and charge length | Engineer | the apparatus's fuel rule (doc 13) | multiply; a fuel item is still consumed, never less than one per stated interval |
| Residue | none | Fulgur overclock (+), the recipe's rule | the recipe's rule; no perk lowers residue |
| Catalyst count | Mystic | the recipe | subtract; never below one |
| Trigger radius | Mystic | none | single source |
| Circle batch size | Hermeticist | Circle Sigil base | multiply |
| Power reach | Mechanist | Boiler N, Coil charges (doc 13) | add |

### 6 · Whose perks apply
- **Hand load:** the player who loaded the machine. **Chain or Automate load:** the machine's owner, the player who placed it (Charles, 2026-10-18).
- **Ritual trigger:** the triggering player's perks (catalyst saving, trigger radius, quality). Altars set off by the Mystic radius use that same player's perks.
- XP goes to the acting player; a farmhand's action never credits the host. XP from automated output goes to the machine's owner while they are in the game, and is not banked while they are away.

## Data shape
| Table | Kind | Fields |
|---|---|---|
| Level-up recipes | CP data | doc 14 `Unlocks` rows with `Source.Type = SkillLevel` and a `Grants` entry of a new type, Recipe, recorded as known |
| `XpSources` | CP data | `Event` (Discovery / EffectDiscovery / RitualTrigger / HandCollect / AutoCollect / Assay / TriedKey), `Amount`, `TierScale`, `OncePer` (Recipe / Effect / Item / Key / none), `CapPerAction` |
| `Professions` | CP data | `Id`, `Level` (5 / 10), `Parent` (L5 ID or null), `NameKey`, `DescKey`, `Icon`, `GrantsRecipes` [recipe IDs, recorded as known with no discovery XP], `Perks` [{`Variable`, `Scope` (apparatus kinds or IDs), `Op` (Add / Multiply / Max), `Value`, `Bonus` (optional condition ID, e.g. a powered Boiler adjacent or a kindled Athanor within 2)}] |
| `PerkVariables` | CP data | `Id`, `Combine` (Add / Multiply / Max), `Min`, `Max` |
| Skill-level unlocks | CP data | rows in doc 14's `Unlocks` table with `Source.Type = SkillLevel` |
| XP, level, professions | C# state | SpaceCore's per-farmer skill data; saved per player, synced by SpaceCore |
| One-time XP keys | C# state | per player, next to the discovery state (doc 12); stops repeat XP |

In 1.0 the six profession rows are fixed, but every perk value and XP amount is data the sweep can rewrite. Content-pack professions are 2.0.

## Content inventory
- One skill, 10 levels, 6 professions, 13 perks (above). **One signature recipe per profession** (Charles, 2026-10-18), six in all, granted on choosing the profession. Any level-up recipes are optional extras. Rows come in the content pass.
- Four odd-level conveniences to name (levels 1, 3, 7, 9).
- Art: a skill icon and 6 profession icons (doc 21). Text: level-up lines and profession descriptions, in i18n from the start.

## Tech hooks
- **SpaceCore** `Skills.RegisterSkill`: levels, professions, the level-up menu and multiplayer sync. The choice at 5 and 10 uses SpaceCore's two-option menu.
- **Perk evaluation** lives in the one resolver behind `OutputMethod` (docs 12, 13): when it computes speed, quality, fuel use and batch size, it asks a small perk service for each variable's total, which applies the combine table. Ritual triggers call the same service.
- **Automated XP:** the resolver credits `AutoCollect` XP to the owner when a chain or Automate takes output, using the same hook that applies the owner's perks.
- **Speed** sets the machine's `MinutesUntilReady` at load, so Automate and chaining see ordinary machines.
- **Risks:** confirm whether SpaceCore custom-skill XP feeds 1.6 Mastery XP (default if not: it doesn't, and nothing else changes); confirm Walk of Life's resets see SpaceCore professions; the all-six fixture joins the test suite (doc 17).
- **1.7:** no known migration beyond SpaceCore's own.

## Balance hooks
Doc 06 reads: XP per day at a diligent pace against the L5 and L10 targets; gold per tile-day with and without each profession; the all-six case against the 150% ceiling; the hand-time share of a Mystic altar setup with and without the trigger radius. Placeholders: every XP amount, every perk value, the speed clamp.

## Open questions
- `[blocks-content]` **The six signature recipes** (§4): design one per profession in the content pass, each with inputs and adjacency that overlap no existing recipe. *Owner: content pass.*
- `[2.0]` Prestige / Ultimate Mastery perks; content-pack professions.

## Change log
- **v0.7 (2026-10-18):** Charles's review. Perks may lower costs and grant recipes, but never change an existing setup. Automated output gives a quarter of the XP instead of none. Profession-granted recipes give no discovery XP. Engineer's residue perk is replaced by a fuel saving for machine-fed ⚙︎ apparatus (Charles's pick). The odd-level default is approved. Skill levels may also grant recipes, under a critical-path guard and an order-of-operations check (§3).
- **v0.6 (2026-10-17):** retrofitted to the doc 00 template. XP source table with once-only keys and no XP from automation; pace targets (L5 early Summer, L10 late Winter); doc 14 §6 skill-level unlocks adopted; perks made independent of Automate and of item provenance; bridges made physical; the rule that perks never choose recipes; the perk-variable combine table; whose perks apply in multiplayer and under automation; data shape and tech hooks.
- **v0.5 (2026-10-13):** pacing check and the combat cap on perks (Charles's guiding answers, doc 01).
- **v0.4 (2026-10-13):** skill curve decided (doc 00, P4).
- **v0.3:** attunement removed from the perks.
