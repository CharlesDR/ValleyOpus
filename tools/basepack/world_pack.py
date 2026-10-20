"""Writes the base pack's world: the workshop's maps and locations, the Laboratory buildings, the NPCs (data, dialogue,
sprites and portraits), mail, shops and the soft-start event. Called by build_base_pack.py.

Maps reference the game's own tilesheets (townInterior, walls_and_floors) by name; SMAPI loads them from the game's
Content/Maps at runtime, so no vanilla texture is copied into the pack (art license rule)."""

import os
from collections import OrderedDict

import world_data as W

P = W.P
TOWN_ENTRANCE = (53, 9)   # behind the Community Center (S13; Charles: never the front door)
TOWN_RETURN = (53, 10)

# Tile indices, read from the vanilla Shed map: u = townInterior, w = walls_and_floors.
BORDER = {"tl": 9, "top": 2, "tr": 11, "left": 64, "right": 68, "bl": 160, "br": 167,
          "bottom": 165, "bottom_l": 161, "bottom_r": 166, "door_l": 162, "door_r": 163,
          "sill": 0, "sill_door_l": 96, "sill_door": 165, "sill_door_r": 130}
BOARD = [241, 242, 273, 274]            # a framed chart, 2x2: the bundle board
SHELF = [577, 578, 609, 610, 641, 642]  # a bookshelf, 2x3
HEARTH = [666, 667, 698, 699, 730, 731]  # a brick chimney breast, 2x3


