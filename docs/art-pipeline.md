# Art pipeline — Blender to Unity

How to get a model from Blender into the game. Day-to-day reference.

The decisions behind these rules live in
[ADR-0005](./adrs/0005-pipeline-et-nommage-des-assets-3d.md). If this file and
the ADR disagree, the ADR wins — and this file is the one to fix.

## The five rules

1. **No `.blend` in this repository.** Sources stay on your machine.
2. **Export with the committed preset.** Never with hand-typed settings.
3. **`Apply Transform` off in Blender. `Bake Axis Conversion` on in Unity.**
   Never both.
4. **Name the asset, not its state.** `Rock_Small_01`, never `Rock_final_v3`.
5. **A palette colour never moves.** You only ever add new ones.

## Where things live

```text
Assets/
  Art/
    Models/<Domain>/<Name>.fbx           exported models
    Textures/Palette.png                 the shared palette
    Textures/<Domain>/<Name>_<Map>.png   dedicated textures — the exception
    Materials/Palette.mat                the shared material
  Prefabs/<Domain>/<Name>.prefab
tools/blender/                           the export preset
```

`<Domain>` is `Characters`, `Environment` or `Props`. Adding a fourth means
amending ADR-0005.

### Why no `.blend` here

This repository holds exported models only. Your sources stay local, and you
back them up yourself. A pre-commit check rejects any `.blend`.

This is a trade, not an oversight. It buys a light clone and an untouched LFS
quota. It costs the ability to ever re-edit an asset if your local sources are
lost. ADR-0005 records it as accepted debt. It also names what reopens the
decision: **a second person modelling.**

## Exporting from Blender

Use the committed preset. Do not type these settings by hand.

Install it once — see [`tools/blender/README.md`](../tools/blender/README.md).
It then shows up in the FBX exporter's **Operator Presets** dropdown.

The preset file is authoritative. The table below is its human-readable
version, with the reasoning for the settings that are not obvious.

| Section | Setting | Value |
|---|---|---|
| Top | Path Mode | `Auto` |
| Include | Limit to → Selected Objects | on |
| Include | Object Types | `Armature`, `Mesh` |
| Include | Custom Properties | off |
| Transform | Scale | `1.00` |
| Transform | Apply Scalings | `FBX All` |
| Transform | Forward / Up | `-Z Forward` / `Y Up` |
| Transform | Apply Unit | on |
| Transform | Use Space Transform | on |
| Transform | **Apply Transform** | **off — always** |
| Geometry | Smoothing | `Face` |
| Geometry | Apply Modifiers | on |
| Geometry | Loose Edges | off |
| Geometry | Tangent Space | off |
| Geometry | Triangulate Faces | off |
| Armature | Primary / Secondary Bone Axis | `Y` / `X` |
| Armature | Armature FBXNode Type | `Null` |
| Armature | Only Deform Bones | off |
| Armature | Add Leaf Bones | off |
| Bake Animation | — | off |

### The ones worth knowing

**Apply Transform — off, always.** The axis conversion happens once, on the
Unity side. Doing it in both places risks converting twice. The option is also
experimental and breaks armatures.

**Smoothing — `Face`.** Blender's default is `Normals Only`. It drops smoothing
information, and you get shading artefacts in Unity.

**Apply Modifiers — watch out.** Modifiers apply at their *viewport* level, not
their render level. A Subsurf set to 1 in the viewport and 3 in render exports
at 1.

**Only Deform Bones — off.** Turning it on drops the root bone, which deforms
no vertex. The hierarchy breaks with it. The cost of leaving it off: IK and
control bones come across too.

**Triangulate Faces — off.** Unity triangulates on import, and the mesh stays
editable.

**Selected Objects — on.** One logical asset per file. Never export a whole
scene and split it in Unity.

## Importing into Unity

Export settings are only half the job. The Unity importer decides as much about
final scale and orientation as Blender does.

So the import side is pinned by committed Presets, not by memory.

### Static props — the project default

`ModelImporter_StaticProp` is the **Default Preset** for the ModelImporter. Any
FBX you drop into `Assets/` already arrives correct.

| Tab | Setting | Value | Why |
|---|---|---|---|
| Model | Bake Axis Conversion | **on** | Unity converts Blender's axes itself, without touching the skeleton. Without it, models arrive rotated -90° on X. |
| Rig | Animation Type | `None` | A static prop needs no rig. `Generic` here would generate an Avatar for every rock. |
| Materials | Material Creation Mode | `None` | Everything shares `Palette.mat`. |

