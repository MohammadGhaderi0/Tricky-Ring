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
        // 1. Get all inactive obstacles
        var inactive = sceneObstacles.Where(o => !o.IsActive).ToList();
        if (inactive.Count == 0) return;

        // 2. Filter: Keep only those far enough from the player
        var safeFromPlayer = new List<Obstacle>();
        foreach (var obs in inactive)
        {
            float diff = Mathf.Abs(Mathf.DeltaAngle(_player.currentAngle, obs.AnglePosition));
            if (diff > safetyMarginDegrees)
            {
                safeFromPlayer.Add(obs);
            }
        }

        // 3. Filter: Keep only those that DON'T have active neighbors
        var safeFromNeighbors = new List<Obstacle>();
        int count = sceneObstacles.Count;

        foreach (var obs in safeFromPlayer)
        {
            // Use modulo (%) to wrap around. 
            // If ID is 0, (0 - 1 + 12) % 12 = 11.
            int prevID = (obs.ID - 1 + count) % count;
            int nextID = (obs.ID + 1) % count;

            // Check if neighbors are active by looking them up in the main list
            bool isNeighborActive = sceneObstacles[prevID].IsActive || sceneObstacles[nextID].IsActive;

            if (!isNeighborActive)
            {
                safeFromNeighbors.Add(obs);
            }
        }

        // 4. Selection Logic
        // Priority 1: Safe from Player AND Safe from Neighbors
        // Priority 2: Safe from Player (but might have a neighbor)
        // Priority 3: Any inactive obstacle (emergency fallback)
        List<Obstacle> pool;

        if (safeFromNeighbors.Count > 0)
        {
            pool = safeFromNeighbors;
        }
        else if (safeFromPlayer.Count > 0)
        {
            pool = safeFromPlayer;
        }
        else
        {
            pool = inactive;
        }

        // Pick random and activate
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
            float diff = Mathf.Abs(Mathf.DeltaAngle(_player.currentAngle, obs.AnglePosition));
            
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