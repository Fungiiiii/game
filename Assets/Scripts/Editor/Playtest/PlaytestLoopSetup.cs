using System;
using Fungiiiii.Champimaison;
using Fungiiiii.Combat;
using Fungiiiii.Crafting;
using Fungiiiii.Game;
using Fungiiiii.Gathering;
using Fungiiiii.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace Fungiiiii.Editor.Playtest
{
    /// <summary>
    /// ONE-OFF (CLAUDE.md, "Scenes, prefabs and data"): writes the prefabs and the scene
    /// wiring of the playtest loop, which was coded without an Editor. Run it once from
    /// Fungiiiii/Playtest, review what it wrote, commit it, then delete this file.
    ///
    /// It writes: the mole hole and pebble placeholders; the floating key in the style of the
    /// wooden panels design system (triangle for a press, border for a hold, texts on a
    /// small wooden panel); the requirement panel, crafting panel, objective and
    /// end-of-demo prefabs; the crafting panel inside the inventory;
    /// the slingshot on the playtest player; the Game scene's new references; the
    /// Champimaison's interior scene and the build scenes.
    /// </summary>
    internal static class PlaytestLoopSetup
    {
        private const string FontPath = "Assets/Art/Fonts/Fredoka_SemiBold.ttf";
        private const string UiFolder = "Assets/Prefabs/UI";
        private const string MoleHolePath = "Assets/Prefabs/Environment/PF_MoleHole.prefab";
        private const string PebblePath = "Assets/Prefabs/Props/PF_Pebble.prefab";
        private const string EarthMaterialPath = "Assets/Art/Materials/PlaceholderEarth.mat";
        private const string HoleMaterialPath = "Assets/Art/Materials/PlaceholderHole.mat";
        private const string StoneMaterialPath = "Assets/Art/Materials/PlaceholderStone.mat";
        private const string InventoryPath = UiFolder + "/Inventory.prefab";
        private const string KeyPromptPath = UiFolder + "/KeyPrompt.prefab";
        private const string ControlHintsPath = UiFolder + "/ControlHints.prefab";
        private const string PlankButtonPath = UiFolder + "/PlankButton.prefab";
        private const string PlayerPath = "Assets/Prefabs/Characters/PlaytestPlayer.prefab";
        private const string PebblesPath = "Assets/Data/Gathering/Pebbles.asset";
        private const string RecipeBookPath = "Assets/Data/Crafting/RecipeBook.asset";

        private static readonly Color Cream = new Color32(0xF2, 0xE6, 0xCC, 0xFF);
        private static readonly Color Sand = new Color32(0xE0, 0xCB, 0xA4, 0xFF);
        private static readonly Color Wood = new Color32(0x4F, 0x34, 0x24, 0xFF);
        private static readonly Color WoodLight = new Color32(0xA8, 0x90, 0x78, 0xFF);
        private static readonly Color WoodDark = new Color32(0x2B, 0x27, 0x23, 0xFF);
        private static readonly Color Cyan = new Color32(0x6F, 0xD6, 0xEA, 0xFF);
        private static readonly Color Missing = new Color32(0xF0, 0x8A, 0x74, 0xFF);
        private static readonly Color Muted = new Color32(0xCD, 0xB8, 0x96, 0xFF);
        private static readonly Color Separator = new Color(0xE0 / 255f, 0xCB / 255f, 0xA4 / 255f, 0.35f);
        private static readonly Color Facet = new Color(0x2B / 255f, 0x27 / 255f, 0x23 / 255f, 0.28f);
        private static readonly Color Bg = new Color32(0x1E, 0x26, 0x30, 0xFF);

        private static Font font;

        [MenuItem("Fungiiiii/Playtest/One-off: Set Up the Playtest Loop")]
        internal static void SetUp()
        {
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
            {
                return;
            }

            font = AssetDatabase.LoadAssetAtPath<Font>(FontPath) ?? throw new InvalidOperationException($"No font at {FontPath}.");
            EnsureFolder("Assets/Prefabs", "Props");

            var moleHole = BuildMoleHole();
            var pebble = BuildPebble();
            SetArray(AssetDatabase.LoadAssetAtPath<GatherableDefinition>(PebblesPath), "variants", pebble);

            var key = RestyleKeyPrompt();
            var requirementPanel = BuildRequirementPanel(key);
            var objectiveHud = BuildObjectiveHud();
            var demoEnd = BuildDemoEnd();
            AddCraftingPanel();
            AddSlingshotToPlayer();

            WireGameScene(moleHole, requirementPanel, objectiveHud);
            BuildInterior(demoEnd);
            PlaytestSetup.SetBuildScenes();

            AssetDatabase.SaveAssets();
            Debug.Log("Playtest loop set up. Review the changes, commit them, then delete PlaytestLoopSetup.cs.");
        }

        // ---------------------------------------------------------------- world placeholders

        /// <summary>Earth cubes in a ring, half in the ground, round a small hole going down a few centimetres.</summary>
        private static GameObject BuildMoleHole()
        {
            var earth = PlaceholderMaterial(EarthMaterialPath, new Color32(0x6B, 0x4A, 0x35, 0xFF), 0.1f);
            var hole = PlaceholderMaterial(HoleMaterialPath, new Color32(0x14, 0x10, 0x0C, 0xFF), 0f);
            var root = new GameObject("PF_MoleHole");
            const int cubes = 9;
            for (var index = 0; index < cubes; index++)
            {
                var angle = index * Mathf.PI * 2f / cubes;
                var size = 0.14f + 0.05f * Mathf.Sin(index * 2.3f);
                var cube = Primitive(PrimitiveType.Cube, "Earth", root.transform, earth);
                cube.transform.localPosition = new Vector3(Mathf.Cos(angle) * 0.3f, size * 0.15f, Mathf.Sin(angle) * 0.3f);
                cube.transform.localRotation = Quaternion.Euler(12f * Mathf.Sin(index), angle * Mathf.Rad2Deg + 20f * index, 9f * Mathf.Cos(index));
                cube.transform.localScale = new Vector3(size, size * 0.8f, size);
            }

            // The hole: a dark cylinder whose top sits just under the ground, going down 6 cm.
            var shaft = Primitive(PrimitiveType.Cylinder, "Hole", root.transform, hole);
            shaft.transform.localPosition = new Vector3(0f, -0.025f, 0f);
            shaft.transform.localScale = new Vector3(0.22f, 0.03f, 0.22f);
            return SavePrefab(root, MoleHolePath);
        }

        private static GameObject BuildPebble()
        {
            var stone = PlaceholderMaterial(StoneMaterialPath, new Color32(0x7D, 0x83, 0x8B, 0xFF), 0.2f);
            var root = new GameObject("PF_Pebble");
            var cube = Primitive(PrimitiveType.Cube, "Pebble", root.transform, stone);
            cube.transform.localPosition = new Vector3(0f, 0.08f, 0f);
            cube.transform.localRotation = Quaternion.Euler(10f, 25f, 6f);
            cube.transform.localScale = new Vector3(0.18f, 0.14f, 0.16f);
            return SavePrefab(root, PebblePath);
        }

        private static GameObject Primitive(PrimitiveType type, string name, Transform parent, Material material)
        {
            var primitive = GameObject.CreatePrimitive(type);
            primitive.name = name;
            Object.DestroyImmediate(primitive.GetComponent<Collider>());
            primitive.transform.SetParent(parent, false);
            primitive.GetComponent<MeshRenderer>().sharedMaterial = material;
            return primitive;
        }

        private static Material PlaceholderMaterial(string path, Color color, float smoothness)
        {
            var existing = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (existing != null)
            {
                return existing;
            }

            var material = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            material.SetColor("_BaseColor", color);
            material.SetFloat("_Smoothness", smoothness);
            AssetDatabase.CreateAsset(material, path);
            return material;
        }

        // ---------------------------------------------------------------- the floating key

        /// <summary>
        /// The wooden panels design system on the floating key: the triangle of a short press
        /// over the key's top edge, the border of a hold close around it, the short text on a
        /// small wooden panel with its pointer. Returns the key, which the panels copy.
        /// </summary>
        private static GameObject RestyleKeyPrompt()
        {
            var root = PrefabUtility.LoadPrefabContents(KeyPromptPath);
            try
            {
                var prompt = root.GetComponent<KeyPrompt>();
                var key = FindDeep(root.transform, "Key") ?? throw new InvalidOperationException("The KeyPrompt prefab has no Key.");
                var border = FindDeep(root.transform, "HoldRing") ?? FindDeep(root.transform, "HoldBorder")
                    ?? throw new InvalidOperationException("The KeyPrompt prefab has no HoldRing.");
                border.name = "HoldBorder";
                var borderGraphic = border.GetComponent<HoldBorderGraphic>();
                var borderRect = (RectTransform)border;
                borderRect.anchorMin = borderRect.anchorMax = borderRect.pivot = new Vector2(0.5f, 0.5f);
                borderRect.anchoredPosition = Vector2.zero;
                // 1 unit of space, then 4 of border, round the 44 unit key.
                borderRect.sizeDelta = new Vector2(54f, 54f);
                var serialized = new SerializedObject(borderGraphic);
                Find(serialized, "thickness").floatValue = 4f;
                Find(serialized, "corner").floatValue = 9f;
                serialized.ApplyModifiedPropertiesWithoutUndo();

                var existingMark = FindDeep(key, "PressMark");
                if (existingMark != null)
                {
                    Object.DestroyImmediate(existingMark.gameObject);
                }

                var mark = UiObject("PressMark", key);
                var markRect = (RectTransform)mark.transform;
                markRect.anchorMin = markRect.anchorMax = markRect.pivot = new Vector2(0.5f, 1f);
                // Over the key's top edge: a third of it above the key, two thirds on it.
                markRect.anchoredPosition = new Vector2(0f, 7f);
                markRect.sizeDelta = new Vector2(19f, 12f);
                var markGraphic = mark.AddComponent<TriangleMarkGraphic>();
                markGraphic.color = Cyan;
                markGraphic.raycastTarget = false;
                mark.transform.SetAsLastSibling();

                var text = FindDeep(root.transform, "Text") ?? throw new InvalidOperationException("The KeyPrompt prefab has no Text.");
                var textPanel = FindDeep(root.transform, "TextPanel")?.gameObject;
                if (textPanel == null)
                {
                    textPanel = UiObject("TextPanel", root.transform);
                    textPanel.transform.SetSiblingIndex(text.GetSiblingIndex());
                    WoodPanel(textPanel);
                    var layout = Horizontal(textPanel, 0f, new RectOffset(18, 18, 14, 16));
                    layout.childAlignment = TextAnchor.MiddleCenter;
                    var fitter = textPanel.AddComponent<ContentSizeFitter>();
                    fitter.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
                    fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
                    text.SetParent(textPanel.transform, false);
                    text.GetComponent<Text>().horizontalOverflow = HorizontalWrapMode.Overflow;
                    Pointer(textPanel.transform);
                }

                SetRef(prompt, "pressMark", markGraphic);
                SetRef(prompt, "holdBorder", borderGraphic);
                SetRef(prompt, "textPanel", textPanel.transform);
                PrefabUtility.SaveAsPrefabAsset(root, KeyPromptPath);
            }
            finally
            {
                PrefabUtility.UnloadPrefabContents(root);
            }

            return AssetDatabase.LoadAssetAtPath<GameObject>(KeyPromptPath).transform.Find("Key").gameObject;
        }

        /// <summary>The small triangle under a wooden panel, pointing down at what it is about.</summary>
        private static void Pointer(Transform panel)
        {
            var tail = UiObject("Pointer", panel);
            tail.AddComponent<LayoutElement>().ignoreLayout = true;
            var rect = (RectTransform)tail.transform;
            rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 0f);
            rect.pivot = new Vector2(0.5f, 1f);
            rect.anchoredPosition = Vector2.zero;
            rect.sizeDelta = new Vector2(24f, 12f);
            var graphic = tail.AddComponent<TriangleMarkGraphic>();
            graphic.color = WoodDark;
            graphic.raycastTarget = false;
        }

        // ---------------------------------------------------------------- the Champimaison's panel

        private static RequirementPanel BuildRequirementPanel(GameObject keyModel)
        {
            var row = UiObject("RequirementRow", null);
            Size(row, 316f, 40f);
            var rowLayout = Horizontal(row, 12f, new RectOffset(0, 0, 0, 0));
            rowLayout.childAlignment = TextAnchor.MiddleLeft;
            row.AddComponent<LayoutElement>().preferredHeight = 40f;
            var icon = IconWell(row.transform, 40f);
            var name = Label(row.transform, "Name", 18, Cream, TextAnchor.MiddleLeft, null);
            name.gameObject.AddComponent<LayoutElement>().flexibleWidth = 1f;
            var carried = Label(row.transform, "Carried", 14, Muted, TextAnchor.MiddleRight, null);
            carried.gameObject.AddComponent<LayoutElement>().preferredWidth = 96f;
            var count = NewText(row.transform, "Count", 18, Cream, TextAnchor.MiddleRight);
            count.gameObject.AddComponent<LayoutElement>().preferredWidth = 44f;
            var tick = TickSlot(row.transform, 22f);
            var rowView = row.AddComponent<RequirementRow>();
            SetRef(rowView, "icon", icon);
            SetRef(rowView, "itemName", name);
            SetRef(rowView, "count", count);
            SetRef(rowView, "carried", carried);
            SetRef(rowView, "tick", tick);
            var rowPrefab = SavePrefab(row, UiFolder + "/RequirementRow.prefab").GetComponent<RequirementRow>();

            var root = CanvasRoot("RequirementPanel", 35);
            var panel = UiObject("Panel", root.transform);
            var rect = (RectTransform)panel.transform;
            rect.anchorMin = rect.anchorMax = Vector2.zero;
            // Placed by its bottom edge, over the Champimaison; its pointer hangs under it.
            rect.pivot = new Vector2(0.5f, 0f);
            rect.sizeDelta = new Vector2(360f, 0f);
            WoodPanel(panel);
            var layout = Vertical(panel, 10f, new RectOffset(22, 22, 22, 24));
            layout.childControlHeight = true;
            panel.AddComponent<ContentSizeFitter>().verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            var header = UiObject("Header", panel.transform);
            Horizontal(header, 8f, new RectOffset(0, 0, 0, 0)).childAlignment = TextAnchor.LowerLeft;
            var title = Label(header.transform, "Title", 22, Cream, TextAnchor.LowerLeft, UiText.PanelAwakenTitle);
            title.gameObject.AddComponent<LayoutElement>().flexibleWidth = 1f;
            var stage = NewText(header.transform, "Stage", 14, Muted, TextAnchor.LowerRight);
            stage.gameObject.AddComponent<LayoutElement>().preferredWidth = 40f;
            var costLine = Dashes(panel.transform, "CostLine");
            var rows = UiObject("Rows", panel.transform);
            Vertical(rows, 8f, new RectOffset(0, 0, 0, 0));
            var footerLine = Dashes(panel.transform, "FooterLine");
            var footer = UiObject("Footer", panel.transform);
            Vertical(footer, 10f, new RectOffset(0, 0, 0, 0));
            var pressLine = BuildActionLine(footer.transform, "PressLine", keyModel);
            var holdLine = BuildActionLine(footer.transform, "HoldLine", keyModel);
            Pointer(panel.transform);

            var view = root.AddComponent<RequirementPanel>();
            SetRef(view, "panel", rect);
            SetArray(view, "costParts", header, costLine, rows);
            SetRef(view, "title", title);
            SetRef(view, "stage", stage);
            SetRef(view, "rowPrefab", rowPrefab);
            SetRef(view, "rows", rows.transform);
            SetArray(view, "footerParts", footerLine, footer);
            SetRef(view, "pressLine", pressLine);
            SetRef(view, "holdLine", holdLine);
            return SavePrefab(root, UiFolder + "/RequirementPanel.prefab").GetComponent<RequirementPanel>();
        }

        /// <summary>
        /// A key and what it does, at the foot of a panel: a copy of the floating key, shrunk
        /// to the panel's 32 unit keys, so that every device's control is drawn the same way.
        /// </summary>
        private static ActionLine BuildActionLine(Transform parent, string name, GameObject keyModel)
        {
            var line = UiObject(name, parent);
            var layout = Horizontal(line, 8f, new RectOffset(0, 0, 0, 0));
            layout.childAlignment = TextAnchor.MiddleLeft;
            line.AddComponent<LayoutElement>().preferredHeight = 44f;

            var slot = UiObject("Key", line.transform);
            var slotElement = slot.AddComponent<LayoutElement>();
            slotElement.preferredWidth = 44f;
            slotElement.preferredHeight = 44f;
            var key = Object.Instantiate(keyModel, slot.transform);
            key.name = "KeyCap";
            key.SetActive(true);
            var keyRect = (RectTransform)key.transform;
            keyRect.anchorMin = keyRect.anchorMax = keyRect.pivot = new Vector2(0.5f, 0.5f);
            keyRect.anchoredPosition = Vector2.zero;
            keyRect.localScale = Vector3.one * (32f / 44f);

            var holdWord = Label(line.transform, "HoldWord", 14, Muted, TextAnchor.MiddleLeft, UiText.PanelHold);
            holdWord.gameObject.AddComponent<LayoutElement>().preferredWidth = 74f;
            var action = Label(line.transform, "Action", 18, Cream, TextAnchor.MiddleLeft, UiText.HintDeposit);
            action.gameObject.AddComponent<LayoutElement>().flexibleWidth = 1f;

            var view = line.AddComponent<ActionLine>();
            SetRef(view, "glyph", key.GetComponent<GlyphView>());
            SetRef(view, "pressMark", key.GetComponentInChildren<TriangleMarkGraphic>(true));
            SetRef(view, "holdBorder", key.GetComponentInChildren<HoldBorderGraphic>(true));
            SetRef(view, "holdWord", holdWord);
            SetRef(view, "action", action);
            return view;
        }

        /// <summary>The dashed line that parts the heading, the body and the keys of a panel.</summary>
        private static GameObject Dashes(Transform parent, string name)
        {
            var line = UiObject(name, parent);
            line.AddComponent<LayoutElement>().preferredHeight = 2f;
            var graphic = line.AddComponent<DashedLineGraphic>();
            graphic.color = Separator;
            graphic.raycastTarget = false;
            return line;
        }

        /// <summary>An item's icon in a dark well, as in the lines of a panel.</summary>
        private static ItemIcon IconWell(Transform parent, float size)
        {
            var well = UiObject("IconWell", parent);
            Cut(well, WoodDark, 6f);
            var element = well.AddComponent<LayoutElement>();
            element.preferredWidth = size;
            element.preferredHeight = size;
            var icon = Icon(well.transform, size - 12f);
            Centre(icon.gameObject, size - 12f);
            return icon;
        }

        /// <summary>The place of the tick of a complete line, kept when the tick is hidden so that the counts stay aligned. Returns the tick.</summary>
        private static GameObject TickSlot(Transform parent, float size)
        {
            var slot = UiObject("TickSlot", parent);
            var element = slot.AddComponent<LayoutElement>();
            element.preferredWidth = size;
            element.preferredHeight = size;
            var tick = UiObject("Tick", slot.transform);
            Stretch(tick);
            tick.AddComponent<TickGraphic>().raycastTarget = false;
            return tick;
        }

        // ---------------------------------------------------------------- crafting

        private static IngredientRow BuildIngredientRow(string prefabName, bool withSource, int textSize, float height)
        {
            var row = UiObject(prefabName, null);
            Size(row, 380f, height);
            if (withSource)
            {
                Cut(row, new Color(0f, 0f, 0f, 0.2f), 6f);
            }

            var layout = Horizontal(row, 10f, withSource ? new RectOffset(4, 10, 4, 4) : new RectOffset(0, 0, 0, 0));
            layout.childAlignment = TextAnchor.MiddleLeft;
            row.AddComponent<LayoutElement>().preferredHeight = height;
            var icon = Icon(row.transform, height - 10f);
            var texts = UiObject("Texts", row.transform);
            Vertical(texts, 0f, new RectOffset(0, 0, 0, 0)).childAlignment = TextAnchor.MiddleLeft;
            texts.AddComponent<LayoutElement>().flexibleWidth = 1f;
            var name = Label(texts.transform, "Name", textSize, withSource ? Cream : Sand, TextAnchor.MiddleLeft, null);
            LocalizedLabel source = null;
            if (withSource)
            {
                source = Label(texts.transform, "Source", 14, Missing, TextAnchor.MiddleLeft, null);
            }

            var count = NewText(row.transform, "Count", textSize + 2, Cream, TextAnchor.MiddleRight);
            count.gameObject.AddComponent<LayoutElement>().preferredWidth = 52f;
            var tick = TickSlot(row.transform, withSource ? 22f : 16f);
            var view = row.AddComponent<IngredientRow>();
            SetRef(view, "tick", tick);
            SetRef(view, "icon", icon);
            SetRef(view, "itemName", name);
            SetRef(view, "source", source);
            SetRef(view, "count", count);
            SetBool(view, "showSource", withSource);
            return SavePrefab(row, $"{UiFolder}/{prefabName}.prefab").GetComponent<IngredientRow>();
        }

        private static RecipeRow BuildRecipeRow()
        {
            var row = UiObject("RecipeRow", null);
            Size(row, 400f, 56f);
            Cut(row, WoodDark, 10f);
            var selectable = row.AddComponent<Selectable>();
            selectable.transition = Selectable.Transition.None;
            row.AddComponent<LayoutElement>().preferredHeight = 56f;
            var layout = Horizontal(row, 12f, new RectOffset(8, 12, 6, 6));
            layout.childAlignment = TextAnchor.MiddleLeft;

            var mark = UiObject("OpenMark", row.transform);
            var markRect = (RectTransform)mark.transform;
            mark.AddComponent<LayoutElement>().ignoreLayout = true;
            markRect.anchorMin = new Vector2(0f, 0f);
            markRect.anchorMax = new Vector2(0f, 1f);
            markRect.pivot = new Vector2(0f, 0.5f);
            markRect.sizeDelta = new Vector2(4f, 0f);
            mark.AddComponent<Image>().color = Cream;

            var iconBox = UiObject("IconBox", row.transform);
            Cut(iconBox, new Color(0f, 0f, 0f, 0.25f), 6f);
            var boxElement = iconBox.AddComponent<LayoutElement>();
            boxElement.preferredWidth = 44f;
            boxElement.preferredHeight = 44f;
            var icon = Icon(iconBox.transform, 36f);
            Centre(icon.gameObject, 36f);

            var name = Label(row.transform, "Name", 20, Cream, TextAnchor.MiddleLeft, null);
            name.gameObject.AddComponent<LayoutElement>().flexibleWidth = 1f;

            var tag = UiObject("Tracked", row.transform);
            Cut(tag, Cyan, 5f);
            var tagElement = tag.AddComponent<LayoutElement>();
            tagElement.preferredWidth = 76f;
            tagElement.preferredHeight = 22f;
            var tagLabel = Label(tag.transform, "Label", 13, Bg, TextAnchor.MiddleCenter, UiText.CraftTracked);
            Stretch(tagLabel.gameObject);

            var state = Label(row.transform, "State", 15, Sand, TextAnchor.MiddleRight, UiText.CraftPossible);
            state.gameObject.AddComponent<LayoutElement>().preferredWidth = 90f;

            var view = row.AddComponent<RecipeRow>();
            SetRef(view, "background", row.GetComponent<CutCornerGraphic>());
            SetRef(view, "icon", icon);
            SetRef(view, "itemName", name);
            SetRef(view, "state", state);
            SetRef(view, "trackedTag", tag);
            SetRef(view, "openMark", mark);
            return SavePrefab(row, UiFolder + "/RecipeRow.prefab").GetComponent<RecipeRow>();
        }

        /// <summary>Puts the crafting panel right of the inventory panel, inside the Inventory prefab, and moves the inventory left.</summary>
        private static void AddCraftingPanel()
        {
            var recipeRow = BuildRecipeRow();
            var ingredientRow = BuildIngredientRow("IngredientRow", true, 18, 48f);
            var root = PrefabUtility.LoadPrefabContents(InventoryPath);
            try
            {
                var screen = root.GetComponent<InventoryScreen>();
                var inventoryPanel = (RectTransform)FindDeep(root.transform, "InventoryPanel")
                    ?? throw new InvalidOperationException("The Inventory prefab has no InventoryPanel.");
                var existing = FindDeep(root.transform, "CraftingPanel");
                if (existing != null)
                {
                    Object.DestroyImmediate(existing.gameObject);
                }

                inventoryPanel.anchoredPosition = new Vector2(-250f, inventoryPanel.anchoredPosition.y);

                var panel = UiObject("CraftingPanel", inventoryPanel.parent);
                var rect = (RectTransform)panel.transform;
                rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 1f);
                rect.pivot = new Vector2(0.5f, 1f);
                rect.anchoredPosition = new Vector2(230f, inventoryPanel.anchoredPosition.y);
                rect.sizeDelta = new Vector2(460f, inventoryPanel.sizeDelta.y);
                var background = panel.AddComponent<CutCornerGraphic>();
                EditorUtility.CopySerialized(inventoryPanel.GetComponent<CutCornerGraphic>(), background);
                // The held item is drawn over everything: the panel goes under it.
                panel.transform.SetSiblingIndex(inventoryPanel.GetSiblingIndex() + 1);

                var title = Label(panel.transform, "Title", 40, Cream, TextAnchor.MiddleCenter, UiText.CraftTitle);
                Top(title.gameObject, 26f, 46f, 0f);
                var shadow = title.gameObject.AddComponent<Shadow>();
                shadow.effectColor = WoodDark;
                shadow.effectDistance = new Vector2(0f, -3f);

                var list = UiObject("Recipes", panel.transform);
                Top(list, 90f, 260f, 24f);
                Vertical(list, 8f, new RectOffset(0, 0, 0, 0));

                var details = UiObject("Details", panel.transform);
                Bottom(details, 70f, 210f, 24f);
                Vertical(details, 8f, new RectOffset(0, 0, 0, 0));
                var head = UiObject("Head", details.transform);
                Horizontal(head, 12f, new RectOffset(0, 0, 0, 0)).childAlignment = TextAnchor.MiddleLeft;
                head.AddComponent<LayoutElement>().preferredHeight = 60f;
                var bigBox = UiObject("IconBox", head.transform);
                Cut(bigBox, WoodDark, 10f);
                var bigElement = bigBox.AddComponent<LayoutElement>();
                bigElement.preferredWidth = 60f;
                bigElement.preferredHeight = 60f;
                var bigIcon = Icon(bigBox.transform, 50f);
                Centre(bigIcon.gameObject, 50f);
                var headTexts = UiObject("Texts", head.transform);
                Vertical(headTexts, 2f, new RectOffset(0, 0, 0, 0)).childAlignment = TextAnchor.MiddleLeft;
                headTexts.AddComponent<LayoutElement>().flexibleWidth = 1f;
                var detailName = Label(headTexts.transform, "Name", 24, Cream, TextAnchor.MiddleLeft, null);
                var description = Label(headTexts.transform, "Description", 15, Sand, TextAnchor.MiddleLeft, null);
                var ingredients = UiObject("Ingredients", details.transform);
                Vertical(ingredients, 6f, new RectOffset(0, 0, 0, 0));

                var nothingOpen = Label(panel.transform, "NothingOpen", 17, new Color(Sand.r, Sand.g, Sand.b, 0.6f), TextAnchor.MiddleCenter, UiText.CraftChoose);
                Bottom(nothingOpen.gameObject, 70f, 210f, 24f);

                var actions = BuildActions(panel.transform);

                var crafting = panel.AddComponent<CraftingPanel>();
                SetRef(crafting, "book", AssetDatabase.LoadAssetAtPath<RecipeBook>(RecipeBookPath));
                SetRef(crafting, "rowPrefab", recipeRow);
                SetRef(crafting, "list", list.transform);
                SetRef(crafting, "details", details);
                SetRef(crafting, "nothingOpen", nothingOpen.gameObject);
                SetRef(crafting, "detailIcon", bigIcon);
                SetRef(crafting, "detailName", detailName);
                SetRef(crafting, "detailDescription", description);
                SetRef(crafting, "ingredientPrefab", ingredientRow);
                SetRef(crafting, "ingredients", ingredients.transform);
                SetRef(crafting, "actions", actions);
                SetRef(screen, "crafting", crafting);
                PrefabUtility.SaveAsPrefabAsset(root, InventoryPath);
            }
            finally
            {
                PrefabUtility.UnloadPrefabContents(root);
            }
        }

        /// <summary>The keys to craft and to track, at the bottom of the panel: a key hints panel drawn like the one bottom right.</summary>
        private static ControlHintsPanel BuildActions(Transform parent)
        {
            var reference = AssetDatabase.LoadAssetAtPath<GameObject>(ControlHintsPath).GetComponent<ControlHintsPanel>();
            var actions = UiObject("Actions", parent);
            Bottom(actions, 22f, 40f, 24f);
            var row = Horizontal(actions, 26f, new RectOffset(0, 0, 0, 0));
            row.childAlignment = TextAnchor.MiddleCenter;
            row.childControlWidth = false;
            row.childForceExpandWidth = false;
            var panel = actions.AddComponent<ControlHintsPanel>();
            var source = new SerializedObject(reference);
            var target = new SerializedObject(panel);
            foreach (var field in new[] { "rowPrefab", "keyPrefab", "pillPrefab", "facePrefab", "mousePrefab", "viewPrefab" })
            {
                target.FindProperty(field).objectReferenceValue = source.FindProperty(field).objectReferenceValue;
            }

            target.FindProperty("list").objectReferenceValue = actions.transform;
            target.ApplyModifiedPropertiesWithoutUndo();
            return panel;
        }

        // ---------------------------------------------------------------- top right

        private static ObjectiveHud BuildObjectiveHud()
        {
            var line = BuildIngredientRow("TrackedIngredientRow", false, 15, 24f);

            var tracked = UiObject("TrackedRecipe", null);
            Size(tracked, 300f, 0f);
            Vertical(tracked, 3f, new RectOffset(0, 0, 0, 0));
            tracked.AddComponent<ContentSizeFitter>().verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            var head = UiObject("Head", tracked.transform);
            Horizontal(head, 6f, new RectOffset(0, 0, 0, 0)).childAlignment = TextAnchor.MiddleLeft;
            head.AddComponent<LayoutElement>().preferredHeight = 26f;
            var icon = Icon(head.transform, 22f);
            var name = Label(head.transform, "Name", 17, Cream, TextAnchor.MiddleLeft, null);
            name.gameObject.AddComponent<LayoutElement>().flexibleWidth = 1f;
            var lines = UiObject("Lines", tracked.transform);
            Vertical(lines, 2f, new RectOffset(6, 0, 0, 0));
            var trackedView = tracked.AddComponent<TrackedRecipeView>();
            SetRef(trackedView, "icon", icon);
            SetRef(trackedView, "itemName", name);
            SetRef(trackedView, "linePrefab", line);
            SetRef(trackedView, "lines", lines.transform);
            var trackedPrefab = SavePrefab(tracked, UiFolder + "/TrackedRecipe.prefab").GetComponent<TrackedRecipeView>();

            var root = CanvasRoot("ObjectiveHud", 30);
            var column = UiObject("Column", root.transform);
            var rect = (RectTransform)column.transform;
            rect.anchorMin = rect.anchorMax = Vector2.one;
            rect.pivot = Vector2.one;
            rect.anchoredPosition = new Vector2(-36f, -36f);
            rect.sizeDelta = new Vector2(320f, 0f);
            Vertical(column, 12f, new RectOffset(0, 0, 0, 0));
            column.AddComponent<ContentSizeFitter>().verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            var heading = Label(column.transform, "Title", 14, Cyan, TextAnchor.MiddleLeft, UiText.ObjectiveTitle);
            AddShadow(heading);
            var objective = Label(column.transform, "Objective", 22, Cream, TextAnchor.MiddleLeft, UiText.ObjectiveFindSeed);
            AddShadow(objective);
            var list = UiObject("Tracked", column.transform);
            Vertical(list, 10f, new RectOffset(0, 0, 0, 0));
            var hud = root.AddComponent<ObjectiveHud>();
            SetRef(hud, "objective", objective);
            SetRef(hud, "trackedPrefab", trackedPrefab);
            SetRef(hud, "tracked", list.transform);
            return SavePrefab(root, UiFolder + "/ObjectiveHud.prefab").GetComponent<ObjectiveHud>();
        }

        // ---------------------------------------------------------------- end of the demo

        private static DemoEnd BuildDemoEnd()
        {
            var root = CanvasRoot("DemoEnd", 50);
            root.AddComponent<GraphicRaycaster>();
            var veil = UiObject("Veil", root.transform);
            Stretch(veil);
            veil.AddComponent<Image>().color = new Color(Bg.r, Bg.g, Bg.b, 0.55f);

            var column = UiObject("Column", root.transform);
            var rect = (RectTransform)column.transform;
            rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.sizeDelta = new Vector2(900f, 360f);
            var layout = Vertical(column, 24f, new RectOffset(0, 0, 0, 0));
            layout.childAlignment = TextAnchor.MiddleCenter;
            layout.childControlWidth = false;
            layout.childForceExpandWidth = false;
            var title = Label(column.transform, "Title", 72, Cream, TextAnchor.MiddleCenter, UiText.DemoEndTitle);
            Size(title.gameObject, 900f, 90f);
            var shadow = title.gameObject.AddComponent<Shadow>();
            shadow.effectColor = WoodDark;
            shadow.effectDistance = new Vector2(0f, -4f);
            var thanks = Label(column.transform, "Thanks", 34, Sand, TextAnchor.MiddleCenter, UiText.DemoEndThanks);
            Size(thanks.gameObject, 900f, 50f);
            var home = (GameObject)PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath<GameObject>(PlankButtonPath), column.transform);
            home.name = "Home";
            var plank = home.GetComponent<PlankButton>();
            SetString(plank.Label, "entryKey", UiText.PauseHome);

            var end = root.AddComponent<DemoEnd>();
            SetRef(end, "homeButton", plank);
            return SavePrefab(root, UiFolder + "/DemoEnd.prefab").GetComponent<DemoEnd>();
        }

        /// <summary>A small room of mycelium in warm light, and the end of the demo over it.</summary>
        private static void BuildInterior(DemoEnd demoEnd)
        {
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            var earth = AssetDatabase.LoadAssetAtPath<Material>(EarthMaterialPath);
            var mycelium = PlaceholderMaterial("Assets/Art/Materials/PlaceholderMycelium.mat", Cream, 0.3f);

            var cameraObject = new GameObject("Main Camera", typeof(Camera), typeof(AudioListener));
            cameraObject.tag = "MainCamera";
            cameraObject.transform.SetPositionAndRotation(new Vector3(0f, 1.6f, -3.2f), Quaternion.Euler(8f, 0f, 0f));
            var camera = cameraObject.GetComponent<Camera>();
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = Bg;

            var lightObject = new GameObject("Warm Light", typeof(Light));
            var light = lightObject.GetComponent<Light>();
            light.type = LightType.Point;
            light.color = new Color32(0xFF, 0xD9, 0xA8, 0xFF);
            light.intensity = 3f;
            light.range = 12f;
            lightObject.transform.position = new Vector3(0f, 2.6f, 0f);

            var room = new GameObject("Room (placeholder)").transform;
            var floor = Primitive(PrimitiveType.Cylinder, "Floor", room, earth);
            floor.transform.localScale = new Vector3(8f, 0.05f, 8f);
            const int arches = 12;
            for (var index = 0; index < arches; index++)
            {
                var angle = index * Mathf.PI * 2f / arches;
                var arch = Primitive(PrimitiveType.Cube, "Mycelium Wall", room, mycelium);
                arch.transform.localPosition = new Vector3(Mathf.Cos(angle) * 3.6f, 1.4f, Mathf.Sin(angle) * 3.6f);
                arch.transform.localRotation = Quaternion.Euler(0f, -angle * Mathf.Rad2Deg, 8f);
                arch.transform.localScale = new Vector3(0.3f, 2.8f, 1.9f);
            }

            var cap = Primitive(PrimitiveType.Sphere, "Ceiling", room, mycelium);
            cap.transform.localPosition = new Vector3(0f, 3f, 0f);
            cap.transform.localScale = new Vector3(8f, 1.2f, 8f);

            new GameObject("EventSystem", typeof(EventSystem), typeof(InputSystemUIInputModule));
            PrefabUtility.InstantiatePrefab(demoEnd.gameObject, scene);
            EditorSceneManager.SaveScene(scene, PlaytestSetup.InteriorScenePath);
        }

        // ---------------------------------------------------------------- player and scene

        private static void AddSlingshotToPlayer()
        {
            var root = PrefabUtility.LoadPrefabContents(PlayerPath);
            try
            {
                if (root.GetComponent<Slingshot>() == null)
                {
                    root.AddComponent<Slingshot>();
                }

                PrefabUtility.SaveAsPrefabAsset(root, PlayerPath);
            }
            finally
            {
                PrefabUtility.UnloadPrefabContents(root);
            }
        }

        private static void WireGameScene(GameObject moleHole, RequirementPanel requirementPanel, ObjectiveHud objectiveHud)
        {
            var scene = EditorSceneManager.OpenScene(PlaytestSetup.GameScenePath, OpenSceneMode.Single);
            var game = Object.FindFirstObjectByType<GameBootstrap>() ?? throw new InvalidOperationException("No GameBootstrap in the Game scene.");
            var holes = Object.FindFirstObjectByType<MoleHoles>() ?? throw new InvalidOperationException("No MoleHoles in the Game scene.");
            SetRef(holes, "model", moleHole);

            var panel = (GameObject)PrefabUtility.InstantiatePrefab(requirementPanel.gameObject, scene);
            var hud = (GameObject)PrefabUtility.InstantiatePrefab(objectiveHud.gameObject, scene);
            SetRef(game, "requirementPanel", panel.GetComponent<RequirementPanel>());
            SetRef(game, "objectiveHud", hud.GetComponent<ObjectiveHud>());
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
        }

        // ---------------------------------------------------------------- uGUI helpers

        private static GameObject CanvasRoot(string name, int sortingOrder)
        {
            var root = new GameObject(name, typeof(RectTransform));
            var canvas = root.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = sortingOrder;
            var scaler = root.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.matchWidthOrHeight = 0.5f;
            return root;
        }

        private static GameObject UiObject(string name, Transform parent)
        {
            var go = new GameObject(name, typeof(RectTransform));
            if (parent != null)
            {
                go.transform.SetParent(parent, false);
            }

            return go;
        }

        private static void Size(GameObject go, float width, float height)
        {
            ((RectTransform)go.transform).sizeDelta = new Vector2(width, height);
        }

        private static void Stretch(GameObject go)
        {
            var rect = (RectTransform)go.transform;
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = rect.offsetMax = Vector2.zero;
        }

        private static void Centre(GameObject go, float size)
        {
            var rect = (RectTransform)go.transform;
            rect.anchorMin = rect.anchorMax = rect.pivot = new Vector2(0.5f, 0.5f);
            rect.sizeDelta = new Vector2(size, size);
            var element = go.GetComponent<LayoutElement>() ?? go.AddComponent<LayoutElement>();
            element.ignoreLayout = true;
        }

        /// <summary>Across the panel, <paramref name="margin"/> from its sides, <paramref name="fromTop"/> down from its top.</summary>
        private static void Top(GameObject go, float fromTop, float height, float margin)
        {
            var rect = (RectTransform)go.transform;
            rect.anchorMin = new Vector2(0f, 1f);
            rect.anchorMax = new Vector2(1f, 1f);
            rect.pivot = new Vector2(0.5f, 1f);
            rect.offsetMin = new Vector2(margin, -fromTop - height);
            rect.offsetMax = new Vector2(-margin, -fromTop);
        }

        /// <summary>Across the panel, <paramref name="margin"/> from its sides, <paramref name="fromBottom"/> up from its bottom.</summary>
        private static void Bottom(GameObject go, float fromBottom, float height, float margin)
        {
            var rect = (RectTransform)go.transform;
            rect.anchorMin = new Vector2(0f, 0f);
            rect.anchorMax = new Vector2(1f, 0f);
            rect.pivot = new Vector2(0.5f, 0f);
            rect.offsetMin = new Vector2(margin, fromBottom);
            rect.offsetMax = new Vector2(-margin, fromBottom + height);
        }

        private static VerticalLayoutGroup Vertical(GameObject go, float spacing, RectOffset padding)
        {
            var layout = go.AddComponent<VerticalLayoutGroup>();
            layout.spacing = spacing;
            layout.padding = padding;
            layout.childControlWidth = true;
            layout.childControlHeight = true;
            layout.childForceExpandWidth = true;
            layout.childForceExpandHeight = false;
            layout.childAlignment = TextAnchor.UpperLeft;
            return layout;
        }

        private static HorizontalLayoutGroup Horizontal(GameObject go, float spacing, RectOffset padding)
        {
            var layout = go.AddComponent<HorizontalLayoutGroup>();
            layout.spacing = spacing;
            layout.padding = padding;
            layout.childControlWidth = true;
            layout.childControlHeight = true;
            layout.childForceExpandWidth = false;
            layout.childForceExpandHeight = false;
            return layout;
        }

        private static Text NewText(Transform parent, string name, int size, Color color, TextAnchor alignment)
        {
            var go = UiObject(name, parent);
            var text = go.AddComponent<Text>();
            text.font = font;
            text.fontSize = size;
            text.color = color;
            text.alignment = alignment;
            text.horizontalOverflow = HorizontalWrapMode.Wrap;
            text.verticalOverflow = VerticalWrapMode.Overflow;
            text.raycastTarget = false;
            return text;
        }

        private static LocalizedLabel Label(Transform parent, string name, int size, Color color, TextAnchor alignment, string entryKey)
        {
            var text = NewText(parent, name, size, color, alignment);
            var label = text.gameObject.AddComponent<LocalizedLabel>();
            if (entryKey != null)
            {
                SetString(label, "entryKey", entryKey);
            }

            return label;
        }

        private static void AddShadow(LocalizedLabel label)
        {
            var shadow = label.gameObject.AddComponent<Shadow>();
            shadow.effectColor = new Color(Bg.r, Bg.g, Bg.b, 0.8f);
            shadow.effectDistance = new Vector2(0f, -2f);
        }

        private static ItemIcon Icon(Transform parent, float size)
        {
            var go = UiObject("Icon", parent);
            go.AddComponent<Image>().raycastTarget = false;
            var element = go.AddComponent<LayoutElement>();
            element.preferredWidth = size;
            element.preferredHeight = size;
            return go.AddComponent<ItemIcon>();
        }

        private static CutCornerGraphic Cut(GameObject go, Color face, float corner)
        {
            var graphic = go.AddComponent<CutCornerGraphic>();
            var serialized = new SerializedObject(graphic);
            serialized.FindProperty("face").colorValue = face;
            serialized.FindProperty("corner").floatValue = corner;
            serialized.ApplyModifiedPropertiesWithoutUndo();
            return graphic;
        }

        /// <summary>The frame of the wooden panels design system: cut corners, a lit top edge, a dark underside, a darker facet over the right part.</summary>
        private static void WoodPanel(GameObject go)
        {
            var graphic = Cut(go, Wood, 14f);
            var serialized = new SerializedObject(graphic);
            Find(serialized, "topBand").colorValue = WoodLight;
            Find(serialized, "topBandHeight").floatValue = 6f;
            Find(serialized, "bottomBand").colorValue = WoodDark;
            Find(serialized, "bottomBandHeight").floatValue = 9f;
            Find(serialized, "facet").colorValue = Facet;
            Find(serialized, "facetTopShare").floatValue = 0.7f;
            Find(serialized, "facetBottomShare").floatValue = 0.64f;
            serialized.ApplyModifiedPropertiesWithoutUndo();
        }

        // ---------------------------------------------------------------- assets

        private static GameObject SavePrefab(GameObject root, string path)
        {
            var prefab = PrefabUtility.SaveAsPrefabAsset(root, path, out var saved);
            Object.DestroyImmediate(root);
            if (!saved)
            {
                throw new InvalidOperationException($"Could not save {path}.");
            }

            return prefab;
        }

        private static Transform FindDeep(Transform parent, string name)
        {
            foreach (Transform child in parent)
            {
                if (child.name == name)
                {
                    return child;
                }

                var found = FindDeep(child, name);
                if (found != null)
                {
                    return found;
                }
            }

            return null;
        }

        private static void EnsureFolder(string parent, string name)
        {
            if (!AssetDatabase.IsValidFolder($"{parent}/{name}"))
            {
                AssetDatabase.CreateFolder(parent, name);
            }
        }

        private static SerializedProperty Find(SerializedObject serialized, string field)
        {
            return serialized.FindProperty(field)
                ?? throw new InvalidOperationException($"{serialized.targetObject.GetType().Name} has no serialized field '{field}'.");
        }

        private static void SetRef(Object target, string field, Object value)
        {
            var serialized = new SerializedObject(target);
            Find(serialized, field).objectReferenceValue = value;
            serialized.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void SetString(Object target, string field, string value)
        {
            var serialized = new SerializedObject(target);
            Find(serialized, field).stringValue = value;
            serialized.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void SetBool(Object target, string field, bool value)
        {
            var serialized = new SerializedObject(target);
            Find(serialized, field).boolValue = value;
            serialized.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void SetArray(Object target, string field, params Object[] values)
        {
            var serialized = new SerializedObject(target);
            var array = Find(serialized, field);
            array.arraySize = values.Length;
            for (var index = 0; index < values.Length; index++)
            {
                array.GetArrayElementAtIndex(index).objectReferenceValue = values[index];
            }

            serialized.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(target);
        }
    }
}
