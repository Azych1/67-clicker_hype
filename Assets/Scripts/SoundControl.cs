using UnityEngine;
using UnityEngine.UI;

public class SoundControl : MonoBehaviour
{
    [SerializeField] private Button muteButton;      // Кнопка «Отключить звук»
    [SerializeField] private Button unmuteButton;    // Кнопка «Включить звук»

    private bool isSoundOn = true;

    void Start()
    {
        // При старте показываем кнопку отключения, если звук включён
        muteButton.gameObject.SetActive(isSoundOn);
        unmuteButton.gameObject.SetActive(!isSoundOn);

        // Назначаем обработчики кликов
        muteButton.onClick.AddListener(OnMuteClicked);
        unmuteButton.onClick.AddListener(OnUnmuteClicked);
    }

    void OnMuteClicked()
    {
        SoundManager.Instance.MuteAll();
        isSoundOn = false;
        UpdateButtons();
    }

    void OnUnmuteClicked()
    {
        SoundManager.Instance.UnmuteAll();
        isSoundOn = true;
        UpdateButtons();
    }

    void UpdateButtons()
    {
        muteButton.gameObject.SetActive(isSoundOn);
        unmuteButton.gameObject.SetActive(!isSoundOn);
    }
}
