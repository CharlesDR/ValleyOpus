"""The 1.0 world (docs 14, 08a, 07, 18, 05): unlocks, workshop rooms and bundles, ley nodes, hints, contracts,
Familiars and professions, as plain data. build_base_pack.py calls fill() to write the registry rows.

IDs are permanent once shipped (doc 09 §11). Every count, cost and price is a placeholder for the sweep (doc 06)."""

from collections import OrderedDict

import full_data as F
import slice_data as D

P = "CharlesDR.ValleyOpus_"

# Which unlock gives each apparatus's crafting recipe (doc 14 §2, §4).
APPARATUS_UNLOCK = {
    "ExperimentTable": "Intro", "Alembic": "Intro", "Retort": "Intro",
    "Athanor": "FurnaceRoom", "HermeticVessel": "FurnaceRoom", "Calcinator": "TobinArrives",
    "ProcessVat": "Cistern", "Press": "Cistern", "Boiler": "Cistern", "LightningCoil": "TobinCoilLetter",
    "Pelican": "BennetArrives", "RitualAltar": "Sanctum", "QuarryCrucible": "AgnesCrucible", "LeyTap": "LeyTap",
}

# Base-game gates (doc 14 §5): one GSQ each, so routes never leak into data. Both routes set these flags.
GATES = {
    "CcOpen": "PLAYER_HAS_MAIL Host ccDoorUnlock Received",
    "QuarryBridge": "ANY \"PLAYER_HAS_MAIL Host ccCraftsRoom Received\" \"PLAYER_HAS_MAIL Host jojaCraftsRoom Received\"",
    "BusRepair": "ANY \"PLAYER_HAS_MAIL Host ccVault Received\" \"PLAYER_HAS_MAIL Host jojaVault Received\"",
    "Guild": "PLAYER_HAS_MAIL Host guildMember Received",
    "SkullCavern": "PLAYER_HAS_MAIL Host HasSkullKey Received",
    "Island": "PLAYER_HAS_MAIL Host willyBoatFixed Received",
}

# id → kind, grants [(type, ref, count)], source (type, ref), requires [unlock ids or "gate:<Gate>"], pace, name, how-to line
UNLOCKS = OrderedDict([
    ("Intro", ("Story", [("Item", "(O)" + P + "Satchel", 1), ("Item", "(O)" + P + "Lens", 1),
                         ("Item", "(BC)" + P + "ExperimentTable", 1), ("Item", "(BC)" + P + "Alembic", 1),
                         ("Item", "(BC)" + P + "Retort", 1), ("Item", "(BC)" + P + "Athanor", 1),
                         ("CraftingRecipe", P + "ExperimentTable", 0), ("CraftingRecipe", P + "Alembic", 0),
                         ("CraftingRecipe", P + "Retort", 0), ("CraftingRecipe", P + "Satchel", 0), ("CraftingRecipe", P + "Lens", 0),
                         ("Mail", P + "Mail_Welcome", 0)],
               ("Intro", ""), ["gate:CcOpen"], "Spring", "The derelict workshop", "Oriel finds you in town after the Community Center opens.")),
    ("FurnaceRoom", ("Apparatus", [("CraftingRecipe", P + "Athanor", 0), ("CraftingRecipe", P + "HermeticVessel", 0), ("Flag", P + "Blueprint_LabT1", 0)],
                     ("WorkshopRoom", "FurnaceRoom"), [], "Spring", "The Furnace Room", "Restore the Furnace Room.")),
    ("TobinArrives", ("NPC", [("CraftingRecipe", P + "Calcinator", 0), ("Mail", P + "Mail_Tobin", 0)],
                      ("Delay", "1"), ["FurnaceRoom"], "Spring", "Tobin arrives", "Tobin comes the day after the Furnace Room is restored.")),
    ("FinchRoute", ("NPC", [("Mail", P + "Mail_Finch", 0)], ("Delay", "7"), ["FurnaceRoom"], "Spring", "Finch's wagon", "Finch starts his route a week after the Furnace Room.")),
    ("IvoGuild", ("NPC", [("Mail", P + "Mail_Ivo", 0)], ("Delay", "0"), ["FurnaceRoom", "gate:Guild"], "Spring", "Ivo at the Guild", "Ivo turns up at the Adventurer's Guild.")),
    ("DeltaArrives", ("NPC", [("Mail", P + "Mail_Delta", 0)], ("Skill", "Fishing 2"), ["FurnaceRoom"], "Spring", "Delta at the tide pools", "Delta comes to the beach once you can fish a little.")),
    ("BennetArrives", ("NPC", [("CraftingRecipe", P + "Pelican", 0), ("Mail", P + "Mail_Bennet", 0)], ("Delay", "0"), ["FurnaceRoom", "gate:BusRepair"],
                       "Winter", "Bennet's kiln", "Bennet writes once the bus runs to the desert.")),
    ("Cistern", ("Apparatus", [("CraftingRecipe", P + "ProcessVat", 0), ("CraftingRecipe", P + "Press", 0), ("CraftingRecipe", P + "Boiler", 0),
                               ("Mail", P + "Mail_Chaining", 0)], ("WorkshopRoom", "Cistern"), [], "Summer", "The Cistern", "Restore the Cistern.")),
    ("SorensonArrives", ("NPC", [("Mail", P + "Mail_Sorenson", 0)], ("Delay", "1"), ["Cistern"], "Summer", "Dr. Sorenson's contracts", "Dr. Sorenson arrives after the Cistern.")),
    ("TobinCoilLetter", ("Apparatus", [("CraftingRecipe", P + "LightningCoil", 0), ("Mail", P + "Mail_Coil", 0)], ("Storm", ""), ["Cistern"],
                         "Summer", "Tobin's lightning letter", "Tobin writes the morning after the next storm.")),
    ("Library", ("Capacity", [("Capacity", "TableSlots", 3), ("Mail", P + "Mail_Library", 0)], ("WorkshopRoom", "Library"), [], "Summer", "The Library",
                 "Restore the Library: the Table takes three things, and residue can be read.")),
    ("HesperArrives", ("NPC", [("Mail", P + "Mail_Hesper", 0)], ("Delay", "1"), ["Library"], "Summer", "Hesper's visits", "Hesper visits after the Library.")),
    ("LeyTap", ("Apparatus", [("CraftingRecipe", P + "LeyTap", 0), ("Mail", P + "Mail_LeyTap", 0)], ("Friendship", P + "Hesper 2"), ["HesperArrives"],
                "Summer", "The Ley Tap", "Hesper shares her Ley Tap at two hearts.")),
    ("AgnesArrives", ("NPC", [("Mail", P + "Mail_Agnes", 0)], ("Delay", "0"), ["AnyTier2"], "Summer", "Agnes at Clint's", "Agnes comes after any Tier II room.")),
    ("AnyTier2", ("Feature", [], ("AnyOf", "Cistern Library"), [], "Summer", "A Tier II room", "Restore the Cistern or the Library.")),
    ("AgnesCrucible", ("Apparatus", [("CraftingRecipe", P + "QuarryCrucible", 0), ("Mail", P + "Mail_Crucible", 0)], ("Delay", "0"),
                       ["AgnesArrives", "gate:QuarryBridge"], "Fall", "The Quarry Crucible", "Agnes's crucible, once the Quarry bridge is mended.")),
    ("Sanctum", ("Apparatus", [("CraftingRecipe", P + "RitualAltar", 0), ("Flag", P + "Blueprint_LabT2", 0), ("Mail", P + "Mail_Sanctum", 0)],
                 ("WorkshopRoom", "Sanctum"), [], "Fall", "The Sanctum", "Restore the Sanctum: the Ritual Altar and the first Aether ley node.")),
    ("Undercroft", ("Tool", [("CraftingRecipe", P + "GloamLantern", 0), ("Mail", P + "Mail_Undercroft", 0)], ("WorkshopRoom", "Undercroft"), [],
                    "Fall", "The Undercroft", "Restore the Undercroft: the Witch's Gloam Lantern.")),
    ("Observatory", ("Place", [("Mail", P + "Mail_Observatory", 0)], ("WorkshopRoom", "Observatory"), [], "Winter", "The Observatory",
                     "Restore the Observatory: the remaining ley nodes.")),
    ("CrystalVault", ("Tool", [("CraftingRecipe", P + "CircleSigil", 0), ("Mail", P + "Mail_Vault", 0)], ("WorkshopRoom", "CrystalVault"), [],
                      "Winter", "The Crystal Vault", "Restore the Crystal Vault: the Circle Sigil.")),
    ("GreatWork", ("Story", [("Flag", P + "Blueprint_LabT3", 0), ("Mail", P + "Mail_GreatWork", 0)], ("WorkshopRoom", "GreatWorkChamber"), [],
                   "Year2", "The Great Work", "Restore every room, then the Great Work Chamber.")),
    ("LarkArrives", ("NPC", [("Mail", P + "Mail_Lark", 0)], ("Discoveries", "10"), ["Intro"], "Summer", "Lark", "Lark turns up after the farm's tenth discovery.")),
    ("LanternUpgrade", ("Capacity", [("Capacity", "LanternShades", 40)], ("Skill", P + "Alchemy 6"), ["Undercroft"], "Fall", "A deeper lantern",
                        "Alchemy level 6, with the Undercroft restored: the Gloam Lantern holds twice as many Shades (40).")),
])

