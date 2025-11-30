using UnityEngine;
using UnityEngine.UIElements;

[RequireComponent(typeof(UIDocument))]
public class FpsCounter : MonoBehaviour
{
    private Label _fpsLabel;
    private float _timer;
    private float _deltaTime;
    private const float RefreshRate = 0.5f; 

    private StyleColor _goodColor = new StyleColor(new Color(1, 1, 1, 0.5f));
    private StyleColor _badColor = new StyleColor(Color.red);

    private void OnEnable()
    {
        var uiDoc = GetComponent<UIDocument>();
        if (uiDoc != null)
        {
            _fpsLabel = uiDoc.rootVisualElement.Q<Label>("FpsLabel");
        }
    }

    private void Update()
    {
        // Smooth out the delta time
        _deltaTime += (Time.unscaledDeltaTime - _deltaTime) * 0.1f;

        _timer += Time.unscaledDeltaTime;
        if (_timer >= RefreshRate)
        {
            if (_fpsLabel != null)
            {
                // Calculate the exact integer we want to show
                float fps = 1.0f / _deltaTime;
                int displayFps = Mathf.CeilToInt(fps);

                _fpsLabel.text = displayFps + " FPS";

                if (displayFps < 30) 
                {
                    _fpsLabel.style.color = _badColor;
                }
                else 
                {
                    _fpsLabel.style.color = _goodColor;
                }
            }
            _timer = 0;
        }
    }
}