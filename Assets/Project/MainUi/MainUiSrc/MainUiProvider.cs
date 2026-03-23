using TetrisTactic.Core;
using UnityEngine;
using UnityEngine.EventSystems;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem.UI;
#endif
using UnityEngine.UI;

namespace TetrisTactic.MainUi
{
    public sealed class MainUiProvider : MonoBehaviour
    {
        [SerializeField] private ServiceLocator serviceLocator;
        [SerializeField] private RectTransform floatingTextParent;
        [SerializeField] private RectTransform hudParent;
        [SerializeField] private RectTransform popupParent;
        [SerializeField] private ProgressionPopup progressionPopup;
        [SerializeField] private ResourceCounter resourceCounter;

        private RectTransform canvasRoot;

        public RectTransform FloatingTextParent => floatingTextParent;
        public RectTransform HudParent => hudParent;
        public RectTransform PopupParent => popupParent;
        public ProgressionPopup ProgressionPopup => progressionPopup;
        public ResourceCounter ResourceCounter => resourceCounter;

        private void Awake()
        {
            EnsureEventSystem();
            EnsureCanvas();
            EnsureMainParents();
            EnsureResourceCounter();
            EnsureProgressionPopup();

            if (serviceLocator != null)
            {
                serviceLocator.RegisterMainUiProvider(this);
            }
        }

        private void Start()
        {
            resourceCounter.Refresh(0);
        }

        private void EnsureEventSystem()
        {
            var currentEventSystem = EventSystem.current;
            if (currentEventSystem == null)
            {
#if ENABLE_INPUT_SYSTEM
                _ = new GameObject("EventSystem", typeof(EventSystem), typeof(InputSystemUIInputModule));
#else
                _ = new GameObject("EventSystem", typeof(EventSystem), typeof(StandaloneInputModule));
#endif
                return;
            }

#if ENABLE_INPUT_SYSTEM
            var standaloneInputModule = currentEventSystem.GetComponent<StandaloneInputModule>();
            if (standaloneInputModule != null)
            {
                Destroy(standaloneInputModule);
            }

            if (currentEventSystem.GetComponent<InputSystemUIInputModule>() == null)
            {
                currentEventSystem.gameObject.AddComponent<InputSystemUIInputModule>();
            }
#endif
        }

        private void EnsureCanvas()
        {
            if (canvasRoot == null)
            {
                var existingCanvas = GetComponentInChildren<Canvas>();
                if (existingCanvas != null)
                {
                    canvasRoot = existingCanvas.GetComponent<RectTransform>();
                }
            }

            if (canvasRoot == null)
            {
                var canvasObject = new GameObject(
                    "MainCanvas",
                    typeof(RectTransform),
                    typeof(Canvas),
                    typeof(CanvasScaler),
                    typeof(GraphicRaycaster));

                canvasRoot = canvasObject.GetComponent<RectTransform>();
                canvasRoot.SetParent(transform, false);
                canvasRoot.anchorMin = Vector2.zero;
                canvasRoot.anchorMax = Vector2.one;
                canvasRoot.offsetMin = Vector2.zero;
                canvasRoot.offsetMax = Vector2.zero;
            }

            var canvas = canvasRoot.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.pixelPerfect = false;

            var scaler = canvasRoot.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.matchWidthOrHeight = 0.5f;
        }

        private void EnsureMainParents()
        {
            floatingTextParent = EnsureParent(floatingTextParent, "FloatingTextParent", canvasRoot);
            hudParent = EnsureParent(hudParent, "HudParent", canvasRoot);
            popupParent = EnsureParent(popupParent, "PopupParent", canvasRoot);

            StretchToParent(floatingTextParent);
            StretchToParent(hudParent);
            StretchToParent(popupParent);
        }

        private void EnsureResourceCounter()
        {
            if (resourceCounter != null)
            {
                return;
            }

            var counterObject = new GameObject("ResourceCounter", typeof(RectTransform), typeof(Image), typeof(ResourceCounter));
            var counterRect = counterObject.GetComponent<RectTransform>();
            counterRect.SetParent(hudParent, false);
            counterRect.anchorMin = new Vector2(0f, 1f);
            counterRect.anchorMax = new Vector2(0f, 1f);
            counterRect.pivot = new Vector2(0f, 1f);
            counterRect.anchoredPosition = new Vector2(40f, -30f);
            counterRect.sizeDelta = new Vector2(280f, 90f);

            var background = counterObject.GetComponent<Image>();
            background.color = new Color(0.12f, 0.12f, 0.16f, 0.85f);

            var labelText = CreateLabel("AmountText", counterRect, "Resource: 0", 36, TextAnchor.MiddleLeft);
            var labelRect = labelText.rectTransform;
            labelRect.anchorMin = new Vector2(0f, 0f);
            labelRect.anchorMax = new Vector2(1f, 1f);
            labelRect.offsetMin = new Vector2(20f, 0f);
            labelRect.offsetMax = new Vector2(-20f, 0f);

            resourceCounter = counterObject.GetComponent<ResourceCounter>();
            resourceCounter.BindAmountText(labelText);
        }

