"""Apparatus sprites (sos.3 placeholder art, drawn for ValleyOpus by tools/basepack): one 16x32 big-craftable sprite per
apparatus, with an idle cell, a ready cell and four working cells, so Data/Machines can animate them (fire, bubbles,
drips, steam, sparks). Arcane apparatus are glass, ceramic and stone with runes; engineered ones are iron, copper and
brass. Inputs go in on the left, products come out on the right (doc 16 chaining aid).

Grids are 16 wide and bottom-aligned (rows are padded on top to 32). Upper-case letters are materials, shaded by this
file as lit cylinders or boxes from the top left; the rest are fixed colours or effect markers:

  Materials: B brick, P stone block, Q stone top, I iron, U iron plate, C copper, M brass, W wood plank, V wood post,
             G glass, L the apparatus's liquid, K glazed ceramic, T terracotta, A cloth, E leaves, R pale root,
             Z cobalt glaze
  Colours:   o outline, i dark interior, 1-5 stone tones, t/s iron light/dark, n/m brass light/dark, y brass shine,
             p/c copper light/dark, v/w wood light/dark, h glass highlight, H white, l cream, k cream shadow,
             r wax red, x glow, X bright glow, j leaf dark, e ember
  Effects:   * furnace fire (dark when idle), & open flame (unlit when idle), ! drip path, ~ gauge needle pivot
"""

import random

import pixelkit as K

CELL_W, CELL_H = 16, 32
CELLS = 8  # cells per apparatus row on the sheet: idle, ready, four working frames, two spare
IDLE, READY = 0, 1
WORKING = [2, 3, 4, 5]

RAMPS = {
    "B": K.ramp("#A85A3C", 5),
    "P": K.ramp("#8E8A86", 5),
    "Q": K.ramp("#A8A39C", 5),
    "I": K.ramp("#5E6674", 5),
    "U": K.ramp("#5E6674", 5),
    "C": K.ramp("#B8603A", 5),
    "M": K.ramp("#D2A048", 5),
    "W": K.ramp("#9A6A3C", 5),
    "V": K.ramp("#86572F", 5),
    "G": [K.rgba("#5E86A0"), K.rgba("#8DB8CC"), K.rgba("#B9DCE8"), K.rgba("#D8EEF4"), K.rgba("#F6FCFF")],
    "K": K.ramp("#E2D6BC", 5, spread=0.8),
    "T": K.ramp("#B8673E", 5),
    "A": K.ramp("#6A3A8E", 5),
    "E": K.ramp("#4E9A3E", 5),
    "R": K.ramp("#D8C49A", 5),
    "Z": K.ramp("#2E52AA", 5),
}
PROFILE = {"B": "cyl", "P": "box", "Q": "flat", "I": "cyl", "U": "box", "C": "cyl", "M": "cyl", "W": "flat", "V": "box",
           "G": "cyl", "L": "cyl", "K": "cyl", "T": "cyl", "A": "flat", "E": "cyl", "R": "cyl", "Z": "cyl"}
SHINY = {"I", "C", "M", "G", "K", "Z"}  # a one-pixel specular column
GROUPS = {"G": "vessel", "L": "vessel"}  # glass and its contents shade as one round body

FIXED = {
    "o": "#2E1C1A", "i": "#1C1418", "e": "#6A2418",
    "1": "#4A4448", "2": "#6A6466", "3": "#8E8A86", "4": "#B2ACA4", "5": "#D6D0C4",
    "t": "#AEB6C2", "s": "#3A3E4A", "n": "#EAC066", "m": "#8A6020", "y": "#FFF0B0",
    "p": "#E8956A", "c": "#7A3420", "v": "#C49460", "w": "#5A3A22",
    "h": "#F6FCFF", "H": "#FFFFFF", "l": "#F2E8D0", "k": "#C8B898", "r": "#B8303A", "j": "#2E6A2A",
}
FIRE = [K.rgba("#7A1E14"), K.rgba("#D8402A"), K.rgba("#F28A2A"), K.rgba("#FFE070")]
EFFECT_MARKS = set("*&!~xX")


def _pad(rows):
    for r in rows:
        if len(r) != CELL_W:
            raise ValueError(f"apparatus row is {len(r)} wide, not {CELL_W}: {r!r}")
    if len(rows) > CELL_H:
        raise ValueError(f"apparatus grid is {len(rows)} tall")
    return ["." * CELL_W] * (CELL_H - len(rows)) + list(rows)


# ── The sprites ──

