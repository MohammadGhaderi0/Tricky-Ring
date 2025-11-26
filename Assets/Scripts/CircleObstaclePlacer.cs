using UnityEngine;

public class CircleObstaclePlacerRuntime : MonoBehaviour
{
    public GameObject obstaclePrefab;
    public int count = 12;
    public float radius = 3f;
    
    [Header("Adjustments")]
    [Tooltip("Change this to 0, 90, 180, or -90 to fix orientation")]
    public float rotationAdjustment = 0f; 
    public bool placeOnStart = true;

    void Start()
    {
        if (placeOnStart)
            Place();
    }

    [ContextMenu("Place Obstacles")]
    public void Place()
    {
        if (obstaclePrefab == null) return;

        // Clear old ones
        for (int i = transform.childCount - 1; i >= 0; i--)
            DestroyImmediate(transform.GetChild(i).gameObject);

        for (int i = 0; i < count; i++)
        {
            float angleDeg = i * (360f / count);
            float angleRad = angleDeg * Mathf.Deg2Rad;

            // Position
            Vector3 pos = new Vector3(
                Mathf.Cos(angleRad) * radius,
                Mathf.Sin(angleRad) * radius,
                0f
            );

            GameObject o = Instantiate(obstaclePrefab, transform);
            o.transform.localPosition = pos;

            // Rotation Logic
            // 1. Calculate angle pointing outward from center
            float rotZ = Mathf.Atan2(pos.y, pos.x) * Mathf.Rad2Deg;
            
            // 2. Apply the adjustment (likely needs to be 90 or -90 based on your sprite)
            o.transform.rotation = Quaternion.Euler(0, 0, rotZ + rotationAdjustment);
        }
    }
}