using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenu : MonoBehaviour
{
    [Header("Panel References")]
    public GameObject mainMenuPanel;      // Панель с главными кнопками
    public GameObject settingsPanel;      // Панель с настройками

    [Header("Settings References")]
    public Slider musicVolumeSlider;      // Ползунок громкости музыки
    public Slider sfxVolumeSlider;        // Ползунок громкости звуков
    public Slider mouseSensitivitySlider; // Ползунок чувствительности мыши
    public TextMeshProUGUI musicVolumeText;          // Текст с значением громкости музыки
    public TextMeshProUGUI sfxVolumeText;            // Текст с значением громкости звуков
    public TextMeshProUGUI mouseSensitivityText;     // Текст с значением чувствительности мыши

    [Header("Scene Settings")]
    public string gameSceneName = "Game"; // Имя сцены игры

    [Header("Audio")]
    public AudioSource clickSound;        // Звук нажатия на кнопку

    void Start()
    {
        // Показываем главное меню, скрываем настройки
        ShowMainMenu();

        // Загружаем настройки из SettingsManager
        LoadSettingsFromPlayerPrefs();
        LoadSettingsFromManager();

        // Добавляем слушатели для ползунков
        if (musicVolumeSlider != null)
            musicVolumeSlider.onValueChanged.AddListener(OnMusicVolumeChanged);

        if (sfxVolumeSlider != null)
            sfxVolumeSlider.onValueChanged.AddListener(OnSFXVolumeChanged);

        if (mouseSensitivitySlider != null)
            mouseSensitivitySlider.onValueChanged.AddListener(OnMouseSensitivityChanged);
    }

    void OnDestroy()
    {
        // Отписываемся от событий SettingsManager
        if (SettingsManager.Instance != null)
        {
            SettingsManager.Instance.OnMusicVolumeChanged -= OnMusicVolumeChangedFromManager;
            SettingsManager.Instance.OnSFXVolumeChanged -= OnSFXVolumeChangedFromManager;
            SettingsManager.Instance.OnMouseSensitivityChanged -= OnMouseSensitivityChangedFromManager;
        }
    }

    // ===== НАВИГАЦИЯ =====

    public void ShowMainMenu()
    {
        if (mainMenuPanel != null)
            mainMenuPanel.SetActive(true);

        if (settingsPanel != null)
            settingsPanel.SetActive(false);
    }

    public void ShowSettings()
    {
        PlayClickSound();

        if (mainMenuPanel != null)
            mainMenuPanel.SetActive(false);

        if (settingsPanel != null)
            settingsPanel.SetActive(true);
    }

    public void HideSettings()
    {
        SaveSettingsToManager();
        PlayClickSound();

        if (settingsPanel != null)
            settingsPanel.SetActive(false);

        if (mainMenuPanel != null)
            mainMenuPanel.SetActive(true);
    }

    // ===== КНОПКИ ГЛАВНОГО МЕНЮ =====

    public void OnStartGame()
    {
        PlayClickSound();
        Debug.Log("Загрузка игры...");
        SceneManager.LoadScene(gameSceneName);
    }

    public void OnContinue()
    {
        PlayClickSound();
        Debug.Log("Продолжить игру (пока не реализовано)");
        // Здесь будет логика загрузки сохранения
    }

    public void OnSettings()
    {
        PlayClickSound();
        ShowSettings();
    }

    public void OnExitGame()
    {
        PlayClickSound();
        Debug.Log("Выход из игры...");

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    // ===== НАСТРОЙКИ ЧЕРЕЗ SETTINGSMANAGER =====

    void LoadSettingsFromManager()
    {
        if (SettingsManager.Instance == null)
        {
            Debug.LogWarning("SettingsManager не найден! Загружаю из PlayerPrefs");
            LoadSettingsFromPlayerPrefs();
            return;
        }

        // Загружаем значения из SettingsManager
        if (musicVolumeSlider != null)
            musicVolumeSlider.value = SettingsManager.Instance.GetMusicVolume();

        if (sfxVolumeSlider != null)
            sfxVolumeSlider.value = SettingsManager.Instance.GetSFXVolume();

        if (mouseSensitivitySlider != null)
            mouseSensitivitySlider.value = SettingsManager.Instance.GetMouseSensitivity();

        // Подписываемся на изменения в SettingsManager
        SettingsManager.Instance.OnMusicVolumeChanged += OnMusicVolumeChangedFromManager;
        SettingsManager.Instance.OnSFXVolumeChanged += OnSFXVolumeChangedFromManager;
        SettingsManager.Instance.OnMouseSensitivityChanged += OnMouseSensitivityChangedFromManager;
    }

    void LoadSettingsFromPlayerPrefs()
    {
        if (musicVolumeSlider != null)
        {
            float savedMusicVolume = PlayerPrefs.GetFloat("MusicVolume", 0.75f);
            musicVolumeSlider.value = savedMusicVolume;
            OnMusicVolumeChanged(savedMusicVolume);
        }

        if (sfxVolumeSlider != null)
        {
            float savedSFXVolume = PlayerPrefs.GetFloat("SFXVolume", 0.75f);
            sfxVolumeSlider.value = savedSFXVolume;
            OnSFXVolumeChanged(savedSFXVolume);
        }

        if (mouseSensitivitySlider != null)
        {
            float savedSensitivity = PlayerPrefs.GetFloat("MouseSensitivity", 2f);
            mouseSensitivitySlider.value = savedSensitivity;
            OnMouseSensitivityChanged(savedSensitivity);
        }
    }

    void SaveSettingsToManager()
    {
        if (SettingsManager.Instance == null)
        {
            // Если нет SettingsManager, сохраняем в PlayerPrefs
            if (musicVolumeSlider != null)
                PlayerPrefs.SetFloat("MusicVolume", musicVolumeSlider.value);

            if (sfxVolumeSlider != null)
                PlayerPrefs.SetFloat("SFXVolume", sfxVolumeSlider.value);

            if (mouseSensitivitySlider != null)
                PlayerPrefs.SetFloat("MouseSensitivity", mouseSensitivitySlider.value);

            PlayerPrefs.Save();
            Debug.Log("Настройки сохранены в PlayerPrefs");
            return;
        }

        // Сохраняем через SettingsManager
        if (musicVolumeSlider != null)
            SettingsManager.Instance.SetMusicVolume(musicVolumeSlider.value);

        if (sfxVolumeSlider != null)
            SettingsManager.Instance.SetSFXVolume(sfxVolumeSlider.value);

        if (mouseSensitivitySlider != null)
            SettingsManager.Instance.SetMouseSensitivity(mouseSensitivitySlider.value);
    }

    // ===== ОБРАБОТЧИКИ ИЗМЕНЕНИЙ ПОЛЗУНКОВ =====

    void OnMusicVolumeChanged(float value)
    {
        if (musicVolumeText != null)
            musicVolumeText.text = Mathf.RoundToInt(value * 100).ToString();
    }

    void OnSFXVolumeChanged(float value)
    {
        if (sfxVolumeText != null)
            sfxVolumeText.text = Mathf.RoundToInt(value * 100).ToString();

        // Применяем громкость звуков эффектов для обратной связи
        if (clickSound != null)
            clickSound.volume = value;
    }

    void OnMouseSensitivityChanged(float value)
    {
        if (mouseSensitivityText != null)
            mouseSensitivityText.text = value.ToString("F1");
    }

    // ===== ОБРАБОТЧИКИ ИЗМЕНЕНИЙ ИЗ SETTINGSMANAGER =====

    void OnMusicVolumeChangedFromManager(float value)
    {
        if (musicVolumeSlider != null && Mathf.Abs(musicVolumeSlider.value - value) > 0.01f)
            musicVolumeSlider.value = value;
    }

    void OnSFXVolumeChangedFromManager(float value)
    {
        if (sfxVolumeSlider != null && Mathf.Abs(sfxVolumeSlider.value - value) > 0.01f)
            sfxVolumeSlider.value = value;
    }

    void OnMouseSensitivityChangedFromManager(float value)
    {
        if (mouseSensitivitySlider != null && Mathf.Abs(mouseSensitivitySlider.value - value) > 0.01f)
            mouseSensitivitySlider.value = value;
    }

    // ===== ЗВУК НАЖАТИЯ =====

    void PlayClickSound()
    {
        if (clickSound != null)
        {
            float volume = SettingsManager.Instance != null ? SettingsManager.Instance.GetSFXVolume() : 1f;
            clickSound.volume = volume;
            clickSound.Play();
        }
    }
}