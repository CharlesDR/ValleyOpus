"""The 1.0 content on top of the slice (docs 03a, 13, 14, 15, 18, 05, 07, 08a), as plain data.

build_base_pack.py reads slice_data.py first, then this file. IDs are permanent once shipped (doc 09 §11): never
rename an ID here. Every number is a placeholder for the balance sweep (doc 06)."""

P = "CharlesDR.ValleyOpus_"

# ── Apparatus (doc 13 §3) ────────────────────────────────────────────────────────
# Neighbour states each apparatus can be in (doc 13 §1a). Tests: {} = always, Heat True/False, Power [kinds], None.
STATES = {
    "Open": {},
    "Cold": {"Heat": False},
    "Heated": {"Heat": True},
    "NoPower": {"None": True},
    "Vapor": {"Power": [P + "Vapor"]},
    "Fulgur": {"Power": [P + "Fulgur"]},
    "Crossed": {"Power": [P + "Vapor", P + "Fulgur"]},
}

# id, display, kind, vanilla Craftables sprite (placeholder by reference), states, {operation: [states]}, description
APPARATUS = [
    ("ExperimentTable", "Experiment Table", "Table", 208, ["Table"], {}, "Try any two things together. Nothing is wasted that teaches you something."),
    ("Athanor", "Athanor", "MinorArcane", 13, ["Open"], {"Calcination": ["Open"]}, "A slow furnace. Takes things apart into their Primes. Feed it Ignis and Ignis to kindle it."),
    ("Alembic", "Alembic", "MinorArcane", 12, ["Cold", "Heated"], {"Dissolution": ["Cold"], "Distillation": ["Heated"]}, "Dissolves when cold. Beside a kindled Athanor or a Boiler, it distills."),
    ("Retort", "Retort", "MinorArcane", 15, ["Open"], {"Separation": ["Open"], "Coagulation": ["Open"]}, "Separates and coagulates. Flowers nearby improve its work."),
    ("HermeticVessel", "Hermetic Vessel", "MinorArcane", 21, ["Open"], {"Conjunction": ["Open"], "Fermentation": ["Open"]}, "The sealed egg: joins things in the light, and rots them in the dark."),
    ("Calcinator", "Calcinator", "Engineered", 114, ["Open"], {"Calcination": ["Open"]}, "A coal-fired furnace. Fast decomposition, with more residue. Burns Coal or Bitumen."),
    ("ProcessVat", "Process Vat", "Engineered", 163, ["NoPower", "Vapor", "Fulgur", "Crossed"],
     {"Dissolution": ["NoPower"], "Fermentation": ["NoPower"], "Distillation": ["Vapor"], "Separation": ["Fulgur"]},
     "Tobin's vessel. Dissolves and ferments; beside a Boiler it distills, beside a Lightning Coil it separates. Both at once: nothing."),
    ("Press", "Press", "Engineered", 16, ["Open"], {"Conjunction": ["Open"], "Coagulation": ["Open"]}, "Alloys and moulds. Joins and sets things, in bulk."),
    ("Boiler", "Boiler", "Engineered", 19, ["Open"], {"Distillation": ["Open"]}, "Feed it Ignis and Aqua: it steams for most of a day, powering and heating up to four neighbours."),
    ("LightningCoil", "Lightning Coil", "Engineered", 9, ["Open"], {"Conjunction": ["Open"]}, "Stores Fulgur as charges. Storms charge it for free. Feed it Ignis and Aer."),
    ("Pelican", "Pelican", "MinorArcane", 20, ["Open"], {"Distillation": ["Open"]}, "Bennet's glass. Circulates a substance with Sal Fixum, raising its quality, up to gold."),
    ("RitualAltar", "Ritual Altar", "RitualAltar", 160, ["Open"],
     {op: ["Open"] for op in ["Calcination", "Dissolution", "Separation", "Conjunction", "Fermentation", "Distillation", "Coagulation", "Ritual"]},
     "Stage a rite on it, then click it with an empty hand to perform it. Ritual quality goes up to iridium."),
    ("QuarryCrucible", "Quarry Crucible", "Specialist", 182, ["NoPower", "Fulgur", "Vapor", "Crossed"], {"Conjunction": ["NoPower", "Fulgur"]},
     "Agnes's crucible. Grows the planetary metals on their days; beside a Lightning Coil, Orichalcum."),
    ("LeyTap", "Ley Tap", "Specialist", 105, ["Open"], {}, "Placed on a ley node, it draws that node's Primes each morning."),
]

# The Ritual Circle is the altar with a Circle Sigil under it (doc 13 §2 "Arrangement"): its own row, no item.
CIRCLE = ("RitualCircle", "Ritual Circle", "RitualCircle", {op: ["Open"] for op in ["Calcination", "Dissolution", "Separation", "Conjunction", "Fermentation", "Distillation", "Coagulation", "Ritual"]})

