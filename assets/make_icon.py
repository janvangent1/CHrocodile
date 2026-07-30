"""Build a multi-size Windows .ico from the crocodile silhouette PNG."""

from __future__ import annotations

import io
import struct
from pathlib import Path

from PIL import Image

ASSETS = Path(__file__).resolve().parent
SRC = ASSETS / "chrocodile_silhouette.png"
OUT = ASSETS / "chrocodile.ico"

ICO_SIZES = (16, 24, 32, 48, 64, 128, 256)
LACOSTE_GREEN = (0, 99, 65)


def _load_silhouette() -> Image.Image:
    img = Image.open(SRC).convert("RGBA")
    pixels = img.load()
    width, height = img.size

    for y in range(height):
        for x in range(width):
            r, g, b, a = pixels[x, y]
            if a < 16:
                continue
            if r > 210 and g > 210 and b > 210:
                pixels[x, y] = (0, 0, 0, 0)
            else:
                pixels[x, y] = (*LACOSTE_GREEN, 255)

    bbox = img.getbbox()
    if bbox:
        img = img.crop(bbox)

    side = max(img.size)
    canvas = Image.new("RGBA", (side, side), (0, 0, 0, 0))
    offset = ((side - img.width) // 2, (side - img.height) // 2)
    canvas.paste(img, offset, img)
    return canvas


def _save_multi_size_ico(path: Path, images: list[Image.Image]) -> None:
    """Write a Vista+ ICO containing PNG-compressed frames (all sizes embedded)."""
    png_frames: list[bytes] = []
    for image in images:
        buf = io.BytesIO()
        image.save(buf, format="PNG", optimize=True)
        png_frames.append(buf.getvalue())

    count = len(png_frames)
    header = struct.pack("<HHH", 0, 1, count)
    entries = bytearray()
    image_data = bytearray()
    offset = 6 + (16 * count)

    for image, png in zip(images, png_frames):
        width, height = image.size
        width_byte = 0 if width >= 256 else width
        height_byte = 0 if height >= 256 else height
        entries.extend(
            struct.pack(
                "<BBBBHHII",
                width_byte,
                height_byte,
                0,  # color count
                0,  # reserved
                1,  # color planes
                32,  # bits per pixel
                len(png),
                offset,
            )
        )
        image_data.extend(png)
        offset += len(png)

    path.write_bytes(header + bytes(entries) + bytes(image_data))


def main() -> None:
    if not SRC.exists():
        raise FileNotFoundError(f"Missing source image: {SRC}")

    silhouette = _load_silhouette()
    icons = [
        silhouette.resize((size, size), Image.Resampling.LANCZOS)
        for size in ICO_SIZES
    ]
    _save_multi_size_ico(OUT, icons)
    print(f"Wrote {OUT} with {len(icons)} embedded sizes: {ICO_SIZES}")


if __name__ == "__main__":
    main()
