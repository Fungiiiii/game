---
name: adr
description: How and when to write an Architecture Decision Record for this project. ADRs live in docs/adrs/ and are written in French — the only French content in an otherwise English-only repository. Use when making, changing or superseding an architectural decision — movement, scenes, save system, networking, input, dependency injection, global services, asset loading, Addressables, rendering pipeline, physics, async architecture, pooling, or adopting a large dependency.
---

# Architecture Decision Records

ADRs live in **`docs/adrs/`** in this repository and are **written in French**. They are the only exception to the English-only rule in `CLAUDE.md`.

> This supersedes the earlier decision to host ADRs in the Atlassian ADR section. Atlassian holds the project documentation; the repository holds the ADRs. See `docs/adrs/0001-adr-en-francais-dans-le-repo.md`.

## When an ADR is required

Movement architecture · scene architecture · save system architecture · networking technology · input architecture · dependency injection strategy · global services and singletons · asset-loading strategy · Addressables introduction · rendering pipeline choice · physics architecture · async architecture · pooling architecture · adoption of a large package or dependency.

Do **not** write an ADR for trivial implementation details.

## Before proposing anything architectural

Search the existing ADRs first — the decision may already exist:

```bash
ls docs/adrs/ && grep -ril "<sujet>" docs/adrs/
```

## File naming

`docs/adrs/NNNN-titre-court-en-kebab-case.md`

- `NNNN` — 4 digits, zero-padded, strictly incrementing, never reused.
- The slug is in French, like the content.
- Example: `docs/adrs/0007-architecture-de-deplacement-du-joueur.md`

Start from `docs/adrs/0000-template.md`.

## Status values

`Proposé` · `Accepté` · `Déprécié` · `Remplacé`

## Superseding

Never silently replace an accepted decision. Write a new ADR, set the old one to `Remplacé`, and link both ways:

- in the old ADR: `Remplacé par [ADR-0012](./0012-....md)`
- in the new ADR: `Remplace [ADR-0007](./0007-....md)`

An accepted ADR beats conflicting documentation unless explicitly superseded.

## Required sections

Every ADR contains all of these, in French — see `docs/adrs/0000-template.md` for the exact skeleton.

| Section | Content |
|---|---|
| `# ADR-NNNN — Titre` | Concise description of the decision |
| `## Statut` | One of the four values above, plus supersede links |
| `## Contexte` | The problem, and why it needs deciding now |
| `## Hypothèses` | What is taken as true, and would invalidate the decision if false |
| `## Contraintes` | Technical, gameplay and product constraints — including Unity version and target platforms |
| `## Alternatives envisagées` | For each: advantages, disadvantages, risks. At least one real alternative; "ne rien faire" counts |
| `## Décision` | The selected solution, stated unambiguously |
| `## Justification` | Why this alternative won, against the constraints above |
| `## Dépendances` | For each new dependency: why required, why existing functionality is insufficient, maintenance implications, Unity and platform compatibility |
| `## Conséquences` | Positive · negative · accepted technical debt · migration impact · editor workflow impact · runtime impact |
| `## Références` | Ticket · PR · documentation · related and superseded ADRs |

Reference an ADR from a PR as `ADR-0007`, with a relative link.
