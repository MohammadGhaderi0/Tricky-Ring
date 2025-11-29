using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ObstacleManager : MonoBehaviour
{
    [Header("Scene References")]
    // Drag your 12 existing obstacles here in order
    public List<Obstacle> sceneObstacles = new List<Obstacle>(); 
    
    [Header("Configuration")]
    public float safetyMarginDegrees = 75f; 

    private PlayerController _player;

    public void Setup(PlayerController player)
    {
        _player = player;
        InitializeExistingObstacles();
    }

    private void InitializeExistingObstacles()
    {
        // Assuming 12 obstacles, 30 degrees apart
        float step = 360f / sceneObstacles.Count;

        for (int i = 0; i < sceneObstacles.Count; i++)
        {
            Obstacle obs = sceneObstacles[i];
            
            // We calculate the angle based on the list index. 
            // Make sure your objects in the scene match this rotation!
            float calculatedAngle = i * step;

            obs.Initialize(i, calculatedAngle);
            
            // Ensure visual state matches logic (all start disabled)
            obs.SetState(false, false); 
        }
    }

    public void ActivateNextObstacle()
    {
        var inactive = sceneObstacles.Where(o => !o.IsActive).ToList();
        if (inactive.Count == 0) return;

        var safeCandidates = new List<Obstacle>();

        foreach (var obs in inactive)
        {
            float diff = Mathf.Abs(Mathf.DeltaAngle(_player.CurrentAngle, obs.AnglePosition));
            if (diff > safetyMarginDegrees)
            {
                safeCandidates.Add(obs);
            }
        }

        var pool = safeCandidates.Count > 0 ? safeCandidates : inactive;
        var choice = pool[Random.Range(0, pool.Count)];

        bool startInner = Random.value > 0.5f;
        choice.SetState(true, startInner);
    }

    public void FlipRandomObstacle()
    {
        var active = sceneObstacles.Where(o => o.IsActive).ToList();
        if (active.Count == 0) return;

        // NEW: Filter candidates to only those far enough from the player
        var safeCandidates = new List<Obstacle>();

        foreach (var obs in active)
        {
            float diff = Mathf.Abs(Mathf.DeltaAngle(_player.CurrentAngle, obs.AnglePosition));
            
            // Reuse your existing safety margin
            if (diff > safetyMarginDegrees)
            {
                safeCandidates.Add(obs);
            }
        }

        // If no obstacles are safe to flip, do nothing (prevent unfair death)
        if (safeCandidates.Count == 0) return;

        var choice = safeCandidates[Random.Range(0, safeCandidates.Count)];
        choice.ToggleSide();
    }

    public bool IsPositionOccupied(float angle, bool isInner)
    {
        foreach (var obs in sceneObstacles)
        {
            if (obs.IsActive && obs.IsInner == isInner)
            {
                float diff = Mathf.Abs(Mathf.DeltaAngle(angle, obs.AnglePosition));
                if (diff < 15f) return true; 
            }
        }
        return false;
    }
}