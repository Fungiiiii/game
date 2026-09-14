---
name: atlassian-docs
description: How to read and update Fungiiiii project documentation on Atlassian Confluence, including the expected space hierarchy and when documentation updates are part of a task. ADRs are not covered here - they live in docs/adrs/. Use when a task needs project, gameplay or architecture information, or when a change affects behavior that is documented.
---

# Atlassian documentation

Space: https://fungiiiii.atlassian.net/wiki/spaces/docs/overview?homepageId=950451

Atlassian is the official documentation source. Read and write it through the Atlassian MCP.

**If the Atlassian MCP is not connected, say so and stop.** Do not substitute assumptions for documentation you could not read, and never claim documentation was consulted when it was not.

Do not duplicate Atlassian documentation inside this repository, and do not duplicate ADRs onto Atlassian — each has exactly one home.

## When documentation must be updated

Update the docs when a change affects: architecture · gameplay behavior · controls · scenes · save data · APIs · configuration · packages · build process · asset workflow · development workflow.

Documentation is part of the task, not a follow-up. Code and documentation must never knowingly contradict each other.

## Space hierarchy

Adapt to what already exists — do not duplicate an existing section.

- **Project Overview**
- **Game Design** — Gameplay · Characters · Mechanics · Progression
- **Architecture** — Game Architecture · Scene Architecture · Systems · Data Model · Save System · Input · Networking · Asset Management
- ~~ADR~~ — **ADRs are not on Atlassian.** They live in `docs/adrs/` and are written in French; see the `adr` skill and `docs/adrs/0001-adr-en-francais-dans-le-repo.md`
- **Unity** — Project Structure · Coding Conventions · Prefabs · Scenes · ScriptableObjects · Testing · Performance
- **Development** — Getting Started · Git Workflow · Local Development · Debugging
- **Build & Release** — Build Process · Environments · Platforms
- **Templates** — ADR · Feature · Bug · Technical Documentation · Postmortem

## Domains not yet decided

The following have **no accepted architecture yet** and no existing implementation in this repository. Do not assume one, and do not implement one without an ADR:

- Save system — save data is a persistent contract. Any format will need backward compatibility, migration, corrupted-save handling, missing fields and versioning. Never break existing saves without explicit authorization.
- Multiplayer / networking — if introduced, authority, ownership, client/server responsibilities, replication, prediction and validation must all be determined. Never trust client-provided data for authoritative decisions, and never assume single-player logic works unmodified in multiplayer.
