---
name: unity-serialization
description: Rules for Unity serialized assets — scenes, prefabs, ScriptableObjects, materials, animations, controllers, timelines, .meta files and asset GUIDs. Use when creating, editing, moving or deleting any Unity asset, when adding or renaming a [SerializeField], and when reviewing a git diff that contains .unity, .prefab, .asset, .controller, .anim, .mat or .meta files.
---

# Unity serialization

Serialized Unity data is a contract. Treat every YAML diff as load-bearing.

## Serialized fields

Prefer `[SerializeField] private float movementSpeed;` over making a field public just for Inspector access. Public fields should represent an actual public API — Inspector visibility alone is never a reason to make a field public.

When renaming a serialized field that existing scenes, prefabs or ScriptableObjects depend on, preserve compatibility:

```csharp
[FormerlySerializedAs("oldName")]
[SerializeField]
private float newName;
```

Do not rename serialized fields casually.

## .meta files and GUIDs

Asset GUIDs are contracts.

- Never remove a `.meta` file unless the asset itself is intentionally deleted.
- Never regenerate a GUID without a specific, stated reason.
- Investigate every unexpected `.meta` change before committing it.
- If assets are moved programmatically or outside Unity, move the `.meta` alongside.

## Prefabs

Prefer reusable prefabs over duplicated GameObject hierarchies.

Before editing, know which of these you are editing: a prefab asset, a nested prefab, a prefab variant, or a scene instance. The consequences differ.

Avoid unnecessary overrides. Never blindly apply all overrides from a scene instance back to the prefab — verify each one is intentional.

## Scenes

Scenes must not become implicit dependency containers. Avoid hard-coded cross-scene object references.

Before modifying a scene, determine: scene ownership, additive loading, scene dependencies, bootstrap behavior, persistent objects.

Do not place a global system independently in several scenes where it could produce duplicates. Do not introduce `DontDestroyOnLoad` casually — persistent systems need documented ownership and lifecycle.

**Scene transitions must be explicit**: loading state, unloading state, persistent objects, async loading, player state, save state, transition UI, failure handling. Never assume a specific scene is active unless the architecture guarantees it. Large scene-management changes require an ADR (see the `adr` skill).

## Assets

Before adding an asset, determine ownership, license, source, import settings, target-platform impact and memory impact. Do not commit unnecessary or duplicated source files.

3D assets are authored in **Blender** and reach Unity as exported `.fbx`. `.blend` sources never enter this repository — see `docs/art-pipeline.md` and ADR-0005 before touching models, and never fix an import by rotating or rescaling in the Inspector. Naming, folder and palette rules are enforced by `scripts/check-art-assets.sh`.

## Diff review — mandatory before committing

For any `.unity`, `.prefab`, `.asset`, `.controller`, `.anim`, `.mat`, `.meta`, `ProjectSettings` or `Packages` file:

1. Read the diff — do not commit Unity YAML sight unseen.
2. Confirm only intended fields changed.
3. Check prefab overrides are intentional.
4. Check for broken references and missing scripts.
5. Check no GUID changed unexpectedly.

Do not save a scene or prefab merely because Unity marked it dirty. Do not commit mass reserialization caused by opening the project in a different Unity version — that is a separate, authorized task.
