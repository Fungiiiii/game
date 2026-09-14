# CLAUDE.md — Fungiiiii

Mandatory development rules for any AI agent or developer working on Fungiiiii, a **Unity project written in C#**. Applies repo-wide unless a more specific `CLAUDE.md` exists in a subdirectory.

---

# 1. Core principles

Priority order: correctness/security/data integrity → existing architectural decisions & contracts → Unity project stability → KISS → YAGNI → SOLID → DRY (when it genuinely helps) → performance (when justified by evidence).

Methodology: XP — small incremental changes, continuous integration, automated testing, simple design, frequent refactoring, fast feedback.

Prefer: composition over inheritance; explicit dependencies over hidden ones; plain C# classes over `MonoBehaviour` when Unity lifecycle isn't needed; small focused components over managers; data-driven config over hardcoded values.

- No abstractions/generic systems/extension points for hypothetical future needs — a little duplication beats a premature abstraction.
- No unrelated refactoring while doing a feature/fix. Boy Scout Rule applies only to code directly touched, and only if it doesn't grow scope.

---

# 2. Source of truth

Order: current task requirements → accepted ADRs → official Atlassian docs → existing game design/gameplay contracts → repo conventions → existing Unity project structure → this document → general Unity/C# conventions.

Docs: https://fungiiiii.atlassian.net/wiki/spaces/docs/overview?homepageId=950451 — consult via the Atlassian MCP when needed.

- Never invent verifiable project/gameplay/architecture rules.
- ADR beats conflicting docs unless explicitly superseded.
- If sources conflict, report the conflict rather than silently picking one.

---

# 3. Mandatory workflow

**Before editing:** understand the request → locate relevant Unity systems → identify affected scenes/prefabs/ScriptableObjects/assemblies → read relevant Atlassian docs → search related ADRs → search similar existing implementations → identify dependencies/side effects → decide if an ADR is needed → find the smallest implementation that satisfies the requirement.

**During:** smallest coherent change, following existing architecture; preserve serialization; avoid unnecessary scene/prefab edits and speculative abstractions; add/update tests; keep docs in sync; create/update ADR if needed.

**After:** compile the project → check Console for errors → run relevant EditMode/PlayMode tests → run lint/static analysis if configured → review full git diff including `.meta`/scene/prefab changes and unintended serialization → check docs and ADR needs → prepare PR.

Never claim a task is complete if required checks weren't run — state explicitly what wasn't tested and why.

---

# 4. Unity version

`ProjectSettings/ProjectVersion.txt` is authoritative. No upgrading as part of an unrelated task or without explicit authorization. An upgrade needs its own branch/PR, package/scene/prefab/build validation, full test run, docs, and an ADR if architecturally impactful.

---

# 5. Unity project files

Version-controlled: `Assets/`, `Packages/`, `ProjectSettings/`. Never commit `Library/`, `Temp/`, `Obj/`, `Logs/`, build outputs, or IDE temp files.

- Don't remove `.meta` files unless the asset itself is intentionally deleted — GUIDs are contracts.
- Investigate any unexpected `.meta` change; never regenerate GUIDs without a specific reason.

---

# 6. Unity serialization

Scenes, prefabs, ScriptableObjects, materials, animations, controllers, timelines and serialized components can produce large/unintended YAML diffs. Before committing: inspect the diff, confirm only intended fields changed, check prefab overrides/references/missing scripts/broken GUIDs. Don't save a scene/prefab just because Unity marked it dirty.

---

# 7. Serialized fields

Prefer `[SerializeField] private float movementSpeed;` over making a field public just for Inspector access — public fields should be an actual API. When renaming a serialized field, keep compatibility with `[FormerlySerializedAs("oldName")]` if scenes/prefabs/SOs depend on it. Don't rename casually.

---

# 8. MonoBehaviour responsibilities

`MonoBehaviour` is for Unity integration (lifecycle, GameObject/component access, Inspector refs, callbacks, scene interaction) — not a dumping ground for business logic.

```text
MonoBehaviour → Plain C# domain/gameplay logic → Data / services
```

Avoid God MonoBehaviours; keep each component focused.

---

# 9. Unity lifecycle