ATHANOR = [
    ".......55.......",
    "......P44P......",
    "......PPPP......",
    ".......PP.......",
    ".....BBBBBB.....",
    "....BBBBBBBB....",
    "...BBBBBBBBBB...",
    "..BBBBBBBBBBBB..",
    ".QQQQQQQQQQQQQQ.",
    ".PPPPPPPPPPPPPP.",
    "..BBBBBBBBBBBB..",
    "..BBBBBooBBBBB..",
    "..BBBBoxxoBBBB..",
    "..BBBBoxxoBBBB..",
    "..BBBBBooBBBBB..",
    "..BBBBBBBBBBBB..",
    "..BBBBBBBBBBBB..",
    "..BBBBBBBBBBBB..",
    "..BBBBBooBBBBB..",
    "..BBBBo**oBBBB..",
    "..BBBo****oBBB..",
    "..BBBo****oBBB..",
    "..BBBo****oBBB..",
    "..BBBo****oBBB..",
    "..BBB5ssss5BBB..",
    ".QQQQQQQQQQQQQQ.",
    ".PPPPPPPPPPPPPP.",
    ".PPPPPPPPPPPPPP.",
]

ALEMBIC = [
    ".....MM.........",
    "....GGGG........",
    "...GGGGGG.......",
    "..GGGGGGGGG.....",
    "..GGGGGGGGGGG...",
    "...GGGGGG..GGG..",
    "....GGGG.....GG.",
    ".....GG......G..",
    ".....GG......G..",
    "...GGGGGG....!..",
    "..GGGGGGGG...!..",
    ".GGGGGGGGGG..!..",
    ".GGGGGGGGGG.GGG.",
    ".GLLLLLLLLG.GLG.",
    ".GLLLLLLLLG.GLG.",
    ".GLLLLLLLLG.GLG.",
    "..GLLLLLLG..GLG.",
    "...GLLLLG...GLG.",
    "....GGGG....GGG.",
    ".WWWWWWWWWWWWWW.",
    ".VV..........VV.",
    ".VV..........VV.",
]

RETORT = [
    "......GGG.......",
    ".....GGGGG......",
    ".....GG..GGG....",
    ".....GG...GGG...",
    "....GGG....GG...",
    "...GGGG.....GG..",
    "..GGGGGG....GG..",
    ".GGGGGGGG....G..",
    ".GLLLLLLG....!..",
    ".GLLLLLLG....!..",
    ".GLLLLLLG....!..",
    "..GLLLLG.....!..",
    "...GGGG......!..",
    "..ssssssss..GGG.",
    "..s..&&..s..GLG.",
    "..s..&&..s..GLG.",
    "..s..mm..s..GLG.",
    "..s.MMMM.s..GLG.",
    "..s.MMMM.s..GLG.",
    "..s.MMMM.s..GLG.",
    ".ss.MMMM.ss.GGG.",
]

HERMETIC_VESSEL = [
    "......MMMM......",
    ".....MyMMMm.....",
    "......rrrr......",
    "......GGGG......",
    ".....GGGGGG.....",
    "....GGGGGGGG....",
    "....GGGGGGGG....",
    "...GGGGGGGGGG...",
    "...GGGGGGGGGG...",
    "...GLLLLLLLLG...",
    "..GLLLLLLLLLLG..",
    "..GLLLLLLLLLLG..",
    "..GLLLLLLLLLLG..",
    ".KKKKKKKKKKKKKK.",
    ".KKKKKKxKKKKKKK.",
    "..KKKKxKxKKKKK..",
    "...KKxxxxxKKK...",
    "....KKKKKKKK....",
    "......KKKK......",
    "......KKKK......",
    ".....KKKKKK.....",
    "....KKKKKKKK....",
    "...KKKKKKKKKK...",
]

CALCINATOR = [
    ".........IIIII..",
    ".........IIIII..",
    "..........III...",
    "..........III...",
    "..........III...",
    "..........III...",
    ".MMMMMM...III...",
    "..MMMM....III...",
    "...MM.....III...",
    ".IIIIIIIIIIIIII.",
    ".IIIIIIIIIIIIII.",
    ".UtUUUUUUUUUUtU.",
    ".UUUUUUUUUUUUUU.",
    ".UUUossssssoUUU.",
    ".UUUs******sUUU.",
    ".UUUs*s*s*s*UUU.",
    ".UUUs******sUUU.",
    ".UUUs*s*s*s*UUU.",
    ".UUUs******sUUU.",
    ".UUUossssssoUUU.",
    ".UtUUUUUUUUUUtU.",
    ".UUUUUUUUUUUUUU.",
    ".UUUommmmmmoUUU.",
    ".UUUoiiiiiioUUU.",
    ".IIIIIIIIIIIIII.",
    ".ss..........ss.",
]

