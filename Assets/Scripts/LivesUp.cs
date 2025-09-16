using UnityEngine;

public class LivesUp : MonoBehaviour
{
    public int livesToAdd = 1;  // How many lives to give (default 1)

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
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
