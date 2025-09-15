using UnityEngine;
using System.Collections.Generic;

public class PowerUpSpawner : MonoBehaviour
{
    [Header("Power-Up Settings")]
    public List<GameObject> powerUpPrefabs; // list of all possible prefabs (teleport at index 0)
    public int activeCount = 5;             // how many to spawn at once
    public Vector3 spawnAreaSize = new Vector3(10, 0, 10);

    [Header("Spawn Options")]
    public bool useRaycast = false;         // if true, raycast to terrain instead of flat ground
    public LayerMask groundMask;            // layer for the ground/terrain

    private List<GameObject> activePowerUps = new List<GameObject>();
    private int teleportSpawned = 0; // track how many teleports we've spawned
    private int totalSpawned = 0;    // track total spawned

    void Start()
    {
        teleportSpawned = 0;
        totalSpawned = 0;
        
        for (int i = 0; i < activeCount; i++)
            SpawnPowerUp();
    }

    void SpawnPowerUp()
    {
        if (powerUpPrefabs.Count == 0) return;

        GameObject prefab;
        
        // For first 5 spawns: 2 teleports, 3 others
        if (totalSpawned < activeCount)
        {
            if (teleportSpawned < 2)
            {
                // Spawn teleport (index 0)
                prefab = powerUpPrefabs[0];
                teleportSpawned++;
            }
            else
            {
                // Spawn random other power-up (index 1 or beyond) - now includes shield
                if (powerUpPrefabs.Count > 1)
                {
                    int randomIndex = Random.Range(1, powerUpPrefabs.Count);
                    prefab = powerUpPrefabs[randomIndex];
                }
                else
                {
                    prefab = powerUpPrefabs[0]; // fallback
                }
            }
            totalSpawned++;
        }
        else
        {
            // After initial spawn, maintain ratio: need to check what was collected
            int currentTeleports = 0;
            int currentOthers = 0;
            
            // Count current power-ups
            foreach (GameObject activePU in activePowerUps)
            {
                if (activePU.GetComponent<TeleportPowerUp>() != null)
                    currentTeleports++;
                else
                    currentOthers++;
            }
            
            // Maintain 2 teleports, 3 others
            if (currentTeleports < 2)
            {
                prefab = powerUpPrefabs[0]; // spawn teleport
            }
            else
            {
                // spawn random other
                if (powerUpPrefabs.Count > 1)
                {
                    int randomIndex = Random.Range(1, powerUpPrefabs.Count);
                    prefab = powerUpPrefabs[randomIndex];
                }
                else
                {
                    prefab = powerUpPrefabs[0];
                }
            }
        }

        // Updated spawn ranges: X(-27 to 28), Z(-27 to 28)
        float randX = Random.Range(-27f, 28f);
        float randZ = Random.Range(-27f, 28f);

        Vector3 spawnPos = new Vector3(randX, 1f, randZ); // Simple flat spawn

        GameObject pu = Instantiate(prefab, spawnPos, Quaternion.identity);
        activePowerUps.Add(pu);
    }

    public void OnPowerUpCollected(GameObject pu)
    {
        if (activePowerUps.Contains(pu))
        {
            // Check if it was a teleport power-up being collected
            TeleportPowerUp teleportScript = pu.GetComponent<TeleportPowerUp>();
            if (teleportScript != null)
            {
                teleportSpawned--;
            }

            activePowerUps.Remove(pu);
            Destroy(pu);
            SpawnPowerUp(); // spawn replacement
        }
    }

    public GameObject GetRandomOtherPowerUp(GameObject exclude)
    {
        List<GameObject> candidates = new List<GameObject>(activePowerUps);
        candidates.Remove(exclude);
        if (candidates.Count > 0)
            return candidates[Random.Range(0, candidates.Count)];
        return null;
    }
}
