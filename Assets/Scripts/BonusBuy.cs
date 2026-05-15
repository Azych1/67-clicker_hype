using TMPro;
using UnityEngine;
using UnityEngine.UI;
using YG;

public class BonusBuy : MonoBehaviour
{
    public static BonusBuy Instance;

    [System.Serializable]
    public class Upgrade
    {
        public string upgradeName;
        public Button buyButton;
        public TextMeshProUGUI costText;
        public TextMeshProUGUI levelText;
        public TextMeshProUGUI maxText;
        public TextMeshProUGUI costWordText;

        public int baseCost = 10;
        public int bonusPerClick = 1;
        public float bonusPerSecond = 0f;
        public int maxLevel = 10;

        [HideInInspector] public int currentLevel = 0;
        [HideInInspector] public int currentCost;
        [HideInInspector] public int upgradeIndex = -1;

        public void Initialize(int index)
        {
            upgradeIndex = index;

            // Загружаем сохраненный уровень из YG2
            if (YG2.isSDKEnabled && YG2.saves.upgradeLevels != null &&
                index < YG2.saves.upgradeLevels.Length)
            {
                currentLevel = YG2.saves.GetUpgradeLevel(index);
            }
            else
            {
                currentLevel = 0;
            }

            // Рассчитываем текущую стоимость на основе уровня
            currentCost = baseCost * (int)Mathf.Pow(2, currentLevel);
            UpdateUI();
        }

        public void UpdateUI()
        {
            // Форматируем стоимость с "Т"
            string formattedCost = FormatCost(currentCost);

            if (costText != null)
                costText.text = $"{formattedCost}";

            if (levelText != null)
                levelText.text = $"{currentLevel}";

            // Показываем "МАКС" если достигнут максимальный уровень
            if (currentLevel >= maxLevel)
            {
                if (costText != null) costText.gameObject.SetActive(false);
                if (maxText != null)
                {
                    maxText.gameObject.SetActive(true);
                    //maxText.text = "МАКС";
                    if (costWordText != null) costWordText.gameObject.SetActive(false);
                }
            }
            else
            {
                if (costText != null)
                {
                    costText.text = $"{formattedCost}";
                    costText.gameObject.SetActive(true);
                }
                if (maxText != null) maxText.gameObject.SetActive(false);
            }
        }

        private string FormatCost(int value)
        {
            if (value >= 1000)
            {
                float thousands = value / 1000f;
                return $"{thousands:0.#}Т";
            }
            return value.ToString();
        }

        public bool CanBuy(int gold)
        {
            return currentLevel < maxLevel && gold >= currentCost;
        }

        public void Buy()
        {
            if (!CanBuy(Click.Instance.GetGold())) return;

            // Получаем текущие значения
            int playerGold = Click.Instance.GetGold();
            int playerGoldPerClick = Click.Instance.GetGoldPerClick();
            float playerGoldPerSecond = Click.Instance.GetGoldPerSecond();

            // Применяем покупку
            playerGold -= currentCost;
            playerGoldPerClick += bonusPerClick;
            playerGoldPerSecond += bonusPerSecond;
            currentLevel++;
            currentCost *= 2;

            // Сохраняем изменения
            Click.Instance.SetGold(playerGold);
            Click.Instance.SetGoldPerClick(playerGoldPerClick);
            Click.Instance.SetGoldPerSecond(playerGoldPerSecond);

            // Сохраняем уровень улучшения
            if (YG2.isSDKEnabled)
            {
                YG2.saves.SetUpgradeLevel(upgradeIndex, currentLevel);
            }

            // Сохраняем ВСЕ прогресс
            if (Click.Instance != null)
            {
                Click.Instance.SaveAllProgress();
            }

            UpdateUI();

            // Обновляем UI в Click
            Click.Instance.UpdateUI();

            // Проигрываем звук
            if (SoundManager.Instance != null)
            {
                if (bonusPerSecond >= 1f)
                    SoundManager.Instance.PlayUpgradeType2();
                else
                    SoundManager.Instance.PlayUpgradeType1();
            }
        }
    }

    public Upgrade[] upgrades;

    void Start()
    {
        Instance = this;

        // Инициализируем улучшения с индексами
        for (int i = 0; i < upgrades.Length; i++)
        {
            upgrades[i].Initialize(i);
            Upgrade currentUpgrade = upgrades[i];
            upgrades[i].buyButton.onClick.AddListener(() => BuyUpgrade(currentUpgrade));
        }

        UpdateButtons();

        // Применяем уровни улучшений при старте
        ApplyUpgradeLevels();
    }

    void Update()
    {
        UpdateButtons();
    }

    public void BuyUpgrade(Upgrade upgrade)
    {
        if (upgrade.CanBuy(Click.Instance.GetGold()))
        {
            upgrade.Buy();
            UpdateButtons();
        }
    }

    private void UpdateButtons()
    {
        int playerGold = Click.Instance.GetGold();

        foreach (var upgrade in upgrades)
        {
            if (upgrade.buyButton != null)
                upgrade.buyButton.interactable = upgrade.CanBuy(playerGold);
        }
    }

    // Применяем все купленные улучшения при загрузке
    private void ApplyUpgradeLevels()
    {
        int totalBonusPerClick = 0;
        float totalBonusPerSecond = 0f;

        // Суммируем бонусы от всех купленных улучшений
        foreach (var upgrade in upgrades)
        {
            if (upgrade.currentLevel > 0)
            {
                totalBonusPerClick += upgrade.bonusPerClick * upgrade.currentLevel;
                totalBonusPerSecond += upgrade.bonusPerSecond * upgrade.currentLevel;
            }
        }

        // Устанавливаем итоговые значения (база + бонусы)
        int baseGoldPerClick = 1;
        float baseGoldPerSecond = 0f;

        int finalGoldPerClick = baseGoldPerClick + totalBonusPerClick;
        float finalGoldPerSecond = baseGoldPerSecond + totalBonusPerSecond;

        Click.Instance.SetGoldPerClick(finalGoldPerClick);
        Click.Instance.SetGoldPerSecond(finalGoldPerSecond);

        // Обновляем UI
        Click.Instance.UpdateUI();

        // Сохраняем изменения
        if (Click.Instance != null)
        {
            Click.Instance.SaveAllProgress();
        }
    }
}