PROCESS_VAT = [
    "......MMMM......",
    ".......MM.......",
    "....CCCCCCCC....",
    "...CCCCCCCCCC...",
    "..CCCCCCCCCCCC..",
    "..MMMMMMMMMMMM..",
    "..CCCCCCCCCCCC..",
    "..CCCMMMCCCCCC..",
    "..CCMl~lMCCCCC..",
    "..CCMlllMCCCCC..",
    "..CCCMMMCCCCCC..",
    "..CCCCCCCCCGGC..",
    "..MMMMMMMMMGLM..",
    "..CCCCCCCCCGLC..",
    "..CCCCCCCCCGLCMM",
    "..CCCCCCCCCGLCMy",
    "..CCCCCCCCCGLCMM",
    "..CCCCCCCCCGLC..",
    "..MMMMMMMMMMMM..",
    "..CCCCCCCCCCCC..",
    "...CCCCCCCCCC...",
    "...I........I...",
    "..II........II..",
]

PRESS = [
    "...MMMMMMMMMM...",
    "......tIIt......",
    ".VVVVVVVVVVVVVV.",
    ".WWWWWWWWWWWWWW.",
    ".VV....II....VV.",
    ".VV....IsI...VV.",
    ".VV....II....VV.",
    ".VV....IsI...VV.",
    ".VV....II....VV.",
    ".VV....IsI...VV.",
    ".VV..........VV.",
    ".VV..........VV.",
    ".VV..........VV.",
    ".VV..........VV.",
    ".VV..........VV.",
    ".VV..........VV.",
    ".VV..........VV.",
    ".VV..........VV.",
    ".VV..........VV.",
    ".VVTTTTTTTTTTVV.",
    ".WWWWWWWWWWWWWW.",
    ".WWWWWWWWWWWWWW.",
    ".VV..........VV.",
    ".VV..........VV.",
]
# The platen and the cake under it are drawn by _press, at a height that changes per frame.
PRESS_PLATEN = [
    "...UUUUUUUUUU...",
    "...UUUUUUUUUU...",
]
PRESS_TOP, PRESS_BED = 18, 26  # platen's highest row; the cake's top row (absolute)

BOILER = [
    "..........MM....",
    ".........MyMM...",
    "..........MM....",
    "....CCCCCCCC....",
    "...CCCCCCCCCC...",
    "..CCCCCCCCCCCC..",
    "..MMMMMMMMMMMM..",
    "..CCCCCCCCCCCC..",
    "..CCCCCCCClllC..",
    "..CCCCCCCll~lC..",
    "..CCCCCCCClllC..",
    "..CCCCCCCCCCCC..",
    "..CCCCCCCCCCCC..",
    "..CCCCCCCCCCCC..",
    "..MMMMMMMMMMMM..",
    "..CCCCCCCCCCCC..",
    ".QQQQQQQQQQQQQQ.",
    ".BBBBBBBBBBBBBB.",
    ".BBBBossssoBBBB.",
    ".BBBBs****sBBBB.",
    ".BBBBs****sBBBB.",
    ".BBBBs****sBBBB.",
    ".BBBBossssoBBBB.",
    ".PPPPPPPPPPPPPP.",
]

LIGHTNING_COIL = [
    ".....MMMMMM.....",
    "....MMMMMMMM....",
    "....MMMMMMMM....",
    "....MMMMMMMM....",
    ".....MMMMMM.....",
    "......KKKK......",
    ".....KKKKKK.....",
    "......KKKK......",
    "....CCCCCCCC....",
    "....cccccccc....",
    "....CCCCCCCC....",
    "....cccccccc....",
    "....CCCCCCCC....",
    "....cccccccc....",
    "....CCCCCCCC....",
    "....cccccccc....",
    "....CCCCCCCC....",
    "......KKKK......",
    ".....KKKKKK.....",
    "......KKKK......",
    "...UUUUUUUUUU...",
    "..UUUUUUUUUUUU..",
    "..UUUMMMMMMUUU..",
    "..UUUMxMMxMUUU..",
    "..UUUMMxxMMUUU..",
    "..UUUMMMMMMUUU..",
    ".IIIIIIIIIIIIII.",
]

