"""Item and furniture sheets for the base pack (sos.3 placeholder art, all drawn for ValleyOpus by tools/basepack, so no
third-party pixels ship): item icons come from item_icons.py; the Circle Sigil and the cobalt glazed décor are drawn
here."""

import math
import os
import random

from PIL import Image

import apparatus_art as A
import icon_shapes as S
import item_icons as I
import pixelkit as K

COLUMNS = 16


def build(sprites, out_path):
    """objects.png: one 16x16 icon per sprite slot, in the order build_base_pack assigned them."""
    cells = [I.icon_for(local) for local, *_ in sprites]
    os.makedirs(os.path.dirname(out_path), exist_ok=True)
    K.sheet(cells, 16, 16, COLUMNS).save(out_path)


def furniture_layout(furniture):
    """Each piece's tile column on the furniture sheet: pieces sit side by side on one row band."""
    cols, x = [], 0
    for local, display, ftype, (w, h), _, desc in furniture:
        cols.append(x)
        x += w
    return cols, x, max(h for _, _, _, (w, h), _, _ in furniture)


# ── Furniture ──

CHALK = ["#6E2A24", "#9A3A2E", "#C2583E"]  # red ochre, as drawn on a floor


def _ring(px, size, r, width, colours, rng, wear=0.06):
    c = (size - 1) / 2
    for y in range(size):
        for x in range(size):
            d = math.hypot(x - c, y - c)
            if abs(d - r) <= width / 2 and rng.random() > wear:
                # Lit from the top left: the upper-left arc is the bright chalk.
                lit = (x - c) + (y - c) < 0
                px[x, y] = K.rgba(colours[2] if lit and abs(d - r) < width / 2 - 0.3 else colours[1])


def _line(px, a, b, colour, rng, wear=0.06):
    (x0, y0), (x1, y1) = a, b
    steps = int(max(abs(x1 - x0), abs(y1 - y0)))
    for i in range(steps + 1):
        x = round(x0 + (x1 - x0) * i / max(1, steps))
        y = round(y0 + (y1 - y0) * i / max(1, steps))
        if rng.random() > wear:
            px[x, y] = K.rgba(colour)


def circle_sigil(size):
    """A ritual circle in red chalk: two rings with the eight Prime runes between them, a triangle and a small circle
    inside (doc 13 §2: the altar stands at its centre)."""
    img = Image.new("RGBA", (size, size), (0, 0, 0, 0))
    px = img.load()
    rng = random.Random(7)
    c = (size - 1) / 2
    _ring(px, size, size / 2 - 3, 2.2, CHALK, rng)
    _ring(px, size, size / 2 - 13, 1.4, CHALK, rng)
    _ring(px, size, size / 2 - 27, 1.2, CHALK, rng)
    r_tri = size / 2 - 14
    pts = [(c + r_tri * math.cos(a), c + r_tri * math.sin(a)) for a in (-math.pi / 2, math.pi / 6, 5 * math.pi / 6)]
    for i in range(3):
        _line(px, pts[i], pts[(i + 1) % 3], CHALK[1], rng)
    runes = ["Ignis", "Sulphur", "Aer", "Mercurius", "Aqua", "Sal", "Terra", "Aether"]
    r_rune = size / 2 - 8
    for i, name in enumerate(runes):
        a = -math.pi / 2 + i * math.pi / 4
        x0, y0 = round(c + r_rune * math.cos(a) - 2), round(c + r_rune * math.sin(a) - 2)
        for dy, row in enumerate(S.RUNES[name]):
            for dx, ch in enumerate(row):
                if ch == "#":
                    px[x0 + dx, y0 + dy] = K.rgba(CHALK[0] if (dx + dy) % 4 == 3 else CHALK[1])
    return img


URN = [
    "....ZZZZZZZZ....",
    "....ZllllllZ....",
    ".....ZZZZZZ.....",
    ".....ZZZZZZ.....",
    ".....ZZZZZZ.....",
    "....ZZZZZZZZ....",
    "...ZZZZZZZZZZ...",
    "..ZZZZZZZZZZZZ..",
    "..ZlZlZlZlZlZZ..",
    ".ZZZZZZZZZZZZZZ.",
    ".ZZZZZZZZZZZZZZ.",
    ".ZZZZllZZllZZZZ.",
    ".ZZZlZZllZZlZZZ.",
    ".ZZZZllZZllZZZZ.",
    ".ZZZZZZlZZZZZZZ.",
    ".ZZZZZZZZZZZZZZ.",
    "..ZlZlZlZlZlZZ..",
    "..ZZZZZZZZZZZZ..",
    "...ZZZZZZZZZZ...",
    "....ZZZZZZZZ....",
    ".....ZZZZZZ.....",
    "......ZZZZ......",
    ".....ZZZZZZ.....",
    "....ZZZZZZZZ....",
]

PLANTER = [
    "......E.E.......",
    "...E.EEEEE.E....",
    "...EEEEEEEEE....",
    "....EEEjEEE.....",
    "..ZZZZZZZZZZZZ..",
    "..ZlZlZlZlZlZZ..",
    "...ZZZZZZZZZZ...",
    "...ZZZllZZZZZ...",
    "...ZZlZZlZZZZ...",
    "...ZZZllZZZZZ...",
    "....ZZZZZZZZ....",
    ".....ZZZZZZ.....",
]


def glazed(grid, height):
    """A cobalt-glazed piece, shaded with the apparatus materials."""
    spec = {"grid": grid, "liquid": None, "glow": "#FFFFFF"}
    img = A.render_static(spec)
    return img.crop((0, 32 - height, 16, 32))


def glazed_tiles(w, h):
    """Blue-and-white tiles with a four-petal flower on each, laid in a square."""
    img = Image.new("RGBA", (w * 16, h * 16), (0, 0, 0, 0))
    px = img.load()
    white, cream, grout = K.rgba("#EEF1F4"), K.rgba("#D8DEE6"), K.rgba("#8E96A4")
    blue = K.ramp("#2E52AA", 5)
    for y in range(h * 16):
        for x in range(w * 16):
            tx, ty = x % 16, y % 16
            if tx == 15 or ty == 15:
                px[x, y] = grout
                continue
            edge = tx in (0, 14) or ty in (0, 14)
            cx, cy = tx - 7, ty - 7
            petal = (abs(cx) <= 1 and 2 <= abs(cy) <= 4) or (abs(cy) <= 1 and 2 <= abs(cx) <= 4)
            corner = abs(cx) + abs(cy) >= 12
            if edge:
                px[x, y] = K.rgba(blue[1])
            elif cx == 0 and cy == 0:
                px[x, y] = K.rgba(blue[4])
            elif petal:
                px[x, y] = K.rgba(blue[3] if cx + cy < 0 else blue[2])
            elif corner:
                px[x, y] = K.rgba(blue[2])
            else:
                px[x, y] = white if (tx + ty) % 9 else cream
    return img


def build_furniture(furniture, out_path):
    cols, width, height = furniture_layout(furniture)
    sheet = Image.new("RGBA", (width * 16, height * 16), (0, 0, 0, 0))
    for (local, display, ftype, (w, h), _, desc), col in zip(furniture, cols):
        if local == "CircleSigil":
            img = circle_sigil(w * 16)
        elif local == "GlazedUrn":
            img = glazed(URN, 32)
        elif local == "GlazedPlanter":
            img = glazed(PLANTER, 16)
        else:
            img = glazed_tiles(w, h)
        sheet.alpha_composite(img, (col * 16, 0))
    os.makedirs(os.path.dirname(out_path), exist_ok=True)
    sheet.save(out_path)