Use callbacks (`Awake`, `OnEnable`, `Start`, `Update`, `FixedUpdate`, `LateUpdate`, `OnDisable`, `OnDestroy`) intentionally. `Awake` for internal init; `Start` when depending on others' `Awake`. Subscribe/unsubscribe symmetrically:

```csharp
private void OnEnable()  { service.OnSomething += HandleSomething; }
private void OnDisable() { service.OnSomething -= HandleSomething; }
```

Don't assume lifecycle order between unrelated objects or rely on Script Execution Order unless unavoidable — document it, and consider an ADR if significant.

---

# 10. Update loops

Don't add `Update()` by default — consider event/timer/coroutine/animation/physics-driven alternatives first. Avoid expensive work in `Update`/`FixedUpdate`/`LateUpdate`: `FindObjectOfType`, `GameObject.Find`, repeated `GetComponent`, repeated allocations, unnecessary LINQ/string concat/collection creation. Cache references.

---

# 11. Physics

Physics-timestep-dependent work goes in `FixedUpdate`. Prefer Rigidbody-based movement; don't directly manipulate transforms of dynamic Rigidbodies unless intentional. Changes to collision detection, Rigidbody settings, layers, physics materials or fixed timestep are potentially gameplay-impacting.

---

# 12. Movement systems

Before touching movement, be explicit about: input source, ownership, physics vs non-physics, authoritative position, rotation, collisions, animation coupling, multiplayer implications. Significant movement architecture needs an ADR; don't duplicate movement logic across components without a documented reason.

---

# 13. Input

Use the project's existing input architecture (continue using the Input System package if already adopted) — don't mix architectures. Prefer actions (`Move`, `Interact`, `Jump`) over raw key checks, for rebinding/controllers/accessibility/platform support/testability.

---

# 14. ScriptableObjects

Good for configuration, immutable/shared data, item/character/ability definitions, balancing, content. Not a global mutable-state mechanism — watch for Play Mode edits unintentionally persisting. Runtime state belongs in runtime objects unless architecture says otherwise.

---

# 15. Prefabs

Prefer reusable prefabs over duplicated hierarchies. Before editing, know whether it's a prefab asset, nested prefab, variant, or scene instance. Avoid blindly applying all scene-instance overrides — verify each is intentional.

---

# 16. Scenes

Avoid scenes becoming implicit dependency containers or excessive hard-coded cross-scene refs. Before modifying: check ownership, additive loading, dependencies, bootstrap behavior, persistent objects. Don't duplicate global systems across scenes or add `DontDestroyOnLoad` casually — persistent systems need clear ownership/lifecycle.

---

# 17. Scene loading

Transitions must be explicit: loading/unloading state, persistent objects, async loading, player/save state, transition UI, failure handling. Don't assume a specific active scene unless guaranteed. Large scene-management changes need an ADR.

---

# 18. GameObject lookup

Avoid `GameObject.Find(...)` or repeated `FindObjectOfType<T>()` as a dependency mechanism. Prefer serialized references, initialization methods, factories, DI, or established service architecture. Global lookup is acceptable for editor tools/bootstrapping only.

---

# 19. GetComponent

Fine to use, but cache instead of repeating every frame:

```csharp
private Rigidbody body;
private void Awake() { body = GetComponent<Rigidbody>(); }
```

Use `TryGetComponent` when absence is expected. Use `[RequireComponent]` for strict dependencies — don't overuse it.

---

# 20. Null handling in Unity

`UnityEngine.Object` has special null semantics — a destroyed object can compare equal to `null` while its managed wrapper still exists. Be careful mixing Unity objects with nullable references, interfaces, and generic containers; don't assume normal C# null semantics apply.

---

# 21. Architecture

Prefer clear layering when no other documented architecture exists:

```text
Presentation / Unity Components → Application / Gameplay → Domain → Infrastructure
```

The existing accepted architecture always takes precedence. Keep gameplay logic testable independently of scenes when reasonable.

---

# 22. Composition over inheritance

Prefer composition (`Player` with `Movement`/`Health`/`Interaction`/`Inventory`) over deep inheritance chains (`Entity → LivingEntity → Character → ControllableCharacter → Player`). Inheritance is fine for a genuine "is-a" relationship with shared behavior — not just to reuse a few methods.

---

# 23. Managers and singletons

Don't default to a new `Manager` or `public static Something Instance`. Singletons bring hidden dependencies, lifecycle issues, scene coupling, testing pain, init-order problems — require architectural justification, documented ownership/lifecycle, and an ADR if significant.

