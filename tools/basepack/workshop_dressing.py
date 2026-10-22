"""Furniture for the workshop and Laboratory maps (sos.3): props laid out from the game's own interior tilesheets by
tile index, so nothing vanilla is copied into the pack (art rule: vanilla art may be referenced, never shipped). Most
props were read off vanilla maps (the Wizard's tower and basement, the Blacksmith, Robin's house and Maru's lab, the
museum); the rest come straight off townInterior. Each prop lists its tiles per map layer, top row first; a bottom row on
Buildings blocks movement, rows on Front draw over the player."""

U2 = 100000  # tiles at or above this are on townInterior_2; the rest are on townInterior

PROPS = {
    "potion_shelf": {
        "Buildings": [[None, None], [None, None], [579, 580], [643, 644]],
        "Front": [[532, 533], [564, 565], [None, None], [None, None]],
        "Front2": [[None, None], [None, None], [None, 833], [None, None]],
    },
    "cabinet": {
        "Buildings": [[None, None], [579, 580], [611, 612], [643, 644]],
        "Front": [[1424, 993], [579, 707], [None, None], [None, None]],
    },
    "bookcase": {
        "Buildings": [[579, 640, 640, 640, 580], [611, 672, 672, 673, 612], [611, 770, 672, 672, 612], [643, 704, 704, 704, 644]],
        "Front": [[None, 640, U2 + 55, 833, 580], [None, None, None, None, None], [None, None, None, None, None], [None, None, None, None, None]],
        "Front2": [[None, U2 + 87, None, None, None], [None, None, None, None, None], [None, None, None, None, None], [None, None, None, None, None]],
    },
    "cauldron": {
        "Buildings": [[None, None, None], [1242, 1243, 1244], [1274, 1275, 1276]],
        "Front": [[1210, 1211, 1212], [1213, 1245, 1277], [None, None, None]],
    },
    "circle": {
        "Buildings": [[1175, 1176, 1177], [1207, 1208, 1209], [1239, 1240, 1241]],
    },
    "fireplace": {
        "Buildings": [[None, None], [None, None], [732, 733], [764, 765], [796, 797]],
        "Front": [[668, 669], [700, 701], [732, 733], [None, None], [None, None]],
    },
    "potion_table": {
        "Buildings": [[None, None, None], [None, 521, None], [504, 505, 506], [536, 537, 538]],
        "Front": [[None, 489, None], [472, 473, 474], [None, 1178, 651], [None, None, 503]],
    },
    "rug_gold": {
        "Back": [[251, 252, 252, 253], [283, 284, 284, 285], [315, 316, 316, 317]],
    },
    "forge": {
        "Buildings": [[None, None, None, None], [None, None, None, None], [903, 904, 905, 906], [935, 936, 937, 938], [967, 968, 969, 970], [None, 1000, 1001, None]],
        "Front": [[None, 668, 669, 1591], [None, 700, 701, 1623], [None, None, None, None], [None, None, None, None], [None, None, None, None], [None, None, None, None]],
    },
    "anvil": {
        "Buildings": [[None, None], [965, 966]],
        "Front": [[933, 934], [None, None]],
    },
    "coal": {
        "Front": [[U2 + 1031, U2 + 1032]],
    },
    "mortar_table": {
        "Buildings": [[None, None, None, None], [504, 505, 506, None], [536, 537, 538, 456]],
        "Front": [[472, 473, 474, None], [None, 366, None, 424], [None, 490, None, None]],
    },
    "plant_gold": {
        "Buildings": [[None], [534]],
        "Front": [[502], [None]],
    },
    "lab_bench": {
        "Buildings": [[1003, 1003, 972, 1003], [1005, 1005, 1005, 1005]],
        "Front": [[973, 1045, 972, 1044], [None, None, 812, None]],
    },
    "beaker_shelf": {
        "Front": [[U2 + 1181, U2 + 1182], [U2 + 1213, U2 + 1214]],
    },
    "periodic": {
        "Front": [[1076, 1077]],
    },
    "barrels": {
        "Buildings": [[None, None], [1367, 1367]],
        "Front": [[1335, 1335], [None, None]],
    },
    "logs": {
        "Buildings": [[1796, 1797]],
    },
    "orbs": {
        "Buildings": [[1178]],
    },
    "mushrooms": {
        "Buildings": [[None, None], [U2 + 50, U2 + 51]],
        "Front": [[U2 + 18, U2 + 19], [None, None]],
    },
    "sconce": {
        "Buildings": [[1269]],
    },
    "bookshelf": {
        "Buildings": [[None, None, None, None, None], [None, None, None, None, None], [None, None, None, None, None], [643, 704, 704, 704, 644]],
        "Front": [[579, 640, 834, 640, 580], [611, 672, 672, 672, 612], [611, 770, 672, 673, 612], [None, None, None, None, None]],
        "Front2": [[None, U2 + 55, None, None, None], [None, None, None, None, None], [None, None, None, None, None], [None, None, None, None, None]],
    },
    "bookshelf_double": {
        "Buildings": [[None, None, None, None, None, None], [None, None, None, None, None, None], [None, None, None, None, None, None], [643, 704, 704, 704, 704, 704]],
        "Front": [[579, 334, 640, 640, 640, 833], [611, 672, 673, 612, 770, 672], [611, 770, 672, 612, 672, 673], [None, None, None, 706, None, None]],
    },
    "plant_tall": {
        "Buildings": [[None], [363]],
        "Front": [[331], [None]],
    },
    "palm": {
        "Buildings": [[None, None], [None, None], [None, 400]],
        "Front": [[None, None], [None, 368], [None, None]],
        "AlwaysFront": [[335, 336], [None, None], [None, None]],
    },
    "reading_table": {
        "Buildings": [[655, 657], [687, 689]],
        "Front": [[None, 480], [None, None]],
    },
    # Straight off townInterior.
    "telescope": {"Front": [[860], [None]], "Buildings": [[None], [892]]},
    "reagent_cabinet": {"Front": [[1287, 1288], [1319, 1320], [None, None]], "Buildings": [[None, None], [None, None], [1351, 1352]]},
    "ladder": {"Front": [[1289], [1321], [1353]]},
    "drum": {"Buildings": [[1354]]},
    "coal_bin": {"Front": [[1522, 1523], [None, None]], "Buildings": [[None, None], [1554, 1555]]},
    "desk_lamp": {"Front": [[1548, 1549], [None, None], [None, None]], "Buildings": [[None, None], [1580, 1581], [1612, 1613]]},
    "pipe_valve": {"Front": [[1519], [1551], [1583], [1615]]},
    "map_table": {"Front": [[1329, 1330], [None, None]], "Buildings": [[None, None], [1361, 1362]]},
    "vines": {"Front": [[1786, 1787], [1818, 1819]]},
    "bookshelf_small": {"Front": [[577, 578], [609, 610], [None, None]], "Buildings": [[None, None], [None, None], [641, 642]]},
    "tank": {"Front": [[1647, 1648], [None, None]], "Buildings": [[None, None], [1679, 1680]]},
    "boiler": {"Front": [[1777, 1778], [None, None], [None, None]], "Buildings": [[None, None], [1809, 1810], [1841, 1842]]},
}

