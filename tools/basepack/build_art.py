"""Placeholder item art (art notes, doc 21): each Element is a CC0 flask (pixel-potion-set-16x16 by yafarida)
recoloured to its Prime colour, with its CC0 Idylwild rune drawn small on the glass. The Satchel and Lens are
simple hand-drawn 16x16 sprites. Worn items, curios and new items use CC0 7soul1 icons scaled to 16x16."""

import os
from PIL import Image

COLUMNS = 16

SATCHEL = [
    "................",
    "......####......",
    ".....#....#.....",
    ".....#....#.....",
    "...##########...",
    "..#bbbbbbbbbb#..",
    "..#bBBBBBBBBb#..",
    "..#bbbbggbbbb#..",
    "..#bbbbggbbbb#..",
    "..#bbbbbbbbbb#..",
    "..#bBbbbbbbBb#..",
    "..#bbbbbbbbbb#..",
    "..#bbbbbbbbbb#..",
    "...##########...",
    "................",
    "................",
]
LENS = [
    "................",
    ".....#####......",
    "....#ccccc#.....",
    "...#cwwcccc#....",
    "...#cwccccc#....",
    "...#ccccccc#....",
    "...#ccccccc#....",
    "...#ccccccc#....",
    "....#ccccc#.....",
    ".....#####g.....",
    "..........gg....",
    "...........hh...",
    "............hh..",
    ".............hh.",
    "..............#.",
    "................",
]
PALETTE = {"#": (40, 26, 18, 255), "b": (140, 90, 50, 255), "B": (110, 68, 36, 255), "g": (210, 170, 60, 255),
           "c": (170, 220, 235, 255), "w": (245, 252, 255, 255), "h": (96, 60, 34, 255)}


def hex_rgb(colour):
    return tuple(int(colour[i:i + 2], 16) for i in (1, 3, 5))


def draw_ascii(rows):
    img = Image.new("RGBA", (16, 16), (0, 0, 0, 0))
    for y, row in enumerate(rows):
        for x, ch in enumerate(row):
            if ch in PALETTE:
                img.putpixel((x, y), PALETTE[ch])
    return img


def flask(sheet, row, colour):
    """Recolour the blue potion (column 0) of a row: liquid pixels (blue-dominant) take the target hue."""
    src = sheet.crop((0, row * 16, 16, row * 16 + 16)).convert("RGBA")
    target = hex_rgb(colour)
    out = src.copy()
    for y in range(16):
        for x in range(16):
            r, g, b, a = src.getpixel((x, y))
            if a == 0:
                continue
            if b > r + 30 and b > g:  # the blue liquid
                shade = (r + g + b) / 3 / 170.0
                out.putpixel((x, y), tuple(min(255, int(c * shade)) for c in target) + (a,))
    return out


def small_rune(codex, rune):
    """The 16px rune reduced to 8x8: a pixel is ink if at least two of its 2x2 block are dark."""
    r, c = rune
    glyph = codex.crop((c * 16, r * 16, c * 16 + 16, r * 16 + 16)).convert("RGBA")
    mask = Image.new("L", (8, 8), 0)
    for y in range(8):
        for x in range(8):
            dark = 0
            for dy in (0, 1):
                for dx in (0, 1):
                    pr, pg, pb, pa = glyph.getpixel((x * 2 + dx, y * 2 + dy))
                    if pa > 0 and pr + pg + pb < 200:
                        dark += 1
            if dark >= 2:
                mask.putpixel((x, y), 255)
    return mask


ICONS = "7soul1-496-rpg-icons/496_RPG_icons"