### Rigged characters

Apply `ModelImporter_RiggedCharacter` **by hand**, then *Apply*.

One difference: **Rig → Animation Type: `Generic`**. The creatures are not
humanoid, so `Humanoid` and its retargeting buy nothing.

> This step cannot be automatic. Unity's Default Preset is global per importer
> type. ADR-0005 accepts the manual step rather than writing an
> AssetPostprocessor.
>
> **Imported a character and it has no Animator? This is the step you
> skipped.**

### The palette texture

Its settings are applied by hand. A default preset for textures would also hit
UI sprites, which want the opposite.

| Setting | Value | Why |
|---|---|---|
| Filter Mode | `Point` | |
| Wrap Mode | `Clamp` | |
| Generate Mip Maps | **off** | Mips blend neighbouring swatches. Distant geometry picks up colours that are not its own. |
| Compression | `None` | A palette is a handful of exact colours. Lossy compression shifts them. |

## Naming

English. ASCII. PascalCase. Segments separated by `_`.

```text
<Name>[_<Variant>][_NN]
```

`Rock_Small_01` · `MushroomCap_Large` · `TreePine_01`

The rule applies to four things at once:

| What | Example |
|---|---|
| The FBX file | `Assets/Art/Models/Environment/Rock_Small_01.fbx` |
| **The object inside Blender** | `Rock_Small_01` |
| The material | `Palette.mat` — shared. A dedicated one takes the model's name. |
| The prefab | `Assets/Prefabs/Environment/Rock_Small_01.prefab` |

**The Blender object name matters.** It becomes the GameObject name on import.
A `.blend` full of `Cube.001` gives you a Unity hierarchy full of `Cube.001`.

Other rules:

- `NN` is two digits. It only separates interchangeable variations.
- Name what the thing **is**. Not what it looks like today.
- **Never name a state.** No `_final`, `_v3`, `_new`, `_old`, `_test`, `_OK`,
  `_copy`. Git already records versions.

`scripts/check-art-assets.sh` enforces all of this on commit.

### Animations

Not exported yet. When they arrive, each clip ships in its own file:

```text
Assets/Art/Models/Characters/PlayerMushroom.fbx
Assets/Art/Models/Characters/PlayerMushroom@Idle.fbx
Assets/Art/Models/Characters/PlayerMushroom@Walk.fbx
```

Unity picks these up automatically for the rig of the same name.

Turning `Bake Animation` on in the export preset is part of that change, not of
this one.

## The palette

One `Assets/Art/Textures/Palette.png`. One `Palette.mat`. Every model's UVs
point at swatches on it.

That is what removes UV unwrapping from the workflow, and keeps the whole set
on a single material.

**The rule that matters: a colour never moves. You only ever append.**

The grid is frozen the day the palette is created. Moving a swatch, reordering
the grid or resizing the image invalidates the UVs of every model already
exported.

Every asset in the game changes colour. And the diff that caused it contains a
single `.png`. Adding a colour in free space is safe. Anything else is not.

<!-- Fill in when Palette.png enters the repository: -->
<!-- Grid: N x N swatches, image WxH px, one swatch = P px -->

Author materials in Unity, against URP
([ADR-0002](./adrs/0002-pipeline-de-rendu-urp.md)). Do not rely on materials
coming across from Blender. The two use different shading models, and the
render pipeline decides what a material even is.

## Verify once, with a reference cube

Before trusting the preset, export a 1×1×1 cube at the origin. Then check in
Unity:

- 1 Blender unit imports as 1 Unity unit (1 metre)
- the forward axis points along Unity's +Z
- the Inspector shows rotation `(0, 0, 0)` and scale `(1, 1, 1)`

**If a fresh model needs a correction in the Inspector, the preset is wrong.
Fix the preset, not the asset.** Rotating or rescaling in Unity hides the
problem, and it comes back on the next export.

## Before committing an art change

- [ ] Exported with the committed preset, not ad-hoc settings
- [ ] `Apply Transform` was off, `Bake Axis Conversion` is on
- [ ] Inspector shows rotation `(0, 0, 0)` and scale `(1, 1, 1)`
- [ ] A character got `ModelImporter_RiggedCharacter` applied
- [ ] No `.blend` staged
- [ ] Names are English, PascalCase, and describe the asset — not its state
- [ ] The Blender object name matches the file name
- [ ] The palette was appended to, never reordered or resized
- [ ] `git lfs status` shows the `.fbx` and `.png` tracked
