using UnityEngine;
using UnityEngine.UIElements;

public class FPSCounter : MonoBehaviour
{
    private Label _fpsLabel;
    private float _timer;
    private float _deltaTime;
    
    // How often to update the text (0.5s is readable)
    private const float RefreshRate = 0.5f; 

    private void OnEnable()
    {
        var uiDoc = GetComponent<UIDocument>();
        var root = uiDoc.rootVisualElement;

        _fpsLabel = root.Q<Label>("FpsLabel");
    }

    private void Update()
    {
        // 1. Calculate the time difference smoothly
        _deltaTime += (Time.unscaledDeltaTime - _deltaTime) * 0.1f;

        // 2. Only update the text periodically to save CPU and prevent flickering
        _timer += Time.unscaledDeltaTime;
        if (_timer >= RefreshRate)
        {
            if (_fpsLabel != null)
            {
                float fps = 1.0f / _deltaTime;
                _fpsLabel.text = Mathf.CeilToInt(fps) + " FPS";

                // Optional: Color code low FPS warnings
                if (fps < 30) 
                    _fpsLabel.style.color = new StyleColor(Color.red);
                else 
                    _fpsLabel.style.color = new StyleColor(new Color(1, 1, 1, 0.5f));
            }
            _timer = 0;
        }
    }
}