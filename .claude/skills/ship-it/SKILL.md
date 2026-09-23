---
name: ship-it
description: Final review and delivery checklist for this Unity project — the self-review questions to run against the full diff, the Definition of Done, and the pull request template. Use when finishing a task, before committing, and when opening a PR.
---

# Finishing a task

## 1. Self-review the whole diff

Read the complete diff — `git diff` and `git diff --staged`, including `.meta`, scene, prefab, `ProjectSettings` and `Packages` files. Then answer:

- Does this solve the requested problem?
- If this is a bug fix: can I explain *why* the defect happened? A patch I cannot explain is a workaround, and the real cause is elsewhere — see `CLAUDE.md`, "Root cause before fix". If it ships as a workaround anyway, is that stated in the PR?
- Is it the simplest reasonable implementation?
- Is Unity lifecycle ownership clear?
- Did I add an unnecessary `MonoBehaviour`, global state, or `Update` loop?
- Could plain C# have been used instead?
- Are dependencies explicit rather than hidden?
- Are serialized references safe? Did any GUID change unexpectedly?
- Did I accidentally modify a scene or prefab? Are prefab overrides intentional?
- Are there missing references or missing scripts?
- Are the tests sufficient? Does a bug fix have a regression test?
- Are performance and allocation implications acceptable? Does anything allocate every frame?
- Is documentation synchronized? On a `feat/` branch, does `docs/features/<id>-<description>.md` list every behaviour, the decisions taken, and every test — automated and manual?
- Does the feature doc cite, under *Docs précédents*, every older doc this branch changes — and does each of them link back under *Modifié par*? Run `scripts/check-feature-doc.sh <branch> origin/develop`.
- Did I add an unnecessary package or ProjectSettings change?
- Does any player-facing string reach the screen without the localization system?
- Is the code written in English — identifiers, comments, test names, log messages, asset names? Are `docs/` and the PR description in French?
- Is there any unrelated code in the diff?

Fix what you find before going further.

## 2. Definition of Done

- Implementation complete
- Unity project compiles — **not verifiable locally, Unity is not installed here**; state this explicitly
- No new Unity Console errors — same caveat
- Tests exist where relevant; EditMode and PlayMode tests pass where applicable — state which were written but not run
- Manually validated where automated testing is insufficient
- Prefab references valid, scenes valid
- Serialization and `.meta` changes reviewed
- Documentation updated
- Feature doc written (`feat/`, or `fix/` changing documented behaviour), older docs linked both ways
- Branch rebased on `origin/develop`, and the tests above run **after** the rebase
- Manual tests of the feature doc and `docs/playbook-tests.md` replayed by the developer
- No unrelated modifications
- PR targets `develop` and is ready for a reviewer, who tests it in Unity before approving

**If something could not be validated, say what and why.** Never claim a check ran when it did not.

## 3. Pull request

Every change goes through a PR to `develop` — only Ilyes opens PRs from `develop` to `main`. No direct pushes to `develop` or `main`. At least one approval from another developer, who tests the branch in Unity; the approving reviewer merges. Claude never approves or merges its own PR, and never credits an AI as author.

The description is written **in French**.

`.github/pull_request_template.md` is inserted automatically by GitHub — fill it in rather than rewriting it:

**Résumé** · **Ticket** (ClickUp task and closed validation subtask) · **Doc de la feature** · **Implémentation** · **Impact Unity** (scenes, prefabs, ScriptableObjects, ProjectSettings, packages) · **Tests** (automated, manual, playbook — and what was not run) · **Risques** · **Checklist**
