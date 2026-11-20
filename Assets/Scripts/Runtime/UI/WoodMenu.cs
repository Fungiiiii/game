using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;

namespace Fungiiiii.UI
{
    /// <summary>
    /// Builds the pieces the menus share: a canvas, the title, the wooden plank buttons
    /// and a full-screen veil. Sizes are in canvas units, on a 1920 x 1080 reference.
    /// </summary>
    internal static class WoodMenu
    {
        public static readonly Vector2 ButtonSize = new Vector2(510f, 117f);
        public const float ButtonSpacing = 30f;

        private const int TitleFontSize = 168;
        private const int ButtonFontSize = 48;

        /// <summary>Palette "Body light", the mycelium cream.</summary>
        private static readonly Color32 Cream = new Color32(0xF2, 0xE6, 0xCC, 0xFF);

        /// <summary>The darkest tone of the forest's sky, under the title.</summary>
        private static readonly Color32 TitleShadow = new Color32(0x1E, 0x26, 0x30, 0xFF);

        /// <summary>Palette "Wood dark", under the button labels.</summary>
        private static readonly Color32 LabelShadow = new Color32(0x4F, 0x34, 0x24, 0xFF);

        private static readonly Vector2 ReferenceResolution = new Vector2(1920f, 1080f);

        public static Canvas CreateCanvas(string canvasName, int sortingOrder)
        {
            EnsureEventSystem();

            var canvasObject = new GameObject(canvasName, typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            var canvas = canvasObject.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = sortingOrder;

            var scaler = canvasObject.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = ReferenceResolution;
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = 0.5f;
            return canvas;
        }

        /// <summary>The game's name, top left of <paramref name="parent"/>.</summary>
        public static Text CreateTitle(RectTransform parent, Font font, string entryKey, Vector2 topLeft)
        {
            var title = CreateLabel("Title", parent, font, TitleFontSize, entryKey, TextAnchor.UpperLeft, TitleShadow, 7f);
            var rect = title.rectTransform;
            rect.anchorMin = rect.anchorMax = rect.pivot = new Vector2(0f, 1f);
            rect.anchoredPosition = topLeft;
            rect.sizeDelta = new Vector2(1400f, 210f);
            return title;
        }

        /// <summary>A plank button anchored like <paramref name="anchor"/>, at <paramref name="position"/> from it.</summary>
        public static PlankButton CreateButton(
            RectTransform parent,
            Font font,
            string entryKey,
            Vector2 anchor,
            Vector2 position,
            Vector2 slideWhenHighlighted,
            float endFacetStart,
            UnityAction onClick)
        {
            // The button's own rectangle catches the pointer and never moves, so the plank
            // can slide under it without slipping away from the cursor.
            var buttonObject = new GameObject("Button " + entryKey, typeof(RectTransform), typeof(Image), typeof(Button));
            var rect = (RectTransform)buttonObject.transform;
            rect.SetParent(parent, false);
            rect.anchorMin = rect.anchorMax = rect.pivot = anchor;
            rect.anchoredPosition = position;
            rect.sizeDelta = ButtonSize;

            var hitArea = buttonObject.GetComponent<Image>();
            hitArea.color = Color.clear;

            var faceObject = new GameObject("Face", typeof(RectTransform));
            var face = (RectTransform)faceObject.transform;
            face.SetParent(rect, false);
            Stretch(face);

            var plankObject = new GameObject("Plank", typeof(RectTransform), typeof(CanvasRenderer));
            var plankRect = (RectTransform)plankObject.transform;
            plankRect.SetParent(face, false);
            Stretch(plankRect);
            var plank = plankObject.AddComponent<PlankGraphic>();
            plank.raycastTarget = false;
            plank.EndFacetStart = endFacetStart;

            var label = CreateLabel("Label", face, font, ButtonFontSize, entryKey, TextAnchor.MiddleCenter, LabelShadow, 3f);
            Stretch(label.rectTransform);
            // The underside of the plank is not part of its face: the label sits above it.
            label.rectTransform.offsetMin = new Vector2(0f, 9f);

            var button = buttonObject.GetComponent<Button>();
            button.transition = Selectable.Transition.None;
            button.targetGraphic = hitArea;
            button.onClick.AddListener(onClick);

            var plankButton = buttonObject.AddComponent<PlankButton>();
            plankButton.Initialize(face, plank, label.GetComponent<LocalizedLabel>(), slideWhenHighlighted);
            return plankButton;
        }

        /// <summary>A plain colour over the whole canvas, which does not catch the pointer.</summary>
        public static Image CreateVeil(RectTransform parent, string veilName, Color color)
        {
            var veilObject = new GameObject(veilName, typeof(RectTransform), typeof(Image));
            var rect = (RectTransform)veilObject.transform;
            rect.SetParent(parent, false);
            Stretch(rect);

            var veil = veilObject.GetComponent<Image>();
            veil.color = color;
            veil.raycastTarget = false;
            return veil;
        }

        private static Text CreateLabel(
            string labelName,
            RectTransform parent,
            Font font,
            int fontSize,
            string entryKey,
            TextAnchor alignment,
            Color shadowColor,
            float shadowDepth)
        {
            var labelObject = new GameObject(labelName, typeof(RectTransform));
            labelObject.transform.SetParent(parent, false);

            var label = labelObject.AddComponent<Text>();
            label.font = font;
            label.fontSize = fontSize;
            label.color = Cream;
            label.alignment = alignment;
            label.horizontalOverflow = HorizontalWrapMode.Overflow;
            label.verticalOverflow = VerticalWrapMode.Overflow;
            label.raycastTarget = false;

            var shadow = labelObject.AddComponent<Shadow>();
            shadow.effectColor = shadowColor;
            shadow.effectDistance = new Vector2(0f, -shadowDepth);

            labelObject.AddComponent<LocalizedLabel>().Initialize(entryKey);
            return label;
        }

        private static void Stretch(RectTransform rect)
        {
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
        }

        private static void EnsureEventSystem()
        {
            if (EventSystem.current != null)
            {
                return;
            }

            // Lives in the scene of the menu that needed it, and goes away with it.
            _ = new GameObject("Menu EventSystem", typeof(EventSystem), typeof(InputSystemUIInputModule));
        }
    }
}
