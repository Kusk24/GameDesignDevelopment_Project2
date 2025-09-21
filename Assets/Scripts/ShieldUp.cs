using UnityEngine;

public class ShieldUp : MonoBehaviour
{
    public float duration = 5f; // shield lasts this long
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

            PlayerMovement player = other.GetComponent<PlayerMovement>();
            if (player != null)
            {
                player.StartCoroutine(player.ActivateShield(duration));
            }

            FindFirstObjectByType<PowerUpSpawner>().OnPowerUpCollected(this.gameObject);
        }
    }
}