        private void EnsureProgressionPopup()
        {
            if (progressionPopup != null)
            {
                return;
            }

            var popupObject = new GameObject("ProgressionPopup", typeof(RectTransform), typeof(Image), typeof(ProgressionPopup));
            var popupRect = popupObject.GetComponent<RectTransform>();
            popupRect.SetParent(popupParent, false);
            StretchToParent(popupRect);

            var popupBackground = popupObject.GetComponent<Image>();
            popupBackground.color = new Color(0.08f, 0.1f, 0.14f, 0.92f);

            var topPlate = new GameObject("TopLeftResourcePlate", typeof(RectTransform), typeof(Image));
            var topPlateRect = topPlate.GetComponent<RectTransform>();
            topPlateRect.SetParent(popupRect, false);
            topPlateRect.anchorMin = new Vector2(0f, 1f);
            topPlateRect.anchorMax = new Vector2(0f, 1f);
            topPlateRect.pivot = new Vector2(0f, 1f);
            topPlateRect.anchoredPosition = new Vector2(30f, -25f);
            topPlateRect.sizeDelta = new Vector2(300f, 80f);
            topPlate.GetComponent<Image>().color = new Color(0.2f, 0.2f, 0.28f, 0.95f);

            var plateLabel = CreateLabel("PlateLabel", topPlateRect, "Resource Plate", 26, TextAnchor.MiddleCenter);
            StretchToParent(plateLabel.rectTransform);

            var levelText = CreateLabel("LevelText", popupRect, "Level 1", 64, TextAnchor.UpperCenter);
            var levelRect = levelText.rectTransform;
            levelRect.anchorMin = new Vector2(0.5f, 1f);
            levelRect.anchorMax = new Vector2(0.5f, 1f);
            levelRect.pivot = new Vector2(0.5f, 1f);
            levelRect.anchoredPosition = new Vector2(0f, -30f);
            levelRect.sizeDelta = new Vector2(420f, 100f);

            var damageUpgrade = CreateUpgradeButton(popupRect, "UpgradeDamageButton", "+1 Ability Damage", new Vector2(0f, 120f));
            var damagePrice = CreatePriceLabel(damageUpgrade.transform as RectTransform, "DamagePriceLabel", "Price: 1");

            var healthUpgrade = CreateUpgradeButton(popupRect, "UpgradeHealthButton", "+1 Player HP", new Vector2(0f, -20f));
            var healthPrice = CreatePriceLabel(healthUpgrade.transform as RectTransform, "HealthPriceLabel", "Price: 1");

            var startLevelButton = CreatePrimaryButton(popupRect, "StartLevelButton", "Start Level", new Vector2(0f, -250f), new Vector2(480f, 120f));

            progressionPopup = popupObject.GetComponent<ProgressionPopup>();
            progressionPopup.BindViews(levelText, damageUpgrade, healthUpgrade, startLevelButton, damagePrice, healthPrice);
        }

        private static RectTransform EnsureParent(RectTransform current, string name, Transform parent)
        {
            if (current != null)
            {
                return current;
            }

            var parentObject = new GameObject(name, typeof(RectTransform));
            var rect = parentObject.GetComponent<RectTransform>();
            rect.SetParent(parent, false);
            return rect;
        }

        private static void StretchToParent(RectTransform target)
        {
            target.anchorMin = Vector2.zero;
            target.anchorMax = Vector2.one;
            target.offsetMin = Vector2.zero;
            target.offsetMax = Vector2.zero;
            target.anchoredPosition = Vector2.zero;
        }

        private static Button CreateUpgradeButton(RectTransform parent, string objectName, string caption, Vector2 anchoredPosition)
        {
            return CreatePrimaryButton(parent, objectName, caption, anchoredPosition, new Vector2(640f, 120f));
        }

        private static Button CreatePrimaryButton(
            RectTransform parent,
            string objectName,
            string caption,
            Vector2 anchoredPosition,
            Vector2 size)
        {
            var buttonObject = new GameObject(objectName, typeof(RectTransform), typeof(Image), typeof(Button));
            var buttonRect = buttonObject.GetComponent<RectTransform>();
            buttonRect.SetParent(parent, false);
            buttonRect.anchorMin = new Vector2(0.5f, 0.5f);
            buttonRect.anchorMax = new Vector2(0.5f, 0.5f);
            buttonRect.pivot = new Vector2(0.5f, 0.5f);
            buttonRect.anchoredPosition = anchoredPosition;
            buttonRect.sizeDelta = size;

            var buttonImage = buttonObject.GetComponent<Image>();
            buttonImage.color = new Color(0.24f, 0.3f, 0.44f, 1f);

            var label = CreateLabel("Label", buttonRect, caption, 34, TextAnchor.MiddleCenter);
            StretchToParent(label.rectTransform);

            return buttonObject.GetComponent<Button>();
        }

        private static Text CreatePriceLabel(RectTransform parent, string objectName, string content)
        {
            var priceLabel = CreateLabel(objectName, parent, content, 26, TextAnchor.LowerRight);
            var priceRect = priceLabel.rectTransform;
            priceRect.anchorMin = new Vector2(0f, 0f);
            priceRect.anchorMax = new Vector2(1f, 1f);
            priceRect.offsetMin = new Vector2(20f, 10f);
            priceRect.offsetMax = new Vector2(-20f, -10f);
            return priceLabel;
        }

        private static Text CreateLabel(
            string objectName,
            Transform parent,
            string content,
            int fontSize,
            TextAnchor alignment)
        {
            var labelObject = new GameObject(objectName, typeof(RectTransform), typeof(Text));
            var text = labelObject.GetComponent<Text>();
            text.text = content;
            text.font = LoadBuiltinFont();
            text.fontSize = fontSize;
            text.alignment = alignment;
            text.color = Color.white;

            var rectTransform = labelObject.GetComponent<RectTransform>();
            rectTransform.SetParent(parent, false);

            return text;
        }

        private static Font LoadBuiltinFont()
        {
            var font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            if (font != null)
            {
                return font;
            }

            return Resources.GetBuiltinResource<Font>("Arial.ttf");
        }
    }
}
