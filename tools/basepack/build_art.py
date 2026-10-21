"""Placeholder item art (art notes, doc 21): each Element is a CC0 flask (pixel-potion-set-16x16 by yafarida)
recoloured to its Prime colour, with its CC0 Idylwild rune drawn small on the glass. The Satchel and Lens are
simple hand-drawn 16x16 sprites."""

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


def build(art_root, sprites, out_path):
    sheet = Image.open(os.path.join(art_root, "pixel-potion-set-16x16", "PotionSet16x16.png")).convert("RGBA")
    codex = Image.open(os.path.join(art_root, "idylwilds-alchemical-codex", "idylwilds_alchemical_codex", "alchemical_codex.png")).convert("RGBA")
    rows = (len(sprites) + COLUMNS - 1) // COLUMNS
    atlas = Image.new("RGBA", (COLUMNS * 16, rows * 16), (0, 0, 0, 0))
    for index, (local, colour, rune, flask_row) in enumerate(sprites):
        if colour == "satchel":
            icon = draw_ascii(SATCHEL)
        elif colour == "lens":
            icon = draw_ascii(LENS)
        else:
            icon = flask(sheet, flask_row, colour)
            mask = small_rune(codex, rune)
            ink = (255, 250, 230, 255) if sum(hex_rgb(colour)) < 420 else (40, 30, 30, 255)
            rune_layer = Image.new("RGBA", (8, 8), ink)
            icon.paste(rune_layer, (4, 7), mask)
        atlas.paste(icon, ((index % COLUMNS) * 16, (index // COLUMNS) * 16))
    os.makedirs(os.path.dirname(out_path), exist_ok=True)
    atlas.save(out_path)
