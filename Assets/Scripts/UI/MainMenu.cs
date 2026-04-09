using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections;

public class MainMenu : MonoBehaviour
{
    [Header("Panel References")]
    public GameObject mainMenuPanel;
    public GameObject settingsPanel;
    public GameObject loadingPanel;

    [Header("Settings References")]
    public Slider musicVolumeSlider;
    public Slider sfxVolumeSlider;
    public Slider mouseSensitivitySlider;
    public TMPro.TextMeshProUGUI musicVolumeText;
    public TMPro.TextMeshProUGUI sfxVolumeText;
    public TMPro.TextMeshProUGUI mouseSensitivityText;

    [Header("Loading Settings")]
    public string firstLevelName = "Level1";

    [Header("UI")]
    public TMPro.TextMeshProUGUI loadingText;
    public Slider loadingProgressBar;

    [Header("Buttons")]
    public Button newGameButton;
    public Button continueButton;
    public Button optionsButton;
    public Button quitButton;
    public Button backFromSettingsButton;

    [Header("Audio")]
    public AudioSource clickSound;

    private bool isLoading = false;

    void Start()
    {
        // Настраиваем кнопки
        if (newGameButton != null)
            newGameButton.onClick.AddListener(NewGame);

        if (continueButton != null)
            continueButton.onClick.AddListener(ContinueGame);

        if (optionsButton != null)
            optionsButton.onClick.AddListener(OpenSettings);

        if (quitButton != null)
            quitButton.onClick.AddListener(QuitGame);

        if (backFromSettingsButton != null)
            backFromSettingsButton.onClick.AddListener(CloseSettings);

        // Добавляем слушатели для ползунков
        if (musicVolumeSlider != null)
            musicVolumeSlider.onValueChanged.AddListener(OnMusicVolumeChanged);

        if (sfxVolumeSlider != null)
            sfxVolumeSlider.onValueChanged.AddListener(OnSFXVolumeChanged);

        if (mouseSensitivitySlider != null)
            mouseSensitivitySlider.onValueChanged.AddListener(OnMouseSensitivityChanged);

        // Загружаем настройки
        LoadSettings();

        UpdateContinueButton();

        // Скрываем панели
        if (loadingPanel != null)
            loadingPanel.SetActive(false);
        if (settingsPanel != null)
            settingsPanel.SetActive(false);
        if (mainMenuPanel != null)
            mainMenuPanel.SetActive(true);
    }

    void UpdateContinueButton()
    {
        if (continueButton != null)
        {
            continueButton.interactable = PlayerData.HasSave();
        }
    }

    public void NewGame()
    {
        if (isLoading) return;
        PlayClickSound();
        PlayerData.ClearSave();
        StartCoroutine(LoadLevel(firstLevelName));
    }

    public void ContinueGame()
    {
        if (isLoading) return;
        PlayClickSound();

        if (PlayerData.LoadSavedData())
        {
            StartCoroutine(LoadLevel(PlayerData.currentLevel));
        }
        else
        {
            NewGame();
        }
    }

    public void OpenSettings()
    {
        PlayClickSound();

        if (mainMenuPanel != null)
            mainMenuPanel.SetActive(false);
        if (settingsPanel != null)
            settingsPanel.SetActive(true);

        // Подгружаем текущие настройки
        LoadSettings();
    }

    public void CloseSettings()
    {
        PlayClickSound();

        if (settingsPanel != null)
            settingsPanel.SetActive(false);
        if (mainMenuPanel != null)
            mainMenuPanel.SetActive(true);
    }

    // ===== НАСТРОЙКИ =====

