using UnityEngine;
using TMPro;
using System.Collections;

public class PlayerHealth : MonoBehaviour
{
    [Header("Health Settings")]
    public int startingLives = 3;                 // player starts with 3 lives
    public GameObject explosionPrefab;            // explosion effect on death
    public AudioClip deathSfx;                    // optional death sound
    public TMP_Text livesText;                    // TextMeshProUGUI for the UI label

    private int currentLives;
    private bool dead;
    private PlayerMovement playerMovement;

    private void OnEnable()
    {
        currentLives = startingLives;
        dead = false;
        playerMovement = GetComponent<PlayerMovement>();
        UpdateUI();

        // Start checking shield status
        StartCoroutine(UpdateShieldUI());
    }

    /// <summary>
    /// Call this when the player is hit by a bullet.
    public void TakeDamage(int amount = 1)
    {
        if (dead) return;

        currentLives -= amount;
        UpdateUI();

        if (currentLives <= 0)
        {
            Die();
            return;
        }

        // else: you might have invuln frames/respawn logic here
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

    // in PlayerHealth.cs
    private void Die()
    {
        if (dead) return;
        dead = true;

        // Stop any engine / looping sounds on the player
        foreach (var src in GetComponentsInChildren<AudioSource>())
            src.Stop();

        // Death SFX (one-shot at player position)
        if (deathSfx)
            AudioSource.PlayClipAtPoint(deathSfx, transform.position);

        // Death VFX
        if (explosionPrefab)
            Instantiate(explosionPrefab, transform.position, transform.rotation);

        // *** Hand off to central Game Over ***
        if (GameManager.Instance != null)
            GameManager.Instance.TriggerGameOver("Lives depleted");

        // Remove the player object from the scene
        Destroy(gameObject);
    }

    private void UpdateUI()
    {
        if (livesText && playerMovement)
        {
            if (playerMovement.shieldActive)
            {
                // Calculate remaining shield time
                float remainingTime = playerMovement.GetShieldRemainingTime();
                livesText.text = $"Lives: {currentLives} (Shield: {remainingTime:F1}s)";
            }
            else
            {
                livesText.text = $"Lives: {currentLives}";
            }
        }
    }

    private IEnumerator UpdateShieldUI()
    {
        while (!dead)
        {
            UpdateUI();
            yield return new WaitForSeconds(0.1f); // Update 10 times per second
        }
    }
}