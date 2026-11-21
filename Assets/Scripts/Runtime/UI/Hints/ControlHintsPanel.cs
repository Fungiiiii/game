using System.Collections.Generic;
using Fungiiiii.Controls;
using UnityEngine;
using UnityEngine.InputSystem.DualShock;
using UnityEngine.UI;

namespace Fungiiiii.UI
{
    /// <summary>
    /// The key hints, bottom right of the screen, without a background (artboard « Aides ·
    /// 2 · textes collés »): one line per hint, the control drawn for the device in hand
    /// and, right after it, what it does. The lines come from the sources in charge, in
    /// their order; with too many, optional ones are left out first.
    ///
    /// The panel never changes for a new hint: its source declares it.
    /// </summary>
    internal sealed class ControlHintsPanel : MonoBehaviour
    {
        /// <summary>Over the inventory (100), under the pause menu (200).</summary>
        private const int CanvasSortingOrder = 150;

        public const int MaxHints = 4;

        // Sizes of the artboard, drawn at 1280 x 720, times 1.5 for the 1920 x 1080 canvas.
        private const float RightMargin = 27f;
        private const float BottomMargin = 24f;
        private const float RowSpacing = 12f;
        private const float GlyphLabelGap = 12f;
        private const int LabelFontSize = 26;
        private const float KeyHeight = 42f;
        private const float KeyPadding = 13f;
        private const int KeyFontSize = 22;
        private const float KeyCorner = 7f;
        private const float PillCorner = 12f;
        private const float BandHeight = 4f;
        private static readonly Vector2 MouseSize = new Vector2(33f, 42f);
        private static readonly Vector2 ViewSize = new Vector2(51f, 36f);
        private const int TwiceFontSize = 19;

        // Palette: "Body light", "Wood dark", the shadow brown and the night blue of the halo.
        private static readonly Color Cream = new Color32(0xF2, 0xE6, 0xCC, 0xFF);
        private static readonly Color WoodDark = new Color32(0x4F, 0x34, 0x24, 0xFF);
        private static readonly Color ShadowBrown = new Color32(0x2B, 0x27, 0x23, 0xFF);
        private static readonly Color Halo = new Color(30f / 255f, 38f / 255f, 48f / 255f, 0.9f);

        private readonly List<ControlHint> shown = new List<ControlHint>();
        private readonly List<Glyph> shownGlyphs = new List<Glyph>();
        private IHintSource[] sources = new IHintSource[0];
        private PlayerControls controls;
        private Font font;
        private RectTransform list;

        /// <summary>The hints on screen, in order.</summary>
        public IReadOnlyList<ControlHint> Shown => shown;

        /// <summary>How each hint on screen is drawn, in order.</summary>
        public IReadOnlyList<Glyph> ShownGlyphs => shownGlyphs;

        public void Initialize(Font hintFont, PlayerControls playerControls)
        {
            font = hintFont;
            controls = playerControls;
            controls.SchemeChanged += Rebuild;

            var canvas = WoodMenu.CreateCanvas("Key Hints Canvas", CanvasSortingOrder);
            canvas.transform.SetParent(transform, false);
            // Nothing on it is clicked.
            Destroy(canvas.GetComponent<GraphicRaycaster>());

            list = (RectTransform)new GameObject("Hints", typeof(RectTransform)).transform;
            list.SetParent(canvas.transform, false);
            list.anchorMin = list.anchorMax = list.pivot = new Vector2(1f, 0f);
            list.anchoredPosition = new Vector2(-RightMargin, BottomMargin);
            var layout = list.gameObject.AddComponent<VerticalLayoutGroup>();
            layout.spacing = RowSpacing;
            layout.childAlignment = TextAnchor.LowerLeft;
            layout.childControlWidth = layout.childControlHeight = true;
            layout.childForceExpandWidth = layout.childForceExpandHeight = false;
            var fitter = list.gameObject.AddComponent<ContentSizeFitter>();
            fitter.horizontalFit = fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
        }

        /// <summary>Shows the hints of these sources, in this order; none hides the panel's lines.</summary>
        public void ShowFrom(params IHintSource[] next)
        {
            foreach (var source in sources)
            {
                source.HintsChanged -= Rebuild;
            }

            sources = next ?? new IHintSource[0];
            foreach (var source in sources)
            {
                source.HintsChanged += Rebuild;
            }

            Rebuild();
        }

        private void OnDestroy()
        {
            foreach (var source in sources)
            {
                source.HintsChanged -= Rebuild;
            }

            if (controls != null)
            {
                controls.SchemeChanged -= Rebuild;
            }
        }

