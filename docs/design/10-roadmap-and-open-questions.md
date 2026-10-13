# 10 · Roadmap & Open Questions

Status: **DRAFT v0.6** (2026-10-13). v0.6: the docs pass against Charles's guiding answers in doc 01, with the questions it raised (below). The path to architecture and every decision on it are in [doc 00](00-critical-path.md).

## Version scope
| | 1.0 | 2.0 |
|---|---|---|
| Primes, decomposition, Tier 1–3 substances | ✓ | expansions |
| Engineered and arcane apparatus, **adjacency**, **grand rituals**, staged altars (animation) | ✓ | **ritual mini-game** |
| Ley nodes and Ley Taps | ✓ | |
| Skill with L5/L10 professions (vanilla-speed curve) | ✓ | **prestige / Ultimate Mastery** perks |
| Laboratory, 3 tiers by interior size, Familiars augment T3 (max 4) | ✓ | more specializations |
| Derelict workshop, storyline, cast of 10 + Familiars | ✓ | more arcs |
| Shared codex and discovery celebrations | ✓ | |
| Content Patcher extensibility (Primes, substances, apparatus, recipes) | ✓ | |
| Vanilla and SVE villagers integrated (dialogue, gift tastes, events) | ✓ | |
| Mod tie-ins | SVE, Alecto the Witch, Ridgeside Village, East Scarp | others, by Nexus ranking |
| Plant transmutation (seeds, the Rootling) | ✓ (candidates) | |
| Ending | a definitive finished state (the Great Work) | an endless long tail |
| Reagent Satchel and the item budget | ✓ | maybe an Essence Reservoir |
| Directional chaining (West → East, South chests) | ✓ | |
| Arcane logistics | ✗ | ✓ |
| Tool and weapon forging (Orichalcum) | ✗ | ✓ |
| Potion Craft-style **brew map** | ✗ | ✓ (needs more design before it's scoped) |
| Commissioned art | before the 1.0 publish | |

**Release plan (Charles, doc 01):** what we call 1.0 is a **public beta**, released after a lot of polish. Success means technical polish, satisfying loops and a fun experience. After 1.0 the mod is maintained: game updates, bug fixes, new content and translations.

## Charles's action items
1. **Nexus popularity export** for integration targets (manual, since Nexus blocks cloud sessions). The 1.0 tie-ins are now chosen (SVE, Alecto the Witch, Ridgeside Village, East Scarp), so this only sets 2.0 priorities.
2. **Full review of the design docs** before the next stage (Phase 1, doc 12).
3. **Answer the questions from the v0.6 pass** (below).

## Questions from the v0.6 pass (waiting on Charles)
Raised while fleshing out docs 02–09 against doc 01. Each has the default the docs currently use; "default is fine" is a valid answer.

**Hand-made only and automation**
1. **Staging hand-made rituals.** Capstones are hand-made, preferably by ritual. May Automate still *stage* materials for those rituals if the player triggers them? *Default: no, the player loads them by hand too* (doc 04).
2. **What counts as a quest item?** *Default: story-quest deliveries only. Dr. Sorenson's bulk contracts, Ivo's bounties and workshop bundles accept automated output* (doc 04).
3. **Gifts.** *Default: products made mainly for gifting are hand-made only, but any alchemy item can still be given as a gift* (doc 03a).
4. **Lab sizing.** "One of each specialized T3 Lab" means four T3 Labs, one per Familiar, holding a setup for every item with room to spare. *Default: yes* (doc 02).

**Tone, lore and characters**
5. **Who fits slapstick?** *Default: Finch's misfiring curios and Tobin's machine hiccups, with Lark kept restrained as decided; Oriel, Hesper and Cyprian stay dry* (doc 08).
6. **The dark-alchemy past.** Should the public-safety trouble be a named historical incident with readables? *Default: yes, a fictional incident elsewhere and decades ago, not the cause of the workshop's ruin, so Cyprian stays sympathetic* (doc 08).
7. **The Witch without Alecto.** *Default: without the Alecto the Witch mod she stays the unnamed vanilla Witch; with it, our Witch content runs through Alecto* (doc 08).
8. **Sofia and Andy.** I read "Sofia" as SVE's Sophia. *Default: yes; their alchemy angles get chosen after a check of their SVE characterizations* (doc 08).
9. **Villager angles.** Do the proposed angles for Gunther, Abigail, Maru, Emily, Krobus and the Dwarf match your intent? *Default: yes* (doc 08).

**Plants, combat and the world**
10. **Plant transmutation.** Approve **seed transmutation** (a ritual that turns one crop's seeds into another's) and the **Rootling** (a small plant homunculus that tends a crop patch) as candidates? *Default: yes, both as candidates* (doc 03a). The v0.2 "Rust Slimes" residue idea is dropped under the ban.
11. **Passive combat effects.** How should they reach the player? *Default: long-lasting buffs from consumables plus at most a couple of rings; no trinket-slot items* (doc 03a).
12. **New drops under "few".** *Default: none chosen until a late area (Skull Cavern, Volcano, a hidden area) clearly needs a signature reagent* (doc 03b).
13. **Forage in hidden areas.** *Default: no new forage; hidden areas host rare ley nodes and lore instead* (docs 03b, 04).
14. **Late-game gates.** Approve the proposed hooks: Skull Cavern bounties and Shade hunting, island and Volcano ley nodes, a Volcano ritual site, and a few Mr. Qi special orders paid in Qi Gems? *Default: yes, as proposals* (doc 02).

**Pacing, ending and scope**
15. **Escalating conditions.** *Default: Tier 1 recipes need none or one condition, Tier 2 one or two, Tier 3 and capstones two or more* (doc 02).
16. **The finished state.** *Default: completing the Great Work ends the 1.0 story; Cyprian finally rests, and the valley changes visibly* (docs 02, 08).
17. **1.0 scope check.** 1.0 has grown: full villager integration, three more tie-in mods and Content Patcher extensibility. *Default: keep all of it in 1.0, written after the architecture gate*; the alternative is to move some villager dialogue to a 1.x update (doc 10).
18. **Version label.** *Default: publish the public beta as 1.0.0 with "beta" in the Nexus description* (doc 10).

## Waiting on Charles (earlier)
- Part 4 decisions: all answered and confirmed 2026-10-13 (doc 00).
- ~~Soul-stone name~~: decided, **Gloam Lantern** collecting **Shades** (doc 04).

Earlier items, all decided as of 2026-10-13 17:39, and the defaults (mod ID, 36-cell grid, dark twins, perks-only-add, residue → Prima Materia) are confirmed.
1. ~~Cast names~~: approved; the apprentice is Lark.
2. ~~Inventory approach~~: decided, Reagent Satchel (doc 03b).
3. ~~Arcane automation default~~: decided. Minor arcane apparatus are fully automatable; ritual altars are staged by Automate and triggered by hand (doc 04).
4. ~~Ritual mini-game~~: decided. 1.0 is animation only; the mini-game moves to 2.0.

## First playable slice (S1)
Spring Y1 end to end: the intro cutscene, the first workshop room, decomposition, the Experiment Table, 4–6 apparatus (engineered and minor arcane), the codex and the Satchel. Specced in doc 11.

## Deferred design passes (agreed to come later)
- Opening cutscene spec (doc 02).
- Laboratory detail (direction set: T1 = Shed size, T2 = Big Shed size, T3 larger; escalating apparatus bonuses; Familiar assignment at T3 is the specialization), after the first slice.
- Capstones, once progression is coherent.
- Skill display name (working name: Alchemy).
- Lark's family (Jodi's or Pam's) and Hesper's outdoor spot.

## Upcoming tasks
- Balance sweep tooling (doc 06), including the item census (doc 03b).
- Nexus popularity ranking for integration targets. This needs a manual export or the browser, because Nexus blocks automated access from cloud sessions.
- Lore check: read the vanilla and SVE wiki entries for every villager with a planned alchemy angle (doc 08), and the Alecto the Witch, Ridgeside Village and East Scarp pages for tie-in spots and map edits to avoid.
- Verify the CC-opening event ID, the Quarry bridge and bus repair flags (both CC and Joja routes), and the SVE Town map behind the CC.

## Change log
- **v0.6 (2026-10-13):** a pass over docs 02–09 against Charles's guiding answers in doc 01: pacing targets and no calendar gates, escalating conditions, the 1.0 finished state, hand-made-only gifts, quest items and capstones, Automate expected but optional, late-game Lab sizing, the combat cap, plants-only living transmutation and the Wizard's ban, tone and reputation, full villager integration, 1.0 tie-ins with Alecto the Witch, Ridgeside Village and East Scarp, and Content Patcher extensibility. 18 questions for Charles.
- **v0.5 (2026-10-13):** Charles's Part 4 decisions (doc 00): Spring Y1 slice, directional chaining in 1.0 with arcane logistics moved to 2.0, i18n, Golem Clay dropped, three Experiment Table inputs, vanilla-speed skill, economy knobs in GMCM, SVE-only integration, Lab tiers by size with Familiars at T3. The Charles action items list was added.
- **v0.4 (2026-10-13):** from Charles's review of the inspiration research: Lore & Worldbuilding pillar and the "boost underwhelming base-game activities" guiding star (doc 01); automation can't make discoveries and undiscovered outputs lock in the machine (docs 04, 07); three celebration tiers, clock freeze, residue hints, codex depth and the per-farm secret recipe with re-roll (doc 07); flower-fed arcane apparatus, Gloam Lanterns and late-game blended machines (doc 04).
- **v0.3 (2026-10-13):** automation model: minor arcane apparatus are fully automatable, ritual altars are staged by Automate and triggered by hand, and attunement decay is removed; profession perks revised to match.
- **v0.2 (2026-10-13):** cast approved and renamed (6F/5M, random assignment); the workshop is the hub under the CC; Oriel/Tobin roles revised; grand rituals, ley nodes, Ley Taps, Ivo/Agnes/Bennet gates and the item budget added; #3 merged into Hesper.
- **v0.1 (2026-10-13):** first draft.
