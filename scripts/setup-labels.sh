#!/usr/bin/env bash
# Creates or updates the pull request labels described in docs/workflow.md.
# Safe to re-run: every label is created or overwritten to match this list.
#
# Usage:
#   scripts/setup-labels.sh            # create / update the labels below
#   scripts/setup-labels.sh --prune    # also delete every label not listed
#
# Needs the GitHub CLI (gh), authenticated with write access to the repository.
set -uo pipefail

# name|colour|description
# Type labels mirror the commit types of scripts/check-commit-message.sh and
# are applied from the PR title by .github/workflows/labels.yml.
labels=$(cat <<'EOF'
feat|1d76db|Nouvelle fonctionnalité
fix|0e8a16|Correction de bug
art|0e9aa7|Modèles 3D, textures, audio
docs|c5def5|Documentation
refactor|bfd4f2|Restructuration sans changement de comportement
test|a2eeef|Tests
chore|d4e5f7|Maintenance, outillage
ci|bfdadc|Intégration continue
net|006b75|Réseau
perf|0052cc|Performance
build|c2e0c6|Build
unity: scène|f9a03f|La PR modifie une scène Unity — relire le diff de près
unity: prefab|fb8c00|La PR modifie un prefab — vérifier les références
unity: config|e36209|La PR touche ProjectSettings/ ou Packages/
process|8250df|La PR change le process : CLAUDE.md, .claude/, .github/, scripts/
PR empilée|6e7781|La PR vise une autre branche que develop ou main
bloqué|b60205|En attente d'autre chose : asset, autre PR, décision
EOF
)

prune=0
[ "${1:-}" = "--prune" ] && prune=1

existing=$(gh label list --limit 200 --json name --jq '.[].name') || {
  echo "Cannot list labels — is gh installed and authenticated?" >&2
  exit 1
}

# Keep the history of the PRs labelled with GitHub's default "enhancement".
if grep -qxF enhancement <<<"$existing" && ! grep -qxF feat <<<"$existing"; then
  gh label edit enhancement --name feat && echo "  renamed      enhancement -> feat"
fi

failed=0
while IFS='|' read -r name colour description; do
  if gh label create "$name" --color "$colour" --description "$description" --force >/dev/null; then
    echo "  ok           $name"
  else
    echo "  FAILED       $name" >&2
    failed=1
  fi
done <<<"$labels"

if [ $prune -eq 1 ]; then
  wanted=$(cut -d'|' -f1 <<<"$labels")
  while IFS= read -r name; do
    [ -z "$name" ] && continue
    grep -qxF "$name" <<<"$wanted" && continue
    if gh label delete "$name" --yes >/dev/null; then
      echo "  deleted      $name"
    else
      echo "  FAILED       delete $name" >&2
      failed=1
    fi
  done < <(gh label list --limit 200 --json name --jq '.[].name')
fi

exit $failed
