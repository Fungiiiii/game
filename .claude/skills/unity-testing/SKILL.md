---
name: unity-testing
description: Testing strategy for this Unity project — choosing EditMode vs PlayMode with the Unity Test Framework, writing regression tests for bug fixes, designing for testability, deterministic randomness and time handling. Use when writing or modifying tests, when fixing a bug, or when deciding whether a system is testable.
---

# Testing

Use the Unity Test Framework.

## EditMode vs PlayMode

**EditMode** — plain C# logic, domain logic, utility logic, deterministic systems, ScriptableObject logic that needs no runtime behavior.

**PlayMode** — anything requiring GameObjects, MonoBehaviours, lifecycle callbacks, physics, scenes, or real runtime Unity behavior.

Prefer EditMode whenever Unity runtime behavior is not genuinely required: it is faster and simpler.

## Regression tests

Every bug fix includes a regression test when reasonably possible. The test must **fail before the fix and pass after it** — write it first and watch it fail.

Never modify an existing test merely to make new code pass. If a test now fails, either the code is wrong or the requirement genuinely changed — and a changed requirement needs to be stated explicitly, not silently absorbed into the test.

## Testability

Do not make systems unnecessarily dependent on scenes, static state, singleton instances, hard-coded GameObject searches, or the Unity lifecycle when plain C# dependencies would make testing simpler.

But do not distort simple production code merely to satisfy tests. Balance testability against KISS.

## Determinism

Systems involving randomness should allow deterministic testing. Avoid hard-wiring random generation deep inside gameplay logic when seeded or injected randomness would simplify testing and replay. Use the project's randomness architecture if one exists.

## Time

Normal MonoBehaviour gameplay can use Unity time APIs (`Time.deltaTime`) directly. Only abstract time away when there is a concrete deterministic-simulation or testing requirement. YAGNI applies.

## Running tests

Check whether an Editor is present before assuming either way. On the maintainer's Windows machine Unity `6000.3.21f1` is installed, and both modes run headlessly:

```bash
"C:/Program Files/Unity/Hub/Editor/6000.3.21f1/Editor/Unity.exe" -batchmode -nographics \
  -runTests -projectPath . -testPlatform EditMode \
  -testResults results.xml -logFile unity.log
```

Use `-testPlatform PlayMode` for the other mode. Batch mode has returned exit code 0 with failing compilation, so read `results.xml` and the log — never the exit code alone.

Where no Editor exists (cloud agents, CI containers), ask the developer or rely on CI. Never report a test result that was not actually observed; state explicitly which tests were written but not executed.