APPARATUS_EXTRA = {
    "Athanor": {"Decompose": {"Concentration": 2, "ResidueRate": {"Count": 1, "Chance": 0.1}, "Minutes": 30},
                "Heat": {"RequiresFlag": "Kindled", "MaxConsumers": 2}},
    "Calcinator": {"Decompose": {"ResidueRate": {"Count": 1, "Chance": 0.35}, "Minutes": 10},
                   "Fuel": {"Item": "(O)382", "Alternates": ["(O)" + P + "Bitumen"], "Capacity": 10, "PerRun": 1}},
    "Boiler": {"Power": {"Emits": P + "Vapor", "MaxConsumers": 4, "Mode": "Duration", "Amount": 840},
               "Heat": {"RequiresFlag": None, "MaxConsumers": 2}},
    "LightningCoil": {"Power": {"Emits": P + "Fulgur", "MaxConsumers": 4, "Mode": "Charges", "Amount": 5, "StormCharge": True}},
    "Pelican": {"QualityCap": 2},
    "RitualAltar": {"Trigger": "Hand", "Automation": "StageOnly", "Chaining": False, "TrayCapacity": 4, "QualityCap": 4},
    "QuarryCrucible": {"TrayCapacity": 2},
    "LeyTap": {"Automation": "None", "Chaining": False, "TrayCapacity": 0},
}

# Every operation's runners outside the Table: (apparatus, state). "Eng" rows only take ⚙︎ recipes.
OP_RUNNERS = {
    "Calcination": {"arc": [("Athanor", "Open")], "eng": [("Calcinator", "Open")]},
    "Dissolution": {"arc": [("Alembic", "Cold")], "eng": [("ProcessVat", "NoPower")]},
    "Distillation": {"arc": [("Alembic", "Heated")], "eng": [("ProcessVat", "Vapor")]},
    "Separation": {"arc": [("Retort", "Open")], "eng": [("ProcessVat", "Fulgur")]},
    "Coagulation": {"arc": [("Retort", "Open")], "eng": [("Press", "Open")]},
    "Conjunction": {"arc": [("HermeticVessel", "Open")], "eng": [("Press", "Open")]},
    "Fermentation": {"arc": [("HermeticVessel", "Open")], "eng": [("ProcessVat", "NoPower")]},
    "Ritual": {"arc": [], "eng": []},
}

# Tier 1 apparatus marks (doc 03a): T table, E engineered, A minor arcane, R ritual altar.
TIER1_MARKS = {
    "FlammaViva": "TA", "RosMaialis": "TEA", "Saxum": "TEA", "Pneuma": "TEA", "SalFixum": "TEA", "SulphurVivum": "TEA",
    "MercuriusCurrens": "TEA", "QuintaEssentia": "TR", "Vapor": "T", "Magma": "TEA", "Fulgur": "T", "Limus": "TEA",
    "Tempestas": "TA", "Pulvis": "TEA", "Calx": "TEA", "Quebrith": "TEA", "Phlogiston": "TA", "SalMarinum": "TEA",
    "AquaFortis": "TEA", "Hydragenum": "TEA", "Vitreus": "TEA", "Bitumen": "TEA", "Metallum": "TEA", "SalVolatile": "TEA",
    "Miasma": "TEA", "Motus": "TA", "Sol": "TA", "Luna": "TA", "Vitae": "TA", "Spiritus": "TA", "Vitriol": "TEA",
    "MercuriusFixus": "TAR", "Cinnabaris": "TEA", "Ordo": "TA", "Anima": "TA", "MercuriusPhilosophorum": "TR",
}
# Power substances' working sources (doc 13 §5).
POWER_SOURCES = {"FlammaViva": ("Athanor", "Open"), "Vapor": ("Boiler", "Open"), "Fulgur": ("LightningCoil", "Open")}

# ── Conditions (doc 12 §4, doc 13 §5) ───────────────────────────────────────────
CONDITIONS = {
    "Darkness": {"Kind": "Gsq", "Query": P + "DARKNESS", "PlayerControlled": True,
                 "name": "Darkness", "hint": "This wants to rot where no light reaches: a Lab alcove, the Undercroft, a cave."},
    "PowerVapor": {"Kind": "Power", "Power": P + "Vapor", "name": "Steam", "hint": "Something here needs steam from a Boiler beside it."},
    "PowerFulgur": {"Kind": "Power", "Power": P + "Fulgur", "name": "Lightning", "hint": "Something here needs a charged Lightning Coil beside it."},
    "PowerFlamma": {"Kind": "Power", "Power": P + "FlammaViva", "name": "Living flame", "hint": "Only a kindled Athanor's own fire will do."},
    "Monday": {"Kind": "Gsq", "Query": "DAY_OF_WEEK Monday", "name": "Monday (the Moon's day)", "hint": "Silver grows on the Moon's day."},
    "Thursday": {"Kind": "Gsq", "Query": "DAY_OF_WEEK Thursday", "name": "Thursday (Jupiter's day)", "hint": "Tin grows on Jupiter's day."},
    "Saturday": {"Kind": "Gsq", "Query": "DAY_OF_WEEK Saturday", "name": "Saturday (Saturn's day)", "hint": "Lead grows on Saturn's day."},
    "SunOrMoonDay": {"Kind": "Gsq", "Query": "DAY_OF_WEEK Sunday Monday", "name": "Sunday or Monday", "hint": "The Sun and the Moon wed only on their own days."},
    "Night": {"Kind": "Gsq", "Query": "TIME 1800 2600", "name": "Night", "hint": "Some things are only done under the stars."},
    "LabHearth": {"Kind": "Adjacency", "Adjacency": {"Tag": "valleyopus_tile_heat", "Directions": "All", "Range": 1}, "PlayerControlled": True,
                  "name": "A Lab hearth", "hint": "A hearth's warmth helps a calcination along."},
    "Shades": {"Kind": "Catalyst", "Catalyst": {"Shades": 1}, "name": "Shades", "hint": "Carry a Gloam Lantern with a Shade in it."},
}

