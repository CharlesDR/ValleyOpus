# 07 · Discovery & the Codex

Status: **DRAFT v0.5** (2026-10-13). v0.5: Experiment Table input counts (doc 00, R2). v0.4 applies Charles's verdicts on the discovery research ([research/inspiration-discovery-moments.md](../research/inspiration-discovery-moments.md)).

## Principle (approved)
**Learning and unlocking through experimentation.** The game invites players to try things and celebrates every new discovery with a pleasing animation, popup or toast.

## Ways to discover
1. **Experiment Table:** place two items or substances, or three after a workshop unlock (Charles, R2). The recipe model allows any input count, so a four-ingredient tier can be added later without rework. A defined combination reveals and records the result. An undefined one yields Caput Mortuum, marks the pair "tried" in the codex so the player never repeats it unknowingly, and gives a tiny bit of XP.
2. **Assaying:** use the **Assayer's Lens** on any item to reveal its Prime profile (first time only), and on places or objects (ley nodes, the Witch's Hut, the Wizard's tower, the workshop's ruined fixtures) to reveal lore and recipe hints. This is the direct analog of Wizardry's Analyze.
5. **Research at the workshop:** restoring workshop rooms unlocks blueprints; NPCs working there (Oriel, Tobin, the Curator, Hesper on visiting days) offer research hints.
3. **Apparatus behaviour:** the first time a machine produces something new, or an adjacency effect fires, it's recorded, but **only when the player unloads it by hand** (see Discovery gates automation).
4. **Conditions:** running a machine under new conditions (a planetary day, storm, night) can reveal variant outputs.

## Discovery gates automation (Charles, v0.4)
Automation **cannot make discoveries, and requires them in order to function.**
- A machine accepts inputs (from the player or Automate) only for recipes the farm has already discovered, except for the manual first attempt itself.
- If a machine produces something undiscovered (a condition variant on a stormy day, a new adjacency effect), the output is **locked in the machine**: Automate and other extractors can't take it. The machine shows a distinct "something new inside" sparkle. When the player unloads it by hand, the discovery plays and the recipe unlocks for automation.
- Discovering the item another manual way (Experiment Table, assaying) also unlocks it.
- Inspired by Kingdom Come: Deliverance II, where you can only auto-brew potions you've brewed by hand.
- Implementation (to verify): the Automate factory for our machines withholds output while the item is undiscovered, and input acceptance checks the codex.

## Celebration (approved requirement; tiers approved v0.4)
Three tiers, from small to large:
- **Toast** for every new entry: icon, name and one line of flavour. Several at once collapse into one "3 new discoveries" toast.
- **Hold-up pose + sound** for firsts that matter (first Tier 2, first dark twin, capstone items), using vanilla's hold-up-item pose.
- **Codex flourish** for milestones (a Tier 1 row complete, first Tier 2, first dark twin): a short page-turn animation with a sound. The first discovery of each Tier opens the codex to its page once (skippable).
- **Anticipation beat:** a 1–2 second simmer/glow on the Experiment Table before the result.
- **Clock freeze:** every mandatory animation above pauses the in-game clock so it never eats daylight; optional ones can be skipped with one press. In multiplayer the clock is shared and can't pause for one player (to verify), so there these play as non-blocking overlays or stay under ~1 second.
- **Feedback styles:** arcane apparatus get glow and shimmer, engineered ones a crisp mechanical clunk; **late-game machines blend the two** to show the disciplines approaching harmony (doc 04).
- **Cauda Pavonis** iridescent shimmer for the first time an item passes nigredo → albedo.
- Settings in GMCM: toast frequency (all / milestones only) for players who find it noisy.

## Hints (approachability pillar)
A layered hint system, from subtle to explicit:
1. Item flavour text ("It smells faintly of brimstone…").
2. The codex grid shows silhouettes and "warmer" hints for pairs adjacent to known ones.
3. **Residue divination (Charles, v0.4):** spend Caput Mortuum to divine a hint (cost may scale with how far the target is from what's known). Every pile of residue is a choice: fertilizer, a tiny sale, saving it for late recipes that turn residue into valuable materials, or hints. Working things out yourself stays optimal; savings get you unstuck.
4. NPC channels, one per owner: **Oriel** (ritual and concept advice), **Tobin** (machine and layout tips), **Hesper** (the weekly almanac letter: planetary days, weather, celestial events), **Finch** (curiosity items), **Lark** (gentle comic attempts that point at untried pairs), **the Curator** (workshop lore).
5. In-world readables in the derelict workshop under the CC (Cyprian's journals, half-burned notes, the wordless Silent Pages).

Hints never cost real money and are never gated outside the game.

## Failure state
Gentle: residue (low value, minor uses: fertilizer, glaze, ballast, hint divination, and later Prima Materia), the pair marked tried, and a little XP. No explosions that destroy machines. Comic "puffs" are fine.

## Codex depth (approved v0.4)
Entries gain a lore line at 1, 10 and 50 crafted, with a progress pip toward the next. Automated output counts after the manual first discovery, which gives players who automate a reason to come back to the codex.

## The farm's secret recipe (approved v0.4)
- One or two late-game **secret recipes are seeded per farm** from a candidate pool, so guides can't spoil them. They're revealed through in-world readables and Hesper's almanac, which hints early at *which* secret this farm holds.
- **Re-roll:** an expensive, time-consuming and difficult way to re-seed it (e.g. a multi-day grand ritual needing Quintessence and rare catalysts). Restarting a Stardew save is a huge undertaking, so players aiming for a specific late recipe must never be stuck without it.
- Multiplayer-safe: stored with the farm's shared codex.

## The shared farm codex (approved)
- **One codex per farm; every discovery is shared** across all players.
- Stored in host save data and synced to farmhands through SMAPI multiplayer messages. A farmhand's discovery is sent to the host, recorded, then broadcast.
- Toasts show for the discovering player; others get a quieter "Alex discovered Steam" notice.
