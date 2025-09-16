using UnityEngine;
using TMPro;   // <-- use TextMeshPro types

public class PlayerHealth : MonoBehaviour
{
    [Header("Health Settings")]
    public int startingLives = 3;                 // player starts with 3 lives
    public GameObject explosionPrefab;            // explosion effect on death
    public AudioClip deathSfx;                    // optional death sound
    public TMP_Text livesText;                    // TextMeshProUGUI for the UI label

    private int currentLives;
    private bool dead;

    private void OnEnable()
    {
        currentLives = startingLives;
        dead = false;
        UpdateUI();
    }

    /// <summary>
    /// Call this when the player is hit by a bullet.
    /// </summary>
    public void TakeDamage(int amount = 1)
    {
        if (dead) return;

        currentLives -= amount;
        UpdateUI();

        if (currentLives <= 0)
        {
            Die();
        }
    }

    /// <summary>
    /// For health or extra-life power-ups.
    /// </summary>
    public void AddHealth(int amount = 1)
    {
        if (dead) return;
        currentLives += amount;
        UpdateUI();
    }

    private void Die()
    {
        dead = true;

        // Stop ALL engine / looping sounds on the player
        foreach (AudioSource src in GetComponentsInChildren<AudioSource>())
        {
            src.Stop();
        }

        // Optional: play a death SFX once at the player position
        if (deathSfx)
            AudioSource.PlayClipAtPoint(deathSfx, transform.position);

        // Optional: spawn explosion VFX
        if (explosionPrefab)
            Instantiate(explosionPrefab, transform.position, transform.rotation);

        // Here you can trigger Game Over UI or respawn logic
        // For now, just destroy the player tank
        Destroy(gameObject);
    }

    private void UpdateUI()
    {
        if (livesText)
            livesText.text = $"Lives: {currentLives}";
    }
}