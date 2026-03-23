using System.Collections;
using TetrisTactic.Core;
using TetrisTactic.Progression;
using UnityEngine;
using UnityEngine.UI;

namespace TetrisTactic.MainUi
{
    public sealed class ProgressionPopup : MonoBehaviour
    {
        private static readonly Color AffordablePriceColor = Color.white;
        private static readonly Color UnaffordablePriceColor = new Color(1f, 0.34f, 0.34f, 1f);

        public event System.Action StartLevelRequested;
        public event System.Action<PlayerUpgradeType> UpgradeRequested;

        [SerializeField] private Text levelText;
        [SerializeField] private Button upgradeDamageButton;
        [SerializeField] private Button upgradeHealthButton;
        [SerializeField] private Button startLevelButton;
        [SerializeField] private Text upgradeDamagePriceText;
        [SerializeField] private Text upgradeHealthPriceText;
        [SerializeField] private Text upgradeDamageValueText;
        [SerializeField] private Text upgradeHealthValueText;

        private Coroutine valueBounceRoutine;

        private void Awake()
        {
            RefreshPriceLabels(1, 1, 0);
            BindListeners();
        }

        public void Initialize()
        {
            // Reserved for future view setup.
        }

        public void Show(int level)
        {
            if (levelText != null)
            {
                GameTextStyling.SetUiText(levelText, $"Level {level}");
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
            Text healthPrice,
            Text damageValue,
            Text healthValue)
        {
            levelText = level;
            upgradeDamageButton = damageUpgrade;
            upgradeHealthButton = healthUpgrade;
            startLevelButton = startLevel;
            upgradeDamagePriceText = damagePrice;
            upgradeHealthPriceText = healthPrice;
            upgradeDamageValueText = damageValue;
            upgradeHealthValueText = healthValue;

            BindListeners();
            RefreshPriceLabels(1, 1, 0);
            SetCurrentUpgradeValues(1, 1);
        }

        public void RefreshUpgradeState(int damageValue, int healthValue, int upgradePrice, int currentResource)
        {
            SetCurrentUpgradeValues(damageValue, healthValue);
            RefreshPriceLabels(upgradePrice, upgradePrice, currentResource);
        }

        public void PlayUpgradeBounce(PlayerUpgradeType upgradeType)
        {
            var target = upgradeType == PlayerUpgradeType.Damage ? upgradeDamageValueText : upgradeHealthValueText;
            if (target == null)
            {
                return;
            }

            if (valueBounceRoutine != null)
            {
                StopCoroutine(valueBounceRoutine);
            }

            valueBounceRoutine = StartCoroutine(BounceTextRoutine(target.rectTransform));
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

        private void SetCurrentUpgradeValues(int damageValue, int healthValue)
        {
            if (upgradeDamageValueText != null)
            {
                GameTextStyling.SetUiText(upgradeDamageValueText, damageValue.ToString());
            }

            if (upgradeHealthValueText != null)
            {
                GameTextStyling.SetUiText(upgradeHealthValueText, healthValue.ToString());
            }
        }

        private void RefreshPriceLabels(int damagePrice, int healthPrice, int currentResource)
        {
            if (upgradeDamagePriceText != null)
            {
                upgradeDamagePriceText.text = FormatPriceShort(damagePrice);
                upgradeDamagePriceText.color = currentResource >= damagePrice ? AffordablePriceColor : UnaffordablePriceColor;
            }

            if (upgradeHealthPriceText != null)
            {
                upgradeHealthPriceText.text = FormatPriceShort(healthPrice);
                upgradeHealthPriceText.color = currentResource >= healthPrice ? AffordablePriceColor : UnaffordablePriceColor;
            }
        }


        private static string FormatPriceShort(int price)
        {
            var safePrice = Mathf.Max(0, price);
            var asText = safePrice.ToString();
            if (asText.Length <= 3)
            {
                return asText;
            }

            var thousands = Mathf.Max(1, safePrice / 1000);
            if (thousands >= 100)
            {
                return "99k";
            }

            return $"{thousands}k";
        }

        private static IEnumerator BounceTextRoutine(RectTransform target)
        {
            if (target == null)
            {
                yield break;
            }

            var duration = 0.24f;
            var elapsed = 0f;
            var baseScale = Vector3.one;

            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;
                var t = Mathf.Clamp01(elapsed / duration);
                var pulse = Mathf.Sin(t * Mathf.PI);
                var scale = 1f + (0.22f * pulse);
                target.localScale = baseScale * scale;
                yield return null;
            }

            target.localScale = baseScale;
        }

        private void OnUpgradeDamagePressed()
        {
            UpgradeRequested?.Invoke(PlayerUpgradeType.Damage);
        }

        private void OnUpgradeHealthPressed()
        {
            UpgradeRequested?.Invoke(PlayerUpgradeType.Health);
        }

        private void OnStartLevelPressed()
        {
            Debug.Log("ProgressionPopup: Start Level button pressed.");
            StartLevelRequested?.Invoke();
        }
    }
}





