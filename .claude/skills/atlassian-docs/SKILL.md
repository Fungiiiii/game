---
name: atlassian-docs
description: How to read and update Fungiiiii project documentation on Atlassian Confluence, including the expected space hierarchy and when documentation updates are part of a task. Feature docs are not covered here - they live in docs/features/. Use when a task needs project, gameplay or architecture information, or when a change affects behavior that is documented.
---

# Atlassian documentation

Space: https://fungiiiii.atlassian.net/wiki/spaces/docs/overview?homepageId=950451

Atlassian is the official documentation source. Read and write it through the Atlassian MCP.

**If the Atlassian MCP is not connected, say so: the work goes on, the guessing does not.** Nothing a page you could not read would say is assumed — what depends on it waits. The documentation update is made once the MCP is connected again. Never claim documentation was consulted when it was not. See `CLAUDE.md`, "Source of truth".

Confluence holds the general project documentation — game design, architecture overview, specifications. The exact description of each shipped feature lives in `docs/features/` in the repository. Do not duplicate one into the other — each has exactly one home; link instead.

## When documentation must be updated

Update the docs when a change affects: architecture · gameplay behavior · controls · scenes · save data · APIs · configuration · packages · build process · asset workflow · development workflow.

Documentation is part of the task, not a follow-up. Code and documentation must never knowingly contradict each other.

## Space hierarchy

The space is in French. Its entry point is the page **🍄 Fungiiiii! · Documentation V2**, whose summary lists the reference documents. V1 pages it marks as replaced are obsolete — never cite them. Update an existing document rather than adding a page next to it.

- **01 · Cadrage et Pilotage** — Note de Cadrage · Plan de Gestion de Projet · WBS et OBS · Charte d'Équipe · Analyse des Risques
- **02 · Conception du Jeu** — Document de Game Design (GDD) · Spécifications Fonctionnelles (SFD) · Périmètre et MVP · Inventaire des Assets
- **03 · Technique** — Spécifications Techniques (STD): stack, network architecture, data, asset pipeline, conventions, CI/CD, performance, security
- **04 · Go-to-Market** — Étude de Marché · Plan Marketing et Contenu
- **05 · Cadre et Rendus** — Charte IA · Contrat de Rendu · Formulaire Alpha Fermée

Gameplay rules live in the GDD and the SFD; technical choices in the STD.

## Domains not yet decided

The following have **no accepted architecture yet** and no existing implementation in this repository. Do not assume one, and do not implement one without a design note validated by the owners:

- Save system — save data is a persistent contract. Any format will need backward compatibility, migration, corrupted-save handling, missing fields and versioning. Never break existing saves without explicit authorization.
- Multiplayer / networking — if introduced, authority, ownership, client/server responsibilities, replication, prediction and validation must all be determined. Never trust client-provided data for authoritative decisions, and never assume single-player logic works unmodified in multiplayer.