# Workshop rooms (doc 14 §2): id, tier, motif, host NPC, opens (mode, rooms), bundles, display name, map size, floor style, wall style
ROOMS = OrderedDict([
    ("EntranceHall", (0, None, "Curator", ("AllOf", []), [], "Entrance Hall", (30, 14), 0, 0)),
    ("FurnaceRoom", (1, "Calcination", "Curator", ("AllOf", ["EntranceHall"]), ["Kindling", "MineSalts", "SpringGleanings"], "Furnace Room", (15, 12), 3, 19)),
    ("Cistern", (2, "Dissolution", "Tobin", ("AllOf", ["FurnaceRoom"]), ["FirstWaters", "Fittings", "RiverAndLake"], "Cistern", (15, 12), 6, 12)),
    ("Library", (2, "Separation", "Oriel", ("AllOf", ["FurnaceRoom"]), ["FirstFires", "Curios", "Bindings"], "Library", (15, 12), 1, 10)),
    ("Sanctum", (3, "Conjunction", "Oriel", ("AnyOf", ["Cistern", "Library"]), ["Unions", "Offerings", "SummerLight"], "Sanctum", (15, 12), 10, 14)),
    ("Undercroft", (3, "Fermentation", "Curator", ("AnyOf", ["Cistern", "Library"]), ["RotAndReturn", "NightHarvest", "DarkThings"], "Undercroft", (15, 12), 14, 23)),
    ("Observatory", (4, "Distillation", "Hesper", ("AnyOf", ["Sanctum", "Undercroft"]), ["StarChart", "ClearSpirits", "WinterSky"], "Observatory", (15, 12), 20, 3)),
    ("CrystalVault", (4, "Coagulation", "Curator", ("AnyOf", ["Sanctum", "Undercroft"]), ["FixedSalts", "Gems", "MetalsOfTheWeek"], "Crystal Vault", (15, 12), 24, 16)),
    ("GreatWorkChamber", (5, "Ritual", "Curator", ("AllOf", ["FurnaceRoom", "Cistern", "Library", "Sanctum", "Undercroft", "Observatory", "CrystalVault"]),
                          ["Nigredo", "Albedo", "Citrinitas", "Rubedo"], "Great Work Chamber", (17, 14), 30, 30)),
])


def o(i):
    return "(O)" + str(i)


def v(local):
    return P + local


