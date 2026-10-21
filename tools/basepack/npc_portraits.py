"""NPC portraits (sos.3 placeholder art): each villager's head is assembled from Nila122's 64x64 portrait kit
(OpenGameArt, OGA-BY 3.0, credited in CREDITS.md), and the clothes, props and expression touches (glasses, goggles,
blush, soot, sparkles, a loupe) are drawn here from the commission brief (review/art-commission-needs.xlsx, Portraits).

Stardew's portrait sheet is 64x64 per expression, two per row: 0 neutral, 1 happy ($h), 2 sad ($s), 3 unique ($u),
4 blush ($l) and 5 angry ($a). The kit's heads fill the frame, so each is lifted a few pixels to leave room for
shoulders."""

import glob
import math
import os

from PIL import Image

import pixelkit as K

KIT_DIR = os.path.join("64x64-portrait", "Portrait", "Portrait")
SIZE = 64
LIFT = 5
EXPRESSIONS = ["neutral", "happy", "sad", "unique", "blush", "angry"]

# Eye and cheek positions on the kit's three-quarter face (it looks toward the viewer's left).
EYES = (9, 24, 44, 35)
NEAR_EYE = (34, 30)
FAR_EYE = (16, 30)
CHEEKS = [(12, 37), (35, 37)]
MOUTH_AT = (24, 51)


class Kit:
    def __init__(self, art_root):
        self.root = os.path.join(art_root, KIT_DIR)
        self._cache = {}

    def layer(self, *parts):
        path = os.path.join(self.root, *parts)
        if path not in self._cache:
            self._cache[path] = Image.open(path).convert("RGBA")
        return self._cache[path]

    def coloured(self, folder, colour):
        """A part that comes in the kit's 31 hair colours, e.g. ("Hair/knot", "raven")."""
        hits = sorted(glob.glob(os.path.join(glob.escape(self.root), folder, f"*_{colour}.png")))
        if not hits:
            raise FileNotFoundError(f"{folder} in {colour}")
        return self.layer(os.path.relpath(hits[0], self.root))


# ── Small drawing helpers ──

def _lum(c):
    return (0.299 * c[0] + 0.587 * c[1] + 0.114 * c[2]) / 255


def recolour(img, colour, keep=None):
    """Repaints a layer onto a ramp of colour by each pixel's brightness relative to the layer's own range, keeping
    its near-black outline (so a red bandana can become a cream head wrap without losing its folds)."""
    out = img.copy()
    px = out.load()
    pixels = [px[x, y] for y in range(out.height) for x in range(out.width) if px[x, y][3]]
    lums = [_lum(c) for c in pixels if _lum(c) > 0.16]
    if not lums:
        return out
    lo, hi = min(lums), max(lums)
    r = K.ramp(colour, 5)
    for y in range(out.height):
        for x in range(out.width):
            c = px[x, y]
            if not c[3] or _lum(c) <= 0.16 or (keep and keep(c)):
                continue
            t = (_lum(c) - lo) / max(0.01, hi - lo)
            px[x, y] = K.rgba(r[min(4, int(t * 4.99))])[:3] + (c[3],)
    return out


def shifted(img, dx, dy):
    out = Image.new("RGBA", img.size, (0, 0, 0, 0))
    out.alpha_composite(img, (max(0, dx), max(0, dy)), (max(0, -dx), max(0, -dy)))
    return out


def put(px, x, y, colour, alpha=None):
    if 0 <= x < SIZE and 0 <= y < SIZE:
        c = K.rgba(colour)
        if alpha is None:
            px[x, y] = c
        else:
            old = px[x, y]
            a = alpha * (c[3] / 255)
            if old[3] == 0:
                px[x, y] = c[:3] + (int(255 * a),)
            else:
                px[x, y] = tuple(int(old[i] * (1 - a) + c[i] * a) for i in range(3)) + (old[3],)


def sparkle(px, x, y, colour="#FFFFFF", big=False):
    put(px, x, y, colour)
    for dx, dy in ((1, 0), (-1, 0), (0, 1), (0, -1)):
        put(px, x + dx, y + dy, colour, 0.7)
    if big:
        for dx, dy in ((2, 0), (-2, 0), (0, 2), (0, -2)):
            put(px, x + dx, y + dy, colour, 0.35)


# ── Clothes: drawn behind the head (shoulders) and over the neck (collars) ──

NECK = 57 - LIFT  # the row where the lifted neck meets the shoulders


