using UnityEngine;
using System;

// Designed to handle logic for player pref loading, such as volume, sensitivity, and fullscreen mode
public class SettingsManager : MonoBehaviour
{
    // Singleton instance for safeguarding
    public static SettingsManager Instance { get; private set; }

    // Look sensitivity triggers an event when changed, allowing other scripts to respond accordingly
    private float _lookSensitivity = 1f;
    public float lookSensitivity
    {
        get => _lookSensitivity;
        set
        {
            _lookSensitivity = value;
            OnLookSensitivityChanged?.Invoke(value);
        }
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        AudioListener.volume = PlayerPrefs.GetFloat("Volume", 1f);
        lookSensitivity = PlayerPrefs.GetFloat("Sensitivity", 1f);
#if !UNITY_WEBGL
        // Browsers only allow fullscreen from a click, so on web the first click (e.g. Start Game)
        // would suddenly go fullscreen. Web players use the settings toggle or itch's fullscreen button instead.
        Screen.fullScreen = PlayerPrefs.GetInt("Fullscreen", 1) == 1;
#endif
        QualitySettings.SetQualityLevel(PlayerPrefs.GetInt("GraphicsQuality", 1));
    }

    private void Awake()
    {

        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }

    }

    public event Action<float> OnLookSensitivityChanged;
}
