# ValleyOpus Design Docs

An alchemy and automation expansion for Stardew Valley. These are **drafts** (started 2026-10-13, last swept 2026-10-18); each doc's status line gives its version and readiness. This repo is their source of truth. **Every open question is collected in [doc 10](10-roadmap-and-open-questions.md#open-questions).**

Each subject has one owning doc; other docs summarize and link to it. Where an older summary and the owning doc differ, the owning doc wins. Doc numbers 16, 19, 20 and 21 are reserved on the path in doc 00.

| Doc | Topic |
|---|---|
| [00 Critical Path](00-critical-path.md) | doc template, gaps, the design path to architecture, and open decisions in order |
| [01 Vision & Pillars](01-vision-and-pillars.md) | vision, design and player-experience pillars, balance rules, compatibility stance |
| [02 Progression & Unlocks](02-progression-and-unlocks.md) | soft start, pacing targets, the Year 1 curve, the finished state, capstone direction |
| [03 Essences & Decomposition](03-essences-and-decomposition.md) | the eight Primes, decomposition model, source-diversity rule |
| [03a Substances Catalog](03a-substances-catalog.md) | the 36-cell Tier 1 grid, dark twins, Tier 2/3, colour stages, residue |
| [03b Item Budget & Inventory](03b-item-budget-and-inventory.md) | few adventure-area drops (the only item restraint), substance classes, Reagent Satchel, item census |
| [04 Machines, Rituals & Automation](04-machines-and-automation.md) | the philosophy: engineered lines vs grand rituals, automation classes, staged rituals, ley nodes, Gloam Lanterns, chaining (the machine list is doc 13) |
| [05 Skill & Professions](05-skill-and-professions.md) | the Alchemy skill, Mechanist/Mystic tree, combination-safety rules |
| [06 Economy & Balance](06-economy-and-balance.md) | per-tile and per-ingredient metrics, the 150% cap, balance sweep |
| [07 Discovery & Codex](07-discovery-and-codex.md) | experimentation, celebrations, hints, shared codex |
| [08 Narrative & NPCs](08-narrative-and-npcs.md) | the derelict workshop, story arc, Wizard and Witch guidelines |
| [08a Cast](08a-cast.md) | the approved cast, with names and genders |
| [09 Technical Architecture](09-technical-architecture.md) | packaging, IDs, dependencies, key systems |
| [10 Roadmap & Open Questions](10-roadmap-and-open-questions.md) | 1.0 vs 2.0, Charles's action items, **every open question across the docs**, upcoming tasks |
| [11 First Playable Slice](11-first-playable-slice.md) | Spring Y1 end to end: the finish line, the first evening, what is in and out, the rows per asset, the technical risks the first build must surface |
| [12 Recipe & Process Model](12-recipe-and-process-model.md) | the keystone: Primes, substances, recipes, conditions, quality, residue, discovery keys, pack format |
| [13 Apparatus Catalog](13-apparatus-catalog.md) | the 14 machines, behaviour classes, power and heat, the resolver behind `OutputMethod`, chaining, the four-Lab check |
| [14 Unlock Map & Workshop Bundles](14-unlock-map-and-workshop-bundles.md) | what opens when: workshop rooms and bundles, NPC arrivals, base-game gates on both routes, Aether pacing |
| [15 Products & Outside Uses](15-products-and-outside-uses.md) | product families, the effect system, fertilizers, combat items, prices and buyers, gifts, Smaltum |
| [17 Multiplayer, Saves & Compatibility](17-multiplayer-saves-and-compatibility.md) | every piece of state and who owns it, multiplayer sync, Satchel storage, the save schema and migration, uninstall, map touchpoints, the test matrix |
| [18 Laboratory & Familiars](18-laboratory-and-familiars.md) | the three Lab tiers, costs and bonuses, nested floorplans with hearths and dark alcoves, the space check, the four Familiars and their bonding |
