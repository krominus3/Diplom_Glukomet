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

    [Header("Audio Mixer Parameters Names")]
    public string musicVolumeParam = "MusicVolume";
    public string sfxVolumeParam = "SFXVolume";

    private const string MUSIC_VOLUME_KEY = "MusicVolume";
    private const string SFX_VOLUME_KEY = "SFXVolume";
    private const string MOUSE_SENSITIVITY_KEY = "MouseSensitivity";

    private float currentMusicVolume = 0.75f;
    private float currentSFXVolume = 0.75f;
    private float currentMouseSensitivity = 2f;

    public System.Action<float> OnMusicVolumeChanged;
    public System.Action<float> OnSFXVolumeChanged;
    public System.Action<float> OnMouseSensitivityChanged;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            LoadSettings();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        ApplyMusicVolume(currentMusicVolume);
        ApplySFXVolume(currentSFXVolume);
        ApplyMouseSensitivity(currentMouseSensitivity);
    }

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
            musicSource.volume = volume;

        if (audioMixer != null)
        {
            float db = volume > 0.0001f ? Mathf.Log10(volume) * 20f : -80f;
            audioMixer.SetFloat(musicVolumeParam, db);
        }
    }

    public float GetMusicVolume() => currentMusicVolume;

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
            sfxSource.volume = volume;

        if (audioMixer != null)
        {
            float db = volume > 0.0001f ? Mathf.Log10(volume) * 20f : -80f;
            audioMixer.SetFloat(sfxVolumeParam, db);
        }
    }

    public float GetSFXVolume() => currentSFXVolume;

    public void SetMouseSensitivity(float sensitivity)
    {
        currentMouseSensitivity = sensitivity;
        ApplyMouseSensitivity(sensitivity);
        OnMouseSensitivityChanged?.Invoke(sensitivity);
        SaveSettings();
    }

    void ApplyMouseSensitivity(float sensitivity)
    {
        PlayerController player = FindFirstObjectByType<PlayerController>();
        if (player != null)
            player.UpdateMouseSensitivity(sensitivity);
    }

    public float GetMouseSensitivity() => currentMouseSensitivity;

    void SaveSettings()
    {
        PlayerPrefs.SetFloat(MUSIC_VOLUME_KEY, currentMusicVolume);
        PlayerPrefs.SetFloat(SFX_VOLUME_KEY, currentSFXVolume);
        PlayerPrefs.SetFloat(MOUSE_SENSITIVITY_KEY, currentMouseSensitivity);
        PlayerPrefs.Save();
    }

    void LoadSettings()
    {
        currentMusicVolume = PlayerPrefs.GetFloat(MUSIC_VOLUME_KEY, 0.75f);
        currentSFXVolume = PlayerPrefs.GetFloat(SFX_VOLUME_KEY, 0.75f);
        currentMouseSensitivity = PlayerPrefs.GetFloat(MOUSE_SENSITIVITY_KEY, 2f);
    }

    public void ResetToDefault()
    {
        SetMusicVolume(0.75f);
        SetSFXVolume(0.75f);
        SetMouseSensitivity(2f);
    }
}