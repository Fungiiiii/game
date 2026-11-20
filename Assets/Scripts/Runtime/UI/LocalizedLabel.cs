using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.UI;

namespace Fungiiiii.UI
{
    /// <summary>
    /// Shows one entry of the "UI" string table in a label, in the current language, and
    /// again whenever the language changes.
    /// </summary>
    [RequireComponent(typeof(Text))]
    internal sealed class LocalizedLabel : MonoBehaviour
    {
        private LocalizedString localized;
        private Text label;

        /// <summary>The key of the entry shown, for tests.</summary>
        public string EntryKey { get; private set; }

        public void Initialize(string entryKey)
        {
            label = GetComponent<Text>();
            EntryKey = entryKey;
            localized = new LocalizedString(UiText.Table, entryKey);

            // OnEnable ran before the entry was known, if the label is already shown.
            if (isActiveAndEnabled)
            {
                localized.StringChanged += Apply;
            }
        }

        private void OnEnable()
        {
            if (localized != null)
            {
                localized.StringChanged += Apply;
            }
        }

        private void OnDisable()
        {
            if (localized != null)
            {
                localized.StringChanged -= Apply;
            }
        }

        private void Apply(string text)
        {
            label.text = text;
        }
    }
}
