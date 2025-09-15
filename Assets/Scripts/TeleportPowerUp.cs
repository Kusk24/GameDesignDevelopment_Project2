using UnityEngine;

public class TeleportPowerUp : MonoBehaviour
{
    private PowerUpSpawner spawner;

    void Start()
    {
        spawner = FindFirstObjectByType<PowerUpSpawner>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player")) // your tank should have this tag
        {
            // find another powerup to teleport to
            GameObject target = spawner.GetRandomOtherPowerUp(this.gameObject);
            if (target != null)
            {
                other.transform.position = target.transform.position + Vector3.up * 1f; 
            }

            // notify spawner
            spawner.OnPowerUpCollected(this.gameObject);
        }
    }
}
