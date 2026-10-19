"""The slice's content (doc 11, doc 03a, doc 13, doc 15), as plain data. build_base_pack.py turns it into the
base Content Patcher pack. IDs are permanent from alpha.2 (doc 09 §11): never rename an ID here."""

P = "CharlesDR.ValleyOpus_"

# id, display name, group, colour, sort order, rune (row, col) on the Idylwild codex sheet, price, gloss
PRIMES = [
    ("Ignis", "Ignis", "Element", "#E0442A", 10, (0, 5), 1, "The Prime of fire: heat, change, appetite."),
    ("Aqua", "Aqua", "Element", "#3A8FD8", 20, (0, 6), 1, "The Prime of water: flow, solution, life."),
    ("Terra", "Terra", "Element", "#8A5A2B", 30, (0, 4), 1, "The Prime of earth: weight, body, patience."),
    ("Aer", "Aer", "Element", "#CFE8F0", 40, (0, 3), 1, "The Prime of air: breath, lightness, scent."),
    ("Sal", "Sal", "Principle", "#E8E8E0", 50, (0, 2), 1, "The Principle of salt: what stays, fixes and preserves."),
    ("Sulphur", "Sulphur", "Principle", "#E3C21E", 60, (4, 8), 1, "The Principle of sulphur: what burns, colours and binds."),
    ("Mercurius", "Mercurius", "Principle", "#A8B0B8", 70, (0, 1), 1, "The Principle of mercury: what moves, joins and quickens."),
    ("Aether", "Aether", "Rare", "#9B59D0", 80, (0, 7), 5, "The fifth essence. Rare, and found only in rare things."),
]

# Operations (doc 13 §3).
OPERATIONS = ["Calcination", "Dissolution", "Separation", "Conjunction", "Fermentation", "Distillation", "Coagulation"]

# Which apparatus run each operation outside the Table (state lists per apparatus).
OP_APPARATUS = {
    "Calcination": [("Athanor", "Open"), ("Calcinator", "Open")],
    "Dissolution": [("Alembic", "Cold")],
    "Distillation": [("Alembic", "Heated")],
    "Separation": [("Retort", "Open")],
    "Coagulation": [("Retort", "Open")],
    "Conjunction": [("HermeticVessel", "Open")],
    "Fermentation": [("HermeticVessel", "Open")],
}

# Operations that get the flower Bonus (doc 11: Athanor, Retort and Hermetic Vessel).
FLOWER_BONUS_OPS = {"Calcination", "Separation", "Coagulation", "Conjunction", "Fermentation"}

