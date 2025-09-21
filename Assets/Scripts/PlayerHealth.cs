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

        // FIXED PLAYER DEATH AUDIO - Reduced volume for new background music
        if (deathSfx)
        {
            GameObject tempAudio = new GameObject("PlayerDeathSFX");
            tempAudio.transform.position = transform.position;
            AudioSource src = tempAudio.AddComponent<AudioSource>();
            
            src.clip = deathSfx;
            src.volume = 1.0f;                          // Reduced from 2.0f to 1.0f (50% reduction)
            src.spatialBlend = 0f;                      // Fully 2D - always clear
            src.pitch = Random.Range(0.9f, 1.1f);
            
            src.Play();
            Destroy(tempAudio, deathSfx.length + 0.2f);
        }

        // Optional: spawn explosion VFX
        if (explosionPrefab)
            Instantiate(explosionPrefab, transform.position, transform.rotation);

        // Here you can trigger Game Over UI or respawn logic
        // For now, just destroy the player tank
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