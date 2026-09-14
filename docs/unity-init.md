# Unity project initialization

Everything to do once, when the Unity project is first created in this
repository. Follow it in order — several steps are much harder to fix after the
first commit than before it.

> **Done.** The Unity project was created on 2026-09-14 with Unity
> `6000.3.21f1`, from the official 3D Cross-Platform (URP) template. This
> document is kept as the record of what was decided and why, and as the
> reference for anyone rebuilding the project or reviewing those choices.
> Steps that produced a lasting rule now live where that rule is enforced —
> analyzer setup in [`docs/linting.md`](./linting.md), the two open decisions
> in [`docs/adrs/`](./adrs).

`ProjectSettings/ProjectVersion.txt` is now present and authoritative:
`6000.3.21f1`.

---

## 1. Decide before creating anything

### Already decided

| Decision | Value |
|---|---|
| Engine | **Unity 6.3 LTS** |
| Language | **C# (.NET)** |
| Dimension | **3D** |
| 3D authoring | **Blender** — see [`docs/art-pipeline.md`](./art-pipeline.md) |

This is recorded in the project documentation on Atlassian. It is **not** yet
mirrored as an ADR in `docs/adrs/`, and writing one would require the rationale
and alternatives that were actually weighed — which are not in this repository.
Either link the Atlassian page from an ADR, or write the ADR from what the team
remembers. Do not reconstruct it from guesses.

### Decided at creation

Both were baked in at creation time and are expensive to reverse. Both were
already settled in the Spécifications Techniques on Atlassian; the ADRs bring
that decision into the repository, where it is reviewed alongside the code it
constrains.

| Decision | Value | ADR |
|---|---|---|
| Render pipeline | **URP** `17.3.0` | [ADR-0002](./adrs/0002-pipeline-de-rendu-urp.md) |
| Input architecture | **Input System** `1.20.0`, legacy Input Manager disabled | [ADR-0003](./adrs/0003-architecture-des-entrees-input-system.md) |

Two more decisions can wait, but must be made deliberately rather than by
accident — see the `unity-localization` and `unity-project-config` skills:

- **Addressables** as the asset-loading strategy.
- **Localization**, which pulls in Addressables whether you wanted it or not.

### On the Unity version

Unity 6.3 LTS is the right call: it is supported until December 2027, whereas
Unity 6.0 LTS loses support in October 2026.

Pick the exact patch release in Unity Hub and record it. From the moment the
project exists, `ProjectSettings/ProjectVersion.txt` is authoritative, and
upgrades need their own branch, PR and validation.

---

## 2. Create the project

Create it through Unity Hub, **directly into this repository root**, so that
`Assets/`, `Packages/` and `ProjectSettings/` sit next to `CLAUDE.md`.

Pick the **3D** template matching the render pipeline decision above.

---

## 3. Project Settings — before the first commit

These three make the difference between a repository that can be diffed and
merged, and one that cannot. Do them before committing anything.

**Edit → Project Settings → Editor:**

- **Asset Serialization → Mode: `Force Text`.** This is the default in current
  Unity, but verify it. In `Force Binary` or `Mixed`, scenes and prefabs are
  opaque blobs: no diff, no merge, no review. Every rule in the
  `unity-serialization` skill depends on this.
- **Version Control → Mode: `Visible Meta Files`** if the option is present in
  your version. `.meta` files must be visible and committed — GUIDs are
  contracts.

**Edit → Project Settings → Player → Other Settings:**

- **Api Compatibility Level** — `.NET Standard 2.1` unless a dependency forces
  `.NET Framework`. Standard is the smaller, more portable target.
- **Scripting Backend** — Mono for fast iteration, IL2CPP for shipping builds.
  Decide per platform, and record it alongside the platform decision.

**Edit → Project Settings → Editor → Enter Play Mode Settings:**

- Consider disabling Domain Reload for faster iteration. If you do, read the
  static-state section of the `unity-runtime-code` skill first: with Domain
  Reload off, static fields are **not** reset when entering Play Mode, and code
  that assumed they were will start failing in ways that do not reproduce in a
  build.

---

## 4. Git plumbing

[`.gitignore`](../.gitignore) and [`.gitattributes`](../.gitattributes) are
already in the repository. What remains is per-clone configuration, which git
deliberately never installs automatically:

```bash
./scripts/setup.sh
```

This enables the hooks, initializes Git LFS, and wires up Unity's
`UnityYAMLMerge` for scene and prefab merges. Install **git-lfs** before running
it, or LFS will be skipped:

- Without LFS, every version of every texture, model and audio file is stored
  whole. The repository becomes painfully slow within months and cannot be
  cleaned up without rewriting history.
- Without `UnityYAMLMerge`, `.gitattributes` still routes scene and prefab
  merges to it, and git silently falls back to line-based merging — which
  corrupts them.

The script reports which parts succeeded. Re-run it after installing anything
that was reported missing.

---

## 5. Folder and assembly structure

Create assembly definitions from the start. Adding them to an existing codebase
means untangling every accidental dependency at once.

