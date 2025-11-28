using System.Collections;
using UnityEngine;
using UnityEngine.UIElements;

public class GameHUDController : MonoBehaviour
{
    private UIDocument _doc;
    private VisualElement _pauseOverlay;
    private Label _countdownLabel;
    private Button _resumeBtn;
    
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

        if (pauseBtn != null) pauseBtn.clicked += PauseGame;
        if (_resumeBtn != null) _resumeBtn.clicked += StartResumeSequence;
        
        if (_pauseOverlay != null) _pauseOverlay.style.display = DisplayStyle.None;
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

        // 1. Hide Overlay immediately
        _pauseOverlay.style.display = DisplayStyle.None;
        _countdownLabel.style.display = DisplayStyle.Flex;

        // 2. Countdown 3.. 2.. 1..
        _countdownLabel.text = "3";
        yield return new WaitForSecondsRealtime(1.0f);

        _countdownLabel.text = "2";
        yield return new WaitForSecondsRealtime(1.0f);

        _countdownLabel.text = "1";
        yield return new WaitForSecondsRealtime(1.0f);

        // 3. Immediately hide label and resume (No "GO" text)
        _countdownLabel.style.display = DisplayStyle.None;
        Time.timeScale = 1f;

        _isResuming = false;
    }
}