        private void Rebuild()
        {
            if (list == null)
            {
                return;
            }

            Compose();
            for (var child = list.childCount - 1; child >= 0; child--)
            {
                Destroy(list.GetChild(child).gameObject);
            }

            var scheme = controls.CurrentScheme;
            var playStation = controls.CurrentDevice is DualShockGamepad;
            shownGlyphs.Clear();
            foreach (var hint in shown)
            {
                var glyph = hint.GlyphFor(scheme, playStation);
                shownGlyphs.Add(glyph);
                CreateRow(glyph, hint.IsTwiceWith(scheme), hint.LabelKey);
            }
        }

        /// <summary>The hints of every source, optional ones left out first while there are too many.</summary>
        private void Compose()
        {
            shown.Clear();
            foreach (var source in sources)
            {
                shown.AddRange(source.Hints);
            }

            for (var index = shown.Count - 1; index >= 0 && shown.Count > MaxHints; index--)
            {
                if (shown[index].IsOptional)
                {
                    shown.RemoveAt(index);
                }
            }

            if (shown.Count > MaxHints)
            {
                shown.RemoveRange(MaxHints, shown.Count - MaxHints);
            }
        }

        private void CreateRow(Glyph glyph, bool twice, string labelKey)
        {
            var row = CreateObject("Hint", list);
            var rowLayout = row.gameObject.AddComponent<HorizontalLayoutGroup>();
            rowLayout.spacing = GlyphLabelGap;
            rowLayout.childAlignment = TextAnchor.MiddleLeft;
            rowLayout.childControlWidth = rowLayout.childControlHeight = true;
            rowLayout.childForceExpandWidth = rowLayout.childForceExpandHeight = false;

            var glyphs = CreateObject("Control", row);
            var glyphLayout = glyphs.gameObject.AddComponent<HorizontalLayoutGroup>();
            glyphLayout.spacing = 6f;
            glyphLayout.childAlignment = TextAnchor.MiddleLeft;
            glyphLayout.childControlWidth = glyphLayout.childControlHeight = true;
            glyphLayout.childForceExpandWidth = glyphLayout.childForceExpandHeight = false;
            CreateGlyph(glyph, glyphs);
            if (twice)
            {
                CreateTwice(glyphs);
            }

            var label = CreateObject("Label", row).gameObject.AddComponent<Text>();
            label.font = font;
            label.fontSize = LabelFontSize;
            label.color = Cream;
            label.alignment = TextAnchor.MiddleLeft;
            label.horizontalOverflow = HorizontalWrapMode.Overflow;
            label.raycastTarget = false;
            label.gameObject.AddComponent<LocalizedLabel>().Initialize(labelKey);
            AddReadableEdge(label.gameObject);
        }

        private void CreateGlyph(Glyph glyph, RectTransform parent)
        {
            switch (glyph.Kind)
            {
                case GlyphKind.MouseLeft:
                    CreateShape(parent, GlyphShapeGraphic.Shape.MouseLeft, MouseSize);
                    break;
                case GlyphKind.MouseRight:
                    CreateShape(parent, GlyphShapeGraphic.Shape.MouseRight, MouseSize);
                    break;
                case GlyphKind.MouseWheel:
                    CreateShape(parent, GlyphShapeGraphic.Shape.MouseWheel, MouseSize);
                    break;
                case GlyphKind.Face:
                    CreateFace(glyph, parent);
                    break;
                case GlyphKind.View:
                    var view = CreateWood(parent, PillCorner, new Vector2(ViewSize.x, ViewSize.y));
                    CreateShape(view, GlyphShapeGraphic.Shape.View, new Vector2(27f, 21f));
                    break;
                default:
                    CreateKey(glyph.Text, parent, glyph.Kind == GlyphKind.Pill ? PillCorner : KeyCorner);
                    break;
            }
        }

        /// <summary>A wooden key with its name, as wide as the name needs.</summary>
        private void CreateKey(string text, RectTransform parent, float corner)
        {
            var key = CreateWood(parent, corner, new Vector2(KeyHeight, KeyHeight));
            var layout = key.gameObject.AddComponent<HorizontalLayoutGroup>();
            layout.padding = new RectOffset((int)KeyPadding, (int)KeyPadding, 0, (int)BandHeight);
            layout.childAlignment = TextAnchor.MiddleCenter;
            layout.childControlWidth = layout.childControlHeight = true;
            layout.childForceExpandWidth = layout.childForceExpandHeight = false;
            CreateKeyText(key, text, KeyFontSize);
        }

