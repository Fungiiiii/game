#if UNITY_EDITOR || DEVELOPMENT_BUILD
using System;
using Fungiiiii.Survival;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;

namespace Fungiiiii.UI
{
    /// <summary>
    /// Developer-only test bench, not player UI. Its English diagnostics and controls
    /// are excluded from release players; the reusable HUD presenter has no text.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class PoisonPrototypeView : MonoBehaviour
    {
        private static readonly Color Ink = new Color(0.91f, 0.94f, 0.96f);
        private static readonly Color Muted = new Color(0.58f, 0.65f, 0.72f);
        private static readonly Color Panel = new Color(0.075f, 0.10f, 0.14f);
        private static readonly Color Purple = new Color(0.72f, 0.49f, 0.98f);
        private static readonly Color Green = new Color(0.28f, 0.85f, 0.69f);
        private static readonly Color Warning = new Color(1f, 0.65f, 0.35f);

        private PoisonPrototypeController controller;
        private PoisonHudPresenter presenter;
        private Font font;
        private Slider healthSlider;
        private Text healthValue;
        private Text poisonValue;
        private Text damageValue;
        private Text status;
        private Text pauseLabel;
        private GameObject clearHint;
        private GameObject deathPanel;
        private GameObject helpPanel;
        private Image healthFill;
        private Button[] doseButtons;
        private float lastHealth = float.NaN;
        private float lastPoison = float.NaN;
        private bool lastPaused;
        private bool lastDead;
        private bool resumeAfterHelp;

        public bool IsPoisonVisible => presenter != null && presenter.IsPoisonVisible;
        public float PoisonPercentage => presenter == null ? 0f : presenter.PoisonPercentage;
        public bool IsDeathVisible => deathPanel != null && deathPanel.activeSelf;
        public bool IsHelpVisible => helpPanel != null && helpPanel.activeSelf;

        public void Initialize(PoisonPrototypeController owner)
        {
            if (owner == null) throw new ArgumentNullException(nameof(owner));
            if (controller != null) throw new InvalidOperationException("The test bench is already initialized.");
            controller = owner;
            font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            Build();
            Refresh();
        }

        private void LateUpdate()
        {
            if (controller == null) return;
            // Update the diagnostic labels only when the displayed values change.
            float displayedHealth = Mathf.Ceil(controller.CurrentHealth * 10f) / 10f;
            if (displayedHealth != lastHealth || controller.Poison.Intensity != lastPoison ||
                controller.IsPaused != lastPaused || controller.IsDead != lastDead)
                Refresh();
        }

        private void Refresh()
        {
            lastHealth = Mathf.Ceil(controller.CurrentHealth * 10f) / 10f;
            lastPoison = controller.Poison.Intensity;
            lastPaused = controller.IsPaused;
            lastDead = controller.IsDead;
            healthSlider.SetValueWithoutNotify(controller.CurrentHealth / controller.MaxHealth);
            healthValue.text = $"{lastHealth:0.0} / {controller.MaxHealth:0} HP";
            poisonValue.text = $"{lastPoison:0.#} %";
            damageValue.text = $"{controller.Poison.DamagePerSecond:0.00} HP / s";
            status.text = lastDead ? "DEAD" : lastPaused ? "PAUSED" :
                lastPoison >= PoisonState.DamageThreshold ? "TAKING DAMAGE" : lastPoison > 0f ? "EXPOSED" : "HEALTHY";
            status.color = lastDead || lastPoison >= PoisonState.DamageThreshold ? Warning : Green;
            pauseLabel.text = lastPaused ? "Resume   [Space]" : "Pause   [Space]";
            healthFill.color = controller.CurrentHealth / controller.MaxHealth <= 0.25f ? Warning : Green;
            clearHint.SetActive(lastPoison == 0f);
            deathPanel.SetActive(lastDead);
            foreach (Button button in doseButtons) button.interactable = !lastDead;
        }

        private void Build()
        {
            GameObject canvasObject = new GameObject("PoisonCanvas", typeof(RectTransform), typeof(Canvas),
                typeof(CanvasScaler), typeof(GraphicRaycaster));
            canvasObject.transform.SetParent(transform, false);
            Canvas canvas = canvasObject.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 100;
            CanvasScaler scaler = canvasObject.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1280f, 720f);
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.Expand;

            Image backdrop = Block("Backdrop", canvasObject.transform, new Color(0.035f, 0.05f, 0.075f), 0, 0, 0, 0);
            RectTransform backdropRect = backdrop.rectTransform;
            backdropRect.anchorMin = Vector2.zero;
            backdropRect.anchorMax = Vector2.one;
            backdropRect.offsetMin = Vector2.zero;
            backdropRect.offsetMax = Vector2.zero;
            RectTransform page = Rect("PoisonTestBench", canvasObject.transform, 0, 0, 1200, 640);
            page.anchorMin = page.anchorMax = page.pivot = new Vector2(0.5f, 0.5f);
            page.anchoredPosition = Vector2.zero;

            Label("Eyebrow", page, "FUNGIIIII   /   SURVIVAL PROTOTYPES", 0, 0, 800, 24, 17, Green);
            Label("Title", page, "Poison test bench", 0, 34, 800, 52, 38, Ink);
            Label("Subtitle", page, "Apply an exposure. Watch the HUD and health respond.", 0, 90, 850, 30, 20, Muted);
            ActionButton("HelpButton", page, "Test guide", 1000, 44, 200, 48, ShowHelp);

            RectTransform preview = Block("HudPreview", page, Panel, 0, 146, 748, 400).rectTransform;
            Label("PreviewTitle", preview, "LIVE HUD", 28, 22, 270, 30, 18, Muted);
            status = Label("StateLabel", preview, string.Empty, 350, 22, 370, 30, 20, Green);
            status.alignment = TextAnchor.MiddleRight;

            Label("HealthLabel", preview, "HEALTH", 28, 76, 180, 28, 18, Muted);
            healthValue = Label("HealthValue", preview, string.Empty, 260, 70, 460, 36, 26, Ink);
            healthValue.alignment = TextAnchor.MiddleRight;
            healthSlider = Gauge("HealthGauge", preview, Green, 28, 118, 692, 24, out healthFill);

            RectTransform poisonContainer = Rect("PoisonGauge", preview, 28, 171, 692, 84);
            Label("PoisonLabel", poisonContainer, "POISON", 0, 0, 240, 28, 18, Purple);
            Slider poisonSlider = Gauge("PoisonSlider", poisonContainer, Purple, 0, 44, 692, 24, out _);
            Block("DamageThreshold", poisonSlider.transform, Ink, 345, -4, 2, 32);
            clearHint = Label("ClearHint", preview, "No exposure. The poison gauge is hidden at 0%.",
                28, 174, 690, 60, 21, Muted).gameObject;
            presenter = gameObject.AddComponent<PoisonHudPresenter>();
            presenter.Bind(controller.Poison, poisonSlider, poisonContainer.gameObject);

            Block("Divider", preview, new Color(0.16f, 0.21f, 0.27f), 28, 275, 692, 1);
            Label("IntensityLabel", preview, "INTENSITY", 28, 296, 280, 26, 17, Muted);
            poisonValue = Label("IntensityValue", preview, string.Empty, 28, 328, 280, 42, 32, Purple);
            Label("DamageLabel", preview, "DAMAGE RATE", 390, 296, 300, 26, 17, Muted);
            damageValue = Label("DamageValue", preview, string.Empty, 390, 328, 330, 42, 30, Ink);

            RectTransform controls = Block("Controls", page, Panel, 772, 146, 428, 400).rectTransform;
            Label("ControlsTitle", controls, "EXPERIMENT CONTROLS", 24, 22, 380, 30, 18, Muted);
            doseButtons = new[]
            {
                ActionButton("ApplyButton", controls, "+10 poison   [P]", 24, 74, 184, 54,
                    () => controller.ApplyPoison(10f)),
                ActionButton("RemoveButton", controls, "-10 poison   [O]", 220, 74, 184, 54,
                    () => controller.RemovePoison(10f)),
                ActionButton("BelowThresholdButton", controls, "49%", 24, 176, 86, 46, () => controller.SetPoison(49f)),
                ActionButton("ThresholdButton", controls, "50%", 122, 176, 86, 46, () => controller.SetPoison(50f)),
                ActionButton("HighPoisonButton", controls, "75%", 220, 176, 86, 46, () => controller.SetPoison(75f)),
                ActionButton("MaximumPoisonButton", controls, "100%", 318, 176, 86, 46, () => controller.SetPoison(100f)),
                ActionButton("CureButton", controls, "Clear poison", 24, 242, 184, 50, controller.ClearPoison)
            };
            Label("PresetsLabel", controls, "SET EXPOSURE", 24, 139, 370, 26, 16, Muted);
            Button pause = ActionButton("PauseButton", controls, string.Empty, 220, 242, 184, 50, controller.TogglePause);
            pauseLabel = pause.GetComponentInChildren<Text>();
            ActionButton("ResetButton", controls, "Reset experiment   [R]", 24, 314, 380, 58, controller.ResetPrototype, true);

            Label("Rule", page, "DAMAGE STARTS AT 50%", 0, 568, 400, 28, 18, Warning);
            Label("RuleDetail", page, "49%: no damage   /   50%: 0.20 HP/s   /   75%: 1.41 HP/s   /   100%: 10.00 HP/s",
                0, 606, 1200, 28, 18, Muted);
            Label("DebugTag", page, "DEVELOPER TEST SCENE", 850, 568, 350, 28, 16, Muted).alignment = TextAnchor.MiddleRight;

            deathPanel = Block("DeathPanel", preview, new Color(0.16f, 0.07f, 0.09f, 0.98f), 0, 66, 748, 210).gameObject;
            deathPanel.GetComponent<Image>().raycastTarget = true;
            Label("DeathTitle", deathPanel.transform, "Subject lost", 28, 20, 692, 48, 32, Warning);
            Label("DeathDescription", deathPanel.transform, "Health reached zero. Reset to start a new experiment.",
                28, 80, 692, 38, 21, Ink);
            ActionButton("RestartButton", deathPanel.transform, "Restart experiment", 28, 135, 300, 52, controller.ResetPrototype, true);
            BuildHelp(page);
            EnsureEventSystem();
        }

