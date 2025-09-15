using UnityEngine;

public class EnemyHealth : MonoBehaviour
{
    [Header("Death Settings")]
    public GameObject explosionPrefab;   // Optional explosion effect
    public AudioClip deathSfx;           // Optional death sound

    private bool dead = false;

    /// <summary>
    /// Call this when the enemy is hit by a bullet.
    /// </summary>
    public void TakeDamage(float damage)
    {
        if (dead) return;
        Die();
    }

    private void Die()
    {
        dead = true;

        // Stop any engine or looping sounds
        foreach (AudioSource src in GetComponentsInChildren<AudioSource>())
            src.Stop();

        // Play a one-shot death sound (independent of this object)
        if (deathSfx)
            AudioSource.PlayClipAtPoint(deathSfx, transform.position);

        // Spawn explosion VFX if assigned
        if (explosionPrefab)
            Instantiate(explosionPrefab, transform.position, transform.rotation);

        // Destroy the enemy tank object and everything under it
        Destroy(gameObject);
    }
}