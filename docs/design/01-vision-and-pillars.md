# 01 · Vision & Pillars

Status: **DRAFT v0.6** (2026-10-13). v0.6: Charles answered every guiding-area stub, and the other docs were revised to match (doc 10 lists the follow-up questions). v0.5: stub sections for guiding areas that still need Charles's direction (see "Open guiding areas"). v0.4: Lore & Worldbuilding pillar, the guiding star, the clock-freeze rule and the discovery gate on automation (Charles's feedback on the inspiration research). Sources: the startup context transcript, Charles's answers, and [research](../research/comparators-and-concepts.md).

## Vision
ValleyOpus brings **alchemy and automation** to Stardew Valley in a way that feels native to the valley: mechanical ingenuity and Hermetic magic are two different ways of working the world, and the interesting choices happen where they meet. It should feel like *wonder you can industrialize, a little, at a price*, never "a medieval sweatshop simulator" (the Alchemy Factory warning).

## Design pillars (agreed)
1. **Mechanical and magical are separate axes, not tiers.** Mechanical is reliable, bulk and automatable: Tobin's engineered lines. Arcane is conditional (day, weather, season, ritual), reaches higher quality or unique results, and resists full automation: Oriel's grand rituals and dramatic single batches.
2. **Everything decomposes.** Any item from any mod breaks down into the eight Primes by its context tags. That's both the fantasy and the compatibility engine.
3. **Sidegrades over upgrades.** New apparatus open different routes (Satisfactory alternates), not faster copies of old ones. Tradeoffs span several axes (Opus Magnum's cost, cycles and area).
4. **Discovery over shopping.** Players learn and unlock by **experimenting** (Zaikin, Wizardry's Analyze). Each discovery is celebrated with an animation, popup or toast, and recorded in a shared farm codex. **Automation never makes discoveries and requires them in order to function** (doc 07).

## Player-experience pillars (Charles; may grow)
1. **Reasons from outside the mod.** Outputs matter to the rest of the game: food, buffs, money, tools, fertilizer, combat aids.
2. **Don't invalidate other content.** Alchemy can earn money, but not eclipse kegs, jars and dehydrators (see doc 06 for the caps).
3. **Approachability.** Failure is gentle (residue, not ruin). Hints are sprinkled through flavour text, letters, NPCs and in-world objects to point at the next milestone. **Any mandatory animation that stops the player acting freezes the in-game clock**, so celebrations never eat daylight (doc 07 covers multiplayer).

## Lore & Worldbuilding pillar (Charles)
1. **No real-world names in the mod**: no real people, texts or places, and no names borrowed from other media (characters, creatures, items, coined terms). Real sources live in the research files as inspiration only; in-mod things get our own names. Generic alchemical terms (salamander, ouroboros, alembic, Caput Mortuum) are fine.
2. **References are flavour text only.** A nod to pop culture or a real historical figure (e.g. Flamel) may appear in item descriptions, codex lore or readables, never in in-game dialogue and never as an explicit character reference.
3. **Lore must feel native to the valley**: unattributed sayings (the Fourfold Saying), in-world texts (the Verdant Tablet, the Silent Pages) and the cast's own histories. Inspiration: [research/inspiration-lore-and-npcs.md](../research/inspiration-lore-and-npcs.md).

## Guiding star (Charles)
**Boost underwhelming base-game activities and items.** The model is flowers: without the mod, growing them in bulk is suboptimal (honey boosts, the odd gift), but flower-fed arcane apparatus (doc 04) make it worthwhile. Look for the same opportunity with other cheap, plentiful, under-used things (fiber, sap, common forage, trash, monster loot, moss) in the balance sweep.

## Inventory rule (Charles)
Inventory space is precious. Few new monster drops or mine finds, a budget of new carriable items, and a Reagent Satchel by default (doc 03b).

## Balance rules
- **No single-source bottlenecks.** Every Prime has at least 3 independent sources across activities (Fire must not mean "only spicy crops").
- **A mix of quick wins and long goals.** No sharp pain points.
- **Only moderate departures from typical Stardew priorities.** The mod rewards what players already do, but provides reasons to do things that would be underwhelming in the base game.

## Compatibility stance
- Tag-first inputs and flavored outputs, so most content mods work on day one.
- Append to vanilla data, never overwrite it.
- Must work without Walk of Life. Under WoL resets, every combination of our professions must coexist safely.
- Other Stardew alchemy mods (Alchemy of the Valley, Alchemistry, Alchemist's Valley): **distinct IDs, no planned compatibility**. Players choose one or the other. Overlapping display names are acceptable.
- Multiplayer-safe code, but a solo player can experience everything.

## Non-goals for 1.0
- Potion Craft-style brew map (planned for 2.0).
- Prestige / "Ultimate Mastery" perks (2.0).
- Arcane logistics (2.0); 1.0 has directional chaining instead (doc 04).
- Tool and weapon forging (2.0).
- Final art (placeholders until before the 1.0 publish).

## Guiding answers (Charles, 2026-10-13)
Charles's direction on the areas that were open stubs in v0.5. The other docs were revised against these in the v0.6 pass; follow-up questions are in doc 10.

### Tone and feel
- The tone of the mod can be dark in a "rated PG" sort of way. Dark alchemy should feel spooky and ominous but not grotesque; ghosts and skeletons are okay but blood and gore is not. It should be close to the Vanilla Stardew thematic tone (which includes ghosts, zombies, skeletons, etc).
- Humour should be mostly gentle whimsy with some characters occasionally getting sassy in cutscenes. Slapstick should be reserved for the characters it fits particularly well.
- Homunculi and living transmutation should only include plants as subjects. Immortality is on the table thematically for worldbuilding purposes (as an aspirational goal) but not for gameplay mechanics. Homunculi/transmutation of animals and people is explicitly forbidden, and the Wizard enforces the ban.
- After a big day of Alchemy, the player should feel accomplished, and frequently they should feel clever for having worked out a tricky solution.

### Target player and complexity ceiling
- Both cozy players and factory enthusiasts should enjoy this mod, but unlocking all of the secrets of Alchemy requires dedication and hard work. Diligent pursuit will be needed to unlock everything; automation is heavily encouraged after the first tier of recipes.
- Optimization-minded players will likely want a spreadsheet or wiki. High complexity is okay since we have many mechanisms for discovering clues about next steps.
- No upper bound on simultaneous conditions for now; the number of conditions to be met should naturally increase as the player advances through the mod. However, simultaneous condition tuning is a likely place we may need to balance around later on in development.

### Pacing and the daily loop
- Mid-game, Alchemy should take about 1/4 of your day if you are heavily invested in it.
- A diligent player chasing progress should be able to discover most things in Year 1 and everything by the end of Year 2, but it may require some insider knowledge from prior playthroughs to do so. Most players won't unlock everything until about year 4, but not because of any hard time-gating steps.
- Alchemy is a new activity and will naturally compete for the player's time against other content, but it shouldn't be intentionally taking up time for no reason.

### Automation scale and the Automate dependency
- Automate is expected but technically optional.
- A late game Alchemy setup producing all of the endgame items should spread through multiple top tier Lab buildings. One of each specialized T3 Lab building should be more than enough space for a setup for each item in the mod.
- Gifts, quest items, and capstones should always be handmade, either through crafting recipes or, preferably, through the Ritual mechanic.

### Endgame and capstones
- The last achievement should be a mixture of powerful items, story conclusions, and some aesthetic changes to the world.
- 1.0 will have a definitive "Finished" state, but we will design a long-tail without a defined endpoint for 2.0.
- Capstones should change the Valley and/or its residents in visible ways, but we need to maintain compatibility when designing how that works.

### Place in the world
- Vanilla and SVE NPCs should be fully integrated into Alchemy with dialogue, gift tastes, events, etc. Notable inclusions to have a unique perspective on Alchemy would be Gunther, Abigail, Maru, Emily, the Wizard, Witch (Alecto), Sofia, Krobus, the Dwarf, and Andy.
- The mod can add onto Vanilla and SVE lore, but in careful ways that don't contradict any established facts or undercut existing characterizations.
- Alchemy as a craft is a respected discipline, but the public knows that dark Alchemical magics were once a problem for public safety, so there is a bit of wariness when Alchemists start working on the more esoteric subjects.

### Combat and exploration
- Alchemy should provide useful consumables and passive effects, but no more impactful than the Trinkets added from Combat Mastery.
- Ginger Island, the Volcano, Skull Cavern and Mr. Qi's content is all fair game for Alchemy to tweak or draw from.
- Some intriguing new Alchemy-related areas would be nice to add, if we can find compatibility-friendly places to hide them.

### Extensibility and the modding ecosystem
- Other modders should be able to add Primes, substances, apparatus or recipes through Content Patcher in 1.0.
- Beyond SVE, players of the mods Alecto, RSV, and East Scarp should find some fun Alchemy tie-ins. Other mods to be added in 2.0.

### What success looks like
- 1.0 will be a success if it is technically polished, with satisfying gameplay loops and a fun experience.
- What we are calling 1.0 will serve as a public beta, but I expect to do a lot of polish before that happens.
- After 1.0 we will maintain the mod with game updates, bug fixes, new content, and implementing translations.
