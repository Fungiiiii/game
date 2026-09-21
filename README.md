# Fungiiiii

**Unity 6.3 LTS · C# (.NET) · 3D · Blender for 3D assets**

Unity project. See [CLAUDE.md](./CLAUDE.md) for the development rules, and the
[project documentation](https://fungiiiii.atlassian.net/wiki/spaces/docs/overview?homepageId=950451)
on Atlassian.

## Setup — run once per clone

```bash
./scripts/setup.sh
```

This enables the git hooks, Git LFS, and Unity's scene/prefab merge tool. Git
never installs hooks on clone and never versions `.git/config` — by design, so
that cloning a repository cannot execute code chosen by its author. There is no
way to automate this away. The script reports what succeeded; re-run it after
installing anything it flagged as missing.

Skipping it is not a way around the rules: the same checks run in CI
(`.github/workflows/conventions.yml`), where `--no-verify` has no effect. The
local hooks only exist to give you the same answer faster.

## Starting the Unity project

The Unity project does not exist yet. When creating it, follow
[`docs/unity-init.md`](./docs/unity-init.md) — it covers the decisions that need
an ADR first, the settings that must be right before the first commit, and the
verification checklist.

## Conventions

- **Code is English.** Identifiers, comments, tests, log messages, asset names,
  branches, commits, PRs.
- **ADRs are French** and live in [`docs/adrs/`](./docs/adrs). Start from
  [`0000-template.md`](./docs/adrs/0000-template.md).
- Commits follow [Conventional Commits](https://www.conventionalcommits.org/).
- **No player-facing string is hardcoded.** Everything the player reads goes
  through the localization system.
- Never work directly on `main`.
- Linting catches bugs only — see [`docs/linting.md`](./docs/linting.md).
- 3D assets ship as exported `.fbx`. `.blend` sources never enter this
  repository — see [`docs/art-pipeline.md`](./docs/art-pipeline.md).
