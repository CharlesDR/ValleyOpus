# 04 · Machines, Rituals, Adjacency & Automation

Status: **DRAFT v0.7** (2026-10-13). v0.7: hand-made-only recipes are fully manual, staging included (Charles). v0.6: Automate is expected but optional, automation is encouraged after Tier 1, hand-made-only products, late-game scale across T3 Labs, the combat cap, and late-game ley nodes (Charles's guiding answers, doc 01). v0.5: directional chaining replaces arcane logistics in 1.0 (S2), and fewer BigCraftables for the same operations (A1). v0.4: discovery gate on automation, flower-fed arcane apparatus, Gloam Lanterns and late-game blended machines. v0.3: Charles's automation model replaces attunement decay. Names are working names. Display names may overlap other alchemy mods; IDs must not.

## Two ways of working (the core axis, voiced by Tobin and Oriel)
| | **Engineered lines** ⚙︎ (Tobin) | **Grand rituals** ✦ (Oriel) |
|---|---|---|
| Philosophy | science focuses and directs magic predictably | magic given room to soar |
| Shape | continuous, scalable, many small cycles | **large, dramatic single-batch processes** |
| Reliability | always works | conditional: weekday, weather, season, time, catalyst |
| Quality ceiling | capped (Albedo/silver, gold with perks) | up to Rubedo/iridium, plus unique results |
| Automate | fully supported | minor apparatus fully automated; altars **staged** by Automate, **triggered by hand** |
| Wonder | low per item, high in aggregate | high: an event the player remembers |

> **Machine count (Charles, A1):** the operations below stay, but they should not each need their own BigCraftable. Doc 13 condenses them so that combinations and arrangements of fewer machines (a shared vessel plus a heat source, catalyst or neighbour) drive the processes. The adjacency mode switches below are the main tool for this. The tables are a list of operations, not a final machine list.

### Engineered apparatus ⚙︎ (proposal)
| Stage | Apparatus |
|---|---|
| Calcination | **Calcinator** (bulk decomposition) |
| Dissolution | **Leaching Vat** |
| Separation | **Centrifuge** |
| Conjunction | **Alloy Press** |
| Fermentation | **Digester** |
| Distillation | **Steam Still** (powered by an adjacent Boiler) |
| Coagulation | **Mould Press** |
Support: **Boiler** (makes Steam power for its neighbours) · **Lightning Coil** (captures Fulgur, like a lightning rod).

### Arcane apparatus ✦ (proposal)
Arcane apparatus come in two kinds (Charles, v0.3):

**Minor arcane apparatus:** no attunement, **fully automatable**, run **lower-tier recipes** (Tier 1 and basic Tier 2). Conditions such as weekday, weather and darkness still give bonuses, so they stay distinctly magical. Quality reaches gold, but throughput is lower than engineered lines.
| Stage | Apparatus |
|---|---|
| Calcination | **Athanor** (slow, steady; better profiles, less residue) |
| Dissolution | **Alembic** |
| Separation | **Retort** (adjacency-sensitive) |
| Fermentation | **Putrefaction Vessel** (nigredo, dark twins; best in darkness) |
| Distillation | **Pelican** (circulating quality refinement) |

**Ritual altars:** run **higher-tier recipes** and reach Rubedo/iridium and ritual-only products. Automate can **load (stage)** materials into them, but the **player must trigger the ritual by hand** (see staged rituals below).
| Stage | Altar |
|---|---|
| Conjunction | **Conjunction Altar** (planetary days) |
| Coagulation | **Fixing Altar** (locks quality with Fixed Salt/Mercury) |
| Any (Great Work) | **Ritual Circle** (grand rituals, below) |
**Glassware:** starter Alembic and Retort come from Oriel and the workshop. **Bennet** (after the bus repair) crafts advanced glassware shapes, which open alternate routes, not faster ones.

### Grand rituals ✦ (new in v0.2, implied by Oriel's role)
- A **Ritual Circle** (a placeable floor pattern plus a central altar) runs **one big batch**: for example, up to a full stack of one input, plus catalysts placed at the circle's points.
- Materials can be staged by hand or by Automate. The rite must be **triggered by hand** under the right conditions (planetary day or hour, weather, moon phase from Hesper's almanac). It runs for a day or more with a visible, dramatic animation.
- **Outcome:** the whole batch is raised in quality stage, or transformed into a ritual-only product. A misjudged ritual degrades gracefully: lower stage plus extra residue, never total loss.
- **Why:** it delivers the "wonder" moments Alchemy Factory lacked, and gives a real alternative to throughput. One great rite versus a week of engineered output is a genuine choice.

### Specialist stations
- **Quarry Crucible** (Agnes; after the Quarry bridge): advanced metal transmutation and planetary-metal production.
- **Ley Tap** (Hesper; ley nodes, below): slowly gathers location-specific Primes.
- **Experiment Table** and **Assayer's Lens**: discovery (doc 07).
- **Combat preparations** (Ivo): bombs, blade coatings and combat elixirs, made at existing apparatus, not a new machine family. Useful, but **never more impactful than a Combat Mastery trinket** (Charles; doc 03a).

## Ley nodes (new in v0.2, from Hesper)
- Fixed spots across the world map (e.g. Secret Woods → Earth/Vitae, Mountain lake → Water, beach → Salt, desert → Fire, Witch's swamp → Tenebrae; later Ginger Island's jungle → Vitae and the Volcano → Fire/Magma), revealed by Hesper or by assaying.
- **Hidden alchemy areas (Charles: nice to have):** a few small new pockets, reached from compatibility-friendly spots (a sealed door in the workshop, a ley-node clearing off an existing path), can host the rarest nodes. Each needs a compatibility check against SVE, RSV and East Scarp map edits (doc 17).
- **Ley Tap:** a placeable that gathers that node's Primes slowly and **directly**, adding no new forage items (the inventory rule, doc 03b).
- Grand rituals performed on a ley node get a bonus, which gives exploration a reason to carry rituals into the world.
- Ley nodes will anchor **arcane logistics** in 2.0 (below).

## Adjacency (approved for 1.0)
- Checks the 4 orthogonal neighbours (8 is a tuning knob), evaluated when input is loaded.
- **Mode switches:** an Alembic beside a heat source distills; beside a Cooling Coil it condenses into a different product.
- **Modifiers:** a Salt Lamp stabilizes quality; a Bellows speeds things up but adds residue.
- **Shared resources:** one Boiler powers up to N adjacent Steam machines (a Factorio-style ratio).
- **Cost, cycles and area:** compact arrangements are slower or lower-yield; sprawling ones are faster but use more tiles (doc 06 counts tiles).
- **Readability:** hover tooltips; codex pages list known neighbour effects, with "?" until discovered.
- **Implementation:** C# `OutputMethod` plus a data-driven neighbour → effect table.

## Automation stance (decided by Charles, v0.3; extended v0.6)
**Automate is expected but technically optional** (Charles, doc 01). Every 1.0 apparatus must be fully usable by hand plus directional chaining, without Automate installed; Automate is the expected tool for scale and gets first-class support. **Automation is heavily encouraged after the first tier of recipes:** Spring is hand work and discovery, and from Summer Tobin's engineered lines make automating discovered Tier 1 recipes the natural next step (doc 02).

| Kind | Automate loads inputs? | Automate collects output? | Player action needed |
|---|---|---|---|
| Engineered apparatus ⚙︎ | yes | yes | none |
| Minor arcane apparatus ✦ | yes | yes | none (conditions still matter) |
| Ritual altars ✦ | **yes (staging)** | yes, after the ritual completes | **trigger the ritual by hand** |
| Ritual Circle (grand rituals) | yes (staging) | yes, after completion | trigger by hand |
| **Hand-made-only recipes** (gifts, quest items, capstones) | **no, not even staging** | no | the player loads, crafts or performs the ritual by hand |

There is **no attunement decay** in this model; the manual trigger replaces it.

**Hand-made only (Charles, doc 01):** gifts, quest items and capstones are always hand-made, by crafting recipe or, preferably, by ritual. Such recipes carry a hand-made-only flag: machines refuse them from Automate and from chaining, and they never run unattended. **Decided (Charles, 2026-10-13):** Automate can't even *stage* materials for these rituals. Capstone rituals are manual from loading to trigger, to keep their gravitas.

**Discovery gate (all kinds, Charles v0.4):** automation can't make discoveries and requires them. A machine only accepts inputs for recipes the farm has discovered, and any undiscovered output is locked in the machine until the player unloads it by hand (doc 07).

### Staged rituals
1. **Staging:** an altar accepts inputs and catalysts until its recipe is complete, then shows a *ready* glow. A Lab HUD notice says "Conjunction Altar is staged" (GMCM toggle).
2. **Triggering:** the player interacts with the staged altar. If conditions are wrong, it says why and what would help (approachability pillar), and the materials stay staged.
3. **Execution:** a **short animation** with the in-game clock frozen while it plays (a few seconds: circle lights, catalysts consumed, colour-stage shimmer), then processing runs as normal and Automate can collect the output.
4. **Mini-game (2.0):** 1.0 ships with the animation only (Charles). A mini-game version of ritual execution is on the 2.0 list.

Why this works: Automate does the logistics (pillar 1's "mechanical" half), while the moment of transformation stays a hands-on, memorable act (the "magical" half). Scaling altars means more manual triggers, which is the natural brake on wonder at scale, as Tobin would note.

**Time budget:** a heavily invested mid-game player spends about a quarter of the day on alchemy (doc 02). Hand triggers, hand unloading of discoveries and hand-made crafting all count against that, so the Mystic perk that triggers nearby staged altars together (doc 05) and a clear "staged" HUD are what keep a large ritual setup inside it.

## The Mystic garden: flower-fed apparatus (approved inspiration, v0.4)
Inspired thematically by Botania; **none of its names are reused** (doc 01).
- Certain arcane apparatus draw a passive boost (or a slow trickle of a Prime) from **flowers planted on adjacent or nearby tiles**, so part of the Mystic branch lives in the garden.
- Different flowers feed different Primes or effects (e.g. sunflowers → Fire/Sulfur, blue jazz → Water/Air, fairy roses → Aether trace), using context tags so modded flowers work too.
- Flowers are consumed or wilt slowly while feeding, so it's a real crop, not decoration.
- This is the guiding star in action: growing flowers in bulk becomes worthwhile.
- Names for our flower-fed apparatus and the energy they give are still to be chosen.

## Gloam Lanterns and the dark arts (Charles, v0.4)
A **Gloam Lantern** collects **Shades**, the currency of the dark arts (names chosen by Charles). Inspired by Graveyard Keeper.
- A lantern carried in the **main inventory** (not the Reagent Satchel, so it costs a real slot) fills as monsters die near the player. No new monster drops (doc 03b).
- Several lanterns split each kill's Shades with diminishing returns: *per lantern = 0.75^log₂(n)*. One lantern gets 1.0, two get 0.75 each, three ≈0.63 each, four ≈0.56 each.
- Base capacity is fixed; higher-quality lanterns unlocked later hold more.
- Shades fuel **higher-intensity dark magic and alchemy**: the Witch's rituals, higher-tier dark twins (Tenebrae, Mors, Perditio), and Putrefaction Vessel or altar boosts. The charge is spent, the lantern kept.
- Some potions add a % bonus to Shades gathered.
- Multiplayer: lanterns fill from deaths near their carrier, whoever got the kill.

## Late-game blended machines (Charles, v0.4)
Some late-game apparatus deliberately **blend the two disciplines**, looking and behaving partly engineered and partly arcane (clockwork that shimmers, an altar with gauges). They symbolize approaching the ideal harmony of the two, and fit the Great Work's ending. Candidates to design with the capstones.

## Directional chaining (1.0; Charles, S2)
Adjacent ValleyOpus machines pass items along a **west-to-east** line, so a production line can be laid out without a chest between every step.

| Priority | Input comes from | Output goes to |
|---|---|---|
| 1 | the machine to the **West**, if it is a ValleyOpus machine holding output that is a valid input here | the machine to the **East**, if it is a ValleyOpus machine that accepts the item |
| 2 | a **chest to the South** | a **chest to the South** |
| 3 | any direction, by Automate's normal rules | any direction, by Automate's normal rules |

- Undiscovered outputs never move along a chain; they stay locked in the machine (doc 07).
- Staged ritual altars accept chained inputs as staging but still need a hand trigger.
- **Confirmed (Charles):** chaining is ValleyOpus's own logic, so priorities 1 and 2 work **without Automate installed**; priority 3 only applies when Automate is present.
- **Built to be superseded:** arcane logistics (2.0) will **replace or extend** chaining, so the transfer logic sits behind a general "item route" interface that a later logistics system can plug into.
- **Tech note:** Automate normally moves items only through chests in a connected group, so direct machine-to-machine transfer is ours to build. With Automate installed, our Automate integration has to offer output East and South before Automate's group logic takes it (doc 09).

## Arcane logistics (2.0; taught by Hesper)
Long-range, low-bandwidth magical transport (paired sympathetic vessels; aether conduits between ley nodes) versus Automate's short-range, high-bandwidth chests and conveyors. **Moved to 2.0** (Charles, S2); directional chaining above covers 1.0.

## Vanilla machines
We **append** rules to vanilla machines (e.g. the Furnace accepts Metallum; the Keg accepts Vitae for Aqua Vitae) and never overwrite them.

## Late-game scale (Charles, doc 01)
A setup producing every endgame item **spreads across several T3 Labs**. One T3 Lab of each specialization (one per Familiar, four in all) must be more than enough room for a setup for every item in the mod. Doc 13's footprints and adjacency layouts are checked against that target.
