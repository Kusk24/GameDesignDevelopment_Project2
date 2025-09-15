using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Rigidbody))]
public class PlayerMovement : MonoBehaviour
{
    public float moveSpeed = 5f;     // Forward/backward speed
    public float turnSpeed = 180f;   // Degrees per second
    private Rigidbody rb;

    public GameObject shellPrefab;        // CompleteShell prefab here
    public Transform shellSpawnPoint;     // ShellSpawnPoint here
    public float shellSpeed = 20f;        // How fast the shell moves

    private bool isTripleShot = false;
    public bool shieldActive = false; // public so ShellExplosion can check it
    public GameObject shieldVisual; // optional: a glowing sphere/mesh around player

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            Shoot();
        }
    }

    private void Shoot()
    {
        if (isTripleShot)
        {
            // Triple shot: center, left, right
            ShootBullet(shellSpawnPoint.position, shellSpawnPoint.rotation);
            
            // Left shot (15 degrees left)
            Quaternion leftRotation = shellSpawnPoint.rotation * Quaternion.Euler(0, -15, 0);
            ShootBullet(shellSpawnPoint.position, leftRotation);
            
            // Right shot (15 degrees right)
            Quaternion rightRotation = shellSpawnPoint.rotation * Quaternion.Euler(0, 15, 0);
            ShootBullet(shellSpawnPoint.position, rightRotation);
        }
        else
        {
            // Normal single shot
            ShootBullet(shellSpawnPoint.position, shellSpawnPoint.rotation);
        }
    }

    private void ShootBullet(Vector3 position, Quaternion rotation)
    {
        // Spawn shell further out to avoid overlap with tank collider
        Vector3 spawnPos = position + rotation * Vector3.forward * 1.5f;
        GameObject shell = Instantiate(shellPrefab, spawnPos, rotation);

        // Ignore collision with ALL colliders on this tank
        Collider shellCol = shell.GetComponent<Collider>();
        Collider[] tankCols = GetComponentsInChildren<Collider>();
        if (shellCol != null)
        {
            foreach (var tc in tankCols)
            {
                if (tc != null)
                {
                    Physics.IgnoreCollision(shellCol, tc);
                }
            }
            StartCoroutine(ReenableCollisions(shellCol, tankCols, 0.2f));
        }

        // Give it a forward velocity
        Rigidbody shellRb = shell.GetComponent<Rigidbody>();
        if (shellRb != null)
        {
            shellRb.linearVelocity = rotation * Vector3.forward * shellSpeed;
        }

        // Optional: destroy shell after 5 seconds to avoid clutter
        Destroy(shell, 5f);
    }

    private System.Collections.IEnumerator ReenableCollisions(Collider shellCol, Collider[] tankCols, float delay)
    {
        yield return new WaitForSeconds(delay);
        if (shellCol != null)
        {
            foreach (var tc in tankCols)
            {
                if (tc != null)
                {
                    Physics.IgnoreCollision(shellCol, tc, false);
                }
            }
        }
    }

    public System.Collections.IEnumerator TripleShot(float duration)
    {
        isTripleShot = true;
        yield return new WaitForSeconds(duration);
        isTripleShot = false;
    }

    public System.Collections.IEnumerator ActivateShield(float duration)
    {
        shieldActive = true;

        if (shieldVisual != null)
            shieldVisual.SetActive(true);

        yield return new WaitForSeconds(duration);

        shieldActive = false;

        if (shieldVisual != null)
            shieldVisual.SetActive(false);
    }

    private void FixedUpdate()
    {
        // Forward/backward input (W/S or Up/Down)
        float moveInput = Input.GetAxis("Vertical");

        // Left/right input (A/D or Left/Right)
        float turnInput = Input.GetAxis("Horizontal");

        // Move the player forward in the direction they are facing
        Vector3 move = transform.forward * moveInput * moveSpeed * Time.fixedDeltaTime;
        rb.MovePosition(rb.position + move);

        // Rotate the player left/right
        float turn = turnInput * turnSpeed * Time.fixedDeltaTime;
        Quaternion turnRotation = Quaternion.Euler(0f, turn, 0f);
        rb.MoveRotation(rb.rotation * turnRotation);
    }

    public System.Collections.IEnumerator SpeedBoost(float boostAmount, float duration)
{
    moveSpeed *= boostAmount;
    yield return new WaitForSeconds(duration);
    moveSpeed /= boostAmount;
}

}