using System.Collections;
using UnityEngine;
using UnityEngine.UIElements; // Required for UI Toolkit

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [Header("References")] public PlayerController player;
    public ObstacleManager obstacleManager;

    [Tooltip("Drag the single Point object from your scene here")]
    public Transform singlePointObject;

    [Header("UI References")] [Tooltip("Assign the GameObject with the UI Document component here")]
    public UIDocument uiDocument;

    private Label _scoreLabel;

    [Header("Game Data")] public int Score = 0;
    public int Streak = 1;

    private float _pointSpawnRotationSnapshot;
    private int _itemsCollected = 0;
    [SerializeField] private Camera _gameCamera;
    private Vector3 _cameraOffset = new Vector3(0, -20, 0);
    
    [Header("Animation Settings")]
    public float uiMoveDistanceY = -1000f;
    public float cameraDuration = 1.5f; // Camera takes 1.5s
    public float uiDuration = 0.5f;     // UI takes 0.5s (Faster!)
    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        // Initialize UI
        if (uiDocument != null)
        {
            var root = uiDocument.rootVisualElement;
            // "ScoreLabel" must match the name in your UXML file
            _scoreLabel = root.Q<Label>("ScoreLabel");

            if (_scoreLabel == null) Debug.LogError("Could not find Label named 'ScoreLabel' in UXML!");
        }
        else
        {
            Debug.LogError("UIDocument is not assigned in GameManager Inspector!");
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

        obstacleManager.ActivateNextObstacle();
        SpawnPoint();
    }

    public void OnPointCollected()
    {
        Score += Streak;
        if (Streak < 4) Streak++;

        UpdateScoreUI(); // Update the UI Label

        player.IncreaseSpeed(0.05f);
        _itemsCollected++;

        if (_itemsCollected % 2 == 0)
        {
            obstacleManager.ActivateNextObstacle();
        }
        else
        {
            obstacleManager.FlipRandomObstacle();
        }

        singlePointObject.gameObject.SetActive(false);
        SpawnPoint();
    }

    private void UpdateScoreUI()
    {
        if (_scoreLabel != null)
        {
            _scoreLabel.text = Score.ToString();
        }
        else
        {
            Debug.LogWarning("ScoreLabel is NULL. UI not updating.");
        }
    }

    private void SpawnPoint()
    {
        bool valid = false;
        int attempts = 0;
        float angle = 0;
        bool side = false;

        while (!valid && attempts < 20)
        {
            angle = Random.Range(0f, 360f);
            side = Random.value > 0.5f;

            if (Mathf.Abs(Mathf.DeltaAngle(player.CurrentAngle, angle)) < 45f)
            {
                attempts++;
                continue;
            }

            if (!obstacleManager.IsPositionOccupied(angle, side))
            {
                valid = true;
            }

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
            if (Streak > 1)
            {
                Streak = 1;
                // Optional: Update UI color or effect here to show streak reset
            }
        }
    }

    public void GameOver()
    {
        player.Die();
        StartCoroutine(MoveDownTheCamera());
    }

    private IEnumerator MoveDownTheCamera()
    {
        yield return new WaitForSeconds(1);

        Vector3 startCamPos = _gameCamera.transform.position;
        Vector3 targetCamPos = startCamPos + _cameraOffset;
        
        float startUiY = 0f;
        float targetUiY = uiMoveDistanceY; 

        float elapsedTime = 0f;

        while (elapsedTime < cameraDuration)
        {
            // 1. Calculate Camera Progress (0 to 1)
            float t_Cam = elapsedTime / cameraDuration;
            t_Cam = t_Cam * t_Cam * (3f - 2f * t_Cam); // Smooth step

            _gameCamera.transform.position = Vector3.Lerp(startCamPos, targetCamPos, t_Cam);

            // 2. Calculate UI Progress independently
            float t_UI = Mathf.Clamp01(elapsedTime / uiDuration); 
            t_UI = t_UI * t_UI * (3f - 2f * t_UI); // Smooth step

            if (_scoreLabel != null)
            {
                float currentUiY = Mathf.Lerp(startUiY, targetUiY, t_UI);
                _scoreLabel.style.translate = new Translate(
                    new Length(0, LengthUnit.Pixel), 
                    new Length(currentUiY, LengthUnit.Pixel), 
                    0);
            }
            
            elapsedTime += Time.deltaTime;
            yield return null;
        }
    }
}