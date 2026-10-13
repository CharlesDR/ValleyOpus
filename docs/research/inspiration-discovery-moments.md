# ValleyOpus Inspiration: Discovery Moments

Research thread, 2026-10-13. **Revision 2** (Charles's review, 18:10). Feeds [07 Discovery & Codex](../design/07-discovery-and-codex.md). Mechanics comparators (Zaikin, Potion Craft, Wizardry) are already covered in [comparators-and-concepts.md](comparators-and-concepts.md); this file is about how a discovery *feels*.

**Naming rule (Charles):** the games below are inspiration only; no names from them or from real people go into the mod.

Verification key: **(v)** checked against a source this session; **(bk)** background knowledge, not re-checked.

---

## 1. Reference moments

| Game | The moment | What makes it land | Steal for ValleyOpus |
|---|---|---|---|
| **Zelda: Breath of the Wild** cooking (bk) | Toss ingredients in a pot, a short bubbling/jingle sequence, then Link holds up the dish. Unknown combos yield *Dubious Food*, which still restores a little health. A dish is recorded on first cook. | A tiny ritual before the reveal builds anticipation; failure is funny and still useful. | A 1–2 second "simmer" anticipation beat before the reveal (clock frozen while it plays); Caput Mortuum as our Dubious Food, with a comic name and a small use. |
| **Return of the Obra Dinn** (v) | Fates are confirmed only in **batches of three**, which pulls you out and stamps them into the ledger with a musical sting. | Delayed, batched confirmation makes each stamp a mini-event and stops brute-force guessing per item. | Milestone flourish when a codex *row* (or three new entries) completes, rather than a big fanfare on every toast. Matches doc 07's "toast for each, flourish for milestones" split. |
| **Outer Wilds** ship log (bk) | Knowledge, not items, is progress. The log's "rumor mode" draws threads between clues, with markers for "there's more to explore here". | The map of what you *don't* know yet is the motivator. | Codex grid shows threads from known substances to unknown silhouettes; a small "more to learn" mark on entries with undiscovered combinations. This is the visual form of the "warmer" hint layer. |
| **Strange Horticulture** (v) | Identify plants with clues and a botany book; correct guesses get labelled on the shelf. Wrong guesses cost little. The reviewer enjoyed *writing labels* and felt "like a detective". | Physical labelling and tidying is satisfying on its own; mistakes sting only mildly. | When the Assayer's Lens reveals a Prime profile, show a small handwritten label on the item tooltip ("Assayed: Ignis 2, Terra 1"). Consider a player-editable note field per codex entry. |
| **Little Alchemy 2** (v) | A new element pops in with a sparkle; hints reveal *what* you could make from known items, not *how*. | Hints that name the goal and leave the recipe to you preserve the discovery. | Oriel's and Hesper's hints should name a target ("something that burns cold") and let the player find the pair. Never sell hints. |
| **Noita** (v) | Two secret alchemy recipes (Lively Concoction, Alchemic Precursor) are **randomized per world seed**, three liquids each. | Every save has a secret that guides can't spoil. | **Approved with tweaks.** One or two late "secret" recipes seeded per farm, revealed through in-world readables. Multiplayer-safe because the codex is per farm. **Re-roll required:** an expensive, time-consuming, difficult way to re-seed the farm's secret, so a player aiming for a specific late recipe is never stuck with a save that lacks it (see §2). |
| **Hades** codex (bk) | Each entry fills in gradually with more encounters; a progress pip shows how close the next lore line is. | Repeat use is rewarded, not just first contact. | Codex entries gain a lore line at 1, 10 and 50 crafted. Gives players who automate a reason to read the codex after the first discovery. Automated output counts toward the 10 and 50, but only after the manual first discovery. |
| **Pokémon** Pokédex (bk) | "Registered to the Pokédex" with a jingle, and a page that opens to the new entry. | The codex *opens itself* to the new page once, so you see where it lives. | First discovery of each Tier opens the codex to that page once (skippable), then later toasts stay small. |
| **Minecraft** advancements (bk) | Corner toast with icon, title and colour by rarity (task / goal / challenge). | Glanceable, non-blocking, tiered by importance. | Toast frame colour by tier or by colour stage (nigredo → rubedo). |
| **Stardew Valley itself** (bk) | Farmer holds an item overhead with a sound for special finds; skill level-up screen; museum donations with Gunther's reaction; Lost Books "you found a lost book" note. | Already familiar to players and fits the art style. | Use vanilla's hold-up-item pose for *firsts that matter* (first Tier 2, first dark twin, capstone items), and the HUD toast for the rest. Fits the tone better than a custom UI. |

## 2. Rules and cautions (revised with Charles's feedback)

- **Freeze the clock during mandatory animations (Charles).** Any animation or sequence that stops the player acting (the anticipation beat, hold-up pose, codex flourish, ritual triggers) must pause the in-game clock so it never eats daylight. Optional or skippable flourishes should be skippable with one press. Multiplayer caveat (bk, verify): the Stardew clock is shared and doesn't pause for one player's menus, so in multiplayer these sequences must either run as a menu-style overlay that doesn't hold up others, or be kept under ~1 second and skippable.
- **Automation never makes discoveries (Charles).** Automation *requires* discoveries in order to work, and cannot make them:
  - A machine can only be configured for (or will only accept inputs for) a recipe the farm has already discovered by hand. This is the Kingdom Come "brew it by hand once" rule.
  - If a machine nevertheless produces something undiscovered (e.g. a condition variant on a stormy day, or a new adjacency effect), the output is **locked in the machine**: Automate and chests cannot extract it. The player must unload it by hand, which plays the discovery, after which the recipe is unlocked for automation. Discovering the item another manual way (Experiment Table, assaying) also unlocks it.
  - A locked machine shows a distinct "something new inside" sparkle so the player knows to go look.
  - This replaces the earlier "While you were away" idea, which Charles rejected.
- **Toast fatigue.** Doc 07 already plans a GMCM setting. If several discoveries fire at once, collapse them into one "3 new discoveries" toast.
- **Residue as the price of a hint (Charles).** Caput Mortuum, the failed-experiment residue, can be spent to **divine a hint**. The player then has a real choice for every pile of residue: (a) inefficient fertilizer, (b) sell for a pittance, (c) save it for late-game recipes that turn residue into valuable materials, or (d) spend it on hints. Working things out yourself stays optimal; spending your savings gets you unstuck. Hint cost could scale with how far the target is from what the player already knows.
- **Secret recipe re-roll (Charles).** An expensive, time-consuming and difficult way to re-seed the farm's secret recipe (e.g. a multi-day grand ritual needing Quintessence and rare catalysts). Restarting a Stardew save is a huge undertaking, so knowledgeable players must not be punished for aiming at a specific late recipe. Optionally Hesper's almanac can hint early at *which* secret the farm holds, so nobody invests blindly.
- **Wonder at conjunction points** (the Alchemy Factory lesson): arcane apparatus get the richer effects (glow, shimmer, Cauda Pavonis), mechanical ones a crisp mechanical "clunk". **Late-game machines blend the two** (Charles): clockwork that shimmers, a ritual altar with gauges, symbolising the disciplines approaching harmony.
- **Don't monetize or gate hints**, unlike Little Alchemy 2. Residue-for-hints is an in-game trade-off, not a gate.

## 3. Stardew implementation notes

- `Game1.addHUDMessage(new HUDMessage(...))` has constructors for icon types (achievement, quest, error, stamina, health), custom colour/fade, and item-subject messages (v, wiki modding page). A custom toast with an item icon and flavour line likely needs our own small HUD drawer via SMAPI's `RenderedHud` event.
- `TemporaryAnimatedSprite` added to `location.temporarySprites` for sparkles and puffs; `location.playSound("…")` for cues (v).
- Farmer hold-up-item pose: vanilla has `Farmer.holdUpItemThenMessage(...)` (bk, verify name and signature against 1.6.15 decompile before using).
- Codex "opens to page" and page-turn flourish: custom `IClickableMenu`.

## 4. Suggested changes to doc 07: Charles's verdicts (2026-10-13)

1. Three celebration tiers: toast (every entry), hold-up pose + sound (meaningful firsts), codex flourish (row or tier complete). **Approved.**
2. An anticipation beat (1–2s simmer/glow) on the Experiment Table before the result. **Approved, with the clock frozen during it.**
3. ~~Discoveries made by automation are queued and replayed in a "While you were away" summary.~~ **Rejected:** automation cannot make discoveries and requires them in order to function (§2).
4. Codex entries deepen with use (lore at 1/10/50 crafted). **Approved.**
5. Per-farm seeded secret recipe for the endgame. **Approved, with an expensive re-roll.**
6. Arcane vs mechanical feedback styles differ on purpose. **Approved; late-game machines blend the two.**

Applied to the design docs on 2026-10-13 (docs 01, 04 and 07).

## Sources

- [Film Stories: Exploring Return of the Obra Dinn's rule of three](https://filmstories.co.uk/?p=83249)
- [PC Gamer: Strange Horticulture review](https://pcgamer.com/strange-horticulture-review)
- [Little Alchemy 2 Help Center: Using hints](https://help.littlealchemy2.com/hints/using-hints)
- [Noita Wiki: Lively Concoction](https://noita.wiki.gg/wiki/Lively_Concoction) and [Alchemic Precursor](https://noita.wiki.gg/wiki/Alchemic_Precursor)
- [Stardew Valley Wiki (zh): Modding common methods](https://zh.stardewvalleywiki.com/%E6%A8%A1%E7%BB%84:%E5%B8%B8%E7%94%A8%E6%96%B9%E6%B3%95) (HUDMessage, TemporaryAnimatedSprite, playSound)
- [Zelda Wiki: Cooking in Breath of the Wild](https://zeldawiki.wiki/wiki/Cooking_in_Breath_of_the_Wild) (page could not be fetched this session; BotW details are background knowledge)
