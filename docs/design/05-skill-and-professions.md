# 05 · Skill & Professions

Status: **DRAFT v0.6** (2026-10-17) · Readiness: ready for architecture once Charles approves the defaults under Open questions
Depends on: 01, 02, 04, 06, 12, 13, 14, 15 · Feeds: 06, 09, 11, 16, 17, 18

## Purpose
The Alchemy skill rewards the player for doing alchemy by hand and paces the two disciplines: level 5 picks a side (Tobin's engineered lines or Oriel's grand rituals) and level 10 deepens it or bridges to the other side. It serves pillar 1 (two separate axes, with the interesting choices where they meet), discovery over shopping (levels open means, never recipes), and the safety rules for Walk of Life resets. XP is one progression track among several: discovery, the codex, workshop restoration and NPC hearts keep going after level 10.

## Decisions locked
- **SpaceCore custom skill**, working name **Alchemy**; internal ID `CharlesDR.ValleyOpus_Alchemy`; the display name is still deferred (doc 10).
- **Vanilla-speed curve** (Charles, P4): level 10 arrives around late Year 1, and level 10 never means everything is discovered.
- **Perks only add, never forbid**, and every combination of the six professions is safe (doc 01, Walk of Life).
- **Combat cap** (Charles, doc 01): no perk pushes a combat preparation past a Combat Mastery trinket.
- **Unlocks open means, never recipes** (Charles, doc 14): skill levels give capacity and tools, never a recipe.
- **No perk duplicates a vanilla profession** (Charles, doc 15 §3a): no sell-price perk, because our products are Artisan Goods and Artisan already applies (doc 15 §6).
- **No prestige in 1.0**; "Ultimate Mastery" perks are 2.0.

## Player-facing spec
- The skill appears in the skills page like a vanilla skill, with its own icon and level-up screen. Each level-up names what it opened (a Satchel tab, a Lens upgrade).
- **XP comes from hand alchemy:** discovering things, collecting by hand, triggering rituals, assaying and failed experiments. A failed Table attempt still gives a little XP, so experimenting never feels wasted.
- **Level 5:** choose **Mechanist** (engineered) or **Mystic** (arcane). **Level 10:** choose a deepening perk or a bridge perk under the level 5 choice.
- **Bridges are physical:** each bridge perk rewards putting the two disciplines side by side on the farm (a Boiler beside an altar, engineered machines around a kindled Athanor), so mixing shows up in layouts, not only in numbers. That feeds doc 18's floorplans and doc 04's blended moments.

## Rules & systems

### 1 · XP sources
| Source | Amount | Notes |
|---|---|---|
| First discovery of a recipe | big (placeholder 100, scaled by tier) | Table, apparatus or ritual reveal; once per recipe per player |
| First discovery of a neighbour or Bonus effect (`effect:` key, doc 12) | medium (placeholder 25) | once per effect |
| Triggering a ritual (altar or circle) | medium (placeholder 20, scaled by tier) | per trigger, to the triggering player |
| Collecting apparatus output **by hand** | small (placeholder 2 per item, capped per collection) | none when Automate or directional chaining moves it, as with vanilla machines |
| Assaying a new item with the Lens | small (placeholder 5) | once per item |
| Failed Table attempt (undefined combination) | tiny (placeholder 3) | once per tried key, so it can't be farmed |

Hand collection is the only repeatable source besides rituals, and both cost player time against doc 02's quarter-of-the-day budget, so a large automated setup gives the skill nothing for free.

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

### 4 · Professions
Numbers are placeholders. ⚙︎ = Engineered kind, ✦ = Minor arcane kind, ✦A / ✦C = altar and circle (doc 12, doc 13 §3). Specialists (Table, Quarry Crucible, Ley Tap) take no perks.

