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

        LoadSettings();
        UpdateAllTexts();

        if (musicVolumeSlider != null)
            musicVolumeSlider.onValueChanged.AddListener(OnMusicVolumeChanged);

        if (sfxVolumeSlider != null)
            sfxVolumeSlider.onValueChanged.AddListener(OnSFXVolumeChanged);

        if (mouseSensitivitySlider != null)
            mouseSensitivitySlider.onValueChanged.AddListener(OnMouseSensitivityChanged);
    }

    void UpdateAllTexts()
    {
        if (musicVolumeSlider != null && musicVolumeText != null)
            musicVolumeText.text = Mathf.RoundToInt(musicVolumeSlider.value * 100).ToString() + " %";

        if (sfxVolumeSlider != null && sfxVolumeText != null)
            sfxVolumeText.text = Mathf.RoundToInt(sfxVolumeSlider.value * 100).ToString() + " %";

        if (mouseSensitivitySlider != null && mouseSensitivityText != null)
            mouseSensitivityText.text = mouseSensitivitySlider.value.ToString("F1");
    }

    void Update()
    {
        if (Input.GetKeyDown(pauseKey))
        {
            if (isPaused)
            {
                ResumeGame();
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

        LoadSettings();
        UpdateAllTexts();

        if (pauseMenuPanel != null)
            pauseMenuPanel.SetActive(false);

        if (settingsPanel != null)
            settingsPanel.SetActive(true);
    }

    public void HideSettings()
    {
        PlayClickSound();

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

    void LoadSettings()
    {
        if (SettingsManager.Instance != null)
        {
            if (musicVolumeSlider != null)
                musicVolumeSlider.value = SettingsManager.Instance.GetMusicVolume();

            if (sfxVolumeSlider != null)
                sfxVolumeSlider.value = SettingsManager.Instance.GetSFXVolume();

            if (mouseSensitivitySlider != null)
                mouseSensitivitySlider.value = SettingsManager.Instance.GetMouseSensitivity();
        }
    }

    void OnMusicVolumeChanged(float value)
    {
        if (musicVolumeText != null)
            musicVolumeText.text = Mathf.RoundToInt(value * 100).ToString() + " %";

        if (SettingsManager.Instance != null)
            SettingsManager.Instance.SetMusicVolume(value);
    }

    void OnSFXVolumeChanged(float value)
    {
        if (sfxVolumeText != null)
            sfxVolumeText.text = Mathf.RoundToInt(value * 100).ToString() + " %";

        if (clickSound != null)
            clickSound.volume = value;

        if (SettingsManager.Instance != null)
            SettingsManager.Instance.SetSFXVolume(value);
    }

    void OnMouseSensitivityChanged(float value)
    {
        if (mouseSensitivityText != null)
            mouseSensitivityText.text = value.ToString("F1");

        if (SettingsManager.Instance != null)
            SettingsManager.Instance.SetMouseSensitivity(value);
    }

    void PlayClickSound()
    {
        if (clickSound != null)
            clickSound.Play();
    }
}