"""Shared pixel-art helpers for the placeholder art (sos.3): colour ramps in the game's warm, hue-shifted style, sprites
drawn from character grids, selective dark outlines, recolouring and sheet packing.

The art these helpers draw is ValleyOpus's own (no third-party pixels), so it ships under the mod's licence. Grids are
lists of strings; each character is a palette key, '.' or ' ' is transparent. A palette maps keys to RGBA tuples.
"""

import colorsys
import math

from PIL import Image

CLEAR = (0, 0, 0, 0)


def hex_rgb(colour):
    colour = colour.lstrip("#")
    return tuple(int(colour[i:i + 2], 16) for i in (0, 2, 4))


def rgba(colour, alpha=255):
    if isinstance(colour, str):
        return hex_rgb(colour) + (alpha,)
    if len(colour) == 3:
        return tuple(colour) + (alpha,)
    return tuple(colour)


def _hls(rgb):
    r, g, b = (c / 255 for c in rgb[:3])
    return colorsys.rgb_to_hls(r, g, b)


def _rgb(h, l, s, a=255):
    r, g, b = colorsys.hls_to_rgb(h % 1.0, max(0.0, min(1.0, l)), max(0.0, min(1.0, s)))
    return (round(r * 255), round(g * 255), round(b * 255), a)


def _toward(h, target, amount):
    """Moves hue h toward target by amount (both 0..1), the short way round."""
    d = ((target - h + 0.5) % 1.0) - 0.5
    step = max(-abs(amount), min(abs(amount), d))
    return (h + step) % 1.0


def ramp(colour, steps=5, spread=1.0, sat=1.0):
    """A dark-to-light ramp around a base colour, with the base in the middle. Shadows lean toward blue-violet and gain
    a little saturation; lights lean toward warm yellow and lose some, as hand-painted pixel art does."""
    h, l, s = _hls(rgba(colour))
    s = min(1.0, s * sat)
    mid = steps // 2
    out = []
    for i in range(steps):
        k = (i - mid) / max(1, mid)  # -1 darkest .. +1 lightest
        if k < 0:
            li = l + k * min(l - 0.08, 0.30 * spread)
            hi = _toward(h, 0.72, 0.045 * -k) if s > 0.08 else h
            si = min(1.0, s * (1 + 0.25 * -k)) if s > 0.08 else s
        elif k > 0:
            li = l + k * min(0.94 - l, 0.24 * spread)
            hi = _toward(h, 0.14, 0.035 * k) if s > 0.08 else h
            si = s * (1 - 0.25 * k)
        else:
            li, hi, si = l, h, s
        out.append(_rgb(hi, li, si))
    return out


def darker(colour, amount=0.35):
    h, l, s = _hls(rgba(colour))
    return _rgb(_toward(h, 0.72, 0.03) if s > 0.08 else h, l * (1 - amount), min(1.0, s * 1.1))


def lighter(colour, amount=0.35):
    h, l, s = _hls(rgba(colour))
    return _rgb(_toward(h, 0.14, 0.02), l + (1 - l) * amount, s * 0.9)


def mix(a, b, t):
    a, b = rgba(a), rgba(b)
    return tuple(round(a[i] + (b[i] - a[i]) * t) for i in range(4))


def outline_of(colour):
    """The outline colour for a material: its darkest ramp step pushed darker, never pure black (the game's style)."""
    h, l, s = _hls(rgba(colour))
    return _rgb(_toward(h, 0.75, 0.05) if s > 0.08 else 0.66, max(0.08, min(0.16, l * 0.32)), min(1.0, max(0.25, s * 0.9)))


def grid(rows, palette, size=None):
    """An image from a character grid. Unknown characters raise, so a typo can't silently vanish."""
    h = len(rows)
    w = max(len(r) for r in rows)
    if size:
        w, h = size
    img = Image.new("RGBA", (w, h), CLEAR)
    px = img.load()
    for y, row in enumerate(rows):
        for x, ch in enumerate(row):
            if ch in ". ":
                continue
            if ch not in palette:
                raise KeyError(f"grid character {ch!r} has no colour (row {y}: {row!r})")
            c = palette[ch]
            if c is not None:
                px[x, y] = rgba(c)
    return img


