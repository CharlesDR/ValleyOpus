# ValleyOpus: Comparators & Concepts Research

Researched 2026-10-13, redoing the research behind the original Chat conversation (`ValleyOpus Startup Context.rtf`). Facts marked *(verified)* were checked against a source this session; others are well-established background knowledge. Corrections to the original Chat's claims are called out explicitly.

---

## 1. Stardew Valley platform (what we build on)

**Game version.** Latest released PC version is **1.6.15** (20 Dec 2024) *(verified, wiki Version History)*. **1.7 is announced (Sept 2025) but has no release date** *(verified, wiki migration page)*. 1.6.16 does not appear as released.

**1.7 migration notes worth designing around** *(verified)*: crafting/cooking recipes become data models (`Ingredients`, `Result`, `Unlock`); new `Data/ContextTags` asset (tags used as crafting ingredients need `DisplayName` + `RepresentativeItem`); `Data/Objects` gains `UseActions`/`ObtainActions`/`UseCondition`; new `Data/Enchantments`; monsters become data-driven with drops in data. Implication: **tag-based ingredients become first-class in 1.7**, which strongly favors the context-tag design. Keep recipe definitions isolated so the 1.7 recipe-format migration is cheap.

**`Data/Machines` (1.6)** *(verified)*, enough to make most machines pure data:
- `OutputRules` → `Triggers` (`ItemPlacedInMachine`, `OutputCollected`, `MachinePutDown`, `DayUpdate`), matched by `RequiredItemId` / **`RequiredTags`** (supports `!negation`) / `RequiredCount`, gated by a **`Condition` game-state query** (GSQs include day of week, season, weather, location, time, so the planetary-weekday idea is doable in data).
- `AdditionalConsumedItems` = fuels/catalysts.
- Flavored outputs: `PreserveType`/`PreserveId: DROP_IN`, `ObjectInternalName: "{0} ..."`, `CopyPrice` + `PriceModifiers`, `CopyQuality` + `QualityModifiers`, `CopyColor`. This is how "[Crop] Tincture" works for any modded crop with zero patches.
- `MinutesUntilReady`/`DaysUntilReady`, `ReadyTimeModifiers`, `PreventTimePass` (Inside/Outside/season/Sun/Rain), `OnlyCompleteOvernight`.
- C# escape hatches: `OutputMethod` (custom output logic, e.g. essence decomposition), `InteractMethod`, `CustomData`. Hybrid approach: data for the 80%, C# hooks for the special cases.
- `ExperienceGainOnHarvest` takes vanilla skill names; custom-skill XP needs C#.

**Context tags** *(verified)*: every item carries tags like `category_*`, `color_*`, `edible_*`, `forage_item`, `fish_*`, `season_*`. This is the decomposition and compatibility engine. 1.7's `Data/ContextTags` makes it better.

**Framework mods.**
- **Content Patcher**: the #1 Stardew mod. Default delivery for data.
- **SpaceCore** (spacechase0, v1.28.4, Oct 2025, used by ~787 mods) *(verified)*: the standard custom-skill framework. **SpaceCore custom skills don't support vanilla Mastery**, so Walk of Life can *reset* a SpaceCore skill but **cannot prestige it** *(verified on WoL page)*. Our prestige perks will need our own implementation or a WoL integration path.
- **Extra Machine Configs (EMC, selph)**: extends `Data/Machines` with extra fuels, byproducts, and rule-specific outputs. Used by other alchemy mods. Optional dependency candidate, but prefer vanilla + our own C#.
- **Machine Control Panel** (mushymato): players toggle machine rules. Note it is incompatible with Producer Framework Mod machines. **Avoid PFM**; 1.6 machines replace it.

**Automate** (Pathoschild, v2.6.1, Mar 2026) *(verified)*: supports custom `Data/Machines` machines (2.0.0), custom floors/paths as connectors (2.0.0), multiple machines per tile (2.0.6). Machine groups are flood-filled from adjacent machines, chests and connectors; connected chests act as one inventory. C# API: `IAutomateAPI.AddFactory(IAutomationFactory)` → our own `IMachine` (`GetState`/`GetOutput`/`SetInput`). **Returning `null` from a factory opts an entity out of automation**, which is how Mystic machines can deliberately resist automation (or accept it with penalties). Chests opt in via the `automate_storage` context tag.

