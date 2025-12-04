using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class SettingsMenu : MonoBehaviour
{
    [Header("Audio")]
    [SerializeField] private AudioSource musicAudioSource;
    [SerializeField] private Slider musicVolumeSlider;
    [SerializeField] private Slider sfxVolumeSlider;
    [SerializeField] private TextMeshProUGUI musicVolumeText;
    [SerializeField] private TextMeshProUGUI sfxVolumeText;

    [Header("Graphics")]
    [SerializeField] private TMP_Dropdown resolutionDropdown;
    [SerializeField] private TMP_Dropdown qualityDropdown;
    [SerializeField] private Toggle fullscreenToggle;

    private Resolution[] resolutions;
    private int currentResolutionIndex;

    // Static variables so other scripts can access current volume settings
    public static float MusicVolume { get; private set; } = 0.75f;
    public static float SFXVolume { get; private set; } = 0.75f;

    private const string MUSIC_VOLUME_KEY = "MusicVolume";
    private const string SFX_VOLUME_KEY = "SFXVolume";
    private const string RESOLUTION_KEY = "ResolutionIndex";
    private const string QUALITY_KEY = "QualityLevel";
    private const string FULLSCREEN_KEY = "Fullscreen";

    void Start()
    {
        SetupResolutions();
        SetupQualitySettings();
        LoadSettings();

        // Add listeners
        musicVolumeSlider.onValueChanged.AddListener(SetMusicVolume);
        sfxVolumeSlider.onValueChanged.AddListener(SetSFXVolume);
        resolutionDropdown.onValueChanged.AddListener(SetResolution);
        qualityDropdown.onValueChanged.AddListener(SetQuality);
        fullscreenToggle.onValueChanged.AddListener(SetFullscreen);
    }

    void SetupResolutions()
    {
        resolutions = Screen.resolutions;
        resolutionDropdown.ClearOptions();

        List<string> options = new List<string>();
        currentResolutionIndex = 0;

        for (int i = 0; i < resolutions.Length; i++)
        {
            string option = resolutions[i].width + " x " + resolutions[i].height + " @ " + resolutions[i].refreshRate + "Hz";
            options.Add(option);

            if (resolutions[i].width == Screen.width &&
                resolutions[i].height == Screen.height)
            {
                currentResolutionIndex = i;
            }
        }

        resolutionDropdown.AddOptions(options);
        resolutionDropdown.value = currentResolutionIndex;
        resolutionDropdown.RefreshShownValue();
    }

    void SetupQualitySettings()
    {
        qualityDropdown.ClearOptions();
        qualityDropdown.AddOptions(new List<string>(QualitySettings.names));
        qualityDropdown.value = QualitySettings.GetQualityLevel();
        qualityDropdown.RefreshShownValue();
    }

    public void SetMusicVolume(float volume)
    {
        MusicVolume = volume;

        // Apply to the music audio source if assigned
        if (musicAudioSource != null)
        {
            musicAudioSource.volume = volume;
        }

        musicVolumeText.text = Mathf.RoundToInt(volume * 100) + "%";
        PlayerPrefs.SetFloat(MUSIC_VOLUME_KEY, volume);
    }

    public void SetSFXVolume(float volume)
    {
        SFXVolume = volume;
        sfxVolumeText.text = Mathf.RoundToInt(volume * 100) + "%";
        PlayerPrefs.SetFloat(SFX_VOLUME_KEY, volume);
    }

    public void SetResolution(int resolutionIndex)
    {
        Resolution resolution = resolutions[resolutionIndex];
        Screen.SetResolution(resolution.width, resolution.height, Screen.fullScreen);
        PlayerPrefs.SetInt(RESOLUTION_KEY, resolutionIndex);
    }

    public void SetQuality(int qualityIndex)
    {
        QualitySettings.SetQualityLevel(qualityIndex);
        PlayerPrefs.SetInt(QUALITY_KEY, qualityIndex);
    }

    public void SetFullscreen(bool isFullscreen)
    {
        Screen.fullScreen = isFullscreen;
        PlayerPrefs.SetInt(FULLSCREEN_KEY, isFullscreen ? 1 : 0);
    }

    void LoadSettings()
    {
        // Load audio settings
        float musicVolume = PlayerPrefs.GetFloat(MUSIC_VOLUME_KEY, 0.75f);
        float sfxVolume = PlayerPrefs.GetFloat(SFX_VOLUME_KEY, 0.75f);

        musicVolumeSlider.value = musicVolume;
        sfxVolumeSlider.value = sfxVolume;

        SetMusicVolume(musicVolume);
        SetSFXVolume(sfxVolume);

        // Load graphics settings
        int savedResolution = PlayerPrefs.GetInt(RESOLUTION_KEY, currentResolutionIndex);
        if (savedResolution < resolutions.Length)
        {
            resolutionDropdown.value = savedResolution;
            SetResolution(savedResolution);
        }

        int savedQuality = PlayerPrefs.GetInt(QUALITY_KEY, QualitySettings.GetQualityLevel());
        qualityDropdown.value = savedQuality;
        SetQuality(savedQuality);

        bool isFullscreen = PlayerPrefs.GetInt(FULLSCREEN_KEY, 1) == 1;
        fullscreenToggle.isOn = isFullscreen;
        SetFullscreen(isFullscreen);
    }

    public void ResetToDefaults()
    {
        musicVolumeSlider.value = 0.75f;
        sfxVolumeSlider.value = 0.75f;
        qualityDropdown.value = QualitySettings.GetQualityLevel();
        fullscreenToggle.isOn = true;
        resolutionDropdown.value = currentResolutionIndex;
    }

    public void SaveAndClose()
    {
        PlayerPrefs.Save();
        gameObject.SetActive(false);
    }

    // Helper method: Call this when playing sound effects
    public static void PlaySFXWithVolume(AudioSource source, AudioClip clip)
    {
        if (source != null && clip != null)
        {
            source.PlayOneShot(clip, SFXVolume);
        }
    }
}