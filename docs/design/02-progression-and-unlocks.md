# 02 · Progression & Unlocks

Status: **DRAFT v0.3** (2026-10-13). v0.3: Lab tier direction from Charles; the first playable slice is Spring Y1 (doc 00, S1). The seasonal roll-out is a proposal to react to.

## Soft start (approved; details to be specified later)
- **Trigger:** 2 days after the Community Center opens, the first time the player enters **Town** between **6:00am and 3:00pm**.
- **Content:** a short cutscene with **Oriel** (a new NPC), deliberately unlike the usual "letter from the Wizard" opening. She walks the player **behind the Community Center** to the **derelict workshop** in its basement, and mentions **Tobin** as someone who could help get it running.
- **Spec later:** the exact flag, conditions (rain? festival days? SVE's Town map), dialogue and gifts are deferred to the cutscene/content design pass.
- **Implementation note:** "CC opens" means the vanilla event where Lewis shows the Community Center. Verify the event ID and record the day it fires in a mail flag. A festival on the trigger day defers the cutscene to the next eligible day.

## Two places, two jobs
| | **Derelict workshop** (under the CC) | **Farm and Laboratories** |
|---|---|---|
| Purpose | unlock hub: bundles, blueprints, recipes, research, NPC time | production: build and run apparatus |
| Player builds here? | no | yes |
| Progress | restoration bundles (like the CC) | apparatus, adjacency layouts, Lab tiers |

## Year 1 roll-out (proposal)
Stages follow the seven operations of the Magnum Opus. Each row's unlocks come mostly from workshop bundles.

| Season | Opus stages | New concepts | Key unlocks |
|---|---|---|---|
| **Spring** | Calcination | decomposing items into Primes; assaying; the codex | starter apparatus and glassware (from Oriel and the workshop), Assayer's Lens, first workshop room; **Laboratory T1** at Robin's once that room is restored |
| **Summer** | Dissolution, Separation | solvents, the alembic, **Steam**; Tobin's engineered lines | Tobin's mechanical line, skill L5 choice, first adjacency combos; **Agnes** introduced in Clint's shop; **Hesper**'s visits and almanac begin |
| **Fall** | Conjunction, Fermentation | planetary weekdays, transmutation, putrefaction → dark twins (with the Witch), first **grand ritual** | **Laboratory T2**, planetary apparatus, ley nodes, Witch events; **advanced transmutation once the Quarry bridge is repaired** |
| **Winter** | Distillation, Coagulation | Gelum/frost, fixing quality, Aether rituals | skill L10 choice, the Great Work begins, Lab T3 groundwork |
| **Year 2+** | the colour stages | Tier 3 substances, capstones | **Laboratory T3** with Familiar augmentation, capstones, long-tail discoveries |

**Gates tied to base-game progress** (either the CC or the Joja route counts):
- **Quarry bridge** (Crafts Room, or the Joja purchase) → Agnes's Quarry crucible and advanced transmutation.
- **Bus repair** (Vault, or Joja) → the desert, Bennet's glassware commissions and advanced arcane apparatus. Basic glassware never depends on the desert.
- **Adventurer's Guild access** → Ivo's bounties.

## The long tail
After Year 1, content comes through **discovery and engagement**: codex milestones, NPC heart events, rare-condition recipes (special weekday and weather combos, Hesper's celestial events) and integration content for installed mods.

## Laboratory (approved: 3 tiers; design deferred)
- A Shed-like building from Robin. Machines work anywhere; the Lab makes them better but isn't required. Players may build several.
- **3 buildings** (Charles, 2026-10-13): **T1** matches the base Shed's footprint and interior, **T2** the Big Shed's, and **T3** is larger again and more expensive.
- **Bonuses:** T1 and T2 give small but escalating bonuses to apparatus inside. That is a reason to pick a Lab over a Shed for alchemy, but not a replacement for Sheds otherwise. T3 gives a modestly stronger version of the same bonus.
- **Familiars augment T3 only:** a T3 Lab can be augmented by assigning one of the four elemental Familiars. **One Familiar per Lab, and a Familiar can be assigned to only one Lab**, so at most **4 Labs per farm are augmented**; other Labs still work without one. Assigning a Familiar **is** the lateral specialization, so three buildings give the variety of more.
- **Without the mod installed:** Labs simply disappear (doc 09). Because no conversion to Sheds is needed, Lab sizes are not tied to the Shed sizes; matching them stays a gameplay choice for the Lab pass.
- **Charles: design after the discovery/recipe flow is partly done.**

## Capstones (approved direction; design deferred)
Multiple capstones in different areas (farming, combat, exploration, economy, automation), each with tradeoffs. Candidate hosts: Ivo (combat), Hesper (exploration), Dr. Sorenson (economy), Tobin (automation), Oriel and Cyprian (the Philosopher's Stone). Seeds are in doc 03a.
