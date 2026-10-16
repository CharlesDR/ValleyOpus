# 03 · Essences & Decomposition

Status: **DRAFT v0.4** (2026-10-16). v0.4: the **data shape** of decomposition (tag → Prime weights, category defaults, overrides, the yield formula and its settings), matching doc 12 §2. v0.3: content packs can add Primes, late-game sources (Ginger Island, Volcano, Skull Cavern), and the 1.0 tie-in mods (Charles's guiding answers, doc 01). Full substance list: [03a-substances-catalog.md](03a-substances-catalog.md) · Item rules: [03b-item-budget-and-inventory.md](03b-item-budget-and-inventory.md).

## The eight Primes (approved)
Ignis, Aqua, Terra, Aer (elements) · Sal, Sulphur, Mercurius (tria prima) · **Aether** (rare).

**Extensible (Charles, 1.0):** other modders can add Primes, substances, apparatus and recipes through Content Patcher. The eight above are ValleyOpus's own set; the Prime list is data, and everything derived from it (the Tier 1 grid, the codex grid, tag weights, the sweep) is computed from that data, never hard-coded to eight. A pack that adds a Prime also supplies its pairings; any pairing it leaves undefined behaves as an undefined combination (residue). Doc 12 specifies the pack format.

## Decomposition model
Any object breaks down into a **profile**: a weighted mix of Primes plus a yield amount.

1. **Profile from tags.** Each context tag and category carries weights (e.g. `category_fish` → Aqua 3, Sal 1; `color_red` → Ignis 1; `edible_mushroom` → Terra 2, Sulphur 1; `forage_item` + `season_fall` → Aer 1). The item's profile is the normalized sum of its weights.
2. **Yield from value.** Total essence units scale **sub-linearly** with base price (e.g. ∝ √price), so cheap items still yield and expensive items can't break the economy. Quality raises yield slightly.
3. **Category defaults.** Every category has a fallback profile, so an untagged modded item still decomposes sensibly.
4. **Overrides.** A data file lets integration patches (SVE, etc.) or we ourselves pin specific items. Hand-tuned exceptions are explicit, never hidden.
5. **Residue.** Every decomposition leaves a little **Caput Mortuum**. More reductive routes (Perditio) give higher yields but more residue.

> "Equivalent exchange" flavour: decomposition never creates value. The essence's market value stays below the input's sell price (pillar: burning your starfruit is a choice, not a free win).

## Source-diversity rule (balance requirement)
Each Prime needs ≥3 independent sources in different activities, available by mid-Spring Y1 at modest volume:

| Prime | Farming | Foraging | Fishing | Mining | Animals / other |
|---|---|---|---|---|---|
| Ignis | peppers, spicy crops | summer forage | lava eel (late) | coal, fire quartz | cooked dishes |
| Aqua | melons, juicy crops | spring forage, seaweed | most fish | frozen tear | milk |
| Terra | roots, potatoes | mushrooms | clams, crabs | stone, clay | — |
| Aer | flowers, hops | fall forage, feathers | — | — | wool, feathers, honey |
| Sal | — | beach forage | ocean fish | minerals, quartz | cheese |
| Sulphur | sunflower and oil crops | mushrooms, sap | — | monster loot | eggs, mayonnaise |
| Mercurius | fermented goods (wine) | — | roe, eels | ores (trace) | slime |
| Aether | rare crops | Secret Woods, ley nodes | legendary fish | prismatic shard | solar/void essence, grand rituals |

Late-game locations add columns rather than replace early ones (proposal): Ginger Island and the Volcano (Ignis, Magma, Vitae; ley nodes), Skull Cavern (Sulphur, Mercurius, Tenebrae from vanilla loot), and Mr. Qi's items as rare Aether sources.

Two v0.2 additions widen sources without adding items. **Ley Taps** on ley nodes (doc 04) give location-specific Primes directly, and **Delta** opens fish-based Aqua/Sal routes. Monster-derived Primes (Sulphur, Mercurius, and Mors/Tenebrae via putrefaction) come mainly from **vanilla** loot; new drops are rare, high-value exceptions found only in late-game adventure areas (doc 03b). This table is illustrative. The **balance sweep** (a later task) computes actual availability per season from the vanilla item list plus the 1.0 tie-in mods (SVE, Alecto the Witch, Ridgeside Village and East Scarp), and flags any Prime with fewer than 3 viable Y1 sources.

## Combination
Primes combine pairwise into the 36 Tier 1 substances (a complete grid), then into Tier 2 compounds and Tier 3 capstones. Every combination is a discovery. See doc 07 for how players experiment, and 03a for the list.

## Twin routes
Many products can be reached by a **mechanical route** (reliable, bulk, capped quality) or an **arcane route** (conditional, higher ceiling). Example: Gold + Argentum smelted into **Electrum**, versus Sol + Luna conjoined into **Rebis**. This is pillar 1 made concrete.

## Living transmutation: plants only (Charles, doc 01)
Transmutation and animation of **living things is limited to plants**. Seeds can be transmuted into other seeds, crops coaxed into new forms, and simple plant homunculi made. **Animals and people are never subjects**, in recipes, flavour or rituals. In-world, this is a ban the **Wizard** enforces (doc 08). Every recipe proposal is checked against this rule.

## Data shape (v0.4, per doc 12)
Decomposition is one built-in rule over four data tables, not a recipe per item (doc 12 §2). All are **public Content Patcher assets** under `CharlesDR.ValleyOpus/…`, edited with `EditData`, each with a `FormatVersion`. They are identical on every screen, so decomposition needs no multiplayer sync, and nothing here touches the save.

| Asset | Kind | Key | Fields |
|---|---|---|---|
| `Primes` | CP data | `Id` | `ItemId`, `Group` (Element / Principle / Rare / pack-defined), `Color`, `NameKey`, `GlossKey`, `SortOrder` (owned by doc 12; listed here because every weight names a Prime) |
| `Decomposition/TagWeights` | CP data | a context tag (`category_fish`, `color_red`) or a category number | `Weights` {Prime `Id` → number}, `AetherSource` (true only for rows on the Aether source list) |
| `Decomposition/CategoryDefaults` | CP data | category number, plus one `Default` row | `Weights` {Prime `Id` → number}; never carries Aether |
| `Decomposition/Overrides` | CP data | item ID (qualified) | `Profile` {Prime `Id` → number} (optional), `Units` (optional), `Refuse` (true for hand-made-only items and capstones) |
| `Decomposition/Settings` | CP data, read by the GMCM economy knobs | single row | `k`, `e`, `QualityBonus`, `Threshold`, `ExchangeRatio`, `DefaultResidue` {count, chance} |

**Route modifiers** live on the decomposing apparatus, not here: `YieldMultiplier`, `ResidueRate` and an optional `Concentration` (keep the top N Primes, e.g. the Athanor's top two). Doc 13 owns those fields.

### The computation
Given an item, its quality and the decomposing apparatus:
1. **Override first.** If `Overrides` has the item: `Refuse` rejects it at load-in; a pinned `Profile` or `Units` replaces the step it covers.
2. **Profile.** Sum `Weights` over every context tag the item carries plus its category row in `TagWeights`. If nothing matched, use its `CategoryDefaults` row, else the `Default` row. Aether weight from a row without `AetherSource` is dropped (and reported by the validator), so the category defaults can never make Aether. Normalize to 1, drop Primes below `Threshold` (placeholder 0.10), renormalize. Apply the apparatus `Concentration` if it has one.
3. **Units.** `units = max(1, floor(k · price^e · (1 + QualityBonus · quality) · YieldMultiplier))`, with `price` the item's base sell price, `quality` 0/1/2/4 and `e ≈ 0.5`.
4. **Split.** Units are shared across the profile by largest remainder (ties broken by `SortOrder`), so the total is exact and deterministic.
5. **Equivalent exchange.** If the Primes' summed sell value exceeds `ExchangeRatio · price` (placeholder 0.8), units are reduced until it doesn't. The validator lists every clamp.
6. **Residue.** Caput Mortuum by the apparatus `ResidueRate` (or `DefaultResidue`), rolled on doc 12's deterministic seed (day, location, tile, recipe), so reloads and farmhands see the same result.

Primes produced carry **no quality** (doc 12, approved 2026-10-16): the input's quality only raises `units`.

**Derived, never stored:** each item's resolved profile (cached per item ID and invalidated on asset reload), the Assayer's Lens tooltip (shown once `assay:<ItemId>` is set in the farm codex, doc 12 §10), and the **source-diversity sweep**, which reads the same tables over the vanilla and tie-in item lists and reports any Prime with fewer than 3 viable Y1 sources.

**Content packs** extend by adding rows: tag weights for their own tags, overrides for their items, and a `Primes` row plus pairing recipes for a new Prime (doc 12 holds the pack format). A row naming an unknown Prime is disabled with a SMAPI warning, never a crash.

**Example rows** (placeholders, not tuned; `id_o_74` is the Prismatic Shard's vanilla ID tag):
```json
"Decomposition/TagWeights": {
  "category_fish":  { "Weights": { "Aqua": 3, "Sal": 1 } },
  "color_red":      { "Weights": { "Ignis": 1 } },
  "id_o_74":        { "Weights": { "Aether": 4, "Terra": 1 }, "AetherSource": true }
}
```
