---
name: unity-runtime-code
description: Conventions for writing runtime C# in this Unity project — MonoBehaviour responsibilities, lifecycle callbacks, Update loops, physics and movement, input, ScriptableObjects, GameObject lookup, GetComponent, Unity null semantics, events, coroutines, async, pooling, performance, LINQ, materials, Animator, UI, naming and magic values. Use before writing or editing any runtime .cs file.
---

# Runtime C# conventions

## Layering

```text
MonoBehaviour → Plain C# domain/gameplay logic → Data / services
```

`MonoBehaviour` is for Unity integration only: lifecycle, GameObject/component access, Inspector references, Unity callbacks, scene interaction. It is not a home for business logic. Avoid God MonoBehaviours — one focused responsibility per component.

Preferred overall layering when nothing else is documented: Presentation/Unity Components → Application/Gameplay → Domain → Infrastructure. The existing accepted architecture always takes precedence. Keep gameplay logic testable without a scene when reasonable.

Prefer composition (`Player` with `Movement`, `Health`, `Interaction`, `Inventory`) over inheritance chains (`Entity → LivingEntity → Character → ControllableCharacter → Player`). Inheritance is for a genuine "is-a" with shared behavior, not to reuse a few methods.

## Lifecycle

Use `Awake` for internal initialization, `Start` when initialization depends on other objects' `Awake`. Understand why a callback is needed before adding it.

Subscribe and unsubscribe symmetrically:

```csharp
private void OnEnable()  { service.OnSomething += HandleSomething; }
private void OnDisable() { service.OnSomething -= HandleSomething; }
```

Never assume lifecycle order between unrelated objects. Do not rely on Script Execution Order unless unavoidable — document it in the *Décisions* section of the feature doc.

## Update loops

Do not add `Update()` by default. First check whether the behavior can be event-driven, timer-driven, coroutine-based, animation-driven or physics-driven.

Never do this repeatedly in `Update`/`FixedUpdate`/`LateUpdate`: `FindObjectOfType`, `GameObject.Find`, repeated `GetComponent`, repeated allocations, unnecessary LINQ, string concatenation, unnecessary collection creation. Cache references.

## Physics and movement

Physics work that depends on the physics timestep goes in `FixedUpdate`. Prefer Rigidbody-based movement; do not manipulate the transform of a dynamic Rigidbody unless intentional.

Changes to collision detection, Rigidbody settings, layers, physics materials or fixed timestep are gameplay-impacting.

Before implementing or modifying movement, be explicit about: input source, movement ownership, physics vs non-physics, authoritative position, rotation behavior, collision behavior, animation coupling, multiplayer implications. Significant movement architecture is validated in the design note and recorded in the *Décisions* section of the feature doc. Do not duplicate movement logic across components without a documented reason.

## Input

The project uses the Input System package, and the legacy Input Manager is disabled (archived ADR-0003). Do not mix input architectures.

Prefer named actions (`Move`, `Interact`, `Jump`) over raw key checks in gameplay code — this is what enables rebinding, controllers, accessibility, future platforms and automated testing.

## ScriptableObjects

Good for configuration, immutable/shared data, item and character definitions, abilities, balancing and content.

Not a global mutable-state mechanism. Play Mode edits to mutable ScriptableObject values can persist unintentionally. Runtime state belongs in runtime objects unless the architecture explicitly says otherwise.

## Dependencies and lookup

Avoid `GameObject.Find(...)` and repeated `FindObjectOfType<T>()` as a dependency mechanism. Prefer serialized references, initialization methods, factories, dependency injection, or the established service architecture. Global lookup is acceptable for editor tools and bootstrapping only.

Cache `GetComponent`:

```csharp
private Rigidbody body;
private void Awake() { body = GetComponent<Rigidbody>(); }
```

Use `TryGetComponent` when absence is expected. Use `[RequireComponent(typeof(Rigidbody))]` for strict Unity-level dependencies, without overusing it.

## Unity null semantics

`UnityEngine.Object` has special null behavior: a destroyed object can compare equal to `null` while its managed wrapper still exists. Be careful mixing Unity objects with nullable references, interfaces and generic containers. Do not assume normal C# null semantics apply.

## Managers, singletons, static state

Do not create a new `Manager` or `public static Something Instance` by default. Singletons bring hidden dependencies, lifecycle issues, scene coupling, testing pain and initialization-order problems. A global service requires architectural justification, documented ownership and lifecycle, recorded in the *Décisions* section of the feature doc.

Avoid mutable static state for gameplay — it interacts badly with Domain Reload settings, Play Mode options, tests and scene reloads. Never assume entering Play Mode resets static fields.

## Events, coroutines, async

Events should reduce coupling, not hide control flow. Always consider publisher lifetime, subscriber lifetime, unsubscribe behavior, duplicate subscriptions and scene transitions. Prefer direct communication for simple local relationships; do not build a global event bus for every interaction.

Coroutines are for Unity-specific asynchronous sequences, not a substitute for architecture. Each needs clear ownership, cancellation and object lifetime. They stop when their GameObject is destroyed — never assume continuation across scene destruction.

Use async/await only where compatible with the existing project architecture. Respect Unity's main-thread restriction — never touch Unity objects from a worker thread. Handle cancellation, destroyed owners, scene transitions and exceptions. Do not introduce a second async framework.

## Allocation and performance

Never optimize blindly — profile first with the Unity Profiler, looking at CPU, GC allocations, rendering, physics, memory and asset loading.

Common allocation sources in hot paths: LINQ, closures, string creation, collection creation, boxing, repeated temporary arrays/lists. LINQ is fine where readability matters and frequency is low; avoid it in frequently executed gameplay loops. It is not banned globally.

Avoid excessive `Instantiate`/`Destroy` in hot paths (projectiles, enemies, VFX, temporary UI) — pool when profiling or frequency clearly warrants it, using the project's existing pooling solution rather than a new generic framework.

Readable code wins outside performance-critical paths.

## Materials

`renderer.material` may instantiate a material. Never do that accidentally every frame. Choose `sharedMaterial` or the instance APIs deliberately, based on whether shared or per-instance behavior is required.

## Animator

Avoid hardcoded Animator parameter strings scattered through the code:

```csharp
private static readonly int SpeedHash = Animator.StringToHash("Speed");
```

Do not over-couple to the Animator without reason.

## UI

Follow the project's existing UI technology — do not mix UI Toolkit and uGUI arbitrarily. Keep gameplay state separate from UI representation: UI must never be the source of truth for gameplay state, and domain logic must not be coupled to buttons, labels or specific GameObjects.

## Naming, namespaces, magic values

Follow existing namespaces; never put production code in the global namespace if the project uses namespaces. Namespaces reflect architecture and domain, not directory depth. Do not rename namespace trees as part of unrelated work.

Avoid unexplained gameplay magic numbers — prefer serialized configuration, constants, ScriptableObjects or documented settings. Do not build a configuration system for values that are genuinely local constants.