| L5 | L10 A (deepen) | L10 B (bridge) |
|---|---|---|
| **Mechanist**: ⚙︎ apparatus work 25% faster; each Boiler powers one more neighbour and each Lightning Coil stores one more charge | **Engineer**: ⚙︎ quality cap rises from silver to gold; ⚙︎ apparatus fed by a chained West neighbour make 25% less residue | **Artificer**: ✦ apparatus also get the Mechanist speed bonus; an altar or circle with a powered Boiler beside it has a 25% chance of +1 quality step when triggered |
| **Mystic**: rituals use one fewer catalyst (never below one); triggering an altar also triggers every other staged altar of yours within 3 tiles | **Hermeticist**: met Bonus conditions on ✦, ✦A and ✦C apparatus count twice for quality; circle batches hold 25% more | **Theurgist**: ⚙︎ apparatus within 2 tiles of a kindled Athanor get +1 quality step (still capped at silver, or gold with Engineer) |

Changes from v0.5, and why:
- **Engineer's residue perk** read "Automate groups with ≥4 machines". Automate is optional (doc 04), so it now reads the built-in directional chain.
- **Artificer's altar perk** read "altars staged by Automate". It now reads a Boiler beside the altar, the same blended picture as Perpetuum Mobile (doc 13 §3), and works without Automate.
- **Theurgist** read "⚙︎-made inputs". Tagging an item by where it was made splits stacks (items with different mod data don't stack), so it now reads a kindled Athanor nearby.
- **Hermeticist** read "the planet's day or matching weather". Bonus conditions are already defined per recipe (doc 12 §4), so the perk doubles those instead of adding its own calendar. The grand-ritual batch bonus moved here from Mystic so each L5 has two perks.

### 5 · Combination-safety rules
A player may own **all six** professions through Walk of Life resets. To keep that safe:
1. **Perks only add, never forbid.** No perk removes a capability or blocks a machine. Caps and automation classes are properties of apparatus kinds, never of professions.
2. **Perks never choose recipes.** A perk changes only the result of a recipe that already matched: speed, quality, residue, catalyst use, batch size, power reach. It may read a neighbour, but only as a bonus, the way adjacency Bonus conditions do (doc 13 §1a). That keeps the recipe uniqueness rule (doc 13 §1a) and the wait-idle rule (doc 12) intact under any set of professions.
3. **One variable per perk, one combine rule per variable** (table below). Shared variables add, then clamp.
4. **All-six test:** a fixture grants every profession and checks that doc 06's caps hold, including the 150% ceiling, after Lab bonuses and indirect boosters.
5. **Combat cap:** no perk changes an effect's strength; perks touch production only, so combat items stay under the trinket benchmark by construction.
6. **No Walk of Life dependency.** Everything works without it.

| Variable | Perks | Other sources | Combine and clamp |
|---|---|---|---|
| Speed (per kind) | Mechanist, Artificer | Lab tier (doc 18); indirect boosters such as the Azoth Diffuser (strongest one only, doc 15) | add; total at most +50% (placeholder) |
| Quality cap (⚙︎) | Engineer | the recipe's own lower cap | Engineer raises the kind's cap to gold; a recipe's lower cap still wins |
| Quality steps | Artificer, Hermeticist, Theurgist | Bonus conditions, catalysts, Lab tier | add, then clamp at the cap (doc 12 §5) |
| Residue | Engineer | Fulgur overclock (+), the recipe's rule | multiply; never below zero |
| Catalyst count | Mystic | the recipe | subtract; never below one |
| Trigger radius | Mystic | none | single source |
| Circle batch size | Hermeticist | Circle Sigil base | multiply |
| Power reach | Mechanist | Boiler N, Coil charges (doc 13) | add |

### 6 · Whose perks apply
- **Hand load:** the player who loaded the machine. **Chain or Automate load:** the machine's owner (the player who placed it).
- **Ritual trigger:** the triggering player's perks (catalyst saving, trigger radius, quality). Altars set off by the Mystic radius use that same player's perks.
- XP always goes to the acting player; a farmhand's action never credits the host.

## Data shape
| Table | Kind | Fields |
|---|---|---|
| `XpSources` | CP data | `Event` (Discovery / EffectDiscovery / RitualTrigger / HandCollect / Assay / TriedKey), `Amount`, `TierScale`, `OncePer` (Recipe / Effect / Item / Key / none), `CapPerAction` |
| `Professions` | CP data | `Id`, `Level` (5 / 10), `Parent` (L5 ID or null), `NameKey`, `DescKey`, `Icon`, `Perks` [{`Variable`, `Scope` (apparatus kinds or IDs), `Op` (Add / Multiply / Max), `Value`, `Bonus` (optional condition ID, e.g. a powered Boiler adjacent or a kindled Athanor within 2)}] |
| `PerkVariables` | CP data | `Id`, `Combine` (Add / Multiply / Max), `Min`, `Max` |
| Skill-level unlocks | CP data | rows in doc 14's `Unlocks` table with `Source.Type = SkillLevel` |
| XP, level, professions | C# state | SpaceCore's per-farmer skill data; saved per player, synced by SpaceCore |
| One-time XP keys | C# state | per player, next to the discovery state (doc 12); stops repeat XP |

In 1.0 the six profession rows are fixed, but every perk value and XP amount is data the sweep can rewrite. Content-pack professions are 2.0.

## Content inventory
- One skill, 10 levels, 6 professions, 13 perks (above).
- Four odd-level conveniences to name (levels 1, 3, 7, 9).
- Art: a skill icon and 6 profession icons (doc 21). Text: level-up lines and profession descriptions, in i18n from the start.

## Tech hooks
- **SpaceCore** `Skills.RegisterSkill`: levels, professions, the level-up menu and multiplayer sync. The choice at 5 and 10 uses SpaceCore's two-option menu.
- **Perk evaluation** lives in the one resolver behind `OutputMethod` (docs 12, 13): when it computes speed, quality, residue and batch size, it asks a small perk service for each variable's total, which applies the combine table. Ritual triggers call the same service.
- **Speed** sets the machine's `MinutesUntilReady` at load, so Automate and chaining see ordinary machines.
- **Risks:** confirm whether SpaceCore custom-skill XP feeds 1.6 Mastery XP (default if not: it doesn't, and nothing else changes); confirm Walk of Life's resets see SpaceCore professions; the all-six fixture joins the test suite (doc 17).
- **1.7:** no known migration beyond SpaceCore's own.

## Balance hooks
Doc 06 reads: XP per day at a diligent pace against the L5 and L10 targets; gold per tile-day with and without each profession; the all-six case against the 150% ceiling; the hand-time share of a Mystic altar setup with and without the trigger radius. Placeholders: every XP amount, every perk value, the speed clamp.

## Open questions
- `[blocks-content]` **Odd-level conveniences** (1, 3, 7, 9). Candidates: a second Lens charge, the codex showing hint counts per recipe, one free hint divination a week, a longer Table "tried" history. Default: pick them in the content pass, each one a means, never a recipe. *Owner: Charles.*
- `[blocks-content]` **Default: perks never choose recipes** (§5 rule 2); bridges read adjacency only as bonuses. *Owner: Charles to approve.*
- `[blocks-content]` **Default: XP only from hand actions**; none when Automate or chaining moves output. *Owner: Charles to approve.*
- `[blocks-content]` **Default: chain and Automate loads use the placer's perks** (§6). *Owner: Charles to approve.*
- `[2.0]` Prestige / Ultimate Mastery perks; content-pack professions.

## Change log
- **v0.6 (2026-10-17):** retrofitted to the doc 00 template. XP source table with once-only keys and no XP from automation; pace targets (L5 early Summer, L10 late Winter); doc 14 §6 skill-level unlocks adopted; perks made independent of Automate and of item provenance; bridges made physical; the rule that perks never choose recipes; the perk-variable combine table; whose perks apply in multiplayer and under automation; data shape and tech hooks.
- **v0.5 (2026-10-13):** pacing check and the combat cap on perks (Charles's guiding answers, doc 01).
- **v0.4 (2026-10-13):** skill curve decided (doc 00, P4).
- **v0.3:** attunement removed from the perks.
