// TeleportPowerUp.cs - Fixed version with better error handling
using UnityEngine;

public class TeleportPowerUp : MonoBehaviour
{
    private PowerUpSpawner spawner;
    private float lastTeleportTime = 0f;
    private float teleportCooldown = 1f;

    void Start()
    {
        spawner = FindFirstObjectByType<PowerUpSpawner>();
        if (spawner == null)
        {
            Debug.LogError("TeleportPowerUp: No PowerUpSpawner found in the scene!");
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && Time.time > lastTeleportTime + teleportCooldown)
        {
            if (spawner == null)
            {
                Debug.LogError("TeleportPowerUp: PowerUpSpawner is null!");
                return;
            }

            // Find another random teleport on the map
            GameObject targetTeleport = spawner.GetRandomTeleportTarget(this.gameObject);
            
            if (targetTeleport != null)
            {
                // Add random offset to avoid spawning directly on power-up
                Vector3 randomOffset = new Vector3(
                    Random.Range(-3f, 3f), 
                    0f, 
                    Random.Range(-3f, 3f)
                );
                Vector3 teleportPos = new Vector3(
                    targetTeleport.transform.position.x, 
                    other.transform.position.y, // Keep player's Y position 
                    targetTeleport.transform.position.z
                ) + randomOffset;
                
                other.transform.position = teleportPos;
                lastTeleportTime = Time.time;
                
                Debug.Log($"Player teleported to {teleportPos}");
                
                // Both teleports are consumed
                spawner.OnTeleportUsed(this.gameObject, targetTeleport);
            }
            else
            {
                Debug.LogWarning("No other teleports available for teleportation!");
                // Just consume this teleport if no target available
                spawner.OnPowerUpCollected(this.gameObject);
            }
        }
    }
}