# Bundles (doc 14 §3): id → room, display, slots [(spec, count, min quality)], required (None = all), distinct, reward (item, count)
# A spec is an item ID, "#tag" (space-separated tags must all match) or "cat:<n>".
BUNDLES = OrderedDict([
    ("Kindling", ("FurnaceRoom", "Kindling", [(o(709), 10, 0), (o(382), 5, 0), (o(92), 20, 0)], None, False, (o(388), 50))),
    ("MineSalts", ("FurnaceRoom", "Mine Salts", [(o(334), 5, 0), (o(84), 1, 0), (o(86), 1, 0), (o(80), 3, 0)], None, False, (o(390), 50))),
    ("SpringGleanings", ("FurnaceRoom", "Spring Gleanings", [("#forage_item season_spring", 1, 0)] * 4, None, True, (o(368), 10))),
    ("FirstWaters", ("Cistern", "First Waters", [(v("RosMaialis"), 5, 0), (v("SalMarinum"), 10, 0), (v("Limus"), 10, 0)], None, False, (o(465), 10))),
    ("Fittings", ("Cistern", "Fittings", [(o(335), 5, 0), (o(338), 5, 0), (o(599), 1, 0)], None, False, (o(382), 20))),
    ("RiverAndLake", ("Cistern", "River and Lake", [("cat:-4", 1, 0)] * 4, None, True, (o(685), 30))),
    ("FirstFires", ("Library", "First Fires", [(v("Calx"), 10, 1), (v("Magma"), 5, 0), (v("Vitrum"), 1, 0)], None, False, (o(382), 15))),
    ("Curios", ("Library", "Curios", [("#item_type_arch", 1, 0)] * 3, None, True, (o(535), 5))),
    ("Bindings", ("Library", "Bindings", [(o(771), 50, 0), (o(330), 10, 0), ("#flower_item season_spring", 1, 0), ("#flower_item season_spring", 1, 0)], None, True, (o(770), 20))),
    ("Unions", ("Sanctum", "Unions", [(v("Vitrum"), 20, 0), (v("Vigor"), 1, 0), ("#valleyopus_tier_2", 1, 1), ("#valleyopus_tier_2", 1, 1), ("#valleyopus_tier_2", 1, 1)], None, True, (o(787), 1))),
    ("Offerings", ("Sanctum", "Offerings", [(o(340), 5, 0), ("#flower_item", 1, 0), ("#flower_item", 1, 0), ("#flower_item", 1, 0), ("cat:-75", 1, 2)], None, True, (o(446), 1))),
    ("SummerLight", ("Sanctum", "Summer Light", [(o(421), 1, 0), (o(254), 1, 0), (o(768), 5, 0)], None, False, (o(769), 3))),
    ("RotAndReturn", ("Undercroft", "Rot and Return", [(v("CaputMortuum"), 50, 0), (v("Miasma"), 10, 0), (v("Bitumen"), 10, 0)], None, False, (o(805), 2))),
    ("NightHarvest", ("Undercroft", "Night Harvest", [(o(422), 1, 0), (o(769), 5, 0), (o(767), 10, 0)], None, False, (o(287), 5))),
    ("DarkThings", ("Undercroft", "Dark Things", [("cat:-28", 1, 0)] * 3, None, True, (o(286), 10))),
    ("StarChart", ("Observatory", "Star Chart", [(v("Luna"), 3, 0), (v("Sol"), 3, 0), (v("Spiritus"), 3, 0)], 2, False, (o(74), 1))),
    ("ClearSpirits", ("Observatory", "Clear Spirits", [(v("AquaVitae"), 5, 0), (v("Elixir"), 1, 2), ("#alcohol_item", 1, 0)], None, False, (o(773), 3))),
    ("WinterSky", ("Observatory", "Winter Sky", [("#forage_item season_winter", 1, 0)] * 4, None, True, (o(414), 5))),
    ("FixedSalts", ("CrystalVault", "Fixed Salts", [(v("SalFixum"), 10, 0), (v("MercuriusFixus"), 1, 1), (v("Gelum"), 5, 0)], None, False, (o(910), 3))),
    ("Gems", ("CrystalVault", "Gems", [(o(72), 1, 0), (o(60), 1, 0), (v("Vitreus"), 10, 0)], None, False, (o(337), 3))),
    ("MetalsOfTheWeek", ("CrystalVault", "Metals of the Week", [(v("Argentum"), 1, 0), (v("Stannum"), 1, 0), (v("Plumbum"), 1, 0), (o(336), 5, 0), (o(335), 10, 0)], 4, False, (o(337), 5))),
    ("Nigredo", ("GreatWorkChamber", "Nigredo", [(v("Tenebrae"), 3, 0), (v("Mors"), 3, 0), (v("Perditio"), 3, 0)], None, False, (o(74), 1))),
    ("Albedo", ("GreatWorkChamber", "Albedo", [("#valleyopus_tier_2", 1, 1)] * 5, None, True, (o(74), 1))),
    ("Citrinitas", ("GreatWorkChamber", "Citrinitas", [("#valleyopus_tier_2", 1, 2)] * 3, None, True, (o(74), 1))),
    ("Rubedo", ("GreatWorkChamber", "Rubedo", [("#valleyopus_tier_2", 1, 4)], None, False, (o(74), 2))),
])

# The farm-secret candidates (doc 07): one per farm, hand-made at the altar. id, display, inputs, operation, gloss
SECRET_CANDIDATES = [
    ("AurumPotabile", "Aurum Potabile", ["Sol", "Vitae", "(O)336"], "Ritual", "Drinkable gold. The old masters swore by it, and went broke making it."),
    ("LacVirginis", "Lac Virginis", ["Luna", "SalFixum", "RosMaialis"], "Ritual", "Virgin's milk: the white water that whitens. A secret of this farm alone."),
    ("SanguisDraconis", "Sanguis Draconis", ["Cinnabaris", "Anima", "Magma"], "Ritual", "Dragon's blood, red and slow. Few farms ever see it."),
]

PRIME_LORE = {
    "Ignis": "Cyprian's note: \"Everything that burns gives some of this back.\"",
    "Aqua": "Cyprian's note: \"Even stone weeps a little Aqua, if you ask it right.\"",
    "Terra": "Cyprian's note: \"The commonest Prime, and the most patient.\"",
    "Aer": "Cyprian's note: \"Flowers are mostly breath.\"",
    "Sal": "Cyprian's note: \"The sea is full of it. So are tears.\"",
    "Sulphur": "Cyprian's note: \"Eggs, sap and anything that stinks.\"",
    "Mercurius": "Cyprian's note: \"It runs out of every vessel but one.\"",
    "Aether": "Cyprian's note: \"I have held it three times. Each time something rare was given up for it.\"",
}

MORE_LORE = [
    ("Vitrum", 1, "Tobin: \"Glass is just sand that's been convinced.\""),
    ("Vitrum", 10, "Bennet: \"Aunt Sandy says the desert sand sings when you melt it. I've never heard it. I keep listening.\""),
    ("Vitrum", 50, "You can tell good Vitrum by the ring it makes. Yours rings."),
    ("SalVolatile", 10, "Lark keeps a bottle in her pocket \"for emergencies\". There have been no emergencies."),
    ("Elixir", 1, "Oriel: \"Health is a balance, not a quantity. But a quantity helps.\""),
    ("Elixir", 10, "The Curator watches you bottle it with something like longing."),
    ("Rebis", 1, "The two-headed thing. It is warm on one side and cool on the other."),
    ("Orichalcum", 1, "Agnes ran the assay three times, then sat down for a while."),
    ("PrimaMateria", 1, "Cyprian: \"I looked for it in gold for forty years. It was in the dung heap all along.\""),
    ("LapisPhilosophorum", 1, "The Stone. It is smaller than you imagined, and heavier."),
    ("CaputMortuum", 50, "The dead head. Somewhere in this pile is the start of everything."),
]

