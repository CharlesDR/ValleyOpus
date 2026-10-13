# 06 · Economy & Balance

Status: **DRAFT v0.3** (2026-10-13). v0.3: economy knobs in GMCM; targets hold at defaults (doc 00, U3). Benchmarks are to be computed in the balance sweep; the targets below are approved policy.

## Metrics (approved: compare on both)
For any production setup (a machine plus its required neighbours and support):

- **Gold per tile-day (GPTD)** = (output sell value − input market value) ÷ (tiles occupied, including required neighbours, boilers and support) ÷ days per cycle.
- **Value-add ratio (VAR)** = output sell value ÷ input market value, on a per-ingredient-cost basis. "Market value" is the input's sell price, or its buy price where it can only be bought.

Both are computed at base quality and again at the setup's realistic quality. Arcane setups are measured on **average conditions over a week**, not their best day.

## Vanilla benchmarks (to compute in the sweep)
Kegs, Preserves Jars and Dehydrators across representative inputs per season (e.g. wine/juice from fruit and vegetables, jelly/pickles, dried fruit and mushrooms). This uses the vanilla processing formulas from `Data/Machines`, verified in the sweep. Casks, Fish Smokers and Bee Houses are secondary benchmarks.

## Targets (approved)
| Phase | Alchemy vs. vanilla (both GPTD and VAR) |
|---|---|
| **Early game** (Y1 Spring–Summer, no Lab or perks) | **below** kegs, jars and dehydrators; alchemy's early draw is buffs, utility and discovery |
| **Mid game** (Lab T2, L5 profession) | roughly at par on its best routes |
| **Late, heavy investment** (Lab T3, L10, adjacency-optimized) | **at most 150%** of the best comparable vanilla machine, *with perks* (to be revisited later) |

Rules that keep this honest:
- **Targets apply at default settings.** Players can change economy knobs in GMCM (Charles, U3: player choice beats exploit-proofing), so the caps are a promise about the default game only.
- The cap applies **after** stacking all owned professions (see the all-six test in doc 05) and Lab bonuses.
- Essences themselves sell for less than their inputs (decomposition never prints money).
- Alchemy's main draw is **non-monetary**: buffs, fertilizers, combat aids, tools and capstones (player-experience pillar 1).
- **Finch** (Travelling Merchant) buy and sell prices respect the same caps. He's a valve for bottlenecks, not a money printer.
- **Dr. Sorenson's bulk contracts:** fixed-price orders for standardized (mechanical-quality) products. They trade a lower unit price for guaranteed volume and story progress, and count toward the caps.
- **Agnes** buys planetary metals at prices benchmarked against vanilla bars.
- **Grand rituals** are measured per batch-day *and* per tile (the circle's footprint), averaged over the conditions needed to run them.

## Balance sweep (future task)
1. Item sweep: decompose every vanilla item (and later, the top content mods) and tabulate Prime yields per season → check the ≥3-source rule (doc 03).
2. Setup sweep: compute GPTD and VAR for each apparatus route and arrangement → check the phase targets.
3. Output: a spreadsheet plus flagged violations, re-run whenever the data changes (scriptable from the data files).
