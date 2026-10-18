# 02 · Progression & Unlocks

Status: **DRAFT v0.7** (2026-10-18). v0.7: the Laboratory section is cut to what this curve needs and points to doc 18, which owns the Lab (cleanup sweep). v0.6: the full unlock map and the workshop rooms now live in doc 14; island ley nodes yield Primes. v0.5: Lab size and floorplan direction, Lab design moved before the architecture gate, late-game gates and escalating conditions approved (Charles). v0.4: pacing targets, no calendar gates, escalating conditions, the 1.0 "finished" state, Lab sizing for the late game, and late-game gates (Ginger Island, Volcano, Skull Cavern, Mr. Qi), all from Charles's guiding answers in doc 01. v0.3: Lab tier direction from Charles; the first playable slice is Spring Y1 (doc 00, S1).

## Soft start (approved; details to be specified later)
- **Trigger:** 2 days after the Community Center opens, the first time the player enters **Town** between **6:00am and 3:00pm**.
- **Content:** a short cutscene with **Oriel** (a new NPC), deliberately unlike the usual "letter from the Wizard" opening. She walks the player **behind the Community Center** to the **derelict workshop** in its basement, and mentions **Tobin** as someone who could help get it running.
- **Spec later:** the exact flag, conditions (rain? festival days? SVE's Town map), dialogue and gifts are deferred to the cutscene/content design pass.
- **Implementation note:** "CC opens" means the vanilla event where Lewis shows the Community Center. Verify the event ID and record the day it fires in a mail flag. A festival on the trigger day defers the cutscene to the next eligible day.

## Pacing targets (Charles, doc 01)
- **A diligent player** chasing progress discovers **most things in Year 1** and **everything by the end of Year 2**. Doing it that fast may need insider knowledge from an earlier playthrough, and that's fine.
- **A typical player** finishes around **Year 4**, because of how much there is to do, **not because of time gates**.
- **No hard calendar gates.** Every unlock is gated by progress: a workshop bundle, skill level, friendship, a base-game milestone, or a discovery. Recurring conditions (planetary weekday, weather, season, moon phase) are allowed, since they come round again. No content may require "Year 2" or later as such.
- **Time share:** a heavily invested mid-game player spends about **a quarter of their day** on alchemy. Alchemy competes for time naturally, as any new activity does, but never asks for busywork (walking back and forth, repeated menus, waiting with nothing to do). Doc 16 (UX) budgets hand actions against this.
- **Escalating conditions.** The number of conditions a recipe needs grows as the player advances (approved: Tier 1 needs none or one, Tier 2 one or two, Tier 3 and capstones two or more). There's no hard upper bound for now; condition stacking is flagged as a likely place to rebalance later (doc 06).
- **Automation is encouraged after the first tier.** Spring is hand work and discovery. From Summer, Tobin's engineered lines make automating discovered Tier 1 recipes the natural next step (doc 04).

## Two places, two jobs
| | **Derelict workshop** (under the CC) | **Farm and Laboratories** |
|---|---|---|
| Purpose | unlock hub: bundles, blueprints, recipes, research, NPC time | production: build and run apparatus |
| Player builds here? | no | yes |
| Progress | restoration bundles (like the CC) | apparatus, adjacency layouts, Lab tiers |

## Year 1 roll-out
The narrative shape of Year 1. The approved, authoritative versions are doc 14 §2 (rooms), §5 (base-game gates) and §8 (what's reachable each season); where this summary and doc 14 differ, doc 14 wins.

Stages follow the seven operations of the Magnum Opus. Each row's unlocks come mostly from workshop bundles. **Seasons here are the expected pace for a diligent player, not gates:** every row unlocks through progress, so a player who restores the workshop faster moves faster.

| Season | Opus stages | New concepts | Key unlocks |
|---|---|---|---|
| **Spring** | Calcination | decomposing items into Primes; assaying; the codex | starter apparatus and glassware (from Oriel and the workshop), Assayer's Lens, first workshop room; **Laboratory T1** at Robin's once that room is restored |
| **Summer** | Dissolution, Separation | solvents, the alembic, **Vapor**; Tobin's engineered lines | Tobin's mechanical line, skill L5 choice, first adjacency combos; **Agnes** introduced in Clint's shop; **Hesper**'s visits and almanac begin |
| **Fall** | Conjunction, Fermentation | planetary weekdays, transmutation, putrefaction → dark twins (with the Witch), first **grand ritual** | **Laboratory T2**, planetary apparatus, ley nodes, Witch events; **advanced transmutation once the Quarry bridge is repaired** |
| **Winter** | Distillation, Coagulation | Gelum/frost, fixing quality, Aether rituals | skill L10 choice, the Great Work begins, Lab T3 groundwork |
| **Year 2** (diligent pace) | the colour stages | Tier 3 substances, capstones | **Laboratory T3** with Familiar augmentation, capstones, the Great Work's ending (the 1.0 finished state) |

**Gates tied to base-game progress** (either the CC or the Joja route counts; the late-game hooks below are approved as proposals):
- **Quarry bridge** (Crafts Room, or the Joja purchase) → Agnes's Quarry crucible and advanced transmutation.
- **Bus repair** (Vault, or Joja) → the desert, Bennet's glassware commissions and advanced arcane apparatus. Basic glassware never depends on the desert.
- **Adventurer's Guild access** → Ivo's bounties.
- **Skull Cavern** (proposal) → deeper Ivo bounties and Shade-rich hunting for Gloam Lanterns (doc 04).
- **Ginger Island** (proposal) → island ley nodes (the Volcano node → Ignis and Terra, the island jungle node → Terra and Aether; ley nodes yield Primes, doc 14) and a tropical source column in the sweep.
- **Volcano forge** (proposal) → a Ignis-aligned ritual site; the obvious home for a combat or forging capstone, with forging itself still 2.0.
- **Mr. Qi** (proposal) → a few Qi special orders that ask for alchemical feats, paid in Qi Gems, plus a hard late recipe that uses Qi Beans or Radioactive Ore.

## The finished state and the long tail
- **1.0 has a definitive finished state** (Charles): completing the Great Work. It concludes the main story (Cyprian's unfinished Opus, doc 08), awards the capstone items and leaves visible, permanent changes in the valley.
- **After finishing, 1.0 still offers** codex depth tiers, NPC heart events, the secret recipe re-roll, rare-condition recipes (special weekday and weather combos, Hesper's celestial events) and integration content for installed mods. None of it is required, and none of it reopens the story.
- **2.0 adds an endless long tail** with no defined endpoint (prestige perks, the brew map, new arcs).

## Laboratory (designed in doc 18)
[Doc 18](18-laboratory-and-familiars.md) owns the Laboratory: tiers, costs, bonuses, floorplans and Familiars. The parts this curve depends on:
- A Shed-like building from Robin with three tiers. **Machines work anywhere**; a Lab makes ValleyOpus apparatus inside it a little better but is never required, and Lab tier is a Bonus condition only, so no product needs a Lab (Charles, 2026-10-18). Players may build several.
- **Pace:** T1 after the Furnace Room (Spring), T2 after the Sanctum (Fall), T3 and Familiar bonding after the Great Work Chamber (Year 2 at a diligent pace; doc 14).
- **Familiars** specialize one T3 Lab each, so at most four Labs per farm are specialized; further Labs still work without one.
- **Late-game scale (Charles, doc 01):** one T3 Lab per Familiar (four) is more than enough room for a setup producing every item, and one Lab does not hold everything (checked in doc 18 §2c).
- **Without the mod installed:** Labs simply disappear (doc 00, S3).

## Capstones (approved direction; design deferred)
Multiple capstones in different areas (farming, combat, exploration, economy, automation), each with tradeoffs. Candidate hosts: Ivo (combat), Hesper (exploration), Dr. Sorenson (economy), Tobin (automation), Oriel and Cyprian (the Lapis Philosophorum). Seeds are in doc 03a.

Direction from Charles (doc 01):
- **The ending is a mix** of powerful items, story conclusions and aesthetic changes to the world.
- **Capstones change the valley or its residents in visible ways.** Compatibility rule (proposal): changes happen in places we own (the workshop, ley-node sites, our NPCs), through new decorations on tiles no common mod edits, or through vanilla NPCs' dialogue, events and outfits. They never redraw shared map areas such as Town or the Mountain. Each change gets a compatibility check against SVE, RSV and East Scarp (doc 17).
- **Capstones are always hand-made**, by crafting or preferably by ritual. They are never produced by Automate or chaining (doc 04).
