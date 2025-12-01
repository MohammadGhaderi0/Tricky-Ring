using UnityEngine;
using System.Collections.Generic;

public class ColorManager : MonoBehaviour
{
    public static ColorManager Instance;

    [Header("Setup")]
    public List<Color> colorPalette; 
    public float transitionSpeed = 2.0f;

    // Public read-only access for the painter
    public Color CurrentColor { get; private set; }
    private Color _targetColor;
    private int _currentIndex = 0;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            InitializeColors();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void InitializeColors()
    {
        if (colorPalette.Count > 0)
        {
            // Start with random color
            _currentIndex = Random.Range(0, colorPalette.Count);
            CurrentColor = colorPalette[_currentIndex];
            _targetColor = colorPalette[_currentIndex];
        }
    }
    
    public void NextColor()
    {
        if (colorPalette.Count == 0) return;

        // Increment index and loop back to 0 if we reach the end
        _currentIndex++;
        if (_currentIndex >= colorPalette.Count)
        {
            _currentIndex = 0;
        }

        // Set the new destination color
        _targetColor = colorPalette[_currentIndex];
    }

    private void Update()
    {
        // Always smooth out the values
        CurrentColor = Color.Lerp(CurrentColor, _targetColor, Time.deltaTime * transitionSpeed);
    }
}