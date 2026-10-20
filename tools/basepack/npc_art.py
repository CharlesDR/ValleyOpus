"""Placeholder NPC art (doc 21 art notes): overworld sprites dressed from cyanowl's CC0 "32-pixel Human Sprites" bases
(clothes recoloured here, CC0 hair pieces from the same pack), portraits from Ravenmore's Fantasy Portrait Pack
(CC-BY, credited in CREDITS.md) hue-shifted per NPC, the Curator from the CC0 "Ghost monster" sheet, and the four
Familiars from Clint Bellanger's CC0 Tiny Creatures."""

import colorsys
import os

from PIL import Image

HUMANS = "32-pixel-human-sprites"
PORTRAITS = "fantasy-portrait-pack-by-ravenmore/RavenmorePortraitPack_1/PortraitPack"
GHOST = "ghost-monster/anim-nme-ghost.png"
CREATURES = "tiny-creatures/tiny-creatures/tiny-creatures/Tilemap/tilemap_packed.png"

# The body sheet: three view rows per skin tone (front walk, side walk, back walk), frames on a 16 px pitch.
SKIN_TOPS = {"light": 11, "tan": 123}
ROW_PITCH = 34


def hex_rgb(colour):
    return tuple(int(colour[i:i + 2], 16) for i in (1, 3, 5))


def bands(values, test):
    out, start, prev = [], None, None
    for v in values:
        if test(v):
            if start is None:
                start = v
            prev = v
        elif start is not None:
            out.append((start, prev))
            start = None
    if start is not None:
        out.append((start, prev))
    return out


def body_frame(sheet, skin, view, frame):
    top = SKIN_TOPS[skin] + view * ROW_PITCH
    x = 11 + 16 * frame
    return sheet.crop((x, top + 1, x + 16, top + 33)).convert("RGBA")


def dress(frame, shirt, pants):
    """Recolours the torso and legs of a bare body frame, keeping its shading."""
    out = frame.copy()
    px = out.load()
    shirt, pants = hex_rgb(shirt), hex_rgb(pants)
    for y in range(32):
        colour = shirt if 13 <= y <= 21 else pants if y >= 22 else None
        if colour is None:
            continue
        for x in range(16):
            r, g, b, a = px[x, y]
            if a == 0:
                continue
            shade = min(1.2, (r + g + b) / 3 / 200.0)
            if y >= 30:
                colour_here = (60, 40, 30)
                shade = min(1.0, shade)
            else:
                colour_here = colour
            px[x, y] = tuple(max(0, min(255, int(c * shade))) for c in colour_here) + (a,)
    return out


def hair_pieces(hair_sheet):
    """Hair pieces by (gender, view) → list of boxes, left to right."""
    px = hair_sheet.load()
    w, h = hair_sheet.size
    rows = [b for b in bands(range(h), lambda y: any(px[x, y][3] > 0 for x in range(w))) if b[1] - b[0] >= 9]
    pieces = {}
    female_rows = [r for r in rows if 37 <= r[0] < 140]
    male_rows = [r for r in rows if r[0] >= 165]
    for gender, view_rows in (("f", female_rows), ("m", male_rows)):
        for view, (y0, y1) in enumerate(view_rows[:3]):
            xs = bands(range(w), lambda x: any(px[x, y][3] > 0 for y in range(y0, y1 + 1)))
            pieces[(gender, view)] = [(x0, y0, x1 + 1, y1 + 1) for x0, x1 in xs]
    return pieces


def add_hair(frame, hair_sheet, pieces, hair, view, mirror):
    gender, colour, style = hair
    boxes = pieces.get((gender, view), [])
    if not boxes:
        return frame
    if gender == "m" and view == 2:
        index = colour
    else:
        index = colour * 2 + (style % 2)
    box = boxes[min(index, len(boxes) - 1)]
    piece = hair_sheet.crop(box)
    if mirror:
        piece = piece.transpose(Image.FLIP_LEFT_RIGHT)
    head = frame.crop((0, 0, 16, 13)).getbbox() or (2, 0, 14, 13)
    cx = (head[0] + head[2]) // 2
    x = cx - piece.width // 2
    y = head[1] - 1
    out = frame.copy()
    out.paste(piece, (x, y), piece)
    return out


