"""Overworld NPC sprites (sos.3 placeholder art, drawn for ValleyOpus by tools/basepack): every villager is built here
from body, clothing, hair and hat parts in Stardew's 16x32 frame (rows: down, right, up, left; four walk frames each),
coloured from the commission brief's palette (doc 21). The Curator floats instead of walking and is drawn translucent.
"""

from PIL import Image

import pixelkit as K

W, H = 16, 32
DOWN, RIGHT, UP, LEFT = 0, 1, 2, 3

SKINS = {"light": "#F2CDA8", "fair": "#F6D8BC", "tan": "#D9A274", "brown": "#A8704A", "dark": "#7A4C32", "ghost": "#DCE6F2"}
EYE = (42, 32, 48, 255)
MOUTH = (176, 82, 82, 255)
SHOES = "#4A3428"


class Painter:
    def __init__(self):
        self.img = Image.new("RGBA", (W, H), (0, 0, 0, 0))
        self.px = self.img.load()

    def set(self, x, y, colour):
        if 0 <= x < W and 0 <= y < H and colour is not None:
            self.px[x, y] = K.rgba(colour)

    def get(self, x, y):
        return self.px[x, y] if 0 <= x < W and 0 <= y < H else (0, 0, 0, 0)

    def rect(self, x0, y0, x1, y1, ramp, shade=True):
        """Fills a box lit from the left: the left column light, the right column in shadow."""
        for y in range(y0, y1 + 1):
            for x in range(x0, x1 + 1):
                if not shade or x0 == x1:
                    self.set(x, y, ramp[2])
                elif x == x0:
                    self.set(x, y, ramp[3])
                elif x == x1:
                    self.set(x, y, ramp[1])
                else:
                    self.set(x, y, ramp[2])


def ramps(spec):
    r = {k: K.ramp(v, 5) for k, v in spec.items() if isinstance(v, str) and v.startswith("#")}
    r["skin"] = K.ramp(SKINS[spec["skin"]], 5, spread=0.7)
    r.setdefault("shoes", K.ramp(spec.get("shoes", SHOES), 5))
    return r


# ── Body ──

