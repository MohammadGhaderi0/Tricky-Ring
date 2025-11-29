using System.Collections;
using Unity.Burst.CompilerServices;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [Header("References")] public PlayerController player;
    public ObstacleManager obstacleManager;
    public CameraController cameraController;
    public Transform singlePointObject;

    // UI Elements
    public UIDocument uiDocument;
    private VisualElement _root;
    private Label _scoreLabel;
    private VisualElement _gameOverPanel;
    private Label _bestScoreLabel;
    private Button _pauseBtn; // To hide it on game over

    // Buttons
    private Button _replayBtn;
    private Button _homeBtn;

    [Header("Game Data")] public int score = 0;
    public int streak = 1;
    private float _pointSpawnRotationSnapshot;
    private int _itemsCollected = 0;

    [Header("Animation Settings")] public float uiDuration = 0.8f;
    public float scoreLabelMoveY = 400f;

    [Header("Spawn Settings")] 
    public LayerMask obstacleLayer;
    public float pointCheckRadius = 0.8f;
    [SerializeField] private GameObject highestScoreParticle;

    [Header("Audio")] [SerializeField] AudioSource audioSource;
    [SerializeField] private AudioClip gameOverSound;
    [SerializeField] private AudioClip beat1Sound;
    [SerializeField] private AudioClip beat2Sound;
    [SerializeField] private AudioClip beat3Sound;
    [SerializeField] private AudioClip beat4Sound;
    [SerializeField] private AudioClip beat5Sound;

    private const int POINTS_TO_CHANGE_COLOR = 5;
    private int _remainingPointsToChangeColor;


    void Awake()
    {
        Instance = this;
        _remainingPointsToChangeColor = 5;
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
        score = 0;
        streak = 1;
        _itemsCollected = 0;
        UpdateScoreUI();

        if (_pauseBtn != null) _pauseBtn.style.display = DisplayStyle.Flex;

        obstacleManager.ActivateNextObstacle();
        SpawnPoint();
    }

    public void OnPointCollected()
    {
        score += streak;
        if (streak < 5) streak++;
        PlayScoreSound();
        UpdateScoreUI();
        player.IncreaseSpeed(0.02f);
        _itemsCollected++;

        if (_itemsCollected % 2 == 0) obstacleManager.ActivateNextObstacle();
        else obstacleManager.FlipRandomObstacle();

        singlePointObject.gameObject.SetActive(false);
        SpawnPoint();
        CheckForBackGroundColorChange();
    }

    private void CheckForBackGroundColorChange()
    {
        _remainingPointsToChangeColor-= streak;
        if (_remainingPointsToChangeColor < 1)
        {
            Debug.Log("Changing color");
            ColorManager.Instance.NextColor();
            _remainingPointsToChangeColor = POINTS_TO_CHANGE_COLOR;
        }
    }

    private void PlayScoreSound()
    {
        switch (streak)
        {
            case 1:
                audioSource.PlayOneShot(beat1Sound);
                break;
            case 2:
                audioSource.PlayOneShot(beat2Sound);
                break;
            case 3:
                audioSource.PlayOneShot(beat3Sound);
                break;
            case 4:
                audioSource.PlayOneShot(beat4Sound);
                break;
            case 5:
                audioSource.PlayOneShot(beat5Sound);
                break;
        }
    }

private void UpdateScoreUI()
    {
        if (_scoreLabel != null) _scoreLabel.text = score.ToString();
    }

    private void SpawnPoint()
    {
        bool valid = false;
        int attempts = 0;
        float angle = 0;
        bool side = false;

        // Loop until we find a valid spot or run out of attempts
        while (!valid && attempts < 20)
        {
            attempts++;
            
            angle = Random.Range(0f, 360f);
            side = Random.value > 0.5f;

            // 1. Keep the check to avoid spawning exactly on top of player
            if (Mathf.Abs(Mathf.DeltaAngle(player.currentAngle, angle)) < 45f) continue;

            // 2. Calculate potential position
            float r = side ? player.innerRadius : player.outerRadius;
            float rad = angle * Mathf.Deg2Rad;
            Vector2 checkPos = new Vector2(Mathf.Cos(rad) * r, Mathf.Sin(rad) * r);

            // Returns null if no collider is hit in the circle
            Collider2D hit = Physics2D.OverlapCircle(checkPos, pointCheckRadius, obstacleLayer);
            
            if (hit == null) 
            {
                valid = true;
            }
        }

        // Apply position
        float finalR = side ? player.innerRadius : player.outerRadius;
        float finalRad = angle * Mathf.Deg2Rad;

        singlePointObject.position = new Vector3(Mathf.Cos(finalRad) * finalR, Mathf.Sin(finalRad) * finalR, 0);
        singlePointObject.rotation = Quaternion.Euler(0, 0, angle);
        singlePointObject.gameObject.SetActive(true);

        _pointSpawnRotationSnapshot = player.totalRotation;
    }

    void Update()
    {
        if (player.totalRotation - _pointSpawnRotationSnapshot > 360f)
        {
            if (streak > 1) streak = 1;
        }
    }

    public void GameOver()
    {
        player.Die();
        audioSource.PlayOneShot(gameOverSound);
        // Hide Pause Button
        if (_pauseBtn != null) _pauseBtn.style.display = DisplayStyle.None;

        // Save Best Score
        int currentBest = PlayerPrefs.GetInt("HighScore", 0);
        if (score > currentBest)
        {
            currentBest = score;
            PlayerPrefs.SetInt("HighScore", currentBest);
            StartCoroutine(PlayParticleWithDelay());
        }
        if (_bestScoreLabel != null) _bestScoreLabel.text = currentBest.ToString();

        // Trigger Animations
        if (cameraController != null) cameraController.MoveDown();
        StartCoroutine(AnimateGameOverUI());
    }

    IEnumerator PlayParticleWithDelay()
    {
        yield return new WaitForSeconds(1.5f);
        highestScoreParticle.SetActive(true);
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