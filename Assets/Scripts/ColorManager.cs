using UnityEngine;
using System.Collections.Generic;

public class ColorManager : MonoBehaviour
{
    public static ColorManager Instance;

    [Header("Setup")]
    public List<Color> colorPalette; 
    public float transitionSpeed = 2.0f; // Higher = Faster transition

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
            // Start at the first color immediately
            _currentIndex = 0;
            CurrentColor = colorPalette[0];
            _targetColor = colorPalette[0];
        }
    }

    // Call this function whenever you stack a block successfully
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