WOOD_FLOOR = [[231, 232], [263, 264]]  # townInterior's plank floor, which the rugs above are drawn on


# What each workshop room keeps along its back wall, as (prop, column in the room's corner, row). The bundle board hangs
# in columns 1-2, so they stay clear down to the floor row the player stands on; NPC homes and the Undercroft's dark floor
# stay open.
ROOM_PROPS = {
    "FurnaceRoom": [("forge", 3, 1), ("anvil", 5, 7), ("coal_bin", 0, 8)],
    "Cistern": [("pipe_valve", 3, 1), ("boiler", 4, 2), ("pipe_valve", 6, 1), ("drum", 0, 4), ("barrels", 5, 8)],
    "Library": [("bookshelf_small", 3, 1), ("bookshelf_small", 5, 1), ("reading_table", 4, 7), ("plant_tall", 0, 3)],
    "Sanctum": [("potion_shelf", 3, 1), ("cabinet", 5, 1), ("orbs", 0, 4)],
    "Undercroft": [("vines", 5, 1), ("mushrooms", 3, 3), ("barrels", 5, 3)],
    "Observatory": [("ladder", 3, 1), ("map_table", 4, 3), ("telescope", 6, 3)],
    "CrystalVault": [("reagent_cabinet", 3, 2), ("beaker_shelf", 5, 1), ("orbs", 6, 4)],
    "GreatWorkChamber": [("orbs", 0, 4), ("orbs", 6, 4), ("circle", 2, 7)],
}

# The open hall in front of the rooms, in map tiles. The middle stays clear for the intro's apparatus and the NPCs who
# stand there; the gold rug sits in front of the door.
HALL_PROPS = [
    ("palm", 2, 10), ("palm", 56, 10), ("plant_gold", 2, 16), ("plant_gold", 57, 16),
    ("cauldron", 6, 12), ("potion_table", 11, 11), ("barrels", 9, 16),
    ("desk_lamp", 44, 11), ("mortar_table", 48, 12), ("logs", 50, 17),
    ("rug_gold", 28, 15),
]
HALL_CLEAR = (20, 13, 40, 17)  # x0, y0, x1, y1 (inclusive) with nothing on Buildings


# The Laboratory keeps its furniture on the back wall: every floor tile is room for apparatus (doc 18 §2), the hearths
# sit at columns 5 (and 15) and a T3 Lab's shrine niche is two columns in from the right wall, lit by a sconce each side.
def lab_props(tier, w):
    props = [("beaker_shelf", 2, 1), ("sconce", 8, 2), ("periodic", 9, 1), ("bookshelf_small", 11, 1), ("sconce", 13, 2)]
    if tier == 2:
        props += [("beaker_shelf", w - 4, 1)]
    if tier >= 3:
        props += [("sconce", w - 4, 2), ("sconce", w - 2, 2)]
    return props


LAB_ALCOVE_FLOOR = 61  # dark slate: the shuttered corners read as dark at a glance