PELICAN = [
    ".......GG.......",
    "......GGGG......",
    ".....GGGGGG.....",
    "....GGGGGGGG....",
    "...GG.GGGG.GG...",
    "..GG...GG...GG..",
    "..G....GG....G..",
    "..G....GG....G..",
    "..G....GG....G..",
    "..G....GG....G..",
    "..G....GG....G..",
    "..GG..GGGG..GG..",
    "...GGGGGGGGGG...",
    "..GGGGGGGGGGGG..",
    ".GGLLLLLLLLLLGG.",
    ".GLLLLLLLLLLLLG.",
    ".GLLLLLLLLLLLLG.",
    ".GLLLLLLLLLLLLG.",
    "..GLLLLLLLLLLG..",
    "...GLLLLLLLLG...",
    "....GGGGGGGG....",
    "...VWWWWWWWWV...",
    "..VV........VV..",
]

RITUAL_ALTAR = [
    "..&&........&&..",
    "..&&........&&..",
    "..ll..MMMM..ll..",
    "..lk..MyMm..lk..",
    "..lk...MM...lk..",
    "..lk..MMMM..lk..",
    ".QQQQQQQQQQQQQQ.",
    ".QQQQQQQQQQQQQQ.",
    "..PPPAAAAAAPPP..",
    "..PPPAAAAAAPPP..",
    "..PPPAAAAAAPPP..",
    "..PPPAAAAAAPPP..",
    "..PPPyAyAyAPPP..",
    "..PPPPPPPPPPPP..",
    "..PPPPPxPPPPPP..",
    "..PPPPxPxPPPPP..",
    "..PPPxPxPxPPPP..",
    "..PPPxxxxxPPPP..",
    "..PPPPPPPPPPPP..",
    ".QQQQQQQQQQQQQQ.",
    ".PPPPPPPPPPPPPP.",
]

QUARRY_CRUCIBLE = [
    "............tI..",
    "...........tI...",
    "..........tI....",
    ".PPPPPPPPPPPPPP.",
    "PPLLLLLLLLLLLLPP",
    "PLLLLLLLLLLLLLLP",
    ".PPLLLLLLLLLLPP.",
    ".PPPPPPPPPPPPPP.",
    ".IIIIIIIIIIIIII.",
    ".PPPPPPPPPPPPPP.",
    "..PPPPPPPPPPPP..",
    "..PPPPPPPPPPPP..",
    "..IIIIIIIIIIII..",
    "...PPPPPPPPPP...",
    "....PPPPPPPP....",
    "....PP....PP....",
    "...PPP....PPP...",
    "..QQQQQQQQQQQQ..",
    "..PPPPPPPPPPPP..",
]

LEY_TAP = [
    ".......X........",
    "......xXx.......",
    ".....xXXXx......",
    ".....xXXXx......",
    "......xXx.......",
    ".....M.x.M......",
    ".....MMMMM......",
    "....PPPPPPP.....",
    "....PPPPPPP.....",
    "....PPxPPPP.....",
    "....PPPxPPP.....",
    "....PPxPPPPMMM..",
    "....PPPxPPPMyM..",
    "....PPxPPPP..M..",
    "....PPPPPPP..!..",
    "....PPPxPPP..!..",
    "....PPxPPPP..!..",
    "...PPPPPPPPP.!..",
    "..PPPPPPPPPPP...",
    ".QQQQQQQQQQQQQ..",
    ".PPPPPPPPPPPPPP.",
]

EXPERIMENT_TABLE = [
    "........MM......",
    ".......GGGG.....",
    "........GG......",
    "........GG......",
    ".......GGGG.....",
    "......GLLLLG....",
    "......GLLLLG....",
    "......GLLLLG..P.",
    ".lll..GLLLLG.PtP",
    "lllkl..GGGG..PPP",
    "WWWWWWWWWWWWWWWW",
    "WWWWWWWWWWWWWWWW",
    ".VVVVVVVVVVVVVV.",
    ".VV..........VV.",
    ".VV..........VV.",
    ".VV..........VV.",
    ".VV..........VV.",
    ".VV..........VV.",
    ".VV..........VV.",
    ".VVVVVVVVVVVVVV.",
    ".VV..........VV.",
    ".VV..........VV.",
    ".VV..........VV.",
]

AZOTH_DIFFUSER = [
    ".....GGGGGG.....",
    "....GGGGGGGG....",
    "...GGxxxxxxGG...",
    "...GxxXXXXxxG...",
    "...GxXXXXXXxG...",
    "...GxxXXXXxxG...",
    "...GGxxxxxxGG...",
    "....GGGGGGGG....",
    ".....MMMMMM.....",
    "......MMMM......",
    ".......MM.......",
    ".......MM.......",
    ".......MM.......",
    ".......MM.......",
    ".......MM.......",
    "......MMMM......",
    ".......MM.......",
    ".......MM.......",
    ".......MM.......",
    "......MMMM......",
    ".....MM..MM.....",
    "....MM....MM....",
]