def legs(p, r, view, frame, spec, top):
    """Trousers (or bare shins) and shoes. frame 1/3 are steps."""
    pants = r.get("bottom", r["top"])
    shin = r["skin"] if spec.get("shins") else pants
    floor = 30
    if view in (DOWN, UP):
        lift_left = frame == 3
        lift_right = frame == 1
        for x0, lifted in ((5, lift_left), (8, lift_right)):
            bottom = floor - (1 if lifted else 0)
            for y in range(top, bottom - 1):
                ramp = shin if spec.get("shins") and y >= top + 3 else pants
                p.rect(x0, y, x0 + 2, y, ramp)
            p.rect(x0, bottom - 1, x0 + 2, bottom, r["shoes"])
    else:
        # Side: the near leg and the far leg swing apart on steps.
        stride = {0: 0, 1: 2, 2: 0, 3: -2}[frame]
        far = r["shoes"], pants
        for leg, (dx, lifted) in enumerate(((-stride // 2 if stride else 0, False), (stride // 2 if stride else 0, frame in (1, 3)))):
            x0 = 6 + dx
            bottom = floor - (1 if lifted and stride else 0)
            for y in range(top, bottom - 1):
                slant = 0 if y < top + 2 else (dx > 0) - (dx < 0)
                ramp = shin if spec.get("shins") and y >= top + 3 else pants
                if leg == 0:
                    ramp = [K.darker(c, 0.15) for c in ramp]
                p.rect(x0 + slant, y, x0 + slant + 2, y, ramp)
            sx = x0 + ((dx > 0) - (dx < 0))
            shoe = r["shoes"] if leg else [K.darker(c, 0.15) for c in r["shoes"]]
            p.rect(sx, bottom - 1, sx + 3, bottom, shoe)


def torso(p, r, view, frame, spec, y0):
    top = r["top"]
    x0, x1 = (4, 11) if view in (DOWN, UP) else (5, 10)
    for y in range(y0, y0 + 9):
        a, b = (x0 + 1, x1 - 1) if y == y0 else (x0, x1)
        p.rect(a, y, b, y, top)
    if spec.get("belt"):
        p.rect(x0, y0 + 8, x1, y0 + 8, r[spec["belt"]], shade=False)


def arms(p, r, view, frame, spec, y0, front):
    """Sleeves and hands. Side view draws the near arm over the body (front=True pass)."""
    sleeve = r.get("sleeve", r["top"])
    hand = r["skin"]
    short = spec.get("rolled")
    if view in (DOWN, UP):
        if front:
            return
        swing = {0: (0, 0), 1: (-1, 1), 2: (0, 0), 3: (1, -1)}[frame]
        for x, s in ((3, swing[0]), (12, swing[1])):
            top = y0 + 1 + s
            for y in range(top, top + 7):
                ramp = hand if short and y >= top + 4 else sleeve
                p.set(x, y, ramp[3] if x == 3 else ramp[1])
            p.set(x, top + 7, hand[2])
    else:
        if not front:
            return
        swing = {0: 0, 1: 1, 2: 0, 3: -1}[frame]
        for i in range(7):
            x = 7 + (swing if i >= 4 else 0)
            ramp = hand if short and i >= 4 else sleeve
            p.set(x, y0 + 1 + i, ramp[2])
            p.set(x + 1, y0 + 1 + i, ramp[1])
        p.set(7 + swing, y0 + 8, hand[2])
        p.set(8 + swing, y0 + 8, hand[1])


def head(p, r, view, y0):
    skin = r["skin"]
    if view in (DOWN, UP):
        for y in range(y0, y0 + 11):
            a, b = (4, 11) if y in (y0, y0 + 10) else (3, 12)
            p.rect(a, y, b, y, skin)
        p.rect(6, y0 + 11, 9, y0 + 11, [skin[1]] * 5, shade=False)
    else:
        for y in range(y0, y0 + 11):
            a, b = (5, 11) if y in (y0, y0 + 10) else (4, 12)
            p.rect(a, y, b, y, skin)
        p.set(13, y0 + 7, skin[2])  # nose
        p.rect(6, y0 + 11, 8, y0 + 11, [skin[1]] * 5, shade=False)


def face(p, r, view, y0, spec):
    if view == DOWN:
        for x in (5, 10):
            p.set(x, y0 + 5, EYE)
            p.set(x, y0 + 6, EYE)
        if spec.get("blush"):
            p.set(4, y0 + 7, K.mix(r["skin"][2], MOUTH, 0.35))
            p.set(11, y0 + 7, K.mix(r["skin"][2], MOUTH, 0.35))
        mouth = r["skin"][0] if spec.get("ghost") else MOUTH
        p.set(7, y0 + 8, mouth)
        p.set(8, y0 + 8, mouth)
    elif view == RIGHT:
        p.set(11, y0 + 5, EYE)
        p.set(11, y0 + 6, EYE)
        p.set(12, y0 + 8, r["skin"][0] if spec.get("ghost") else MOUTH)


# ── Hair ──

def hair(p, r, view, y0, spec):
    style = spec.get("hair_style", "short")
    if style == "none":
        return
    h = r["hair"]
    vol = 1 if style in ("curly", "long", "bunches") else 0
    if view in (DOWN, UP):
        # Cap over the crown.
        for y in range(y0 - 1 - vol, y0 + 3):
            a, b = (5, 10) if y < y0 else (3, 12)
            if y == y0 - 1 - vol:
                a, b = 5, 10
            for x in range(a, b + 1):
                p.set(x, y, h[3] if (y < y0 + 1 and 5 <= x <= 7) else h[2])
        if view == DOWN:
            fringe = {"short": [4, 5, 7, 8, 10, 11], "long": [3, 4, 5, 6, 9, 10, 11, 12], "bob": [3, 4, 5, 6, 7, 10, 11, 12],
                      "curly": [3, 4, 6, 7, 8, 9, 11, 12], "bun": [4, 5, 6, 9, 10, 11], "ponytail": [4, 5, 6, 7, 8],
                      "bunches": [4, 5, 6, 7, 8, 9, 10, 11], "neat": [3, 4, 5, 6, 7, 8, 9]}.get(style, [4, 5, 10, 11])
            for x in fringe:
                p.set(x, y0 + 3, h[1])
            side = {"short": 5, "long": 17, "bob": 9, "curly": 9, "bun": 5, "ponytail": 6, "bunches": 8, "neat": 8}.get(style, 5)
            for y in range(y0 + 3, y0 + side):
                p.set(3, y, h[2] if y < y0 + 11 else h[1])
                p.set(12, y, h[1])
                if style in ("long", "curly") and y >= y0 + 4:
                    p.set(2, y, h[1] if y > y0 + 11 else h[2])
                    p.set(13, y, h[1])
            if style == "bunches":
                for side_x in (1, 2, 13, 14):
                    for y in range(y0 + 3, y0 + 9):
                        p.set(side_x, y, h[2] if side_x in (2, 13) else h[1])
        else:
            # Back of the head: all hair.
            length = {"short": 10, "long": 18, "bob": 11, "curly": 12, "bun": 10, "ponytail": 10, "bunches": 10, "neat": 11}.get(style, 10)
            for y in range(y0, y0 + length):
                a, b = (3, 12) if y < y0 + 11 else (3, 12) if style == "long" else (4, 11)
                for x in range(a, b + 1):
                    p.set(x, y, h[1] if x > 9 else h[2])
            if style == "ponytail":
                for y in range(y0 + 8, y0 + 16):
                    p.set(7, y, h[2])
                    p.set(8, y, h[1])
            if style == "bunches":
                for y in range(y0 + 3, y0 + 9):
                    for x in (1, 2, 13, 14):
                        p.set(x, y, h[2])
        if style == "bun":
            for y in range(y0 - 3, y0 - 1):
                for x in range(6, 10):
                    p.set(x, y, h[3] if x < 8 else h[2])
    else:
        for y in range(y0 - 1 - vol, y0 + 3):
            a, b = (5, 11) if y < y0 else (4, 12)
            for x in range(a, b + 1):
                p.set(x, y, h[3] if y < y0 + 1 and 6 <= x <= 9 else h[2])
        length = {"short": 6, "long": 17, "bob": 10, "curly": 10, "bun": 6, "ponytail": 6, "bunches": 7, "neat": 9}.get(style, 6)
        for y in range(y0 + 3, y0 + length):
            for x in range(4, 8 if y < y0 + 9 else 7):
                p.set(x, y, h[1] if x == 4 else h[2])
        p.set(12, y0 + 3, h[1])
        if style == "ponytail":
            for y in range(y0 + 3, y0 + 13):
                p.set(3, y, h[2])
                p.set(2, y + 1, h[1]) if y > y0 + 5 else None
        if style == "bun":
            for y in range(y0 - 2, y0 + 1):
                for x in (3, 4, 5):
                    p.set(x, y, h[2])
        if style == "bunches":
            for y in range(y0 + 2, y0 + 8):
                p.set(3, y, h[2])
                p.set(2, y, h[1])


def beard(p, r, view, y0, spec):
    if not spec.get("beard"):
        return
    b = r[spec.get("beard_colour", "hair")]
    if view == DOWN:
        full = spec["beard"] in ("full", "long")
        for y in range(y0 + 7, y0 + 11):
            for x in range(4 if full else 6, 12 if full else 10):
                if not (y == y0 + 8 and 7 <= x <= 8):
                    p.set(x, y, b[2] if x < 8 else b[1])
        if spec["beard"] == "moustache":
            for y in range(y0 + 7, y0 + 11):
                for x in range(3, 13):
                    if p.get(x, y)[:3] == b[2][:3] or p.get(x, y)[:3] == b[1][:3]:
                        p.set(x, y, r["skin"][2])
            for x in range(5, 11):
                p.set(x, y0 + 7, b[2])
        if spec["beard"] == "long":
            # A sage's beard: full at the jaw, then tapering to a point over the chest.
            for i, (a, c) in enumerate([(4, 11), (5, 10), (5, 10), (6, 9), (6, 9), (7, 8)]):
                for x in range(a, c + 1):
                    p.set(x, y0 + 11 + i, b[2] if x < 8 else b[1])
            p.set(6, y0 + 9, b[3])
    elif view == RIGHT:
        rows = range(y0 + 7, y0 + 11) if spec["beard"] in ("full", "long") else range(y0 + 7, y0 + 8)
        for y in rows:
            for x in range(8, 13):
                p.set(x, y, b[2] if spec["beard"] in ("full", "long") or x > 9 else None)
        if spec["beard"] == "long":
            for i, (a, c) in enumerate([(8, 12), (8, 11), (9, 11), (9, 10), (9, 10)]):
                for x in range(a, c + 1):
                    p.set(x, y0 + 11 + i, b[2] if x < 11 else b[1])


# ── Clothing overlays ──

def coat(p, r, view, frame, spec, y0, hem):
    """A long coat (or robe) over the torso, down to the hem row; open at the front for coats."""
    c = r["coat"]
    open_front = spec.get("coat_kind") == "open"
    sway = {0: 0, 1: 1, 2: 0, 3: -1}[frame] if view in (RIGHT, LEFT) else 0
    if view in (DOWN, UP):
        for y in range(y0, hem + 1):
            a, b = (4, 11) if y < y0 + 9 else (3, 12)
            if y == y0:
                a, b = 5, 10
            for x in range(a, b + 1):
                if view == DOWN and open_front and 7 <= x <= 8 and y < y0 + 9:
                    continue
                if view == DOWN and open_front and 7 <= x <= 8:
                    continue
                p.set(x, y, c[3] if x == a else c[1] if x == b else c[2])
    else:
        for y in range(y0, hem + 1):
            a, b = (5, 10) if y < y0 + 9 else (4 + (sway if y > y0 + 11 else 0), 11 + (sway if y > y0 + 11 else 0))
            for x in range(a, b + 1):
                p.set(x, y, c[3] if x == a else c[1] if x == b else c[2])


def apron(p, r, view, y0):
    a = r["apron"]
    if view == DOWN:
        for y in range(y0 + 1, y0 + 13):
            x0, x1 = (6, 9) if y < y0 + 4 else (5, 10)
            for x in range(x0, x1 + 1):
                p.set(x, y, a[3] if x == x0 else a[1] if x == x1 else a[2])
        p.set(5, y0, a[1])
        p.set(10, y0, a[1])
    elif view == RIGHT:
        for y in range(y0 + 1, y0 + 13):
            p.set(10, y, a[2])
            p.set(11, y, a[1])
    else:
        p.set(6, y0, a[1])
        p.set(9, y0, a[1])
        for x in range(4, 12):
            p.set(x, y0 + 7, a[1])
        p.set(7, y0 + 8, a[2])
        p.set(8, y0 + 8, a[2])


def strap(p, r, view, y0, key):
    """A diagonal strap (satchel, bandolier) with little jars on it."""
    s = r[key]
    if view == DOWN:
        for i in range(8):
            p.set(4 + i, y0 + i, s[1])
        for i in (2, 5):
            p.set(4 + i, y0 + i - 1, (200, 230, 240, 255))
    elif view == UP:
        for i in range(8):
            p.set(11 - i, y0 + i, s[1])
    else:
        p.set(8, y0, s[1])
        p.set(9, y0 + 1, s[1])


def scarf(p, r, view, y0):
    s = r["scarf"]
    if view in (DOWN, UP):
        for x in range(4, 12):
            p.set(x, y0, s[2] if x < 9 else s[1])
        if view == DOWN:
            p.set(9, y0 + 1, s[2])
            p.set(9, y0 + 2, s[1])
    else:
        for x in range(5, 11):
            p.set(x, y0, s[2])
        p.set(4, y0 + 1, s[1])


def patches(p, view, y0, colours):
    if view == UP:
        return
    spots = [(5, y0 + 3), (10, y0 + 6), (6, y0 + 10)] if view == DOWN else [(6, y0 + 4), (9, y0 + 9)]
    for (x, y), c in zip(spots, colours):
        if p.get(x, y)[3]:
            p.set(x, y, c)
            if p.get(x + 1, y)[3]:
                p.set(x + 1, y, K.darker(c, 0.2))


def stars(p, view, y0, hem):
    if view == UP:
        return
    for x, y in [(5, y0 + 2), (10, y0 + 5), (4, y0 + 10), (11, hem - 1), (6, hem - 2)]:
        if p.get(x, y)[3] and not (view == DOWN and 7 <= x <= 8):
            p.set(x, y, "#F8E070")


# ── Hats and eyewear ──

def hat(p, r, view, y0, spec):
    kind = spec.get("hat_kind")
    if not kind:
        return
    if kind in ("widebrim", "sunhat"):
        c = r["hat"]
        brim_y = y0 + 1
        for x in range(1, 15):
            p.set(x, brim_y, c[2] if x < 9 else c[1])
        crown = (4, 11) if view in (DOWN, UP) else (5, 11)
        for y in range(y0 - 3, brim_y):
            for x in range(crown[0], crown[1] + 1):
                p.set(x, y, c[3] if x < 7 else c[2])
        band = r.get("band")
        if band:
            for x in range(crown[0], crown[1] + 1):
                p.set(x, brim_y - 1, band[2])
        if spec.get("feather") and view != UP:
            fx = 11 if view != LEFT else 4
            for i, (dx, dy) in enumerate([(0, -3), (1, -4), (1, -5), (2, -6)]):
                p.set(fx + dx, y0 + dy, r["feather"][3 if i % 2 else 2])
    elif kind == "wrap":
        c = r["hat"]
        for y in range(y0 - 2, y0 + 4):
            a, b = (4, 11) if y < y0 else (3, 12)
            for x in range(a, b + 1):
                p.set(x, y, c[3] if (x + y) % 4 == 0 else c[2] if x < 9 else c[1])
        if view in (UP, RIGHT):
            p.set(4, y0 + 4, c[1])
            p.set(4, y0 + 5, c[1])
            p.set(3, y0 + 5, c[2])
    elif kind == "goggles":
        band = r["goggle_band"]
        for x in range(3, 13):
            p.set(x, y0 + 1, band[1])
        if view == DOWN:
            for gx in (5, 9):
                p.set(gx, y0, "#D2A64E")
                p.set(gx + 1, y0, "#D2A64E")
                p.set(gx, y0 + 1, "#9AD8F0")
                p.set(gx + 1, y0 + 1, "#E8F8FF")
        elif view == RIGHT:
            p.set(10, y0, "#D2A64E")
            p.set(11, y0, "#D2A64E")
            p.set(10, y0 + 1, "#9AD8F0")
            p.set(11, y0 + 1, "#E8F8FF")


def glasses(p, view, y0, colour=None):
    frame = K.rgba(colour) if colour else (60, 50, 50, 255)
    if view == DOWN:
        for x in (4, 6, 9, 11):
            p.set(x, y0 + 5, frame)
        for x in (5, 10):
            p.set(x, y0 + 4, frame)
        p.set(7, y0 + 5, frame)
        p.set(8, y0 + 5, frame)
    elif view == RIGHT:
        p.set(10, y0 + 5, frame)
        p.set(12, y0 + 5, frame)
        p.set(11, y0 + 4, frame)


# ── Assembly ──

def frame(spec, view, f):
    r = ramps(spec)
    p = Painter()
    teen = spec.get("teen")
    mirror = view == LEFT
    v = RIGHT if mirror else view
    ghost = spec.get("ghost")
    bob = (0 if f in (0, 2) else -1) if not ghost else [0, -1, -1, 0][f]
    head_y = 3 + (2 if teen else 0) + bob
    torso_y = head_y + 12
    leg_top = torso_y + 9
    if ghost:
        _ghost_tail(p, r, v, f, torso_y)
    else:
        legs(p, r, v, f, spec, leg_top)
    arms(p, r, v, f, spec, torso_y, front=False)
    torso(p, r, v, f, spec, torso_y)
    if spec.get("coat"):
        coat(p, r, v, f, spec, torso_y, spec.get("hem", 26) + (head_y - 3))
        if spec.get("stars"):
            stars(p, v, torso_y, spec.get("hem", 26) + (head_y - 3))
    if spec.get("patches"):
        patches(p, v, torso_y, spec["patches"])
    if spec.get("apron"):
        apron(p, r, v, torso_y)
    if spec.get("strap"):
        strap(p, r, v, torso_y, spec["strap"])
    arms(p, r, v, f, spec, torso_y, front=True)
    head(p, r, v, head_y)
    if v != UP:
        face(p, r, v, head_y, spec)
    beard(p, r, v, head_y, spec)
    hair(p, r, v, head_y, spec)
    if spec.get("scarf"):
        scarf(p, r, v, torso_y)
    if spec.get("glasses") and v != UP:
        glasses(p, v, head_y, spec.get("glasses_colour"))
    hat(p, r, v, head_y, spec)
    # A ghost's edge is a cold blue rather than the warm near-black everyone else gets, so he reads as lit from within.
    edge = (46, 62, 112, 255) if ghost else (36, 24, 30, 255)
    img = K.outline(p.img, lambda c: K.rgba(K.mix(K.outline_of(c), edge, 0.5)))
    if mirror:
        img = K.flip(img)
    if ghost:
        img = _translucent(img)
    return img


def _ghost_tail(p, r, view, f, y0):
    """The Curator has no legs: his coat thins to a wisp that sways."""
    c = r["coat"]
    sway = [0, 1, 0, -1][f]
    widths = [(4, 11), (4, 11), (5, 10), (5, 10), (6, 9), (6, 9), (7, 9), (7, 8)]
    for i, (a, b) in enumerate(widths):
        y = y0 + 9 + i
        dx = sway if i > 3 else 0
        for x in range(a + dx, b + dx + 1):
            p.set(x, y, c[2] if i < 4 else c[3])


def _translucent(img, alpha=0.82):
    out = img.copy()
    px = out.load()
    for y in range(out.height):
        for x in range(out.width):
            r, g, b, a = px[x, y]
            if a:
                fade = alpha * (1 - max(0, y - 24) * 0.08)
                px[x, y] = (r, g, b, int(a * fade))
    return out


def walk_sheet(spec):
    sheet = Image.new("RGBA", (W * 4, H * 4), (0, 0, 0, 0))
    for view in (DOWN, RIGHT, UP, LEFT):
        for f in range(4):
            sheet.alpha_composite(frame(spec, view, f), (f * W, view * H))
    return sheet


# Per NPC, from the commission brief (doc 21): skin, hair, clothes and props.
LOOKS = {
    "Oriel": dict(skin="fair", hair="#3A2440", hair_style="bun", top="#5B3A8C", coat="#4A2E74", bottom="#3C2A5A", hem=29,
                  rolled=True, strap="strapc", strapc="#7A4A2A", belt="sash", sash="#D2A64E"),
    "Tobin": dict(skin="tan", hair="#6A4426", hair_style="short", beard="full", top="#3E4656", bottom="#2E3440", rolled=True,
                  apron="#7A5232", hat_kind="goggles", goggle_band="#5A3A22", belt="beltc", beltc="#3A2618"),
    "Finch": dict(skin="light", hair="#9A4420", hair_style="bob", beard="moustache", top="#E3C21E", coat="#C2542D",
                  bottom="#3B5E3A", hem=25, scarf="#E3C21E", hat_kind="widebrim", hat="#3B5E3A", band="#C2542D", feather="#E04A3A",
                  patches=["#3B5E3A", "#E3C21E", "#5A8ACA"], blush=True),
    "Sorenson": dict(skin="fair", hair="#3A2A22", hair_style="neat", top="#4A5560", coat="#ECECEC", coat_kind="open",
                     bottom="#4A5560", hem=26, glasses=True),
    "Hesper": dict(skin="dark", hair="#1A1420", hair_style="curly", top="#3A4A80", coat="#2A3A6E", bottom="#1C2340", hem=27,
                   stars=True),
    "Ivo": dict(skin="tan", hair="#3A3030", hair_style="short", beard="full", top="#5A4A38", bottom="#2A2A2A",
                strap="strapc", strapc="#3A2A1E", belt="beltc", beltc="#2A1E16"),
    "Delta": dict(skin="tan", hair="#D8B060", hair_style="ponytail", top="#3A8FD8", bottom="#E0D7B0", shins=True,
                  hat_kind="sunhat", hat="#E8DCB4", band="#3A8FD8", strap="strapc", strapc="#8A6A4A", rolled=True),
    "Agnes": dict(skin="light", hair="#A8A8B0", hair_style="bun", top="#5A5A5A", coat="#7A7A7A", bottom="#3A3A3A", hem=25,
                  glasses=True, belt="beltc", beltc="#3A3A3A"),
    "Bennet": dict(skin="brown", hair="#2A2020", hair_style="short", top="#E0A040", bottom="#8A5A2B", hat_kind="wrap", hat="#ECE2C4",
                   rolled=True, belt="sash", sash="#C2542D"),
    "Curator": dict(skin="ghost", hair="#B8C4DA", hair_style="short", beard="long", top="#D8E0EE", coat="#2E5A6E",
                    bottom="#2E5A6E", hem=24, glasses=True, glasses_colour="#D2A64E", ghost=True),
    "Lark": dict(skin="fair", hair="#C8602A", hair_style="bunches", top="#5B8C3A", bottom="#5B8C3A", apron="#E3C21E",
                 hat_kind="goggles", goggle_band="#6A4A2A", teen=True, blush=True),
}


def build(nid):
    return walk_sheet(LOOKS[nid])
