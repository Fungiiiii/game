---
name: unity-localization
description: Internationalization rules for this project — no player-facing string is ever hardcoded. Covers the Unity Localization package (Locales, String Table Collections, LocalizedString, Smart Strings, LocalizeStringEvent, async initialization), what must never be localized, and the pitfalls that silently break translations (string concatenation, plural rules, text expansion, RTL, CJK font atlases, culture-aware number and date formatting). Use when adding or changing any text or asset the player can see or hear, when building UI, and when deciding how a string reaches the screen.
---

# Internationalization

**No player-facing string is ever hardcoded.** Every string, and every asset with baked-in language (voice lines, localized textures, signage), goes through the localization system. This is not deferrable: retrofitting i18n means revisiting every UI call site a second time.

This is the counterpart to the English-only rule in `CLAUDE.md`. Source code is English; what the player reads is data.

## Status in this project

The Unity Localization package (`com.unity.localization`) is **not currently installed** — there is no Unity project in the repository yet. Check before assuming:

```bash
grep -n 'com.unity.localization' Packages/manifest.json
```

**Adopting it requires an ADR**, for a reason that is easy to miss: the Localization package is built on top of **Addressables** and stores localized assets as AssetBundles. Adding localization therefore also adopts Addressables as the asset-loading architecture — which `unity-project-config` already flags as an ADR-level decision. Decide both in one ADR, deliberately, rather than acquiring Addressables as a side effect.

Until that ADR is accepted, do not hand-roll a second localization mechanism to "get started". Raise the decision instead.

## What is localized, what is not

**Localized** — anything the player sees or hears: UI labels, buttons, menus, tooltips, dialogue, item and ability names and descriptions, tutorial text, player-facing error messages, credits, localized audio, localized textures and signage.

**Never localized, always English** — log and exception messages, `Debug.Log`, editor tooling UI, asset and scene names, serialized field names, table keys, analytics events, save-file contents, code comments. These are engineering artifacts, covered by the English-only rule.

A message shown to the player *and* logged is two strings, not one: a localized one for the UI, an English one for the log.

## Core concepts

| Concept | Role |
|---|---|
| `Locale` | One language/region. `LocalizationSettings.AvailableLocales.Locales` lists them |
| String Table Collection | Keyed translations, one column per locale |
| Asset Table Collection | Per-locale assets (audio, sprites, fonts) |
| `LocalizedString` | Serializable reference to a table + entry; exposes a `StringChanged` event |
| `LocalizedAsset<T>` | Same for assets — `LocalizedSprite`, `LocalizedAudioClip`, `LocalizedTmpFont`, `LocalizedGameObject`, … Concrete subclasses must be `[Serializable]` |
| Smart Strings | Placeholders, plurals and conditionals inside the translated text itself |
| Pseudo-localization | Fake locale that exposes hardcoded strings and layout overflow *before* translators deliver |

Tables import and export as **XLIFF, CSV or Google Sheets** — that is the translator handoff, not copy-paste.

## Getting text on screen

Prefer the **component-based** path: a `LocalizeStringEvent` on the UI object, wired in the Inspector. No code, and no call site to forget.

In code, use a serialized `LocalizedString` and subscribe to `StringChanged` — do not poll, and do not re-read the value every frame:

```csharp
[SerializeField] private LocalizedString scoreLabel;

private void OnEnable()  { scoreLabel.StringChanged += HandleScoreLabelChanged; }
private void OnDisable() { scoreLabel.StringChanged -= HandleScoreLabelChanged; }
```

`StringChanged` also fires when the locale changes, which is what makes live language switching work for free. Anything that reads a string once and caches it will show a stale language after a switch.

For dynamic values, set `Arguments` and call `RefreshString()` — never rebuild the sentence yourself.

## Async initialization

The system loads on demand through `AsyncOperationHandle`. **Localized values are not available in `Awake`.** Wait for initialization:

```csharp
private IEnumerator Start()
{
    yield return LocalizationSettings.InitializationOperation;
    // localized data is usable from here
}
```

`LocalizationSettings.InitializationOperation` also exposes a `Completed` callback. Change language at runtime with `LocalizationSettings.SelectedLocale`.

Avoid `WaitForCompletion` on localization operations: it has a known interaction with Addressables that throws `OperationException` on some Unity/Addressables/Localization version combinations. Yield, or use the `Completed` callback.

## Pitfalls that silently break translations

**Never concatenate translated fragments.** `"You found " + count + " mushrooms"` is untranslatable — word order, agreement and plural rules differ per language. Use one Smart String entry with a placeholder and let the grammar live in the translation.

**Plurals are not `if (count == 1)`.** Several languages have three to six plural forms. That is what Smart Strings are for.

**Text expands.** German and Finnish commonly run 30–40% longer than English. UI that fits exactly in English will clip. Catch it with pseudo-localization, before translators deliver.

**RTL languages** (Arabic, Hebrew) need a mirrored layout, not just a translated string.

**CJK needs font coverage.** TextMeshPro font assets only contain the glyphs baked into their atlas. Chinese, Japanese and Korean need dynamic font assets or a much larger atlas — a memory and build-size decision, not a detail. Swap fonts per locale with `LocalizedTmpFont`.

**Numbers, dates and currency are cultural.** Decimal separators, digit grouping, date order and currency position all vary. Format through the locale's culture, never by assembling the string by hand.

## Checklist before merging UI work

- No string literal reaches a `Text`, `TMP_Text` or UI Toolkit label
- Entries exist in the String Table Collection, with keys that describe *meaning*, not the English wording
- Dynamic values pass through Smart String arguments, never concatenation
- The screen was checked with pseudo-localization, for overflow and for missed hardcoded strings
- Nothing reads a localized value in `Awake`
- Log and exception messages stayed English
