using TMPro;
using UnityEngine;
using UnityEngine.UI;
using YG;

public class Click : MonoBehaviour
{
    public static Click Instance;

    [SerializeField] private TextMeshProUGUI goldText;
    [SerializeField] private TextMeshProUGUI goldPerClickText;
    [SerializeField] private TextMeshProUGUI goldPerSecondText;
    [SerializeField] private TextMeshProUGUI levelText;
    [SerializeField] private TextMeshProUGUI clicksText;
    [SerializeField] private TextMeshProUGUI maxLevelText; // Новое поле для текста "МАКС"
    [SerializeField] private Button mainButton;
    [SerializeField] private Sprite[] levelSprites;

    private float goldAccumulator = 0f;
    private float updateInterval = 0.1f;
    private float timer = 0f;
    private float saveInterval = 1.0f;
    private float saveTimer = 0f;

    private const int MAX_LEVEL = 7;
    private int clicksForNextLevel = 800;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        if (YG2.isSDKEnabled)
        {
            YG2.onGetSDKData += OnDataLoaded;
        }
    }

    private void OnDestroy()
    {
        if (YG2.isSDKEnabled)
        {
            YG2.onGetSDKData -= OnDataLoaded;
        }
    }

    private void OnDataLoaded()
    {
        // Загружаем сохраненные значения
        if (YG2.isSDKEnabled)
        {
            // Если это новая игра, инициализируем значения
            if (YG2.saves.currentLevel == 0)
            {
                YG2.saves.currentLevel = 1;
                YG2.saves.clicksForNextLevel = 800;
                SaveAllProgress();
            }

            // Восстанавливаем clicksForNextLevel из сохранений
            clicksForNextLevel = YG2.saves.clicksForNextLevel;
        }

        // Устанавливаем правильный спрайт для текущего уровня при загрузке
        SetButtonSpriteForLevel(GetCurrentLevel());

        UpdateUI();
    }

    void Start()
    {
        // Также устанавливаем спрайт в Start на случай, если данные уже загружены
        SetButtonSpriteForLevel(GetCurrentLevel());
        UpdateUI();
    }

    void Update()
    {
        // Автоматическое накопление золота в секунду
        timer += Time.deltaTime;
        saveTimer += Time.deltaTime;

        // Получаем текущее значение goldPerSecond
        float currentGoldPerSecond = GetGoldPerSecond();
        goldAccumulator += currentGoldPerSecond * Time.deltaTime;

        if (timer >= updateInterval && currentGoldPerSecond > 0)
        {
            // Добавляем накопленное золото
            if (goldAccumulator >= 1f)
            {
                int earned = Mathf.FloorToInt(goldAccumulator);
                SetGold(GetGold() + earned);
                goldAccumulator -= earned;

                if (earned > 0)
                {
                    UpdateUI();

                    // Сохраняем каждую секунду при накоплении goldPerSecond
                    if (saveTimer >= saveInterval && YG2.isSDKEnabled)
                    {
                        SaveAllProgress();
                        saveTimer = 0f;
                    }
                }
            }
            timer = 0f;
        }

        // Также сохраняем если прошла секунда (на случай если goldPerSecond очень маленький)
        if (saveTimer >= saveInterval && YG2.isSDKEnabled && currentGoldPerSecond > 0)
        {
            SaveAllProgress();
            saveTimer = 0f;
        }
    }

    public void OnBtnClick()
    {
        // Инициализируем аудио при первом клике (для WebGL)
        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.InitializeAudio();
        }

        // Увеличиваем золото
        SetGold(GetGold() + GetGoldPerClick());

        // Увеличиваем счетчик кликов
        if (YG2.isSDKEnabled)
        {
            YG2.saves.totalClicks++;

            // Проверяем, нужно ли повысить уровень
            if (YG2.saves.currentLevel < MAX_LEVEL &&
                YG2.saves.totalClicks >= YG2.saves.clicksForNextLevel)
            {
                LevelUp();
            }

            // Сохраняем после клика
            SaveAllProgress();
            saveTimer = 0f;
        }

        // Воспроизводим звук клика
        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.PlayMainClick();
        }

        UpdateUI();
    }

    private void LevelUp()
    {
        if (YG2.isSDKEnabled)
        {
            YG2.saves.currentLevel++;
            YG2.saves.clicksForNextLevel *= 2;

            // Обновляем локальную переменную
            clicksForNextLevel = YG2.saves.clicksForNextLevel;

            SaveAllProgress();

            // Устанавливаем новый спрайт кнопки
            SetButtonSpriteForLevel(YG2.saves.currentLevel);

            // Воспроизводим звук повышения уровня
            if (SoundManager.Instance != null)
            {
                SoundManager.Instance.PlayLevelUp();
            }

            Debug.Log($"Новый уровень: {YG2.saves.currentLevel}! Кликов до следующего: {YG2.saves.clicksForNextLevel}");
        }
    }

    private void SetButtonSpriteForLevel(int level)
    {
        if (mainButton != null && levelSprites != null && levelSprites.Length > 0)
        {
            // Ограничиваем уровень в пределах доступных спрайтов
            int safeLevel = Mathf.Clamp(level, 1, Mathf.Min(levelSprites.Length, MAX_LEVEL)) - 1;

            if (safeLevel >= 0 && safeLevel < levelSprites.Length)
            {
                Image btnImage = mainButton.GetComponent<Image>();
                if (btnImage != null && levelSprites[safeLevel] != null)
                {
                    btnImage.sprite = levelSprites[safeLevel];
                }
            }
        }
    }

    public void UpdateUI()
    {
        // Золото без форматирования - просто число
        if (goldText != null)
        {
            goldText.text = GetGold().ToString();
        }

        // Gold per click с форматированием (с "Т" для тысяч)
        if (goldPerClickText != null)
        {
            goldPerClickText.text = $"+{FormatNumber(GetGoldPerClick())}";
        }

        // Gold per second с форматированием (с "Т" для тысяч)
        if (goldPerSecondText != null)
        {
            goldPerSecondText.text = $"+{FormatNumber(GetGoldPerSecond())}";
        }

        // Уровень
        if (levelText != null)
        {
            levelText.text = $"{GetCurrentLevel()}";
        }

        // Клики до следующего уровня и текст "МАКС"
        if (clicksText != null && maxLevelText != null)
        {
            int currentClicks = GetTotalClicks();
            int neededClicks = GetClicksForNextLevel();

            if (GetCurrentLevel() >= MAX_LEVEL)
            {
                // Скрываем основной текст кликов
                clicksText.text = "";

                // Показываем текст "МАКС"
                maxLevelText.gameObject.SetActive(true);
            }
            else
            {
                // Показываем прогресс кликов
                clicksText.text = $"{currentClicks}/{neededClicks}";

                // Скрываем текст "МАКС"
                maxLevelText.gameObject.SetActive(false);
            }
        }
        else if (clicksText != null) // Если maxLevelText не назначен, оставляем старую логику
        {
            int currentClicks = GetTotalClicks();
            int neededClicks = GetClicksForNextLevel();

            if (GetCurrentLevel() >= MAX_LEVEL)
            {
                clicksText.text = "МАКС";
            }
            else
            {
                clicksText.text = $"{currentClicks}/{neededClicks}";
            }
        }
    }

    private string FormatNumber(int num)
    {
        if (num >= 1000)
        {
            float thousands = num / 1000f;
            return $"{thousands:0.#}Т";
        }
        return num.ToString();
    }

    private string FormatNumber(float num)
    {
        if (num >= 1000)
        {
            float thousands = num / 1000f;
            return $"{thousands:0.#}Т";
        }
        return num.ToString("0.#");
    }

    public int GetGold()
    {
        if (YG2.isSDKEnabled)
        {
            return YG2.saves.gold;
        }
        return 0;
    }

    public void SetGold(int value)
    {
        if (YG2.isSDKEnabled)
        {
            YG2.saves.gold = value;
        }
    }

    public int GetGoldPerClick()
    {
        if (YG2.isSDKEnabled)
        {
            return YG2.saves.goldPerClick;
        }
        return 1;
    }

    public void SetGoldPerClick(int value)
    {
        if (YG2.isSDKEnabled)
        {
            YG2.saves.goldPerClick = value;
        }
    }

    public float GetGoldPerSecond()
    {
        if (YG2.isSDKEnabled)
        {
            return YG2.saves.goldPerSecond;
        }
        return 0f;
    }

    public void SetGoldPerSecond(float value)
    {
        if (YG2.isSDKEnabled)
        {
            YG2.saves.goldPerSecond = value;
        }
    }

    public int GetTotalClicks()
    {
        if (YG2.isSDKEnabled)
        {
            return YG2.saves.totalClicks;
        }
        return 0;
    }

    public int GetCurrentLevel()
    {
        if (YG2.isSDKEnabled)
        {
            return YG2.saves.currentLevel;
        }
        return 1;
    }

    public int GetClicksForNextLevel()
    {
        if (YG2.isSDKEnabled)
        {
            return YG2.saves.clicksForNextLevel;
        }
        return clicksForNextLevel;
    }

    // Метод для сохранения всех текущих значений
    public void SaveAllProgress()
    {
        if (YG2.isSDKEnabled && YG2.saves != null)
        {
            // Явно обновляем все значения в saves перед сохранением
            YG2.saves.gold = GetGold();
            YG2.saves.goldPerClick = GetGoldPerClick();
            YG2.saves.goldPerSecond = GetGoldPerSecond();
            YG2.saves.currentLevel = GetCurrentLevel();
            YG2.saves.clicksForNextLevel = GetClicksForNextLevel();
            YG2.saves.totalClicks = GetTotalClicks();

            YG2.SaveProgress();
        }
    }
}