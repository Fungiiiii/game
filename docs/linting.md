# Linting

The linter is deliberately limited to rules that catch **bugs**. Style and
"code quality" opinions are left advisory: they belong in review, not in a
build failure. Severities live in [`.editorconfig`](../.editorconfig).

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

**Not yet installed** — there is no Unity project in this repository.

Rider and Visual Studio bundle these analyzers, so an IDE user already sees the
diagnostics. Command-line and CI compilation do **not**: the analyzer has to be
in the project.

When the Unity project exists:

1. Download `Microsoft.Unity.Analyzers.dll` from the
   [releases](https://github.com/microsoft/Microsoft.Unity.Analyzers/releases).
2. Place it under `Assets/` (a `Assets/Plugins/Analyzers/` folder keeps it tidy).
3. Select it in the Project window and give the asset the label
   **`RoslynAnalyzer`**. Unity only loads analyzers carrying that label.
4. Verify a diagnostic fires — write `private void update() { }` in a
   MonoBehaviour and confirm UNT0033 appears in the Console.

Step 4 is the one that matters: an analyzer that is present but not loaded
produces no errors and looks exactly like a clean codebase.

## What is deliberately not enabled

The general .NET analyzers (`CA…`) and the IDE rules (`IDE…`) are not adopted.
They are mostly style and quality opinions, they are not installed by default in
Unity, and turning them on wholesale produces exactly the noise this
configuration is meant to avoid.

If a specific `CA` rule is wanted later because it catches a real bug class,
enable that rule by ID — not the whole ruleset.