---

# 24. Static state

Avoid mutable static state for gameplay — it behaves unpredictably with Domain Reload settings, Play Mode options, tests, and scene reloads. Don't assume Play Mode always resets static fields.

---

# 25. Events

Events should reduce coupling, not hide control flow. Always consider publisher/subscriber lifetime, unsubscribe behavior, duplicate subscriptions, scene transitions. Don't build a global event bus for every interaction — prefer direct communication for simple local relationships.

---

# 26. Coroutines

Use for Unity-specific async sequences, not as a substitute for architecture. Each coroutine needs clear ownership, cancellation, and object lifetime. They stop when their object is destroyed — don't assume continuation across scene destruction.

---

# 27. Async / Await

Only when compatible with existing project architecture. Respect Unity's main-thread restriction — never touch Unity objects from worker threads. Handle cancellation, destroyed owners, scene transitions, exceptions. Don't introduce a second async framework if one is already established.

---

# 28. Instantiate / Destroy

Avoid excessive `Instantiate`/`Destroy` in hot paths (projectiles, enemies, VFX, temporary UI) — pool when profiling or frequency clearly warrants it. Use the project's existing pooling solution rather than building a generic one prematurely.

---

# 29. Performance

Don't optimize blindly — profile first (Unity Profiler) and look at CPU, GC allocations, rendering, physics, memory, asset loading. Watch for allocation sources: LINQ, closures, string creation, collection creation, boxing, repeated temp arrays/lists. Readable code wins outside hot paths.

---

# 30. LINQ

Fine where readability matters and performance doesn't. Avoid in frequently executed gameplay loops where allocation/CPU cost matters. Not banned globally — use judgment based on execution frequency.

---

# 31. Rendering

Don't casually touch URP/HDRP, render pipeline assets, shaders, post-processing, lighting, or quality settings — these can affect the whole project. Requires explicit review of affected platforms/scenes; major decisions need an ADR.

---

# 32. Materials

`renderer.material` can instantiate a material — don't do this accidentally every frame. Use `sharedMaterial` vs instance APIs deliberately depending on whether shared or per-instance behavior is needed.

---

# 33. Assets

Before adding an asset, check ownership, license, source, import settings, platform/memory impact. Don't commit unnecessary or duplicated source files. Preserve `.meta` files if assets are moved programmatically or outside Unity.

---

# 34. Asset loading

Use the project's established loading system. Don't introduce `Resources.Load` as a general architecture unless approved; continue using Addressables if already adopted. Don't mix loading systems without justification — significant changes need an ADR.

---

# 35. Editor code

Editor-only code belongs in `Editor/` folders or an Editor `.asmdef`. Never reference `UnityEditor` from runtime assemblies — runtime builds must not depend on Editor-only APIs.

---

# 36. Assembly Definitions

Follow existing `.asmdef` structure and explicit assembly boundaries. No circular dependencies; runtime must not depend on Editor assemblies. Don't create new asmdefs without clear reason — verify dependency direction is architecturally correct.

---

# 37. Namespaces

Follow existing project namespaces; don't add new code to the global namespace if the project uses them. Namespaces should reflect architecture/domain, not arbitrary folder depth. Don't rename large namespace trees as part of unrelated work.

---

# 38. C# conventions

Use only language features supported by the project's Unity/C# version. Prefer clear names, explicit ownership, small methods, immutable state and `readonly` where practical, early returns for readability. Avoid clever code that hurts readability.

---

# 39. Naming

Descriptive names — avoid `Manager2`, `DataStuff`, `Helper`, `Utils`, `Thing` unless genuinely warranted. Intention-revealing booleans (`isGrounded`, `canMove`, `hasTarget`); action-describing methods (`Move()`, `ApplyDamage()`, `OpenInventory()`).

---

# 40. Magic values

Avoid unexplained gameplay magic numbers — prefer serialized config, constants, ScriptableObjects, or documented settings. Don't build config systems for values that are genuinely local constants.

---

# 41. Testing

Use Unity Test Framework.

- **EditMode** — plain C#/domain/utility logic, deterministic systems, ScriptableObject logic without runtime behavior. Prefer this when Unity runtime isn't required (faster, simpler).
- **PlayMode** — GameObjects, MonoBehaviours, lifecycle callbacks, physics, scenes, runtime behavior.

