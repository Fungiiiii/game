#!/usr/bin/env bash
# Enforces ADR-0005 "Pipeline, nommage et organisation des assets 3D", and the
# PF_ prefix for prefabs from the STD §6, adopted in
# docs/features/12487v2byt9-mystic-forest.md.
#
# Usage:
#   scripts/check-art-assets.sh              # staged files (local pre-commit)
#   scripts/check-art-assets.sh <file>...    # explicit list (CI)
#
# Three rules, all of them cheap to check and expensive to discover late:
#   1. Blender sources never enter this repository.
#   2. Models and prefabs live under a known domain folder.
#   3. Asset names describe the asset, not its state or its version.
#   4. Prefabs are named PF_<Name>: a prefab always carries the name of the
#      model it wraps, and without the prefix the two look identical in pickers.
#
# Naming is checked on the file name only. The object name *inside* the .blend
# matters just as much — it becomes the GameObject name — but no script in this
# repository can see it. That one stays a human check; see docs/art-pipeline.md.
set -uo pipefail

domains='Characters|Environment|Props'
seg='[A-Z][A-Za-z0-9]*'

name_re="^${seg}(_${seg})*(_[0-9]{2})?(@${seg})?$"
fbx_re="^Assets/Art/Models/(${domains})/[^/]+[.]fbx$"
prefab_re="^Assets/Prefabs/(${domains})/[^/]+[.]prefab$"
forbidden_re='^(final|finale|new|old|copy|backup|bak|test|temp|tmp|def|ok|bis|wip|draft|v[0-9]+)$'

if [ $# -gt 0 ]; then
  candidates=("$@")
else
  mapfile -t candidates < <(git diff --cached --name-only --diff-filter=ACM)
fi

found=0

fail() {
  [ $found -eq 0 ] && echo "" >&2
  echo "  $1" >&2
  found=1
}

for f in "${candidates[@]}"; do
  base="${f##*/}"
  [ -z "$base" ] && continue
  [[ "$base" == .* ]] && continue

  case "$base" in
    *.blend|*.blend1|*.blend2)
      fail "$f — Blender source. Sources stay on your machine, never in this repository."
      continue
      ;;
  esac

  # Everything below is about art assets only.
  case "$f" in
    Assets/Art/*|Assets/Prefabs/*) ;;
    *.fbx)
      fail "$f — models live in Assets/Art/Models/<Domain>/"
      continue
      ;;
    *) continue ;;
  esac

  # A .meta describes the asset next to it; validate the underlying name.
  path="${f%.meta}"
  base="${path##*/}"
  [[ "$base" == .* ]] && continue

  if [[ "$base" == *.* ]]; then
    stem="${base%.*}"
    ext="${base##*.}"
  else
    stem="$base"
    ext=""
  fi

  case "$ext" in
    fbx)
      [[ "$path" =~ $fbx_re ]] ||
        fail "$f — models live in Assets/Art/Models/<${domains//|/ | }>/"
      ;;
    prefab)
      [[ "$path" =~ $prefab_re ]] ||
        fail "$f — prefabs live in Assets/Prefabs/<${domains//|/ | }>/"
      [[ "$stem" == PF_* ]] ||
        fail "$f — prefabs are named PF_<Name>, e.g. PF_Tree"
      ;;
  esac

  if ! [[ "$stem" =~ $name_re ]]; then
    fail "$f — expected PascalCase <Name>[_<Variant>][_NN], e.g. Rock_Small_01"
    continue
  fi

  IFS='_@' read -ra segments <<< "$stem"
  for s in "${segments[@]}"; do
    lowered=$(printf '%s' "$s" | tr '[:upper:]' '[:lower:]')
    if [[ "$lowered" =~ $forbidden_re ]]; then
      fail "$f — \"$s\" names a state, not the asset."
    fi
  done
done

if [ $found -eq 1 ]; then
  cat >&2 <<'MSG'

Art assets must follow ADR-0005.

  Sources    .blend files never enter this repository. It holds exported
             models only, and your sources stay local.

  Location   Assets/Art/Models/<Domain>/<Name>.fbx
             Assets/Prefabs/<Domain>/PF_<Name>.prefab
             Domain is Characters, Environment or Props.

  Names      English, ASCII, PascalCase: <Name>[_<Variant>][_NN].
             Rock_Small_01, MushroomCap_Large, TreePine_01.
             Animation clips: <Character>@<Clip>.fbx

             Name what the asset is, never what state it is in. Git already
             records versions, so _final, _v3, _new and _test say nothing
             that survives the week.

  See docs/art-pipeline.md and docs/adrs/0005-pipeline-et-nommage-des-assets-3d.md

  Locally, bypass this check with: git commit --no-verify
MSG
  exit 1
fi

exit 0
