using UnityEngine;

public class Obstacle : MonoBehaviour
{
    public int ID;
    public float AnglePosition; // Set by ObstacleManager
    public bool IsActive = false;
    public bool IsInner = false;

    [Header("Settings")]
    public float innerRadius = 2.5f;
    public float outerRadius = 4.5f;

    [Header("Optional")]
    public bool flipRotation = false; 

    public void Initialize(int id, float angle)
    {
        ID = id;
        AnglePosition = angle;
        UpdatePosition(); 
        gameObject.SetActive(false);
    }

    public void SetState(bool active, bool inner)
    {
        IsActive = active;
        IsInner = inner;
        
        gameObject.SetActive(active);
        
        if (active)
        {
            UpdatePosition();
        }
    }

    public void ToggleSide()
    {
        if (!IsActive) return;
        SetState(true, !IsInner);
    }

    private void UpdatePosition()
    {
        float rad = AnglePosition * Mathf.Deg2Rad;
        float r = IsInner ? innerRadius : outerRadius;

        float x = Mathf.Cos(rad) * r;
        float y = Mathf.Sin(rad) * r;

        transform.localPosition = new Vector3(x, y, 0);

        if (flipRotation)
        {
            float zRot = AnglePosition + (IsInner ? 180f : 0f);
            transform.localRotation = Quaternion.Euler(0, 0, zRot);
        }
    }
}