# ── Tier 2 and beyond (doc 03a): id, display, inputs, operation, marks, conditions, extra, rune, gloss ───
# Marks: T table, 3 three-slot table, E engineered, A minor arcane, R altar, C circle, H hand-made only,
# X: explicit runners in extra["Runners"] (specialists).
TIER2 = [
    ("PulvisPyrius", "Pulvis Pyrius", ["Bitumen", "Quebrith", "SalVolatile"], "Conjunction", "3E", [], {}, (4, 9), "Black powder. Teaches the Bomb and Mega Bomb recipes."),
    ("AquaRegia", "Aqua Regia", ["AquaFortis", "SalMarinum"], "Dissolution", "TEA", [], {}, (1, 8), "Royal water. It dissolves even gold."),
    ("AquaVitae", "Aqua Vitae", ["Vitae", "RosMaialis"], "Distillation", "TA", [], {}, (1, 10), "Water of life. A spirit that makes friends of strangers."),
    ("Electrum", "Electrum", ["(O)336", "Argentum"], "Conjunction", "TE", [], {}, (4, 10), "Gold and silver, smelted together. A reliable alloy."),
    ("Rebis", "Rebis", ["Sol", "Luna"], "Conjunction", "TR", [("SunOrMoonDay", "Required")], {}, (0, 14), "The double thing: Sun and Moon wed in one body."),
    ("Orichalcum", "Orichalcum", ["Metallum"], "Conjunction", "TX", [("PowerFulgur", "Required")],
     {"Runners": [("QuarryCrucible", "Fulgur")]}, (4, 11), "The lost metal of the old stories, grown with lightning."),
    ("Elixir", "Elixir", ["Vitae", "Anima"], "Distillation", "TA", [], {}, (3, 11), "A healing draught. Health returns slowly while it lasts."),
    ("Panacea", "Panacea", ["Elixir", "Ordo"], "Coagulation", "TAR", [], {}, (3, 13), "The cure for all ills: clears every debuff."),
    ("CinisPhoenicis", "Cinis Phoenicis", ["Vitae"], "Calcination", "TX", [("PowerFlamma", "Required")],
     {"Runners": [("Athanor", "Open")]}, (3, 14), "Phoenix ash. Rub it on, and the next fall doesn't finish you."),
    ("Gelum", "Gelum", ["Tempestas", "SalFixum"], "Coagulation", "TEA", [], {}, (2, 8), "Frost in a jar. Feed it to a cow in winter."),
    ("Venenum", "Venenum", ["Miasma", "Mors"], "Fermentation", "TEA", [], {}, (3, 15), "Poison for blades. Apply it before a fight."),
    ("Ectoplasm", "Ectoplasm", ["Spiritus", "Limus"], "Conjunction", "TAH", [], {}, (2, 11), "Ghost-stuff. The Curator has uses for it."),
    ("PerpetuumMobile", "Perpetuum Mobile", ["Motus", "Ordo", "Orichalcum"], "Ritual", "3RH", [("PowerVapor", "Required")], {}, (2, 12),
     "The engine that never stops. It needs an altar with a Boiler beside it."),
    ("Radicula", "Radicula", ["Anima", "TerraViva", "#category_vegetable"], "Ritual", "3RH", [], {}, (4, 12),
     "A rootling: a little plant spirit. Place it in your fields; it shrieks at monsters."),
    ("Argentum", "Argentum", ["Metallum", "Luna"], "Conjunction", "TX", [("Monday", "Required")],
     {"Runners": [("QuarryCrucible", "NoPower")]}, (0, 12), "Silver, grown from the metal seed on the Moon's day."),
    ("Stannum", "Stannum", ["Metallum", "Tempestas"], "Conjunction", "TX", [("Thursday", "Required")],
     {"Runners": [("QuarryCrucible", "NoPower")]}, (4, 13), "Tin, grown on Jupiter's day."),
    ("Plumbum", "Plumbum", ["Metallum", "Mors"], "Conjunction", "TX", [("Saturday", "Required")],
     {"Runners": [("QuarryCrucible", "NoPower")]}, (4, 14), "Lead, grown on Saturn's day."),
    ("RosaVitrea", "Rosa Vitrea", ["Vitrum", "#flower_item"], "Conjunction", "AH", [], {}, (2, 9), "A glass rose. A gift most people like."),
    ("MelVitae", "Mel Vitae", ["Vitae", "(O)340"], "Conjunction", "AH", [], {}, (3, 4), "Living honey. Sweet, and a little lucky."),
    ("PhialaStellarum", "Phiala Stellarum", ["Luna", "Vitrum"], "Ritual", "RH", [("Night", "Required")], {}, (0, 11), "A phial of starlight, sealed at night. Stargazers love it."),
]

