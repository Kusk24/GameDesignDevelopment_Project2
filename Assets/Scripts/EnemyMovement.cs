using UnityEngine;
using System.Collections;
using System;

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

    public static event Action OnEnemyDeath;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.isKinematic = true;
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
            transform.position = Vector3.MoveTowards(transform.position, targetPosition, moveSpeed * Time.deltaTime);

            if (Vector3.Distance(transform.position, targetPosition) < 0.001f)
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

        targetPosition = transform.position + moveDirection * tileSize;
        moving = true;
    }

    private void ChooseNewDirection()
    {
        Vector3[] dirs = { Vector3.forward, Vector3.back, Vector3.left, Vector3.right };

        for (int i = 0; i < 10; i++)
        {
            Vector3 dir = dirs[UnityEngine.Random.Range(0, dirs.Length)];
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
        Vector3 origin = transform.position + Vector3.up * 0.5f;
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
        shootTimer = UnityEngine.Random.Range(minShootTime, maxShootTime);
    }

    private void Shoot()
    {
        if (bulletPrefab != null && firePoint != null)
        {
            Vector3 spawnPos = firePoint.position + transform.forward * 0.2f;
            GameObject bullet = Instantiate(bulletPrefab, spawnPos, firePoint.rotation);

            Collider bulletCol = bullet.GetComponent<Collider>();
            if (bulletCol != null)
            {
                Physics.IgnoreCollision(bulletCol, col);
                StartCoroutine(ReenableCollision(bulletCol, col, 0.1f));
            }

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

    // ---------- NEW COLLISION TURNING ----------
    private void OnCollisionEnter(Collision collision)
    {
        // Only react if the collided object is on the obstacle layer(s)
        if (((1 << collision.gameObject.layer) & obstacleMask) != 0)
        {
            TurnNinetyDegrees();
            targetPosition = transform.position + moveDirection * tileSize;
            moving = true;
        }
    }

    private void TurnNinetyDegrees()
    {
        // pick left or right randomly
        float angle = UnityEngine.Random.value < 0.5f ? -90f : 90f;
        moveDirection = Quaternion.Euler(0f, angle, 0f) * moveDirection;
        transform.rotation = Quaternion.LookRotation(moveDirection, Vector3.up);
    }
    // -------------------------------------------

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

    void OnDestroy()
    {
        // Notify spawn manager when this enemy dies
        OnEnemyDeath?.Invoke();
    }
}