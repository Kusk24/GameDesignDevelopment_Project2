using UnityEngine;

public class LivesUp : MonoBehaviour
{
    public int livesToAdd = 1;  // How many lives to give (default 1)
    public AudioClip pickupSfx; // Power-up collection sound (assign same clip to all power-ups)

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            // Play uniform pickup sound
            if (pickupSfx != null)
            {
                GameObject tempAudio = new GameObject("PowerUpPickupSFX");
                AudioSource src = tempAudio.AddComponent<AudioSource>();
                src.clip = pickupSfx;
                src.volume = 1.0f;          // Uniform volume
                src.spatialBlend = 0f;      // 2D sound
                src.pitch = 1.0f;          // Uniform pitch - no variation
                src.Play();
                Destroy(tempAudio, pickupSfx.length + 0.1f);
            }

            PlayerHealth playerHealth = other.GetComponent<PlayerHealth>();
            if (playerHealth != null)
            {
                playerHealth.AddHealth(livesToAdd);
                Debug.Log($"Player gained {livesToAdd} life! Lives up power-up collected.");
            }

            // Notify spawner that this power-up was collected
            PowerUpSpawner spawner = FindFirstObjectByType<PowerUpSpawner>();
            if (spawner != null)
            {
                spawner.OnPowerUpCollected(this.gameObject);
            }
        }
    }
}