def outline(img, colour_for=None, diagonal=False, alpha_min=1):
    """Adds a 1 px outline outside the opaque pixels. colour_for(neighbour_rgba) picks each outline pixel's colour
    from the pixel it borders (selective outline); by default the neighbour's outline_of colour."""
    src = img.copy()
    out = img.copy()
    sp, op = src.load(), out.load()
    w, h = img.size
    dirs = [(1, 0), (-1, 0), (0, 1), (0, -1)] + ([(1, 1), (1, -1), (-1, 1), (-1, -1)] if diagonal else [])
    for y in range(h):
        for x in range(w):
            if sp[x, y][3] >= alpha_min:
                continue
            nbr = None
            for dx, dy in dirs:
                nx, ny = x + dx, y + dy
                if 0 <= nx < w and 0 <= ny < h and sp[nx, ny][3] >= alpha_min:
                    nbr = sp[nx, ny]
                    break
            if nbr is not None:
                op[x, y] = (colour_for or outline_of)(nbr)
    return out


def recolour(img, mapping):
    """Swaps exact colours: mapping {rgba or hex: rgba or hex}."""
    table = {rgba(k): rgba(v) for k, v in mapping.items()}
    out = img.copy()
    px = out.load()
    for y in range(out.height):
        for x in range(out.width):
            c = px[x, y]
            if c in table:
                px[x, y] = table[c]
    return out


def tint_luminance(img, colour, keep=lambda c: False, steps=5, spread=1.0):
    """Repaints every pixel by its brightness onto a ramp of colour (keeps alpha). keep(c) leaves a pixel alone."""
    r = ramp(colour, steps, spread)
    out = img.copy()
    px = out.load()
    for y in range(out.height):
        for x in range(out.width):
            c = px[x, y]
            if c[3] == 0 or keep(c):
                continue
            lum = (0.299 * c[0] + 0.587 * c[1] + 0.114 * c[2]) / 255
            i = min(steps - 1, int(lum * steps))
            px[x, y] = r[i][:3] + (c[3],)
    return out


def paste(dst, src, x, y):
    dst.alpha_composite(src, (x, y))


def flip(img):
    return img.transpose(Image.FLIP_LEFT_RIGHT)


def sheet(cells, cell_w, cell_h, columns):
    rows = (len(cells) + columns - 1) // columns
    out = Image.new("RGBA", (cell_w * columns, cell_h * max(1, rows)), CLEAR)
    for i, img in enumerate(cells):
        if img is not None:
            out.alpha_composite(img, ((i % columns) * cell_w, (i // columns) * cell_h))
    return out


def glyph(rows, colour):
    """A small symbol from a grid of '#' (ink) and '.'."""
    return grid(rows, {"#": colour})


def disc(size, colour, cx=None, cy=None, r=None):
    img = Image.new("RGBA", (size, size), CLEAR)
    px = img.load()
    cx = (size - 1) / 2 if cx is None else cx
    cy = (size - 1) / 2 if cy is None else cy
    r = size / 2 if r is None else r
    for y in range(size):
        for x in range(size):
            if (x - cx) ** 2 + (y - cy) ** 2 <= r * r:
                px[x, y] = rgba(colour)
    return img


def shade_sphere(img, ramp_colours, light=(-0.6, -0.7), rim=True):
    """Shades every opaque pixel of a blob as a lit sphere with the given ramp (dark..light); keeps alpha."""
    px = img.load()
    w, h = img.size
    pts = [(x, y) for y in range(h) for x in range(w) if px[x, y][3] > 0]
    if not pts:
        return img
    x0, x1 = min(p[0] for p in pts), max(p[0] for p in pts)
    y0, y1 = min(p[1] for p in pts), max(p[1] for p in pts)
    cx, cy = (x0 + x1) / 2, (y0 + y1) / 2
    rx, ry = max(1, (x1 - x0 + 1) / 2), max(1, (y1 - y0 + 1) / 2)
    lx, ly = light
    ln = math.hypot(lx, ly) or 1
    n = len(ramp_colours)
    for x, y in pts:
        nx, ny = (x - cx) / rx, (y - cy) / ry
        d = min(1.0, math.hypot(nx, ny))
        nz = math.sqrt(max(0.0, 1 - d * d))
        dot = (nx * lx + ny * ly) / ln * 0.75 + nz * 0.55
        t = max(0.0, min(0.999, (dot + 0.35) / 1.2))
        if rim and d > 0.82:
            t = min(t, 0.45)
        c = ramp_colours[int(t * n)]
        px[x, y] = c[:3] + (px[x, y][3],)
    return img
