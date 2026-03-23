using UnityEngine;
using UnityEngine.UI;

namespace TetrisTactic.MainUi
{
    public sealed class ProgressionPopup : MonoBehaviour
    {
        public event System.Action StartLevelRequested;

        [SerializeField] private Text levelText;
        [SerializeField] private Button upgradeDamageButton;
        [SerializeField] private Button upgradeHealthButton;
        [SerializeField] private Button startLevelButton;
        [SerializeField] private Text upgradeDamagePriceText;
        [SerializeField] private Text upgradeHealthPriceText;

        private void Awake()
        {
            RefreshPriceLabels("1", "1");
        }

        public void Show(int level)
        {
            if (levelText != null)
            {
                levelText.text = $"Level {level}";
            }

            gameObject.SetActive(true);
        }

        public void Hide()
        {
            gameObject.SetActive(false);
        }

        public void BindViews(
            Text level,
            Button damageUpgrade,
            Button healthUpgrade,
            Button startLevel,
            Text damagePrice,
            Text healthPrice)
        {
            levelText = level;
            upgradeDamageButton = damageUpgrade;
            upgradeHealthButton = healthUpgrade;
            startLevelButton = startLevel;
            upgradeDamagePriceText = damagePrice;
            upgradeHealthPriceText = healthPrice;

            BindListeners();
            RefreshPriceLabels("1", "1");
        }

        private void BindListeners()
        {
            if (upgradeDamageButton != null)
            {
                upgradeDamageButton.onClick.RemoveListener(OnUpgradeDamagePressed);
                upgradeDamageButton.onClick.AddListener(OnUpgradeDamagePressed);
            }

            if (upgradeHealthButton != null)
            {
                upgradeHealthButton.onClick.RemoveListener(OnUpgradeHealthPressed);
                upgradeHealthButton.onClick.AddListener(OnUpgradeHealthPressed);
            }

            if (startLevelButton != null)
            {
                startLevelButton.onClick.RemoveListener(OnStartLevelPressed);
                startLevelButton.onClick.AddListener(OnStartLevelPressed);
            }
        }

        private void RefreshPriceLabels(string damagePrice, string healthPrice)
        {
            if (upgradeDamagePriceText != null)
            {
                upgradeDamagePriceText.text = $"Price: {damagePrice}";
            }

            if (upgradeHealthPriceText != null)
            {
                upgradeHealthPriceText.text = $"Price: {healthPrice}";
            }
        }

        private void OnUpgradeDamagePressed()
        {
            Debug.Log("ProgressionPopup: Damage upgrade button pressed.");
        }

        private void OnUpgradeHealthPressed()
        {
            Debug.Log("ProgressionPopup: Health upgrade button pressed.");
        }

        private void OnStartLevelPressed()
        {
            Debug.Log("ProgressionPopup: Start Level button pressed.");
            StartLevelRequested?.Invoke();
        }
    }
}
