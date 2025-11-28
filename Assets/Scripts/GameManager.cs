using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [Header("References")] 
    public PlayerController player;
    public ObstacleManager obstacleManager;
    public CameraController cameraController; // New Reference

    [Tooltip("Drag the single Point object from your scene here")]
    public Transform singlePointObject;

    [Header("UI References")] 
    public UIDocument uiDocument;

    // UI Elements
    private VisualElement _root;
    private Label _scoreLabel;
    private VisualElement _gameOverPanel;
    private Label _bestScoreLabel;
    private Button _pauseBtn; // To hide it on game over
    
    // Buttons
    private Button _replayBtn;
    private Button _homeBtn;

    [Header("Game Data")] 
    public int Score = 0;
    public int Streak = 1;

    private float _pointSpawnRotationSnapshot;
    private int _itemsCollected = 0;

    [Header("Animation Settings")]
    public float uiDuration = 0.8f;     
    // How far DOWN the score label should move (pixels)
    public float scoreLabelMoveY = 400f; 

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        if (uiDocument != null)
        {
            _root = uiDocument.rootVisualElement;
            _scoreLabel = _root.Q<Label>("ScoreLabel");
            _gameOverPanel = _root.Q<VisualElement>("GameOverPanel");
            _bestScoreLabel = _root.Q<Label>("BestScoreVal");
            _pauseBtn = _root.Q<Button>("PauseBtn");
            
            _replayBtn = _root.Q<Button>("ReplayBtn");
            _homeBtn = _root.Q<Button>("HomeBtn");

            if (_replayBtn != null) _replayBtn.clicked += RestartGame;
            if (_homeBtn != null) _homeBtn.clicked += GoHome;

            // Ensure Game Over panel is hidden/reset at start
            if (_gameOverPanel != null) _gameOverPanel.style.bottom = Length.Percent(-100); 
        }

        obstacleManager.Setup(player);
        StartGame();
    }

    public void StartGame()
    {
        Score = 0;
        Streak = 1;
        _itemsCollected = 0;
        UpdateScoreUI();
        
        if (_pauseBtn != null) _pauseBtn.style.display = DisplayStyle.Flex;

        obstacleManager.ActivateNextObstacle();
        SpawnPoint();
    }

    public void OnPointCollected()
    {
        Score += Streak;
        if (Streak < 4) Streak++;

        UpdateScoreUI();
        player.IncreaseSpeed(0.05f);
        _itemsCollected++;

        if (_itemsCollected % 2 == 0) obstacleManager.ActivateNextObstacle();
        else obstacleManager.FlipRandomObstacle();

        singlePointObject.gameObject.SetActive(false);
        SpawnPoint();
    }

    private void UpdateScoreUI()
    {
        if (_scoreLabel != null) _scoreLabel.text = Score.ToString();
    }

    private void SpawnPoint()
    {
        // (Logic unchanged)
        bool valid = false;
        int attempts = 0;
        float angle = 0;
        bool side = false;

        while (!valid && attempts < 20)
        {
            angle = Random.Range(0f, 360f);
            side = Random.value > 0.5f;
            if (Mathf.Abs(Mathf.DeltaAngle(player.CurrentAngle, angle)) < 45f) { attempts++; continue; }
            if (!obstacleManager.IsPositionOccupied(angle, side)) valid = true;
            attempts++;
        }

        float r = side ? player.innerRadius : player.outerRadius;
        float rad = angle * Mathf.Deg2Rad;

        singlePointObject.position = new Vector3(Mathf.Cos(rad) * r, Mathf.Sin(rad) * r, 0);
        singlePointObject.rotation = Quaternion.Euler(0, 0, angle);
        singlePointObject.gameObject.SetActive(true);

        _pointSpawnRotationSnapshot = player.TotalRotation;
    }

    void Update()
    {
        if (player.TotalRotation - _pointSpawnRotationSnapshot > 360f)
        {
            if (Streak > 1) Streak = 1;
        }
    }

    public void GameOver()
    {
        player.Die();
        
        // Hide Pause Button
        if (_pauseBtn != null) _pauseBtn.style.display = DisplayStyle.None;

        // Save Best Score
        int currentBest = PlayerPrefs.GetInt("HighScore", 0);
        if (Score > currentBest)
        {
            currentBest = Score;
            PlayerPrefs.SetInt("HighScore", currentBest);
        }
        if (_bestScoreLabel != null) _bestScoreLabel.text = currentBest.ToString();

        // Trigger Animations
        if (cameraController != null) cameraController.MoveDown();
        StartCoroutine(AnimateGameOverUI());
    }

    private IEnumerator AnimateGameOverUI()
    {
        yield return new WaitForSeconds(1f); // Wait for camera to start/finish logic

        float elapsedTime = 0f;
        
        // Setup Start/End values
        // Score moves from Y=0 to Y=scoreLabelMoveY
        float startScoreY = 0f;
        float targetScoreY = scoreLabelMoveY;

        // Game Over Panel moves from -100% to 0% (using Length)
        Length startPanelBottom = Length.Percent(-100);
        Length targetPanelBottom = Length.Percent(0);

        while (elapsedTime < uiDuration)
        {
            float t = elapsedTime / uiDuration;
            t = t * t * (3f - 2f * t); // Smooth step

            // 1. Move Score Label DOWN
            if (_scoreLabel != null)
            {
                float currentY = Mathf.Lerp(startScoreY, targetScoreY, t);
                _scoreLabel.style.translate = new Translate(0, currentY, 0);
            }

            // 2. Move Game Over Panel UP
            if (_gameOverPanel != null)
            {
                // We manually interpolate the percent value
                float currentPercent = Mathf.Lerp(-100f, 0f, t);
                _gameOverPanel.style.bottom = Length.Percent(currentPercent);
            }

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        // Snap to final values
        if (_scoreLabel != null) _scoreLabel.style.translate = new Translate(0, targetScoreY, 0);
        if (_gameOverPanel != null) _gameOverPanel.style.bottom = Length.Percent(0);
    }

    private void RestartGame()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    private void GoHome()
    {
        SceneManager.LoadScene("Main Menu"); 
    }
}