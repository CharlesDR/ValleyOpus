# 03 · Essences & Decomposition

Status: **DRAFT v0.3** (2026-10-13). v0.3: content packs can add Primes, late-game sources (Ginger Island, Volcano, Skull Cavern), and the 1.0 tie-in mods (Charles's guiding answers, doc 01). Full substance list: [03a-substances-catalog.md](03a-substances-catalog.md) · Item rules: [03b-item-budget-and-inventory.md](03b-item-budget-and-inventory.md).

## The eight Primes (approved)
Fire, Water, Earth, Air (elements) · Salt, Sulfur, Mercury (tria prima) · **Aether** (rare).

**Extensible (Charles, 1.0):** other modders can add Primes, substances, apparatus and recipes through Content Patcher. The eight above are ValleyOpus's own set; the Prime list is data, and everything derived from it (the Tier 1 grid, the codex grid, tag weights, the sweep) is computed from that data, never hard-coded to eight. A pack that adds a Prime also supplies its pairings; any pairing it leaves undefined behaves as an undefined combination (residue). Doc 12 specifies the pack format.

## Decomposition model
Any object breaks down into a **profile**: a weighted mix of Primes plus a yield amount.

1. **Profile from tags.** Each context tag and category carries weights (e.g. `category_fish` → Water 3, Salt 1; `color_red` → Fire 1; `edible_mushroom` → Earth 2, Sulfur 1; `forage_item` + `season_fall` → Air 1). The item's profile is the normalized sum of its weights.
2. **Yield from value.** Total essence units scale **sub-linearly** with base price (e.g. ∝ √price), so cheap items still yield and expensive items can't break the economy. Quality raises yield slightly.
3. **Category defaults.** Every category has a fallback profile, so an untagged modded item still decomposes sensibly.
4. **Overrides.** A data file lets integration patches (SVE, etc.) or we ourselves pin specific items. Hand-tuned exceptions are explicit, never hidden.
5. **Residue.** Every decomposition leaves a little **Caput Mortuum**. More reductive routes (Perditio) give higher yields but more residue.

> "Equivalent exchange" flavour: decomposition never creates value. The essence's market value stays below the input's sell price (pillar: burning your starfruit is a choice, not a free win).

## Source-diversity rule (balance requirement)
Each Prime needs ≥3 independent sources in different activities, available by mid-Spring Y1 at modest volume:

| Prime | Farming | Foraging | Fishing | Mining | Animals / other |
|---|---|---|---|---|---|
| Fire | peppers, spicy crops | summer forage | lava eel (late) | coal, fire quartz | cooked dishes |
| Water | melons, juicy crops | spring forage, seaweed | most fish | frozen tear | milk |
| Earth | roots, potatoes | mushrooms | clams, crabs | stone, clay | — |
| Air | flowers, hops | fall forage, feathers | — | — | wool, feathers, honey |
| Salt | — | beach forage | ocean fish | minerals, quartz | cheese |
| Sulfur | sunflower and oil crops | mushrooms, sap | — | monster loot | eggs, mayonnaise |
| Mercury | fermented goods (wine) | — | roe, eels | ores (trace) | slime |
| Aether | rare crops | Secret Woods, ley nodes | legendary fish | prismatic shard | solar/void essence, grand rituals |

Late-game locations add columns rather than replace early ones (proposal): Ginger Island and the Volcano (Fire, Magma, Vitae; ley nodes), Skull Cavern (Sulfur, Mercury, Tenebrae from vanilla loot), and Mr. Qi's items as rare Aether sources.

Two v0.2 additions widen sources without adding items. **Ley Taps** on ley nodes (doc 04) give location-specific Primes directly, and **Delta** opens fish-based Water/Salt routes. Monster-derived Primes (Sulfur, Mercury, and Mors/Tenebrae via putrefaction) come mainly from **vanilla** loot; new drops are rare exceptions under the item budget (doc 03b). This table is illustrative. The **balance sweep** (a later task) computes actual availability per season from the vanilla item list plus the 1.0 tie-in mods (SVE, Alecto the Witch, Ridgeside Village and East Scarp), and flags any Prime with fewer than 3 viable Y1 sources.

## Combination
Primes combine pairwise into the 36 Tier 1 substances (a complete grid), then into Tier 2 compounds and Tier 3 capstones. Every combination is a discovery. See doc 07 for how players experiment, and 03a for the list.

## Twin routes
Many products can be reached by a **mechanical route** (reliable, bulk, capped quality) or an **arcane route** (conditional, higher ceiling). Example: Gold + Silver smelted into **Electrum**, versus Sol + Luna conjoined into **Rebis**. This is pillar 1 made concrete.

## Living transmutation: plants only (Charles, doc 01)
Transmutation and animation of **living things is limited to plants**. Seeds can be transmuted into other seeds, crops coaxed into new forms, and simple plant homunculi made. **Animals and people are never subjects**, in recipes, flavour or rituals. In-world, this is a ban the **Wizard** enforces (doc 08). Every recipe proposal is checked against this rule.
