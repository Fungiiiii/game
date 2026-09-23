#!/usr/bin/env bash
# Run once per clone.
#
# Git never versions .git/config and never installs hooks on clone — by design,
# so that cloning a repository cannot execute code chosen by its author.
# Everything configured here is therefore per-clone and has to be opted into.
#
# Skipping the hooks is not a way around the rules: the same checks run in CI
# (.github/workflows/conventions.yml), where --no-verify has no effect.
set -uo pipefail

cd "$(git rev-parse --show-toplevel)"

# --- Git hooks -------------------------------------------------------------
git config core.hooksPath .githooks
chmod +x .githooks/* scripts/*.sh 2>/dev/null
echo "  hooks        core.hooksPath = $(git config core.hooksPath)"

# --- Git LFS ---------------------------------------------------------------
if command -v git-lfs >/dev/null 2>&1; then
  git lfs install --local >/dev/null
  echo "  lfs          enabled"
else
  echo "  lfs          NOT INSTALLED — binary assets will bloat the repository."
  echo "               Install git-lfs, then re-run this script."
fi

# --- jq (Claude Code hooks) -------------------------------------------------
# .claude/hooks/ read their input with jq and refuse every command without it.
if command -v jq >/dev/null 2>&1; then
  echo "  jq           $(jq --version)"
else
  echo "  jq           NOT INSTALLED — Claude Code's guard hooks will block every command."
  echo "               Windows: winget install jqlang.jq — then restart Claude Code."
fi

# --- Unity scene/prefab merge tool -----------------------------------------
# .gitattributes routes Unity YAML through UnityYAMLMerge. Without the tool
# configured here, those merges silently fall back to git's line-based merge,
# which corrupts scenes and prefabs.
find_unityyamlmerge() {
  local candidates=(
    "$HOME/Unity/Hub/Editor"/*/Editor/Data/Tools/UnityYAMLMerge
    "/opt/unity/editors"/*/Editor/Data/Tools/UnityYAMLMerge
    "/Applications/Unity/Hub/Editor"/*/Unity.app/Contents/Tools/UnityYAMLMerge
    "/Applications/Unity/Unity.app/Contents/Tools/UnityYAMLMerge"
    "/c/Program Files/Unity/Hub/Editor"/*/Editor/Data/Tools/UnityYAMLMerge.exe
  )
  for c in "${candidates[@]}"; do
    [ -x "$c" ] && { echo "$c"; return 0; }
  done
  return 1
}

if merge_tool=$(find_unityyamlmerge); then
  git config merge.tool unityyamlmerge
  git config mergetool.unityyamlmerge.trustExitCode false
  git config mergetool.unityyamlmerge.cmd \
    "'$merge_tool' merge -p \"\$BASE\" \"\$REMOTE\" \"\$LOCAL\" \"\$MERGED\""
  echo "  merge tool   $merge_tool"
else
  echo "  merge tool   NOT FOUND — scene and prefab merges will be unreliable."
  echo "               Install Unity, then re-run. See docs/unity-init.md."
fi

echo
echo "Setup complete for this clone."
