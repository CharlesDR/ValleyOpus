"""Item icons for every ValleyOpus object (sos.3 placeholder art, drawn for the mod by tools/basepack): a shape per group
so items sort by silhouette (art direction: Elements in vials, Principles in jars, Aether a crystal star), coloured per
item. Shapes live in icon_shapes.py; this file says which shape and colours each item gets, and draws them."""

import pixelkit as K
import icon_shapes as S

GLASS = "#BFE3F0"
CORK = "#B8783C"
BRASS = "#D2A64E"
STEEL = "#9AA6B2"

# local id: (shape, main colour, accent colour or None)
ICONS = {
    # Elements and Principles carry their rune (Charles 2026-10-18); Aether is the crystal star.
    "Ignis": ("VIAL", "#E0442A", None),
    "Aqua": ("VIAL", "#3A8FD8", None),
    "Terra": ("VIAL", "#8A5A2B", None),
    "Aer": ("VIAL", "#BFE0EE", None),
    "Sal": ("JAR", "#E8E8E0", "#F4E8C8"),
    "Sulphur": ("JAR", "#E3C21E", "#F4E8C8"),
    "Mercurius": ("JAR", "#A8B0B8", "#F4E8C8"),
    "Aether": ("STAR", "#9B59D0", None),
    # Tier 1 substances, by what they are.
    "RosMaialis": ("BOTTLE", "#9ED8C8", "#F4E8C8"),
    "Saxum": ("BRICK", "#A89C8C", "#7E7264"),
    "Pneuma": ("FLASK_GAS", "#D8EEF4", "#FFFFFF"),
    "SalFixum": ("CRYSTALS", "#ECECE4", None),
    "SulphurVivum": ("CRYSTALS", "#F2D22A", None),
    "MercuriusCurrens": ("BEADS", "#C8D0DA", None),
    "QuintaEssentia": ("PHIAL", "#B07AE0", "#F8E8FF"),
    "Magma": ("LUMP_GLOW", "#5A3A30", "#FF8A20"),
    "Limus": ("BLOB", "#6E6A3A", None),
    "Tempestas": ("FLASK_STORM", "#6A7896", "#FFF27A"),
    "Pulvis": ("HEAP", "#C2AE8C", None),
    "Calx": ("HEAP", "#F0EEE6", None),
    "Quebrith": ("LUMP", "#E0A21E", None),
    "Phlogiston": ("PHIAL", "#F07A2A", "#FFE680"),
    "SalMarinum": ("FLASK", "#4AB0C8", None),
    "AquaFortis": ("FLASK", "#B8D23A", None),
    "Hydragenum": ("FLASK", "#9CB8D8", None),
    "Vitreus": ("LUMP", "#8EB89A", None),
    "Bitumen": ("BLOB", "#3A3440", None),
    "Metallum": ("NUGGET", "#B8A890", None),
    "SalVolatile": ("SALTS", "#E8E4F0", "#FFFFFF"),
    "Miasma": ("FLASK_GAS", "#8AA048", "#5A6A30"),
    "Motus": ("DROPPER", "#5AD0E8", "#C04A3A"),
    "Sol": ("SUN", "#F2B82A", "#FFE680"),
    "Luna": ("MOON", "#D0D8E4", None),
    "Vitae": ("SPARK", "#62C24A", "#4A9A3A"),
    "Spiritus": ("FLAME", "#A8D8F0", "#FFFFFF"),
    "Vitriol": ("CRYSTALS", "#3AA86A", None),
    "MercuriusFixus": ("NUGGET", "#C8D0DA", None),
    "Cinnabaris": ("CRYSTALS", "#C8302A", None),
    "Ordo": ("CUBE", "#D8E4F4", None),
    "Anima": ("FLAME", "#F0A0C0", "#FFF0F6"),
    "MercuriusPhilosophorum": ("PHIAL", "#A8B8D0", "#FFFFFF"),
    "CaputMortuum": ("LUMP", "#7A5A4A", None),
    # Tier 2.
    "Vitrum": ("PANE", "#BFE3F0", None),
    "TerraViva": ("SACK", "#C8A070", "#5A8A3A"),
    "Vigor": ("BOTTLE", "#E8602A", "#F4E8C8"),
    "Smaltum": ("HEAP", "#2E52AA", None),
    "PulvisPyrius": ("POUCH", "#4A4440", "#B8783C"),
    "AquaRegia": ("FLASK", "#E8902A", None),
    "AquaVitae": ("BOTTLE", "#D8A04A", "#F4E8C8"),
    "Electrum": ("INGOT", "#E8D890", None),
    "Rebis": ("ORB_TWIN", "#F2C24A", "#C8D0DA"),
    "Orichalcum": ("INGOT", "#E07A3A", None),
    "Elixir": ("POTION", "#E03A4A", None),
    "Panacea": ("POTION", "#F4E0A0", None),
    "CinisPhoenicis": ("POUCH", "#8A8480", "#E0602A"),
    "Gelum": ("JAR_PLAIN", "#BCE8F8", None),
    "Venenum": ("DROPPER", "#7AC03A", "#3A3440"),
    "Ectoplasm": ("BLOB", "#8AF0B0", None),
    "PerpetuumMobile": ("GEAR", "#D2A64E", "#7AE0F0"),
    "Radicula": ("ROOT", "#C8A070", "#5AA84A"),
    "Argentum": ("INGOT", "#D8E0E8", None),
    "Stannum": ("INGOT", "#B8BCB0", None),
    "Plumbum": ("INGOT", "#7A8090", None),
    "RosaVitrea": ("ROSE", "#F0A0B8", "#5AA84A"),
    "MelVitae": ("JAR_PLAIN", "#F0B030", None),
    "PhialaStellarum": ("PHIAL", "#2A3A7A", "#FFF6B0"),
    "Tenebrae": ("ORB", "#4A2A6A", None),
    "Mors": ("DROPPER", "#3A3A44", "#6A6A74"),
    "Perditio": ("ORB_CRACKED", "#5A4A6A", "#E0B0FF"),
    # Tier 3 and the farm's secret.
    "PrimaMateria": ("LUMP", "#3A3036", None),
    "Alkahest": ("FLASK", "#3AE0C8", None),
    "Azoth": ("PHIAL", "#F4F0D0", "#FFFFFF"),
    "LapisPhilosophorum": ("GEM", "#D0202A", None),
    "AurumPotabile": ("BOTTLE", "#F2C22A", "#F4E8C8"),
    "LacVirginis": ("BOTTLE", "#F4F4F0", "#D8E4F4"),
    "SanguisDraconis": ("TEAR", "#8A1A2A", None),
    # Tools.
    "Satchel": ("SATCHEL", "#9A6234", None),
    "Lens": ("LENS", "#BFE3F0", "#7A4A2A"),
    "GloamLantern": ("LANTERN", "#3A2A5A", "#B07AE0"),
    # Worn.
    "VialPendant": ("PENDANT_VIAL", "#E03A4A", None),
    "SmoulderingCenser": ("CENSER", "#E06A2A", "#FFB060"),
    "RebisAmulet": ("AMULET", "#F2C24A", "#C8D0DA"),
    "PlanetaryBand": ("RING", "#C89A5A", None),
    "RingOfTheSeven": ("RING_GEM", "#E8D890", "#9B59D0"),
    "OuroborosRing": ("RING_SNAKE", "#5AA84A", "#E0442A"),
    # New produce.
    "BrimstoneEgg": ("EGG", "#F2D040", None),
    "GoldenFleece": ("FLEECE", "#F4EEDC", "#F2B82A"),
    # Curios.
    "CurioSyntheticVial": ("SYNTH_VIAL", "#7AD0F0", "#3A6AC8"),
    "CurioRedactedNotebook": ("NOTEBOOK", "#5A6A8A", None),
    "CurioBrokenAssay": ("SCALE", "#D2A64E", None),
    "CurioJojaBadge": ("BADGE", "#E8ECF0", "#3A6AC8"),
    "CurioCrystalSeed": ("CRYSTAL_JAR", "#9AE0F0", None),
}

