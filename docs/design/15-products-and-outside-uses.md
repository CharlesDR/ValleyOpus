# 15 · Products & Outside Uses

Status: **DRAFT v0.6** (2026-10-17) · Readiness: ready for architecture (Charles approved every default he owns on 2026-10-17; no `[blocks-arch]` item is open)
Depends on: 01, 03, 03a, 03b, 05, 06, 07, 12, 13 · Feeds: 03a, 03b, 05, 06, 14, 16, 17, 19, 20

## Purpose
What leaves the mod. Every alchemical item either feeds another recipe or does something in the rest of the valley: it is eaten, spread on soil, carried into the mines, shipped, given, or used in a vanilla system. This doc gives each product family its rules, its vanilla benchmark and its data shape, and checks every Item-class substance in doc 03a for a use. It serves player-experience pillar 1 (reasons from outside the mod), pillar 2 (don't invalidate kegs, jars and dehydrators, per doc 06's caps), the guiding star (cheap, under-used things such as flowers, honey and residue become worth something), and the combat cap.

## Decisions locked
- **P1 · Product families** (Charles, 2026-10-13): consumables, fertilizers, combat preparations, sellable compounds and gifts in 1.0. Tool and weapon forging is 2.0. **Added (Charles, 2026-10-17):** a sixth family, **animal infusions** (§7a).
- **P2 · Effects** (Charles, 2026-10-13; widened 2026-10-16): vanilla buff stats plus custom effects, through an **extensible** effect system. The 2–3 cap is **suspended** (Charles, 2026-10-16) until the scale of the effect list is dialled in; more feedback on the list is coming. Charles approved the thematic-effect catalogue for 1.0 on 2026-10-17 (§3a), and **every custom effect is tiered**, weak for early items and strong for late items and capstones.
- **Defaults approved** (Charles, 2026-10-17): every default this doc proposed that Charles owns is confirmed: Smaltum kept (§9); Second Wind, Cleanse and Glow kept; Caput Mortuum as fertilizer without a weed tradeoff, and refusing decomposition; the Artisan category; the three gift products; Pulvis Pyrius making vanilla bombs; trinket upgrades at the altar; the SVE top end applying with or without SVE, under the trinket cap. Defaults owned by later docs are settled there.
- **Element** (Charles, 2026-10-17): Charles's umbrella term for Primes, substances and anything derived from them. "Prime" and "substance" still name those specific things.
- **P3 · Bundles** (Charles, 2026-10-13): workshop bundles take both vanilla items and mod substances; early bundles are all vanilla.
- **Economy caps** (doc 06, approved): early alchemy earns **below** kegs, jars and dehydrators; mid-game reaches par on its best routes; heavy late investment with every perk reaches **at most 150%** of the best comparable vanilla machine. Compared on gold per tile-day and value-add per ingredient cost, at default settings.
- **Residue** (doc 03a, approved): Caput Mortuum has minor uses and a very low price; late, it becomes the source of Prima Materia.
- **Hand-made only** (Charles, doc 01 and doc 10 #3): gifts, quest items and capstones are hand-made. Products made mainly for gifting are ✋; any other item can still be gifted.
- **Power baselines** (Charles, 2026-10-16): early- and mid-game product power is benchmarked against **vanilla**; the **top end** (late game) is benchmarked against **SVE's consumables and equipment**. The SVE ceiling applies whether or not SVE is installed, so a player without SVE gets the same items. **Prices** follow the same pattern (§6).
- **Combat cap** (Charles, doc 01): no combat preparation, trinket or ring is more impactful than a Combat Mastery trinket.
- **Plants only, relaxed for animal infusions** (Charles, doc 01; relaxed 2026-10-17): no product transmutes or animates an animal or a person. Charles relaxed this slightly for **animal infusions**, under the five guard rails in §7a: an infusion changes how an animal lives or what it gives, never what it is.
- **Control premium** (Charles, 2026-10-16): an effect that depends on something the player can't control (weekday, weather, season, moon) is about ×1.5 the size of an always-on effect of the same tier, since the player has to plan to get the full benefit (placeholder).
- **Latin first** (Charles, 2026-10-14): every product is a material and follows doc 03a's naming rule. Effects, trinkets, rings and furniture are not materials and take plain English working names, like vanilla buffs and the Gloam Lantern.
- **Recipe uniqueness and the environment rule** (Charles, 2026-10-16; doc 13 §1a): every product recipe here names its neighbour states, no two share apparatus, state and inputs, and weather or calendar only gates or adds a Bonus to an automatable recipe. On a day the gate isn't met, the machine **waits idle** (Charles, 2026-10-17): it refuses inputs and resumes later, so nothing is consumed and no residue is made. Power is never an input (doc 13 §5).
- **Selling pays the shipping price everywhere** (Charles, 2026-10-17): as in vanilla, any shop that buys an item pays its shipping price. There is no per-vendor pricing.
- **Drinks over food** (Charles, 2026-10-17): content lists carry more drinks than foods, since vanilla has many good foods and few drinks. Every food we add carries at least one of our custom effects, so it sits on a different axis from vanilla and SVE food.
- **Alternate recipes for vanilla items** (Charles, 2026-10-17): never cost more than the vanilla recipe. A discount above about 10% needs a reason, such as crafted ingredients or extra steps: higher effort for a better ratio is the accepted trade.

## Player-facing spec
- **Six exits.** A product is eaten or drunk, spread on tilled soil, used in combat, sold, given, or used as a material in a vanilla system (a dye, a fuel, an alternate crafting recipe). The tooltip and codex entry show a **Uses** line for each ("Drink: Speed +1, 4 min" · "Dye: strong blue" · "Sold at Agnes's"), so the player never has to guess why an item exists.
- **Familiar handling.** Every exit uses a vanilla verb: eat from the toolbar, place fertilizer on hoed dirt, ship in the bin, give with the gift button, drop into a dye pot. The one new verb is **Apply** (right-click a non-edible product to apply its effect, used by coatings and Phoenix Ash), which shows the same prompt as eating.
- **Buffs look vanilla.** Alchemical effects appear in the buff bar with an icon, a timer and a tooltip, including the three custom ones.
- **Where products go.** The Reagent Satchel auto-collects **reagents** (Primes and inputs). Finished products whose main use is outside the mod (food, fertilizer, combat, gifts) go to the main inventory, where the player uses them; the player can still put them in the Satchel by hand *(default; changes doc 03b's auto-collect line)*.
- **Failure state:** none new. A product can't be used wrongly; fertilizer follows vanilla's one-per-tile rule, and an Apply item says why it can't be applied (no weapon equipped).

## Rules & systems

### 1 · Families and uses
A **product** is an Item-class substance (doc 12 §1) with at least one outside **use**. Each product has one **primary family** (P1, for the codex and Satchel tabs) and any number of typed uses:

| Use | What it does | Vanilla plumbing |
|---|---|---|
| **Edible** | restores energy and health, applies effects | `Data/Objects` `Edibility`, `IsDrink`, `Buffs` |
| **Apply** | applies effects without eating (coatings, ash) | our use action, same buff path |
| **Fertilizer** | quality, speed and water-retention boosts on one tile | vanilla fertilizer placement; our fertilizer handler (§4) |
| **Field treatment** | acts on an area of the world (clears weeds) | our use action |
| **Equip** | passive effects while worn (trinket or ring) | `Data/Trinkets` / ring hooks (§5) |
| **Animal infusion** | used on a farm animal, like Golden Animal Crackers; changes its produce or care (§7a) | our use action on `FarmAnimal`, state in its `modData` |
| **Placeable** | a placed helper (the Radicula) | a BigCraftable with `modData` |
| **Material** | an ingredient in a vanilla system: dye, fuel, an alternate crafting recipe | color tags, appended `Data/Machines` rules, `Data/CraftingRecipes` |
| **Sell** | shipping bin plus named buyers (§6) | `Price`, category |
| **Gift** | default gift taste; per-NPC tastes in doc 20 | `Data/NPCGiftTastes` |
| **Quest** | a story-quest delivery | doc 08 / doc 20 |

Every product has **Sell**. "Sellable compound" is the primary family of a product whose only outside use is sale; that is a real use (P1), not a gap.

**No dead ends.** Every Item-class or in-machine substance must either feed a 1.0 recipe or carry an outside use. An intermediate that feeds nothing is bottled and sold until a recipe pass gives it a downstream. The audit in the content inventory lists the ten current gaps.

### 2 · Consumables
- **Benchmarks** (Charles's baselines, above). Early and mid game: energy, health and buffs per item are compared with vanilla foods at the same stage (early: Salad, Field Snack; mid: Seafoam Pudding-class buff foods). Late game: the ceiling is SVE's top consumables (its late-game foods and drinks, listed in the sweep), not vanilla's Triple Shot Espresso or Squid Ink Ravioli. An alchemical consumable may match the benchmark of its tier on one axis, never beat the best item of its tier on all of them.
- **Effects** come from the effect registry (§3), so a product names effect IDs, not buff fields.
- **Stacking** follows vanilla: one food buff and one drink buff at a time; a new one of the same kind replaces the old. Custom effects ride on those same buffs (§3), so they follow the same rule.
- **Drinks over food** (Decisions locked): most new consumables are drinks; each food carries a custom effect.
- **Automatable** unless marked ✋: consumables are ordinary production goods, and Sorenson's contracts may ask for them.

### 3 · The effect system (P2)
One registry, used by consumables, Apply items, trinkets, rings and any pack:
- **An effect is a row**: an ID, a kind (**Vanilla stats** or **Custom**), its stat changes or handler ID with parameters, a duration, an icon and its text keys.
- **Every effect is a vanilla buff.** Vanilla-stat effects are `Data/Buffs` rows. A custom effect is also a `Data/Buffs` row, carrying a `CustomFields` entry that names its **handler**. Our C# watches each player's active buffs and runs the handler while the buff is active. So the buff bar, timers, save data, multiplayer and the food-and-drink rule are all vanilla's.
- **Extensible.** A content pack adds effects by adding rows that reuse an existing handler with new parameters. A C# mod registers a new handler through our API (doc 09). A row naming an unknown handler is disabled with a SMAPI warning.
- **Passive effects** (trinkets, rings) use the same rows: while the item is worn, its effects are applied as an unending buff that ends when it is unequipped.

**The first three custom effects** (kept, Charles 2026-10-17, alongside the catalogue in §3a):
| Effect | Does | Carried by | Why custom |
|---|---|---|---|
| **Second Wind** | while active, the next time health hits zero the player stays up at low health instead of passing out; the buff ends and leaves a vanilla Weakness debuff for the rest of the day | Cinis Phoenicis (Apply) | vanilla has no death softening; the Weakness debuff is the tradeoff doc 03a asks for |
| **Cleanse** | removes active debuffs (Slimed, Jinxed, Darkness, Nauseous, Weakness, Tipsy and pack-added ones) and grants a short vanilla Immunity bonus | Panacea | vanilla can resist debuffs but can't remove them |
| **Glow** | a light circle around the player, like a Glow Ring, for the duration | Sol (drink), the Smouldering Censer (passive) | vanilla light comes only from rings and placed objects |

Everything else uses vanilla stats: Speed, Defense, Attack, Immunity, MaxStamina, MagneticRadius, the skill levels, Luck, and the 1.6 multipliers (attack, crit chance, crit power, weapon speed, knockback).

### 3a · Tiered effects (Charles, 2026-10-16; catalogue approved 2026-10-17)
- **One effect, two tiers.** Each custom effect row has a numeric **strength** and an optional **augmentation** (a secondary effect). The **weak tier** uses a low strength and no augmentation and goes on early items (an early ring, a Tier 1 drink). The **strong tier** raises the strength and adds the augmentation, and goes on late items and capstones. Packs can add tiers as rows.
- **Balance rules** (Charles, 2026-10-16 and 17; doc 06): the weak tier is benchmarked against vanilla items of its stage, the strong tier against SVE's late consumables and equipment, and both stay under the combat cap (§5). Effects gated by something the player can't control get the **control premium**. **Indirect boosters** (effects that speed other machines rather than produce) use looser rules, and several in one area never stack: a machine takes only the strongest in range. **No effect duplicates a vanilla profession** (a flower-quality bonus was cut because Botanist already makes forage iridium).
- **The catalogue** below is approved for 1.0 (Charles, 2026-10-17). Every number is a placeholder for the sweep; the ideas backlog keeps the brainstorm notes. Feasibility follows the backlog's key: **L** is light C# on the handler path, **H** needs a Harmony patch on vanilla behaviour.

**Effects carried by consumables**
| Effect | Carrier | Weak tier | Strong tier adds | Feas. |
|---|---|---|---|---|
| **Fixative** | drink from Sal Fixum; strong from Mercurius Fixus | pauses food and drink buff timers for 2 min | 8 min, and debuff timers run twice as fast | L |
| **Volatile** | drink from Mercurius Currens | active buffs' stats ×1.5, remaining time halved | ×2, and time only drops by a third | L |
| **Lunar Dream** | food or drink from Luna, taken before bed | a vague codex hint toward one undiscovered recipe | a precise hint naming one input, and sleep restores 10% more energy | L (doc 07's hint pool, never a repeat) |
| **Undine's Gift** (Aqua) | drink from Ros Maialis | 50% chance each watering uses no water | no water used, and each swing waters one extra tile | L |
| **Midas Draught** | drink, mid-late game (Metallum line) | 5% chance mined ore comes out one metal higher | 12%, and gold can become iridium at a lower chance | L |
| **Hourglass of Mercury** | late drink | the clock runs 25% slower for a while | 40% | L–H; **single-player only**: does nothing with farmhands connected (shared clock); TimeSpeed test |
| **Vitality** | Elixir | slow health regeneration | faster, and Max Health up while active | L |
| **Savour** | Sal Marinum | cooked food restores 15% more energy and health | 30%, and food buffs last 25% longer | L |
| **Good Cheer** | Aqua Vitae | friendship +5% while active, and no Tipsy | +15%, and Luck +1 | L |
| **Spectral** | Ectoplasm (its quest use stays) | no contact damage for a few seconds after a hit | longer, and ghosts ignore you | L |
| **Dissolution** | Alkahest (capstone) | stones, wood and weeds break in one hit for N swings | longer, and stumps and boulders too | L |
| **Second Wind** | Cinis Phoenicis | as §3 | survive at higher health, with no Weakness | L |
| **Cleanse** | Panacea | as §3 | also immune to new debuffs for a while | L |

**One bonus per Prime** (each Prime's signature effect; carriers are the Prime's bottled self-pair or a drink from it, set in the content pass)
| Prime | Effect | Weak tier | Strong tier adds |
|---|---|---|---|
| Ignis | **Kindled** | your hits set monsters burning on 20% of hits | every hit, and no lava or burn damage |
| Aqua | **Undine's Gift** | (above) | |
| Terra | **Stonesense** | a chance of an extra ore from stones | higher, and rocks break in one hit |
| Aer | **Tailwind** | Speed +0.5 | Speed +1, and your horse also moves faster |
| Sal | **Fixative** | (above) | |
| Sulphur | **Eruption** | 5% chance a broken stone bursts and breaks the stones around it | 15%, a wider burst that also hurts monsters, and extra Cinder Shards in the volcano |
| Mercurius | **Volatile** | (above) | |
| Aether | **Quintessence** | your other alchemical effects are 10% stronger | 25%, and Luck +1 |

**Worn and placed items**
| Item | Kind | Weak tier | Strong tier adds | Feas. |
|---|---|---|---|---|
| **Vial Pendant** | trinket | below 25% health, drinks one Elixir from your inventory (once per 3 in-game hours) | below 35%, once an hour, and the drink also grants Cleanse | L |
| **Smouldering Censer** | trinket | Glow, plus burn damage to every monster in a small radius (about 40% of the Magic Quiver's damage per target, so the Quiver wins on 1–2 targets and the Censer on 3+) | wider radius, and burning monsters slow down | L |
| **Rebis Amulet** | trinket | by day Attack +6%, by night Defense +2 | +15% / +4, and both apply from noon to 3pm and from dusk to 3 hours after (control premium) | L |
| **Ouroboros Ring** | ring | trashed items return as Caput Mortuum; each kill restores 1 energy | each kill restores 3 energy and 1 health, and when one of your food or drink buffs runs out on its own you get a little energy back | L |
| **Planetary Band** / **Ring of the Seven** | rings | Planetary Influence, weak (below) | strong (below); the Ring of the Seven is a hand-made capstone ring | L |
| **Azoth Diffuser** | placed, lasts a few days, then crumbles to Caput Mortuum | every machine (vanilla, modded and ours) within radius 7 works 10% faster | 25% | L; radius is a config setting (default 7); an indirect booster, so it never stacks |
| **Ever-burning Lamp** | placed, very expensive (Flamma Viva) | crops in a small radius keep growing out of season, winter included | a larger radius | H (per-tile season check) |
| **Radicula** | placed plant companion (§1) | **Shriek**: now and then stuns nearby monsters | more often, wider radius | L |

**Capstone tiers.** **Transmutation** (Lapis Philosophorum: 5% chance a picked-up item comes out one quality higher; strong 15%, and mined ore can come out one metal higher) and the strong tiers of Dissolution and the Ring of the Seven are capstone effects. Doc 19 places them; the rows live here.

**Kept for the content pass** (approved, not yet given rows): Giant's Catalyst and Divining Rod from the first shortlist. Salamander Skin, Quicksilver Hands and Mandrake Sprout are already folded into Kindled, Wednesday and the Radicula.

**Planetary Influence.** One effect whose buff on any day comes from that day's planet, matching the planetary weekdays the recipes already use (doc 03a). The weak ring, the **Planetary Band**, is an early ring of cheap metal. The strong ring, the **Ring of the Seven**, is a hand-made capstone ring of all seven metals plus Aether. It is a worn item, not a recipe, so the environment rule for automated recipes doesn't apply.
| Day | Planet · metal | Weak ring | Strong ring adds |
|---|---|---|---|
| Mon | Moon · Argentum | Fishing +2 | Fishing +4, and a higher treasure chance |
| Tue | Mars · iron | Attack +8% | Attack +20%, and more crit power |
| Wed | Mercury · Mercurius | Speed +0.75 | Speed +1.5, and tools charge faster |
| Thu | Jupiter · Stannum | Luck +1 | Luck +3, and a small chance of an extra crop at harvest |
| Fri | Venus · copper | gifts and talk give 15% more friendship | 35%, and one gift that day doesn't count toward the two-a-week limit |
| Sat | Saturn · Plumbum | tools cost 15% less energy | 35% less, and Defense +3 |
| Sun | Sun · gold | Glow and Farming +1 | Farming +3, and slow health regeneration |
The numbers include the control premium, since the buff depends on the weekday. The Tuesday peak of the strong ring is checked against the combat cap and SVE's late rings.

**Not in 1.0:** the backlog's long list (Exchange Tincture, Scrying Draught, Windsight, Hermetic Mirrors) stays in the backlog.

### 4 · Fertilizers and field treatments
- **Sidegrades, not upgrades** (pillar 3, doc 03a): an alchemical fertilizer may match a vanilla tier on one axis and add a second axis vanilla can't combine, but it never beats Deluxe Fertilizer, Hyper Speed-Gro or Deluxe Retaining Soil on their own axis. Vanilla allows one fertilizer per tile, so combining axes is the whole appeal.
- **Fertilizer profile.** A fertilizer use carries three numbers: `QualityLevel` (vanilla 0–3), `SpeedBoost` (vanilla 0.10–0.33) and `WaterRetention` (vanilla 0.33–1). Our C# handler answers vanilla's three fertilizer queries for our item IDs from this data, so packs can add fertilizers as rows.
- **1.0 rows** (placeholders):
  - **Caput Mortuum**: quality level 1 at a reduced chance (below Basic Fertilizer). The "slight weed growth" tradeoff in doc 03a is dropped (approved): residue is cheap enough that it needs no drawback, and the weed hook costs a farm-spawn patch.
  - **Terra Viva**: quality level 2 (Quality Fertilizer) **plus** 33% water retention (Basic Retaining Soil). It competes with Deluxe Fertilizer without beating its quality.
- **Field treatment: Mors** (Death) clears weeds in a small radius (placeholder 2 tiles) when used on the ground; crops, trees and placed objects are untouched (doc 03a: "weed-killing"). It never kills a planted crop, so it can't be used against the plants-only rule's spirit.
- Fertilizers are ordinary production goods: automatable, bundle-eligible, contract-eligible.

### 5 · Combat preparations, trinkets and rings
- **The cap.** Each item is benchmarked against the **Combat Mastery trinkets** (Basilisk Paw, Fairy Box, Frog Egg, Golden Spur, Ice Rod, Magic Hair Gel, Magic Quiver, Parrot Egg). Early and mid-game items are also held to vanilla equipment of their stage (rings, the Mines' weapons' buffs). At the top end, SVE's late equipment and consumables set the ceiling; where SVE's ceiling sits above the trinkets, the trinket cap still holds until Charles says otherwise. A passive item is held to the weakest-to-middle of the trinket set; a consumable's whole-day impact (effect × uptime, counting its cost) may not exceed wearing one trinket all day. The sweep reports both (doc 06).
- **Explosives through vanilla bombs.** Pulvis Pyrius is a **Material**: alternate crafting recipes turn it into vanilla **Bombs** and **Mega Bombs**, more per craft than the vanilla recipe; the discount is justified by Pulvis Pyrius being a three-input compound made from other Elements (Decisions locked: alternate recipes). No custom explosive in 1.0, because vanilla bomb behaviour is hard-coded by item ID and a custom one needs its own C#. Custom-effect bombs (Miasma smoke, frost) are `[2.0]`.
- **Coatings (Venenum).** An **Apply** item: a timed buff on vanilla stats (attack and crit-chance multipliers). It is a buff, not a weapon enchantment, so it ends with its timer and works with any weapon.
- **Phoenix Ash (Cinis Phoenicis).** Apply, Second Wind (§3). Benchmarked against Fairy Box-class sustain.
- **Trinkets:** the **Vial Pendant**, the **Smouldering Censer** and the **Rebis Amulet** (§3a), each in a weak and a strong version (the strong one is made by upgrading the weak one at the altar). Crafted, never dropped (doc 03b rule 1 doesn't apply). They use one C# trinket class that applies the trinket's effect rows while equipped, so a new trinket is a data row plus art.
- **Rings:** the **Planetary Band**, the **Ring of the Seven** and the **Ouroboros Ring** (§3a), through the ring equip and unequip hooks.
- Elixir (health) and Vigor (energy) are combat-useful consumables but sit in the consumable family.

### 6 · Sellables and prices
- **Every product has a price; most are derived.** Default rule: `Price = round(Markup × Σ input prices + Flat)`, computed at load from the product's **reference recipe** (the recipe flagged as such, else the cheapest route). A row may pin a fixed `Price` instead (metals, gifts, residue). Primes' prices come from doc 03's equivalent-exchange rule, so every chain starts below the price of what was decomposed.
- **Benchmarks** (Charles, 2026-10-17): the same pattern as consumables: vanilla machines and goods for early and mid game, and the top end set by what's possible with vanilla plus SVE.
- **Markup bands** (placeholders, tuned in the sweep): Tier 1 about ×1.2, Tier 2 about ×1.4, Tier 3 and capstones not priced for profit. **The arcane line earns through quality, not markup:** both lines share a band, and arcane apparatus reach higher quality, which vanilla's quality price multipliers (×1.25, ×1.5, ×2) turn into money. That keeps pillar 1 (mechanical is bulk, arcane is quality) visible in the price.
- **Chains are measured end to end.** Markups compound along a chain, so value-add is measured from the raw vanilla inputs that were decomposed to the final product, never per step. A short chain is the early game; mid-game par comes from quality, Lab bonuses, perks and speed (gold per tile-day), not from higher markups; the 150% ceiling is checked with every perk and Lab bonus stacked (doc 05's all-six test). For orientation (vanilla formulas, verified in the sweep): a Keg makes about ×3 the fruit price, a Preserves Jar about ×2 + 50, a Dehydrator about ×1.5 per fruit.
- **Category** (approved): sellable compounds, consumables, gifts and dyes use the vanilla **Artisan Goods** category, so the Artisan profession applies to them exactly as it does to wine and jelly. That keeps the comparison with kegs fair, and doc 06's caps already count it. Primes, intermediates and residue use a category no vanilla profession boosts. Metals use the metal-bar category like Copper or Gold Bars (to verify which professions that triggers, doc 09).
- **Where to sell.** The shipping bin buys everything, and every shop that buys an item pays its **shipping price**, as in vanilla (Charles, 2026-10-17). Shops are places to sell, never price levers:
  - **Finch** buys Primes and substances and sells rare reagents, a valve for bottlenecks.
  - **Agnes** buys the planetary metals and certifies Electrum and Orichalcum. The metals' prices are benchmarked between vanilla bars (placeholders: Stannum near Copper-to-Iron, Plumbum near Iron, Argentum between Iron and Gold).
- **Dr. Sorenson's contracts.** Players are cash-motivated, so a bulk contract must offer **a higher unit price than shipping**, or a significant non-monetary reward: a story step, an unlock or a unique item. A contract never pays less than the bin. Contracts accept automated output (doc 10 #2) and count toward doc 06's caps.
- **Residue:** Caput Mortuum sells for a token price (placeholder 2g), the lowest of any mod item. Its value is its uses: fertilizer, Smaltum, ballast in apparatus recipes, hint divination (doc 07) and, late, Prima Materia. It **refuses decomposition** (approved): it is what decomposition leaves, and decomposing it would loop.

### 7 · Gifts and quest items
- **Any item can be gifted.** Gift tastes for every vanilla and tie-in villager are content (doc 20); this doc sets each product's **default taste** (most alchemy is a neutral gift, Primes and residue a dislike).
- **Gift products are hand-made** (✋): made by hand at an apparatus or altar, never automated, staged or chained. Three proposals, all built from cheap, under-used inputs (the guiding star):
  - **Rosa Vitrea** (Glass Rose): Vitrum + any flower, Conjunction on a minor arcane vessel, ✋. Flavoured by its flower like vanilla honey ("Glass Tulip"). A broadly liked gift, loved by a few.
  - **Mel Vitae** (Living Honey): Vitae + any honey, Conjunction on a minor arcane vessel, ✋. A loved sweet for a few villagers, also edible (small health, Luck +1).
  - **Phiala Stellarum** (Star Phial): Luna + Vitrum, a Ritual at the altar at night, ✋. Hesper, the Wizard, Emily and Abigail are natural fans.
  Each recipe has its own input set, so none collides with an existing recipe (doc 13 §1a). The night condition is allowed because the ritual is hand-triggered.
- **Quest items** (Ectoplasm, and any story delivery doc 08 adds) are ✋ and are only asked for by story quests. Contracts, bounties and workshop bundles never ask for a ✋ item.

### 7a · Animal infusions (Charles, 2026-10-17)
A one-time item fed to a farm animal the same way as **Golden Animal Crackers**; the animal keeps the effect permanently. Charles wants infusions that make animals do or produce things they usually don't, drawing on natural, elemental and magical materials, as long as the flavour fits. The rows below are approved (2026-10-17); numbers are placeholders.

**Guard rails** (Charles, 2026-10-17; these are what relax the plants-only rule):
1. An infusion changes **how an animal lives or what it gives**, never **what it is**: no change of species, body, size or mind, and nothing is created or animated.
2. It is a **tonic, not a transformation**: flavour text speaks of health, vigour and attunement.
3. **One infusion per animal**, in its own slot, separate from the Crackers, so the two stack. A **Panacea** washes it out, so the choice is reversible.
4. A subtle visual tell (a faint shimmer in the infusion's colour), and the animal's info card names the infusion.
5. **Hand-made only** (✋), since each one is a deliberate choice for one animal.

- **Benchmark.** Infusions stack with the Crackers, so produce gains count toward doc 06's caps, including the animal goods that then go into kegs and jars. Early and mid-game infusions are compared with vanilla animal upkeep and the Rancher and Coopmaster professions; strong-tier ones with SVE's late animal items.

**The 1.0 infusions** (Charles, 2026-10-17; each one gives a use to a substance that had none, per his direction to make animals do or produce things they usually don't):

| Infusion | From | Animals | Weak tier | Strong tier adds | Feas. |
|---|---|---|---|---|---|
| **Featherlight** | Pneuma | birds, rabbits | rare drops (Duck Feather, Rabbit's Foot) come more often | a small chance of a second drop | L |
| **Gizzard Stones** | Vitreus | chickens, ducks | now and then leaves a quartz or common mineral with its eggs | rarer gems and geodes | L |
| **Ember Snout** | Magma | pigs | some days digs up Coal or Cinder Shards instead of a truffle | Fire Quartz and Magma Caps too | H (truffle hook) |
| **Tidecaller** | Hydragenum | ducks | after a day outside near water, leaves seaweed, clams or a small fish by the coop door | crab-pot catches and now and then a rarer fish | L–H |
| **Projection** | Mercurius Currens | all | a small chance the produce comes out already processed (milk → cheese, wool → cloth, egg → mayonnaise) | higher, and it can be gold quality | L |
| **Warmblood** | Phlogiston | all | heats its building like a Heater, so the animals near it stay content in winter | a hen sometimes lays a Fried Egg | L |
| **Iron Stomach** | Vitriol | goats | now and then eats a stone and leaves copper or iron ore | gold ore, rarely | L |
| **Vellus Aureum** (Golden Fleece) | Aqua Regia | sheep | wool sometimes comes with a Gold Ore or two | a rare Golden Fleece, a high-value sellable and gift | L |
| **Brimstone Egg** | Sulphur Vivum | chickens | sometimes lays a Brimstone Egg, which decomposes into extra Sulphur and Ignis or crafts into a Cherry Bomb | crafts into a Bomb, and decomposes better | L |
| **Void-touched** | Tenebrae | chickens, ducks | sometimes lays a Void Egg (the bird stays what it is) | Void Essence now and then | L |
| **Kindred Spirit** | Spiritus | all | at full hearts, now and then brings you a forage item, like a pet | rarer finds, and Junimo-favoured items | L |
| **Frostcoat** | Gelum | cows, goats | in winter, milk sometimes comes as Ice Cream | in every season | L |
New items: Brimstone Egg and the Golden Fleece. Iron Stomach, Vellus Aureum, Projection and Ember Snout make money and are checked per animal-day against the animal's normal produce (doc 06); one infusion per animal keeps any one of them from stacking.

- **Data:** an `AnimalInfusion` use {`Animals` (types or tags), `Effects` []}; the animal keeps the infusion ID in its `modData`. Effects are rows in the same registry (§3), with handlers for things like produce timing, produce quality, extra or new produce, and mood.
- **Tech:** a use action on `FarmAnimal`, Panacea's wash-out, and a day-start hook that reads `modData`; vanilla's produce rules in `Data/FarmAnimals` are read, not replaced, and new produce goes through our handler. Multiplayer: the animal's `modData` syncs with its location. Uninstall: the data is inert.

### 8 · Materials in vanilla systems
- **Dyes.** Pigment substances carry vanilla color tags and a strong dye strength, so they work in Emily's dye pots and tailoring with no code: **Cinnabaris** (red) and **Smaltum** (blue).
- **Fuel.** **Bitumen** is a coal substitute: the Calcinator burns it (doc 13), and appended Furnace rules accept it in place of Coal. Both rules give the same bar, so uniqueness isn't affected; when a chest holds both, vanilla rule order uses Coal first.
- **Alternate crafting recipes for vanilla items.** Pulvis Pyrius → Bombs and Mega Bombs; **Tempestas** + Hardwood → a vanilla **Rain Totem** (doc 03a: "links to rain totems"). These add crafting recipes, never change vanilla ones, and follow the cost rule in Decisions locked: never dearer than vanilla, and a discount above about 10% only for a more complex route.
- **Vanilla seeds** from seed transmutation (doc 03a) leave the mod as ordinary seeds.

### 9 · Smaltum: confirmed (Charles, 2026-10-17)
Doc 03a v0.8 asked this doc to confirm Smaltum (Caput Mortuum + Calx) or cut it. **Kept**, because it now has two outside uses that cost no C#:
1. a **strong blue dye** (§8), the classical use of smalt, pairing with Cinnabaris red as a pigment family;
2. the ingredient of a small **glazed décor set** (2–3 crafted Furniture pieces, such as a glazed urn, a glazed planter and a glazed-tile rug), which lets a residue pile end up in the house.

It sells just above its inputs. Cutting it would leave Caput Mortuum with four minor uses, which is still within the approved direction, so the cut is a clean fallback. Décor is Furniture rather than flooring because unknown Furniture becomes an error item on uninstall, like the Circle Sigil, while custom flooring is riskier (doc 17).

### 10 · Bundles, contracts and other sinks (P3)
- **Workshop bundles** (doc 14 owns the list): early bundles ask only for vanilla items; later ones may ask for any automatable product, including automated output. They never ask for ✋ items. ValleyOpus never edits the Community Center's bundles.
- **Qi special orders** (doc 02 proposal) may ask for products in volume or at a quality stage.
- **Ivo's bounties** count kills or ask for vanilla drops (doc 03b), not products.

### 11 · Edge cases
- **Multiplayer:** buffs and custom effects are per player and synced by vanilla; our handlers run on each player's own screen. Fertilizer lives on the tile as vanilla's fertilizer ID; the Radicula's state is its object `modData`.
- **Walk of Life and perks:** perks may change product yield, quality and speed through doc 12's additive-then-clamp rule; no perk changes an effect's strength past the combat cap.
- **SVE and tie-ins:** their items enter as inputs by tag (flowers, honey, fish), so Rosa Vitrea and Mel Vitae accept modded flowers and honey with no patch. Their villagers get gift tastes in doc 20.
- **Joja route:** no product depends on a CC flag; Sorenson's contracts carry more story weight on that route (doc 08), as their non-monetary reward.
- **Uninstall (S3):** products become error items; a buff with a missing ID is dropped; fertilizer with an unknown ID on a tile must do nothing (a doc 17 test item). Trinkets and rings are emptied from slots by the "prepare for removal" command.

## Data shape
All assets are public Content Patcher data under `CharlesDR.ValleyOpus/…`, with `FormatVersion`, namespaced IDs and doc 12's "disable the row, never crash" rule.

| Asset | Kind | Key fields |
|---|---|---|
| `Products` | CP data | `ItemId` (key), `Family` (Consumable / Fertilizer / Combat / Sellable / Gift / AnimalInfusion / Reagent / pack-defined), `Uses` [typed records below], `Price` {`Mode` (Derived / Fixed), `Markup`, `Flat`, `Value`, `ReferenceRecipe`}, `CategoryRole` (Artisan / Reagent / MetalBar), `SoldAt` [Shipping, Finch, Agnes, pack-defined] (where it can be sold; the price is always the shipping price), `AutoCollect` (null = from `Family`: only Reagent goes to the Satchel), `GiftDefault` (Love / Like / Neutral / Dislike / Hate), `UsesKeys` (i18n for the Uses line) |
| `Effects` | CP data | `Id`, `Kind` (VanillaStats / Custom), `Stats` {vanilla buff fields} or `Handler` + `Params`, `Tiers` {`Weak`, `Strong`, pack-defined: {`Strength`, `Augment` (bool or params)}}, `DaySchedule` (optional: weekday → effect ID, for Planetary Influence), `Duration` (minutes, or `Passive`), `Slot` (Food / Drink / Apply / Passive), `IconKey`, `NameKey`, `DescKey`, `CombatBenchmark` (a trinket ID, for the sweep) |
| `Data/Buffs` entries | CP data (generated from `Effects`) | one row per effect; custom ones carry `CustomFields` naming the handler |
| `Data/Objects` entries | CP data (generated from `Products` + doc 12 `Substances`) | `Price`, `Category`, `Edibility`, `IsDrink`, `Buffs`, `ContextTags` (color and dye tags, `valleyopus_reagent` or `valleyopus_product`) |
| `Data/Trinkets` / ring data | CP data | one row per trinket; `TrinketEffectClass` = our passive-effect class; effect IDs in `CustomFields` |
| `Data/CraftingRecipes` | CP data (compiled from doc 12 `Craft` recipes) | alternate recipes for vanilla Bombs, Mega Bombs and the Rain Totem; trinkets, rings, décor |
| `Data/Machines` appends | CP data | Furnace rules with Bitumen as fuel |
| Contracts | CP data | `Id`, `ItemId` or tag, `Quality`, `Count`, `Price`, `UnlockFlag`, story key (doc 08 owns the arc) |
| Effect handler registry | C# | handler ID → class; public API for C# mods |
| Player effect state | C# state, none saved | derived each tick from the player's vanilla buffs |

Typed use records in `Uses`: `AnimalInfusion` {`Animals`, `Effects` []}, `Edible` {`Energy`, `Health`, `IsDrink`, `Effects` []}, `Apply` {`Effects` [], `Requires` (e.g. a weapon)}, `Fertilizer` {`QualityLevel`, `QualityChance`, `SpeedBoost`, `WaterRetention`}, `FieldTreatment` {`Target` (Weeds / pack-defined), `Radius`}, `Equip` {`Slot` (Trinket / Ring), `Effects` []}, `Placeable` {`ObjectId`}, `Material` {`System` (Dye / Fuel / Crafting), `Ref`}, `Gift` {`Taste`}, `Quest` {`QuestKey`}.

**Derived, never stored:** derived prices (recomputed on asset reload), the Uses line, value-add per chain and per tile-day (the sweep), and each combat item's benchmark ratio.

## Content inventory
Working counts for 1.0 (names and numbers are proposals):

**Products by family**
| Family | Products |
|---|---|
| **Consumable** | Sal Volatile (quick small energy), Vigor (large energy), Elixir (health), Motus (Speed +1), Sol (Glow), Panacea (Cleanse), Sal Fixum and Mercurius Fixus (Fixative), Mercurius Currens (Volatile), Luna (Lunar Dream), Ros Maialis (Undine's Gift), Elixir (also Vitality), Sal Marinum (Savour), Aqua Vitae (a sellable spirit with Good Cheer), Midas Draught, Hourglass of Mercury, Mel Vitae (also a gift) |
| **Fertilizer** | Caput Mortuum (weak quality), Terra Viva (quality + retention); field treatment: Mors (weed clearing) |
| **Combat and worn** | Pulvis Pyrius (→ vanilla bombs), Venenum (coating), Cinis Phoenicis (Second Wind), Vial Pendant, Smouldering Censer and Rebis Amulet (weak and strong), Ouroboros Ring, Planetary Band, Ring of the Seven (✋, capstone) |
| **Placed** | Azoth Diffuser, Ever-burning Lamp |
| **Sellable compound** | Vitrum, Gelum, Aqua Regia, Electrum, Rebis, Orichalcum, the planetary metals (to Agnes), Smaltum (also dye and décor), Cinnabaris (also dye), Bitumen (also fuel), Tempestas (also the Rain Totem), plus the bottled Tier 1 substances |
| **Gift** | Rosa Vitrea, Mel Vitae, Phiala Stellarum (all ✋) |
| **Quest** | Ectoplasm (✋) |
| **Animal infusion** | the twelve infusions in §7a (✋) |
| **Placeable** | Radicula (✋, doc 03a): waters or nudges one small crop patch; never better than the sprinkler of its tier |
| **Capstone** | Perpetuum Mobile, the four Tier 3 items: their outside effects belong to doc 19 |

New items this doc adds to the census: 3 gifts, 2–3 décor Furniture pieces, and the trinkets and rings doc 03b already counts. Effects: 3 custom, about 10 vanilla-stat rows.

**Dead-end audit:** all ten substances that had no 1.0 use (Pneuma, Sulphur Vivum, Hydragenum, Mercurius Currens, Magma, Phlogiston, Vitreus, Vitriol, Aqua Regia, Tenebrae) now carry an animal infusion (§7a), and Mercurius Currens also carries Volatile. They stay sellable, and doc 03a's content pass may still give them recipe downstreams (the catalyst ideas in v0.1 of this doc).

## Tech hooks
- **Buffs:** `Data/Buffs` and the `Buffs` field on `Data/Objects` carry every effect; custom handlers key off `CustomFields`. C# reads active buffs each tick (cheap: a handful per player).
- **Fertilizer:** 1.6 hard-codes fertilizer behaviour by item ID, so one Harmony postfix per fertilizer query (quality, speed, retention) returns our data for our IDs. Vanilla placement and one-per-tile rules stay. *Risk:* other fertilizer mods patch the same methods; test with the popular ones (doc 17).
- **Apply and field-treatment actions:** one use-action handler for non-edible products.
- **Trinkets:** one `TrinketEffect` subclass for all our trinkets. **Rings:** Harmony on equip and unequip, or a framework if doc 09 picks one.
- **Dyes:** color and dye-strength context tags (exact tag names to verify against 1.6).
- **Category numbers** and the professions they trigger (Artisan, Blacksmith for bars): verify in doc 09.
- **Prices:** derived prices are computed in C# at load and written into the generated `Data/Objects` rows, so every shop, the shipping bin and Lookup Anything read a plain vanilla price.
- **1.7:** `Data/ContextTags` may let crafting recipes take tags, which would let the Glass Rose become a crafting recipe; nothing here depends on it.

## Balance hooks
Doc 06 metrics: value-add measured end to end from raw inputs; gold per tile-day for each production route to a product; quality-weighted at realistic quality. Placeholders to tune in the sweep: markup bands, `Flat`, Primes' prices, metal prices, residue price, every effect's size and duration, fertilizer numbers, Mors's radius, bomb yields, contract prices. The sweep also reports each consumable against its tier's benchmark (vanilla early and mid, SVE late), each combat item against the trinket benchmark, and the all-six-professions ceiling.

## Open questions
- `[blocks-content]` **Effect and infusion carriers:** which item carries each Prime's bonus, and the recipes for the new items (Midas Draught, Hourglass, the worn and placed items, Brimstone Egg, Golden Fleece). *Owner: doc 03a content pass / doc 14.*
- `[blocks-content]` **Giant's Catalyst and Divining Rod** are approved but have no rows yet. *Owner: content pass.*
- `[blocks-content]` **Effect count:** the 2–3 cap stays suspended until the scale is settled; the approved catalogue (§3a) is the 1.0 list for now. The registry handles any number, so this is content, not shape. *Owner: Charles.*
- `[blocks-content]` **Prime and famous-substance bonuses** from the brainstorm wait in the ideas backlog for Charles's picks. *Owner: Charles.*
- `[blocks-content]` **Satchel auto-collect** takes reagents only; products go to the main inventory (default). *Owner: doc 03b / doc 16.*
- `[blocks-content]` **Gift tastes** for Rosa Vitrea, Mel Vitae and Phiala Stellarum (approved products). *Owner: doc 20.*
- `[blocks-content]` **Capstone effects** (Perpetuum Mobile, Lapis Philosophorum and the other Tier 3 items). *Owner: doc 19.*
- `[2.0]` Forging uses of Orichalcum, Electrum and Magma; custom-effect bombs; brew-map potions; Phlogiston in arcane logistics.

## Change log
- **v0.6 (2026-10-17):** Charles's review: every default he owns approved; "Element" as the umbrella term; selling pays the shipping price everywhere, so per-vendor pricing is gone and Sorenson's contracts must pay more than shipping or give a non-monetary reward; price benchmarks follow the consumables pattern; more drinks than foods, with every food carrying a custom effect; the cost rule for alternate vanilla recipes; Tailwind loses its grass part and its strong tier adds horse speed (approved); Rebis Amulet's strong window moves to noon–3pm and dusk; Ouroboros wording; the infusion table fixed; the environment rule's wrong-day behaviour is waiting idle.
- **v0.5 (2026-10-17):** Charles approved the thematic-effect catalogue (all but the long list): consumable effects, one bonus per Prime, worn and placed items, capstone tiers, and twelve animal infusions, which close the dead-end audit. Balance rules for indirect boosters and profession overlap added.
- **v0.4 (2026-10-17):** animal infusions added as a sixth family, under Charles's five guard rails, which relax the plants-only rule; the control premium; planetary numbers updated to the ideas backlog.
- **v0.3 (2026-10-16):** P2's cap suspended until the effect list's scale is settled; Charles's seven tiered effects (Fixative, Volatile, Lunar Dream, Undine's Gift, Vial Pendant, Smouldering Censer, Planetary Influence on a weak and a capstone ring); effects gain weak and strong tiers.
- **v0.2 (2026-10-16):** Charles's power baselines: vanilla for early and mid game, SVE's consumables and equipment for the top end.
- **v0.1 (2026-10-16):** first draft: families and typed uses, the effect registry on vanilla buffs with three custom effects, fertilizer sidegrades, combat items under the trinket cap, derived prices with end-to-end value-add, buyers, gifts, vanilla-system materials, Smaltum confirmed as dye and décor, and a dead-end audit of doc 03a.
