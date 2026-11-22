using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.UI;

namespace Fungiiiii.UI
{
    /// <summary>
    /// Shows one entry of the "UI" string table in a label, in the current language, and
    /// again whenever the language changes. The entry is set in the Inspector, or by the
    /// owner of a label made at runtime. An entry with placeholders (<c>{0} in the
    /// backpack</c>) is given its values with <see cref="Show(string, object[])"/>: the
    /// sentence stays whole in the table, never assembled in code.
    /// </summary>
    [RequireComponent(typeof(Text))]
    internal sealed class LocalizedLabel : MonoBehaviour
    {
        [Tooltip("Key of the entry in the \"UI\" string table.")]
        [SerializeField]
        private string entryKey;

        private LocalizedString localized;
        private object[] arguments;

        /// <summary>The key of the entry shown.</summary>
        public string EntryKey => entryKey;

        /// <summary>Shows another entry of the table.</summary>
        public void Show(string key)
        {
            Show(key, null);
        }

        /// <summary>Shows an entry whose placeholders take <paramref name="values"/>; the same entry with other values is only refreshed.</summary>
        public void Show(string key, params object[] values)
        {
            arguments = values;
            if (key == entryKey && localized != null)
            {
                localized.Arguments = arguments;
                localized.RefreshString();
                return;
            }

            StopListening();
            entryKey = key;
            if (isActiveAndEnabled)
            {
                Listen();
            }
        }

        private void OnEnable()
        {
            Listen();
        }

        private void OnDisable()
        {
            StopListening();
        }

        private void Listen()
        {
            if (string.IsNullOrEmpty(entryKey))
            {
                return;
            }

            localized = new LocalizedString(UiText.Table, entryKey) { Arguments = arguments };
            localized.StringChanged += Apply;
        }

        private void StopListening()
        {
            if (localized != null)
            {
                localized.StringChanged -= Apply;
                localized = null;
            }
        }

        private void Apply(string text)
        {
            GetComponent<Text>().text = text;
        }
    }
}