def wall_tiles(style):
    base = (style % 16) + 48 * (style // 16)
    return base, base + 16, base + 32


def floor_tiles(style):
    base = 336 + (style % 8) * 2 + (style // 8) * 32
    return base, base + 1, base + 16, base + 17


class TileMap:
    """A small Stardew map in Tiled's TMX format: Back, Buildings, Front and AlwaysFront, plus tile properties."""

    SHEETS = [("untitled tile sheet", "townInterior", 512, 1088), ("walls_and_floors", "walls_and_floors", 256, 688)]

    def __init__(self, w, h):
        self.w, self.h = w, h
        self.layers = OrderedDict((name, [[None] * w for _ in range(h)]) for name in ["Back", "Buildings", "Front", "AlwaysFront"])
        self.tile_props = OrderedDict((name, {}) for name in self.layers)
        self.props = OrderedDict()

    def set(self, layer, x, y, sheet, index):
        if 0 <= x < self.w and 0 <= y < self.h:
            self.layers[layer][y][x] = (sheet, index)

    def clear(self, layer, x, y):
        self.layers[layer][y][x] = None

    def prop(self, layer, x, y, key, value):
        self.tile_props[layer].setdefault((x, y), OrderedDict())[key] = value

    def walkable(self, x, y):
        return self.layers["Back"][y][x] is not None and self.layers["Buildings"][y][x] is None

    def tmx(self):
        firstgid, gids = 1, {}
        sets = []
        for sid, image, iw, ih in self.SHEETS:
            count = (iw // 16) * (ih // 16)
            gids[sid] = firstgid
            sets.append(f' <tileset firstgid="{firstgid}" name="{sid}" tilewidth="16" tileheight="16" tilecount="{count}" columns="{iw // 16}">\n'
                        f'  <image source="{image}.png" width="{iw}" height="{ih}"/>\n </tileset>')
            firstgid += count
        out = ['<?xml version="1.0" encoding="UTF-8"?>',
               f'<map version="1.10" tiledversion="1.10.2" orientation="orthogonal" renderorder="right-down" width="{self.w}" height="{self.h}" tilewidth="16" tileheight="16" infinite="0">']
        if self.props:
            out.append(" <properties>")
            for k, v in self.props.items():
                out.append(f'  <property name="{k}" value="{xml(v)}"/>')
            out.append(" </properties>")
        out += sets
        lid = 1
        for name, rows in self.layers.items():
            data = ",\n".join(",".join(str(gids[t[0]] + t[1]) if t else "0" for t in row) for row in rows)
            out.append(f' <layer id="{lid}" name="{name}" width="{self.w}" height="{self.h}">\n  <data encoding="csv">\n{data}\n  </data>\n </layer>')
            lid += 1
        oid = 1
        for name, props in self.tile_props.items():
            if not props:
                continue
            out.append(f' <objectgroup id="{lid}" name="{name}">')
            lid += 1
            for (x, y), kv in props.items():
                out.append(f'  <object id="{oid}" name="TileData" x="{x * 16}" y="{y * 16}" width="16" height="16">\n   <properties>')
                oid += 1
                for k, v in kv.items():
                    out.append(f'    <property name="{k}" value="{xml(v)}"/>')
                out.append("   </properties>\n  </object>")
            out.append(" </objectgroup>")
        out.append("</map>")
        return "\n".join(out) + "\n"


def xml(v):
    return str(v).replace("&", "&amp;").replace('"', "&quot;").replace("<", "&lt;").replace(">", "&gt;")


U, WF = "untitled tile sheet", "walls_and_floors"


def room_shell(w, h, wall_style, floor_style, door_x):
    """A Shed-style room: a three-tile wall at the top, a floor, and a door in the bottom wall at door_x."""
    m = TileMap(w, h)
    wt = wall_tiles(wall_style)
    ft = floor_tiles(floor_style)
    for x in range(w):
        for layer in ("Buildings", "Front"):
            m.set(layer, x, 0, U, BORDER["tl"] if x == 0 else BORDER["tr"] if x == w - 1 else BORDER["top"])
        if 0 < x < w - 1:
            m.set("Back", x, 1, WF, wt[0])
            m.set("Back", x, 2, WF, wt[1])
            m.set("Buildings", x, 3, WF, wt[2])
        for y in range(3, h):
            m.set("Back", x, y, WF, ft[(x % 2) + 2 * (y % 2)])
    for y in range(1, h - 2):
        for layer in ("Buildings", "Front"):
            m.set(layer, 0, y, U, BORDER["left"])
            m.set(layer, w - 1, y, U, BORDER["right"])
    m.set("Buildings", 0, h - 2, U, BORDER["bl"])
    m.set("Buildings", w - 1, h - 2, U, BORDER["br"])
    for x in range(w):
        if x == 0:
            front = BORDER["bl"]
        elif x == w - 1:
            front = BORDER["br"]
        elif x == 1:
            front = BORDER["bottom_l"]
        elif x == w - 2:
            front = BORDER["bottom_r"]
        elif x == door_x - 1:
            front = BORDER["door_l"]
        elif x == door_x + 1:
            front = BORDER["door_r"]
        elif x == door_x:
            front = None
        else:
            front = BORDER["bottom"]
        if front is not None:
            m.set("Front", x, h - 2, U, front)
        sill = {door_x - 1: BORDER["sill_door_l"], door_x: BORDER["sill_door"], door_x + 1: BORDER["sill_door_r"]}.get(x, BORDER["sill"])
        m.set("Front", x, h - 1, U, sill)
        if x != door_x:
            m.set("Buildings", x, h - 1, U, 0)
    m.props["AmbientLight"] = "255 255 255"
    m.props["AmbientNightLight"] = "180 180 0"
    return m


def stamp(m, layer_bottom, x, y, tiles, cols=2):
    """Puts a cols-wide piece of furniture with its bottom row on Buildings and the rest on Front."""
    rows = len(tiles) // cols
    for r in range(rows):
        for c in range(cols):
            layer = layer_bottom if r == rows - 1 else "Front"
            m.set(layer, x + c, y + r, U, tiles[r * cols + c])


def location_name(room):
    return P + "Workshop_" + room


ROOM_DOORS = OrderedDict()  # room → hall door column


def workshop_maps():
    """The Entrance Hall and one map per room; returns {map asset suffix: TileMap} and the hall arrival tile."""
    maps = OrderedDict()
    hall_w, hall_h = W.ROOMS["EntranceHall"][6]
    hall = room_shell(hall_w, hall_h, 0, 0, hall_w // 2)
    rooms = [r for r in W.ROOMS if r != "EntranceHall"]
    for i, room in enumerate(rooms):
        x = 3 + 3 * i
        ROOM_DOORS[room] = x
        hall.clear("Back", x, 1)
        hall.clear("Back", x, 2)
        hall.set("Back", x, 3, WF, floor_tiles(0)[0])
        hall.clear("Buildings", x, 3)
        hall.prop("Back", x, 3, "TouchAction", P + "Door " + P + room)
    stamp(hall, "Buildings", 1, 1, SHELF)
    hall.props["Warp"] = f"{hall_w // 2} {hall_h} Town {TOWN_RETURN[0]} {TOWN_RETURN[1]}"
    maps["Workshop_EntranceHall"] = hall

    for room in rooms:
        tier, motif, host, opens, bundles, display, (w, h), floor, wall = W.ROOMS[room]
        m = room_shell(w, h, wall, floor, w // 2)
        stamp(m, "Buildings", 2, 1, BOARD)
        for x in (2, 3):
            m.prop("Buildings", x, 3, "Action", P + "Bundles " + P + room)
        if room in ("FurnaceRoom", "GreatWorkChamber"):
            stamp(m, "Buildings", w - 4, 1, HEARTH)
        if room == "Library":
            stamp(m, "Buildings", w - 4, 1, SHELF)
            stamp(m, "Buildings", w - 7, 1, SHELF)
        if room == "Undercroft":
            for x in range(1, w - 1):
                for y in range(4, h - 2):
                    m.prop("Back", x, y, "ValleyOpus_Dark", "T")
        m.props["Warp"] = f"{w // 2} {h} {location_name('EntranceHall')} {ROOM_DOORS[room]} 4"
        maps["Workshop_" + room] = m
    return maps


# Laboratory floorplans (doc 18 §2): the floor is (fw × fh) inside the walls; each tier nests the last.
LABS = [(1, 14, 9), (2, 20, 12), (3, 20, 18)]


def lab_map(tier, fw, fh):
    w, h = fw + 2, fh + 5
    door = 7
    m = room_shell(w, h, 12, 2, door)
    # Hearths in the back wall: the floor tiles in front of them are heated (doc 18 §2a).
    hearth_cols = [5] + ([15] if tier >= 2 else [])
    for hx in hearth_cols:
        stamp(m, "Buildings", hx, 1, HEARTH)
        for x in (hx, hx + 1):
            m.prop("Back", x, 4, "ValleyOpus_Heat", "T")
    # Dark alcoves: shuttered corners that always count as dark.
    alcoves = [(1, 5)] + ([(1, 12)] if tier >= 3 else [])
    for ax, ay in alcoves:
        for dx in (0, 1):
            for dy in (0, 1):
                m.set("Back", ax + dx, ay + dy, WF, floor_tiles(14)[dx + 2 * dy])
                m.prop("Back", ax + dx, ay + dy, "ValleyOpus_Dark", "T")
    if tier >= 3:
        # The Familiar's shrine niche (doc 18 §3).
        m.prop("Buildings", w - 3, 3, "Action", P + "Shrine")
        m.prop("Back", w - 3, 4, "ValleyOpus_Shrine", "T")
    m.props["Warp"] = f"{door} {h} Farm 0 0"
    return m


def walk_check(maps, homes):
    problems = []
    for (map_name, x, y), who in homes.items():
        m = maps.get(map_name)
        if m and not m.walkable(x, y):
            problems.append(f"{who}'s home {map_name} {x},{y} is not walkable")
    return problems


def write(pack, i18n, write_json, header, art_root):
    import build_art
    import npc_art
    files = []
    maps = workshop_maps()
    for tier, fw, fh in LABS:
        maps[f"Lab{tier}"] = lab_map(tier, fw, fh)
    os.makedirs(os.path.join(pack, "assets", "maps"), exist_ok=True)
    for name, m in maps.items():
        with open(os.path.join(pack, "assets", "maps", name + ".tmx"), "w", encoding="utf-8") as f:
            f.write(m.tmx())

    changes = []
    for name in maps:
        changes.append({"Action": "Load", "Target": "Maps/" + P + name, "FromFile": f"assets/maps/{name}.tmx"})

    locations = OrderedDict()
    for room, row in W.ROOMS.items():
        w, h = row[6]
        locations[location_name(room)] = OrderedDict([
            ("DisplayName", "{{i18n:room." + room + ".name}}"),
            ("DefaultArrivalTile", {"X": w // 2, "Y": h - 3}),
            ("CreateOnLoad", {"MapPath": "Maps/" + P + "Workshop_" + room, "AlwaysActive": True}),
            ("CanPlantHere", False)])
    changes.append({"Action": "EditData", "Target": "Data/Locations", "Entries": locations})
    changes.append({"Action": "EditMap", "Target": "Maps/Town", "MapTiles": [
        {"Position": {"X": TOWN_ENTRANCE[0], "Y": TOWN_ENTRANCE[1]}, "Layer": "Back", "SetProperties": {"TouchAction": P + "Enter"}}]})

    # ── Laboratories (doc 18 §1) ──
    costs = {1: (25000, [("(O)388", 400), ("(O)338", 20), ("(O)334", 10), ("(O)" + P + "Saxum", 10)], 2, None, "Buildings/Shed", "FurnaceRoom"),
             2: (25000, [("(O)388", 550), ("(O)390", 300), ("(O)" + P + "Vitrum", 20), ("(O)" + P + "Saxum", 10)], 2, P + "LabT1", "Buildings/Big Shed", "Sanctum"),
             3: (60000, [("(O)709", 200), ("(O)" + P + "Saxum", 30), ("(O)337", 10), ("(O)" + P + "QuintaEssentia", 1)], 3, P + "LabT2", "Buildings/Big Shed", "GreatWork")}
    names = {1: ("Still Room", "A small alchemy laboratory. ValleyOpus apparatus inside work 10% faster, with a hearth and a dark alcove."),
             2: ("Long Bench", "A larger laboratory. Apparatus inside work 15% faster and sometimes better; a ritual dais fits a Circle."),
             3: ("Great Hall", "The great laboratory. Apparatus inside work 20% faster, and a Familiar can bond to it.")}
    buildings = OrderedDict()
    for tier, (gold, mats, days, upgrade_from, texture, unlock) in costs.items():
        i18n[f"building.LabT{tier}.name"] = "Laboratory: " + names[tier][0]
        i18n[f"building.LabT{tier}.desc"] = names[tier][1]
        row = OrderedDict([
            ("Name", "{{i18n:building.LabT%d.name}}" % tier), ("Description", "{{i18n:building.LabT%d.desc}}" % tier),
            ("Texture", texture), ("Size", {"X": 7, "Y": 3}), ("Builder", "Robin"), ("BuildCost", gold),
            ("BuildMaterials", [{"ItemId": i, "Amount": n} for i, n in mats]), ("BuildDays", days),
            ("BuildCondition", f"PLAYER_HAS_MAIL Host {P}Unlock_{unlock} Received"),
            ("HumanDoor", {"X": 3, "Y": 2}), ("IndoorMap", P + f"Lab{tier}"), ("IndoorMapType", "StardewValley.GameLocation"),
            ("CustomFields", {P + "LabTier": str(tier)})])
        if upgrade_from:
            row["BuildingToUpgrade"] = upgrade_from
        buildings[P + f"LabT{tier}"] = row
    changes.append({"Action": "EditData", "Target": "Data/Buildings", "Entries": buildings})

    # ── NPCs (doc 08a, doc 14 §4) ──
    characters, gifts = OrderedDict(), OrderedDict()
    homes = {}
    os.makedirs(os.path.join(pack, "data", "dialogue"), exist_ok=True)
    for nid, (display, gender, age, home, birthday, unlock, portrait, hair, clothes, (loves, likes), lines) in W.NPCS.items():
        name = P + nid
        loc, hx, hy = home
        location = location_name(loc) if loc in W.ROOMS else loc
        if loc in W.ROOMS:
            homes[("Workshop_" + loc, hx, hy)] = nid
        i18n[f"npc.{nid}.name"] = display
        row = OrderedDict([
            ("DisplayName", "{{i18n:npc.%s.name}}" % nid), ("BirthSeason", birthday[0]), ("BirthDay", birthday[1]),
            ("HomeRegion", "Town"), ("Gender", gender), ("Age", age), ("Manner", "Neutral"), ("SocialAnxiety", "Neutral"),
            ("Optimism", "Neutral"), ("CanBeRomanced", False), ("Calendar", "HiddenUntilMet"), ("SocialTab", "UnknownUntilMet"),
            ("IntroductionsQuest", False), ("ItemDeliveryQuests", "FALSE"), ("PerfectionScore", False), ("EndSlideShow", "Hidden"),
            ("FlowerDanceCanDance", False), ("WinterStarParticipant", "FALSE"), ("CanVisitIsland", "FALSE"),
            ("UnlockConditions", f"PLAYER_HAS_MAIL Host {P}Unlock_{unlock} Received"),
            ("Home", [{"Id": "Default", "Location": location, "Tile": {"X": hx, "Y": hy}, "Direction": "down"}]),
            ("CustomFields", {P + "Npc": "true"})])
        if nid == "Curator":
            row["Size"] = {"X": 32, "Y": 32}
            row["Breather"] = False
            row["CanReceiveGifts"] = True
        characters[name] = row
        strip = lambda ids: " ".join(i[3:] if i.startswith("(O)") else i for i in ids)
        gifts[name] = f"This is wonderful. Thank you!/{strip(loves)}/Thank you!/{strip(likes)}/Oh. Thanks./ /Hm./ /Thanks./ "
        dialogue = OrderedDict((k, v) for k, v in lines.items())
        write_json(os.path.join(pack, "data", "dialogue", nid + ".json"), dialogue)
        changes.append({"Action": "Load", "Target": "Characters/Dialogue/" + name, "FromFile": f"data/dialogue/{nid}.json"})
        changes.append({"Action": "Load", "Target": "Characters/" + name, "FromFile": f"assets/npcs/{nid}.png"})
        changes.append({"Action": "Load", "Target": "Portraits/" + name, "FromFile": f"assets/npcs/{nid}_portrait.png"})
    problems = walk_check(maps, homes)
    if problems:
        raise SystemExit("\n".join(problems))
    changes.append({"Action": "EditData", "Target": "Data/Characters", "Entries": characters})
    changes.append({"Action": "EditData", "Target": "Data/NPCGiftTastes", "Entries": gifts})
    npc_art.build(art_root, W.NPCS, os.path.join(pack, "assets", "npcs"))
    npc_art.build_familiars(art_root, W.FAMILIARS, os.path.join(pack, "assets", "familiars.png"))
    changes.append({"Action": "Load", "Target": "Mods/CharlesDR.ValleyOpus/Familiars", "FromFile": "assets/familiars.png"})

    # ── Mail ──
    mail = OrderedDict()
    for mid, text in W.MAIL.items():
        mail[P + mid] = text + "[#]" + mid.replace("Mail_", "")
    changes.append({"Action": "EditData", "Target": "Data/mail", "Entries": mail})

    # ── Shops (opened by talking to their owner once their dialogue for the day is done) ──
    shops = OrderedDict()
    for sid, (owner, items, buys) in W.SHOPS.items():
        rows = []
        for n, (item, price, trade, count, unlock) in enumerate(items):
            r = OrderedDict([("Id", f"{sid}_{n}"), ("ItemId", item)])
            if price is not None:
                r["Price"] = price
            if trade:
                r["TradeItemId"] = trade
                r["TradeItemAmount"] = count
                r["Price"] = 0
            if unlock:
                r["Condition"] = f"PLAYER_HAS_MAIL Host {P}Unlock_{unlock} Received"
            rows.append(r)
        shop = OrderedDict([("Owners", [{"Name": P + owner, "Id": "Default"}]), ("Items", rows)])
        if buys:
            shop["SalableItemTags"] = buys
        shops[P + sid] = shop
    changes.append({"Action": "EditData", "Target": "Data/Shops", "Entries": shops})

    # ── The soft start (doc 14 §2): Oriel meets the player in town behind the Community Center ──
    oriel = P + "Oriel"
    script = "/".join([
        "continue", f"{TOWN_ENTRANCE[0]} {TOWN_ENTRANCE[1] + 4}",
        f"farmer {TOWN_ENTRANCE[0]} {TOWN_ENTRANCE[1] + 6} 0 {oriel} {TOWN_ENTRANCE[0]} {TOWN_ENTRANCE[1] + 2} 2",
        "skippable", "pause 500",
        f"speak {oriel} \"{{{{i18n:event.intro.1}}}}\"",
        f"move farmer 0 -2 0", "pause 300",
        f"speak {oriel} \"{{{{i18n:event.intro.2}}}}\"",
        f"faceDirection {oriel} 0", "pause 600", f"faceDirection {oriel} 2",
        f"speak {oriel} \"{{{{i18n:event.intro.3}}}}\"",
        "pause 300", "end"])
    i18n["event.intro.1"] = "Oh! You must be the farmer who opened the Community Center. I'm Oriel.#$b#I study the older kind of craft. Alchemy."
    i18n["event.intro.2"] = "There's a workshop under this building. Derelict, but not dead. A spirit keeps it.#$b#He calls himself the Curator."
    i18n["event.intro.3"] = "The door's just here, behind the Center. Come and see. I've left you a few things to start with.#$b#And if you meet a man called Tobin, he's the one who could get the machines running again."
    precondition = f"G PLAYER_HAS_MAIL Host ccDoorUnlock Received, !PLAYER_HAS_MAIL Host {P}Unlock_Intro Received/t 600 1500"
    changes.append({"Action": "EditData", "Target": "Data/Events/Town", "Entries": {f"{P}Intro/{precondition}": script}})

    write_json(os.path.join(pack, "data", "world.json"), {"Changes": changes}, header)
    files.append("data/world.json")
    return files