MILESTONES = [
    ("FirstTier1", "FirstOfTier:1", "HoldUp", "Your first Tier 1 substance!"),
    ("FirstTier2", "FirstOfTier:2", "HoldUp", "A Tier 2 compound: two substances made one."),
    ("FirstTier3", "FirstOfTier:3", "Flourish", "A work of the third order. Cyprian would be proud."),
    ("Kindled", "Process:" + P + "FlammaViva", "HoldUp", "The Athanor is lit, and will stay lit."),
    ("FirstSteam", "Process:" + P + "Vapor", "HoldUp", "Steam! Tobin will want to hear about this."),
    ("FirstLightning", "Process:" + P + "Fulgur", "HoldUp", "Lightning in a coil. The hair on your arms stands up."),
    ("Tier1Complete", "TierComplete:1", "Flourish", "Every cell of the First Work is filled."),
    ("Tier2Complete", "TierComplete:2", "Flourish", "The Second Work is complete."),
    ("IgnisRow", "RowComplete:" + P + "Ignis", "Toast", "Every Ignis pair, found."),
    ("AquaRow", "RowComplete:" + P + "Aqua", "Toast", "Every Aqua pair, found."),
    ("TerraRow", "RowComplete:" + P + "Terra", "Toast", "Every Terra pair, found."),
    ("AerRow", "RowComplete:" + P + "Aer", "Toast", "Every Aer pair, found."),
    ("TheStone", "Process:" + P + "LapisPhilosophorum", "Flourish", "The Philosopher's Stone. The Great Work is done."),
]

# Hints (doc 07): id → target recipe, channel, reveals, requires unlock, divination cost, text
HINTS = OrderedDict()


def _hint(hid, target, channel, reveals, requires, cost, text):
    HINTS[hid] = (target, channel, reveals, requires, cost, text)


# Recipes the Table can't attempt get at least one input-naming hint, from the NPC whose subject it is.
_HINT_OWNERS = {
    "Ritual": "Oriel", "Conjunction": "Oriel", "Coagulation": "Curator", "Calcination": "Tobin", "Distillation": "Hesper",
    "Dissolution": "Delta", "Fermentation": "Curator", "Separation": "Tobin",
}


def display_of(local):
    for row in D.PRIMES:
        if row[0] == local:
            return row[1]
    for table in (D.TIER1, D.TIER2):
        for row in table:
            if row[0] == local:
                return row[1]
    for table in (F.TIER2, F.DARK_TWINS, F.TIER3):
        for row in table:
            if row[0] == local:
                return row[1]
    for row in SECRET_CANDIDATES:
        if row[0] == local:
            return row[1]
    if local == "CaputMortuum":
        return "Caput Mortuum"
    return {"(O)336": "a Gold Bar", "(O)340": "Honey", "#flower_item": "any flower", "#category_vegetable": "any vegetable"}.get(local, local)


APPARATUS_NAMES = {row[0]: row[1] for row in F.APPARATUS}
APPARATUS_NAMES["RitualCircle"] = "a Ritual Circle"


def recipe_hint(local, row):
    inputs = []
    for spec in row["Inputs"]:
        name = spec.get("Item") or ("#" + spec.get("Tag", ""))
        name = name[len(P):] if name.startswith(P) else name
        inputs.append(display_of(name) + (" ×" + str(spec["Count"]) if spec.get("Count", 1) > 1 else ""))
    places = [a[len(P):] for a in row["ApparatusIds"] if a != P + "ExperimentTable"]
    where = APPARATUS_NAMES.get(places[0], places[0]) if places else "the Experiment Table"
    return f"{display_of(local)}: {', '.join(inputs)}, worked in the {where}." if inputs else display_of(local)


# Contracts (doc 15 §6, doc 14 §1): Dr. Sorenson's bulk orders. id → item, count, gold, reward unlock (optional extra), weeks
CONTRACTS = OrderedDict([
    ("CalxBulk", (v("Calx"), 30, 1200, None, "Summer")),
    ("SalVolatileBulk", (v("SalVolatile"), 25, 1500, None, "Summer")),
    ("VitrumBulk", (v("Vitrum"), 20, 3000, P + "Contract_CleanRoom", "Summer")),
    ("BitumenBulk", (v("Bitumen"), 40, 1600, None, "Fall")),
    ("AquaRegiaOrder", (v("AquaRegia"), 10, 2500, P + "Contract_Curio1", "Fall")),
    ("ElixirOrder", (v("Elixir"), 10, 4000, P + "Contract_Curio2", "Winter")),
    ("TerraVivaBulk", (v("TerraViva"), 40, 3200, P + "Contract_Curio3", "Fall")),
    ("SmaltumOrder", (v("Smaltum"), 15, 1800, P + "Contract_Curio4", "Summer")),
    ("ElectrumOrder", (v("Electrum"), 5, 3500, P + "Contract_Curio5", "Winter")),
])
CONTRACT_REWARDS = {
    P + "Contract_CleanRoom": ("Flag", P + "CleanRoomSkin"),
    P + "Contract_Curio1": ("Item", "(O)" + P + "CurioSyntheticVial"),
    P + "Contract_Curio2": ("Item", "(O)" + P + "CurioRedactedNotebook"),
    P + "Contract_Curio3": ("Item", "(O)" + P + "CurioBrokenAssay"),
    P + "Contract_Curio4": ("Item", "(O)" + P + "CurioJojaBadge"),
    P + "Contract_Curio5": ("Item", "(O)" + P + "CurioCrystalSeed"),
}

# Familiars (doc 18 §3–4): id → element, operations, signature, meet location + condition, bond inputs, liked gift, tiny-creatures index
FAMILIARS = OrderedDict([
    ("Salamander", ("Ignis", ["Calcination"], "FuelSaving", ("UndergroundMine", "MINE_LEVEL 80"), ["Anima", "(O)82"], "PowerFlamma", "Magma", 45)),
    ("Undine", ("Aqua", ["Dissolution", "Fermentation"], "ChargeLength", ("Forest", "WEATHER Here Rain Storm Snow"), ["Anima", "RosMaialis"], None, "SalMarinum", 46)),
    ("Gnome", ("Terra", ["Conjunction", "Coagulation"], "CatalystSave", ("Mountain", "TRUE"), ["Anima", "Saxum"], None, "Limus", 47)),
    ("Sylph", ("Aer", ["Separation", "Distillation"], "CoilCharge", ("Mountain", "WEATHER Here Wind Storm"), ["Anima", "Pneuma"], None, "SalVolatile", 48)),
])

