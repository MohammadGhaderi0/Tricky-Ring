using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [Header("References")]
    public PlayerController player;
    public ObstacleManager obstacleManager;
    
    [Tooltip("Drag the single Point object from your scene here")]
    public Transform singlePointObject; 

    [Header("Game Data")]
    public int Score = 0;
    public int Streak = 1;

    private float _pointSpawnRotationSnapshot;
    private int _itemsCollected = 0;

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        obstacleManager.Setup(player);
        StartGame();
    }

    public void StartGame()
    {
        Score = 0;
        Streak = 1;
        _itemsCollected = 0;
        
        obstacleManager.ActivateNextObstacle();
        SpawnPoint();
    }

    public void OnPointCollected()
    {
        Score += Streak;
        if (Streak < 4) Streak++;

        player.IncreaseSpeed(0.05f);
        _itemsCollected++;

        // Every 2 points -> New Obstacle
        // Every 1 point (if not new obstacle) -> Flip Obstacle
        if (_itemsCollected % 2 == 0)
        {
            obstacleManager.ActivateNextObstacle();
        }
        else
        {
            obstacleManager.FlipRandomObstacle();
        }

        // Instead of destroying, we just move it
        singlePointObject.gameObject.SetActive(false); 
        SpawnPoint();
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

        // Reposition the existing point
        float r = side ? player.innerRadius : player.outerRadius;
        float rad = angle * Mathf.Deg2Rad;
        
        singlePointObject.position = new Vector3(Mathf.Cos(rad) * r, Mathf.Sin(rad) * r, 0);
        singlePointObject.rotation = Quaternion.Euler(0, 0, angle); // Optional: Rotate sprite to face center
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
                Debug.Log("Streak Lost!");
            }
        }
    }

    public void GameOver()
    {
        player.Die();
    }
}