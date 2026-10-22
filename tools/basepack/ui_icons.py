"""The UI sheet (sos.3 placeholder art, drawn for ValleyOpus by tools/basepack): the unknown-Element vial for tooltips,
the codex's power icons, the Alchemy skill and profession icons, and one buff icon per effect, in the same style as
the item icons. A 16x16 grid, 16 cells wide; Data/Buffs rows point at it by sprite index."""

import math

from PIL import Image

import icon_shapes as S
import item_icons as I
import pixelkit as K

TEXTURE = "Mods/CharlesDR.ValleyOpus/UI"
COLUMNS = 16

HOURGLASS = [
    "................",
    "..MMMMMMMMMMMM..",
    "..MnnnNNNNNmmM..",
    "..MMMMMMMMMMMM..",
    "...ohGGGGGGgo...",
    "...oh444433go...",
    "....oh4433go....",
    ".....oh32go.....",
    "......o3go......",
    ".....ohG3go.....",
    "....ohGG3Ggo....",
    "...ohGGG4GGgo...",
    "...oh444433go...",
    "..MMMMMMMMMMMM..",
    "..MnnnNNNNNmmM..",
    "..MMMMMMMMMMMM..",
]

BOWL = [
    "................",
    "......w...w.....",
    ".....w...w......",
    "......w...w.....",
    ".....w...w......",
    "................",
    "..qqqqqqqqqqqq..",
    ".qeeddddddcccbq.",
    ".o554444333221o.",
    "..o5444433322o..",
    "...o44433322o...",
    "....o433221o....",
    ".....oooooo.....",
    "................",
    "................",
    "................",
]

MUG = [
    "................",
    "................",
    "...qqqqqqq......",
    "..qeeeddddq.....",
    "..qedddcccq.....",
    "..o5544332oooo..",
    "..oNNNNNNNo..o..",
    "..o5544332o..o..",
    "..o5544332o..o..",
    "..oNNNNNNNoooo..",
    "..o5544332o.....",
    "..o5544332o.....",
    "..o4433221o.....",
    "..ooooooooo.....",
    "................",
    "................",
]

HAMMER = [
    "................",
    "..TTTTTTTT......",
    ".TttttSSSsT.....",
    ".TtSSSSSssT.....",
    "..TTTOkOTTT.....",
    ".....OkuO.......",
    ".....OkuO.......",
    ".....OkuO.......",
    ".....OkuO.......",
    ".....OkuO.......",
    ".....OkuO.......",
    ".....OkuO.......",
    ".....OkuO.......",
    ".....OOOO.......",
    "................",
    "................",
]

HEART = [
    "................",
    "................",
    "...ooo....ooo...",
    "..o554o..o443o..",
    ".o5w5544o44332o.",
    ".o554444433322o.",
    ".o544444433221o.",
    "..o4444433221o..",
    "...o44433221o...",
    "....o433221o....",
    ".....o3221o.....",
    "......o21o......",
    ".......oo.......",
    "................",
    "................",
    "................",
]

# 10x10, for SpaceCore's skills page.
SKILL_SMALL = [
    "...OOOO...",
    "...OkuO...",
    "...ohgo...",
    "..ohGGgo..",
    ".ohw5543o.",
    ".o554433o.",
    ".o544332o.",
    ".o443321o.",
    "..o4322o..",
    "...oooo...",
]


def _grid(rows, main, accent=None, w=16, h=16):
    pal = I.palette(main, accent)
    pal["o"] = K.outline_of(main)
    return K.grid(rows, pal, size=(w, h))


def _mask_icon(inside, main, light=(-0.6, -0.7)):
    """A rounded shape from a mask function, shaded as a lit body and outlined."""
    img = Image.new("RGBA", (16, 16), (0, 0, 0, 0))
    px = img.load()
    for y in range(16):
        for x in range(16):
            if inside(x + 0.5, y + 0.5):
                px[x, y] = K.rgba(main)
    K.shade_sphere(img, K.ramp(main, 5), light=light)
    return K.outline(img, lambda c: K.rgba(K.outline_of(main)))


def heart(main):
    return _grid(HEART, main)


def ore(main, fleck):
    """A stone with ore glinting in it."""
    img = I.draw("LUMP", main)
    px = img.load()
    for x, y in [(6, 7), (7, 7), (9, 9), (10, 9), (5, 10)]:
        if px[x, y][3]:
            px[x, y] = K.rgba(fleck)
    return img


def seal(main):
    """A wax seal, its edge pressed uneven, with the Sal rune in it."""
    def inside(x, y):
        a = math.atan2(y - 8, x - 8)
        return math.hypot(x - 8, y - 8) <= 5.6 + 0.6 * math.sin(a * 5)
    img = _mask_icon(inside, main)
    px = img.load()
    ink = K.rgba(K.darker(main, 0.45))
    for dy, row in enumerate(S.RUNES["Sal"]):
        for dx, ch in enumerate(row):
            if ch == "#":
                px[6 + dx, 6 + dy] = ink
    return img


def ghost(main):
    def inside(x, y):
        if y < 7:
            return math.hypot(x - 8, y - 7) <= 5
        wave = 13 if int(x) % 3 else 14
        return 3 <= x <= 13 and y <= wave
    img = _mask_icon(inside, main)
    px = img.load()
    for x, y in [(6, 6), (6, 7), (10, 6), (10, 7)]:
        px[x, y] = K.rgba("#3A3448")
    return img


