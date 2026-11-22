#!/usr/bin/env bash
# Renders the placeholder item icons (SVG, drawn by hand in code with Hugo's go-ahead,
# 22/11) to 256 px PNG sprites in Assets/Art/Textures/Icons/. Temporary: they make way
# for the icons drawn by a human (Charte IA). Needs ImageMagick 7 (`magick`).
set -euo pipefail
here="$(cd "$(dirname "$0")" && pwd)"
out="$here/../../Assets/Art/Textures/Icons"
# On Windows, magick.exe needs native paths, not Git Bash ones.
native() { if command -v cygpath >/dev/null; then cygpath -m "$1"; else printf '%s' "$1"; fi; }
mkdir -p "$out"
for svg in "$here"/*.svg; do
  name="$(basename "$svg" .svg)"
  magick -background none -density 384 "$(native "$svg")" -resize 256x256 "PNG32:$(native "$out")/Icon_${name}.png"
done
