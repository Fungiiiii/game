using UnityEngine;

namespace Fungiiiii.Outlines
{
    /// <summary>The outline around what can be gathered or aimed at, as data.</summary>
    [CreateAssetMenu(fileName = "OutlineSettings", menuName = "Fungiiiii/Outline Settings")]
    public sealed class OutlineSettings : ScriptableObject
    {
        [Tooltip("Shader of the outline (Fungiiiii/ItemOutline).")]
        [SerializeField]
        private Shader shader;

        [Tooltip("Palette Body light, a mycelium cream.")]
        [SerializeField]
        private Color color = new Color32(0xF2, 0xE6, 0xCC, 0xFF);

        [Tooltip("Width in pixels on a 1080p screen.")]
        [SerializeField, Range(0f, 8f)]
        private float width = 2.5f;

        [Tooltip("Cyan of what is aimed at, as on the Ramassage artboard.")]
        [SerializeField]
        private Color targetedColor = new Color32(0x6F, 0xD6, 0xEA, 0xFF);

        [SerializeField, Range(0f, 8f)]
        private float targetedWidth = 4f;

        [Tooltip("The outline is at full strength up to the first distance and gone at the second, in metres.")]
        [SerializeField]
        private Vector2 fade = new Vector2(16f, 30f);

        public Shader Shader => shader;

        internal ItemOutlineFactory.Settings ToFactorySettings()
        {
            return new ItemOutlineFactory.Settings(color, width, targetedColor, targetedWidth, fade.x, Mathf.Max(fade.x, fade.y));
        }
    }
}
