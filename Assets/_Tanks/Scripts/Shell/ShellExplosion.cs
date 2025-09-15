using UnityEngine;

namespace Tanks.Complete
{
    public class ShellExplosion : MonoBehaviour
    {
        [Header("Explosion Settings")]
        public LayerMask m_TankMask;                        // Layers affected by explosion
        public ParticleSystem m_ExplosionParticles;         // Explosion particles
        public AudioSource m_ExplosionAudio;                // Explosion sound
        [HideInInspector] public float m_MaxLifeTime = 2f;  // Shell lifetime
        [HideInInspector] public float m_MaxDamage = 100f; // Max damage
        [HideInInspector] public float m_ExplosionForce = 50f; // Force applied to tanks
        [HideInInspector] public float m_ExplosionRadius = 5f; // Explosion radius

        private void Start()
        {
            // Destroy shell after its max lifetime
            Destroy(gameObject, m_MaxLifeTime);
        }

        private void OnTriggerEnter(Collider other)
        {
            // --- Destroy walls tagged "Wall 1" ---
            if (other.CompareTag("Wall 1"))
            {
                Destroy(other.gameObject);
            }

            // --- Check if the shell hits the player or enemy directly ---
            if (other.CompareTag("Player"))
            {
                PlayerMovement pm = other.GetComponent<PlayerMovement>();
                if (pm != null && pm.shieldActive)
                {
                    // Shield absorbs the bullet, just destroy the shell
                    // Play explosion effects but don't harm player
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
                    
                    Destroy(gameObject);
                    return;
                }

                // No shield -> destroy player
                Destroy(other.gameObject);
            }

            // --- Optional: handle enemy tanks ---
            if (other.CompareTag("Enemy"))
            {
                TankHealth targetHealth = other.GetComponent<TankHealth>();
                if (targetHealth != null)
                {
                    float damage = CalculateDamage(other.transform.position);
                    targetHealth.TakeDamage(damage);
                }
            }

            // --- Handle explosion effects for all tanks in radius ---
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

                TankHealth targetHealth = targetRigidbody.GetComponent<TankHealth>();
                if (targetHealth != null)
                {
                    float damage = CalculateDamage(targetRigidbody.position);
                    targetHealth.TakeDamage(damage);
                }
            }

            // --- Play explosion particles and audio ---
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

            // Destroy the shell itself
            Destroy(gameObject);
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