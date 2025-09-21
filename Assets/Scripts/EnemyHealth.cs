using UnityEngine;

public class EnemyHealth : MonoBehaviour
{
    [Header("Death Settings")]
    public GameObject explosionPrefab;   // Optional explosion effect
    public AudioClip deathSfx;           // Optional death sound
    
    [Header("Audio Settings")]
    [Range(0.1f, 5.0f)]
    public float deathSoundVolume = 0.7f;  // Reduced from 2.0f to 1.0f (50% reduction)
    public float maxDistance = 100f;       // How far the sound travels
    public bool use3DSound = true;         // 3D positional vs 2D omnipresent

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

        // Create a single temporary object for both VFX and SFX
        GameObject effectsObject = new GameObject("EnemyDeathEffects");
        effectsObject.transform.position = transform.position;
        effectsObject.transform.rotation = transform.rotation;

        // Spawn explosion VFX as child of effects object
        if (explosionPrefab != null)
        {
            GameObject explosion = Instantiate(explosionPrefab, effectsObject.transform);
            explosion.transform.localPosition = Vector3.zero;
            explosion.transform.localRotation = Quaternion.identity;
        }

        // Add amplified death sound - FIXED FOR CLEAR POV AUDIO
        if (deathSfx != null)
        {
            AudioSource src = effectsObject.AddComponent<AudioSource>();
            src.clip = deathSfx;
            src.volume = deathSoundVolume;              // Amplified volume
            src.spatialBlend = 0.2f;                    // Mostly 2D with slight spatial feel
            src.minDistance = 50f;                      // Very close before falloff starts
            src.maxDistance = 200f;                     // Long range before silent
            src.rolloffMode = AudioRolloffMode.Linear;  // Smooth falloff
            src.pitch = Random.Range(0.9f, 1.1f);      // Pitch variation
            src.playOnAwake = false;
            
            src.Play();
        }

        // Clean up the effects object after a reasonable time
        float destroyTime = Mathf.Max(
            deathSfx != null ? deathSfx.length + 0.5f : 0f,
            3f
        );
        
        Destroy(effectsObject, destroyTime);
        Destroy(gameObject);
    }
}