    void LoadSettings()
    {
        // Загружаем из SettingsManager или PlayerPrefs
        if (SettingsManager.Instance != null)
        {
            if (musicVolumeSlider != null)
                musicVolumeSlider.value = SettingsManager.Instance.GetMusicVolume();

            if (sfxVolumeSlider != null)
                sfxVolumeSlider.value = SettingsManager.Instance.GetSFXVolume();

            if (mouseSensitivitySlider != null)
                mouseSensitivitySlider.value = SettingsManager.Instance.GetMouseSensitivity();
        }

        // Загружаем из PlayerPrefs
        if (musicVolumeSlider != null)
        {
            float savedMusic = PlayerPrefs.GetFloat("MusicVolume", 0.75f);
            musicVolumeSlider.value = savedMusic;
            OnMusicVolumeChanged(savedMusic);
        }

        if (sfxVolumeSlider != null)
        {
            float savedSFX = PlayerPrefs.GetFloat("SFXVolume", 0.75f);
            sfxVolumeSlider.value = savedSFX;
            OnSFXVolumeChanged(savedSFX);
        }

        if (mouseSensitivitySlider != null)
        {
            float savedSensitivity = PlayerPrefs.GetFloat("MouseSensitivity", 2f);
            mouseSensitivitySlider.value = savedSensitivity;
            OnMouseSensitivityChanged(savedSensitivity);
        }
    }

    void SaveSettings()
    {
        if (SettingsManager.Instance != null)
        {
            if (musicVolumeSlider != null)
                SettingsManager.Instance.SetMusicVolume(musicVolumeSlider.value);

            if (sfxVolumeSlider != null)
                SettingsManager.Instance.SetSFXVolume(sfxVolumeSlider.value);

            if (mouseSensitivitySlider != null)
                SettingsManager.Instance.SetMouseSensitivity(mouseSensitivitySlider.value);
        }
        else
        {
            // Сохраняем в PlayerPrefs
            if (musicVolumeSlider != null)
                PlayerPrefs.SetFloat("MusicVolume", musicVolumeSlider.value);

            if (sfxVolumeSlider != null)
                PlayerPrefs.SetFloat("SFXVolume", sfxVolumeSlider.value);

            if (mouseSensitivitySlider != null)
                PlayerPrefs.SetFloat("MouseSensitivity", mouseSensitivitySlider.value);

            PlayerPrefs.Save();
        }
    }

    // Обработчики изменения значений
    void OnMusicVolumeChanged(float value)
    {
        if (musicVolumeText != null)
            musicVolumeText.text = Mathf.RoundToInt(value * 100).ToString() + " %";

        SaveSettings();
    }

    void OnSFXVolumeChanged(float value)
    {
        if (sfxVolumeText != null)
            sfxVolumeText.text = Mathf.RoundToInt(value * 100).ToString() + " %";

        SaveSettings();
    }

    void OnMouseSensitivityChanged(float value)
    {
        if (mouseSensitivityText != null)
            mouseSensitivityText.text = value.ToString("F1");

        SaveSettings();
    }

    // ===== ЗАГРУЗКА УРОВНЯ =====

    IEnumerator LoadLevel(string levelName)
    {
        isLoading = true;
        PlayClickSound();

        if (loadingPanel != null)
            loadingPanel.SetActive(true);
        if (mainMenuPanel != null)
            mainMenuPanel.SetActive(false);
        if (settingsPanel != null)
            settingsPanel.SetActive(false);

        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(levelName);
        asyncLoad.allowSceneActivation = false;

        while (!asyncLoad.isDone)
        {
            float progress = Mathf.Clamp01(asyncLoad.progress / 0.9f);

            if (loadingText != null)
                loadingText.text = $"ЗАГРУЗКА... {(progress * 100):F0}%";

            if (loadingProgressBar != null)
                loadingProgressBar.value = progress;

            if (asyncLoad.progress >= 0.9f)
            {
                if (loadingText != null)
                    loadingText.text = "НАЖМИТЕ ЛЮБУЮ КЛАВИШУ";

                if (Input.anyKeyDown)
                {
                    asyncLoad.allowSceneActivation = true;
                }
            }

            yield return null;
        }

        isLoading = false;
    }

    public void QuitGame()
    {
        PlayClickSound();

        Application.Quit();
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }

    void PlayClickSound()
    {
        if (clickSound != null)
        {
            clickSound.Play();
        }
    }
}