using UnityEngine;

public class FrameRateManager : MonoBehaviour
{
    [Header("Settings")]
    public int targetFPS = 60;
    public bool keepScreenOn = true;

    private void Awake()
    {
        // 1. VSync must be disabled for targetFrameRate to work on many platforms
        QualitySettings.vSyncCount = 0;
        Application.targetFrameRate = targetFPS;
        if (keepScreenOn)
        {
            Screen.sleepTimeout = SleepTimeout.NeverSleep;
        }
        else
        {
            Screen.sleepTimeout = SleepTimeout.SystemSetting;
        }
    }
}