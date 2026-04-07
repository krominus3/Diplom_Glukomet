using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PauseMenu : MonoBehaviour
{
    [Header("Panel References")]
    public GameObject pauseMenuPanel;      // Панель с кнопками паузы
    public GameObject settingsPanel;      // Панель с настройками
    public GameObject backGround;


    [Header("Settings References")]
    public Slider musicVolumeSlider;      // Ползунок громкости музыки
    public Slider sfxVolumeSlider;        // Ползунок громкости звуков
    public Slider mouseSensitivitySlider; // Ползунок чувствительности мыши
    public TextMeshProUGUI musicVolumeText;          // Текст с значением громкости музыки
    public TextMeshProUGUI sfxVolumeText;            // Текст с значением громкости звуков
    public TextMeshProUGUI mouseSensitivityText;     // Текст с значением чувствительности мыши

    [Header("Scene Settings")]
    public string mainMenuSceneName = "MainMenu"; // Имя сцены главного меню

    [Header("Audio")]
    public AudioSource clickSound;        // Звук нажатия на кнопку

    [Header("Input")]
    public KeyCode pauseKey = KeyCode.Escape; // Клавиша паузы

    private bool isPaused = false;
    private float previousTimeScale = 1f;
    private PlayerController PC;
    private RuleWeapon RW;

    void Start()
    {
        // Скрываем меню паузы при старте
        if (pauseMenuPanel != null)
            pauseMenuPanel.SetActive(false);

        if (settingsPanel != null)
            settingsPanel.SetActive(false);

        if (backGround != null)
            backGround.SetActive(false);

        PC = FindFirstObjectByType<PlayerController>();
        RW = FindFirstObjectByType<RuleWeapon>();

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

    void Update()
    {
        // Проверяем нажатие клавиши паузы
        if (Input.GetKeyDown(pauseKey))
        {
            if (isPaused)
            {
                ResumeGame();
                SaveSettingsToManager();
                Cursor.lockState = CursorLockMode.Locked;
            }
            else
            {
                PauseGame();
                Cursor.lockState = CursorLockMode.None;
            }
        }
    }

    // ===== УПРАВЛЕНИЕ ПАУЗОЙ =====

    public void PauseGame()
    {
        isPaused = true;
        previousTimeScale = Time.timeScale;
        Time.timeScale = 0f;

        ShowPauseMenu();
        ShowBackground();
        OnPlayerPaused();

        // Разблокируем курсор
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    private void ShowBackground()
    {
        if (backGround != null)
            backGround.SetActive(isPaused);
    }

    private void OnPlayerPaused()
    {
        if (PC != null)
            PC.enabled = !isPaused;

        if (RW != null)
            RW.enabled = !isPaused;
    }

    public void ResumeGame()
    {
        isPaused = false;
        Time.timeScale = previousTimeScale;

        ShowPauseMenu();
        ShowBackground();
        OnPlayerPaused();

        // Блокируем курсор обратно
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    // ===== НАВИГАЦИЯ =====

    public void ShowPauseMenu()
    {
        if (pauseMenuPanel != null)
            pauseMenuPanel.SetActive(isPaused && !settingsPanel.activeSelf);

        if (settingsPanel != null && !isPaused)
            settingsPanel.SetActive(false);
    }

    public void ShowSettings()
    {
        PlayClickSound();

        if (pauseMenuPanel != null)
            pauseMenuPanel.SetActive(false);

        if (settingsPanel != null)
            settingsPanel.SetActive(true);
    }

    public void HideSettings()
    {
        PlayClickSound();
        SaveSettingsToManager();

        if (settingsPanel != null)
            settingsPanel.SetActive(false);

        if (pauseMenuPanel != null)
            pauseMenuPanel.SetActive(true);
    }

    // ===== КНОПКИ МЕНЮ ПАУЗЫ =====

    public void OnResume()
    {
        PlayClickSound();
        ResumeGame();
    }

    public void OnSettings()
    {
        PlayClickSound();
        ShowSettings();
    }

    public void OnMainMenu()
    {
        PlayClickSound();

        // Возвращаем время
        Time.timeScale = 1f;
        isPaused = false;

        // Загружаем главное меню
        SceneManager.LoadScene(mainMenuSceneName);
    }

    public void OnRestart()
    {
        PlayClickSound();

        // Возвращаем время
        Time.timeScale = 1f;
        isPaused = false;

        // Перезагружаем текущую сцену
        SaveManager.Instance.HardRestart();
    }

    public void OnQuitGame()
    {
        PlayClickSound();

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

    // ===== ОБРАБОТЧИКИ ИЗМЕНЕНИЙ =====

    void OnMusicVolumeChanged(float value)
    {
        if (musicVolumeText != null)
            musicVolumeText.text = Mathf.RoundToInt(value * 100).ToString();
    }

    void OnSFXVolumeChanged(float value)
    {
        if (sfxVolumeText != null)
            sfxVolumeText.text = Mathf.RoundToInt(value * 100).ToString();

        // Применяем громкость звуков эффектов
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

    // Звук нажатия
    void PlayClickSound()
    {
        if (clickSound != null && Time.timeScale != 0)
        {
            float volume = SettingsManager.Instance != null ? SettingsManager.Instance.GetSFXVolume() : 1f;
            clickSound.volume = volume;
            clickSound.Play();
        }
    }
}