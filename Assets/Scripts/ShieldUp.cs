using UnityEngine;

public class ShieldUp : MonoBehaviour
{
    public float duration = 5f; // shield lasts this long

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            PlayerMovement player = other.GetComponent<PlayerMovement>();
            if (player != null)
            {
                player.StartCoroutine(player.ActivateShield(duration));
            }

            FindFirstObjectByType<PowerUpSpawner>().OnPowerUpCollected(this.gameObject);
        }
    }
}
