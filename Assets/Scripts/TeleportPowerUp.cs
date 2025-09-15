using UnityEngine;

public class TeleportPowerUp : MonoBehaviour
{
    private PowerUpSpawner spawner;
    private float lastTeleportTime = 0f;
    private float teleportCooldown = 1f; // 1 second cooldown

    void Start()
    {
        spawner = FindFirstObjectByType<PowerUpSpawner>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && Time.time > lastTeleportTime + teleportCooldown)
        {
            GameObject target = spawner.GetRandomOtherPowerUp(this.gameObject);
            if (target != null)
            {
                // Add random offset to avoid spawning directly on power-up
                Vector3 randomOffset = new Vector3(
                    Random.Range(-3f, 3f), 
                    0f, 
                    Random.Range(-3f, 3f)
                );
                Vector3 teleportPos = new Vector3(target.transform.position.x, 0f, target.transform.position.z) + randomOffset;
                other.transform.position = teleportPos;
                
                lastTeleportTime = Time.time;
            }

            spawner.OnPowerUpCollected(this.gameObject);
        }
    }
}