# Codex-only icons (power and the unknown-Element mark), drawn to a separate sheet.
EXTRA_ICONS = {
    "FlammaViva": ("FLAME", "#F07A2A", "#FFE680"),
    "Vapor": ("STEAM", "#E4ECF0", None),
    "Fulgur": ("BOLT", "#FFE84A", None),
    "Unknown": ("VIAL", "#C4C8D4", None),
}

PRIME_RUNES = {"Ignis", "Aqua", "Terra", "Aer", "Sal", "Sulphur", "Mercurius"}


def palette(main, accent=None):
    r = K.ramp(main, 5)
    a = K.ramp(accent or K.darker(main, 0.25), 5)
    glass = K.ramp(GLASS, 5)
    cork = K.ramp(CORK, 5)
    brass = K.ramp(BRASS, 5)
    steel = K.ramp(STEEL, 5)
    main_out = K.outline_of(main)
    return {
        "1": r[0], "2": r[1], "3": r[2], "4": r[3], "5": r[4],
        "a": a[0], "b": a[1], "c": a[2], "d": a[3], "e": a[4],
        "o": main_out, "q": K.outline_of(accent or main),
        "g": glass[1], "G": glass[3], "h": glass[4], "w": (255, 255, 250, 255),
        "u": cork[1], "C": cork[2], "k": cork[3], "O": K.outline_of("#7A4A20"),
        "m": brass[1], "N": brass[2], "n": brass[4], "M": K.outline_of("#8A6420"),
        "s": steel[1], "S": steel[2], "t": steel[3], "T": K.outline_of(STEEL),
        "x": a[4] if accent else K.lighter(main, 0.6), "z": K.outline_of(main),
    }


