using UnityEngine;
using UnityEngine.UI;
using YG;

public class AdManager : MonoBehaviour
{
    [SerializeField] private Button adButton;

    void Start()
    {
        if (adButton != null)
        {
            adButton.onClick.AddListener(ShowRewardedAd);
        }
    }

    private void ShowRewardedAd()
    {
        // В редакторе - имитация
#if UNITY_EDITOR
        Debug.Log("Реклама (имитация в редакторе)");
        Invoke("GiveReward", 1f);
#else
        // В WebGL - используем рекламу
        // YG2.RewardedAdvShow проверяет, что реклама действительно просмотрена
        YG2.RewardedAdvShow("main_rewarded_ad", () => 
        {
            if (YG2.isSDKEnabled) 
            {
                GiveReward();
            }
        });
#endif
    }

    private void GiveReward()
    {
        if (Click.Instance == null)
        {
            Debug.LogError("Click.Instance is null!");
            return;
        }

        if (!YG2.isSDKEnabled)
        {
            Debug.LogError("YG2 SDK is not enabled!");
            return;
        }

        if (YG2.saves == null)
        {
            Debug.LogError("YG2.saves is null!");
            return;
        }

        // Получаем текущее значение
        int currentGoldPerClick = Click.Instance.GetGoldPerClick();
        Debug.Log($"Текущий goldPerClick: {currentGoldPerClick}");

        // Удваиваем
        int newGoldPerClick = currentGoldPerClick * 2;
        Debug.Log($"Новый goldPerClick после рекламы: {newGoldPerClick}");

        // Вариант 1: Используем метод Click.Instance
        Click.Instance.SetGoldPerClick(newGoldPerClick);

        // Вариант 2: Явно устанавливаем в YG2.saves (для надежности)
        YG2.saves.goldPerClick = newGoldPerClick;

        // Сохраняем ВСЕ прогресс
        YG2.SaveProgress();

        // Обновляем UI
        Click.Instance.UpdateUI();

        Debug.Log($"Награда за рекламу: goldPerClick увеличен до {newGoldPerClick}! Сохранено.");

        // Дополнительная проверка
        int savedValue = Click.Instance.GetGoldPerClick();
        Debug.Log($"Проверка после сохранения: GetGoldPerClick() = {savedValue}");
    }
}