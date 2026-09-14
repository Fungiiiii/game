# Art pipeline — Blender to Unity

3D assets are authored in **Blender**. This describes how they reach Unity.

## The decision: export FBX, do not commit `.blend` into `Assets/`

Unity can import `.blend` files directly, but it does so by **launching Blender
in the background** to convert the file to FBX. That has consequences that are
easy to miss until they break a build:

- **Blender must be installed on every machine that opens the project** —
  including the CI runner. Without it, the import simply fails.
- The conversion is slow on first import, and re-runs whenever the file changes.
- It breaks across Blender versions. "Blender could not convert the .blend file
  to FBX file" is a recurring failure after Blender major releases.
- You get no control over export settings, because you are not the one
  exporting.

Unity's own documentation recommends exporting to FBX rather than using native
application formats in production.

**So:**

```text
ArtSource/          .blend sources — outside Assets/, Unity never touches them
  Characters/
  Props/
  Environment/

Assets/
  Art/
    Models/         exported .fbx — this is what Unity imports
    Textures/
    Materials/
```

`ArtSource/` sits outside `Assets/` deliberately: anything inside `Assets/` gets
imported, and `.blend` is exactly what we do not want imported.

Both `.blend` and `.fbx` are tracked by Git LFS — see
[`.gitattributes`](../.gitattributes). Blender's `.blend1` / `.blend2` backup
files are ignored.

## Export settings must be one shared preset

The single most common Blender-to-Unity problem is a model arriving rotated 90°
on X, or at 100× or 0.01× the intended scale. The cause is always the same:
**Blender is Z-up, Unity is Y-up**, and the two disagree on unit scaling, so
every artist's export settings produce a slightly different result.

Do not solve this per-asset by rotating the mesh in Unity or in the prefab. That
hides the problem and it comes back on the next export.

Instead, agree **one** FBX export configuration, save it as a Blender export
preset, commit the preset to `ArtSource/`, and have everyone use it. Then verify
once, with a reference cube:

- 1 Blender unit imports as 1 Unity unit (1 metre)
- the model's forward axis points along Unity's +Z
- the transform in Unity shows rotation `(0, 0, 0)` and scale `(1, 1, 1)`

If a freshly imported model needs a correction in the Inspector, the preset is
wrong — fix the preset, not the asset.

## Naming and organisation

Asset names are **English**, like everything else outside `docs/adrs/` — see
`CLAUDE.md`. Name by what the thing is, not by what it looks like today:
`MushroomCap_Large`, not `Champignon_final_v3_OK`.

One exported FBX per logical asset. Do not export an entire scene as one FBX and
split it in Unity.

## Materials and textures

Do not rely on materials coming across from Blender. Blender and Unity use
different shading models, and the render pipeline (URP/HDRP/Built-in) decides
what a material even is. Author materials in Unity, against the project's
pipeline.

Textures are imported separately and referenced by Unity materials. Keep them in
`Assets/Art/Textures/`, not embedded in the FBX.

## Before committing an art change

- [ ] The `.fbx` is exported with the shared preset, not with ad-hoc settings
- [ ] Import shows rotation `(0, 0, 0)` and scale `(1, 1, 1)` in Unity
- [ ] The `.blend` source is in `ArtSource/`, not in `Assets/`
- [ ] `git lfs status` shows both files tracked by LFS
- [ ] Names are English and describe the asset
- [ ] No `.blend1` / `.blend2` backup files staged