# Tier 1 (doc 03a): id, display, inputs, operation, class, table_only, rune (row, col), gloss
TIER1 = [
    ("FlammaViva", "Flamma Viva", ["Ignis", "Ignis"], "Calcination", "Power", False, None, "Living flame. Fed to an Athanor, it kindles it for good."),
    ("RosMaialis", "Ros Maialis", ["Aqua", "Aqua"], "Distillation", "Item", False, (1, 11), "May dew, distilled. Drink it and your watering can goes further."),
    ("Saxum", "Saxum", ["Terra", "Terra"], "Coagulation", "Item", False, (2, 13), "Worked stone, dense and true. Builders like it."),
    ("Pneuma", "Pneuma", ["Aer", "Aer"], "Separation", "Item", False, (2, 5), "Breath held in a bottle."),
    ("SalFixum", "Sal Fixum", ["Sal", "Sal"], "Coagulation", "Item", False, (1, 7), "Fixed salt. It does not move, and keeps what it touches."),
    ("SulphurVivum", "Sulphur Vivum", ["Sulphur", "Sulphur"], "Calcination", "Item", False, (3, 2), "Living sulphur, bright and restless."),
    ("MercuriusCurrens", "Mercurius Currens", ["Mercurius", "Mercurius"], "Distillation", "Item", False, (1, 0), "Running mercury. Hard to keep in one place."),
    ("QuintaEssentia", "Quinta Essentia", ["Aether", "Aether"], "Distillation", "Item", True, (0, 9), "The quintessence. Nobody is sure what it is for yet."),
    ("Vapor", "Vapor", ["Ignis", "Aqua"], "Distillation", "Power", True, None, "Steam: the strength of a Boiler."),
    ("Magma", "Magma", ["Ignis", "Terra"], "Calcination", "Item", False, (3, 8), "Molten earth, cooled just enough to carry."),
    ("Fulgur", "Fulgur", ["Ignis", "Aer"], "Conjunction", "Power", True, None, "Lightning: the strength of a Coil."),
    ("Limus", "Limus", ["Aqua", "Terra"], "Dissolution", "Item", False, (2, 3), "Primordial mud. Things grow in it."),
    ("Tempestas", "Tempestas", ["Aqua", "Aer"], "Conjunction", "Item", False, (2, 6), "A storm in a flask. Keep the stopper in."),
    ("Pulvis", "Pulvis", ["Terra", "Aer"], "Calcination", "Item", False, (3, 4), "Fine dust, calcined. The start of glass."),
    ("Calx", "Calx", ["Ignis", "Sal"], "Calcination", "Item", False, (1, 6), "Burnt lime, white and dry. Sells, and makes glass."),
    ("Quebrith", "Quebrith", ["Ignis", "Sulphur"], "Conjunction", "Item", False, (3, 6), "Brimstone, joined with fire."),
    ("Phlogiston", "Phlogiston", ["Ignis", "Mercurius"], "Distillation", "Item", False, (3, 7), "The stuff that leaves a thing when it burns, they say."),
    ("SalMarinum", "Sal Marinum", ["Aqua", "Sal"], "Dissolution", "Item", False, (2, 4), "Sea salt in solution."),
    ("AquaFortis", "Aqua Fortis", ["Aqua", "Sulphur"], "Distillation", "Item", False, (1, 3), "Strong water. It bites metal."),
    ("Hydragenum", "Hydragenum", ["Aqua", "Mercurius"], "Dissolution", "Item", False, (1, 4), "Water that remembers being mercury."),
    ("Vitreus", "Vitreus", ["Terra", "Sal"], "Coagulation", "Item", False, (1, 5), "Glassy earth, half-way to Vitrum."),
    ("Bitumen", "Bitumen", ["Terra", "Sulphur"], "Coagulation", "Item", False, (3, 5), "Pitch. It burns slowly and seals well."),
    ("Metallum", "Metallum", ["Terra", "Mercurius"], "Conjunction", "Item", False, (4, 2), "The seed of every metal."),
    ("SalVolatile", "Sal Volatile", ["Aer", "Sal"], "Separation", "Item", False, (3, 0), "Smelling salts. A quick, small lift of energy."),
    ("Miasma", "Miasma", ["Aer", "Sulphur"], "Calcination", "Item", False, (3, 3), "Bad air, bottled before it can spread."),
    ("Motus", "Motus", ["Aer", "Mercurius"], "Conjunction", "Item", False, (2, 10), "Motion itself, distilled to a few drops."),
    ("Sol", "Sol", ["Ignis", "Aether"], "Conjunction", "Item", False, (0, 13), "Gold's spirit. Warm to hold."),
    ("Luna", "Luna", ["Aqua", "Aether"], "Conjunction", "Item", False, (0, 12), "Silver's spirit. Cool to hold."),
    ("Vitae", "Vitae", ["Terra", "Aether"], "Conjunction", "Item", False, (4, 3), "The spark of life, bound to earth."),
    ("Spiritus", "Spiritus", ["Aer", "Aether"], "Conjunction", "Item", False, (3, 10), "Spirit. It rises if you let it."),
    ("Vitriol", "Vitriol", ["Sal", "Sulphur"], "Dissolution", "Item", False, (3, 1), "Green vitriol. Visit the interior of the earth."),
    ("MercuriusFixus", "Mercurius Fixus", ["Sal", "Mercurius"], "Coagulation", "Item", False, (1, 1), "Mercury made to stand still."),
    ("Cinnabaris", "Cinnabaris", ["Sulphur", "Mercurius"], "Conjunction", "Item", False, (4, 6), "Cinnabar, the red of the old dyers."),
    ("Ordo", "Ordo", ["Sal", "Aether"], "Coagulation", "Item", False, (1, 9), "Order. It holds other things in place."),
    ("Anima", "Anima", ["Sulphur", "Aether"], "Conjunction", "Item", False, (3, 12), "Soul, or something close to it."),
    ("MercuriusPhilosophorum", "Mercurius Philosophorum", ["Mercurius", "Aether"], "Distillation", "Item", True, (1, 2), "The philosophers' mercury. Its use is not yet known."),
]

