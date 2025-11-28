using UnityEngine;
using UnityEngine.UIElements;

public class SettingsController : MonoBehaviour
{
    public UIDocument uiDocument;

    public Sprite vibrationOnSprite;
    public Sprite vibrationOffSprite;

    public Sprite soundOnSprite;
    public Sprite soundOffSprite;

    public string websiteUrl = "https://www.google.com";

    private bool _isVibrationOn;
    private bool _isSoundOn;

    private VisualElement _vibIconElement;
    private VisualElement _soundIconElement;

    private void OnEnable()
    {
        if (uiDocument == null)
        {
            uiDocument = GetComponent<UIDocument>();
        }

        if (uiDocument == null) return;

        var root = uiDocument.rootVisualElement;

        // 1. Find Buttons
        var vibBtn = root.Q<Button>("VibBtn");
        var soundBtn = root.Q<Button>("SoundBtn");
        var helpBtn = root.Q<Button>("HelpBtn");

        // 2. Find the inner Icon elements
        if (vibBtn != null) _vibIconElement = vibBtn.Q<VisualElement>(className: "setting-icon");
        if (soundBtn != null) _soundIconElement = soundBtn.Q<VisualElement>(className: "setting-icon");

        // 3. Register Click Events
        if (vibBtn != null) vibBtn.clicked += ToggleVibration;
        if (soundBtn != null) soundBtn.clicked += ToggleSound;
        if (helpBtn != null) helpBtn.clicked += OpenWebsite;

        // 4. Initialize State from PlayerPrefs
        LoadSettings();

        // 5. Update Visuals
        UpdateVibIcon();
        UpdateSoundIcon();
    }

    private void LoadSettings()
    {
        // Load int from prefs (0 = off, 1 = on). Default is 1 (On).
        _isVibrationOn = PlayerPrefs.GetInt("VibrationEnabled", 1) == 1;
        _isSoundOn = PlayerPrefs.GetInt("SoundEnabled", 1) == 1;

        Debug.Log($"Settings Loaded: Vib={_isVibrationOn}, Sound={_isSoundOn}");
    }

    private void ToggleVibration()
    {
        // Flip state
        _isVibrationOn = !_isVibrationOn;

        // Save immediately
        PlayerPrefs.SetInt("VibrationEnabled", _isVibrationOn ? 1 : 0);
        PlayerPrefs.Save();

        // Visual Update
        UpdateVibIcon();

        Debug.Log($"Vibration Toggled: {_isVibrationOn}");
    }

    private void ToggleSound()
    {
        // Flip state
        _isSoundOn = !_isSoundOn;

        // Save immediately
        PlayerPrefs.SetInt("SoundEnabled", _isSoundOn ? 1 : 0);
        PlayerPrefs.Save();

        // Visual Update
        UpdateSoundIcon();

        Debug.Log($"Sound Toggled: {_isSoundOn}");
    }

    private void OpenWebsite()
    {
        Application.OpenURL(websiteUrl);
    }

    private void UpdateVibIcon()
    {
        if (_isVibrationOn)
        {
            _vibIconElement.style.backgroundImage = new StyleBackground(vibrationOnSprite);
        }
        else
        {
            _vibIconElement.style.backgroundImage = new StyleBackground(vibrationOffSprite);
        }
    }

    private void UpdateSoundIcon()
    {
        if (_isSoundOn)
        {
            _soundIconElement.style.backgroundImage = new StyleBackground(soundOnSprite);
        }
        else
        {
            _soundIconElement.style.backgroundImage = new StyleBackground(soundOffSprite);
        }
    }
}