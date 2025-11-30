using UnityEngine;

public class CameraColorChanger : MonoBehaviour
{
    private Camera _cam;

    private void Awake()
    {
        _cam = GetComponent<Camera>();
    }

    private void Update()
    {
        if (ColorManager.Instance != null)
        {
            // This is the absolute cheapest way to change background color.
            // It changes the "Clear" color before any geometry is drawn.
            _cam.backgroundColor = ColorManager.Instance.CurrentColor;
        }
    }
}