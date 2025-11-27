using UnityEngine;
using UnityEngine.UIElements;

public class MainMenuManager : MonoBehaviour
{
    public UIDocument uiDocument;
    private VisualElement _settingsOverlay;

    void Start() {
        var root = uiDocument.rootVisualElement;
        _settingsOverlay = root.Q<VisualElement>("SettingsOverlay");
    
        // Setup Buttons
        var settingsBtn = root.Q<Button>("SettingsBtn");
        var closeBtn = root.Q<Button>("CloseSettingsBtn");

        settingsBtn.clicked += () => _settingsOverlay.style.display = DisplayStyle.Flex;
        closeBtn.clicked += () => _settingsOverlay.style.display = DisplayStyle.None;
    }
}
