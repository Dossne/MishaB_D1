using System;
using TetrisTactic.Core;
using UnityEngine;
using UnityEngine.UI;

namespace TetrisTactic.FinishFlow
{
    public sealed class FinishPopup : MonoBehaviour
    {
        public event Action ContinueRequested;

        [SerializeField] private Text titleText;
        [SerializeField] private Image resourceIconImage;
        [SerializeField] private Text resourceAmountText;
        [SerializeField] private GameObject victoryBonusBlock;
        [SerializeField] private Text victoryBonusText;
        [SerializeField] private Button continueButton;

        public void Show(bool isVictory, int resourceAmount, int victoryBonusAmount)
        {
            if (titleText != null)
            {
                GameTextStyling.SetUiText(titleText, isVictory ? "Victory" : "Defeat");
            }

            if (resourceAmountText != null)
            {
                GameTextStyling.SetUiText(resourceAmountText, $"+{Mathf.Max(0, resourceAmount)}");
            }

            if (victoryBonusBlock != null)
            {
                victoryBonusBlock.SetActive(isVictory);
            }

            if (victoryBonusText != null)
            {
                GameTextStyling.SetUiText(victoryBonusText, $"+{Mathf.Max(0, victoryBonusAmount)}");
            }

            gameObject.SetActive(true);
        }

        public void Hide()
        {
            gameObject.SetActive(false);
        }

        public void BindViews(
            Text title,
            Image icon,
            Text resourceAmount,
            GameObject bonusBlock,
            Text bonusAmount,
            Button continueActionButton)
        {
            titleText = title;
            resourceIconImage = icon;
            resourceAmountText = resourceAmount;
            victoryBonusBlock = bonusBlock;
            victoryBonusText = bonusAmount;
            continueButton = continueActionButton;

            BindListeners();
        }

        private void BindListeners()
        {
            if (continueButton == null)
            {
                return;
            }

            continueButton.onClick.RemoveListener(OnContinuePressed);
            continueButton.onClick.AddListener(OnContinuePressed);
        }

        private void OnContinuePressed()
        {
            Debug.Log("FinishPopup: Continue button pressed.");
            ContinueRequested?.Invoke();
        }
    }
}

