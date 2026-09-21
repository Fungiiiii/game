# Blender export preset

[`Fungiiiii.py`](./Fungiiiii.py) is the project's FBX export preset.

It is authoritative for the export settings.
[`docs/art-pipeline.md`](../../docs/art-pipeline.md) explains them.
[ADR-0005](../../docs/adrs/0005-pipeline-et-nommage-des-assets-3d.md) decides
them.

It lives in the repository even though `.blend` sources do not. A preset is
configuration, not art. Versioning it is what makes "everyone exports the same
way" true instead of aspirational.

## Install

Copy it into Blender's operator preset directory. Then restart Blender.

It appears in the FBX exporter's **Operator Presets** dropdown, as *Fungiiiii*.

| OS | Path |
|---|---|
| Windows | `%APPDATA%\Blender Foundation\Blender\<version>\scripts\presets\operator\export_scene.fbx\` |
| macOS | `~/Library/Application Support/Blender/<version>/scripts/presets/operator/export_scene.fbx/` |
| Linux | `~/.config/blender/<version>/scripts/presets/operator/export_scene.fbx/` |

Create the `export_scene.fbx` folder if it is missing. Blender only creates it
the first time you save a preset yourself.

## Changing it

Never change an export setting in the Blender UI and keep it to yourself. The
next person exports differently. The difference shows up weeks later, as a
model that is subtly the wrong scale.

Change it here instead. Or save a new preset in Blender, then copy the
generated file over this one.

Re-run the reference cube check in `docs/art-pipeline.md` before committing. A
preset change is a change to every future asset.

## Verification status

**This preset has never been loaded in Blender.**

It was written from the settings agreed for the project, following the
`export_scene.fbx` operator property names. But nobody has opened Blender,
selected it and exported with it.

Before relying on it:

1. Install it. Confirm it appears in the Operator Presets dropdown.
2. Confirm every value in the exporter panel matches the table in
   `docs/art-pipeline.md`.
3. Export the reference cube and check it in Unity.

If a property name is wrong, Blender reports it in the console when the preset
is applied.