# Professions (doc 05 §4): id → level, parent, name, description, perks [(variable, scope kinds, op, value, bonus)]
PROFESSIONS = OrderedDict([
    ("Mechanist", (5, None, "Mechanist", "Engineered apparatus work 25% faster; Boilers power one more neighbour and Coils hold one more charge.",
                   [("Speed", ["Engineered"], "Add", 0.25, None), ("PowerReach", ["Engineered"], "Add", 1, None)])),
    ("Mystic", (5, None, "Mystic", "Rituals use one fewer catalyst; triggering an altar sets off your other staged altars within 3 tiles.",
                [("CatalystCount", ["RitualAltar", "RitualCircle"], "Add", -1, None), ("TriggerRadius", ["RitualAltar"], "Max", 3, None)])),
    ("Engineer", (10, "Mechanist", "Engineer", "Engineered apparatus reach gold quality; machine-fed ones burn 25% less fuel.",
                  [("QualityCap", ["Engineered"], "Max", 2, None), ("FuelCost", ["Engineered"], "Multiply", 0.75, "Fed")])),
    ("Artificer", (10, "Mechanist", "Artificer", "Minor arcane apparatus also work 25% faster; an altar beside a working Boiler may make better.",
                   [("Speed", ["MinorArcane"], "Add", 0.25, None), ("QualityChance", ["RitualAltar", "RitualCircle"], "Add", 0.25, "PowerVapor")])),
    ("Hermeticist", (10, "Mystic", "Hermeticist", "Bonus conditions on arcane work count twice; circle batches hold 25% more.",
                     [("BonusWeight", ["MinorArcane", "RitualAltar", "RitualCircle"], "Max", 2, None), ("BatchSize", ["RitualCircle"], "Multiply", 1.25, None)])),
    ("Theurgist", (10, "Mystic", "Theurgist", "Engineered apparatus within 2 tiles of a kindled Athanor make one step better.",
                   [("QualitySteps", ["Engineered"], "Add", 1, "NearKindled")])),
])

