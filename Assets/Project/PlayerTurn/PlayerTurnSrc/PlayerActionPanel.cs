using UnityEngine;
using UnityEngine.UI;

namespace TetrisTactic.PlayerTurn
{
    public sealed class PlayerActionPanel : MonoBehaviour
    {
        [SerializeField] private WaitButtonView waitButtonView;

        public WaitButtonView WaitButtonView => waitButtonView;

        public void Initialize(RectTransform hudParent)
        {
            var rectTransform = GetComponent<RectTransform>();
            rectTransform.SetParent(hudParent, false);
            rectTransform.anchorMin = new Vector2(0f, 0f);
            rectTransform.anchorMax = new Vector2(1f, 0f);
            rectTransform.pivot = new Vector2(0.5f, 0f);
            rectTransform.anchoredPosition = Vector2.zero;
            rectTransform.sizeDelta = new Vector2(0f, 170f);

            var background = GetComponent<Image>();
            background.color = new Color(0.07f, 0.1f, 0.16f, 0.9f);

            if (waitButtonView == null)
            {
                waitButtonView = CreateWaitButton(rectTransform);
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

        private static WaitButtonView CreateWaitButton(RectTransform panelTransform)
        {
            var waitButtonObject = new GameObject("WaitButton", typeof(RectTransform), typeof(Image), typeof(Button), typeof(WaitButtonView));
            var waitButtonRect = waitButtonObject.GetComponent<RectTransform>();
            waitButtonRect.SetParent(panelTransform, false);
            waitButtonRect.anchorMin = new Vector2(0.5f, 0.5f);
            waitButtonRect.anchorMax = new Vector2(0.5f, 0.5f);
            waitButtonRect.pivot = new Vector2(0.5f, 0.5f);
            waitButtonRect.sizeDelta = new Vector2(420f, 96f);

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
            waitLabel.fontSize = 36;
            waitLabel.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

            var waitButton = waitButtonObject.GetComponent<Button>();
            var waitButtonView = waitButtonObject.GetComponent<WaitButtonView>();
            waitButtonView.Bind(waitButton);
            return waitButtonView;
        }
    }
}
