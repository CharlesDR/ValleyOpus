# 03a · Substances Catalog (Combinatorics)

Status: **DRAFT v0.6** (2026-10-14). v0.6: **naming rule, Latin first** (Charles): every material's display name is its Latin or classical form, with an English gloss second; names swept across all docs. v0.5: seed transmutation and the Radicula approved as candidates; trinket-slot items added (Charles). v0.4: the plants-only rule for living transmutation, plant transmutation candidates, hand-made-only products, the combat-strength cap and the immortality rule (Charles's guiding answers, doc 01). v0.3: Golem Clay dropped (doc 00, R5); colour stages confirmed as vanilla quality (R1). This is a candidate list. Names, recipes and tiers are proposals for review, not commitments.
Parent doc: [03-essences-and-decomposition.md](03-essences-and-decomposition.md) · **Inventory rule:** not every substance below is a carriable item. Many are power, process states or in-machine intermediates. See [03b-item-budget-and-inventory.md](03b-item-budget-and-inventory.md).

## Naming rule: Latin first (Charles, 2026-10-14)
- **Every alchemical material** (Primes, substances, compounds, products, metals, residue) is named **Latin or classical first, English gloss second**: **Ignis** (Fire), **Vapor** (Steam), **Sal Marinum** (Brine). The gloss is never another Latin word.
- In play, the Latin name is the item's display name, and the English gloss appears as its subtitle in tooltips and the codex, so cozy players are never lost. Translations translate the gloss, not the Latin.
- A name that already *is* the classical term with no useful gloss stands alone (Calx is glossed, Magma, Electrum, Rebis, Azoth and Elixir aren't).
- Families stay consistent: Sal Fixum, Sal Marinum and Sal Volatile all build on **Sal**; Mercurius Fixus, Mercurius Currens and Mercurius Philosophorum on **Mercurius**; Flamma Viva, Terra Viva and Sulphur Vivum share *vivus*, "living".
- **Out of scope, deliberately:** machines and apparatus (their own scheme: classical vessels like Athanor and Alembic, plain English for Tobin's engineered machines), tools and the Gloam Lantern's Shades (named by Charles), NPCs and places. The research files keep their original names as inspiration.
- **Renamed in the sweep:** the eight Primes (Ignis, Aqua, Terra, Aer, Sal, Sulphur, Mercurius, Aether), Flamma Viva, Ros Maialis, Sal Fixum, Sulphur Vivum, Mercurius Currens, Quinta Essentia, Vapor, Limus, Pulvis, Sal Marinum, Mercurius Fixus, Cinnabaris, Mercurius Philosophorum, Pulvis Pyrius, Cinis Phoenicis, Lapis Philosophorum, Radicula (the Rootling), Argentum, Stannum and Plumbum (the planetary metals). Glosses fixed to English: Sol (Sunlight), Spiritus (Spirit), Miasma (Smoke), Aether (the Fifth Element).
- **Aura (Ether):** the energy restorative was "Ether", which nearly collided with the Prime **Aether**. It's renamed **Aura** (Latin for breeze or breath) under the consistency principle. Flagged for Charles to confirm.

Sourcing rule (Charles): classical alchemy lore plus well-known or moderately well-known media, **nothing more obscure than Thaumcraft** unless it's an unusually good fit. Each entry notes its inspiration:
**[C]** classical alchemy · **[TC]** Thaumcraft · **[W3]** The Witcher 3 alchemy substances · **[OM]** Opus Magnum · **[FMA]** Fullmetal Alchemist · **[FF]** Final Fantasy · **[LA]** Zaikin's Alchemy / Little Alchemy / Doodle God · **[AT]** Atelier series · **[SDV]** an existing Stardew item.

---

## Tier 0: the eight Primes (approved)

| Prime | Group | Gist | Typical sources (by context tag, see doc 03) |
|---|---|---|---|
| **Ignis** (Fire) | Element | heat, transformation | hot/spicy crops, coal, fire quartz, magma, cooked dishes, summer forage |
| **Aqua** (Water) | Element | flow, dissolution | fish, rain-harvested goods, juices, seaweed, spring forage |
| **Terra** (Earth) | Element | body, structure | roots, minerals, stone, clay, geodes, mushrooms |
| **Aer** (Air) | Element | breath, motion | flowers, feathers, wool, honey, seeds, fall forage |
| **Sal** (Salt) | Principle (tria prima) | the body: fixity, preservation | minerals, sea products, cheese, pickles, bones |
| **Sulphur** (Sulfur) | Principle | the soul: combustibility, colour, flavour | oils, spices, eggs, sap, mushrooms, monster loot |
| **Mercurius** (Mercury) | Principle | the spirit: volatility, change | wine and spirits, slime, quicksilver-bearing ores, fish roe |
| **Aether** (the Fifth Element) | Rare fifth | the heavens | prismatic shard, solar/void essence, legendary fish, **ley nodes** (Ley Tap), grand rituals |

Design note: no Prime may depend on a single item family (balance rule). Each needs at least 3 independent sources spread across farming, foraging, fishing, mining and animals. Doc 03 holds the sweep plan.

---

## Tier 1: all 36 pairings of the Primes

Every unordered pair of Primes, including a Prime with itself, yields one Tier 1 substance. That gives a complete, Zaikin-style grid the player can fill in by experimenting. Self-pairs are *purified* forms.

### Self-pairs (8)
| Recipe | Substance | Inspiration | Gameplay hook |
|---|---|---|---|
| Ignis + Ignis | **Flamma Viva** (Living Flame) | [C] Paracelsus's salamander | *power:* a lamp fuel state for arcane apparatus; never goes out |
| Aqua + Aqua | **Ros Maialis** (May Dew) | [C] alchemists' dew, collected at dawn | harvested on rainy mornings; base solvent for tinctures |
| Terra + Terra | **Saxum** (Living Stone) | [TC] | construction material for apparatus and Lab upgrades |
| Aer + Aer | **Pneuma** (Breath) | [C] | aeration; speeds Aer-aligned processes |
| Sal + Sal | **Sal Fixum** (Fixed Salt) | [C] | stabilizer; prevents a brew's quality from dropping |
| Sulphur + Sulphur | **Sulphur Vivum** (Brimstone) | [C] | strong combustible; volatile catalyst |
| Mercurius + Mercurius | **Mercurius Currens** (Quicksilver) | [C][OM] | the classic projection agent; Wednesday metal |
| Aether + Aether | **Quinta Essentia** (Quintessence) | [C] | the "fifth essence"; very rare, used in capstones |

### Element × Element (6)
| Recipe | Substance | Inspiration | Gameplay hook |
|---|---|---|---|
| Ignis + Aqua | **Vapor** (Steam) | [LA][C] | *power:* drives Tobin's engineered machines through Boiler adjacency |
| Ignis + Terra | **Magma** | [LA][SDV Magma Cap] | heat source; volcanic forging |
| Ignis + Aer | **Fulgur** (Lightning) | [W3][SDV lightning rod / Battery Pack] | *power:* captured by a Lightning Coil; overclocks neighbours |
| Aqua + Terra | **Limus** (Silt) | [LA mud][TC Limus][SDV Clay] | fertility; base of fertilizers |
| Aqua + Aer | **Tempestas** (Weather) | [TC] | weather rituals; links to rain totems |
| Terra + Aer | **Pulvis** (Dust) | [LA] | sand → glassware for alembics |

### Element × Principle (12)
| Recipe | Substance | Inspiration | Gameplay hook |
|---|---|---|---|
| Ignis + Sal | **Calx** (Ash) | [C] product of calcination | potash, glaze, cheap fertilizer additive |
| Ignis + Sulphur | **Quebrith** (Kindling Spirit) | [W3] | explosives and bombs (combat and mining; taught by Ivo) |
| Ignis + Mercurius | **Phlogiston** | [C] (18th-century chemistry) | "fire that moves": heat transport for arcane logistics |
| Aqua + Sal | **Sal Marinum** (Brine) | [C] | preserving; ocean-fish tie-in (taught by Delta) |
| Aqua + Sulphur | **Aqua Fortis** ("strong water") | [C] | acid solvent; Dissolution stage |
| Aqua + Mercurius | **Hydragenum** | [W3] (Hg comes from *hydrargyrum*, "water-silver") | fluidity; slows or speeds processing (Delta) |
| Terra + Sal | **Vitreus** (Crystal) | [TC] | gem growth; crystalarium-style apparatus |
| Terra + Sulphur | **Bitumen** (Earth-Fire) | [C] | coal analog; furnace fuel |
| Terra + Mercurius | **Metallum** (Metal Seed) | [TC][C] metals "grow" from mercury in the earth | ore growing; metal transmutation |
| Aer + Sal | **Sal Volatile** (the White Eagle) | [C] sal ammoniac / smelling salts | restoratives: energy buffs |
| Aer + Sulphur | **Miasma** (Smoke) | [C] | smoke: pest control, bee calming, debuff bombs |
| Aer + Mercurius | **Motus** (Motion) | [TC] | speed buffs; logistics conduits |

### Element × Aether (4): the "bright" celestial substances
| Recipe | Substance | Inspiration | Gameplay hook |
|---|---|---|---|
| Ignis + Aether | **Sol** (Sunlight) | [W3][TC][SDV Solar Essence] | light; Sunday/gold |
| Aqua + Aether | **Luna** (Moonwater) | [C] | tides and night; Monday/silver (Delta and Hesper) |
| Terra + Aether | **Vitae** (Life) | [OM][TC Victus] | growth: crops, healing, animal care (care only, never transmutation); the base of plant transmutation |
| Aer + Aether | **Spiritus** (Spirit) | [TC][W3 Caelum] | spirit: ghosts, the Witch, Junimos |

### Principle × Principle (3)
| Recipe | Substance | Inspiration | Gameplay hook |
|---|---|---|---|
| Sal + Sulphur | **Vitriol** (the Green Lion) | [C][W3] | the great solvent of metals; "devours the sun" (dissolves gold) |
| Sal + Mercurius | **Mercurius Fixus** (Fixed Mercury) | [C] "fixing the volatile" | Coagulation stage; locks a quality tier in |
| Sulphur + Mercurius | **Cinnabaris** (Vermilion) | [C][W3] (cinnabar really is mercury sulfide) | red pigment and dye; rubedo catalyst |

### Principle × Aether (3)
| Recipe | Substance | Inspiration | Gameplay hook |
|---|---|---|---|
| Sal + Aether | **Ordo** (Order) | [TC] | structure; machine stability, anti-spoilage |
| Sulphur + Aether | **Anima** (Soul) | [C] (sulfur as the soul) | animation **of plants only** (plant homunculi); an offering when bonding Familiars, who are spirits befriended, not made |
| Mercurius + Aether | **Mercurius Philosophorum** (Philosophic Mercury) | [C] | key to Alkahest and Azoth |

---

## Polarity: the "dark twins"

Three of the bright Tier 1 substances have shadow counterparts. Players get them by **putrefying** the bright form (the nigredo process: fermentation in darkness, inside the Lab, or in the Witch's swamp) or directly from shadow sources (void essence, monster loot, the Witch's Hut).

| Bright | Dark twin | Inspiration | Notes |
|---|---|---|---|
| Sol (Lux) | **Tenebrae** (Void) | [TC][SDV Void Essence] | Witch/Krobus tie-in |
| Vitae | **Mors** (Death) | [OM][TC Mortuus] | decay; composting, weed-killing, combat debuffs |
| Ordo | **Perditio** (Entropy) | [TC] | breaks things down; ultra-efficient decomposition but adds residue |

This gives the Wizard (bright) and the Witch (dark) natural, complementary roles as teachers. See doc 08.

---

## Tier 2: notable compounds (selected, ~25)

Tier 2 is open-ended. These are the ones with clear gameplay purpose. "Twin routes" (the same output reachable by a mechanical or a magical path) are marked ⚙︎/✦, which illustrates design pillar 1.

| Compound | Recipe (proposal) | Inspiration | Purpose |
|---|---|---|---|
| **Vitrum** (Alchemical Glass) | Pulvis + Calx | [C] | glassware for alembics and retorts; Bennet's raw material for advanced shapes |
| **Pulvis Pyrius** (Black Powder) | Bitumen + Quebrith + Sal Volatile | [C][AT bombs] | bombs and mining charges (Ivo) |
| **Aqua Regia** | Aqua Fortis + Sal Marinum | [C] (it really does dissolve gold) | dissolves gold for transmutation |
| **Aqua Vitae** | Vitae + Ros Maialis (distilled) | [C] | spirits; a distillation upgrade path off the Keg |
| **Electrum** ⚙︎ | Gold + Argentum smelted | [C] | mechanical alloy; reliable and bulk |
| **Rebis** ✦ | Sol + Luna in Conjunction | [C][W3] | the "double thing": the same two inputs give a magical result, conditional and higher-tier |
| **Orichalcum** | Metallum + Fulgur | [C] (Plato's lost metal; common in games) | late metal; tool/weapon forging sidegrade |
| **Aura** (Ether) | Spiritus + Ros Maialis | [FF][C] | restores energy (Stardew's "mana" analog) |
| **Elixir** | Vitae + Anima | [FF][C] | restores health |
| **Panacea** | Elixir + Ordo | [C] | cures debuffs |
| **Cinis Phoenicis** (Phoenix Ash) | Flamma Viva + Vitae | [FF Phoenix Down][C] | combat: softens the death penalty (with a tradeoff); held to the combat cap below |
| **Terra Viva** (Living Soil) | Limus + Vitae | [C] | premium fertilizer that competes with deluxe fertilizer, not above it |
| **Gelum** (Frost) | Tempestas + Sal Fixum | [TC][C] (salt-ice freezing mixtures) | preservation; Frozen Tear tie-in; winter content |
| **Venenum** (Poison) | Miasma + Mors | [TC] | weed and pest control; combat coatings |
| **Ectoplasm** | Spiritus + Limus | [common media] | the Curator's restoration requests and Witch quests (kept: the item budget was loosened, doc 03b) |
| **Perpetuum Mobile** | Motus + Vapor + Ordo | [C] perpetual motion | Mechanist capstone candidate (always has a cost) |
| **Seed transmutation** *(approved candidate)* | a stack of seeds + Vitae, as a ritual | [C] the "growing" of matter | turns one crop's seeds into another's (same season, similar value); a guiding-star use for cheap seeds and the main legal outlet for living transmutation |
| **Radicula** (Rootling) *(approved candidate; working name)* | Anima + Terra Viva + a crop | [C] the homunculus, as a plant | a small plant homunculus that tends one crop patch (waters, or nudges growth); hand-made only; the furthest the ban allows |
| **Cauda Pavonis** (Peacock's Tail) | *process state*, not an item or recipe | [C] the iridescent stage of the Opus | shown when a brew passes nigredo→albedo; ties to Prismatic Shard |
| **Planetary metals** | via Metallum + the planet's substance on its weekday | [C][OM] | Argentum/Silver (Mon), Stannum/Tin (Thu), Plumbum/Lead (Sat) are new items, bought and certified by Agnes; Copper, Iron and Gold exist in vanilla. Advanced transmutation needs the Quarry bridge |
| **Caput Mortuum** | residue of any reduction | [C] | waste; see residue below |

---

## Tier 3: the Great Work (capstone substances)

| Substance | Route (proposal) | Inspiration | Role |
|---|---|---|---|
| **Prima Materia** | Caput Mortuum + Perditio (or Alkahest) | [C] "found in the dung heap": the first matter hides in the commonest refuse | **links the waste stream to the endgame**: residue becomes precious late |
| **Alkahest** (Universal Solvent) | Aqua Fortis + Mercurius Philosophorum | [C] | lossless decomposition of anything; a capstone with a cost |
| **Azoth** | Mercurius Philosophorum + Quinta Essentia | [C] | universal agent; transmutation capstone |
| **Lapis Philosophorum** (Philosopher's Stone) | Prima Materia carried through all four colour stages | [C][FMA][AT][Alchemy Factory] | a multi-stage *process*, not a recipe; one capstone among several, each with tradeoffs (FMA's "equivalent exchange" flavour). Its legend promises immortality, but **in play it grants none** (Charles): immortality is lore and aspiration only |

---

## Colour stages = quality tiers (approved, R1)

| Stage | Colour | Stardew quality |
|---|---|---|
| Nigredo | black | base (no star) |
| Albedo | white | silver |
| Citrinitas | yellow | gold |
| Rubedo | red | iridium |

**Cauda Pavonis** (iridescent) is the flourish between nigredo and albedo: a discovery toast moment.

---

## Waste residue (approved direction)

**Caput Mortuum** ("dead head"): the by-product of reductions and failed experiments. Very low sell price, plus minor uses:
- low-grade fertilizer (a small chance of quality, slight weed growth)
- mixed with Calx into a cheap **Glaze** for decorative pottery
- ballast for apparatus construction
- **late reveal:** the source of Prima Materia (above), so a player who kept a residue pile is rewarded

Open to more. *(The v0.2 "Rust Slimes" idea, feeding residue to a Slime Hutch, is dropped: it would transmute a living creature, which the plants-only rule forbids.)*

---

## Hand-made only (Charles, doc 01)
**Gifts, quest items and capstones are always hand-made**: through a crafting recipe or, preferably, a ritual. Automate and directional chaining never produce them (doc 04). In this catalog that covers the four Tier 3 substances, Perpetuum Mobile, the Radicula, and any product whose main use is gifting or a quest. The recipe model marks this with a hand-made-only flag (doc 12).

## Combat cap (Charles, doc 01)
Combat preparations (Pulvis Pyrius, Venenum coatings, Cinis Phoenicis, combat elixirs) give useful consumables and passive effects, but **none is more impactful than a Combat Mastery trinket**. The sweep benchmarks each against the trinkets (doc 06).

**Delivery (Charles):** passive effects come from long-lasting buffs, a couple of rings, and **some trinket-slot items** of our own (alchemical trinkets, e.g. a vial pendant or a smouldering censer). Those sit at the same power level as the Combat Mastery trinkets, not above it. They're ordinary crafted items, not adventure drops, so the item restraint in doc 03b doesn't apply.

## Grid size and discovery pacing

- Tier 1 grid: 36 cells with the default eight Primes. Content packs that add Primes grow it (n Primes give n(n+1)/2 cells; doc 03). That's the Zaikin-style "fill the board" goal, visible as a codex grid with silhouettes.
- Tier 2: about 25 shown here, extensible by content packs.
- Tier 3: 4 capstones.
- About 65 named substances is in the right ballpark for 1.0. Integration patches (SVE etc.) map their items to *sources*, not new substances, to keep the grid clean.
