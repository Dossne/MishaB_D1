using TetrisTactic.Core;
using TetrisTactic.FinishFlow;
using UnityEngine;
using UnityEngine.EventSystems;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem.UI;
#endif
using UnityEngine.UI;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace TetrisTactic.MainUi
{
    public sealed class MainUiProvider : MonoBehaviour
    {
        [SerializeField] private ServiceLocator serviceLocator;
        [SerializeField] private RectTransform floatingTextParent;
        [SerializeField] private RectTransform hudParent;
        [SerializeField] private RectTransform popupParent;
        [SerializeField] private ProgressionPopup progressionPopup;
        [SerializeField] private FinishPopup finishPopup;
        [SerializeField] private ResourceCounter resourceCounter;

        private RectTransform canvasRoot;
        private Sprite resourceIconSprite;
        private Sprite damageIconSprite;
        private Sprite healthIconSprite;

        public RectTransform FloatingTextParent => floatingTextParent;
        public RectTransform HudParent => hudParent;
        public RectTransform PopupParent => popupParent;
        public ProgressionPopup ProgressionPopup => progressionPopup;
        public FinishPopup FinishPopup => finishPopup;
        public ResourceCounter ResourceCounter => resourceCounter;

        private void Awake()
        {
            EnsureEventSystem();
            EnsureCanvas();
            EnsureMainParents();
            EnsureUiSpritesLoaded();
            EnsureResourceCounter();
            EnsureProgressionPopup();
            EnsureFinishPopup();

            if (serviceLocator != null)
            {
                serviceLocator.RegisterMainUiProvider(this);
            }
        }

        public ResourceCounter[] GetResourceCounters()
        {
            return GetComponentsInChildren<ResourceCounter>(true);
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

        private void EnsureUiSpritesLoaded()
        {
            resourceIconSprite ??= LoadSprite("Project/Resource/ResourceArt/resource", "Assets/Project/Resource/ResourceArt/resource.png");
            damageIconSprite ??= LoadSprite("Project/Abilities/AbilitiesArt/damage", "Assets/Project/Abilities/AbilitiesArt/damage.png");
            healthIconSprite ??= LoadSprite("Project/Abilities/AbilitiesArt/health", "Assets/Project/Abilities/AbilitiesArt/health.png");
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
            counterRect.sizeDelta = new Vector2(320f, 90f);

            var background = counterObject.GetComponent<Image>();
            background.color = new Color(0.12f, 0.12f, 0.16f, 0.85f);

            AddInlineIcon(counterRect, "ResourceIcon", resourceIconSprite, 46f, new Vector2(22f, 0f));

            var labelText = CreateLabel("AmountText", counterRect, "Resource: 0", 36, TextAnchor.MiddleLeft);
            var labelRect = labelText.rectTransform;
            labelRect.anchorMin = new Vector2(0f, 0f);
            labelRect.anchorMax = new Vector2(1f, 1f);
            labelRect.offsetMin = new Vector2(78f, 0f);
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

            var topPlate = new GameObject("TopLeftResourcePlate", typeof(RectTransform), typeof(Image), typeof(ResourceCounter));
            var topPlateRect = topPlate.GetComponent<RectTransform>();
            topPlateRect.SetParent(popupRect, false);
            topPlateRect.anchorMin = new Vector2(0f, 1f);
            topPlateRect.anchorMax = new Vector2(0f, 1f);
            topPlateRect.pivot = new Vector2(0f, 1f);
            topPlateRect.anchoredPosition = new Vector2(30f, -25f);
            topPlateRect.sizeDelta = new Vector2(320f, 80f);
            topPlate.GetComponent<Image>().color = new Color(0.2f, 0.2f, 0.28f, 0.95f);

            AddInlineIcon(topPlateRect, "ResourceIcon", resourceIconSprite, 36f, new Vector2(18f, 0f));

            var plateLabel = CreateLabel("PlateLabel", topPlateRect, "Resource: 0", 26, TextAnchor.MiddleLeft);
            var plateRect = plateLabel.rectTransform;
            plateRect.anchorMin = new Vector2(0f, 0f);
            plateRect.anchorMax = new Vector2(1f, 1f);
            plateRect.offsetMin = new Vector2(62f, 0f);
            plateRect.offsetMax = new Vector2(-16f, 0f);

            var popupCounter = topPlate.GetComponent<ResourceCounter>();
            popupCounter.BindAmountText(plateLabel);

            var levelText = CreateLabel("LevelText", popupRect, "Level 1", 64, TextAnchor.UpperCenter);
            var levelRect = levelText.rectTransform;
            levelRect.anchorMin = new Vector2(0.5f, 1f);
            levelRect.anchorMax = new Vector2(0.5f, 1f);
            levelRect.pivot = new Vector2(0.5f, 1f);
            levelRect.anchoredPosition = new Vector2(0f, -30f);
            levelRect.sizeDelta = new Vector2(420f, 100f);

            var damageUpgrade = CreateUpgradeButton(popupRect, "UpgradeDamageButton", "+1 Ability Damage", new Vector2(0f, 120f));
            var damagePrice = CreatePriceLabel(damageUpgrade.transform as RectTransform, "DamagePriceLabel", "Price: 1");
            AddButtonIcon(damageUpgrade.transform as RectTransform, damageIconSprite, "DamageIcon");

            var healthUpgrade = CreateUpgradeButton(popupRect, "UpgradeHealthButton", "+1 Player HP", new Vector2(0f, -20f));
            var healthPrice = CreatePriceLabel(healthUpgrade.transform as RectTransform, "HealthPriceLabel", "Price: 1");
            AddButtonIcon(healthUpgrade.transform as RectTransform, healthIconSprite, "HealthIcon");

            var startLevelButton = CreatePrimaryButton(popupRect, "StartLevelButton", "Start Level", new Vector2(0f, -250f), new Vector2(480f, 120f));

            progressionPopup = popupObject.GetComponent<ProgressionPopup>();
            progressionPopup.BindViews(levelText, damageUpgrade, healthUpgrade, startLevelButton, damagePrice, healthPrice);
        }

        private void EnsureFinishPopup()
        {
            if (finishPopup != null)
            {
                return;
            }

            var popupObject = new GameObject("FinishPopup", typeof(RectTransform), typeof(Image), typeof(FinishPopup));
            var popupRect = popupObject.GetComponent<RectTransform>();
            popupRect.SetParent(popupParent, false);
            StretchToParent(popupRect);

            var popupBackground = popupObject.GetComponent<Image>();
            popupBackground.color = new Color(0.07f, 0.08f, 0.12f, 0.92f);

            var panelObject = new GameObject("Panel", typeof(RectTransform), typeof(Image));
            var panelRect = panelObject.GetComponent<RectTransform>();
            panelRect.SetParent(popupRect, false);
            panelRect.anchorMin = new Vector2(0.5f, 0.5f);
            panelRect.anchorMax = new Vector2(0.5f, 0.5f);
            panelRect.pivot = new Vector2(0.5f, 0.5f);
            panelRect.sizeDelta = new Vector2(760f, 560f);
            panelObject.GetComponent<Image>().color = new Color(0.16f, 0.2f, 0.3f, 0.98f);

            var titleText = CreateLabel("TitleText", panelRect, "Defeat", 66, TextAnchor.UpperCenter);
            var titleRect = titleText.rectTransform;
            titleRect.anchorMin = new Vector2(0.5f, 1f);
            titleRect.anchorMax = new Vector2(0.5f, 1f);
            titleRect.pivot = new Vector2(0.5f, 1f);
            titleRect.anchoredPosition = new Vector2(0f, -44f);
            titleRect.sizeDelta = new Vector2(360f, 100f);

            var rewardRowObject = new GameObject("RewardRow", typeof(RectTransform));
            var rewardRowRect = rewardRowObject.GetComponent<RectTransform>();
            rewardRowRect.SetParent(panelRect, false);
            rewardRowRect.anchorMin = new Vector2(0.5f, 0.5f);
            rewardRowRect.anchorMax = new Vector2(0.5f, 0.5f);
            rewardRowRect.pivot = new Vector2(0.5f, 0.5f);
            rewardRowRect.anchoredPosition = new Vector2(0f, 76f);
            rewardRowRect.sizeDelta = new Vector2(360f, 84f);

            var resourceIconObject = new GameObject("ResourceIcon", typeof(RectTransform), typeof(Image));
            var resourceIconRect = resourceIconObject.GetComponent<RectTransform>();
            resourceIconRect.SetParent(rewardRowRect, false);
            resourceIconRect.anchorMin = new Vector2(0f, 0.5f);
            resourceIconRect.anchorMax = new Vector2(0f, 0.5f);
            resourceIconRect.pivot = new Vector2(0f, 0.5f);
            resourceIconRect.anchoredPosition = new Vector2(0f, 0f);
            resourceIconRect.sizeDelta = new Vector2(64f, 64f);

            var resourceIconImage = resourceIconObject.GetComponent<Image>();
            resourceIconImage.sprite = resourceIconSprite;
            resourceIconImage.color = resourceIconSprite != null ? Color.white : new Color(0.97f, 0.78f, 0.17f, 1f);

            var resourceAmountText = CreateLabel("ResourceAmountText", rewardRowRect, "+0", 48, TextAnchor.MiddleLeft);
            var resourceAmountRect = resourceAmountText.rectTransform;
            resourceAmountRect.anchorMin = new Vector2(0f, 0f);
            resourceAmountRect.anchorMax = new Vector2(1f, 1f);
            resourceAmountRect.offsetMin = new Vector2(86f, 0f);
            resourceAmountRect.offsetMax = new Vector2(0f, 0f);

            var victoryBonusObject = new GameObject("VictoryBonusBlock", typeof(RectTransform), typeof(Image));
            var victoryBonusRect = victoryBonusObject.GetComponent<RectTransform>();
            victoryBonusRect.SetParent(panelRect, false);
            victoryBonusRect.anchorMin = new Vector2(0.5f, 0.5f);
            victoryBonusRect.anchorMax = new Vector2(0.5f, 0.5f);
            victoryBonusRect.pivot = new Vector2(0.5f, 0.5f);
            victoryBonusRect.anchoredPosition = new Vector2(0f, -20f);
            victoryBonusRect.sizeDelta = new Vector2(520f, 100f);
            victoryBonusObject.GetComponent<Image>().color = new Color(0.25f, 0.35f, 0.26f, 0.95f);

            var victoryBonusLabel = CreateLabel("VictoryBonusLabel", victoryBonusRect, "Victory Bonus", 32, TextAnchor.MiddleLeft);
            var victoryBonusLabelRect = victoryBonusLabel.rectTransform;
            victoryBonusLabelRect.anchorMin = new Vector2(0f, 0f);
            victoryBonusLabelRect.anchorMax = new Vector2(1f, 1f);
            victoryBonusLabelRect.offsetMin = new Vector2(20f, 0f);
            victoryBonusLabelRect.offsetMax = new Vector2(-160f, 0f);

            var victoryBonusValueText = CreateLabel("VictoryBonusAmountText", victoryBonusRect, "+1", 34, TextAnchor.MiddleRight);
            var victoryBonusValueRect = victoryBonusValueText.rectTransform;
            victoryBonusValueRect.anchorMin = new Vector2(0f, 0f);
            victoryBonusValueRect.anchorMax = new Vector2(1f, 1f);
            victoryBonusValueRect.offsetMin = new Vector2(160f, 0f);
            victoryBonusValueRect.offsetMax = new Vector2(-24f, 0f);

            var continueButton = CreatePrimaryButton(panelRect, "ContinueButton", "Continue", new Vector2(0f, -190f), new Vector2(420f, 112f));

            finishPopup = popupObject.GetComponent<FinishPopup>();
            finishPopup.BindViews(
                titleText,
                resourceIconImage,
                resourceAmountText,
                victoryBonusObject,
                victoryBonusValueText,
                continueButton);
            finishPopup.Hide();
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

        private static void AddInlineIcon(RectTransform parent, string iconName, Sprite iconSprite, float size, Vector2 anchoredPosition)
        {
            var iconObject = new GameObject(iconName, typeof(RectTransform), typeof(Image));
            var iconRect = iconObject.GetComponent<RectTransform>();
            iconRect.SetParent(parent, false);
            iconRect.anchorMin = new Vector2(0f, 0.5f);
            iconRect.anchorMax = new Vector2(0f, 0.5f);
            iconRect.pivot = new Vector2(0f, 0.5f);
            iconRect.anchoredPosition = anchoredPosition;
            iconRect.sizeDelta = new Vector2(size, size);

            var iconImage = iconObject.GetComponent<Image>();
            iconImage.sprite = iconSprite;
            iconImage.color = iconSprite != null ? Color.white : new Color(0.97f, 0.78f, 0.17f, 1f);
        }

        private static void AddButtonIcon(RectTransform buttonRect, Sprite iconSprite, string iconName)
        {
            if (buttonRect == null)
            {
                return;
            }

            AddInlineIcon(buttonRect, iconName, iconSprite, 84f, new Vector2(18f, 0f));

            var labelTransform = buttonRect.Find("Label");
            if (labelTransform == null)
            {
                return;
            }

            var labelRect = labelTransform.GetComponent<RectTransform>();
            if (labelRect == null)
            {
                return;
            }

            labelRect.offsetMin = new Vector2(108f, 0f);
            labelRect.offsetMax = new Vector2(-16f, 0f);
        }

        private static Sprite LoadSprite(string resourcesPath, string editorAssetPath)
        {
            var sprite = Resources.Load<Sprite>(resourcesPath);

#if UNITY_EDITOR
            if (sprite == null)
            {
                sprite = AssetDatabase.LoadAssetAtPath<Sprite>(editorAssetPath);
            }
#endif

            return sprite;
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