# NPCs (doc 08a, doc 14 §4): id → display, gender, age, home room/location + tile, birthday, unlock id, portrait base, hair, clothes colours, gifts, lines
NPCS = OrderedDict([
    ("Oriel", ("Oriel", "Female", "Adult", ("Library", 7, 5), ("Spring", 9), "Intro", "elf_f", ("f", 1, 0), ("#5B3A8C", "#3C2A5A"),
               ([v("Rebis"), v("PhialaStellarum"), o(74)], [v("Sol"), v("Luna"), o(66)]),
               {"Introduction": "So you're the one who opened the old Community Center. I'm Oriel. I study... the older kind of craft.",
                "Mon": "The Primes are everywhere, @. Everything you own is made of seven or eight of them.",
                "Tue": "My old teacher still thinks alchemy is cookery with pretensions. He isn't wrong, exactly.",
                "Wed": "A ritual is just a recipe that asks you to mean it.",
                "Thu": "Have you tried working something at the altar with a Boiler beside it? Tobin's steam and my rites agree more than either of us admits.",
                "Fri": "The Curator and I go back further than he remembers.",
                "Sat": "Gold is not the goal. Gold is the proof.",
                "Sun": "Rest today. The Work keeps."})),
    ("Tobin", ("Tobin", "Male", "Adult", ("Cistern", 7, 5), ("Summer", 14), "TobinArrives", "dwarf_m", ("m", 1, 1), ("#6E4B2A", "#2E3440"),
               ([v("Vitrum"), o(335), o(787)], [o(382), o(334), v("Saxum")]),
               {"Introduction": "Name's Tobin. Clint said someone was fixing up the old works under the Community Center. Let's see what she can do.",
                "Mon": "West in, east out, chest below, power above. Remember that and a line runs itself.",
                "Tue": "A Boiler steams for most of a day. Feed it Ignis and Aqua and don't let it run dry.",
                "Wed": "The Process Vat does four jobs, depending on what's beside it. Both a Boiler and a Coil at once? Nothing. Pick one.",
                "Thu": "The Curator and I had words about my Calcinator. He'll come round. Coal's honest.",
                "Fri": "Storms charge a Lightning Coil for free. Best weather there is, for a man like me.",
                "Sat": "If it can be done twice, it can be done by a machine.",
                "Sun": "Even engines need a day off. Not mine, but some engines."})),
    ("Finch", ("Finch", "Male", "Adult", ("EntranceHall", 24, 8), ("Fall", 3), "FinchRoute", "human_m", ("m", 0, 2), ("#C2542D", "#3B5E3A"),
               ([o(74), v("MelVitae")], [v("SalMarinum"), o(72)]),
               {"Introduction": "Finch, purveyor of the rare and the regrettable! I come by every week. Bring me your surplus, take my curiosities.",
                "Mon": "Fresh stock! Some of it even fresher than it looks.",
                "Tue": "A Prismatic Shard? I might have one. For a price. Everything's for a price.",
                "Wed": "I buy what you brew. Not everything, mind. Just the good stuff.",
                "Thu": "Aether's rare, friend. The rarer the thing you break down, the more of it you get.",
                "Fri": "The road's long and the wagon creaks, but the valley always pays.",
                "Sat": "Ask me about my curiosity. Go on.",
                "Sun": "Sunday prices are the same as Monday prices. Don't tell anyone."})),
    ("Sorenson", ("Dr. Sorenson", "Female", "Adult", ("Cistern", 11, 8), ("Winter", 21), "SorensonArrives", "human_f", ("f", 3, 0), ("#E8E8E8", "#4A5560"),
                  ([v("Vitrum"), v("AquaRegia")], [o(395), v("Calx")]),
                  {"Introduction": "Dr. Sorenson. Formerly of Joja Research. I have contracts, if you have volume.",
                   "Mon": "Joja wanted to patent 'synthetic essence'. I wanted to keep my soul. We parted ways.",
                   "Tue": "Standard product, standard price, on time. That's all a contract is.",
                   "Wed": "Bring me what I ordered and I'll pay above shipping. Hold one in your hand and talk to me.",
                   "Thu": "Industry isn't evil. Indifference is.",
                   "Fri": "The Saloon's coffee is better than Joja's. That's not a compliment to the Saloon.",
                   "Sat": "I'm told I should relax. I've scheduled it for next Tuesday.",
                   "Sun": "No contracts on Sunday. Mostly."})),
    ("Hesper", ("Hesper", "Female", "Adult", ("Observatory", 7, 5), ("Fall", 22), "HesperArrives", "elf_f", ("f", 0, 1), ("#2A3A6E", "#1C2340"),
                ([v("PhialaStellarum"), v("Luna")], [o(768), o(769)]),
                {"Introduction": "Hesper. I chart the stars and the currents under the valley. They're more alike than you'd think.",
                 "Mon": "Monday is the Moon's day. Silver grows on it.",
                 "Tue": "Tuesday belongs to Mars. Iron and fire.",
                 "Wed": "Mercury rules Wednesday. Quick hands, quick minds.",
                 "Thu": "Thursday is Jupiter's. Tin, and good fortune.",
                 "Fri": "Venus has Friday. Copper, and kindness.",
                 "Sat": "Saturn's day. Lead, and patience.",
                 "Sun": "The Sun's own day. Gold, and rest."})),
    ("Ivo", ("Ivo", "Male", "Adult", ("AdventureGuild", 9, 14), ("Winter", 6), "IvoGuild", "human_m", ("m", 2, 0), ("#4A3C2E", "#2A2A2A"),
             ([o(769), o(768)], [o(767), o(684)]),
             {"Introduction": "Ivo. I hunt for the gentlemen in the city. They pay for bits of monsters. So will I.",
              "Mon": "Bat wings. Slime. Bug meat. Bring it, I'll trade.",
              "Tue": "Rub Venenum on your blade before a fight. Thank me later.",
              "Wed": "Void Essence is the dark Sun. Don't ask me what that means, ask your mentor.",
              "Thu": "Bombs are just Pulvis Pyrius in a hurry.",
              "Fri": "Scars? Slimes. Mostly slimes.",
              "Sat": "The deeper you go, the better they pay.",
              "Sun": "Even hunters rest. Not long, though."})),
    ("Delta", ("Delta", "Female", "Adult", ("Beach", 20, 34), ("Summer", 2), "DeltaArrives", "human_f", ("f", 2, 2), ("#3A8FD8", "#E0D7B0"),
               ([v("SalMarinum"), o(392)], [o(152), o(393)]),
               {"Introduction": "Delta. Marine chemistry. The sea is the oldest alembic there is.",
                "Mon": "Seawater is Aqua and Sal, mostly. The rest is the interesting part.",
                "Tue": "Luna? Moon and water. Fish caught at night have a little more of it.",
                "Wed": "Hydragenum: water that remembers being mercury. Dissolve the two together.",
                "Thu": "Willy and I? Long story. Not one I'm telling.",
                "Fri": "Bring me fish I haven't seen. I'll tell you what's in them.",
                "Sat": "The tide pools are a whole kingdom, if you kneel.",
                "Sun": "Sunday at the beach. Nothing better."})),
    ("Agnes", ("Agnes", "Female", "Adult", ("Blacksmith", 8, 14), ("Spring", 24), "AgnesArrives", "dwarf_f", ("f", 3, 2), ("#7A7A7A", "#3A3A3A"),
               ([v("Orichalcum"), v("Argentum")], [o(336), o(337)]),
               {"Introduction": "Agnes, Ministry of Mining. I assay. I certify. I do not believe in transmutation.",
                "Mon": "Silver on Monday, tin on Thursday, lead on Saturday. I've seen it. I still don't believe it.",
                "Tue": "I'll buy metals at the shipping price. Certified. Hand them over.",
                "Wed": "The Quarry crucible is old Ministry property. Once the bridge is fixed, you may use it.",
                "Thu": "Clint is a good smith. He doesn't need to know what you do with his bars.",
                "Fri": "Orichalcum isn't real. Show me some and I'll change my report.",
                "Sat": "Assaying is just honesty with a scale.",
                "Sun": "Off duty. Still not believing."})),
    ("Bennet", ("Bennet", "Male", "Young", ("Desert", 18, 23), ("Summer", 26), "BennetArrives", "human_m", ("m", 1, 0), ("#E0A040", "#8A5A2B"),
                ([v("Vitrum"), v("RosaVitrea")], [o(80), o(338)]),
                {"Introduction": "Bennet! Sandy's nephew. I blow glass. The desert sand here is perfect.",
                 "Mon": "A Pelican circulates a substance round and round. Add Sal Fixum and it comes back finer.",
                 "Tue": "Aunt Sandy says hi. She always says hi.",
                 "Wed": "Glass is just patience you can see through.",
                 "Thu": "Bring me Vitrum and I'll show you shapes you've never thought of.",
                 "Fri": "The kiln's hottest on Fridays. Don't ask why. I don't know.",
                 "Sat": "Sometimes I just watch the light go through things.",
                 "Sun": "Even glass needs to cool."})),
    ("Curator", ("The Curator", "Male", "Adult", ("EntranceHall", 14, 5), ("Fall", 13), "Intro", None, None, None,
                 ([v("Spiritus"), v("Ectoplasm")], [v("Anima"), v("QuintaEssentia")]),
                 {"Introduction": "Ah. A visitor. I keep the workshop. Or it keeps me. Will you help me restore it?",
                  "Mon": "Each room asks for things. The board will tell you what.",
                  "Tue": "This workshop was built for the Great Work. It was never finished.",
                  "Wed": "Tobin's coal-furnace offends me. It also works. I am... reconsidering.",
                  "Thu": "Spiritus. Air and Aether. Some of us are made of little else.",
                  "Fri": "I remember a name. It might have been mine.",
                  "Sat": "The Undercroft is dark for a reason. Things rot well in the dark.",
                  "Sun": "Sunday. Once, there were bells."})),
    ("Lark", ("Lark", "Female", "Teen", ("EntranceHall", 6, 9), ("Spring", 3), "LarkArrives", "human_f", ("f", 1, 2), ("#E3C21E", "#5B8C3A"),
              ([v("SalVolatile"), o(221)], [o(18), o(22)]),
              {"Introduction": "You're the alchemist! I'm Lark! Can I watch? I won't touch anything. Much.",
               "Mon": "I tried mixing salt and sulphur. It turned green! Is that good?",
               "Tue": "What happens if you put two of the same thing in? Like, two airs?",
               "Wed": "I made residue! Lots of it. Do you want some?",
               "Thu": "Have you tried things in the dark? Mum says nothing good happens in the dark. I think she's wrong.",
               "Fri": "One day I'll have my own workshop. With a Boiler. Two Boilers!",
               "Sat": "I read the Curator's journal. Well, the parts that weren't burnt.",
               "Sun": "Can we do alchemy on Sunday? Please?"})),
])

