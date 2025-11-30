using System.Collections;
using System.IO; // Required for Path and File operations
using UnityEngine;
using UnityEngine.UIElements;

public class GameHUDController : MonoBehaviour
{
    private UIDocument _doc;
    private VisualElement _pauseOverlay;
    private Label _countdownLabel;
    private Button _resumeBtn;
    
    // Added: Reference for the share button
    private Button _shareBtn;

    private bool _isResuming = false;

    private void OnEnable()
    {
        _doc = GetComponent<UIDocument>();
        if (_doc == null) { Debug.LogError("No UIDocument found!"); return; }

        var root = _doc.rootVisualElement;

        var pauseBtn = root.Q<Button>("PauseBtn");
        _pauseOverlay = root.Q<VisualElement>("PauseOverlay");
        _resumeBtn = root.Q<Button>("ResumeBtn");
        _countdownLabel = root.Q<Label>("CountdownLabel");

        // Added: Query the Share Button from UXML
        _shareBtn = root.Q<Button>("ShareBtn");

        if (pauseBtn != null) pauseBtn.clicked += PauseGame;
        if (_resumeBtn != null) _resumeBtn.clicked += StartResumeSequence;
        
        // Added: Hook up the share click event
        if (_shareBtn != null) _shareBtn.clicked += OnShareBtnClicked;
        
        if (_pauseOverlay != null) _pauseOverlay.style.display = DisplayStyle.None;
    }

    private void OnShareBtnClicked()
    {
        // We run this as a Coroutine to wait for the screenshot to save
        StartCoroutine(ShareRoutine());
    }

    // Added: The Sharing Logic
    private IEnumerator ShareRoutine()
    {
        // 1. Wait for the frame to finish rendering
        yield return new WaitForEndOfFrame();

        // 2. Define the path (Persistent Data Path works best on mobile)
        string fileName = "tricky_ball_screenshot.png";
        string filePath = Path.Combine(Application.persistentDataPath, fileName);

        // 3. Capture the screenshot
        // If a previous file exists, this overwrites it
        ScreenCapture.CaptureScreenshot(fileName);

        // 4. Wait a moment to ensure the file is written to the disk
        yield return new WaitForSecondsRealtime(0.5f);

        if (!File.Exists(filePath))
        {
            Debug.LogError("Screenshot failed to save at: " + filePath);
            yield break;
        }

        // 5. Use NativeShare to share the text and image
        new NativeShare()
            .SetSubject("Tricky Ball High Score")
            .SetText("Hey I'm playing Tricky ball. Come and see if you can break my record!\ngoogle.com")
            .AddFile(filePath)
            .Share();
            
        // Optional: Delete the file afterwards to save space, 
        // but NativeShare usually handles temp files well.
    }

    private void PauseGame()
    {
        if (_isResuming) return;

        _pauseOverlay.style.display = DisplayStyle.Flex;
        _resumeBtn.style.display = DisplayStyle.Flex;
        _countdownLabel.style.display = DisplayStyle.None;

        Time.timeScale = 0f;
    }

    private void StartResumeSequence()
    {
        StartCoroutine(ResumeRoutine());
    }

    private IEnumerator ResumeRoutine()
    {
        _isResuming = true;

        _pauseOverlay.style.display = DisplayStyle.None;
        _countdownLabel.style.display = DisplayStyle.Flex;

        _countdownLabel.text = "3";
        yield return new WaitForSecondsRealtime(1.0f);

        _countdownLabel.text = "2";
        yield return new WaitForSecondsRealtime(1.0f);

        _countdownLabel.text = "1";
        yield return new WaitForSecondsRealtime(1.0f);

        _countdownLabel.style.display = DisplayStyle.None;
        Time.timeScale = 1f;

        _isResuming = false;
    }
}