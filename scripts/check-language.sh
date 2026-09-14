#!/usr/bin/env bash
# Enforces CLAUDE.md "Language" on source files: code is English.
#
# Usage:
#   scripts/check-language.sh              # staged files (local pre-commit)
#   scripts/check-language.sh <file>...    # explicit list (CI)
#
# Scope is deliberately narrow — source files and filenames only.
# Markdown is excluded: English docs legitimately quote French ADR titles and
# filenames, which no accent heuristic can distinguish from French prose.
# Markdown stays a human review concern.
set -uo pipefail

accents='[àâäçéèêëîïôöùûüÿœæÀÂÄÇÉÈÊËÎÏÔÖÙÛÜŸŒÆ]'
sources='\.(cs|shader|cginc|hlsl|asmdef|asmref|uxml|uss)$'

if [ $# -gt 0 ]; then
  candidates=("$@")
else
  mapfile -t candidates < <(git diff --cached --name-only --diff-filter=ACM)
fi

found=0

for f in "${candidates[@]}"; do
  [[ "$f" == docs/adrs/* ]] && continue
  [ -f "$f" ] || continue

  if [[ "$f" =~ $accents ]]; then
    [ $found -eq 0 ] && echo "" >&2
    echo "  $f — accented characters in the filename" >&2
    found=1
  fi

  [[ "$f" =~ $sources ]] || continue

  hits=$(grep -nE "$accents" -- "$f" 2>/dev/null | head -5)
  if [ -n "$hits" ]; then
    [ $found -eq 0 ] && echo "" >&2
    echo "  $f" >&2
    sed 's/^/      /' <<<"$hits" | cut -c1-120 >&2
    found=1
  fi
done

if [ $found -eq 1 ]; then
  cat >&2 <<'MSG'

CLAUDE.md requires source code to be English.

  Identifiers, comments, XML docs, test names, log and exception messages
  and asset names are all English. docs/adrs/ is the only French content.

  Player-facing text is localization content: it belongs in the localization
  system, never hardcoded in a source file.

  This check is a heuristic. Locally, bypass it with: git commit --no-verify
MSG
  exit 1
fi

exit 0
