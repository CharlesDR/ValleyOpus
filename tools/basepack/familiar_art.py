"""The four Familiars (sos.3 placeholder art, drawn for ValleyOpus by tools/basepack, after the commission brief's
Familiars sheet): a fire lizard, a water sprite, a mossy stone gnome and a wind wisp, 16x16 each with a four-frame idle
loop. The sheet has one column per Familiar (in sprite-index order, which is how the C# side finds each one) and one row
per frame."""

import math

from PIL import Image

import pixelkit as K

FRAMES = 4
EYE = "#2A1E2A"


def _blank():
    return Image.new("RGBA", (16, 16), (0, 0, 0, 0))


def _blob(inside, colour, light=(-0.6, -0.7)):
    img = _blank()
    px = img.load()
    for y in range(16):
        for x in range(16):
            if inside(x + 0.5, y + 0.5):
                px[x, y] = K.rgba(colour)
    return K.shade_sphere(img, K.ramp(colour, 5), light=light)


def _eyes(px, points, shine=True):
    for x, y in points:
        px[x, y] = K.rgba(EYE)
        if shine and y > 0:
            px[x, y - 1] = K.rgba("#FFFFFF")


def _finish(img, colour):
    return K.outline(img, lambda c: K.rgba(K.mix(K.outline_of(colour), (36, 24, 30, 255), 0.4)))


def salamander(f):
    """A small fire lizard, side on, with a crest and tail of living flame that flicker."""
    body = "#E0442A"
    img = _blob(lambda x, y: ((x - 7) / 4.6) ** 2 + ((y - 10.5) / 2.6) ** 2 <= 1
                or ((x - 3.2) / 2.4) ** 2 + ((y - 9.2) / 2.0) ** 2 <= 1, body)
    px = img.load()
    belly = K.ramp("#F2B04A", 5)
    for x in range(4, 11):
        px[x, 12] = K.rgba(belly[2])
    for x, y in [(4, 13), (5, 13), (9, 13), (10, 13)]:
        px[x, y] = K.rgba(K.darker(body, 0.3))
    # The tail is a tongue of flame: red at the root, yellow at the flickering tip.
    flame = ["#C8302A", "#F07A2A", "#FFB040", "#FFE680"]
    lean = [0, 1, 0, -1][f]
    for y in range(3, 12):
        t = (11 - y) / 8
        cx = 11.5 + t * 2.2 + (lean * t if t > 0.5 else 0)
        half = 1.6 * math.sin(min(1.0, (y - 2.5) / 9) * math.pi) + 0.3
        for x in range(10, 16):
            if abs(x + 0.5 - cx) <= half:
                px[x, y] = K.rgba(flame[min(3, int(t * 3.6))])
    for i, x in enumerate(range(4, 10)):
        h = 1 + (i + f) % 2
        for k in range(h):
            px[x, 7 - k] = K.rgba(flame[3] if k else flame[1])
    _eyes(px, [(2, 9)])
    return _finish(img, body)


def undine(f):
    """A water sprite: a drop with a face, its lower edge rippling."""
    body = "#3A8FD8"
    ripple = [0, 0.6, 0, -0.6][f]

    def inside(x, y):
        if y < 8:
            return abs(x - 8) <= (y - 1.5) * 0.85 and y >= 2
        wave = 13.4 + math.sin(x * 1.3 + f * 1.6) * 0.8
        return ((x - 8) / 5.4) ** 2 + ((y - 9.2) / 4.6) ** 2 <= 1 and y <= wave
    img = _blob(inside, body)
    px = img.load()
    _eyes(px, [(6, 9), (10, 9)])
    px[6, 10] = px[10, 10] = K.rgba(EYE)
    px[8, 11] = K.rgba(K.darker(body, 0.4))
    # Shimmer moving across the surface.
    for k, (x, y) in enumerate([(5, 6), (6, 5), (4, 8)]):
        if (k + f) % 3:
            px[x, y] = K.rgba("#E8F8FF")
    drops = [(2, 13 + round(ripple)), (13, 12 - round(ripple))]
    for x, y in drops:
        px[x, y] = K.rgba(K.lighter(body, 0.4))
    return _finish(img, body)


def gnome(f):
    """A stout earth spirit: a rounded stone with a moss cap, a face and stubby feet, bobbing on the spot."""
    body = "#8A7A6A"
    squash = [0, 0.5, 0, -0.3][f]
    img = _blob(lambda x, y: ((x - 8) / (5.2 + squash * 0.4)) ** 2 + ((y - 9.5 - squash * 0.5) / (4.6 - squash * 0.5)) ** 2 <= 1,
                body)
    px = img.load()
    moss = K.ramp("#5AA84A", 5)
    top = min(y for y in range(16) for x in range(16) if px[x, y][3])
    for x in range(4, 13):
        for y in range(top, top + 3):
            if px[x, y][3] and (y < top + 2 or (x + f) % 3):
                px[x, y] = K.rgba(moss[3] if x < 9 else moss[2])
    px[7, top - 1 if top else 0] = K.rgba(moss[4])
    px[8, top - 1 if top else 0] = K.rgba(moss[3])
    _eyes(px, [(6, 10), (10, 10)])
    for x, y in [(5, 8), (11, 12), (9, 13)]:
        px[x, y] = K.rgba(K.darker(body, 0.35))
    for x in (5, 6, 10, 11):
        px[x, 14] = K.rgba("#6A4A2E")
    return _finish(img, body)


def sylph(f):
    """A wisp of wind: a pale head with streamers trailing and fluttering behind it."""
    body = "#CFE8F0"
    img = _finish(_blob(lambda x, y: math.hypot(x - 6.5, y - 6.5) <= 3.6, body), "#7AA8C0")
    px = img.load()
    for x, y in [(5, 6), (5, 7), (8, 6), (8, 7)]:
        px[x, y] = K.rgba("#2A3A5A")
    streamer = K.ramp("#9AD0E8", 5)
    for s, (y0, amp) in enumerate([(5, 1.0), (8, 1.4), (11, 0.8)]):
        for i, x in enumerate(range(10, 16 - s)):
            y = round(y0 + math.sin(i * 0.9 + f * 1.57 + s) * amp * (i / 5))
            if 0 <= y < 16 and not px[x, y][3]:
                px[x, y] = K.rgba(streamer[4] if i < 2 else streamer[3], 255 - i * 28)
    for x, y in [(2, 3), (12, 2), (3, 12)]:
        if (x + y + f) % 2:
            px[x, y] = K.rgba("#FFFFFF", 200)
    return img


DRAW = {"Salamander": salamander, "Undine": undine, "Gnome": gnome, "Sylph": sylph}


def build(familiars, out_path):
    order = [fid for fid, row in sorted(familiars.items(), key=lambda kv: kv[1][-1])]
    out = Image.new("RGBA", (16 * len(order), 16 * FRAMES), (0, 0, 0, 0))
    for col, fid in enumerate(order):
        for f in range(FRAMES):
            out.alpha_composite(DRAW[fid](f), (col * 16, f * 16))
    out.save(out_path)
