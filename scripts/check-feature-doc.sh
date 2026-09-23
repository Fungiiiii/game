#!/usr/bin/env bash
# Enforces docs/features/README.md: a feat/ branch ships its feature doc, and
# a doc that modifies another one is linked both ways.
#
# Usage:
#   scripts/check-feature-doc.sh <branch> <base-ref>
#   e.g. scripts/check-feature-doc.sh feat/12487v29zj3-mouvement-ia origin/develop
set -uo pipefail

branch="${1:?branch name required}"
base="${2:?base ref required}"
dir="docs/features"
failed=0

fail() { echo "  $1" >&2; failed=1; }

# 1. A feat/ branch must carry docs/features/<suffix>.md.
if [[ "$branch" == feat/* ]]; then
  expected="$dir/${branch#feat/}.md"
  if [ ! -f "$expected" ]; then
    fail "$expected is missing. A feat/ branch ships its feature doc:
      cp $dir/_template.md $expected
    then fill it in — example: $dir/86c9mnqxa-champimaison.md"
  fi
fi

# 2. Every feature doc added or changed by the branch declares its previous
#    docs, and each one links back.
mapfile -t docs < <(git diff --name-only --diff-filter=AM "$base...HEAD" -- "$dir/*.md" \
  | grep -vE "^$dir/(README|_template)\.md$")

for doc in "${docs[@]}"; do
  [ -f "$doc" ] || continue
  name=$(basename "$doc")

  # Lines of the "Docs précédents" section, HTML comments and blanks removed.
  section=$(awk '
    /^## Docs précédents/ { on = 1; next }
    /^## /                { on = 0 }
    on' "$doc" | sed '/<!--/,/-->/d' | grep -v '^[[:space:]]*$')

  if [ -z "$section" ]; then
    fail "$doc — section \"## Docs précédents\" is empty.
    Search for existing docs of this feature first:
      grep -ril \"<keyword>\" $dir/
    Then, under \"## Docs précédents\", either link each doc this branch modifies:
      - [<id>-<description>](./<id>-<description>.md) — what changes
    or, if nothing exists yet, write the single line:
      Aucun."
    continue
  fi

  mapfile -t previous < <(grep -oE '\]\(\./[^)]+\.md\)' <<<"$section" | sed -E 's/^\]\(\.\/(.*)\)$/\1/')
  for prev in "${previous[@]}"; do
    target="$dir/$prev"
    if [ ! -f "$target" ]; then
      fail "$doc — links $target, which does not exist. Check the file name in docs/features/."
    elif ! grep -q "$name" "$target"; then
      fail "$target — must link back to the doc that modifies it. Add under its \"## Modifié par\":
      - [${name%.md}](./$name) — what changed"
    fi
  done
done

if [ $failed -ne 0 ]; then
  echo "" >&2
  echo "Feature doc check failed — see docs/features/README.md" >&2
fi
exit $failed
