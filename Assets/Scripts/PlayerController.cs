using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("Settings")]
    public float baseRotationSpeed = 100f; // Degrees per second
    public float innerRadius = 2.5f;
    public float outerRadius = 4.5f;
    public float switchSpeed = 10f; // Lerp speed

    [Header("State (Read Only)")]
    public float CurrentAngle; // 0 to 360
    public float TotalRotation; // Accumulates forever (for streak calculation)
    public bool IsInner = false;

    private float _currentRadius;
    private float _speedMultiplier = 1.0f;
    private bool _isDead = false;

    void Start()
    {
        _currentRadius = outerRadius;
        CurrentAngle = 0f; 
    }

    void Update()
    {
        if (_isDead) return;

        HandleInput();
        MovePlayer();
    }

    private void HandleInput()
    {
        // Touch or Click
        if (Input.GetMouseButtonDown(0))
        {
            IsInner = !IsInner;
        }
    }

    private void MovePlayer()
    {
        // 1. Increment Angle
        float step = baseRotationSpeed * _speedMultiplier * Time.deltaTime;
        CurrentAngle = (CurrentAngle + step) % 360f;
        TotalRotation += step;

        // 2. Lerp Radius
        float targetR = IsInner ? innerRadius : outerRadius;
        _currentRadius = Mathf.Lerp(_currentRadius, targetR, Time.deltaTime * switchSpeed);

        // 3. Apply Position (Polar to Cartesian)
        float rad = CurrentAngle * Mathf.Deg2Rad;
        Vector3 newPos = new Vector3(Mathf.Cos(rad) * _currentRadius, Mathf.Sin(rad) * _currentRadius, 0);
        transform.position = newPos;
    }

    public void IncreaseSpeed(float amount)
    {
        _speedMultiplier += amount;
    }

    public void Die()
    {
        _isDead = true;
        Debug.Log("Game Over");
    }

    // ADDED: Centralized Collision Detection
    private void OnTriggerEnter2D(Collider2D other)
    {
        // Check for Obstacle Tag
        if (other.CompareTag("Obstacle"))
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.GameOver();
            }
            else
            {
                Die();
            }
        }
        else if (other.CompareTag("Point"))
        {
            // Debug Log to confirm collision logic works
            Debug.Log("Player hit Point! notifying GameManager...");
            
            if (GameManager.Instance != null)
            {
                GameManager.Instance.OnPointCollected();
            }
            else 
            {
                Debug.LogError("GameManager Instance is NULL!");
            }
        }
    }
}