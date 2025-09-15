using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Rigidbody), typeof(Collider))]
public class EnemyMovement : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 3f;          // units per second
    public float tileSize = 1f;           // grid step size
    public LayerMask obstacleMask;        // assign both wall layers here

    [Header("Shooting Settings")]
    public GameObject bulletPrefab;       // assign your bullet prefab
    public Transform firePoint;           // empty child at tank barrel
    public float minShootTime = 1.5f;     // min seconds between shots
    public float maxShootTime = 3f;       // max seconds between shots
    public float bulletSpeed = 10f;       // bullet travel speed

    private Rigidbody rb;
    private Collider col;
    private Vector3 moveDirection;
    private Vector3 targetPosition;
    private bool moving = false;
    private float tankHalfSize = 0.5f;
    private float shootTimer;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.isKinematic = false; // Change to false for physics collisions
        rb.freezeRotation = true;

        col = GetComponent<Collider>();
        tankHalfSize = Mathf.Max(col.bounds.extents.x, col.bounds.extents.z);

        ChooseNewDirection();
        SetNextTarget();

        ResetShootTimer();
    }

    void Update()
    {
        HandleMovement();
        HandleShooting();
    }

    #region Movement
    private void HandleMovement()
    {
        if (moving)
        {
            // Use physics-based movement instead of direct transform manipulation
            Vector3 newPos = Vector3.MoveTowards(rb.position, targetPosition, moveSpeed * Time.deltaTime);
            rb.MovePosition(newPos);

            if (Vector3.Distance(rb.position, targetPosition) < 0.001f)
            {
                moving = false;
                SetNextTarget();
            }
        }
    }

    private void SetNextTarget()
    {
        if (IsBlocked(moveDirection))
        {
            ChooseNewDirection();
        }

        targetPosition = rb.position + moveDirection * tileSize;
        moving = true;
    }

    private void ChooseNewDirection()
    {
        Vector3[] dirs = { Vector3.forward, Vector3.back, Vector3.left, Vector3.right };

        for (int i = 0; i < 10; i++)
        {
            Vector3 dir = dirs[Random.Range(0, dirs.Length)];
            if (!IsBlocked(dir))
            {
                moveDirection = dir;
                transform.rotation = Quaternion.LookRotation(moveDirection, Vector3.up);
                return;
            }
        }

        moveDirection = Vector3.zero;
    }

    private bool IsBlocked(Vector3 dir)
    {
        Vector3 origin = rb.position + Vector3.up * 0.5f;
        float checkDist = tileSize + tankHalfSize;
        return Physics.Raycast(origin, dir, checkDist, obstacleMask, QueryTriggerInteraction.Collide);
    }
    #endregion

    #region Shooting
    private void HandleShooting()
    {
        shootTimer -= Time.deltaTime;
        if (shootTimer <= 0f)
        {
            Shoot();
            ResetShootTimer();
        }
    }

    private void ResetShootTimer()
    {
        shootTimer = Random.Range(minShootTime, maxShootTime);
    }

    private void Shoot()
    {
        if (bulletPrefab != null && firePoint != null)
        {
            // Spawn bullet slightly ahead of firePoint
            Vector3 spawnPos = firePoint.position + transform.forward * 0.2f;
            GameObject bullet = Instantiate(bulletPrefab, spawnPos, firePoint.rotation);

            // Ignore collision with this tank temporarily
            Collider bulletCol = bullet.GetComponent<Collider>();
            if (bulletCol != null)
            {
                Physics.IgnoreCollision(bulletCol, col);
                StartCoroutine(ReenableCollision(bulletCol, col, 0.1f));
            }

            // Make bullet move forward immediately
            Rigidbody bulletRb = bullet.GetComponent<Rigidbody>();
            if (bulletRb != null)
            {
                bulletRb.linearVelocity = transform.forward * bulletSpeed;
            }
        }
    }

    private IEnumerator ReenableCollision(Collider bulletCol, Collider tankCol, float delay)
    {
        yield return new WaitForSeconds(delay);
        if (bulletCol != null && tankCol != null)
        {
            Physics.IgnoreCollision(bulletCol, tankCol, false);
        }
    }
    #endregion

    private void OnDrawGizmosSelected()
    {
        if (moveDirection != Vector3.zero)
        {
            Gizmos.color = Color.red;
            Vector3 origin = transform.position + Vector3.up * 0.5f;
            float checkDist = tileSize + tankHalfSize;
            Gizmos.DrawLine(origin, origin + moveDirection * checkDist);
        }
    }
}