# Shops (Data/Shops): id → owner, items [(item, price or None, trade item, trade count, condition unlock)], buys tags
SHOPS = OrderedDict([
    ("Finch", ("Finch", [(o(768), 120, None, 0, None), (o(769), 120, None, 0, None), (o(74), 5000, None, 0, None),
                         ("(O)" + P + "Aether", 400, None, 0, None), (o(395), 300, None, 0, None), (o(340), 200, None, 0, None)],
               ["valleyopus_substance"])),
    ("Tobin", ("Tobin", [(o(382), 150, None, 0, None), (o(334), 400, None, 0, None), ("(BC)" + P + "Calcinator", 1500, None, 0, "TobinArrives"),
                         ("(BC)" + P + "Boiler", 2500, None, 0, "Cistern"), ("(BC)" + P + "ProcessVat", 2500, None, 0, "Cistern")], [])),
    ("Agnes", ("Agnes", [(o(334), 300, None, 0, None), (o(335), 600, None, 0, None)], ["valleyopus_metal_bar"])),
    ("Bennet", ("Bennet", [("(BC)" + P + "Alembic", 800, None, 0, None), ("(BC)" + P + "Retort", 800, None, 0, None),
                           ("(BC)" + P + "Pelican", 4000, None, 0, None), (o(338), 200, None, 0, None)], [])),
    ("Ivo", ("Ivo", [(o(287), None, o(767), 10, None), (o(288), None, o(769), 5, None), ("(O)" + P + "Venenum", None, o(684), 20, None),
                     (o(773), None, o(766), 50, None)], [])),
    ("Hesper", ("Hesper", [("(O)" + P + "Luna", 600, None, 0, None), (o(797), 1000, None, 0, None)], [])),
])

# Mail (Data/mail): id → text. "@" is the farmer's name; "%item id <qid> <count> %%" attaches items.
MAIL = OrderedDict([
    ("Mail_Welcome", "@,^Welcome to the workshop. The Curator will show you the bundle board in the Entrance Hall.^Decompose things in the Athanor, combine Primes at the Table, and write down everything.^   -Oriel"),
    ("Mail_Tobin", "Heard you got the old furnace lit.^I'll be in the workshop from today. I've brought plans for a coal-fired Calcinator. Faster than that old Athanor, if less pretty.^   -Tobin"),
    ("Mail_Finch", "Greetings, alchemist!^My wagon rolls into the workshop's hall each week. Rare reagents, fair prices, and a curiosity or two.^   -Finch"),
    ("Mail_Ivo", "Farmer.^I'm at the Guild. I trade for monster parts. Come and see.^   -Ivo"),
    ("Mail_Delta", "Hello!^I'm doing research at the tide pools. If you've caught anything unusual, I'd love to see it.^   -Delta"),
    ("Mail_Bennet", "Hi! Aunt Sandy says you're the alchemist everyone's talking about.^I've got a kiln out in the desert, and plans for a Pelican: glass that makes substances finer. Come by!^   -Bennet"),
    ("Mail_Chaining", "Right. Lesson one.^Put a machine east of another and the first one passes its output along. West in, east out. A chest south of a machine feeds it.^Power goes north. Do that and your line runs itself.^   -Tobin"),
    ("Mail_Sorenson", "To the farmer at the old works,^I am Dr. Sorenson, lately of Joja Research. I offer bulk contracts at above-shipping prices.^Find me in the Cistern.^   -Dr. Sorenson"),
    ("Mail_Coil", "That storm last night! Got me thinking.^Here's how to build a Lightning Coil. It stores Fulgur as charges, and storms top it up for free.^   -Tobin"),
    ("Mail_Library", "@,^The Library is open again. The Experiment Table now holds three things at once.^Bring Caput Mortuum to the bundle board if you want me to read it for you.^   -the Curator"),
    ("Mail_Hesper", "Dear @,^I'll be visiting the workshop's Observatory a few days each week. I chart the valley's currents. They run under your feet.^   -Hesper"),
    ("Mail_LeyTap", "@,^You've been kind to me, so here is my oldest secret: the Ley Tap. Place it on a ley node and it draws that node's Primes each morning.^The nodes I've charted are in your codex.^   -Hesper"),
    ("Mail_Agnes", "Farmer,^Agnes, Ministry of Mining. I'm stationed at Clint's. I buy and certify metals.^   -Agnes"),
    ("Mail_Crucible", "Farmer,^The Quarry is reachable again. The Ministry's old crucible is yours to use: plans enclosed.^I remain unconvinced about the planetary days.^   -Agnes"),
    ("Mail_Sanctum", "@,^The Sanctum is restored. The Ritual Altar is yours to build. And the Wizard showed me something: a node under the forest that breathes Aether.^   -Oriel"),
    ("Mail_Undercroft", "Child,^You opened the dark room under the hall. Good. Here is how to make a lantern that holds Shades. Sleep with it, and it fills.^   -the Witch"),
    ("Mail_Observatory", "Dear @,^With the Observatory open I've charted the rest of the valley's nodes. They're in your codex now.^   -Hesper"),
    ("Mail_Vault", "@,^The Crystal Vault is restored. Lay a Circle Sigil under an altar and you have a Ritual Circle: one great rite at a time.^   -the Curator"),
    ("Mail_GreatWork", "@,^You have restored every room. I remember now. My name was Cyprian, and this was my workshop.^The Great Work is yours to finish.^   -Cyprian"),
    ("Mail_Lark", "Hi!!^I'm Lark, I'm visiting my aunt, and I heard you do ALCHEMY. Can I come and watch? I'll be in the workshop.^   -Lark"),
])