# Tier 2 (doc 11 §2): id, display, inputs, operation, rune, gloss
TIER2 = [
    ("Vitrum", "Vitrum", ["Pulvis", "Calx"], "Calcination", (2, 14), "Clear glass. Sells well, and the Lab is built with it."),
    ("TerraViva", "Terra Viva", ["Limus", "Vitae"], "Fermentation", (4, 4), "Living earth. Spread it on tilled soil for better crops."),
    ("Vigor", "Vigor", ["Spiritus", "RosMaialis"], "Distillation", (3, 9), "A strong tonic. A large lift of energy."),
    ("Smaltum", "Smaltum", ["CaputMortuum", "Calx"], "Calcination", (2, 7), "Cobalt-blue glass, ground fine. A strong blue dye."),
]

CAPUT_MORTUUM = ("CaputMortuum", "Caput Mortuum", "The dead head: what is left. A weak fertilizer, and the start of better things.")

# Sort key blocks (doc 09 §11, spike S11): the internal Name sorts.
SORT_PRIME, SORT_T1, SORT_T2, SORT_RESIDUE = 1000, 2100, 2200, 2500

# Products (doc 15, doc 11 §2 "Use").
PRODUCTS = {
    "SalVolatile": {"Family": "Consumable", "Edible": {"Edibility": 10, "IsDrink": True}, "Category": -26},
    "Vigor": {"Family": "Consumable", "Edible": {"Edibility": 30, "IsDrink": True}, "Category": -26},
    "RosMaialis": {"Family": "Consumable", "Edible": {"Edibility": 1, "IsDrink": True, "Effects": ["UndinesGift"]}, "Category": -26},
    "Smaltum": {"Family": "Sellable", "Dye": "blue", "Category": -26},
    "CaputMortuum": {"Family": "Fertilizer", "Fertilizer": {"QualityLevel": 1, "QualityChance": 0.5}, "Category": -19, "FixedPrice": 2},
    "TerraViva": {"Family": "Fertilizer", "Fertilizer": {"QualityLevel": 2, "QualityChance": 1.0, "WaterRetention": 0.33}, "Category": -19},
}

# Apparatus (doc 13): id, display, kind, vanilla Craftables sprite index (placeholder), description
APPARATUS = [
    ("ExperimentTable", "Experiment Table", "Table", 208, "Try any two things together. Nothing is wasted that teaches you something."),
    ("Athanor", "Athanor", "MinorArcane", 13, "A slow furnace. Takes things apart into their Primes. Feed it Ignis and Ignis to kindle it."),
    ("Alembic", "Alembic", "MinorArcane", 12, "Dissolves when cold. Beside a kindled Athanor, it distills."),
    ("Retort", "Retort", "MinorArcane", 15, "Separates and coagulates. Flowers nearby improve its work."),
    ("HermeticVessel", "Hermetic Vessel", "MinorArcane", 21, "The sealed egg: joins things, and ferments them."),
    ("Calcinator", "Calcinator", "Engineered", 114, "A coal-fired furnace. Fast decomposition, with more residue."),
]

# Crafting recipes for the apparatus and tools (placeholders until the workshop's unlocks, doc 14).
CRAFTING = {
    "ExperimentTable": ("388 30 390 20", True),
    "Athanor": ("390 40 334 2 330 10", True),
    "Alembic": ("338 2 388 10", True),
    "Retort": ("338 2 390 10", True),
    "HermeticVessel": ("338 3 334 1", True),
    "Calcinator": ("390 50 334 3 382 5", True),
    "Satchel": ("771 30 388 10", False),
    "Lens": ("338 1 334 1", False),
}

