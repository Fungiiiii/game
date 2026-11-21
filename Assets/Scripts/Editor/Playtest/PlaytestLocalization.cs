using System.IO;
using Fungiiiii.UI;
using UnityEditor;
using UnityEditor.Localization;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;
using UnityEngine.Localization.Tables;

namespace Fungiiiii.Editor.Playtest
{
    /// <summary>
    /// Creates what the Localization package needs for the menus: its settings, the
    /// French and English locales, and the "UI" string table with the menus' entries.
    ///
    /// It only adds what is missing, and removes the entries it once added that the game
    /// no longer uses. A text already in the table is never overwritten: once created, the
    /// table is the source of truth, edited in Window > Asset Management > Localization
    /// Tables.
    /// </summary>
    internal static class PlaytestLocalization
    {
        private const string Folder = "Assets/Localization";
        private const string LocalesFolder = Folder + "/Locales";
        private const string TablesFolder = Folder + "/Tables";
        private const string SettingsPath = Folder + "/LocalizationSettings.asset";

        private const string FrenchCode = "fr";
        private const string EnglishCode = "en";

        /// <summary>First texts of the entries, used only when an entry does not exist yet.</summary>
        private static readonly (string Key, string French, string English)[] Entries =
        {
            (UiText.GameTitle, "Fungiiiii", "Fungiiiii"),
            (UiText.MenuPlay, "Jouer", "Play"),
            (UiText.MenuQuit, "Quitter", "Quit"),
            (UiText.PauseResume, "Reprendre", "Resume"),
            (UiText.PauseHome, "Accueil", "Home"),
            (UiText.PauseQuit, "Quitter", "Quit"),
            (UiText.InventoryTitle, "Inventaire", "Inventory"),
            (UiText.InventoryHotbar, "Barre rapide", "Quick bar"),
            (UiText.HintThrowNet, "Lancer le filet", "Throw the net"),
            (UiText.HintAim, "Viser", "Aim"),
            (UiText.HintThrow, "Lancer", "Throw"),
            (UiText.HintStopAiming, "Relâcher : ne plus viser", "Release: stop aiming"),
            (UiText.HintSwitchItem, "Changer d’objet", "Switch item"),
            (UiText.HintInventory, "Inventaire", "Inventory"),
            (UiText.HintGather, "Ramasser", "Gather"),
            (UiText.HintDeposit, "Déposer", "Deposit"),
            (UiText.HintTakeOrPlace, "Prendre ou poser", "Take or place"),
            (UiText.HintHalfOrOne, "La moitié, ou un seul", "Half, or just one"),
            (UiText.HintGatherAll, "Tout ramasser", "Gather all"),
            (UiText.HintClose, "Fermer", "Close"),
            (UiText.HintDrop, "Jeter", "Drop"),

            // Item names, keys of the items' definitions (nameKey).
            ("item.wood", "Bâton", "Stick"),
            ("item.spore", "Champignon", "Mushroom"),
            ("item.net", "Filet", "Net"),
            ("item.spongeus", "Spongeus", "Spongeus"),
            ("item.champoule", "Champoule", "Champoule"),
        };

        /// <summary>
        /// Entries this script once added and the game no longer uses: the old line of hints
        /// of the inventory, replaced by the key hints. Removed, so the table keeps only texts
        /// the game shows.
        /// </summary>
        private static readonly string[] RetiredKeys =
        {
            "inventory.hint.left.key",
            "inventory.hint.left",
            "inventory.hint.right.key",
            "inventory.hint.right",
            "inventory.hint.double.key",
            "inventory.hint.double",
            "inventory.hint.close.key",
            "inventory.hint.close",
        };

        [MenuItem("Fungiiiii/Playtest/1. Set Up Localization")]
        internal static void SetUp()
        {
            Directory.CreateDirectory(LocalesFolder);
            Directory.CreateDirectory(TablesFolder);
            AssetDatabase.Refresh();

            var settings = LocalizationEditorSettings.ActiveLocalizationSettings;
            if (settings == null)
            {
                settings = ScriptableObject.CreateInstance<LocalizationSettings>();
                settings.name = "Localization Settings";
                AssetDatabase.CreateAsset(settings, SettingsPath);
                LocalizationEditorSettings.ActiveLocalizationSettings = settings;
            }

            var french = EnsureLocale(FrenchCode, "French (fr)");
            var english = EnsureLocale(EnglishCode, "English (en)");

            // The language of the system is used when it is French or English. Anything
            // else falls back to French: the team and its testers are French.
            foreach (var selector in settings.GetStartupLocaleSelectors())
            {
                if (selector is SpecificLocaleSelector fallback)
                {
                    fallback.LocaleId = french.Identifier;
                }
            }

            EditorUtility.SetDirty(settings);

            var collection = LocalizationEditorSettings.GetStringTableCollection(UiText.Table);
            if (collection == null)
            {
                collection = LocalizationEditorSettings.CreateStringTableCollection(UiText.Table, TablesFolder);
            }

            var frenchTable = TableOf(collection, french);
            var englishTable = TableOf(collection, english);

            foreach (var (key, frenchText, englishText) in Entries)
            {
                AddIfMissing(frenchTable, key, frenchText);
                AddIfMissing(englishTable, key, englishText);
            }

            foreach (var key in RetiredKeys)
            {
                collection.RemoveEntry(key);
            }

            EditorUtility.SetDirty(collection.SharedData);
            EditorUtility.SetDirty(frenchTable);
            EditorUtility.SetDirty(englishTable);
            AssetDatabase.SaveAssets();
        }

        /// <param name="assetName">Name of the locale's asset, as the Localization window would name it.</param>
        private static Locale EnsureLocale(string code, string assetName)
        {
            var identifier = new LocaleIdentifier(code);
            var locale = LocalizationEditorSettings.GetLocale(identifier);
            if (locale != null)
            {
                return locale;
            }

            locale = Locale.CreateLocale(identifier);
            locale.name = assetName;
            AssetDatabase.CreateAsset(locale, $"{LocalesFolder}/{assetName}.asset");
            LocalizationEditorSettings.AddLocale(locale);
            return locale;
        }

        private static StringTable TableOf(StringTableCollection collection, Locale locale)
        {
            var table = collection.GetTable(locale.Identifier) as StringTable;
            if (table == null)
            {
                table = collection.AddNewTable(locale.Identifier) as StringTable;
            }

            return table;
        }

        private static void AddIfMissing(StringTable table, string key, string text)
        {
            if (table.GetEntry(key) == null)
            {
                table.AddEntry(key, text);
            }
        }
    }
}