# The dark twins (doc 03a): putrefaction of the bright form in darkness; ✦ and ✦A.
DARK_TWINS = [
    ("Tenebrae", "Tenebrae", "Sol", (0, 10), "Void, the Sun's shadow. Feed it to a hen and she may lay a Void Egg."),
    ("Mors", "Mors", "Vitae", (4, 0), "Death, bottled. Sprinkle it on weeds, and they wither."),
    ("Perditio", "Perditio", "Ordo", (4, 1), "Entropy. It unmakes order, and leaves the first matter behind."),
]

# Tier 3 (doc 03a): hand-made, behind the Great Work.
TIER3 = [
    ("PrimaMateria", "Prima Materia", ["CaputMortuum", "Perditio"], "Fermentation", "R", (0, 0), "The first matter, found in the dung heap after all."),
    ("Alkahest", "Alkahest", ["AquaFortis", "MercuriusPhilosophorum"], "Dissolution", "R", (0, 8), "The universal solvent. Stone and wood fall apart at a touch."),
    ("Azoth", "Azoth", ["MercuriusPhilosophorum", "QuintaEssentia"], "Conjunction", "R", (0, 3), "The universal agent. Machines near an Azoth Diffuser hurry."),
    ("LapisPhilosophorum", "Lapis Philosophorum", ["PrimaMateria", "Azoth", "Alkahest", "Rebis"], "Ritual", "C", (0, 4),
     "The Stone. It promises everything, and gives a little more than nothing."),
]

# Stage advances (doc 12 §5, doc 13 Pelican): the substance comes back one quality step higher.
SAME_AS_INPUT = P + "SameAsInput"
STAGE_ADVANCES = [
    ("AdvancePelican", "Pelican", "Distillation", "SalFixum", 2),
    ("AdvanceAltar", "RitualAltar", "Ritual", "MercuriusFixus", 4),
]

