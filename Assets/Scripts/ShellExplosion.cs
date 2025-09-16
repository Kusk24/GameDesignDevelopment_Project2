using UnityEngine;

namespace Tanks.Complete
{
    public class ShellExplosion : MonoBehaviour
    {
        [Header("Explosion Settings")]
        public LayerMask m_TankMask;                 // Layers affected by explosion
        public ParticleSystem m_ExplosionParticles;  // Explosion particles
        public AudioSource m_ExplosionAudio;         // Explosion sound
        [HideInInspector] public float m_MaxLifeTime = 2f;   // Shell lifetime
        [HideInInspector] public float m_MaxDamage = 100f;   // Max damage for area damage
        [HideInInspector] public float m_ExplosionForce = 50f;
        [HideInInspector] public float m_ExplosionRadius = 5f;

        private void Start()
        {
            // Destroy the shell after its max lifetime just in case
            Destroy(gameObject, m_MaxLifeTime);
        }

        private void OnTriggerEnter(Collider other)
        {
            // --- Destroy destructible walls ---
            if (other.CompareTag("Wall 1"))
            {
                Destroy(other.gameObject);
            }

            // --- Handle direct hit on PLAYER ---
            if (other.CompareTag("Player"))
            {
                // check for shield on the player movement (if you still use that)
                PlayerMovement pm = other.GetComponent<PlayerMovement>();
                if (pm != null && pm.shieldActive)
                {
                    PlayExplosionEffects();
                    Destroy(gameObject);
                    return;
                }

                // Apply 1 point of damage to the PlayerHealth script
                PlayerHealth playerHealth = other.GetComponent<PlayerHealth>();
                if (playerHealth != null)
                {
                    playerHealth.TakeDamage(1);
                }
            }

            // --- Handle direct hit on ENEMY ---
            if (other.CompareTag("Enemy"))
            {
                EnemyHealth enemyHealth = other.GetComponent<EnemyHealth>();
                if (enemyHealth != null)
                {
                    // One hit kill
                    enemyHealth.TakeDamage(1);
                }
            }

            // --- Optional splash damage to any tanks within radius ---
            Collider[] colliders = Physics.OverlapSphere(transform.position, m_ExplosionRadius, m_TankMask);
            for (int i = 0; i < colliders.Length; i++)
            {
                Rigidbody targetRigidbody = colliders[i].GetComponent<Rigidbody>();
                if (!targetRigidbody) continue;

                TankMovement tankMove = targetRigidbody.GetComponent<TankMovement>();
                if (tankMove != null)
                {
                    tankMove.AddExplosionForce(m_ExplosionForce, transform.position, m_ExplosionRadius);
                }

                // If you still want area damage for tanks with old TankHealth, keep this
                TankHealth targetHealth = targetRigidbody.GetComponent<TankHealth>();
                if (targetHealth != null)
                {
                    float damage = CalculateDamage(targetRigidbody.position);
                    targetHealth.TakeDamage(damage);
                }
            }

            // Play explosion VFX/SFX and destroy shell
            PlayExplosionEffects();
            Destroy(gameObject);
        }

        private void PlayExplosionEffects()
        {
            if (m_ExplosionParticles != null)
            {
                m_ExplosionParticles.transform.parent = null;
                m_ExplosionParticles.Play();
                ParticleSystem.MainModule mainModule = m_ExplosionParticles.main;
                Destroy(m_ExplosionParticles.gameObject, mainModule.duration);
            }

            if (m_ExplosionAudio != null)
            {
                m_ExplosionAudio.Play();
            }
        }

        private float CalculateDamage(Vector3 targetPosition)
        {
            Vector3 explosionToTarget = targetPosition - transform.position;
            float explosionDistance = explosionToTarget.magnitude;
            float relativeDistance = (m_ExplosionRadius - explosionDistance) / m_ExplosionRadius;
            float damage = relativeDistance * m_MaxDamage;
            return Mathf.Max(0f, damage);
        }
    }
}