using UnityEngine;
using UnityEngine.Audio;

public class SettingsManager : MonoBehaviour
{
    public static SettingsManager Instance { get; private set; }

    [Header("Audio Mixer")]
    public AudioMixer audioMixer;

    [Header("Audio Sources")]
    public AudioSource musicSource;
    public AudioSource sfxSource;

    // Ключи для сохранения
    private const string MUSIC_VOLUME_KEY = "MusicVolume";
    private const string SFX_VOLUME_KEY = "SFXVolume";
    private const string MOUSE_SENSITIVITY_KEY = "MouseSensitivity";

    // Текущие значения
    private float currentMusicVolume = 0.75f;
    private float currentSFXVolume = 0.75f;
    private float currentMouseSensitivity = 2f;

    // События для обновления других скриптов
    public System.Action<float> OnMusicVolumeChanged;
    public System.Action<float> OnSFXVolumeChanged;
    public System.Action<float> OnMouseSensitivityChanged;

    void Awake()
    {
        // Синглтон
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

        // Загружаем сохраненные настройки
        LoadSettings();
    }

    void Start()
    {
        // Применяем настройки при старте
        ApplyMusicVolume(currentMusicVolume);
        ApplySFXVolume(currentSFXVolume);
        ApplyMouseSensitivity(currentMouseSensitivity);
    }

    // ===== ГРОМКОСТЬ МУЗЫКИ =====

    public void SetMusicVolume(float volume)
    {
        currentMusicVolume = volume;
        ApplyMusicVolume(volume);
        OnMusicVolumeChanged?.Invoke(volume);
        SaveSettings();
    }

    void ApplyMusicVolume(float volume)
    {
        if (musicSource != null)
        {
            musicSource.volume = volume;
        }

        // Если используете AudioMixer
        if (audioMixer != null)
        {
            float db = Mathf.Log10(Mathf.Max(volume, 0.0001f)) * 20f;
            audioMixer.SetFloat("MusicVolume", db);
        }
    }

    public float GetMusicVolume()
    {
        return currentMusicVolume;
    }

    // ===== ГРОМКОСТЬ ЗВУКОВ =====

    public void SetSFXVolume(float volume)
    {
        currentSFXVolume = volume;
        ApplySFXVolume(volume);
        OnSFXVolumeChanged?.Invoke(volume);
        SaveSettings();
    }

    void ApplySFXVolume(float volume)
    {
        if (sfxSource != null)
        {
            sfxSource.volume = volume;
        }

        // Если используете AudioMixer
        if (audioMixer != null)
        {
            float db = Mathf.Log10(Mathf.Max(volume, 0.0001f)) * 20f;
            audioMixer.SetFloat("SFXVolume", db);
        }
    }

    public float GetSFXVolume()
    {
        return currentSFXVolume;
    }

    // ===== ЧУВСТВИТЕЛЬНОСТЬ МЫШИ =====

    public void SetMouseSensitivity(float sensitivity)
    {
        currentMouseSensitivity = sensitivity;
        ApplyMouseSensitivity(sensitivity);
        OnMouseSensitivityChanged?.Invoke(sensitivity);
        SaveSettings();
    }

    void ApplyMouseSensitivity(float sensitivity)
    {
        // Применяем к PlayerController
        PlayerController player = FindFirstObjectByType<PlayerController>();
        if (player != null)
        {
            player.UpdateMouseSensitivity(sensitivity);
        }
    }

    public float GetMouseSensitivity()
    {
        return currentMouseSensitivity;
    }

    // ===== СОХРАНЕНИЕ И ЗАГРУЗКА =====

    void SaveSettings()
    {
        PlayerPrefs.SetFloat(MUSIC_VOLUME_KEY, currentMusicVolume);
        PlayerPrefs.SetFloat(SFX_VOLUME_KEY, currentSFXVolume);
        PlayerPrefs.SetFloat(MOUSE_SENSITIVITY_KEY, currentMouseSensitivity);
        PlayerPrefs.Save();
        Debug.Log("Настройки сохранены");
    }

    void LoadSettings()
    {
        currentMusicVolume = PlayerPrefs.GetFloat(MUSIC_VOLUME_KEY, 0.75f);
        currentSFXVolume = PlayerPrefs.GetFloat(SFX_VOLUME_KEY, 0.75f);
        currentMouseSensitivity = PlayerPrefs.GetFloat(MOUSE_SENSITIVITY_KEY, 2f);

        Debug.Log($"Настройки загружены: Music={currentMusicVolume}, SFX={currentSFXVolume}, Sensitivity={currentMouseSensitivity}");
    }

    // Сброс настроек по умолчанию
    public void ResetToDefault()
    {
        SetMusicVolume(0.75f);
        SetSFXVolume(0.75f);
        SetMouseSensitivity(2f);
    }
}