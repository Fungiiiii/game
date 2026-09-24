# Generates the project's shared palette texture and its human-readable guide.
#
# The palette is authoritative here, not in the .png: this file is what makes
# ADR-0005's rule enforceable rather than declarative — a colour never moves,
# you only ever append. Adding a colour means adding an entry below and
# re-running this script. Moving one invalidates the UVs of every model
# already exported.
#
# Run:  python tools/palette/generate_palette.py
# Needs Pillow.  Writes:
#   Assets/Art/Textures/Palette.png   the texture the game uses
#   docs/palette-guide.png            the reference sheet, with UV coordinates
import os

from PIL import Image, ImageDraw, ImageFont

GRID = 8
CELL = 16
FREE_A = "#B0B0B0"
FREE_B = "#A0A0A0"

PALETTE = {
    (0, 0): ("#F2E6CC", "Body light"),
    (0, 1): ("#E0CBA4", "Body base"),
    (0, 2): ("#C2A67C", "Body shadow"),
    (0, 3): ("#8F7550", "Body dark"),

    (1, 0): ("#2B2723", "Eyes"),
    (1, 1): ("#FFFFFF", "White / spots"),
    (1, 2): ("#E59BAF", "Cheeks"),

    (2, 0): ("#D94F3D", "Mushroom red"),
    (2, 1): ("#C9B98E", "Bulb off"),
    (2, 2): ("#FFE9A3", "Bulb on"),
    (2, 3): ("#6E6480", "Enemy"),

    (3, 0): ("#6F9E5C", "Green"),
    (3, 1): ("#3E6438", "Green dark"),
    (3, 2): ("#8A5F42", "Wood"),
    (3, 3): ("#4F3424", "Wood dark"),

    # Rock — ramp A (cool neutral, keeps the original 413D3F as the shadow)
    (4, 0): ("#A49CA0", "Rock light"),
    (4, 1): ("#827A7E", "Rock base"),
    (4, 2): ("#605A5E", "Rock shadow"),
    (4, 3): ("#413D3F", "Rock dark"),

    # Sponge — the value gap is deliberate: it is what makes the holes read at distance
    (5, 0): ("#9CC3DA", "Sponge"),
    (5, 1): ("#35506B", "Sponge holes"),
}

# Rock variants — replace the four hex values on row 4 with one of these ramps:
#   B — warm earthy grey : #A79B8D  #857A6C  #625A50  #443E38
#   C — mossy green-grey : #96A096  #727D74  #525B54  #383F3A


def build_palette(path):
    img = Image.new("RGB", (GRID * CELL, GRID * CELL))
    d = ImageDraw.Draw(img)
    for row in range(GRID):
        for col in range(GRID):
            entry = PALETTE.get((row, col))
            if entry:
                color = entry[0]
            else:
                color = FREE_A if (row + col) % 2 == 0 else FREE_B
            x0, y0 = col * CELL, row * CELL
            d.rectangle([x0, y0, x0 + CELL - 1, y0 + CELL - 1], fill=color)
    img.save(path)
    return img


def build_guide(path, scale=64):
    size = GRID * scale
    img = Image.new("RGB", (size, size + 40), "#FFFFFF")
    d = ImageDraw.Draw(img)
    try:
        font = ImageFont.load_default(size=11)
        small = ImageFont.load_default(size=9)
    except TypeError:
        font = small = ImageFont.load_default()

    for row in range(GRID):
        for col in range(GRID):
            entry = PALETTE.get((row, col))
            color = entry[0] if entry else (FREE_A if (row + col) % 2 == 0 else FREE_B)
            x0, y0 = col * scale, row * scale
            d.rectangle([x0, y0, x0 + scale - 1, y0 + scale - 1], fill=color)
            r, g, b = Image.new("RGB", (1, 1), color).getpixel((0, 0))
            ink = "#000000" if (r * 299 + g * 587 + b * 114) / 1000 > 140 else "#FFFFFF"
            label = entry[1] if entry else "free"
            d.text((x0 + 4, y0 + 4), label, fill=ink, font=font)
            if entry:
                d.text((x0 + 4, y0 + 18), color, fill=ink, font=small)
            u = (col + 0.5) / GRID
            v = 1 - (row + 0.5) / GRID
            d.text((x0 + 4, y0 + scale - 14), f"U {u:.4f}  V {v:.4f}", fill=ink, font=small)

    d.text((6, size + 12), "Fungiiiii - project palette - 128x128 px - Point filtering",
           fill="#000000", font=font)
    img.save(path)


if __name__ == "__main__":
    repo = os.path.dirname(os.path.dirname(os.path.dirname(os.path.abspath(__file__))))
    texture = os.path.join(repo, "Assets", "Art", "Textures", "Palette.png")
    guide = os.path.join(repo, "docs", "palette-guide.png")
    build_palette(texture)
    build_guide(guide)
    print("ok ->", texture)
    print("ok ->", guide)
