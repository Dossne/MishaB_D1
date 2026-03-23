using System;
using System.Collections.Generic;
using TetrisTactic.Abilities;
using UnityEngine;
using UnityEngine.UI;

namespace TetrisTactic.PlayerTurn
{
    public sealed class PlayerActionPanel : MonoBehaviour
    {
        [SerializeField] private WaitButtonView waitButtonView;
        [SerializeField] private RectTransform actionRowRoot;

        private readonly List<AbilityButtonView> abilityButtons = new();

        public WaitButtonView WaitButtonView => waitButtonView;

        public void Initialize(RectTransform hudParent)
        {
            var rectTransform = GetComponent<RectTransform>();
            rectTransform.SetParent(hudParent, false);
            rectTransform.anchorMin = new Vector2(0f, 0f);
            rectTransform.anchorMax = new Vector2(1f, 0f);
            rectTransform.pivot = new Vector2(0.5f, 0f);
            rectTransform.anchoredPosition = Vector2.zero;
            rectTransform.sizeDelta = new Vector2(0f, 150f);

            var background = GetComponent<Image>();
            background.color = new Color(0.07f, 0.1f, 0.16f, 0.9f);

            if (actionRowRoot == null)
            {
                actionRowRoot = CreateActionRowRoot(rectTransform);
            }

            if (waitButtonView == null)
            {
                waitButtonView = CreateWaitButton(actionRowRoot);
            }

            gameObject.SetActive(false);
        }

        public void Show()
        {
            gameObject.SetActive(true);
        }

        public void Hide()
        {
            gameObject.SetActive(false);
        }

        public void SetWaitInteractable(bool canUse)
        {
            if (waitButtonView != null)
            {
                waitButtonView.SetInteractable(canUse);
            }
        }

        public void EnsureAbilityButtonCount(int count)
        {
            if (count < 0)
            {
                count = 0;
            }

            while (abilityButtons.Count < count)
            {
                abilityButtons.Add(CreateAbilityButton(actionRowRoot, abilityButtons.Count));
            }

            for (var i = 0; i < abilityButtons.Count; i++)
            {
                var isActive = i < count;
                abilityButtons[i].gameObject.SetActive(isActive);
                if (isActive)
                {
                    SetSlotAnchors(abilityButtons[i].GetComponent<RectTransform>(), i + 1);
                }
            }
        }

        public void ConfigureAbilityButton(int index, string label, bool selected, bool interactable, Action onPressed)
        {
            if (index < 0 || index >= abilityButtons.Count)
            {
                return;
            }

            abilityButtons[index].Bind(label, onPressed, interactable, selected);
            abilityButtons[index].gameObject.SetActive(true);
        }

        private static RectTransform CreateActionRowRoot(RectTransform panelTransform)
        {
            var rootObject = new GameObject("ActionRowRoot", typeof(RectTransform));
            var root = rootObject.GetComponent<RectTransform>();
            root.SetParent(panelTransform, false);
            root.anchorMin = new Vector2(0.5f, 0.5f);
            root.anchorMax = new Vector2(0.5f, 0.5f);
            root.pivot = new Vector2(0.5f, 0.5f);
            root.anchoredPosition = Vector2.zero;
            root.sizeDelta = new Vector2(1000f, 96f);
            return root;
        }

        private static WaitButtonView CreateWaitButton(RectTransform rowRoot)
        {
            var waitButtonObject = new GameObject("WaitButton", typeof(RectTransform), typeof(Image), typeof(Button), typeof(WaitButtonView));
            var waitButtonRect = waitButtonObject.GetComponent<RectTransform>();
            waitButtonRect.SetParent(rowRoot, false);
            waitButtonRect.sizeDelta = new Vector2(232f, 84f);
            SetSlotAnchors(waitButtonRect, 0);

            var buttonImage = waitButtonObject.GetComponent<Image>();
            buttonImage.color = new Color(0.23f, 0.36f, 0.55f, 1f);

            var waitLabelObject = new GameObject("Label", typeof(RectTransform), typeof(Text));
            var waitLabelRect = waitLabelObject.GetComponent<RectTransform>();
            waitLabelRect.SetParent(waitButtonRect, false);
            waitLabelRect.anchorMin = Vector2.zero;
            waitLabelRect.anchorMax = Vector2.one;
            waitLabelRect.offsetMin = Vector2.zero;
            waitLabelRect.offsetMax = Vector2.zero;

            var waitLabel = waitLabelObject.GetComponent<Text>();
            waitLabel.text = "Wait";
            waitLabel.color = Color.white;
            waitLabel.alignment = TextAnchor.MiddleCenter;
            waitLabel.fontSize = 30;
            waitLabel.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

            var waitButton = waitButtonObject.GetComponent<Button>();
            var waitButtonView = waitButtonObject.GetComponent<WaitButtonView>();
            waitButtonView.Bind(waitButton);
            return waitButtonView;
        }

        private static AbilityButtonView CreateAbilityButton(RectTransform rowRoot, int index)
        {
            var buttonObject = new GameObject($"AbilityButton_{index}", typeof(RectTransform), typeof(Image), typeof(Button), typeof(AbilityButtonView));
            var rectTransform = buttonObject.GetComponent<RectTransform>();
            rectTransform.SetParent(rowRoot, false);
            rectTransform.sizeDelta = new Vector2(232f, 84f);

            var image = buttonObject.GetComponent<Image>();
            image.color = new Color(0.24f, 0.31f, 0.44f, 1f);

            var textObject = new GameObject("Label", typeof(RectTransform), typeof(Text));
            var textRect = textObject.GetComponent<RectTransform>();
            textRect.SetParent(rectTransform, false);
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = new Vector2(12f, 8f);
            textRect.offsetMax = new Vector2(-12f, -8f);

            var text = textObject.GetComponent<Text>();
            text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            text.fontSize = 30;
            text.alignment = TextAnchor.MiddleCenter;
            text.color = Color.white;

            var view = buttonObject.GetComponent<AbilityButtonView>();
            view.BindView(buttonObject.GetComponent<Button>(), image, text);
            return view;
        }

        private static void SetSlotAnchors(RectTransform rect, int slotIndex)
        {
            var slotWidth = 232f;
            var spacing = 16f;
            var totalWidth = (slotWidth * 4f) + (spacing * 3f);
            var startX = -totalWidth * 0.5f + slotWidth * 0.5f;

            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = new Vector2(startX + (slotWidth + spacing) * slotIndex, 0f);
        }
    }
}