        private void BuildHelp(Transform page)
        {
            helpPanel = Block("HelpPanel", page, Panel, 0, 0, 1200, 640).gameObject;
            helpPanel.GetComponent<Image>().raycastTarget = true;
            Label("GuideTitle", helpPanel.transform, "Test guide", 40, 32, 1100, 56, 38, Ink);
            Label("GuideBody", helpPanel.transform,
                "01   START CLEAR\nHealth is full; the poison gauge is hidden. Diagnostics remain visible.\n\n" +
                "02   APPLY EXPOSURE\nUse +10 / -10 or a preset. At 49%, health is stable; at 50%, it decreases.\n\n" +
                "03   OBSERVE AND RECOVER\n100% drains 10 HP/s. Clear poison stops damage without restoring health.\n\n" +
                "04   PAUSE, DIE, RESET\nPause freezes health. At zero HP, the death panel offers a restart.\nReset restores health, removes poison and resumes the experiment.",
                40, 110, 1120, 410, 23, Ink);
            Label("GuideKeys", helpPanel.transform, "Keyboard: P apply / O remove / R reset / Space pause. Click the Game view first.",
                40, 528, 1120, 32, 20, Muted);
            ActionButton("CloseHelpButton", helpPanel.transform, "Back to experiment", 40, 574, 320, 48,
                HideHelp, true);
            helpPanel.SetActive(false);
        }