RADICULA_PLANT = [
    "......E..E......",
    ".....EE.EE..E...",
    "..E..EEEEE.EE...",
    "..EE.EEEEEEEE...",
    "...EEEEEEEEE....",
    ".....jEEEj......",
    "......RRR.......",
    ".....RRRRR......",
    "....RRRRRRR.....",
    "....RoRRRoR.....",
    "....RRRRRRR.....",
    "....RRRoRRR.....",
    "..TTTTTTTTTTTT..",
    "..TTTTTTTTTTTT..",
    "...TTTTTTTTTT...",
    "...TTTTTTTTTT...",
    "...TTTTTTTTTT...",
    "....TTTTTTTT....",
    "....TTTTTTTT....",
]

# local id: grid, liquid colour, glow colour, effects
SPRITES = {
    "ExperimentTable": dict(grid=EXPERIMENT_TABLE, liquid="#6AC84A", glow="#9B59D0"),
    "Athanor": dict(grid=ATHANOR, liquid=None, glow="#FF9A3A", smoke=[(7, 3)]),
    "Alembic": dict(grid=ALEMBIC, liquid="#5AC8B0", glow="#9B59D0", bubbles=True),
    "Retort": dict(grid=RETORT, liquid="#E0A040", glow="#FFB060", bubbles=True),
    "HermeticVessel": dict(grid=HERMETIC_VESSEL, liquid="#8A5AD0", glow="#C890FF", bubbles=True, motes=True),
    "Calcinator": dict(grid=CALCINATOR, liquid=None, glow="#FF9A3A", smoke=[(11, 6)], dark_smoke=True),
    "ProcessVat": dict(grid=PROCESS_VAT, liquid="#8AB040", glow="#C8E070", bubbles=True, gauge=True, steam=[(7, 7)]),
    "Press": dict(grid=PRESS, liquid="#C04A6A", glow="#FFB060", press=True),
    "Boiler": dict(grid=BOILER, liquid=None, glow="#FF9A3A", gauge=True, steam=[(10, 8)]),
    "LightningCoil": dict(grid=LIGHTNING_COIL, liquid=None, glow="#8AD8FF", sparks=(7, 7)),
    "Pelican": dict(grid=PELICAN, liquid="#E06A8A", glow="#FFC0D0", bubbles=True, circulate=True),
    "RitualAltar": dict(grid=RITUAL_ALTAR, liquid=None, glow="#C890FF"),
    "QuarryCrucible": dict(grid=QUARRY_CRUCIBLE, liquid="#F08A2A", glow="#FFB060", molten=True),
    "LeyTap": dict(grid=LEY_TAP, liquid="#7AE0F0", glow="#7AE0F0"),
    "AzothDiffuser": dict(grid=AZOTH_DIFFUSER, liquid=None, glow="#F4E8A0"),
    "RadiculaPlant": dict(grid=RADICULA_PLANT, liquid=None, glow="#9B59D0"),
}
ORDER = list(SPRITES)


# ── Shading ──

def _spans(rows, y, x, group_of):
    """The run of same-group cells around (x, y) on its row."""
    g = group_of(rows[y][x])
    x0 = x
    while x0 > 0 and group_of(rows[y][x0 - 1]) == g:
        x0 -= 1
    x1 = x
    while x1 < CELL_W - 1 and group_of(rows[y][x1 + 1]) == g:
        x1 += 1
    return x0, x1


def _profile_index(kind, t, n):
    if kind == "cyl":
        return 2 if t < 0.14 else 3 if t < 0.42 else 2 if t < 0.74 else 1
    if kind == "box":
        return 3 if t < 0.1 else 1 if t >= 1 - 1.0 / max(n, 2) else 2
    return 3 if t < 0.5 else 2  # flat: lit left half


