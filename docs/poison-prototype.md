# Poison prototype

Task: [12487v29zj5](https://app.clickup.com/t/12487v29zj5).

Open `Assets/Scenes/Prototype/PoisonScene.unity` in Unity 6000.3.21f1 and press Play.
The scene opens directly onto a developer test bench; no Inspector edits or other
prototype branches are required. Click the Game view to give keyboard input focus.

## Controls and expected results

| Control | Result |
| --- | --- |
| +10 / P | Add ten percentage points, capped at 100. |
| -10 / O | Remove ten percentage points, floored at zero. |
| 49 / 50 / 75 / 100 presets | Replace intensity with the selected level. |
| Clear poison | Remove poison and stop damage without healing. |
| Pause / Space | Freeze health simulation; exposure controls remain usable. |
| Reset / R | Restore 100 HP, clear poison and resume simulation. |
| Test guide | Open instructions and pause; closing restores the previous pause state. |

Health, poison intensity, potential damage rate and simulation status are always
observable. The HUD poison gauge itself is hidden at zero, while the test bench
shows an explicit clear-state message. The line halfway along the gauge marks the
damage threshold. The death panel offers a restart when health reaches zero.

At 100 maximum HP, expected damage rates are 0 HP/s at 49%, 0.2 at 50%,
approximately 1.4142 at 75%, and 10 at 100%. The diagnostic rate describes the
current dose even while paused/dead; actual damage stops in those states.

## Ownership and scope

- `PoisonState`: bounded intensity, change snapshots and the damage curve; no Unity dependencies.
- `PoisonSimulation`: health, deterministic time steps, pause/death/reset; no Unity dependencies.
- `PoisonPrototypeController`: scene bootstrap, input and the single automatic simulation tick.
- `PoisonHudPresenter`: reusable state-to-slider binding; no diagnostic labels or gameplay ownership.
- `PoisonPrototypeView`: developer-only uGUI test bench, compiled for the Editor and Development builds.

The test bench is engineering tooling, with English diagnostic labels. It is not
production player UI and introduces no localization or Addressables system.
Its labels and controls are excluded from release players. The prototype scene is
not added to production build settings. To test a Development player, explicitly
include only this scene in a temporary build profile and enable Development Build.

The separate HUD task `12487v29zj4` has a `PlayerVitals` API on its own branch.
It is not directly wired here. A future integration must choose one health/poison
owner and one damage tick; do not run both simulation drivers on one player.

## Verification

Run EditMode and PlayMode suites in **Window > General > Test Runner**.
The pure tests cover the curve, bounds, invalid numbers, exact clearing, time
subdivision, pause and death/reset. PlayMode tests cover HUD lifecycle, scene
camera setup, displayed diagnostics, button callbacks, the guide and restart.

Manual acceptance: start clear; select 49% and confirm stable health; select 50%
and observe damage; pause and confirm a stable health reading; clear poison and
confirm no healing; reset; select 100% and wait for death; restart; open/close the
guide. Check pointer input and P/O/R/Space in the Game view, including a smaller
Game view. Automated state tests alone do not establish visible rendering.

### Local verification — 2026-09-21

Unity 6000.3.21f1: 47/47 EditMode and 14/14 PlayMode tests passed;
both result XML files were inspected. PlayMode includes queued Input System
keyboard events, guide suppression and post-death shortcut behavior.
Native pointer checks covered thresholds, pause, cure without healing, death,
restart and the guide. Native injected keyboard events were inconclusive;
the automated keyboard test passed. No standalone player build was verified.

The PR demonstration is an animated sequence of actual Game-view captures,
not a real-time recording: `media/poison-prototype.gif`.