# ── Products (doc 15): uses per substance ────────────────────────────────────────
# Edible: energy points as Edibility; Effects names effect IDs. Apply: used from the hand. Categories: -26 artisan.
PRODUCTS = {
    "SalFixum": {"Family": "Consumable", "Edible": {"Edibility": 5, "IsDrink": True, "Effects": ["Fixative"]}, "Category": -26},
    "MercuriusFixus": {"Family": "Consumable", "Edible": {"Edibility": 5, "IsDrink": True, "Effects": ["FixativeStrong"]}, "Category": -26},
    "MercuriusCurrens": {"Family": "Consumable", "Edible": {"Edibility": 5, "IsDrink": True, "Effects": ["Volatile"]}, "Category": -26},
    "Luna": {"Family": "Consumable", "Edible": {"Edibility": 10, "IsDrink": True, "Effects": ["LunarDream"]}, "Category": -26},
    "SalMarinum": {"Family": "Consumable", "Edible": {"Edibility": 5, "IsDrink": True, "Effects": ["Savour"]}, "Category": -26},
    "AquaVitae": {"Family": "Consumable", "Edible": {"Edibility": 20, "IsDrink": True, "Effects": ["GoodCheer"]}, "Category": -26},
    "Elixir": {"Family": "Consumable", "Edible": {"Edibility": 25, "IsDrink": True, "Effects": ["Vitality"]}, "Category": -26},
    "Panacea": {"Family": "Consumable", "Edible": {"Edibility": 10, "IsDrink": True, "Effects": ["Cleanse"]}, "Category": -26},
    "Sol": {"Family": "Consumable", "Edible": {"Edibility": 10, "IsDrink": True, "Effects": ["Glow"]}, "Category": -26},
    "Pneuma": {"Family": "Consumable", "Edible": {"Edibility": 5, "IsDrink": True, "Effects": ["Tailwind"]}, "Category": -26},
    "Magma": {"Family": "Consumable", "Edible": {"Edibility": 5, "IsDrink": True, "Effects": ["Kindled"]}, "Category": -26},
    "Saxum": {"Family": "Reagent", "Apply": {"Effects": ["Stonesense"]}, "Category": -16},
    "Quebrith": {"Family": "Consumable", "Apply": {"Effects": ["Eruption"]}, "Category": -26},
    "QuintaEssentia": {"Family": "Consumable", "Edible": {"Edibility": 10, "IsDrink": True, "Effects": ["Quintessence"]}, "Category": -26},
    "Metallum": {"Family": "Consumable", "Edible": {"Edibility": 5, "IsDrink": True, "Effects": ["Midas"]}, "Category": -26},
    "Motus": {"Family": "Consumable", "Edible": {"Edibility": 5, "IsDrink": True, "Effects": ["Hourglass"]}, "Category": -26},
    "Ectoplasm": {"Family": "Consumable", "Apply": {"Effects": ["Spectral"]}, "Category": -26},
    "Alkahest": {"Family": "Consumable", "Apply": {"Effects": ["Dissolution"]}, "Category": -26},
    "CinisPhoenicis": {"Family": "Consumable", "Apply": {"Effects": ["SecondWind"]}, "Category": -26},
    "Venenum": {"Family": "Consumable", "Apply": {"Effects": ["Venom"]}, "Category": -26},
    "Mors": {"Family": "Consumable", "Field": {"Handler": "Weedkiller", "Radius": 2}, "Category": -26},
    "Cinnabaris": {"Family": "Sellable", "Dye": "red", "Category": -26},
    "Bitumen": {"Family": "Reagent", "Fuel": True, "Category": -16},
    "PulvisPyrius": {"Family": "Reagent", "Category": -26},
    "Tempestas": {"Family": "Reagent", "Category": -16},
    "Vitrum": {"Family": "Sellable", "Category": -26},
    "AquaRegia": {"Family": "Sellable", "Category": -26},
    "Gelum": {"Family": "Sellable", "Category": -26},
    "Rebis": {"Family": "Sellable", "Category": -26},
    "Argentum": {"Family": "Sellable", "Category": -15, "FixedPrice": 220, "Metal": True},
    "Stannum": {"Family": "Sellable", "Category": -15, "FixedPrice": 110, "Metal": True},
    "Plumbum": {"Family": "Sellable", "Category": -15, "FixedPrice": 160, "Metal": True},
    "Electrum": {"Family": "Sellable", "Category": -15, "FixedPrice": 450, "Metal": True},
    "Orichalcum": {"Family": "Sellable", "Category": -15, "FixedPrice": 800, "Metal": True},
    "RosaVitrea": {"Family": "Gift", "Category": -26, "FixedPrice": 300},
    "MelVitae": {"Family": "Gift", "Edible": {"Edibility": 20, "Effects": ["LuckyHoney"]}, "Category": -26, "FixedPrice": 350},
    "PhialaStellarum": {"Family": "Gift", "Category": -26, "FixedPrice": 500},
    "Tenebrae": {"Family": "Sellable", "Category": -26},
    "Perditio": {"Family": "Sellable", "Category": -26},
    "PerpetuumMobile": {"Family": "Sellable", "Category": -26, "FixedPrice": 5000},
    "Radicula": {"Family": "Sellable", "Category": -26, "FixedPrice": 1500},
    "PrimaMateria": {"Family": "Sellable", "Category": -26, "FixedPrice": 2000},
    "Azoth": {"Family": "Sellable", "Category": -26, "FixedPrice": 3000},
    "LapisPhilosophorum": {"Family": "Sellable", "Category": -26, "FixedPrice": 25000},
}