def _shade(rows, liquid):
    """Paints material cells: cylinder/box profile across the row's run, lighter tops, darker bottoms, a specular
    column on shiny things and per-material texture (brick courses, stone blocks, wood grain, glass)."""
    group_of = lambda ch: GROUPS.get(ch, ch)
    lramp = K.ramp(liquid, 5) if liquid else None
    img = K.grid(["." * CELL_W] * CELL_H, {}, size=(CELL_W, CELL_H))
    px = img.load()
    for y in range(CELL_H):
        for x in range(CELL_W):
            ch = rows[y][x]
            if ch not in PROFILE:
                continue
            x0, x1 = _spans(rows, y, x, group_of)
            n = x1 - x0 + 1
            t = (x - x0 + 0.5) / n
            idx = _profile_index(PROFILE[ch], t, n)
            above = rows[y - 1][x] if y > 0 else "."
            below = rows[y + 1][x] if y < CELL_H - 1 else "."
            if group_of(above) != group_of(ch) and above not in "x":
                idx += 1
            if group_of(below) != group_of(ch) and PROFILE[ch] != "flat":
                idx -= 1
            spec = ch in SHINY or ch == "L"
            if spec and n >= 4 and x == x0 + max(1, round(n * 0.25)) and idx >= 2:
                idx = 4
            if ch == "B":  # brick courses: two rows of brick, one of mortar, joints offset each course
                course, row_in = divmod(y, 3)
                if row_in == 2 or (x + (course % 2) * 2) % 4 == 0:
                    idx = max(0, idx - 2)
                elif (course * 7 + (x + (course % 2) * 2) // 4 * 3) % 5 == 0:
                    idx = max(0, idx - 1)
            elif ch == "P":  # dressed stone blocks
                course, row_in = divmod(y, 4)
                if row_in == 3 or (x + (course % 2) * 3) % 6 == 0:
                    idx = max(0, idx - 1)
            elif ch == "W":  # plank grain
                if y % 3 == 2 and idx > 0:
                    idx -= 1
            elif ch == "U":  # iron plate seams
                if (x - x0) in (0, n - 1):
                    idx = max(0, idx - 1)
            idx = max(0, min(4, idx))
            if ch == "L":
                colour = lramp[idx] if lramp else RAMPS["G"][idx]
                if lramp and _is_surface(rows, y, x):
                    colour = lramp[min(4, idx + 1)]
            else:
                colour = RAMPS[ch][idx]
            px[x, y] = K.rgba(colour)
    return img


def _is_surface(rows, y, x):
    return y > 0 and rows[y - 1][x] != "L"


def _base(spec):
    rows = _pad(spec["grid"])
    img = _shade(rows, spec.get("liquid"))
    px = img.load()
    for y, row in enumerate(rows):
        for x, ch in enumerate(row):
            if ch in FIXED:
                px[x, y] = K.rgba(FIXED[ch])
    return rows, img


def _cells(rows, mark):
    return [(x, y) for y, row in enumerate(rows) for x, ch in enumerate(row) if ch == mark]


# ── Effects ──

def _fire(rows, px, frame, furnace=True):
    mark = "*" if furnace else "&"
    cells = _cells(rows, mark)
    if not cells:
        return
    cols = {}
    for x, y in cells:
        lo, hi = cols.get(x, (y, y))
        cols[x] = (min(lo, y), max(hi, y))
    xs = sorted(cols)
    rng = random.Random(1000 + frame * 31 + len(cells))
    for x in xs:
        top, bottom = cols[x]
        height = bottom - top + 1
        if frame == IDLE:
            for y in range(top, bottom + 1):
                if furnace:
                    px[x, y] = K.rgba(FIXED["i"]) if y < bottom or x % 2 else FIRE[0]
            continue
        if frame == READY:
            for y in range(top, bottom + 1):
                if furnace:
                    px[x, y] = FIRE[1] if y == bottom and x % 2 == 0 else FIRE[0] if y == bottom else K.rgba(FIXED["i"])
                elif y >= bottom - 1:
                    px[x, y] = FIRE[3] if y == bottom else FIRE[2]
            continue
        centre = 1 - abs((x - (xs[0] + xs[-1]) / 2) / max(1, (xs[-1] - xs[0]) / 2 + 1))
        flame = max(1, min(height, round(height * (0.45 + 0.5 * centre) + rng.choice([-1, 0, 0, 1]))))
        for y in range(top, bottom + 1):
            d = bottom - y
            if d >= flame:
                if furnace:
                    px[x, y] = K.rgba(FIXED["i"])
                continue
            f = d / flame
            px[x, y] = FIRE[3] if f < 0.35 and centre > 0.3 else FIRE[2] if f < 0.7 else FIRE[1]


def _glow(rows, px, frame, colour):
    ramp = K.ramp(colour, 5)
    for x, y in _cells(rows, "x") + _cells(rows, "X"):
        bright = rows[y][x] == "X"
        if frame == IDLE:
            c = ramp[2] if bright else ramp[1]
        elif frame == READY:
            c = (255, 255, 240, 255) if bright else ramp[4]
        else:
            pulse = [3, 4, 3, 2][WORKING.index(frame)]
            c = ramp[min(4, pulse + 1)] if bright else ramp[pulse]
        px[x, y] = K.rgba(c)


def _bubbles(rows, px, frame, liquid):
    if frame not in WORKING:
        return
    cells = set(_cells(rows, "L"))
    if not cells:
        return
    xs = sorted({x for x, _ in cells})
    light = K.rgba(K.lighter(liquid, 0.7))
    k = WORKING.index(frame)
    for n, x in enumerate(xs[1:-1:3]):
        col = sorted(y for cx, y in cells if cx == x)
        if len(col) < 2:
            continue
        y = col[-1] - ((k * 2 + n * 3) % len(col))
        px[x, y] = light
    # The surface shimmers.
    for x in xs:
        col = sorted(y for cx, y in cells if cx == x)
        if col and (x + k) % 3 == 0:
            px[x, col[0]] = K.rgba(K.lighter(liquid, 0.45))


def _drip(rows, px, frame, liquid):
    path = sorted(_cells(rows, "!"), key=lambda c: c[1])
    if not path or frame not in WORKING:
        return
    x, y = path[WORKING.index(frame) % len(path)]
    px[x, y] = K.rgba(K.lighter(liquid or "#BFE3F0", 0.3))


def _puffs(img, frame, at, dark=False):
    """Steam or smoke: a small puff at the vent and a bigger one drifting up, both rising each frame."""
    if frame not in WORKING:
        return
    k = WORKING.index(frame)
    px = img.load()
    lit, shade = ((150, 142, 146), (104, 98, 104)) if dark else ((250, 252, 255), (204, 218, 228))
    for x0, y0 in at:
        for dy, dx, r in [(2 + k, 0, 1.25), (6 + k * 2, (k % 2) - 1 + (k // 2), 2.0)]:
            cx, cy = x0 + dx + 0.5, y0 - dy + 0.5
            alpha = max(110, 235 - dy * 12)
            for y in range(int(cy - r - 1), int(cy + r + 2)):
                for x in range(int(cx - r - 1), int(cx + r + 2)):
                    d2 = (x + 0.5 - cx) ** 2 + (y + 0.5 - cy) ** 2
                    if 0 <= x < CELL_W and 0 <= y < CELL_H and d2 <= r * r and px[x, y][3] == 0:
                        c = lit if (x + 0.5 - cx) + (y + 0.5 - cy) < 0.5 else shade
                        px[x, y] = c + (alpha,)

def _gauge(rows, px, frame):
    for x, y in _cells(rows, "~"):
        px[x, y] = K.rgba(FIXED["o"])
        dx, dy = {IDLE: (-1, 1), READY: (1, 0)}.get(frame, [(-1, -1), (0, -1), (1, -1), (0, -1)][WORKING.index(frame)] if frame in WORKING else (1, 0))
        px[x + dx, y + dy] = K.rgba("#3A2A2A")


def _sparks(img, frame, origin):
    if frame not in WORKING:
        return
    px = img.load()
    rng = random.Random(77 + frame)
    k = WORKING.index(frame)
    for arc in range(2):
        x, y = origin
        side = 1 if (k + arc) % 2 else -1
        for step in range(5 + rng.randint(0, 2)):
            x += side * rng.choice([1, 1, 2])
            y += rng.choice([-1, 0, 1])
            if 0 <= x < CELL_W and 0 <= y < CELL_H and px[x, y][3] == 0:
                px[x, y] = (255, 255, 255, 255) if step % 2 == 0 else K.rgba("#8AD8FF")


def _motes(img, frame, rows, colour):
    if frame not in WORKING and frame != READY:
        return
    px = img.load()
    cells = _cells(rows, "L")
    rng = random.Random(500 + frame)
    for x, y in rng.sample(cells, min(3, len(cells))):
        px[x, y] = K.rgba(K.lighter(colour, 0.6))


def _press(img, frame, spec):
    """The platen screws down onto a cake of pulp; juice runs into the tray while it works."""
    px = img.load()
    drop = {IDLE: 0, READY: 3}.get(frame, [1, 2, 3, 2][WORKING.index(frame)] if frame in WORKING else 0)
    top = PRESS_TOP + drop
    cake_top = PRESS_BED - (1 if frame in (IDLE,) else 0)
    # Screw between the beam and the platen.
    for y in range(14, top):
        for x in (7, 8):
            if px[x, y][3] == 0:
                px[x, y] = K.rgba(RAMPS["I"][3 if (y + x) % 2 else 1])
    platen = K.grid(PRESS_PLATEN, {"U": RAMPS["U"][2]})
    pp = platen.load()
    for y in range(platen.height):
        for x in range(platen.width):
            if pp[x, y][3]:
                c = RAMPS["U"][3] if y == 0 else RAMPS["U"][1]
                px[x, top + y] = K.rgba(c)
    cake = K.ramp(spec["liquid"], 5)
    for y in range(max(top + 2, cake_top), PRESS_BED + 1):
        for x in range(4, 12):
            px[x, y] = K.rgba(cake[3] if y == max(top + 2, cake_top) else cake[2] if x < 9 else cake[1])
    if frame in WORKING or frame == READY:
        for x in range(3, 13):
            if (x + frame) % 3 == 0:
                px[x, PRESS_BED + 1] = K.rgba(cake[3])


def _circulate(rows, px, frame, liquid):
    """Liquid climbs the Pelican's arms while it works."""
    if frame not in WORKING:
        return
    k = WORKING.index(frame)
    arms = [(x, y) for x, y in _cells(rows, "G") if y < CELL_H - 18 and (x <= 2 or x >= 13)]
    lit = K.rgba(K.ramp(liquid, 5)[3])
    for x, y in arms:
        if (y + k) % 4 == 0:
            px[x, y] = lit


def _molten(rows, px, frame, liquid):
    ramp = K.ramp(liquid, 5)
    for x, y in _cells(rows, "L"):
        if frame == IDLE:
            px[x, y] = K.rgba(K.mix(K.rgba("#3A3036"), ramp[1], 0.35 if (x + y) % 3 else 0.6))
        elif frame in WORKING:
            k = WORKING.index(frame)
            px[x, y] = K.rgba(ramp[4] if (x * 3 + y + k) % 7 == 0 else ramp[3] if (x + k) % 3 else ramp[2])


def render_static(spec):
    """A still sprite from a grid in this file's materials (used for furniture)."""
    rows, img = _base(spec)
    return K.outline(img)


def render(local, frame):
    spec = SPRITES[local]
    rows, img = _base(spec)
    px = img.load()
    for ch in EFFECT_MARKS:
        for x, y in _cells(rows, ch):
            px[x, y] = (0, 0, 0, 0)
    _fire(rows, px, frame, furnace=True)
    _fire(rows, px, frame, furnace=False)
    if spec.get("molten"):
        _molten(rows, px, frame, spec["liquid"])
    if spec.get("bubbles") and spec.get("liquid"):
        _bubbles(rows, px, frame, spec["liquid"])
    if spec.get("circulate"):
        _circulate(rows, px, frame, spec["liquid"])
    _drip(rows, px, frame, spec.get("liquid"))
    _glow(rows, px, frame, spec["glow"])
    if spec.get("gauge"):
        _gauge(rows, px, frame)
    if spec.get("press"):
        _press(img, frame, spec)
    img = K.outline(img)
    if spec.get("motes"):
        _motes(img, frame, rows, spec["glow"])
    if spec.get("smoke"):
        _puffs(img, frame, spec["smoke"], dark=spec.get("dark_smoke", False))
    if spec.get("steam"):
        _puffs(img, frame, spec["steam"])
    if spec.get("sparks"):
        _sparks(img, frame, spec["sparks"])
    return img


ANIMATED = {k for k, s in SPRITES.items()
            if k not in ("ExperimentTable", "AzothDiffuser", "RadiculaPlant")}


def sprite_index(local):
    return ORDER.index(local) * CELLS


def build(out_path):
    cells = []
    for local in ORDER:
        frames = [render(local, IDLE), render(local, READY)] + [render(local, f) for f in WORKING]
        cells += frames + [None] * (CELLS - len(frames))
    K.sheet(cells, CELL_W, CELL_H, CELLS).save(out_path)

# Light while working (Data/Machines LightWhileWorking): radius in tiles and the light's colour, which the game's
# lighting inverts (DarkCyan gives the warm orange of vanilla's own furnace light; Orange gives blue).
LIGHT = {
    "Athanor": (1.25, "DarkCyan"),
    "Calcinator": (1.25, "DarkCyan"),
    "Boiler": (1.0, "DarkCyan"),
    "QuarryCrucible": (1.0, "DarkCyan"),
    "RitualAltar": (0.75, "DarkCyan"),
    "HermeticVessel": (0.75, "DarkGreen"),
    "LightningCoil": (1.0, "Orange"),
    "LeyTap": (0.75, "DarkRed"),
}
