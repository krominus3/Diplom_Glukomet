using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PauseMenu : MonoBehaviour
{
    [Header("Panel References")]
    public GameObject pauseMenuPanel;
    public GameObject settingsPanel;
    public GameObject backGround;

    [Header("Settings References")]
    public Slider musicVolumeSlider;
    public Slider sfxVolumeSlider;
    public Slider mouseSensitivitySlider;
    public TextMeshProUGUI musicVolumeText;
    public TextMeshProUGUI sfxVolumeText;
    public TextMeshProUGUI mouseSensitivityText;

    [Header("Scene Settings")]
    public string mainMenuSceneName = "MainMenu";

    [Header("Audio")]
    public AudioSource clickSound;

    [Header("Input")]
    public KeyCode pauseKey = KeyCode.Escape;

    private bool isPaused = false;
    private float previousTimeScale = 1f;
    private PlayerController PC;
    private RuleWeapon RW;

    void Start()
    {
        if (pauseMenuPanel != null)
            pauseMenuPanel.SetActive(false);

        if (settingsPanel != null)
            settingsPanel.SetActive(false);

        if (backGround != null)
            backGround.SetActive(false);

        PC = FindFirstObjectByType<PlayerController>();
        RW = FindFirstObjectByType<RuleWeapon>();

        LoadSettingsFromManager();
        LoadSettingsFromPlayerPrefs();

        // Обновляем текст ползунков после загрузки
        UpdateAllTexts();

        if (musicVolumeSlider != null)
            musicVolumeSlider.onValueChanged.AddListener(OnMusicVolumeChanged);

        if (sfxVolumeSlider != null)
            sfxVolumeSlider.onValueChanged.AddListener(OnSFXVolumeChanged);

        if (mouseSensitivitySlider != null)
            mouseSensitivitySlider.onValueChanged.AddListener(OnMouseSensitivityChanged);
    }

    // Новый метод для обновления всех текстов
    void UpdateAllTexts()
    {
        if (musicVolumeSlider != null && musicVolumeText != null)
        {
            float value = musicVolumeSlider.value;
            musicVolumeText.text = Mathf.RoundToInt(value * 100).ToString() + " %";
        }

        if (sfxVolumeSlider != null && sfxVolumeText != null)
        {
            float value = sfxVolumeSlider.value;
            sfxVolumeText.text = Mathf.RoundToInt(value * 100).ToString() + " %";
        }

        if (mouseSensitivitySlider != null && mouseSensitivityText != null)
        {
            float value = mouseSensitivitySlider.value;
            mouseSensitivityText.text = value.ToString("F1");
        }
    }

    void OnDestroy()
    {
        if (SettingsManager.Instance != null)
        {
            SettingsManager.Instance.OnMusicVolumeChanged -= OnMusicVolumeChangedFromManager;
            SettingsManager.Instance.OnSFXVolumeChanged -= OnSFXVolumeChangedFromManager;
            SettingsManager.Instance.OnMouseSensitivityChanged -= OnMouseSensitivityChangedFromManager;
        }
    }

    void Update()
    {
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

    public void PauseGame()
    {
        isPaused = true;
        previousTimeScale = Time.timeScale;
        Time.timeScale = 0f;

        ShowPauseMenu();
        ShowBackground();
        OnPlayerPaused();

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

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

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

        // Перезагружаем настройки перед открытием
        LoadSettingsFromManager();
        LoadSettingsFromPlayerPrefs();
        UpdateAllTexts();

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

        Time.timeScale = 1f;
        isPaused = false;

        SceneManager.LoadScene(mainMenuSceneName);
    }

    public void OnRestart()
    {
        PlayClickSound();

        Time.timeScale = 1f;
        isPaused = false;

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

    void LoadSettingsFromManager()
    {
        if (SettingsManager.Instance == null)
        {
            Debug.LogWarning("SettingsManager не найден! Загружаю из PlayerPrefs");
            LoadSettingsFromPlayerPrefs();
            return;
        }

        if (musicVolumeSlider != null)
            musicVolumeSlider.value = SettingsManager.Instance.GetMusicVolume();

        if (sfxVolumeSlider != null)
            sfxVolumeSlider.value = SettingsManager.Instance.GetSFXVolume();

        if (mouseSensitivitySlider != null)
            mouseSensitivitySlider.value = SettingsManager.Instance.GetMouseSensitivity();

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
        }

        if (sfxVolumeSlider != null)
        {
            float savedSFXVolume = PlayerPrefs.GetFloat("SFXVolume", 0.75f);
            sfxVolumeSlider.value = savedSFXVolume;
        }

        if (mouseSensitivitySlider != null)
        {
            float savedSensitivity = PlayerPrefs.GetFloat("MouseSensitivity", 2f);
            mouseSensitivitySlider.value = savedSensitivity;
        }
    }

    void SaveSettingsToManager()
    {
        if (SettingsManager.Instance == null)
        {
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

        if (musicVolumeSlider != null)
            SettingsManager.Instance.SetMusicVolume(musicVolumeSlider.value);

        if (sfxVolumeSlider != null)
            SettingsManager.Instance.SetSFXVolume(sfxVolumeSlider.value);

        if (mouseSensitivitySlider != null)
            SettingsManager.Instance.SetMouseSensitivity(mouseSensitivitySlider.value);
    }

    void OnMusicVolumeChanged(float value)
    {
        if (musicVolumeText != null)
            musicVolumeText.text = Mathf.RoundToInt(value * 100).ToString() + " %";

        if (SettingsManager.Instance != null)
            SettingsManager.Instance.SetMusicVolume(value);
        else
            PlayerPrefs.SetFloat("MusicVolume", value);
    }

    void OnSFXVolumeChanged(float value)
    {
        if (sfxVolumeText != null)
            sfxVolumeText.text = Mathf.RoundToInt(value * 100).ToString() + " %";

        if (clickSound != null)
            clickSound.volume = value;

        if (SettingsManager.Instance != null)
            SettingsManager.Instance.SetSFXVolume(value);
        else
            PlayerPrefs.SetFloat("SFXVolume", value);
    }

    void OnMouseSensitivityChanged(float value)
    {
        if (mouseSensitivityText != null)
            mouseSensitivityText.text = value.ToString("F1");

        if (SettingsManager.Instance != null)
            SettingsManager.Instance.SetMouseSensitivity(value);
        else
            PlayerPrefs.SetFloat("MouseSensitivity", value);
    }

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

    void PlayClickSound()
    {

        clickSound.Play();

    }
}