# Effects (doc 15 §3, §3a): id → (kind, stats or handler, params, duration minutes, tier, name, description)
EFFECTS = {
    "Fixative": ("Custom", "Fixative", {"Minutes": "2"}, 60, "Weak", "Fixative", "Your food and drink buffs stop running down for a while."),
    "FixativeStrong": ("Custom", "Fixative", {"Minutes": "8"}, 120, "Strong", "Fixative", "Your food and drink buffs stop running down, and debuffs wear off twice as fast."),
    "Volatile": ("Custom", "Volatile", {"Factor": "1.5"}, 30, "Weak", "Volatile", "Your other buffs grow stronger, and shorter."),
    "LunarDream": ("Custom", "LunarDream", {}, 600, "Weak", "Lunar Dream", "Sleep tonight and dream of an undiscovered recipe."),
    "Savour": ("Custom", "Savour", {"Bonus": "0.15"}, 180, "Weak", "Savour", "Cooked food restores 15% more."),
    "GoodCheer": ("Custom", "GoodCheer", {"Bonus": "0.05"}, 240, "Weak", "Good Cheer", "Friendship comes a little easier, and you never get tipsy."),
    "Vitality": ("Custom", "Vitality", {"PerTenMinutes": "2"}, 180, "Weak", "Vitality", "Health returns slowly."),
    "Cleanse": ("Custom", "Cleanse", {}, 60, "Weak", "Cleanse", "Clears your debuffs, and keeps you a little more immune."),
    "Glow": ("Custom", "Glow", {"Radius": "6"}, 240, "Weak", "Glow", "You give off light."),
    "Tailwind": ("VanillaStats", {"Speed": 0.5}, {}, 180, "Weak", "Tailwind", "Speed +0.5."),
    "Kindled": ("Custom", "Kindled", {"Chance": "0.2"}, 180, "Weak", "Kindled", "Some of your hits set monsters burning."),
    "Stonesense": ("Custom", "Stonesense", {"Chance": "0.15"}, 240, "Weak", "Stonesense", "Stones sometimes give an extra ore."),
    "Eruption": ("Custom", "Eruption", {"Chance": "0.05"}, 240, "Weak", "Eruption", "A broken stone may burst and break the stones around it."),
    "Quintessence": ("Custom", "Quintessence", {"Bonus": "0.1"}, 240, "Weak", "Quintessence", "Your other alchemical effects are a little stronger."),
    "Midas": ("Custom", "Midas", {"Chance": "0.05"}, 240, "Weak", "Midas Draught", "Mined ore sometimes comes out one metal higher."),
    "Hourglass": ("Custom", "Hourglass", {"Slow": "0.25"}, 120, "Weak", "Hourglass of Mercury", "The clock runs slower. Single player only."),
    "Spectral": ("Custom", "Spectral", {"Seconds": "3"}, 180, "Weak", "Spectral", "After a hit, nothing can touch you for a moment."),
    "Dissolution": ("Custom", "Dissolution", {"Swings": "40"}, 120, "Weak", "Dissolution", "Stones, wood and weeds break in one hit."),
    "SecondWind": ("Custom", "SecondWind", {}, 600, "Weak", "Second Wind", "The next time you'd pass out, you stay up instead."),
    "Venom": ("VanillaStats", {"AttackMultiplier": 0.1, "CriticalChanceMultiplier": 0.1}, {}, 180, "Weak", "Venom", "Your weapon is coated: attack and crit chance +10%."),
    "LuckyHoney": ("VanillaStats", {"LuckLevel": 1}, {}, 240, "Weak", "Lucky Honey", "Luck +1."),
    "Planetary": ("Custom", "Planetary", {"Strong": "false"}, 1440, "Weak", "Planetary Influence", "Today's planet lends you its gift."),
    "PlanetaryStrong": ("Custom", "Planetary", {"Strong": "true"}, 1440, "Strong", "Planetary Influence", "Today's planet lends you its gift, strongly."),
    "Ouroboros": ("Custom", "Ouroboros", {"Energy": "1"}, 1440, "Weak", "Ouroboros", "Each kill restores a little energy."),
    "Censer": ("Custom", "Censer", {"Radius": "3", "Damage": "6"}, 1440, "Weak", "Smouldering Censer", "Nearby monsters burn, and you glow."),
    "VialPendant": ("Custom", "VialPendant", {"Threshold": "0.25"}, 1440, "Weak", "Vial Pendant", "Below a quarter of your health, you drink an Elixir from your pack."),
    "RebisAmulet": ("Custom", "RebisAmulet", {}, 1440, "Weak", "Rebis Amulet", "Attack by day, defence by night."),
    "Transmutation": ("Custom", "Transmutation", {"Chance": "0.05"}, 1440, "Strong", "Transmutation", "Picked-up items sometimes come out one quality higher."),
}

# Worn and placed items (doc 15 §3a, §5): id, display, kind, effect, sprite (7soul1 icon file), price, description.
WORN = [
    ("VialPendant", "Vial Pendant", "Trinket", "VialPendant", "Ac_Necklace03", 600, "A trinket. Below a quarter of your health, it drinks an Elixir from your pack for you."),
    ("SmoulderingCenser", "Smouldering Censer", "Trinket", "Censer", "Ac_Medal02", 800, "A trinket. Burns monsters near you and gives off light."),
    ("RebisAmulet", "Rebis Amulet", "Trinket", "RebisAmulet", "Ac_Necklace07", 900, "A trinket. By day you hit harder; by night you take less."),
    ("PlanetaryBand", "Planetary Band", "Ring", "Planetary", "Ac_Ring01", 400, "A ring of cheap metals. Each day's planet lends a small gift."),
    ("RingOfTheSeven", "Ring of the Seven", "Ring", "PlanetaryStrong", "Ac_Ring02", 4000, "Seven metals and the fifth essence. Each day's planet lends a great gift."),
    ("OuroborosRing", "Ouroboros Ring", "Ring", "Ouroboros", "Ac_Necklace05", 700, "The snake that eats its tail. Kills restore energy; your trash comes back as residue."),
]
WORN_CRAFTING = {
    "VialPendant": ("CharlesDR.ValleyOpus_Vitrum 2 CharlesDR.ValleyOpus_Elixir 1 334 2", "Elixir"),
    "SmoulderingCenser": ("CharlesDR.ValleyOpus_Magma 3 CharlesDR.ValleyOpus_SulphurVivum 3 335 2", "Magma"),
    "RebisAmulet": ("CharlesDR.ValleyOpus_Rebis 1 336 2", "Rebis"),
    "PlanetaryBand": ("334 2 335 1 CharlesDR.ValleyOpus_Metallum 2", "Metallum"),
    "RingOfTheSeven": ("CharlesDR.ValleyOpus_Argentum 1 CharlesDR.ValleyOpus_Stannum 1 CharlesDR.ValleyOpus_Plumbum 1 334 1 335 1 336 1 CharlesDR.ValleyOpus_MercuriusCurrens 1 CharlesDR.ValleyOpus_QuintaEssentia 1", "QuintaEssentia"),
    "OuroborosRing": ("CharlesDR.ValleyOpus_CaputMortuum 20 CharlesDR.ValleyOpus_Mors 2 335 1", "Mors"),
}

