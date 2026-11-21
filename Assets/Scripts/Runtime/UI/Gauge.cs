using UnityEngine;

namespace Fungiiiii.UI
{
    /// <summary>A horizontal gauge: its fill covers the share of the track that is full.</summary>
    internal sealed class Gauge : MonoBehaviour
    {
        [Tooltip("Stretched over the track; its right edge moves with the value.")]
        [SerializeField]
        private RectTransform fill;

        /// <summary>How full the gauge is, from 0 to 1.</summary>
        public float Value => fill.anchorMax.x;

        public bool IsVisible => gameObject.activeSelf;

        public void Show(float normalized)
        {
            fill.anchorMin = Vector2.zero;
            fill.anchorMax = new Vector2(Mathf.Clamp01(normalized), 1f);
            fill.offsetMin = Vector2.zero;
            fill.offsetMax = Vector2.zero;
        }

        public void SetVisible(bool visible)
        {
            gameObject.SetActive(visible);
        }
    }
}