        /// <summary>An octagonal pad button with its letter, or its PlayStation shape.</summary>
        private void CreateFace(Glyph glyph, RectTransform parent)
        {
            var face = CreateWood(parent, KeyHeight * 0.3f, new Vector2(KeyHeight, KeyHeight));
            switch (glyph.Face)
            {
                case FaceShape.Cross:
                    CreateShape(face, GlyphShapeGraphic.Shape.Cross, new Vector2(KeyHeight, KeyHeight)).ignoreLayout = true;
                    break;
                case FaceShape.Circle:
                    CreateShape(face, GlyphShapeGraphic.Shape.Circle, new Vector2(KeyHeight, KeyHeight)).ignoreLayout = true;
                    break;
                case FaceShape.Square:
                    CreateShape(face, GlyphShapeGraphic.Shape.Square, new Vector2(KeyHeight, KeyHeight)).ignoreLayout = true;
                    break;
                case FaceShape.Triangle:
                    var triangle = CreateObject("Triangle", face);
                    triangle.sizeDelta = Vector2.one * (KeyHeight * 0.42f);
                    triangle.gameObject.AddComponent<TriangleGlyphGraphic>().color = Cream;
                    break;
                default:
                    var letter = CreateKeyText(face, glyph.Text, KeyFontSize);
                    var rect = letter.rectTransform;
                    rect.anchorMin = Vector2.zero;
                    rect.anchorMax = Vector2.one;
                    rect.offsetMin = new Vector2(0f, BandHeight);
                    rect.offsetMax = Vector2.zero;
                    break;
            }
        }

        /// <summary>The ×2 of a double click, after the mouse.</summary>
        private void CreateTwice(RectTransform parent)
        {
            var badge = CreateObject("Twice", parent);
            var graphic = badge.gameObject.AddComponent<CutCornerGraphic>();
            graphic.SetShape(6f, ShadowBrown);
            graphic.raycastTarget = false;
            var layout = badge.gameObject.AddComponent<HorizontalLayoutGroup>();
            layout.padding = new RectOffset(8, 8, 0, 3);
            layout.childAlignment = TextAnchor.MiddleCenter;
            layout.childControlWidth = layout.childControlHeight = true;
            layout.childForceExpandWidth = layout.childForceExpandHeight = false;
            badge.gameObject.AddComponent<LayoutElement>().minHeight = 30f;
            CreateKeyText(badge, "×2", TwiceFontSize);
        }

        private static RectTransform CreateWood(RectTransform parent, float corner, Vector2 minSize)
        {
            var wood = CreateObject("Key", parent);
            var graphic = wood.gameObject.AddComponent<CutCornerGraphic>();
            graphic.SetShape(corner, WoodDark);
            graphic.SetBands(Color.clear, 0f, ShadowBrown, BandHeight);
            graphic.raycastTarget = false;
            var size = wood.gameObject.AddComponent<LayoutElement>();
            size.minWidth = minSize.x;
            size.minHeight = size.preferredHeight = minSize.y;
            return wood;
        }

        private static LayoutElement CreateShape(RectTransform parent, GlyphShapeGraphic.Shape shape, Vector2 size)
        {
            var shapeObject = CreateObject("Shape", parent);
            shapeObject.sizeDelta = size;
            var graphic = shapeObject.gameObject.AddComponent<GlyphShapeGraphic>();
            graphic.Drawn = shape;
            graphic.raycastTarget = false;
            var element = shapeObject.gameObject.AddComponent<LayoutElement>();
            element.minWidth = element.preferredWidth = size.x;
            element.minHeight = element.preferredHeight = size.y;
            return element;
        }

        private Text CreateKeyText(RectTransform parent, string text, int fontSize)
        {
            var label = CreateObject("Text", parent).gameObject.AddComponent<Text>();
            label.font = font;
            label.fontSize = fontSize;
            label.color = Cream;
            label.alignment = TextAnchor.MiddleCenter;
            label.horizontalOverflow = HorizontalWrapMode.Overflow;
            label.raycastTarget = false;
            label.text = text;
            return label;
        }

        /// <summary>A dark edge and halo, so the text reads over the forest without a background.</summary>
        private static void AddReadableEdge(GameObject textObject)
        {
            var edge = textObject.AddComponent<Outline>();
            edge.effectColor = ShadowBrown;
            edge.effectDistance = new Vector2(1.5f, -1.5f);
            var halo = textObject.AddComponent<UnityEngine.UI.Shadow>();
            halo.effectColor = Halo;
            halo.effectDistance = new Vector2(0f, -2f);
        }

        private static RectTransform CreateObject(string objectName, Transform parent)
        {
            var created = new GameObject(objectName, typeof(RectTransform));
            var rect = (RectTransform)created.transform;
            rect.SetParent(parent, false);
            return rect;
        }
    }
}