GLASS_SHAPES = {"VIAL", "JAR", "JAR_PLAIN", "FLASK", "FLASK_GAS", "FLASK_STORM", "BOTTLE", "POTION", "PHIAL", "DROPPER",
                "SALTS", "PENDANT_VIAL", "CRYSTAL_JAR", "VIAL_UNKNOWN"}


def draw(shape, main, accent=None, rune=None):
    rows = [r[:16].ljust(16, ".") for r in getattr(S, shape)][:16]
    pal = palette(main, accent)
    if shape in GLASS_SHAPES:
        # Glass outlines lean toward a cool slate, so clear vessels read as glass whatever they hold.
        pal["o"] = K.outline_of(K.mix(main, "#2A3348", 0.6))
    if shape == "PANE":
        pal["o"] = K.outline_of("#2A5A70")
    if shape == "LENS":
        pal["a"], pal["b"], pal["c"] = K.ramp(accent, 5)[1], K.ramp(accent, 5)[2], K.ramp(accent, 5)[3]
        pal["q"] = K.outline_of(accent)
    img = K.grid(rows, pal, size=(16, 16))
    if rune:
        glyph = S.RUNES[rune]
        ramp = K.ramp(main, 5)
        light = sum(K.rgba(main)[:3]) > 520
        ink = K.outline_of(main) if light else K.lighter(main, 0.9)
        x0, y0 = S.JAR_RUNE_AT if shape == "JAR" else S.VIAL_RUNE_AT
        if shape == "JAR":
            ink = K.outline_of(main) if sum(K.rgba(main)[:3]) > 300 else ramp[1]
        px = img.load()
        for dy, row in enumerate(glyph):
            for dx, ch in enumerate(row):
                if ch == "#":
                    px[x0 + dx, y0 + dy] = K.rgba(ink)
    return img


def icon_for(local):
    shape, main, accent = ICONS[local]
    return draw(shape, main, accent, local if local in PRIME_RUNES else None)


def extra_icon(key):
    shape, main, accent = EXTRA_ICONS[key]
    img = draw(shape, main, accent)
    if key == "Unknown":
        # A pale, empty-looking vial with a dark "?" on the glass (the Element row in tooltips).
        px = img.load()
        for dy, row in enumerate(S.RUNES["Unknown"]):
            for dx, ch in enumerate(row):
                if ch == "#":
                    px[6 + dx, 7 + dy] = K.rgba("#2A2436")
    return img