def _shoulders(y):
    """The body's left and right edge on row y, or None above the shoulders."""
    if y < NECK:
        return None
    return max(0, 30 - 6 * (y - NECK)), min(SIZE - 1, 51 + 4 * (y - NECK))


def clothes_back(look, skin_mid):
    img = Image.new("RGBA", (SIZE, SIZE), (0, 0, 0, 0))
    px = img.load()
    cloth = K.ramp(look["cloth"], 5)
    inner = K.ramp(look.get("inner", look["cloth"]), 5)
    edge = K.outline_of(look["cloth"])
    kind = look.get("clothes", "shirt")
    for y in range(NECK, SIZE):
        a, b = _shoulders(y)
        for x in range(a, b + 1):
            # Lit from the left like the kit's faces, with a fold under the near shoulder.
            lit = 3 if x < 28 else 1 if x > 52 or (x == 50 and y > NECK + 6) else 2
            px[x, y] = K.rgba(cloth[lit])
        put(px, a, y, edge)
        put(px, b, y, edge)
    # Top edge of the shoulders, with a highlight just under it.
    for x in range(SIZE):
        for y in range(NECK, SIZE):
            if px[x, y][3]:
                put(px, x, y, edge)
                if y + 1 < SIZE:
                    put(px, x, y + 1, cloth[4] if x < 40 else cloth[3])
                break
    # The neckline: a V from the neck down, showing the inner layer (a shirt, or skin for an open collar).
    vx = 40
    for y in range(NECK + 2, SIZE):
        half = int((y - NECK - 2) * look.get("v", 0.55)) + 4
        for x in range(vx - half, vx + half + 1):
            px[x, y] = K.rgba(skin_mid) if look.get("open") else K.rgba(inner[3 if x < vx else 2])
        if kind in ("robe", "labcoat", "coat", "patchwork", "leather"):
            # Lapels or robe edges either side of the V.
            lap = K.ramp(look.get("lapel", look["cloth"]), 5)
            put(px, vx - half - 1, y, lap[4])
            put(px, vx - half - 2, y, lap[2])
            put(px, vx + half + 1, y, lap[2])
            put(px, vx + half + 2, y, lap[1])
            put(px, vx - half - 3, y, edge)
            put(px, vx + half + 3, y, edge)
        else:
            put(px, vx - half - 1, y, edge)
            put(px, vx + half + 1, y, edge)
        if look.get("trim"):
            put(px, vx - half - 1, y, K.ramp(look["trim"], 5)[3])
            put(px, vx + half + 1, y, K.ramp(look["trim"], 5)[2])
    if kind == "patchwork":
        for (x0, dy, c) in [(6, 7, "#3B5E3A"), (53, 5, "#E3C21E"), (17, 9, "#5A8ACA"), (49, 10, "#7A3A8A")]:
            for y in range(NECK + dy, min(SIZE, NECK + dy + 5)):
                for x in range(x0, x0 + 6):
                    if px[x, y][3]:
                        px[x, y] = K.rgba(K.ramp(c, 5)[2 if (x + y) % 5 else 1])
            for x in range(x0, x0 + 6):
                if NECK + dy < SIZE and px[x, NECK + dy][3]:
                    put(px, x, NECK + dy, K.darker(c, 0.5))
    if kind == "leather":
        # Stitched seams on worn leather.
        for x in range(4, 60, 3):
            if px[x, NECK + 10][3]:
                put(px, x, NECK + 10, K.darker(look["cloth"], 0.45))
    if look.get("stars"):
        for x, dy in [(10, 9), (22, 6), (56, 7), (49, 11), (16, 12), (59, 12), (4, 13)]:
            if NECK + dy < SIZE and px[x, NECK + dy][3]:
                sparkle(px, x, NECK + dy, "#F2D27A")
    if look.get("strap"):
        # A strap over the near shoulder, across the chest toward the far hip (satchel or bandolier).
        st = K.ramp(look["strap"], 5)
        for y in range(NECK, SIZE):
            x = 53 - int((y - NECK) * 1.6)
            for k in range(3):
                put(px, x + k, y, st[3 - k])
            put(px, x - 1, y, K.outline_of(look["strap"]))
            put(px, x + 3, y, K.outline_of(look["strap"]))
        if look.get("buckle"):
            bx = 53 - int(7 * 1.6)
            for x, y in [(bx, NECK + 7), (bx + 1, NECK + 7), (bx, NECK + 8), (bx + 1, NECK + 8)]:
                put(px, x, y, "#D2A64E" if y == NECK + 7 else "#9A7A2A")
    if look.get("braces"):
        # Apron straps (Tobin, Lark) down both sides of the chest, and the bib they hold up.
        st = K.ramp(look["braces"], 5)
        for y in range(NECK + 1, SIZE):
            for x0, way in ((30, -1), (48, 1)):
                x = x0 + way * ((y - NECK - 1) // 4)
                put(px, x, y, st[3])
                put(px, x + 1, y, st[2])
        if look.get("bib"):
            bib = K.ramp(look["bib"], 5)
            for y in range(NECK + 8, SIZE):
                for x in range(29, 52):
                    put(px, x, y, bib[3] if x < 40 else bib[2])
            for x in range(29, 52):
                put(px, x, NECK + 8, bib[4])
                put(px, x, NECK + 7, K.outline_of(look["bib"]))
    return img


def collar_front(look):
    """What sits over the neck: a high collar and cravat for the Curator, a neckerchief for Finch, a tie for
    Dr. Sorenson, a cloth wrap for Bennet."""
    img = Image.new("RGBA", (SIZE, SIZE), (0, 0, 0, 0))
    px = img.load()
    kind = look.get("collar")
    if kind == "wing":
        # A high wing collar with its tips turned down, and a cravat knotted at the throat with a gold pin.
        white = K.ramp("#EEF2F8", 5)
        line = K.outline_of("#9AA6C0")
        for y in range(NECK - 1, NECK + 3):
            for x in range(32, 50):
                put(px, x, y, white[3] if x < 41 else white[2])
        for x in range(32, 50):
            put(px, x, NECK - 2, line)
        for i in range(4):
            for x in range(34 - i, 38):
                put(px, x, NECK + 3 + i, white[4] if x < 36 else white[3])
            for x in range(43, 47 + i):
                put(px, x, NECK + 3 + i, white[2])
            put(px, 33 - i, NECK + 3 + i, line)
            put(px, 47 + i, NECK + 3 + i, line)
        colour = look.get("cravat", "#3A4C78")
        cr = K.ramp(colour, 5)
        for y in range(NECK + 1, SIZE):
            w = 2 if y < NECK + 4 else 3 if y < NECK + 8 else 2
            for x in range(40 - w, 41 + w):
                put(px, x, y, cr[3] if x < 40 else cr[2])
            put(px, 39 - w, y, K.outline_of(colour))
            put(px, 41 + w, y, K.outline_of(colour))
        put(px, 40, NECK + 5, "#F2D27A")
        put(px, 40, NECK + 6, "#B8862A")
    elif kind == "kerchief":
        colour = look.get("kerchief", "#E3C21E")
        k = K.ramp(colour, 5)
        for y in range(NECK + 1, NECK + 6):
            for x in range(31 - (y - NECK), 52 + (y - NECK) // 2):
                put(px, x, y, k[3] if (x + y) % 7 else k[4])
        for i in range(SIZE - NECK - 6):
            for x in range(37 - i // 2, 44 + i // 3):
                put(px, x, NECK + 6 + i, k[2] if x > 40 else k[3])
        for x in range(30, 53):
            put(px, x, NECK, K.outline_of(colour))
    elif kind == "tie":
        shirt = K.ramp("#F4F4F0", 5)
        for y in range(NECK + 2, SIZE):
            half = int((y - NECK - 2) * 0.55) + 3
            for x in range(40 - half, 41 + half):
                put(px, x, y, shirt[3] if x < 40 else shirt[2])
        tie = K.ramp(look.get("tie", "#4A5560"), 5)
        for y in range(NECK + 3, SIZE):
            w = 1 if y < NECK + 5 else 2
            for x in range(40 - w, 41 + w):
                put(px, x, y, tie[3] if x <= 40 else tie[2])
    elif kind == "wrap":
        # Loose cloth around the neck (Bennet's desert wrap falls from his head).
        colour = look.get("wrap", "#ECE2C4")
        w = K.ramp(colour, 5)
        for y in range(NECK - 1, NECK + 8):
            for x in range(27, 55):
                d = (x - 41) ** 2 / 169 + (y - NECK - 3) ** 2 / 20
                if d <= 1:
                    put(px, x, y, K.outline_of(colour) if d > 0.82 else w[3] if (x + 2 * y) % 9 else w[2])
    return img


# ── Props ──

def glasses(px, kind, colour):
    rim = K.outline_of(colour) if kind != "gold" else "#B8862A"
    if kind in ("round", "gold"):
        lenses = [(15, 30, 6.2), (34, 30, 7.4)]
        for cx, cy, r in lenses:
            for y in range(int(cy - r) - 1, int(cy + r) + 2):
                for x in range(int(cx - r) - 1, int(cx + r) + 2):
                    d = math.hypot(x + 0.5 - cx, (y + 0.5 - cy) * 1.15)
                    if r - 0.6 <= d <= r + 0.5:
                        put(px, x, y, rim)
                    elif d < r - 0.6:
                        put(px, x, y, "#DDEFFA", 0.18)
        bridge = [(21, 29), (22, 28), (23, 28), (24, 28), (25, 28), (26, 29)]
    else:
        boxes = [(8, 25, 21, 33), (26, 25, 42, 33)]
        for x0, y0, x1, y1 in boxes:
            for x in range(x0, x1 + 1):
                put(px, x, y0, rim)
                put(px, x, y1, rim)
            for y in range(y0, y1 + 1):
                put(px, x0, y, rim)
                put(px, x1, y, rim)
            for y in range(y0 + 1, y1):
                for x in range(x0 + 1, x1):
                    put(px, x, y, "#DDEFFA", 0.16)
        bridge = [(22, 28), (23, 27), (24, 27), (25, 28)]
    for x, y in bridge:
        put(px, x, y, rim)
    for x in range(42, 48):
        put(px, x, 28, rim)
    # Glare on the near lens.
    for x, y in [(37, 26), (38, 27), (39, 28), (29, 30)]:
        put(px, x, y, "#FFFFFF", 0.75)


def goggles(px, down=False, strap="#5A3A22", rim="#D2A64E", lens="#9AD8F0", big=False):
    cy = 30 if down else 15
    r = (8.0 if big else 6.5) if down else (6.5 if big else 5.2)
    centres = [(15, cy), (34, cy)] if down else [(19, cy), (35, cy)]
    st = K.ramp(strap, 5)
    for x in range(4, 60):
        for dy in (-1, 0, 1):
            put(px, x, cy + dy, st[2 if dy else 3])
    rr = K.ramp(rim, 5)
    ll = K.ramp(lens, 5)
    for cx, cyy in centres:
        for y in range(int(cyy - r) - 2, int(cyy + r) + 3):
            for x in range(int(cx - r) - 2, int(cx + r) + 3):
                d = math.hypot(x + 0.5 - cx, y + 0.5 - cyy)
                if d <= r - 1.4:
                    lit = (x - cx) + (y - cyy) < -1
                    put(px, x, y, ll[4] if lit else ll[2])
                elif d <= r:
                    put(px, x, y, rr[3] if y < cyy else rr[1])
                elif d <= r + 0.9:
                    put(px, x, y, K.outline_of(rim))
        put(px, cx - 2, cyy - 2, "#FFFFFF")
        put(px, cx - 1, cyy - 2, "#FFFFFF", 0.6)


def loupe(img, px):
    """A jeweller's loupe over the near eye, the eye inside it magnified."""
    cx, cy, r = 34, 30, 9.5
    eye = img.crop((28, 25, 41, 35)).resize((26, 20), Image.NEAREST)
    lens = Image.new("RGBA", (SIZE, SIZE), (0, 0, 0, 0))
    lens.alpha_composite(eye, (21, 20))
    lp = lens.load()
    for y in range(SIZE):
        for x in range(SIZE):
            d = math.hypot(x + 0.5 - cx, y + 0.5 - cy)
            if d <= r - 1.5 and lp[x, y][3]:
                px[x, y] = lp[x, y]
            elif d <= r - 1.5:
                put(px, x, y, "#FFFFFF")
    brass = K.ramp("#D2A64E", 5)
    for y in range(SIZE):
        for x in range(SIZE):
            d = math.hypot(x + 0.5 - cx, y + 0.5 - cy)
            if r - 1.5 < d <= r + 0.6:
                put(px, x, y, brass[3] if y < cy else brass[1])
            elif r + 0.6 < d <= r + 1.4:
                put(px, x, y, "#5A3A12")
    for x, y in [(29, 25), (30, 24), (31, 24)]:
        put(px, x, y, "#FFFFFF", 0.8)


def blush(px, strength=0.45):
    for cx, cy in CHEEKS:
        for i in range(3):
            for k in range(4):
                x, y = cx + i * 2 + k // 2, cy + 1 - k // 2 + (k % 2)
                put(px, x, y, "#E8506A", strength)


def soot(px, spots):
    for x, y, r in spots:
        for yy in range(y - r, y + r + 1):
            for xx in range(x - r - 1, x + r + 2):
                if math.hypot((xx - x) * 0.8, yy - y) <= r and (xx * 3 + yy * 5) % 4:
                    put(px, xx, yy, "#3A3236", 0.55)


def puff(px, cx, cy):
    for (dx, dy, r) in [(-8, -2, 5), (0, -5, 6), (8, -1, 5), (-3, 3, 4), (5, 4, 4)]:
        for y in range(cy + dy - r, cy + dy + r + 1):
            for x in range(cx + dx - r, cx + dx + r + 1):
                d = math.hypot(x - cx - dx, y - cy - dy)
                if d <= r:
                    put(px, x, y, "#B8B4B0" if d > r - 1.2 else "#E4E0DC", 0.8)


def scar(px):
    for i, (x, y) in enumerate([(38, 19), (38, 20), (39, 21), (39, 22), (40, 33), (40, 34), (41, 35), (41, 36), (42, 37)]):
        put(px, x, y, "#C07A6A")


def freckles(px, colour):
    for x, y in [(14, 38), (17, 39), (19, 37), (33, 38), (36, 39), (38, 37), (27, 40)]:
        put(px, x, y, colour, 0.75)


def glow_eyes(px, colour):
    g = K.ramp(colour, 5)
    for cx, cy in (FAR_EYE, NEAR_EYE):
        for y in range(cy - 4, cy + 4):
            for x in range(cx - 6, cx + 7):
                d = math.hypot(x - cx, (y - cy) * 1.6)
                if 5 <= d <= 7:
                    put(px, x, y, g[4], 0.35)


# ── Assembly ──

def _eye_mask(base, pupils):
    bp, pp = base.load(), pupils.load()
    x0, y0, x1, y1 = EYES
    mask = set()
    for y in range(y0, y1):
        for x in range(x0, x1):
            c = bp[x, y]
            if pp[x, y][3] or (c[3] and min(c[:3]) > 225):
                mask.add((x, y))
    return mask


def head(kit, look, expr):
    """The kit layers for one expression (still at the kit's own position: lifted later)."""
    skin = look["skin"]
    face = lambda name: kit.layer("Face", skin, f"{name}_{skin}.png")
    base = kit.layer("Base", f"base_{skin}.png")
    img = base.copy()
    pupils = kit.layer("Face", "(Other", "pupil", f"eyes_{look['eyes']}.png")
    if expr.get("pupil_colour"):
        pupils = recolour(pupils, expr["pupil_colour"])
    mask = _eye_mask(base, pupils)
    look_dx, look_dy = expr.get("look", (0, 0))
    px = img.load()
    if (look_dx, look_dy) != (0, 0):
        white = (255, 255, 255, 255)
        for x, y in mask:
            px[x, y] = white
        moved = shifted(pupils, look_dx, look_dy).load()
        for x, y in mask:
            if moved[x, y][3]:
                px[x, y] = moved[x, y]
    else:
        img.alpha_composite(pupils)
    if expr.get("eye_sparkle"):
        for x, y in [(13, 28), (32, 28), (35, 31)]:
            put(px, x, y, "#FFFFFF")
    lids = expr.get("lids")
    if lids:
        img.alpha_composite(face(f"eyes_{lids}"))
    if look.get("lashes"):
        img.alpha_composite(kit.layer("Face", "(Other", "Eyelashes",
                                      f"eyelashes_{ {'closed': 'closed', 'sadeye': 'sadeyes'}.get(lids, 'normal') }.png"))
    for line in look.get("lines", []):
        img.alpha_composite(face(f"lines_{line}"))
    img.alpha_composite(face(f"nose_{look.get('nose', 'normal')}"))
    mood = expr.get("brow", "normal")
    brows = kit.coloured(f"Face/(Other/Brow/{look.get('brow', 'basic')}-{mood}", look["brow_colour"])
    browbase = face(f"browbase_{mood}")
    if expr.get("raise_near_brow"):
        # One brow raised (Dr. Sorenson's scepticism): the near half moves up two pixels.
        def raised(layer):
            out = layer.copy()
            out.paste((0, 0, 0, 0), (27, 0, SIZE, SIZE))
            out.alpha_composite(layer.crop((27, 3, SIZE, SIZE)), (27, 0))
            return out
        brows, browbase = raised(brows), raised(browbase)
    img.alpha_composite(browbase)
    img.alpha_composite(brows)
    mouth = expr.get("mouth", "normal")
    if mouth == "smile":
        img.alpha_composite(face("mouth_normal_smile"))
    elif mouth == "frown":
        smile = face("mouth_normal_smile")
        box = smile.getbbox()
        part = smile.crop(box).transpose(Image.FLIP_TOP_BOTTOM)
        img.alpha_composite(part, (box[0], box[1] + 1))
    elif mouth == "open":
        dark, red = K.rgba("#3A1A1E"), K.rgba("#A04048")
        for y in range(49, 55):
            for x in range(22, 29):
                d = ((x + 0.5 - 25.5) / 3.2) ** 2 + ((y + 0.5 - 52) / 2.9) ** 2
                if d <= 1:
                    px[x, y] = dark if d > 0.55 or y < 51 else red
    else:
        img.alpha_composite(face("mouth_normal"))
    if mouth == "tongue":
        for x, y in [(28, 53), (29, 53), (28, 54), (29, 54), (30, 54)]:
            put(px, x, y, "#E0707A")
        put(px, 28, 55, "#B04A58")
    for kind in look.get("facial_hair", []):
        img.alpha_composite(kit.coloured(f"facialhair/{kind}", look["hair_colour"]))
    for acc in look.get("accessories", []):
        img.alpha_composite(kit.layer("Accessory", f"{acc}.png"))
    return img


def portrait(kit, nid, expr_name):
    look = LOOKS[nid]
    expr = dict(EXPRESSION_DEFAULTS[expr_name])
    if expr_name == "unique":
        expr.update(look["unique"])
    expr.update(look.get("overrides", {}).get(expr_name, {}))
    face_img = head(kit, look, expr)
    fp = face_img.load()
    if look.get("freckles"):
        freckles(fp, look["freckles"])
    if look.get("scar"):
        scar(fp)
    if expr.get("blush"):
        blush(fp, expr["blush"])
    if look.get("soot") or expr.get("soot"):
        soot(fp, expr.get("soot") or look["soot"])
    # Hair, then headwear and props over it.
    hair = kit.coloured(f"Hair/{look['hair']}", look["hair_colour"]) if look.get("hair") else None
    hat = None
    if look.get("hat"):
        name, colour = look["hat"]
        hat = recolour(kit.layer("Headwear", f"{name}.png"), colour, keep=look.get("hat_keep"))
    skin_mid = kit.layer("Base", f"base_{look['skin']}.png").getpixel((30, 40))
    out = Image.new("RGBA", (SIZE, SIZE), (0, 0, 0, 0))
    out.alpha_composite(clothes_back(look, skin_mid))
    lifted = Image.new("RGBA", (SIZE, SIZE), (0, 0, 0, 0))
    lifted.alpha_composite(face_img, (0, 0))
    out.alpha_composite(shifted(lifted, 0, -LIFT))
    out.alpha_composite(collar_front(look))
    top = Image.new("RGBA", (SIZE, SIZE), (0, 0, 0, 0))
    tp = top.load()
    if hair:
        top.alpha_composite(hair)
    goggle = look.get("goggles")
    if goggle and not expr.get("goggles_down"):
        goggles(tp, down=False, big=goggle == "big", strap=look.get("goggle_strap", "#5A3A22"))
    if expr.get("goggles_down"):
        goggles(tp, down=True, big=goggle == "big", strap=look.get("goggle_strap", "#5A3A22"))
    if look.get("glasses"):
        glasses(tp, look["glasses"], look.get("glasses_colour", "#3A3436"))
    if hat:
        top.alpha_composite(hat)
    if look.get("feather"):
        fx, fy = look.get("feather_at", (50, 4))
        fr = K.ramp(look["feather"], 5)
        for i in range(10):
            x, y = fx + i // 2, fy + i
            put(tp, x, y, fr[3])
            put(tp, x + 1, y, fr[2])
            put(tp, x - 1, y, K.outline_of(look["feather"]))
    out.alpha_composite(shifted(top, 0, -LIFT))
    op = out.load()
    if expr.get("loupe"):
        loupe(out, op)
    if expr.get("glow"):
        glow_eyes(op, expr["glow"])
        for x, y in [(6, 18), (52, 12), (58, 30), (4, 40)]:
            sparkle(op, x, y, K.lighter(expr["glow"], 0.5))
    for x, y, big in expr.get("sparkles", []):
        sparkle(op, x, y, "#FFF6C8", big)
    if expr.get("puff"):
        puff(op, *expr["puff"])
    if look.get("ghost"):
        out = _spectral(out)
    return out


def _spectral(img):
    """The Curator is pale and faintly luminous: a cool tint over everything and a soft halo at the edge (the game draws
    his portrait opaque, as the brief asks)."""
    out = img.copy()
    px = out.load()
    for y in range(SIZE):
        for x in range(SIZE):
            r, g, b, a = px[x, y]
            if a:
                px[x, y] = (int(r * 0.9 + 200 * 0.1), int(g * 0.9 + 220 * 0.1), int(b * 0.9 + 255 * 0.1), a)
    halo = Image.new("RGBA", (SIZE, SIZE), (0, 0, 0, 0))
    hp = halo.load()
    for y in range(SIZE):
        for x in range(SIZE):
            if px[x, y][3]:
                continue
            near = any(0 <= x + dx < SIZE and 0 <= y + dy < SIZE and px[x + dx, y + dy][3] > 200
                       for dx, dy in ((1, 0), (-1, 0), (0, 1), (0, -1)))
            if near:
                hp[x, y] = (190, 230, 255, 110)
    halo.alpha_composite(out)
    return halo


EXPRESSION_DEFAULTS = {
    "neutral": {},
    "happy": {"mouth": "smile"},
    "sad": {"brow": "sad", "lids": "sadeye", "mouth": "frown", "look": (0, 1)},
    "unique": {},
    "blush": {"brow": "sad", "mouth": "smile", "look": (-1, 1), "blush": 0.55},
    "angry": {"brow": "angry", "mouth": "frown"},
}

# Per NPC, from the commission brief: skin, eyes, hair, clothes, props, and the brief's unique expression.
LOOKS = {
    "Oriel": dict(skin="light", eyes="violet", brow="basic", brow_colour="raven2", hair="knot", hair_colour="raven2",
                  lashes=True, nose="straight", clothes="robe", cloth="#5B3A8C", inner="#8A6AB8", lapel="#3C2A5A",
                  trim="#D2A64E", strap="#7A4A2A",
                  # Ritual focus: eyes half-closed, a faint glow in them.
                  unique={"lids": "sadeye", "pupil_colour": "#E8C8FF", "glow": "#B07AE0", "mouth": "normal"},
                  accessories=["earring_gold"]),
    "Tobin": dict(skin="tanned2", eyes="brown", brow="thick", brow_colour="brown", hair="messy", hair_colour="brown",
                  facial_hair=["beard"], nose="big", clothes="shirt", cloth="#3E4656", inner="#3E4656",
                  braces="#7A5232", bib="#7A5232", goggles="up", soot=[(42, 40, 1), (12, 22, 1)],
                  # Concentrating: goggles down, tongue out, mid-tinker.
                  unique={"goggles_down": True, "mouth": "tongue", "brow": "angry"}),
    "Curator": dict(skin="darkelf2", eyes="gray", brow="thick", brow_colour="gray", hair="parted", hair_colour="gray",
                    facial_hair=["beard", "mustache"], nose="straight", lines=["forehead", "eye"], clothes="coat",
                    cloth="#2E5A6E", lapel="#24485A", inner="#EEF2F8", collar="wing", cravat="#3A4C78",
                    glasses="gold", ghost=True,
                    # Wistful, looking into the distance.
                    unique={"brow": "sad", "look": (-2, -1), "mouth": "normal"}),
    "Finch": dict(skin="light", eyes="green", brow="thick", brow_colour="darkred", hair="halfmessy", hair_colour="darkred",
                  facial_hair=["frenchstache"], nose="potato", clothes="patchwork", cloth="#C2542D", lapel="#9A3A1E",
                  inner="#E3C21E", collar="kerchief", kerchief="#E3C21E", hat=("leathercap", "#3B5E3A"),
                  hat_keep=lambda c: c[0] > c[1] * 1.9 and c[0] > 90, accessories=["earring_gold"],
                  # Sales pitch: a grin and a sparkle off the curio he holds up.
                  unique={"mouth": "smile", "brow": "normal", "eye_sparkle": True,
                          "sparkles": [(56, 50, True), (60, 44, False), (50, 56, False)]},
                  overrides={"angry": {"brow": "angry", "mouth": "frown"}}),
    "Sorenson": dict(skin="light", eyes="gray", brow="basic", brow_colour="brown", hair="page",
                     hair_colour="brown", lashes=True, nose="button", clothes="labcoat", cloth="#ECECEC",
                     lapel="#F8F8F8", inner="#4A5560", collar="tie", tie="#4A5560", glasses="square",
                     glasses_colour="#3A4048",
                     # Sceptical: an eyebrow raised over the glasses.
                     unique={"raise_near_brow": True, "lids": "sadeye", "mouth": "normal"}),
    "Hesper": dict(skin="dark2", eyes="brown", brow="basic", brow_colour="black", hair="jefro", hair_colour="black",
                   lashes=True, nose="normal", clothes="coat", cloth="#2A3A6E", lapel="#1C2340", inner="#D8E0EE",
                   stars=True, accessories=["earring_gold"],
                   # Starstruck: gazing upward, lost in thought.
                   unique={"look": (1, -3), "mouth": "smile", "eye_sparkle": True,
                           "sparkles": [(8, 10, True), (56, 8, False), (60, 22, True), (14, 2, False)]}),
    "Ivo": dict(skin="tanned2", eyes="gray", brow="thick", brow_colour="black", hair="bedhead", hair_colour="black",
                facial_hair=["beard"], nose="straight", clothes="leather", cloth="#4A3C2E", lapel="#3A2E22",
                inner="#2A2A2A", strap="#6A4A2A", buckle=True, scar=True,
                # Appraising: squinting at a monster part.
                unique={"lids": "sadeye", "brow": "angry", "mouth": "normal", "look": (-1, 1)}),
    "Delta": dict(skin="tanned", eyes="blue", brow="basic", brow_colour="darkblonde", hair="ponytail",
                  hair_colour="lightblonde", lashes=True, nose="button", clothes="shirt", cloth="#3A8FD8", open=True,
                  strap="#8A6A3A", freckles="#B0703A", hat=("chainhat", "#E8DCB4"),
                  # Fascinated: peering at a sample jar.
                  unique={"look": (-2, 2), "mouth": "open", "eye_sparkle": True}),
    "Agnes": dict(skin="light", eyes="gray", brow="thick", brow_colour="gray", hair="knot", hair_colour="gray",
                  lashes=True, nose="straight", lines=["forehead", "eyecorner"], clothes="coat", cloth="#7A7A7A",
                  lapel="#5A5A5A", inner="#3A3A3A", strap="#5A4632",
                  # Inspecting through a loupe, one eye magnified.
                  unique={"loupe": True, "lids": None, "brow": "angry", "mouth": "normal"}),
    "Bennet": dict(skin="dark", eyes="brown", brow="thick", brow_colour="black", hair_colour="black",
                   facial_hair=["fiveoclock"], nose="big", clothes="shirt", cloth="#E0A040", inner="#8A5A2B", open=True,
                   hat=("bandana", "#D8C8A0"),
                   # Proud artisan: holding a piece up to the light.
                   unique={"look": (-1, -2), "mouth": "smile", "eye_sparkle": True,
                           "sparkles": [(6, 14, True), (12, 6, False)]}),
    "Lark": dict(skin="light", eyes="green", brow="basic", brow_colour="red2", hair="bunches", hair_colour="red2",
                 lashes=True, nose="button", clothes="shirt", cloth="#5B8C3A", inner="#5B8C3A", braces="#E3C21E",
                 bib="#E3C21E", goggles="big", goggle_strap="#6A4A2A", freckles="#D07A4A",
                 # Starry-eyed admiration.
                 unique={"mouth": "smile", "eye_sparkle": True, "look": (0, -1), "blush": 0.3,
                         "sparkles": [(6, 30, True), (58, 34, True), (54, 16, False)]}),
}


def sheet(kit, nid):
    out = Image.new("RGBA", (SIZE * 2, SIZE * 3), (0, 0, 0, 0))
    for i, name in enumerate(EXPRESSIONS):
        out.alpha_composite(portrait(kit, nid, name), ((i % 2) * SIZE, (i // 2) * SIZE))
    return out


def build(art_root, nid, out_path):
    sheet(Kit(art_root), nid).save(out_path)
