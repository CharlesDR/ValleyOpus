# 05 · Skill & Professions

Status: **DRAFT v0.4** (2026-10-13). v0.4: skill curve decided (doc 00, P4). v0.3 removes attunement from the perks. Perk numbers are placeholders. The two branches mirror Tobin (engineered lines) and Oriel (grand rituals); see doc 04.

## Skill
- A SpaceCore custom skill, working name **Alchemy** (an overlapping display name is acceptable; the internal ID is unique: `CharlesDR.ValleyOpus_Alchemy`).
- **XP from:** first-time discoveries (big), collecting apparatus output (small), triggering rituals (medium), assaying new items (small), failed experiments (tiny, so failure still progresses).
- Contributes to vanilla Mastery XP where SpaceCore allows; no prestige in 1.0.
- **Curve (Charles, P4):** vanilla speed, so level 10 arrives around late Year 1. Level 10 does **not** mean everything is discovered: recipe discovery, the codex, workshop restoration and NPC hearts are separate progression tracks that keep going after the XP bar is full.

## Profession tree (proposal)
Level 5 picks the axis; level 10 offers a deepening perk or a **bridge** perk that rewards mixing.

| L5 | L10 option A (deepen) | L10 option B (bridge) |
|---|---|---|
| **Mechanist**: ⚙︎ apparatus +25% speed; Boilers power +1 neighbour | **Engineer**: ⚙︎ quality cap raised to gold; Automate groups with ≥4 ⚙︎ machines get −10% residue | **Artificer**: minor arcane apparatus also get the Mechanist speed bonus; altars staged by Automate give +1 quality step chance when triggered |
| **Mystic**: rituals cost one less catalyst; triggering an altar also triggers other staged altars within 3 tiles; grand-ritual batches +10% size | **Hermeticist**: ✦ apparatus +1 quality step on the planet's day or in matching weather | **Theurgist**: hand-loading a ✦ machine with ⚙︎-made inputs gives a chance at a bonus output |

## Combination-safety rules (required by Walk of Life resets)
A player may end up owning **all six** professions. To make that safe:
1. **Perks only add, never forbid.** No perk removes a capability or blocks a machine. Caps and automation resistance are **properties of apparatus**, not of professions, so owning Mechanist never makes Mystic content worse.
2. **Each perk touches a distinct variable** (speed, quality cap, trigger radius, catalyst cost, residue, bonus chance). Where two perks touch the same variable, they combine by a documented rule (additive, with a clamp).
3. **All-six test:** a test fixture grants every profession and checks the economy caps in doc 06 still hold, since the 150% ceiling must survive full stacking.
4. **No WoL dependency.** Everything works without WoL. With WoL, resets simply let players collect more perks.

## Deferred to 2.0
Prestige / "Ultimate Mastery" perks (SpaceCore custom skills can't be prestiged through WoL today).
