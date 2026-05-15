using UnityEngine;

public class SoundManager : MonoBehaviour
{
    [Header("Основные настройки")]
    [SerializeField] private AudioSource musicSource;
    [SerializeField] private bool isMusicEnabled = true;
    [SerializeField] private bool isSfxEnabled = true;

    [Range(0f, 1f)][SerializeField] private float musicVolume = 0.5f;
    [Range(0f, 1f)][SerializeField] private float sfxVolume = 0.7f;

    [Space]
    [Header("Префабы со звуковыми объектами")]
    [SerializeField] private GameObject musicObject;
    [SerializeField] private GameObject clickMainObject;
    [SerializeField] private GameObject upgradeType1Object;
    [SerializeField] private GameObject upgradeType2Object;
    [SerializeField] private GameObject levelUpObject;

    private AudioSource _musicInstance;
    private bool audioInitialized = false;

    public static SoundManager Instance { get; private set; }

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        if (musicSource == null)
            musicSource = gameObject.AddComponent<AudioSource>();
    }

    void Start()
    {
        // Для WebGL: инициализируем аудио только после первого взаимодействия
        // Не запускаем музыку автоматически
    }

    // Метод для инициализации аудио после первого клика
    public void InitializeAudio()
    {
        if (!audioInitialized)
        {
            audioInitialized = true;
            PlayMusic();
        }
    }

    // В Click.cs в OnBtnClick() добавьте вызов: SoundManager.Instance.InitializeAudio();

    public void MuteAll()
    {
        SetMusicVolume(0f);
        SetSfxVolume(0f);
        isMusicEnabled = false;
        isSfxEnabled = false;
    }

    public void UnmuteAll()
    {
        SetMusicVolume(0.1f);
        SetSfxVolume(0.5f);
        isMusicEnabled = true;
        isSfxEnabled = true;
    }

    public void PlayMusic()
    {
        if (!isMusicEnabled || musicObject == null) return;
        if (_musicInstance != null && _musicInstance.isPlaying) return;

        GameObject musicGO = Instantiate(musicObject, transform);
        _musicInstance = musicGO.GetComponent<AudioSource>();

        if (_musicInstance != null)
        {
            _musicInstance.volume = musicVolume;
            _musicInstance.loop = true;
            _musicInstance.Play();
        }
    }

    private void PlaySfx(GameObject sfxPrefab)
    {
        if (!isSfxEnabled || sfxPrefab == null) return;

        GameObject sfxGO = Instantiate(sfxPrefab, transform);
        AudioSource sfxSource = sfxGO.GetComponent<AudioSource>();

        if (sfxSource != null)
        {
            sfxSource.volume = sfxVolume;
            sfxSource.Play();
            float clipLength = sfxSource.clip.length;
            Destroy(sfxGO, clipLength + 0.1f);
        }
    }

    public void PlayMainClick() => PlaySfx(clickMainObject);
    public void PlayUpgradeType1() => PlaySfx(upgradeType1Object);
    public void PlayUpgradeType2() => PlaySfx(upgradeType2Object);
    public void PlayLevelUp() => PlaySfx(levelUpObject);

    public void SetMusicVolume(float volume)
    {
        musicVolume = Mathf.Clamp(volume, 0f, 1f);
        if (_musicInstance != null)
            _musicInstance.volume = musicVolume;
    }

    public void SetSfxVolume(float volume)
    {
        sfxVolume = Mathf.Clamp(volume, 0f, 1f);
    }
}