# Placed helpers: id, display, sprite (vanilla Craftables index), recipe, learned by, description.
PLACED = [
    ("AzothDiffuser", "Azoth Diffuser", 25, "CharlesDR.ValleyOpus_Azoth 1 CharlesDR.ValleyOpus_Vitrum 5", "Azoth",
     "Placed, it makes every machine within 7 tiles work 25% faster for 7 days, then crumbles to residue."),
    ("RadiculaPlant", "Radicula", 90, None, None, "A rootling, planted. Now and then it shrieks, stunning monsters nearby."),
]

# Alternate vanilla crafting recipes (doc 15 §8): id → (ingredients, yield item, count, learned by substance).
ALT_CRAFTING = {
    "Bombs": ("CharlesDR.ValleyOpus_PulvisPyrius 1 382 1", "(O)287", 3, "PulvisPyrius"),
    "MegaBombs": ("CharlesDR.ValleyOpus_PulvisPyrius 2 382 1", "(O)288", 2, "PulvisPyrius"),
    "RainTotem": ("CharlesDR.ValleyOpus_Tempestas 1 709 1", "(O)681", 1, "Tempestas"),
    "GlazedUrn": ("CharlesDR.ValleyOpus_Smaltum 3 330 5", "(F)" + P + "GlazedUrn", 1, "Smaltum"),
    "GlazedPlanter": ("CharlesDR.ValleyOpus_Smaltum 2 330 3", "(F)" + P + "GlazedPlanter", 1, "Smaltum"),
    "GlazedTileRug": ("CharlesDR.ValleyOpus_Smaltum 6", "(F)" + P + "GlazedTileRug", 1, "Smaltum"),
}

# New produce items (doc 15 §7a): id, display, 7soul1 icon, price, category, description.
NEW_ITEMS = [
    ("BrimstoneEgg", "Brimstone Egg", "I_Eye", 90, -5, "A warm, yellow egg that smells of struck matches. Rich in Sulphur."),
    ("GoldenFleece", "Golden Fleece", "I_Fabric", 1200, -18, "Wool spun through with gold. Rare, and beautiful."),
]

# Curios (doc 14 §1): keepsakes with no effect, from Sorenson's contracts. id, display, 7soul1 icon, lore.
CURIOS = [
    ("CurioSyntheticVial", "Synthetic Essence Vial", "I_Bottle01", "Joja Labs, batch 7. The label promises \"Essence of Spring, 100% synthetic\"."),
    ("CurioRedactedNotebook", "Redacted Notebook", "I_Book", "Most of every page is blacked out. One margin note survives: \"It won't scale.\""),
    ("CurioBrokenAssay", "Broken Assay Scale", "I_Mirror", "Dr. Sorenson's first instrument. She kept it after they let her go."),
    ("CurioJojaBadge", "Lab Badge", "I_Key01", "A Joja Labs access badge. The photo has been scratched out."),
    ("CurioCrystalSeed", "Grown Crystal", "I_Crystal01", "Sorenson's one success at Joja, grown in a jar on her desk."),
]

# Furniture (Circle Sigil rug, glazed décor). id, display, type, size (tiles), vanilla furniture sheet index (placeholder), description.
FURNITURE = [
    ("CircleSigil", "Circle Sigil", "rug", (5, 5), 1, "Lay it under a Ritual Altar to make a Ritual Circle: one big rite at a time."),
    ("GlazedUrn", "Glazed Urn", "decor", (1, 2), 1376, "Cobalt glaze over cheap clay."),
    ("GlazedPlanter", "Glazed Planter", "decor", (1, 1), 1376, "A small blue planter."),
    ("GlazedTileRug", "Glazed Tile Rug", "rug", (3, 2), 0, "Blue tiles laid in a square."),
]

# ── Tools (doc 16): Satchel, Lens are in the slice. The Gloam Lantern holds Shades. ──
GLOAM_LANTERN = ("GloamLantern", "Gloam Lantern", "I_Torch02",
                 "Holds Shades: one fills it each night you sleep with it. Some dark rites burn a Shade for a better result.")

