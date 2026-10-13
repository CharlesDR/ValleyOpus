# ValleyOpus Inspiration: Wider Alchemy Media

Research thread, 2026-10-13. **Revision 3** (Charles's review, 18:10). Media *not* already covered in [comparators-and-concepts.md](comparators-and-concepts.md) (which has Thaumcraft, Witcher 3, Opus Magnum, Potion Craft, FMA, Final Fantasy, Little Alchemy, Atelier, Alchemy Factory and the Stardew alchemy mods).

**Naming and reference rule (Charles; now the Lore & Worldbuilding pillar in doc 01):** everything here is inspiration only. No names from these works (characters, creatures, items, places, terms they coined) or from real people go into the mod. Borrow the idea and give it our own name. Any nod to pop culture or a real historical figure (e.g. Flamel) is **flavour text only**: item descriptions, codex lore, readables. Never in-game dialogue, and never an explicit character reference. See [inspiration-lore-and-npcs.md](inspiration-lore-and-npcs.md).

Verification key: **(v)** checked against a source this session; **(bk)** background knowledge, not re-checked. ★ = top pick.

---

## Games

| Title | What it does | Lesson / steal |
|---|---|---|
| ★ **Kingdom Come: Deliverance II** (v) | Brewing is hands-on: follow the Herbarium's steps, set flame with bellows, time with a sand-glass, grind with mortar and pestle. Mistakes ruin the brew. At skill 10 the **Routine** perk auto-brews any potion you've brewed by hand before; Routine II gives 3 for 1. | **The exact bridge we need between discovery and automation:** you must make something by hand once before a machine (or Automate) will produce it. Doc 07's codex already records firsts; making that the automation unlock is a small, strong rule. **Adopted (Charles):** automation can't make discoveries and requires them to function (docs 04, 07). |
| ★ **Wylde Flowers** (v) | Farming life-sim where you brew spells in a basement cauldron; spells drive the story; a levitation potion enables broom fast-travel. | The closest genre neighbour. Proves potions with **world utility** (travel, story) beat sell-value potions. Supports pillar "outputs matter outside the mod". |
| **Witchbrook** (Chucklefish) (v) | Magic-school life sim, still unreleased; planned for 2026 with no date as of a June 2026 dev blog. | Watch it at launch: it will set player expectations for magic in pixel-art life sims. |
| **Skyrim** (bk) | Eating an ingredient reveals its first effect; more effects come through brewing. | A cheap, cosy discovery channel: **eating an item reveals its strongest Prime** (an alternative to the Assayer's Lens early on). |
| **Cultist Simulator / Book of Hours** (Weather Factory) (bk) | Every card carries **aspects** (Lantern, Forge, Edge…) with numbers; recipes ask for aspect totals, not specific cards. | The same idea as our tag→Prime profiles, executed beautifully. Validates "recipes ask for Prime amounts, any source will do". Their prose style is a model for one-line codex flavour. |
| **Potionomics** (bk) | Potion shop where ingredients carry "magimin" values that must hit ratios; selling is a card-based haggle. | Ratio-based recipes are readable; our Primes could show as a simple bar with a target zone. |
| **Potion Permit** (bk) | You're the town's chemist; diagnose villagers and brew cures, which raises their trust. | NPC sick-day requests as a friendship hook (Harvey the doctor tie-in). |
| **Graveyard Keeper** (bk) | Alchemy from bodies and herbs; gritty, morbidly funny. | Tone reference for the Witch's dark substances: funny, not grim. **Led to Charles's Gloam Lantern idea** for fuelling the Witch's dark magic (see below). |
| **Noita** (v, see discovery file) | Per-seed secret recipes. | Already proposed for the endgame secret. |
| **Dragon Quest** Alchemy Pot / **Ni no Kuni** (bk) | Drop items in, wait, get a surprise. | Charm of a "mystery pot" as a low-stakes early toy. |

## Gloam Lanterns for the Witch's dark arts (Charles's idea, from Graveyard Keeper)

Named by Charles: the **Gloam Lantern**, which collects **Shades**.

- **Filling:** a Gloam Lantern in the player's main inventory (not the Reagent Satchel, so carrying it costs a real slot) slowly fills as monsters die nearby, in the Mines, Skull Cavern, Volcano or anywhere else. It adds no new monster drop, so the inventory rule holds.
- **Several lanterns, diminishing returns:** each kill's Shades is split across all carried lanterns with a falloff. One lantern gets 1 per kill. Two get 0.75 each (1.5 total), matching Charles's example. A clean formula is *per lantern = 0.75^log₂(n)*: three lanterns get ≈0.63 each (≈1.9 total) and four get ≈0.56 each (2.25 total). More slots spent means faster harvesting, at a falling rate.
- **Capacity and upgrades:** a base lantern holds a fixed amount. Higher-quality lanterns unlocked later hold more (and could fill a little faster).
- **Use:** full or partly full lanterns fuel **higher-intensity dark magic and alchemy**: the Witch's rituals, the dark twins (Tenebrae, Mors, Perditio) at higher tiers, and Putrefaction Vessel or altar boosts. The charge is spent, the lantern kept.
- **Synergy:** some potions give a % boost to Shades gathered, so combat players can invest in faster harvesting. This gives combat a real alchemy role without new drops.
- **Multiplayer:** each player's carried lanterns fill from monster deaths near *them*, whoever landed the kill, so co-op parties don't compete.
- **Tone:** keep it Stardew-friendly. The lanterns gather the monsters' lingering Shades, not people's souls, and the Witch's dialogue can treat them matter-of-factly and a little spooky.

## Minecraft mods (the closest technical cousins)

| Mod | What it does | Lesson / steal |
|---|---|---|
| ★ **Botania** (v) | Magic powered by **flowers**: generating flowers make mana, functional flowers spend it; an in-game book (Lexica Botania) explains everything. | **Approved by Charles as thematic inspiration for part of the Mystic branch.** A farming-native magic economy, the best fit for Stardew: arcane apparatus fed by specific *planted* flowers next to them (adjacency), so the Mystic branch lives partly in the garden, not only the factory. **Do not reuse Botania's names** (its flower names, "mana", "Lexica", "Petal Apothecary" and similar): our flowers, the energy they give and any guide book get their own names, ideally drawn from the eight Primes. **Why it's so good (Charles):** it gives players a reason for a base-game activity most skip. Without the mod, bulk flowers are only honey boosters and occasional gifts. This became a design **guiding star** in doc 01: boost underwhelming base-game activities and items. |
| ★ **Ars Nouveau** (bk) | Glyph-based spells; **Starbuncles**, small creatures you bind to move items between chests. | Cute creature logistics = our **arcane logistics** answer, and fits the Four Familiars / Junimo tone. Idea only; no "Starbuncle"-like name. |
| **Create** (bk) | Visible spinning shafts, cogs, belts, water wheels. Machines you can *watch*. | Visual language for the Mechanist branch: show motion, not boxes. |
| **ProjectE** (bk) | Every item has an EMC value; transmute anything into anything. | The cautionary case: a single universal currency flattens everything (and it's what Alchemistry for Stardew copied). Our eight-Prime profiles avoid this; keep it that way. |
| **Astral Sorcery** (bk) | Constellations visible only at night or in certain moon phases gate rituals. | Night-sky rituals for the Astrologer; pairs with the planetary weekdays. |

## Books, anime and film

| Work | Steal |
|---|---|
| ★ **Witch Hat Atelier** (manga) (bk) | Magic is drawn with ink, and the "secret" is that anyone can learn it. The warmest possible framing for Oriel teaching a farmer. Strong tone match for Stardew. |
| **Howl's Moving Castle** (bk) | Calcifer, a fire demon, powers a walking castle: magic fuelling machinery. A great image for **Living Flame as fuel for Tobin's machines** and a possible fire Familiar's personality (with its own name). |
| **Dr. Stone** (bk) | The joy of *firsts* (first glass, first soap, first light bulb) rebuilding civilisation. Tone for the Mechanist's restoration of the workshop. |
| **Frieren** (bk) | Magic as patient, centuries-long study; a mentor who has outlived her teachers. Tone for Cyprian and Oriel. |
| **Jonathan Strange & Mr Norrell** (bk) | Two rival magicians reviving English magic. Tone for Oriel and the Wizard: a pupil who went beyond her teacher. |
| **The Alchemist** (Coelho) (bk) | "Personal legend"; the treasure was at home. A gentle theme for the ending: the Great Work is the farm itself. |
| **Harry Potter and the Philosopher's Stone** (bk) | Flamel and the Stone are pop-culture shorthand. Allowed only as a flavour-text nod, never by name in dialogue. |
| **Discworld**'s Alchemists' Guild (bk) | Famous for blowing up its own guildhall. Comic register for failure "puffs". |

## Art (for the later art brief)
- **Joseph Wright of Derby, *The Alchymist, in Search of the Philosopher's Stone, Discovers Phosphorus*** (1771) (bk): Brand kneeling in the glow. Lighting reference for the moment of discovery.
- **Splendor Solis**, the **Ripley Scroll**, **Mutus Liber** (bk): emblem art for codex pages and workshop murals.
- **Bruegel's *The Alchemist*** and **David Teniers'** lab paintings (bk): cluttered, homely labs; the right register for a farm lab.

## Guiding star (Charles)
**Boost underwhelming base-game activities and items.** Flowers in bulk are the model. Other candidates worth checking in the balance sweep (bk): fiber, sap, common forage, low-value fish and trash, most monster loot, and moss. Each is cheap and plentiful, and alchemy can make it matter.

## Top recommendations
1. **KCD2's "brew it once by hand, then it can be automated" rule.** Small, testable, and it makes discovery the gate for automation.
2. **Flower-fed arcane apparatus (Botania-inspired, approved; in doc 04).** Puts part of the Mystic branch in the garden, with our own flower names.
3. **Ars Nouveau-style creature logistics** for arcane item transport.
4. **Witch Hat Atelier** as the tone reference for Oriel.

## Sources
- [Kingdom Come Deliverance Wiki: Alchemy](https://kingdom-come-deliverance.fandom.com/wiki/Alchemy)
- [Games Asylum: Wylde Flowers review](https://www.gamesasylum.com/2022/10/22/wylde-flowers-review/)
- [MeowDB: Witchbrook release date](https://meowdb.com/db/witchbrook/release-date) and [Game Informer: Witchbrook delayed to 2026](https://gameinformer.com/2025/10/15/wizarding-life-sim-witchbrook-delayed-to-2026)
- [FTB Wiki: Botania](https://ftb.fandom.com/wiki/Botania)
