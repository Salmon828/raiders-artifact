using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using System;

public class SettingsMenu : MonoBehaviour
{
    [SerializeField]
    private GameObject settingsMenuCanvas;

    [SerializeField] private Slider volumeSlider;
    [SerializeField] private Slider sensitivitySlider;
    [SerializeField] private Toggle fullscreenToggle;
    [SerializeField] private TMP_Dropdown graphicsQualityDropdown;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        volumeSlider.onValueChanged.AddListener(OnVolumeChanged);
        sensitivitySlider.onValueChanged.AddListener(OnSensitivityChanged);
        fullscreenToggle.onValueChanged.AddListener(OnFullscreenToggled);
        graphicsQualityDropdown.onValueChanged.AddListener(OnGraphicsQualityChanged);

        // Set the dropdown options based on the available quality levels in the project settings
        graphicsQualityDropdown.ClearOptions();
        graphicsQualityDropdown.AddOptions(new List<string>(QualitySettings.names));
    }

    void OnEnable()
    {
        // Load settings from PlayerPrefs or set default values
        volumeSlider.SetValueWithoutNotify(AudioListener.volume);
        if (SettingsManager.Instance != null)
        {
            sensitivitySlider.SetValueWithoutNotify(SettingsManager.Instance.lookSensitivity);
        }
        fullscreenToggle.SetIsOnWithoutNotify(Screen.fullScreen);
        graphicsQualityDropdown.SetValueWithoutNotify(QualitySettings.GetQualityLevel());

    }


    void OnVolumeChanged(float value)
    {
        AudioListener.volume = value;
    }
    void OnSensitivityChanged(float value)
    {
        if (SettingsManager.Instance != null)
        {
            SettingsManager.Instance.lookSensitivity = value;
        }
    }
    void OnFullscreenToggled(bool isFullscreen)
    {
        Screen.fullScreen = isFullscreen;
    }
    void OnGraphicsQualityChanged(int qualityIndex)
    {
        QualitySettings.SetQualityLevel(qualityIndex);
    }
    
    public event Action OnClosed;

    public void CloseSettingsMenu()
    {
        settingsMenuCanvas.SetActive(false);
        OnClosed?.Invoke();
    }

    void OnDisable()
    {
        // Save settings to PlayerPrefs
        PlayerPrefs.SetFloat("Volume", AudioListener.volume);
        if (SettingsManager.Instance != null)
        {
            PlayerPrefs.SetFloat("Sensitivity", SettingsManager.Instance.lookSensitivity);
        }
        PlayerPrefs.SetInt("Fullscreen", Screen.fullScreen ? 1 : 0);
        PlayerPrefs.SetInt("GraphicsQuality", QualitySettings.GetQualityLevel());
        PlayerPrefs.Save();
    }
}