---

# 42. Regression tests

Every bug fix should include a regression test when reasonably possible — failing before the fix, passing after. Don't modify tests just to make new code pass unless requirements genuinely changed.

---

# 43. Testability

Don't make systems unnecessarily dependent on scenes, static state, singletons, hard-coded GameObject searches, or Unity lifecycle when plain C# would simplify testing. Don't distort simple production code just to satisfy tests — balance with KISS.

---

# 44. Determinism

Systems using randomness should support deterministic testing — avoid hard-wiring random generation when seeded/injected randomness would simplify testing/replay. Use the project's randomness architecture if one exists.

---

# 45. Time

Regular MonoBehaviour gameplay can use Unity time APIs directly. Only abstract away from `Time.deltaTime` when there's a concrete deterministic-simulation/testing requirement — YAGNI otherwise.

---

# 46. Packages

Before adding a package: confirm no equivalent exists, check Unity-version compatibility, evaluate maintenance/platform support/license, and justify the need. Significant packages need an ADR. Don't upgrade unrelated packages as part of another feature; `manifest.json`/`packages-lock.json` changes must be intentional.

---

# 47. ProjectSettings

Treat changes carefully, especially Input, Physics, Time, Tags/Layers, Quality, Graphics, Player, package config, Script Execution Order. Don't touch unrelated settings — always inspect the diff before committing.

---

# 48. Layers and tags

Don't create/rename casually — they're referenced by physics, scenes, prefabs, scripts, cameras, raycasts. Changes need impact analysis; prefer existing project-defined constants/abstractions.

---

# 49. Animator

Avoid hardcoded parameter strings scattered around — prefer hashed/centralized identifiers:

```csharp
private static readonly int SpeedHash = Animator.StringToHash("Speed");
```

Don't over-couple to the Animator without reason.

---

# 50. UI

Follow the project's existing UI tech — don't mix UI Toolkit and uGUI arbitrarily. Keep gameplay state separate from UI representation; UI shouldn't be the source of truth for gameplay state or tightly coupled to domain logic.

---

# 51. Save systems

Save data is a persistent contract. Changes must consider backward compatibility, migration, corrupted saves, missing fields, versioning. Never break existing saves without explicit authorization; significant format changes need an ADR.

---

# 52. Multiplayer / networking

Always determine authority, ownership, client/server responsibilities, replication, prediction, validation. Never trust client-provided data for authoritative decisions. Don't assume single-player logic works unmodified in multiplayer — network architecture changes need an ADR.

---

# 53. Architecture Decision Records — ADR

Create one for meaningful architectural decisions: movement, scene, save-system, networking, input, DI strategy, global services, asset-loading strategy, Addressables adoption, rendering pipeline, physics architecture, async architecture, pooling, large dependency adoption. Skip ADRs for trivial details.

Required sections: **Title**, **Status** (Proposed/Accepted/Deprecated/Superseded), **Context**, **Assumptions**, **Constraints**, **Alternatives considered** (pros/cons/risks each), **Decision**, **Rationale**, **Dependencies** (why needed, why existing functionality is insufficient, maintenance/Unity/platform compatibility), **Consequences** (positive, negative, tech debt, migration, editor/runtime impact), **References** (ticket/PR/docs/related ADRs).

ADRs live in the Atlassian ADR section. Never silently replace an accepted decision — supersede it with a new ADR.

---

# 54. Atlassian documentation

Atlassian is the official doc source — read/update via the Atlassian MCP. Don't duplicate it in the repo. Suggested hierarchy (adapt to what already exists, don't duplicate sections): Project Overview; Game Design (Gameplay, Characters, Mechanics, Progression); Architecture (Game/Scene Architecture, Systems, Data Model, Save System, Input, Networking, Asset Management); ADR; Unity (Project Structure, Coding Conventions, Prefabs, Scenes, ScriptableObjects, Testing, Performance); Development (Getting Started, Git Workflow, Local Development, Debugging); Build & Release (Build Process, Environments, Platforms); Templates (ADR, Feature, Bug, Technical Documentation, Postmortem).

---

# 55. Documentation maintenance

