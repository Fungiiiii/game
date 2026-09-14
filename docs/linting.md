# Linting

The linter is deliberately limited to rules that catch **bugs**. Style and
"code quality" opinions are left advisory: they belong in review, not in a
build failure. Severities live in **two** files that must stay in sync —
[`.editorconfig`](../.editorconfig) for the IDEs and
[`Assets/Default.ruleset`](../Assets/Default.ruleset) for Unity itself. The
Installation section below explains why both are needed.

## Microsoft.Unity.Analyzers

[Microsoft.Unity.Analyzers](https://github.com/microsoft/Microsoft.Unity.Analyzers)
is the Roslyn analyzer set for Unity. It catches things the C# compiler cannot,
because they are only wrong in Unity's object model — for example:

- **UNT0008** null propagation (`transform?.parent`) on a Unity object. `?.`
  bypasses Unity's overloaded `==`, so a destroyed object is treated as
  non-null. This is the bug class described in `CLAUDE.md` null semantics,
  and it is invisible to a normal C# reader.
- **UNT0007 / UNT0023** the same trap with `??` and `??=`.
- **UNT0006 / UNT0033** a Unity message with the wrong signature or the wrong
  case — `void update()` compiles fine and is simply never called.
- **UNT0012** a coroutine called without `StartCoroutine`, which silently does
  nothing.
- **UNT0030** `Destroy` on a `Transform`, which throws at runtime.

These are configured as **errors**. Performance rules are **suggestions**,
because `CLAUDE.md` requires profiling before optimizing. Two readability rules
and `UNT0039` are **off** — `UNT0039` pushes `[RequireComponent]`, which the
`unity-runtime-code` skill says not to overuse.

## Installation

**Installed.** `Assets/Plugins/Analyzers/Microsoft.Unity.Analyzers.dll`,
version 1.27.0, labelled `RoslynAnalyzer`, with every platform disabled in its
importer so it never ships inside a build.

Rider and Visual Studio bundle these analyzers, so an IDE user already sees the
diagnostics. Command-line and CI compilation do **not**: the analyzer has to be
in the project.

The GitHub releases carry no downloadable asset — the analyzer is published on
NuGet. To update it:

1. Download the package from
   `https://api.nuget.org/v3-flatcontainer/microsoft.unity.analyzers/<version>/microsoft.unity.analyzers.<version>.nupkg`
   and extract `analyzers/dotnet/cs/Microsoft.Unity.Analyzers.dll` from it.
2. Replace the DLL under `Assets/Plugins/Analyzers/`.
3. Keep the asset label **`RoslynAnalyzer`**. Unity only loads analyzers
   carrying that label — and without it Unity imports the DLL as an
   auto-referenced managed plugin, whose bundled `Vector2` and `Vector3` then
   collide with `UnityEngine.CoreModule` and fail every assembly with CS0433.
4. Keep every platform unchecked in the importer. An analyzer is a compile-time
   tool and must not be included in builds.
5. Verify a diagnostic fires — write `private void update() { }` in a
   MonoBehaviour and confirm UNT0033 appears, **as an error**.

Step 5 is the one that matters: an analyzer that is present but not loaded
produces no errors and looks exactly like a clean codebase.

### Why severities are duplicated

Unity does **not** pass `.editorconfig` to Roslyn. Verified on Unity
`6000.3.21f1`: the generated response file under `Library/Bee/artifacts/`
contains `-analyzer:` for the DLL, but no `/analyzerconfig:` — at any path,
including inside `Assets/`. With severities declared only in `.editorconfig`,
a lowercase `update` was reported as `warning UNT0033` and the build passed,
even though `.editorconfig` marks that rule as an error.

Unity does pass `-ruleset:` for `Assets/Default.ruleset`. With the same rule
declared there, the same code produced `error UNT0033` and compilation failed.

So both files are required, and they serve different readers:

| File | Read by | Effect |
|---|---|---|
| `.editorconfig` | Rider, Visual Studio | Squiggles while typing |
| `Assets/Default.ruleset` | Unity's compiler, therefore CI | Breaks the build |

**Any change to a severity must be made in both files.** This duplication is
accepted technical debt: nothing enforces that they agree. Generating one from
the other, and checking it in CI, is the obvious follow-up.

## What is deliberately not enabled

The general .NET analyzers (`CA…`) and the IDE rules (`IDE…`) are not adopted.
They are mostly style and quality opinions, they are not installed by default in
Unity, and turning them on wholesale produces exactly the noise this
configuration is meant to avoid.

If a specific `CA` rule is wanted later because it catches a real bug class,
enable that rule by ID — not the whole ruleset.