# Decomposition weights for Spring's items (doc 03 table, placeholders for the sweep).
CATEGORY_DEFAULTS = {
    "-75": {"Terra": 2, "Aqua": 1},          # vegetables
    "-79": {"Aqua": 2, "Ignis": 1},          # fruit
    "-80": {"Aer": 3, "Sulphur": 1},         # flowers
    "-81": {"Terra": 1, "Aqua": 1, "Aer": 1},  # forage
    "-4": {"Aqua": 3, "Sal": 1},             # fish
    "-2": {"Sal": 2, "Terra": 1},            # gems
    "-12": {"Sal": 2, "Terra": 1},           # minerals
    "-15": {"Terra": 2, "Mercurius": 1},     # ores and bars
    "-16": {"Terra": 2},                     # resources
    "-5": {"Sulphur": 2, "Aer": 1},          # eggs
    "-6": {"Aqua": 2, "Sal": 1},             # milk
    "-18": {"Aer": 2, "Terra": 1},           # animal goods
    "-26": {"Mercurius": 1, "Aqua": 1},      # artisan goods
    "-27": {"Sulphur": 2, "Terra": 1},       # tapper goods
    "-28": {"Sulphur": 2, "Mercurius": 1},   # monster loot
    "-7": {"Ignis": 2, "Terra": 1},          # cooking
    "-20": {"Terra": 1},                     # trash
    "Default": {"Terra": 2, "Aqua": 1},
}

TAG_WEIGHTS = {
    "id_o_382": {"Ignis": 3, "Terra": 1},            # Coal
    "id_o_82": {"Ignis": 3, "Sal": 1},               # Fire Quartz
    "id_o_84": {"Aqua": 3, "Sal": 1},                # Frozen Tear
    "id_o_92": {"Sulphur": 2, "Terra": 1},           # Sap
    "id_o_152": {"Aqua": 2, "Sal": 1},               # Seaweed
    "id_o_330": {"Terra": 3, "Aqua": 1},             # Clay
    "id_o_388": {"Terra": 2, "Ignis": 1},            # Wood
    "id_o_771": {"Aer": 1, "Terra": 1},              # Fiber
    "id_o_80": {"Sal": 3},                           # Quartz
    "id_o_378": {"Terra": 2, "Mercurius": 1},        # Copper Ore
    "id_o_380": {"Terra": 2, "Mercurius": 1},        # Iron Ore
    "id_o_260": {"Ignis": 3},                        # Hot Pepper
    "id_o_348": {"Mercurius": 2, "Aqua": 1},         # Wine
    "id_o_444": {"Aer": 3},                          # Duck Feather
    "id_o_440": {"Aer": 2},                          # Wool
    "id_o_766": {"Sulphur": 1, "Mercurius": 2},      # Slime
    "edible_mushroom": {"Terra": 2, "Sulphur": 1},
    "egg_item": {"Sulphur": 2, "Aer": 1},
    "fish_ocean": {"Aqua": 2, "Sal": 2},
    "fish_river": {"Aqua": 3},
    "fish_lake": {"Aqua": 3},
}

# The Aether source list (doc 03 rule 6; doc 11): weights by source rarity.
AETHER_SOURCES = {
    "id_o_768": {"Aether": 1, "Ignis": 3, "Sal": 1},          # Solar Essence: trace
    "id_o_769": {"Aether": 1, "Mercurius": 2, "Sulphur": 2},  # Void Essence: trace
    "id_o_74": {"Aether": 3, "Sal": 1},                       # Prismatic Shard: bulk
    "id_o_72": {"Aether": 1, "Sal": 3},                       # Diamond: trace
    "id_o_159": {"Aether": 2, "Ignis": 1, "Aqua": 1},         # Crimsonfish: bulk
    "id_o_160": {"Aether": 2, "Aqua": 2},                     # Angler
    "id_o_163": {"Aether": 2, "Aqua": 2},                     # Legend
    "id_o_682": {"Aether": 2, "Aqua": 1, "Mercurius": 1},     # Mutant Carp
    "id_o_775": {"Aether": 2, "Aqua": 2},                     # Glacierfish
}