Update docs when changes affect architecture, gameplay behavior, controls, scenes, save data, APIs, config, packages, build process, or workflows. Documentation is part of the task — code and docs must never knowingly contradict each other.

---

# 56. Git workflow

Never work directly on the default/main branch. One branch per coherent change: `feat/`, `fix/`, `refactor/`, `docs/`, `test/`, `chore/` + `<ticket>-<description>` (e.g. `feat/FUN-123-player-movement`). Keep branches focused.

---

# 57. Commits

Conventional Commits: `type(scope): description` — types: `feat`, `fix`, `refactor`, `test`, `docs`, `chore`, `perf`, `build`, `ci`. E.g. `feat(movement): add player sprint`, `fix(physics): prevent player clipping through ramps`. Never credit Claude/AI as author or add `Co-authored-by: Claude` — use the developer's configured Git identity.

---

# 58. Unity Git diffs

Inspect `.unity`, `.prefab`, `.asset`, `.controller`, `.anim`, `.mat`, `.meta`, `ProjectSettings`, `Packages` before committing. Investigate unexpected YAML changes; don't commit mass reserialization caused by opening the project in a different Unity version.

---

# 59. Pull Requests

Every change goes through a PR — no direct pushes to protected branches, at least one human reviewer required, Claude must never approve its own PR.

**Template:** Summary / Why / Implementation / Unity impact (scenes, prefabs, ScriptableObjects, ProjectSettings, packages, builds) / Testing (EditMode, PlayMode, manual) / Documentation / ADR (link, or `ADR not required — <reason>`) / Risks.

**Checklist:** scope limited · project compiles · no new Console errors · EditMode/PlayMode tests pass · manual validation done where applicable · no broken prefab refs or missing scripts · scene/prefab/`.meta`/ProjectSettings/package diffs reviewed · docs updated · ADR created/updated if needed · no secrets · no unrelated changes · human reviewer assigned.

---

# 60. Definition of Done

Implementation complete; project compiles; no new Console errors; tests exist and pass (EditMode/PlayMode as relevant); manually validated where automation is insufficient; prefab/scene references valid; serialization and `.meta` changes reviewed; docs and ADR updated as needed; no unrelated modifications; PR ready for review. State explicitly what couldn't be validated and why.

---

# 61. Security

Never commit passwords, tokens, API keys, certificates, credentials, or secrets — including in ScriptableObjects, prefabs, scenes, logs, docs, or test fixtures. Client builds must never contain secrets that need to stay confidential — assume anything shipped can be inspected.

---

# 62. Destructive operations

Never do these without explicit human authorization: force push, deleting unmerged branches, destructive resets, deleting major assets/scenes/save data, dropping databases, changing production config, deploying production builds, rotating credentials, rewriting published history. Prefer reversible operations.

---

# 63. Scope control

Only modify what the task requires — no unrelated refactors, scene/asset resaves or renames, package/Unity upgrades, ProjectSettings changes, or new architecture unless actually needed. Report unrelated problems separately instead of fixing them inline.

---

# 64. Final self-review

Before declaring done, review the whole diff against: does this solve the actual problem, is it the simplest reasonable implementation, is Unity lifecycle ownership clear, any unnecessary MonoBehaviour/global state/Update loop, could plain C# have been used, are dependencies explicit, are serialized references and GUIDs safe, any accidental scene/prefab changes or unintended overrides, any missing references, are tests sufficient, are performance/allocation implications acceptable, is documentation in sync, is an ADR needed, any unnecessary package/ProjectSettings change, any unrelated code in the diff. Fix what you find before calling it done.

---

# 65. Agent behavior

Claude must: inspect before modifying, verify rather than guess, search existing implementations first, understand Unity object lifecycle, preserve serialized data and GUIDs, make minimal changes, reuse existing systems over parallel ones, keep code simple, explain meaningful architectural decisions, use MCP tools for authoritative project info.

Claude must never claim: tests passed without running them, the project compiles without compiling it, gameplay was validated without testing it, docs were updated when they weren't, an ADR exists without checking, a PR was created when it wasn't, or Atlassian docs were consulted when they weren't.

When uncertain about a potentially destructive Unity change, preserve the existing asset/config and report the uncertainty.

---

# 66. AGENTS.md

`AGENTS.md` must contain only: `[CLAUDE.md](./CLAUDE.md)` — no duplicated content.