def fill(registries, i18n, learned_by):
    """Writes the world's registry rows and strings into the base pack's tables."""
    for uid, (kind, grants, source, requires, pace, name, how) in UNLOCKS.items():
        row = OrderedDict([
            ("Kind", kind),
            ("Grants", [OrderedDict([("Type", t), ("Ref", r)] + ([("Count", c)] if c else [])) for t, r, c in grants]),
            ("Source", {"Type": source[0], "Ref": source[1]}),
            ("Requires", [GATES[r[5:]] if r.startswith("gate:") else P + r for r in requires]),
            ("Pace", pace), ("NameKey", "unlock." + uid + ".name"), ("SourceKey", "unlock." + uid + ".how")])
        registries["Unlocks"][P + uid] = row
        i18n["unlock." + uid + ".name"] = name
        i18n["unlock." + uid + ".how"] = how

    for rid, (tier, motif, host, opens, bundles, display, size, floor, wall) in ROOMS.items():
        registries["WorkshopRooms"][P + rid] = OrderedDict([
            ("Tier", tier), ("Motif", P + motif if motif else None), ("Host", P + host),
            ("Opens", {opens[0]: [P + r for r in opens[1]]}), ("Bundles", [P + b for b in bundles]),
            ("Unlocks", [P + rid] if P + rid in registries["Unlocks"] or rid in UNLOCKS else []),
            ("Location", P + "Workshop_" + rid), ("NameKey", "room." + rid + ".name")])
        i18n["room." + rid + ".name"] = display
    registries["WorkshopRooms"][P + "GreatWorkChamber"]["Unlocks"] = [P + "GreatWork"]

    for bid, (room, display, slots, required, distinct, reward) in BUNDLES.items():
        slot_rows = []
        for spec, count, quality in slots:
            s = OrderedDict()
            if spec.startswith("#"):
                s["Tag"] = spec[1:]
            elif spec.startswith("cat:"):
                s["Category"] = int(spec[4:])
            else:
                s["Item"] = spec
            if count > 1:
                s["Count"] = count
            if quality:
                s["MinQuality"] = quality
            slot_rows.append(s)
        row = OrderedDict([("Room", P + room), ("Slots", slot_rows), ("Distinct", distinct),
                           ("Reward", {"Item": reward[0], "Count": reward[1]}), ("NameKey", "bundle." + bid + ".name")])
        if required:
            row["Required"] = required
        registries["WorkshopBundles"][P + bid] = row
        i18n["bundle." + bid + ".name"] = display

    for nid, location, tile, profile, units, reveal in F.LEY_NODES:
        registries["LeyNodes"][P + nid] = OrderedDict([
            ("Location", location), ("Tile", {"X": tile[0], "Y": tile[1]}), ("Profile", {P + k: n for k, n in profile.items()}),
            ("Units", units), ("RevealedBy", P + reveal), ("NameKey", "ley." + nid + ".name")])
        i18n["ley." + nid + ".name"] = f"The {nid} node"

    # Hints: every recipe the Table can't attempt gets an NPC hint naming its inputs; every recipe gets a divination hint.
    for rid, row in registries["Recipes"].items():
        local = rid[len(P):]
        if row.get("SecretPool"):
            owner, requires = "Hesper", "Observatory"
        else:
            op = row["Operation"][len(P):]
            owner = _HINT_OWNERS.get(op, "Oriel")
            requires = {"Oriel": "Intro", "Curator": "Intro", "Tobin": "TobinArrives", "Hesper": "HesperArrives", "Delta": "DeltaArrives"}[owner]
        if not row.get("Discovery", {}).get("TableAttemptable", True) or row.get("SecretPool"):
            _hint(local + "_npc", rid, owner, "Inputs", requires, 0, "hint.recipe." + local)
        _hint(local + "_div", rid, "Divination", "Inputs", "Library", 5 * (1 + (2 if row.get("RequiresUnlock") else 0)), "hint.recipe." + local)
    for hid, (target, channel, reveals, requires, cost, key) in HINTS.items():
        registries["Hints"][P + hid] = OrderedDict([
            ("Target", target), ("Channel", channel), ("Reveals", reveals), ("Requires", P + requires),
            ("DivinationCost", cost), ("TextKey", key)])

    for cid, (item, count, gold, reward, pace) in CONTRACTS.items():
        row = OrderedDict([("Item", "(O)" + item), ("Count", count), ("Gold", gold), ("Pace", pace), ("NameKey", "contract." + cid + ".name")])
        if reward:
            row["RewardUnlock"] = reward
        registries["Contracts"][P + cid] = row
        i18n["contract." + cid + ".name"] = f"{count} {display_of(item[len(P):])}"
    for uid, (kind, ref) in CONTRACT_REWARDS.items():
        registries["Unlocks"][uid] = OrderedDict([
            ("Kind", "Feature"), ("Grants", [{"Type": kind, "Ref": ref, "Count": 1}]), ("Source", {"Type": "Contract", "Ref": ""}),
            ("Requires", []), ("Pace", "Fall"), ("NameKey", "unlock." + uid[len(P):] + ".name"), ("SourceKey", "unlock." + uid[len(P):] + ".how")])
        i18n["unlock." + uid[len(P):] + ".name"] = "A keepsake from Dr. Sorenson"
        i18n["unlock." + uid[len(P):] + ".how"] = "Fulfil one of Dr. Sorenson's contracts."

    for fid, (element, ops, signature, meet, bond, bond_power, liked, sprite) in FAMILIARS.items():
        registries["Familiars"][P + fid] = OrderedDict([
            ("Element", P + element), ("Operations", [P + op for op in ops]), ("Signature", signature),
            ("Meet", {"Location": meet[0], "Condition": meet[1]}), ("BondRecipe", P + "Bond" + fid),
            ("LikedGifts", [{"Item": P + liked}]), ("Sprite", sprite), ("NameKey", "familiar." + fid + ".name")])
        i18n["familiar." + fid + ".name"] = fid
        rite = OrderedDict([
            ("Inputs", [{"Item": i if i.startswith("(") else P + i} for i in bond]), ("Operation", P + "Ritual"),
            ("ApparatusIds", [P + "RitualAltar"]), ("NeighbourStates", ["Open"]),
            ("Conditions", [{"ConditionId": P + bond_power, "Use": "Required"}] if bond_power else []),
            ("Output", {"Id": P + "FamiliarBond_" + fid}), ("Quality", {"Mode": "Fixed", "Steps": 0}), ("Minutes", 10),
            ("HandMadeOnly", True), ("Discovery", {"TableAttemptable": False, "HintKeys": ["hint.recipe.Bond" + fid]})])
        registries["Recipes"][P + "Bond" + fid] = rite
        registries["Substances"][P + "FamiliarBond_" + fid] = OrderedDict([("Class", "ProcessState"), ("Tier", 3), ("NameKey", "substance.FamiliarBond_" + fid)])
        i18n["substance.FamiliarBond_" + fid] = f"{fid}'s bond"
        i18n["hint.recipe.Bond" + fid] = f"To bond the {fid}: Anima and {display_of(bond[1]) if not bond[1].startswith('(') else 'Fire Quartz'}, at an altar in a great Lab."

    for pid, (level, parent, name, desc, perks) in PROFESSIONS.items():
        registries["Professions"][P + pid] = OrderedDict([
            ("Level", level), ("Parent", P + parent if parent else None), ("NameKey", "profession." + pid + ".name"),
            ("DescKey", "profession." + pid + ".desc"),
            ("Perks", [OrderedDict([("Variable", var), ("Scope", scope), ("Op", op), ("Value", value)] + ([("Bonus", bonus)] if bonus else []))
                       for var, scope, op, value, bonus in perks])])
        i18n["profession." + pid + ".name"] = name
        i18n["profession." + pid + ".desc"] = desc

    for local, learn in learned_by.items():
        registries["Unlocks"][P + "Learn_" + local[len(P):]] = OrderedDict([
            ("Kind", "Feature"), ("Grants", [{"Type": "CraftingRecipe", "Ref": local}]), ("Source", {"Type": "Discovery", "Ref": learn}),
            ("Requires", []), ("Pace", "Summer"), ("NameKey", "unlock.Learn.name"), ("SourceKey", "unlock.Learn.how")])
    i18n["unlock.Learn.name"] = "A crafting recipe"
    i18n["unlock.Learn.how"] = "Discover the substance it is made from."
