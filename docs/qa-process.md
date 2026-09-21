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
| Incomplete (task requirements not fully met) | QA comment on the task listing what is missing; optionally a subtask under the task (to confirm, see below) |
| Discrepancy / bug (doesn't respect the request or the documentation) | QA comment on the task; new ClickUp task linked to the original task and the PR, assigned to the developer (to confirm, see below) |

The QA task
[[QA] Validation des tâches en product reviewing](https://app.clickup.com/t/12487v2aetg)
expects three deliverables: the list of validated tasks, the list of tasks
needing a correction or completion, and QA comments directly in ClickUp.

## Open points

- The task only asks for QA comments. Confirm with the team whether the
  subtask (incomplete) and linked task (discrepancy) conventions above are
  wanted, or whether comments alone are enough.
- How does the project manager know a task has passed QA? Today, only the
  "Conforms" comment. Confirm that is enough, or whether a tag or status is
  wanted.
- Nothing here has been checked against the current Confluence
  documentation structure (no Atlassian MCP connection in the authoring
  session).
- Once confirmed, decide whether this process itself becomes a Confluence
  page (durable, team-facing) with this file kept only as the local
  pointer, per `CLAUDE.md`'s Confluence-vs-ClickUp split.