        private void ShowHelp()
        {
            resumeAfterHelp = !controller.IsPaused;
            if (resumeAfterHelp) controller.TogglePause();
            helpPanel.SetActive(true);
        }

        private void HideHelp()
        {
            helpPanel.SetActive(false);
            if (resumeAfterHelp && controller.IsPaused) controller.TogglePause();
        }

        private void EnsureEventSystem()
        {
            if (FindFirstObjectByType<EventSystem>() != null) return;
            GameObject events = new GameObject("PoisonEventSystem", typeof(EventSystem), typeof(InputSystemUIInputModule));
            events.transform.SetParent(transform, false);
        }

        private Button ActionButton(string name, Transform parent, string caption, float x, float y,
            float width, float height, UnityAction action, bool primary = false)
        {
            Image image = Block(name, parent, primary ? new Color(0.18f, 0.40f, 0.35f) : new Color(0.15f, 0.20f, 0.27f),
                x, y, width, height);
            image.raycastTarget = true;
            Button button = image.gameObject.AddComponent<Button>();
            button.targetGraphic = image;
            Navigation navigation = button.navigation;
            navigation.mode = Navigation.Mode.None; // Pointer controls do not retain keyboard selection.
            button.navigation = navigation;
            button.onClick.AddListener(action);
            Text label = Label("Label", image.transform, caption, 8, 0, width - 16, height, 18, Ink);
            label.alignment = TextAnchor.MiddleCenter;
            return button;
        }

        private Slider Gauge(string name, Transform parent, Color color, float x, float y, float width, float height, out Image fill)
        {
            Image track = Block(name, parent, new Color(0.035f, 0.05f, 0.075f), x, y, width, height);
            fill = Block("Fill", track.transform, color, 0, 0, width, height);
            fill.rectTransform.anchorMin = Vector2.zero;
            fill.rectTransform.anchorMax = Vector2.one;
            fill.rectTransform.offsetMin = fill.rectTransform.offsetMax = Vector2.zero;
            Slider slider = track.gameObject.AddComponent<Slider>();
            slider.fillRect = fill.rectTransform;
            slider.interactable = false;
            slider.transition = Selectable.Transition.None;
            return slider;
        }

        private Text Label(string name, Transform parent, string value, float x, float y, float width, float height, int size, Color color)
        {
            RectTransform rect = Rect(name, parent, x, y, width, height);
            Text text = rect.gameObject.AddComponent<Text>();
            text.font = font;
            text.fontSize = size;
            text.color = color;
            text.alignment = TextAnchor.MiddleLeft;
            text.supportRichText = false;
            text.raycastTarget = false;
            text.text = value;
            return text;
        }

        private static Image Block(string name, Transform parent, Color color, float x, float y, float width, float height)
        {
            Image image = Rect(name, parent, x, y, width, height).gameObject.AddComponent<Image>();
            image.color = color;
            image.raycastTarget = false;
            return image;
        }

        private static RectTransform Rect(string name, Transform parent, float x, float y, float width, float height)
        {
            RectTransform rect = new GameObject(name, typeof(RectTransform)).GetComponent<RectTransform>();
            rect.SetParent(parent, false);
            rect.anchorMin = rect.anchorMax = rect.pivot = new Vector2(0f, 1f);
            rect.anchoredPosition = new Vector2(x, -y);
            rect.sizeDelta = new Vector2(width, height);
            return rect;
        }
    }
}
#endif