```text
ArtSource/          .blend sources — OUTSIDE Assets/, never imported by Unity
  Characters/
  Props/
  Environment/

Assets/
  Scripts/
    Runtime/        Fungiiiii.Runtime.asmdef
    Editor/         Fungiiiii.Editor.asmdef      (references Runtime)
    Tests/
      EditMode/     Fungiiiii.Tests.EditMode.asmdef
      PlayMode/     Fungiiiii.Tests.PlayMode.asmdef
  Art/
    Models/         exported .fbx
    Textures/
    Materials/
  Audio/
  Prefabs/
  Scenes/
  Settings/
```

`ArtSource/` is outside `Assets/` on purpose: Unity imports everything inside
`Assets/`, and importing `.blend` requires Blender on every machine including
CI. See [`docs/art-pipeline.md`](./art-pipeline.md).

Rules from the `unity-project-config` skill that apply here:

- Editor code lives in an Editor assembly. **Runtime must never reference
  `UnityEditor`** — a runtime build that does will fail, often only at build
  time on the CI machine.
- No circular dependencies between assemblies.
- Namespaces follow the assembly structure, not folder depth.

---

## 6. Packages

Add only what the decisions in step 1 require. Every package is a dependency:
`CLAUDE.md` requires justification, Unity-version compatibility and a license
check before adding one.

Likely from the start:

- **Input System** (`com.unity.inputsystem`) if that was the input decision.
- **Test Framework** (`com.unity.test-framework`) — usually already present.
- **TextMeshPro** — bundled in Unity 6.

Deliberately deferred until their ADR exists: Addressables, Localization,
Cinemachine, any networking package.

---

## 7. Analyzers

Install `Microsoft.Unity.Analyzers` — the DLL has to be in the project for
command-line and CI compilation, even though Rider and Visual Studio bundle it.

Full procedure, including the verification step that catches a silently
unloaded analyzer: [`docs/linting.md`](./linting.md).

---

## 8. Tests

Create both test assemblies immediately, each with at least one passing test.
An empty test project and a broken test runner look identical in CI.

Prefer EditMode; see the `unity-testing` skill for the split.

---

## 9. CI

[`.github/workflows/unity-tests.yml`](../.github/workflows/unity-tests.yml)
skips itself until `ProjectSettings/ProjectVersion.txt` exists. Once it does,
it needs a Unity licence in the secrets, and nothing else will make it run.

game-ci accepts several activation strategies. Its CLI (`v0.1.63`, the version
the action pulls) lists these when none is configured:

| Secrets | Seat |
|---|---|
| `UNITY_EMAIL` + `UNITY_PASSWORD` | Personal (free) |
| `UNITY_EMAIL` + `UNITY_PASSWORD` + `UNITY_SERIAL` | Pro / Plus |
| `UNITY_LICENSE`, the contents of a `.ulf` | Enterprise / Industry |

The workflow currently passes through `UNITY_LICENSE`, `UNITY_EMAIL` and
`UNITY_PASSWORD`. A Pro seat would also need `UNITY_SERIAL` added to the `env:`
block of the test step — it is not wired up.

The published documentation at <https://game.ci/docs/github/activation>
disagrees with the CLI: it asks for `UNITY_LICENSE` *and* the email and
password for a Personal seat. The CLI message is the one that matches the
version actually running. If a Personal seat refuses to activate on email and
password alone, the `.ulf` route is the fallback.

**Use an account created for the project, never a personal one.** Anyone who
can push a workflow to this repository can make it print a secret, so the
credential should be a project asset, not somebody's Unity identity — and a
project account can be rotated without asking a person to change their own
password.

`Fungiiiii` is a GitHub organization, so these belong at organization level
(Settings → Secrets and variables → Actions), scoped to this repository: set
once, rotated in one place, usable by every repository of the org. Repository
secrets behave identically but have to be repeated per repository. Either way,
**never commit them** — see `CLAUDE.md`.

One trap: GitHub does not expose secrets to workflows triggered by a pull
request from a **fork**. Work on branches of this repository, or these tests
cannot run at all.

Also worth configuring at the same time, since local hooks only protect the
clone they were installed in: branch protection on `main`, requiring a PR and
at least one human reviewer.

---

## 10. Verify, then commit

Before the first commit, confirm:

- [ ] `ProjectSettings/ProjectVersion.txt` shows the version from the ADR
- [ ] Scene and prefab files open as readable text
- [ ] `.meta` files exist next to every asset and are not ignored
- [ ] `git status` shows no `Library/`, `Temp/`, `Obj/` or `Logs/`
- [ ] `git lfs status` shows binary assets tracked by LFS
- [ ] `./scripts/setup.sh` reports hooks, LFS and merge tool all configured
- [ ] The project compiles with no Console errors
- [ ] Both test assemblies run and pass
- [ ] An analyzer diagnostic actually fires (see `docs/linting.md`)
- [ ] A reference cube exported from Blender imports at rotation `(0,0,0)` and
      scale `(1,1,1)` — see [`docs/art-pipeline.md`](./art-pipeline.md)
- [ ] ADRs for the still-open step 1 decisions exist in `docs/adrs/`

The Unity project itself is a large first commit, but it is one coherent change.
Branch and PR as usual — `CLAUDE.md` allows no exception for it.