def walk_sheet(art_root, skin, shirt, pants, hair):
    sheet = Image.open(os.path.join(art_root, HUMANS, "human_sprites_0.png")).convert("RGBA")
    hair_sheet = Image.open(os.path.join(art_root, HUMANS, "human_sprites_hair.png")).convert("RGBA")
    pieces = hair_pieces(hair_sheet)
    out = Image.new("RGBA", (64, 128), (0, 0, 0, 0))
    # Stardew rows: 0 down, 1 right, 2 up, 3 left. The source side view faces left.
    plan = [(0, [0, 2, 4, 6], False, 0), (1, [0, 1, 2, 3], True, 1), (2, [0, 2, 4, 6], False, 2), (1, [0, 1, 2, 3], False, 1)]
    for row, (view, frames, mirror, hair_view) in enumerate(plan):
        for col, f in enumerate(frames):
            img = dress(body_frame(sheet, skin, view, f), shirt, pants)
            if mirror:
                img = img.transpose(Image.FLIP_LEFT_RIGHT)
            img = add_hair(img, hair_sheet, pieces, hair, hair_view, mirror)
            out.paste(img, (col * 16, row * 32))
    return out


def shift_hue(img, degrees):
    """Rotates the hue of everything but skin tones, so two NPCs built from one portrait base look different."""
    out = img.copy()
    px = out.load()
    for y in range(out.height):
        for x in range(out.width):
            r, g, b, a = px[x, y]
            if a == 0:
                continue
            h, s, v = colorsys.rgb_to_hsv(r / 255, g / 255, b / 255)
            if h <= 0.12 and 0.1 <= s <= 0.9 and v > 0.3:
                continue  # skin
            h = (h + degrees / 360) % 1.0
            nr, ng, nb = colorsys.hsv_to_rgb(h, s, v)
            px[x, y] = (int(nr * 255), int(ng * 255), int(nb * 255), a)
    return out


def adjust(img, factor, desaturate=False):
    out = img.copy()
    px = out.load()
    for y in range(out.height):
        for x in range(out.width):
            r, g, b, a = px[x, y]
            if desaturate:
                m = (r + g + b) // 3
                r, g, b = (r + m) // 2, (g + m) // 2, (b + m) // 2
            px[x, y] = (min(255, int(r * factor)), min(255, int(g * factor)), min(255, int(b * factor)), a)
    return out


def portrait_sheet(base):
    """Four expressions in a 2x2 sheet (neutral, happy, sad, unique), all variations on one 64x64 face."""
    sheet = Image.new("RGBA", (128, 128), (0, 0, 0, 0))
    for i, img in enumerate([base, adjust(base, 1.12), adjust(base, 0.85, desaturate=True), adjust(base, 1.0, desaturate=True)]):
        sheet.paste(img, ((i % 2) * 64, (i // 2) * 64))
    return sheet


def build(art_root, npcs, out_dir):
    os.makedirs(out_dir, exist_ok=True)
    used = {}
    for nid, row in npcs.items():
        display, gender, age, home, birthday, unlock, portrait, hair, clothes, gifts, lines = row
        if nid == "Curator":
            ghost = Image.open(os.path.join(art_root, GHOST)).convert("RGBA")
            frames = [ghost.crop((i * 32, 0, i * 32 + 32, 32)) for i in range(4)]
            sheet = Image.new("RGBA", (128, 128), (0, 0, 0, 0))
            for r in range(4):
                for c, f in enumerate(frames):
                    sheet.paste(f.transpose(Image.FLIP_LEFT_RIGHT) if r == 3 else f, (c * 32, r * 32))
            sheet.save(os.path.join(out_dir, nid + ".png"))
            face = frames[0].crop((4, 0, 28, 24)).resize((64, 64), Image.NEAREST)
            portrait_sheet(face).save(os.path.join(out_dir, nid + "_portrait.png"))
            continue
        skin = {"Tobin": "tan", "Ivo": "tan", "Delta": "tan", "Bennet": "tan", "Agnes": "light"}.get(nid, "light")
        walk_sheet(art_root, skin, clothes[0], clothes[1], hair).save(os.path.join(out_dir, nid + ".png"))
        base = Image.open(os.path.join(art_root, PORTRAITS, portrait + "_64.png")).convert("RGBA")
        n = used.get(portrait, 0)
        used[portrait] = n + 1
        portrait_sheet(shift_hue(base, 75 * n) if n else base).save(os.path.join(out_dir, nid + "_portrait.png"))


def build_familiars(art_root, familiars, out_path):
    sheet = Image.open(os.path.join(art_root, CREATURES)).convert("RGBA")
    cols = sheet.width // 16
    out = Image.new("RGBA", (16 * len(familiars), 16), (0, 0, 0, 0))
    for i, (fid, row) in enumerate(familiars.items()):
        index = row[-1]
        out.paste(sheet.crop(((index % cols) * 16, (index // cols) * 16, (index % cols) * 16 + 16, (index // cols) * 16 + 16)), (i * 16, 0))
    out.save(out_path)
