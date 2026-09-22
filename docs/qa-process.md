# QA validation process

> **Status: draft.** Checked against the live ClickUp list on 2026-09-21
> (statuses, custom fields). **Not** checked against Confluence — no
> Atlassian connection was available. Nothing here is written to Confluence
> or ClickUp until the developer approves it — see
> [Open points](#open-points).

## Role

Anne-Charlotte validates QA on pushed work: for each task moved to
**`product reviewing`** on ClickUp, she checks that the implementation
matches what the task asked for and follows project documentation, then
records the result — raising a ticket when something doesn't match.

## Workflow

Tasks live in the ClickUp list "Liste de tâche Fungiiiii". The statuses are:

`a traiter` → `en cours` → `product reviewing` → `Closed` (or `annulé`)

`product reviewing` is a status of that list, not a list of its own. The
list has no custom fields.

**Only the project manager moves a task to `Closed`.** The QA validation is
an input to that decision; it does not close anything itself.

## When

**Validation happens while the task is in `product reviewing`, before the
project manager closes it.** It is not tied to the PR merge: the code is
often already merged when the task reaches review, and that is fine. A
defect found on merged code is fixed by follow-up work (see
[Outcome](#outcome-per-task)), not by blocking a merge.

QA validation does **not** replace the human PR review required by
`CLAUDE.md` ("every change goes through a PR with at least one human
reviewer"). The two are separate: the PR review happens on the PR, the QA
validation happens on the ClickUp task.

## What gets checked, per task

1. **Traceability** — the PR links the ClickUp task; the task has a clear
   description / acceptance criteria.
2. **Scope conformity** — the implementation matches what the task asked
   for, no more and no less (see `CLAUDE.md`, "Scope": no unrelated
   refactors, renames, or new architecture slipped into an unrelated task).
3. **Documentation conformity** — respects the relevant Confluence pages
   (game design / gameplay contracts) and existing ADRs (`docs/adrs/`). For
   a new feature: the design note exists on the ClickUp task, and its
   validation subtask was closed by a manager (see `CLAUDE.md`, "Feature
   design note"). A feature implemented without that note/approval is a
   process deviation to flag, not silently accept.
4. **Execution quality** — the PR checklist from
   `.github/pull_request_template.md` (compilation, EditMode/PlayMode
   tests, no new Console errors, scene/prefab/`.meta` diffs reviewed, no
   hardcoded player-facing text, no unrelated changes, no secrets) is
   honestly filled in, not just ticked. The `ship-it` skill's self-review
   questions are the reference for what "well executed" means on this
   project. Because CI does not run Unity (ADR-0004), compilation and
   test results are **not** taken on the developer's word alone:
   Anne-Charlotte opens the code in Unity `6000.3.21f1` herself (the
   branch, or `main` once merged) and re-verifies (project compiles, no
   new Console errors, EditMode/PlayMode tests pass) before recording the
   outcome.

## Outcome, per task

In every case the task **stays in `product reviewing`** until the project
manager closes it.

| Result | Action |
|---|---|
| Conforms | Comment "Conforms" on the ClickUp task recording the validation. The project manager can then close it |
| Incomplete (task requirements not fully met) | QA comment on the task listing what is missing, **and a subtask under the task**, assigned to the developer, naming exactly what's missing. A completion subtask is not a new feature: it closes out work already scoped by the parent task, so it needs neither a design note nor a manager validation subtask (`CLAUDE.md`, "Feature design note") |
| Discrepancy / bug (doesn't respect the request or the documentation) | QA comment on the task; new ClickUp task linked to the original task and the PR, assigned to the developer |

The QA task
[[QA] Validation des tâches en product reviewing](https://app.clickup.com/t/12487v2aetg)
expects three deliverables: the list of validated tasks, the list of tasks
needing a correction or completion, and QA comments directly in ClickUp.

## Validation log

Dated entries recording what was actually checked and found, per validation
pass. This is the log referred to by "record the result" under
[Role](#role) — the ClickUp comments are the record on each task, this is
the record across a pass.

### 2026-09-22 — Sprint prototypage `[ARCH]` tasks

Scope: the 7 tasks tied to
[[ARCH] Sprint prototypage — note de design](https://app.clickup.com/t/12487v2acwh),
plus [PLAYER] Mouvement joueur.

- **Compilation incident (already fixed).** Four PRs from Pol-Mattis (#4, #5,
  #7, #8) reached `develop` without the project compiling — three analyzer
  errors in `GenericPatrolDemoBootstrap`, `MushroomCaptureDemo` and
  `InventoryDemoController`. Fixed inline in PR #9 (`b8f5a82`) and again as
  its own PR #13 (`fix/unity-null-operators-develop`). Both merged. Verified
  clean on `develop` HEAD (`d018074`) with a batch-mode recompile — no
  `error CS`, clean exit.
- **5 tasks validated Conforms**, technical and manual, each recorded with
  its own ClickUp comment: inventory (#8), HUD (#6), Champimaison (#7),
  mushroom AI movement (#4), mushroom capture (#5).
  - Technical: 68/68 automated tests pass on `develop` (56 EditMode + 12
    PlayMode), 0 failure, 0 exception in the logs.
  - Manual: each prototype scene opened and played in the Editor
    (`6000.3.21f1`) — behaviour matched what its PR described, no new
    Console errors.
  - Minor reporting note, not a defect: each of these 5 PRs reported an
    EditMode/PlayMode count 2/1 tests higher than what's actually in that
    class today. Likely explanation: two project-wide tests
    (`ProjectSerializationSettingsTests`, the `GameObject_SurvivesAFrame`
    PlayMode smoke test) counted against each feature instead of only the
    tests that feature added. Worth a word with the author so future PRs
    report only what they add.
- **[PLAYER] Mouvement joueur (86c9mnqxm) — Incomplete.** The task asks for
  `NetworkTransform` multiplayer sync; PR #9 explicitly ships local-only
  movement ("Networking debt" in its Risks section). QA comment posted, and
  completion subtask
  [[PLAYER] Ajouter NetworkTransform multijoueur au mouvement du joueur](https://app.clickup.com/t/12487v2b0jr)
  created, assigned to Benkerri Ilyes.
- **Prototype poison (12487v29zj5) — not actioned.** Its own design note's
  validation subtask is still open; PR #10 is correctly left unmerged
  pending that approval, disclosed on the task by its author. No QA action
  needed until the note is approved.
- **Process finding, deferred by QA owner's call this cycle.** Four of
  Pol-Mattis's five PRs (#4, #5, #7, #8) were merged into `develop` on
  2026-09-18, 14:41–14:55 UTC — roughly 4h30 after the sprint design note
  was created, and about 46h **before** its validation subtask closed
  (2026-09-20 12:36 UTC). The task descriptions said "Validation en
  attente, ne pas démarrer l'implémentation." This is the process deviation
  `CLAUDE.md`'s "Feature design note" section asks to flag. Anne-Charlotte's
  call: check these tasks for bugs regardless (see above — none found) and
  leave the process question for a separate conversation rather than block
  on it here.

## Open points

- ~~The task only asks for QA comments. Confirm with the team whether the
  subtask (incomplete) and linked task (discrepancy) conventions above are
  wanted, or whether comments alone are enough.~~ **Resolved 2026-09-22** by
  Anne-Charlotte (QA owner): subtask for incomplete, linked task for
  discrepancy, as reflected in the table above. First applied on
  [[PLAYER] Mouvement joueur](https://app.clickup.com/t/86c9mnqxm) (missing
  NetworkTransform). This is a working convention, not an architectural
  decision — no ADR needed.
- How does the project manager know a task has passed QA? Today, only the
  "Conforms" comment. Confirm that is enough, or whether a tag or status is
  wanted.
- Nothing here has been checked against the current Confluence
  documentation structure (no Atlassian MCP connection in the authoring
  session).
- Once confirmed, decide whether this process itself becomes a Confluence
  page (durable, team-facing) with this file kept only as the local
  pointer, per `CLAUDE.md`'s Confluence-vs-ClickUp split.