**Nexus popularity** *(verified, Nexus "top all time" page; figures look like endorsements, not downloads)*: Content Patcher, SMAPI, NPC Map Locations, GMCM, SpaceCore, CJB Cheats, **SVE (#7)**, Farm Type Manager, Lookup Anything, CJB Item Spawner, Json Assets, Skull Cavern Elevator, Expanded Preconditions Utility, Tractor Mod, **Automate (#15)**, … Content mods for bespoke integration (SVE, Ridgeside, East Scarp, Cornucopia) need a separate ranking of content mods for current-version downloads; Nexus blocks automated access from this environment, so that sweep needs a manual export or the browser.

---

## 2. Stardew mod comparators

**Stardew Valley Expanded (FlashShifter).** The quality and tone bar: new areas, NPCs, forage, crops that still feel vanilla. Its ingredients are natural alchemy inputs. Lesson: **thematic restraint, Stardew-native art and writing**. (Wizardry's own page notes SVE raises the Wizard heart requirement, a small example of cross-mod gating we must handle.)

**Walk of Life – Profession Overhaul** *(verified, Nexus 24355; current listing by Arleau, v1.5.0-Beta5, Sept 2026; requires DaLionheart + SpaceCore)*. Originally DaLion's (also author of MARGO). Features: reworked L5/L10 professions that change *how* you play; **Prestige** (levels 11–20, prestige professions at 15/20, on by default); **Skill Reset** via Statue of Transcendence (optional, off by default) letting players own every profession; **Limit Breaks** (combat); Mastery locks resets. Lessons for us:
- Professions as playstyle identity, not +% bonuses (e.g. Heavy Tapper reworked to double yield rather than speed).
- **All Mechanist/Mystic perks must coexist safely** since resets let a player own every one (Charles's explicit requirement).
- Prestige versions of our perks need our own system or integration, because SpaceCore skills can't be prestiged through WoL today. Nothing important can be locked behind WoL.

**Wizardry – A Magical Skill Mod** *(verified, Nexus 38383; MoonSlime's port of spacechase0's archived Magic mod; v1.11.14, June 2026)*. Charles cited it as the soft-start reference. **Correction:** its unlock is **2 hearts with the Wizard (5 with SVE)**, which triggers a cutscene; the skill unlocks the next morning. It isn't tied to the Community Center, so the "few days after the CC opens" start is our own design. Other transferable ideas: **spells learned by "Analyzing" objects, places and people** (discovery-as-progression, very Zaikin), hints on the Wizard's radio, and craftable spell scrolls that let non-specialists use effects without XP. Uses SpaceCore + its own mana-bar API, and isn't split-screen safe.

**Existing Stardew alchemy mods (the competitive landscape)** *(verified)*:
- **Alchemy of the Valley** (ILlIDAN8905, v1.0.5, Oct 2026): SpaceCore Alchemy skill (Herbalist/Potioneer → Blender/Nature Alchemist), Cauldron/Alembic/Mortar, **7 essences (Life, Mana, Fire, Ice, Shadow, Earth, Air)**, 23 potions, Philosopher's Stone, guild contracts. **Intentionally not Automate-compatible.**
- **Alchemistry – A Witchy Gameplay Expansion** (Morghoula, v2.0.2, Aug 2026): Cauldron, Tincture Bin, Dehydrator, Mortar & Pestle, Heavy Furnace, Rune Carver, Transmuter Coil; the Wizard as mentor; new NPC and map; requires EMC, FTM, Better Crafting, etc. Author asks for no derivatives.
- **Alchemist's Valley** (Dolly, 2023): ~45 potions via a cauldron; PFM/Json Assets era.
- **Our differentiation:** none of these are automation-first, tag-driven for universal compatibility, or built around mechanical-vs-magical tradeoffs. Two already use "Alchemy" as a skill name and generic cauldron/alembic/mortar machines, so **name our skill and machines distinctively**, avoid ID collisions, and test side-by-side compatibility with them.

---

## 3. Factory-game comparators

**Factorio.** Ratios, throughput bottlenecks, logistics as a spatial puzzle, **byproducts that force decisions** (oil cracking is the classic case), and blueprinting. Lesson: waste streams and ratio balancing create depth. Caution: pure throughput maximization is exactly the linear escalation Charles wants to avoid.

**Satisfactory.** **Alternate recipes** (found via hard drives in crash sites) are sidegrades that reshape whole chains, which is the direct model for our "sidegrades over upgrades" pillar. Exploration feeds production unlocks.

**Alchemy Factory** *(verified; D5 Copperhead / Gamirror Games; 1.0 on 8 Sept 2026 after Early Access with 200k+ sales)*. Compact voxel factory plus a shop loop, pipes for magical liquids, portals and wagons for logistics, Philosopher's Stone endgame. **The source of the cautionary critique is NeonLightsMedia's review (8/10):** "lacks the 'wonder' of alchemy", "a medieval sweatshop simulator", "I want to feel like a wizard, not a shift manager at an Amazon fulfillment center", plus early manual tedium and scaling into "a plantation". (Other reviews, e.g. GameCritix 4.5/5, praise its charm, so treat it as one sharp warning, not consensus.) Lessons: **keep wonder at the conjunction points**, avoid early manual drudgery, and make sure demand doesn't force monoculture farming.

---

## 4. Alchemy-game comparators

**Opus Magnum** (Zachtronics, 2017). Hex-grid "transmutation engines" built from arms and glyphs (bonding, calcification into salt, projection of metals with quicksilver, lead-to-gold). Solutions are scored on **cost, cycles and area** with no single best answer. Its element set draws on real alchemy (the four elements, salt, quicksilver, planetary metals, vitae and mors). Lesson: multi-axis scoring and layout as composition; adjacency mechanics are our analog.

**Potion Craft: Alchemist Simulator** (niceplay games, 2022) *(verified)*. Every brew starts at the map's center; **each ingredient pushes it along its own path**; **grinding extends the push**; hazards fail the brew; bellows/heat "locks in" an effect when you reach it; salts (unlocked by upgrades) bend or erase the path; the map is revealed by experimenting and recipes are saved for rebrewing. Lessons: ingredients as vectors, preparation (grinding) as a skill lever, and save-and-replay recipes as the bridge to automation.

**Alchemy by Andrey Zaikin** (Android, ~2010). Start from Fire/Water/Earth/Air and combine pairs to discover hundreds of elements; the codex fills by experimentation. It's the ancestor of Little Alchemy and Doodle God. (Sources on the original app are thin online, so this is background knowledge.) Lessons: discovery is the reward, failed combos cost nothing, and the codex is the progression UI.

---

## 5. Real alchemical concepts in play

- **Four elements** (Fire, Water, Earth, Air) plus **Aether/quintessence**.
- **Tria prima** (Paracelsus): **Salt** (body), **Sulfur** (soul), **Mercury** (spirit).
- **Seven planetary metals ↔ seven weekdays**: Sun/gold/Sunday, Moon/silver/Monday, Mars/iron/Tuesday, Mercury/quicksilver/Wednesday, Jupiter/tin/Thursday, Venus/copper/Friday, Saturn/lead/Saturday. Stardew already has copper, iron and gold (plus iridium and radioactive ore as non-classical extras) and a 7-day week; `DAY_OF_WEEK` GSQs make planetary-day machines pure data.
- **Seven operations** of the Magnum Opus (one common list): Calcination, Dissolution, Separation, Conjunction, Fermentation, Distillation, Coagulation.
- **Colour stages**: nigredo (black), albedo (white), citrinitas (yellow), rubedo (red). These map onto quality tiers.
- **Caput mortuum**: the "dead head" residue after distillation, our byproduct and waste mechanic.
- **Apparatus vocabulary**: athanor (slow, steady furnace), alembic, retort, crucible, pelican (circulating vessel), aludel, bain-marie.

**Vanilla Stardew already has alchemical hooks**: Solar/Void Essence, Fire Quartz, Frozen Tear, Prismatic Shard, Cinder Shards, slime and monster loot, the Wizard (Rasmodius), the Witch, Junimos and Forest Magic, and the Dwarves. Use these as essence sources and story anchors rather than inventing parallel versions.

---

## 6. Design takeaways (carry forward)

1. **Tag-first everything.** Inputs by context tag, outputs as flavored items (`DROP_IN` + `{0}` names + `CopyPrice`). 1.7's `Data/ContextTags` reinforces this.
2. **Data-first, C#-where-it-earns-it.** `Data/Machines` + GSQs for most machines; C# for decomposition math, adjacency, the brew map, lab passive effects, custom XP, and Automate factories.
3. **Automate is a first-class target**, with deliberate, explicit exceptions (factory returns `null`, or accepts automation with a penalty) for Mystic apparatus. Alchemy of the Valley's blanket "no Automate" is the opposite of our stance.
4. **Avoid the Alchemy Factory trap**: wonder at the magical conjunction points, no early drudgery, no monoculture pressure. This connects to Charles's rule that no element should hinge on a single crop type.
5. **Sidegrades (Satisfactory alternates) and multi-axis tradeoffs (Opus Magnum)** over linear tiers.
6. **Discovery as progression** (Zaikin, Wizardry's Analyze, Potion Craft's map) with a non-punishing failure state and in-world hints.
7. **WoL-safe professions**: every perk combination must coexist, prestige is our own concern because SpaceCore skills can't be WoL-prestiged, and nothing important is locked behind WoL.
8. **Differentiate from the existing alchemy mods** in name, IDs and pillars, and test side-by-side with Alchemy of the Valley and Alchemistry.
9. **Design for 1.6.15 now; isolate recipe and tag data for the 1.7 migration.**

## Sources
- [Modding:Machines](https://stardewvalleywiki.com/Modding:Machines) · [Modding:Context tags](https://stardewvalleywiki.com/Modding:Context_tags) · [Migrate to 1.7](https://stardewvalleywiki.com/Modding:Migrate_to_Stardew_Valley_1.7) · [Version History](https://stardewvalleywiki.com/Version_History)
- [Automate technical docs](https://github.com/Pathoschild/StardewMods/blob/develop/Automate/docs/technical.md) · [Automate release notes](https://github.com/Pathoschild/StardewMods/blob/stable/Automate/docs/release-notes.md)
- [SpaceCore](https://www.nexusmods.com/stardewvalley/mods/1348) · [Machine Control Panel](https://nexusmods.com/stardewvalley/mods/28261) · [Nexus top all time](https://www.nexusmods.com/stardewvalley/mods/topalltime)
- [Walk of Life](https://nexusmods.com/stardewvalley/mods/24355) · [Wizardry](https://www.nexusmods.com/stardewvalley/mods/38383)
- [Alchemy of the Valley](https://www.nexusmods.com/stardewvalley/mods/53528) · [Alchemistry](https://www.nexusmods.com/stardewvalley/mods/22743) · [Alchemist's Valley](https://www.nexusmods.com/stardewvalley/mods/19284)
- [Alchemy Factory 1.0 (G2A)](https://www.g2a.com/news/game-spotlights/alchemy-factory-1-0-turns-medieval-alchemy-into-a-factorio-style-production-line/) · [NeonLightsMedia review](https://www.neonlightsmedia.com/blog/alchemy-factory-is-a-dangerous-cocktail-of-cozy-shop-management-and-hardcore-industrial-automation-that-will-absolutely-ruin-your-sleep-schedule) · [GameCritix review](https://gamecritix.co.uk/alchemy-factory-review/) · [VaporLens summary](https://vaporlens.app/app/3669570/alchemy_factory)
- [Opus Magnum (Wikipedia)](https://en.wikipedia.org/wiki/Opus_Magnum) · [Potion Craft overview (HackerNoon)](https://hackernoon.com/a-look-at-potion-craft-an-alchemist-simulator)