# Apparatus and tool crafting (doc 14 sets the unlocks).
CRAFTING = {
    "ExperimentTable": ("388 30 390 20", True),
    "Athanor": ("390 40 334 2 330 10", True),
    "Alembic": ("338 2 388 10", True),
    "Retort": ("338 2 390 10", True),
    "HermeticVessel": ("338 3 334 1", True),
    "Calcinator": ("390 50 334 3 382 5", True),
    "ProcessVat": ("388 30 335 2 CharlesDR.ValleyOpus_Vitrum 2", True),
    "Press": ("390 40 335 3 CharlesDR.ValleyOpus_Saxum 5", True),
    "Boiler": ("334 5 335 2 330 10", True),
    "LightningCoil": ("335 3 787 1 338 2", True),
    "Pelican": ("CharlesDR.ValleyOpus_Vitrum 5 CharlesDR.ValleyOpus_Saxum 2", True),
    "RitualAltar": ("CharlesDR.ValleyOpus_Saxum 10 336 2 338 5", True),
    "QuarryCrucible": ("390 50 335 5 CharlesDR.ValleyOpus_Saxum 5", True),
    "LeyTap": ("CharlesDR.ValleyOpus_Saxum 5 CharlesDR.ValleyOpus_Metallum 2 709 5", True),
    "Satchel": ("771 30 388 10", False),
    "Lens": ("338 1 334 1", False),
    "GloamLantern": ("CharlesDR.ValleyOpus_Vitrum 2 CharlesDR.ValleyOpus_Tenebrae 1 335 1", False),
    "CircleSigil": ("CharlesDR.ValleyOpus_Saxum 20 CharlesDR.ValleyOpus_Ordo 4 428 5", "(F)"),
}

# Decomposition weights for the rest of the year (doc 03; placeholders).
MORE_TAG_WEIGHTS = {
    "id_o_86": {"Terra": 2, "Sal": 2},            # Earth Crystal
    "id_o_335": {"Terra": 2, "Mercurius": 2},     # Iron Bar
    "id_o_336": {"Ignis": 1, "Mercurius": 3},     # Gold Bar
    "id_o_337": {"Mercurius": 3, "Sal": 1},       # Iridium Bar
    "id_o_384": {"Terra": 2, "Mercurius": 2},     # Gold Ore
    "id_o_386": {"Mercurius": 3, "Terra": 1},     # Iridium Ore
    "id_o_340": {"Aer": 2, "Sulphur": 1},         # Honey
    "id_o_421": {"Ignis": 3, "Aer": 1},           # Sunflower
    "id_o_767": {"Aer": 2, "Mercurius": 1},       # Bat Wing
    "id_o_684": {"Sulphur": 2, "Terra": 1},       # Bug Meat
    "id_o_420": {"Sulphur": 2, "Terra": 2},       # Red Mushroom
    "id_o_422": {"Sulphur": 2, "Mercurius": 2},   # Purple Mushroom
    "id_o_848": {"Ignis": 3, "Terra": 1},         # Cinder Shard
    "id_o_909": {"Ignis": 2, "Terra": 2},         # Radioactive Ore
    "id_o_709": {"Terra": 3, "Aer": 1},           # Hardwood
    "id_o_92": {"Sulphur": 2, "Terra": 1},        # Sap
    "id_o_303": {"Mercurius": 2, "Aer": 1},       # Pale Ale
    "id_o_459": {"Mercurius": 2, "Aer": 1},       # Mead
    "fish_legendary": {"Aqua": 2, "Sulphur": 1},
    "season_winter": {"Aqua": 1, "Sal": 1},
}
MORE_AETHER_SOURCES = {
    "id_o_898": {"Aether": 2, "Ignis": 1, "Aqua": 1},   # Son of Crimsonfish
    "id_o_899": {"Aether": 2, "Aqua": 2},               # Ms. Angler
    "id_o_900": {"Aether": 2, "Aqua": 2},               # Legend II
    "id_o_901": {"Aether": 2, "Aqua": 1, "Mercurius": 1},  # Radioactive Carp
    "id_o_902": {"Aether": 2, "Aqua": 2},               # Glacierfish Jr.
    "id_o_896": {"Aether": 2, "Sal": 1},                # Galaxy Soul
    "id_o_337": {"Aether": 1, "Mercurius": 3, "Sal": 1},  # Iridium Bar: trace
}

# ── Ley nodes (doc 13 data shape, doc 14 §7) ────────────────────────────────────
# id, location, tile, profile, units per day, revealed by (unlock id)
LEY_NODES = [
    ("Forest", "Forest", (50, 55), {"Terra": 2, "Aqua": 1}, 3, "LeyTap"),
    ("Mountain", "Mountain", (40, 12), {"Terra": 2, "Sal": 1}, 3, "LeyTap"),
    ("Beach", "Beach", (26, 26), {"Aqua": 2, "Sal": 1}, 3, "LeyTap"),
    ("Tower", "Forest", (5, 29), {"Aether": 1, "Mercurius": 2}, 2, "Sanctum"),
    ("Backwoods", "Backwoods", (19, 19), {"Aether": 1, "Terra": 2}, 2, "Observatory"),
    ("Railroad", "Railroad", (34, 40), {"Ignis": 2, "Aether": 1}, 2, "Observatory"),
    ("Desert", "Desert", (30, 30), {"Ignis": 2, "Aer": 1, "Aether": 1}, 3, "Observatory"),
    ("Volcano", "IslandNorth", (40, 25), {"Ignis": 3, "Aether": 1}, 4, "Observatory"),
]
