using System;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("Settings")]
    public float baseRotationSpeed = 100f; 
    public float innerRadius = 2.5f;
    public float outerRadius = 4.5f;
    public float switchSpeed = 10f; 

    [Header("State (Read Only)")]
    public float currentAngle; 
    public float totalRotation; 
    public bool isInner = false;

    private float _currentRadius;
    private float _speedMultiplier = 1.0f;
    private bool _isDead = false;
    private Rigidbody2D _rb;
    
    [Header("Audio")]
    [SerializeField] AudioSource audioSource;
    [SerializeField] private AudioClip jumpSound;


    private void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
    }

    void Start()
    {
        _currentRadius = outerRadius;
        currentAngle = 0f; 
    }

    void Update()
    {
        if (_isDead) return;
        HandleInput();
    }

    void FixedUpdate()
    {
        if (_isDead) return;
        MovePlayer();
    }

    private void HandleInput()
    {
        if (Input.GetMouseButtonDown(0))
        {
            isInner = !isInner;
            audioSource.PlayOneShot(jumpSound);
        }
    }

    private void MovePlayer()
    {
        // 1. Increment Angle
        float step = baseRotationSpeed * _speedMultiplier * Time.fixedDeltaTime;
        currentAngle = (currentAngle + step) % 360f;
        totalRotation += step;

        // 2. Lerp Radius
        float targetR = isInner ? innerRadius : outerRadius;
        // fixedDeltaTime here as well
        _currentRadius = Mathf.Lerp(_currentRadius, targetR, Time.fixedDeltaTime * switchSpeed);

        // 3. Apply Position (Polar to Cartesian)
        float rad = currentAngle * Mathf.Deg2Rad;
        Vector3 newPos = new Vector3(Mathf.Cos(rad) * _currentRadius, Mathf.Sin(rad) * _currentRadius, 0);
        
        // Using MovePosition instead of transform.position
        _rb.MovePosition(newPos);
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

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Obstacle"))
        {
            GameManager.Instance.GameOver();
        }
        else if (other.CompareTag("Point"))
        {
            GameManager.Instance.OnPointCollected();
        }
    }
}