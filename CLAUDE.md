# CLAUDE.md — Fungiiiii

Fungiiiii is a 3D game: **Unity 6.3 LTS · C# (.NET) · 3D assets authored in Blender**. These rules apply repo-wide unless a more specific `CLAUDE.md` exists in a subdirectory.

This file holds only what applies to *every* task. Domain rules live in skills and load when they become relevant — see [Where the rest lives](#where-the-rest-lives).

---

## Source of truth

Order: current task requirements → accepted ADRs → official Atlassian docs → existing game design/gameplay contracts → repo conventions → existing Unity project structure → this file → general Unity/C# conventions.

Docs: https://fungiiiii.atlassian.net/wiki/spaces/docs/overview?homepageId=950451

Read and write project documentation through the Atlassian MCP. **If the Atlassian MCP is not connected, say so and stop — do not substitute assumptions for documentation you could not read.**

`ProjectSettings/ProjectVersion.txt` is authoritative for the Unity version. Never upgrade Unity or unrelated packages as part of another task.

**There is no Unity project in the repository yet.** If the task is to create it, follow `docs/unity-init.md` — order matters, and several steps are far harder to fix after the first commit. Blender sources live in `ArtSource/`, outside `Assets/`; see `docs/art-pipeline.md`.

Never invent project, gameplay or architecture rules that could be verified. An accepted ADR beats conflicting documentation unless explicitly superseded. If authoritative sources conflict, report the conflict instead of silently picking one.

---

## Priorities

correctness / security / data integrity → existing architectural decisions and contracts → Unity project stability → KISS → YAGNI → SOLID → DRY (when it genuinely helps) → performance (only with profiling evidence).

Methodology is XP: small incremental changes, continuous integration, automated testing, simple design, frequent refactoring, fast feedback.

Prefer composition over inheritance; explicit dependencies over hidden ones; plain C# classes over `MonoBehaviour` when the Unity lifecycle is not required; small focused components over managers; data-driven config over hardcoded values.

No abstraction, generic system or extension point for a hypothetical future need. A little duplication beats a premature abstraction.

---

## Root cause before fix

A quick fix is a signal, not a solution. If the patch is a null check that silences a crash, a frame delay that makes a race go away, a magic offset that makes a collision look right, a `try/catch` around a symptom, or a `[DefaultExecutionOrder]` that reorders the problem away — **the real defect is almost certainly somewhere else**. A workaround that works is evidence the cause has not been found yet, not evidence the task is done.

Before patching: reproduce the problem, then explain *why* the wrong value, state or timing exists. If that explanation cannot be given, the fix is a guess.

If a workaround must ship anyway, say so explicitly in the PR: what the suspected real cause is, what the patch masks, and what remains to be investigated. Never leave that implicit in the diff.

---

## Scope

Only modify what the task requires. No unrelated refactoring, asset resave or rename, package or Unity upgrade, ProjectSettings change, or new architecture.

The Boy Scout Rule applies only to code the task already touches, and only when it does not grow scope. Report unrelated problems separately instead of fixing them inline.

---

## Language

**All code and repository content is written in English** — identifiers, comments, XML docs, test names, log and exception messages, Unity asset/scene/prefab/ScriptableObject names, branch names, commit messages, PR descriptions, and the Markdown in this repository. This holds for throwaway and temporary code too.

**ADRs are the single exception: they are written in French** and live in `docs/adrs/`. See the `adr` skill.

**No player-facing string is ever hardcoded.** Everything the player reads or hears goes through the localization system — UI, dialogue, item names, player-facing errors. Log and exception messages are engineering artifacts and stay English. See the `unity-localization` skill; adopting the Localization package requires an ADR, because it also adopts Addressables.

---

## Git

Never work directly on `main`. One branch per coherent change:

`feat|fix|refactor|docs|test|chore/<ticket>-<description>` — e.g. `feat/FUN-123-player-movement`

Conventional Commits: `type(scope): description`, types `feat` `fix` `refactor` `test` `docs` `chore` `perf` `build` `ci`.

**Never credit Claude or any AI.** No `Co-authored-by: Claude`, no AI mention in commit messages or PR descriptions. Use the developer's configured Git identity.

Every change goes through a PR with at least one human reviewer. Claude must never approve its own PR.

---

## Never without explicit human authorization

Force push · delete unmerged branches · destructive reset · delete assets, scenes or save data · drop databases · change production configuration · deploy production builds · rotate credentials · rewrite published history.

Prefer reversible operations. When uncertain about a potentially destructive Unity change, preserve the existing asset/configuration and report the uncertainty.

Never commit passwords, tokens, API keys, certificates or credentials — including in ScriptableObjects, prefabs, scenes, logs, docs or test fixtures. Assume anything shipped in a client build can be inspected.

---

## Honesty about verification

**Unity is not installed in this environment** — the project cannot be compiled, and EditMode/PlayMode tests cannot be run locally. Compilation and tests are verified by CI (see `.github/workflows/`) and by the developer.

Never claim that tests passed, the project compiles, gameplay was validated, documentation was updated, an ADR exists, a PR was created, or Atlassian was consulted — unless it actually happened. Always state explicitly what was not verified and why.

---

## Where the rest lives

Load the matching skill *before* starting that kind of work.

| Skill | Use when |
|---|---|
| `unity-serialization` | Touching `.unity`, `.prefab`, `.asset`, `.meta`, materials, animations, controllers — or reviewing a Unity diff |
| `unity-runtime-code` | Writing or editing runtime C#: MonoBehaviours, lifecycle, physics, input, events, performance |
| `unity-testing` | Writing EditMode/PlayMode tests, regression tests, deterministic systems |
| `unity-localization` | Any text or asset the player sees or hears; building UI |
| `unity-project-config` | Touching `ProjectSettings/`, `Packages/`, layers, tags, assemblies, asset loading, rendering |
| `adr` | Making or superseding an architectural decision (ADRs live in `docs/adrs/`, in French) |
| `atlassian-docs` | Reading or updating project documentation |
| `ship-it` | Finishing a task: self-review, Definition of Done, PR |

Enforced mechanically, not by prose — do not re-implement these by hand:

- `.gitignore` / `.gitattributes` — generated directories excluded; Unity YAML routed to `UnityYAMLMerge`; binary assets to Git LFS
- `scripts/setup.sh` — per-clone setup (hooks, LFS, merge tool); see `docs/unity-init.md`
- `.claude/hooks/` — blocks work on `main`, destructive git commands, and writes into generated directories
- `.githooks/commit-msg` — validates Conventional Commits
- `.github/pull_request_template.md` — the PR template is inserted by GitHub
- `.editorconfig` + `docs/linting.md` — Unity analyzers, limited to rules that catch bugs; style stays advisory
- `.githooks/pre-commit` + `.githooks/commit-msg` — local fast feedback; both delegate to `scripts/`
- `scripts/check-localization.sh` — flags hardcoded player-facing text once the Localization package is adopted
- `.github/workflows/conventions.yml` — runs the same `scripts/` checks in CI, where they cannot be skipped with `--no-verify`
