using UnityEngine;

public class ShootUp : MonoBehaviour
{
    public float duration = 5f;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            PlayerMovement player = other.GetComponent<PlayerMovement>();
            if (player != null)
            {
                player.StartCoroutine(player.TripleShot(duration));
            }

            FindFirstObjectByType<PowerUpSpawner>().OnPowerUpCollected(this.gameObject);
        }
    }
}
