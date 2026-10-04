---
name: unity-project-config
description: Rules for project-level Unity configuration — ProjectSettings, Packages/manifest.json, adding or upgrading packages, layers and tags, assembly definitions, editor-only code, asset loading (Resources vs Addressables), and rendering configuration (URP, shaders, lighting, quality). Use before changing anything outside Assets/ scripts, or when adding a dependency.
---

# Project configuration

Everything here has repo-wide blast radius. Inspect the diff before committing, and change nothing unrelated to the task.

## Unity version

`ProjectSettings/ProjectVersion.txt` is authoritative. Never upgrade Unity as part of an unrelated task, and never without explicit authorization.

A Unity upgrade requires its own branch and PR, plus validation of packages, scenes, prefabs and builds, a full test run, and documentation — including the *Décisions* section of the feature doc when architecturally impactful.

## ProjectSettings

Especially sensitive: Input, Physics, Time, Tags/Layers, Quality, Graphics, Player, package configuration, Script Execution Order.

Always read the `ProjectSettings/` diff before committing.

## Layers and tags

Never create or rename Unity layers and tags casually — they are referenced by physics, scenes, prefabs, scripts, cameras and raycasts, often by index or string. Changes require impact analysis. Prefer existing project-defined constants or abstractions.

## Packages

Before adding a package: verify no equivalent already exists, verify Unity-version compatibility, evaluate maintenance and platform support, evaluate the license, and justify the need. Significant packages are a decision: validated in the design note, recorded in the *Décisions* section of the feature doc.

Never upgrade unrelated packages as part of another feature. Changes to `Packages/manifest.json` and `Packages/packages-lock.json` must be intentional and reviewed.

## Assembly definitions

Follow the existing `.asmdef` structure and explicit assembly boundaries. No circular dependencies. Runtime assemblies must never depend on Editor assemblies.

Do not create new asmdefs without a clear reason. When adding an assembly dependency, verify the direction is architecturally correct.

## Editor code

Editor-only code belongs in an `Editor/` folder or a dedicated Editor `.asmdef`. Never reference `UnityEditor` from a runtime assembly — runtime builds must not depend on Editor-only APIs.

## Asset loading

Use the loading system already established by the project. Do not introduce `Resources.Load` as a general architecture unless explicitly approved. If Addressables is adopted, continue with it per project conventions.

Do not mix loading systems without justification. Significant asset-loading decisions are validated in the design note and recorded in the *Décisions* section of the feature doc.

## Rendering

Do not casually modify URP settings, render pipeline assets, shaders, post-processing, lighting or quality settings — they affect the entire project across every scene and platform.

Such changes require explicit review of the affected platforms and scenes. Major rendering architecture decisions are validated in the design note and recorded in the *Décisions* section of the feature doc.
