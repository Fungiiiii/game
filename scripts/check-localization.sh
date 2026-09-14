#!/usr/bin/env bash
# Enforces CLAUDE.md "Language": no player-facing string is hardcoded.
#
# Usage:
#   scripts/check-localization.sh              # staged files (local pre-commit)
#   scripts/check-localization.sh <file>...    # explicit list (CI)
#
# No-op until the Localization package is adopted, so that early UI work is not
# blocked before the ADR exists. Once com.unity.localization is in the manifest,
# this becomes enforcing.
#
# Narrow by design: it flags string literals assigned to a UI text property,
# which is the one pattern that is almost always a missed translation.
# Editor tooling and tests are exempt. Escape hatch on a line: // i18n-exempt
set -uo pipefail

manifest="Packages/manifest.json"
if [ ! -f "$manifest" ] || ! grep -q 'com.unity.localization' "$manifest"; then
  exit 0
fi

# .text = "Some text"   /   .SetText("Some text")
pattern='(\.text\s*=\s*\$?@?"|\.SetText\(\s*\$?@?")[^"]*[A-Za-z]{2,}'

if [ $# -gt 0 ]; then
  candidates=("$@")
else
  mapfile -t candidates < <(git diff --cached --name-only --diff-filter=ACM)
fi

found=0

for f in "${candidates[@]}"; do
  [[ "$f" == *.cs ]] || continue
  [[ "$f" == */Editor/* ]] && continue
  [[ "$f" == *Tests* ]] && continue
  [ -f "$f" ] || continue

  hits=$(grep -nE "$pattern" -- "$f" 2>/dev/null | grep -v 'i18n-exempt' | head -5)
  if [ -n "$hits" ]; then
    [ $found -eq 0 ] && echo "" >&2
    echo "  $f" >&2
    sed 's/^/      /' <<<"$hits" | cut -c1-120 >&2
    found=1
  fi
done

if [ $found -eq 1 ]; then
  cat >&2 <<'MSG'

Hardcoded player-facing text — CLAUDE.md requires every visible string to go
through the localization system.

  Use a LocalizeStringEvent component, or a serialized LocalizedString with
  its StringChanged event. Dynamic values go through Smart String arguments,
  never concatenation. See the unity-localization skill.

  If this string is genuinely not player-facing, mark the line: // i18n-exempt
MSG
  exit 1
fi

exit 0