def icon(art_root, name):
    """A 7soul1 CC0 icon (34x34 with a frame of empty space), cropped to its pixels and scaled into 16x16."""
    src = Image.open(os.path.join(art_root, ICONS, name + ".png")).convert("RGBA")
    box = src.getbbox() or (0, 0, src.width, src.height)
    src = src.crop(box)
    scale = 16 / max(src.width, src.height)
    w, h = max(1, round(src.width * scale)), max(1, round(src.height * scale))
    small = src.resize((w, h), Image.LANCZOS)
    out = Image.new("RGBA", (16, 16), (0, 0, 0, 0))
    px = small.load()
    for y in range(h):
        for x in range(w):
            r, g, b, a = px[x, y]
            px[x, y] = (r, g, b, 255 if a >= 110 else 0)
    out.paste(small, ((16 - w) // 2, (16 - h) // 2), small)
    return out


def build(art_root, sprites, out_path):
    sheet = Image.open(os.path.join(art_root, "pixel-potion-set-16x16", "PotionSet16x16.png")).convert("RGBA")
    codex = Image.open(os.path.join(art_root, "idylwilds-alchemical-codex", "idylwilds_alchemical_codex", "alchemical_codex.png")).convert("RGBA")
    rows = (len(sprites) + COLUMNS - 1) // COLUMNS
    atlas = Image.new("RGBA", (COLUMNS * 16, rows * 16), (0, 0, 0, 0))
    for index, (local, kind, value, rune, flask_row) in enumerate(sprites):
        if kind == "satchel":
            img = draw_ascii(SATCHEL)
        elif kind == "lens":
            img = draw_ascii(LENS)
        elif kind == "icon":
            img = icon(art_root, value)
        else:
            img = flask(sheet, flask_row, value)
            mask = small_rune(codex, rune)
            ink = (255, 250, 230, 255) if sum(hex_rgb(value)) < 420 else (40, 30, 30, 255)
            img.paste(Image.new("RGBA", (8, 8), ink), (4, 7), mask)
        atlas.paste(img, ((index % COLUMNS) * 16, (index // COLUMNS) * 16))
    os.makedirs(os.path.dirname(out_path), exist_ok=True)
    atlas.save(out_path)


def furniture_layout(furniture):
    """Each piece's tile column on the furniture sheet: pieces sit side by side on one row band."""
    cols, x = [], 0
    for local, display, ftype, (w, h), _, desc in furniture:
        cols.append(x)
        x += w
    return cols, x, max(h for _, _, _, (w, h), _, _ in furniture)


def build_furniture(art_root, furniture, out_path):
    """The Circle Sigil (a ring of Idylwild runes, drawn here) and the cobalt glazed décor (drawn here)."""
    from PIL import ImageDraw
    codex = Image.open(os.path.join(art_root, "idylwilds-alchemical-codex", "idylwilds_alchemical_codex", "alchemical_codex.png")).convert("RGBA")
    cols, width, height = furniture_layout(furniture)
    sheet = Image.new("RGBA", (width * 16, height * 16), (0, 0, 0, 0))
    cobalt, light, dark = (46, 82, 170, 255), (120, 160, 230, 255), (24, 40, 96, 255)
    for (local, display, ftype, (w, h), _, desc), col in zip(furniture, cols):
        img = Image.new("RGBA", (w * 16, h * 16), (0, 0, 0, 0))
        d = ImageDraw.Draw(img)
        if local == "CircleSigil":
            size = w * 16
            d.ellipse((2, 2, size - 3, size - 3), outline=(150, 40, 40, 255), width=2)
            d.ellipse((12, 12, size - 13, size - 13), outline=(150, 40, 40, 255), width=1)
            d.polygon([(size // 2, 14), (size - 18, size - 22), (18, size - 22)], outline=(170, 120, 40, 255))
            import math
            for i in range(8):
                ang = i * math.pi / 4
                cx, cy = size / 2 + math.cos(ang) * (size / 2 - 8), size / 2 + math.sin(ang) * (size / 2 - 8)
                mask = small_rune(codex, (0, 1 + i))
                img.paste(Image.new("RGBA", (8, 8), (150, 40, 40, 255)), (int(cx - 4), int(cy - 4)), mask)
        elif ftype == "rug":
            for ty in range(h):
                for tx in range(w):
                    x0, y0 = tx * 16, ty * 16
                    d.rectangle((x0, y0, x0 + 15, y0 + 15), fill=cobalt, outline=dark)
                    d.rectangle((x0 + 5, y0 + 5, x0 + 10, y0 + 10), fill=light)
        elif local == "GlazedUrn":
            d.ellipse((3, 4, 12, 9), fill=dark)
            d.polygon([(4, 8), (11, 8), (14, 18), (12, 28), (3, 28), (1, 18)], fill=cobalt, outline=dark)
            d.line((4, 15, 11, 15), fill=light)
        else:
            d.polygon([(2, 5), (13, 5), (11, 15), (4, 15)], fill=cobalt, outline=dark)
            d.ellipse((4, 0, 11, 6), fill=(70, 140, 60, 255))
        sheet.paste(img, (col * 16, 0))
    os.makedirs(os.path.dirname(out_path), exist_ok=True)
    sheet.save(out_path)
