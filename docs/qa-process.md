# QA validation process

> **Status: draft.** This describes the process agreed on so far. It has not
> yet been cross-checked against the live ClickUp "Product Reviewing" list
> (statuses, custom fields) — see [Open points](#open-points). Nothing here
> is written to Confluence or ClickUp until that check happens and the
> developer approves it.

## Role

Anne-Charlotte validates QA on pushed work: for each task moved to
**Product Reviewing** on ClickUp, she checks that the implementation matches
what the task asked for and follows project documentation, then records the
result — filing a ticket when something doesn't match.

## When

**This is a pre-merge gate, not a retroactive review.** A PR does not get
merged until Anne-Charlotte has validated it — either with a "Conforms"
comment on the ClickUp task, or after any subtask/linked task raised below
has been resolved and re-checked. The PR's human-reviewer requirement in
`CLAUDE.md` ("every change goes through a PR with at least one human
reviewer") is satisfied by this QA validation for tasks that go through
Product Reviewing.

## What gets checked, per push

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
   Anne-Charlotte opens the branch in Unity `6000.3.21f1` herself and
   re-verifies (project compiles, no new Console errors, EditMode/
   PlayMode tests pass) before recording the outcome.

## Outcome, per push

| Result | Action |
|---|---|
| Conforms | Comment on the ClickUp task recording the validation |
| Incomplete (task requirements not fully met) | Subtask under the task, explaining what's missing |
| Discrepancy / bug (doesn't respect the request or the documentation) | New ClickUp task, linked to the original task and the PR, assigned to the developer |

## Open points

- Confirm this against the actual ClickUp "Product Reviewing" list:
  available statuses, custom fields, and whether "comment for pass /
  subtask for incomplete / linked task for discrepancy" matches how the
  team already works.
- Since this is a merge gate, confirm how "not yet validated" is made
  visible/enforced on the PR and the ClickUp task (e.g. a status, a label,
  branch protection) so a merge cannot slip through before her comment.
- This draft was written without an active Atlassian/ClickUp MCP
  connection in the authoring session — nothing here has been checked
  against the current Confluence documentation structure either.
- Once confirmed, decide whether this process itself becomes a Confluence
  page (durable, team-facing) with this file kept only as the local
  pointer, per `CLAUDE.md`'s Confluence-vs-ClickUp split.