def planet(main, ring):
    img = _mask_icon(lambda x, y: math.hypot(x - 8, y - 8) <= 4.6, main)
    px = img.load()
    rr = K.ramp(ring, 5)
    for x in range(16):
        for y in range(16):
            u, v = (x + 0.5 - 8) / 7.4, (y + 0.5 - 8.6 + (x - 8) * 0.18) / 2.0
            d = u * u + v * v
            behind = y < 8 + (8 - x) * 0.18 and math.hypot(x + 0.5 - 8, y + 0.5 - 8) <= 4.6
            if 0.62 <= d <= 1.0 and not behind:
                px[x, y] = K.rgba(rr[3] if v < 0 else rr[1])
    return K.outline(img, lambda c: K.rgba(K.outline_of(main)))


def wind(main):
    """Three gusts with curled ends."""
    img = Image.new("RGBA", (16, 16), (0, 0, 0, 0))
    px = img.load()
    r = K.ramp(main, 5)
    strokes = [
        [(x, 5) for x in range(2, 11)] + [(11, 4), (11, 3), (10, 2), (9, 2), (8, 3)],
        [(x, 9) for x in range(4, 14)],
        [(x, 12) for x in range(1, 9)] + [(9, 13), (9, 14), (8, 14)],
    ]
    for stroke in strokes:
        for n, (x, y) in enumerate(stroke):
            px[x, y] = K.rgba(r[4] if n < len(stroke) // 3 else r[3])
    return K.outline(img, lambda c: K.rgba(K.outline_of("#5A7A90")))


def arrow(main):
    """A fat arrow pointing east, for the chain overlay's intake and spout marks."""
    img = Image.new("RGBA", (16, 16), (0, 0, 0, 0))
    px = img.load()
    r = K.ramp(main, 5)
    for y in range(16):
        for x in range(16):
            cx, cy = x + 0.5, y + 0.5
            shaft = 2 <= cx <= 9 and 5.5 <= cy <= 10.5
            head = 8 <= cx <= 14 and abs(cy - 8) <= (14 - cx) * 0.95 + 0.2
            if shaft or head:
                px[x, y] = K.rgba(r[4] if cy < 6.5 else r[3] if cy < 8.5 else r[2])
    return K.outline(img, lambda c: K.rgba(K.outline_of(main)))


def shape(name, main, accent=None):
    return I.draw(name, main, accent)


# The sheet, in order: name → drawing. Indices are what the C# mod and Data/Buffs use.
def cells():
    out = [
        ("Unknown", I.extra_icon("Unknown")),
        ("FlammaViva", I.extra_icon("FlammaViva")),
        ("Vapor", I.extra_icon("Vapor")),
        ("Fulgur", I.extra_icon("Fulgur")),
        ("Skill", shape("FLASK", "#9B59D0")),
        ("SkillSmall", _grid(SKILL_SMALL, "#9B59D0", w=16, h=16)),
        ("Mechanist", shape("GEAR", "#D2A64E", "#7AE0F0")),
        ("Mystic", shape("MOON", "#B07AE0")),
        ("Engineer", shape("GEAR", "#9AA6B2", "#F07A2A")),
        ("Artificer", _grid(HAMMER, "#9AA6B2")),
        ("Hermeticist", shape("EGG", "#C8B0F0")),
        ("Theurgist", shape("FLAME", "#B07AE0", "#F8E8FF")),
        ("Intake", arrow("#6495ED")),
        ("Spout", arrow("#F0A030")),
    ]
    out += [(None, None)] * (16 - len(out))
    buffs = [
        ("UndinesGift", shape("TEAR", "#3A8FD8")),
        ("Fixative", seal("#B8303A")),
        ("Volatile", shape("FLASK_GAS", "#F07A2A", "#FFE680")),
        ("LunarDream", shape("MOON", "#D0D8E4")),
        ("Savour", _grid(BOWL, "#C8A070", "#E8902A")),
        ("GoodCheer", _grid(MUG, "#9A6234", "#F4ECD8")),
        ("Vitality", heart("#62C24A")),
        ("Cleanse", shape("SPARK", "#BFE3F0", "#FFFFFF")),
        ("Glow", shape("SUN", "#F2B82A", "#FFE680")),
        ("Tailwind", wind("#CFE8F0")),
        ("Kindled", shape("FLAME", "#F07A2A", "#FFE680")),
        ("Stonesense", ore("#8E8A86", "#F0A060")),
        ("Eruption", shape("ORB_CRACKED", "#7A5A4A", "#FF8A20")),
        ("Quintessence", shape("PHIAL", "#B07AE0", "#F8E8FF")),
        ("Midas", shape("INGOT", "#F2C22A")),
        ("Hourglass", _grid(HOURGLASS, "#E8D890")),
        ("Spectral", ghost("#E4ECF6")),
        ("Dissolution", shape("TEAR", "#8AD040")),
        ("SecondWind", heart("#E03A4A")),
        ("Venom", shape("DROPPER", "#7AC03A", "#3A3440")),
        ("LuckyHoney", shape("JAR_PLAIN", "#F0B030")),
        ("Planetary", planet("#E0A050", "#D8E4F4")),
        ("Ouroboros", I.icon_for("OuroborosRing")),
        ("Censer", I.icon_for("SmoulderingCenser")),
        ("VialPendant", I.icon_for("VialPendant")),
        ("RebisAmulet", I.icon_for("RebisAmulet")),
        ("Transmutation", shape("STAR", "#F2C22A")),
    ]
    return out + [(f"Buff.{k}", img) for k, img in buffs]


def index_of():
    return {name: i for i, (name, _) in enumerate(cells()) if name}


def build(out_path):
    K.sheet([img for _, img in cells()], 16, 16, COLUMNS).save(out_path)


def buff_index(effect_local):
    """The buff icon for an effect row (strong rows share their weak row's icon)."""
    idx = index_of()
    key = "Buff." + (effect_local[:-len("Strong")] if effect_local.endswith("Strong") else effect_local)
    return idx.get(key)
