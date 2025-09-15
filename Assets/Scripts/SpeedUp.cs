using UnityEngine;

public class SpeedUp : MonoBehaviour
{
    public float boostAmount = 2f;   // how much faster
    public float duration = 5f;      // how long boost lasts

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            PlayerMovement player = other.GetComponent<PlayerMovement>();
            if (player != null)
            {
                player.StartCoroutine(player.SpeedBoost(boostAmount, duration));
            }

            FindFirstObjectByType<PowerUpSpawner>().OnPowerUpCollected(this.gameObject);
        }
    }
}
