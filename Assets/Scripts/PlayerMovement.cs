using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Rigidbody))]
public class PlayerMovement : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 5f;     // Forward/backward speed
    public float turnSpeed = 180f;   // Degrees per second

    [Header("Audio")]
    public AudioSource engineAudio;  // Drag your EngineIdle AudioSource here in the Inspector

    [Header("Shooting")]
    public GameObject shellPrefab;        // CompleteShell prefab
    public Transform shellSpawnPoint;     // ShellSpawnPoint
    public float shellSpeed = 20f;

    private Rigidbody rb;
    private bool isTripleShot = false;
    public bool shieldActive = false;
    public GameObject shieldVisual;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    private void Update()
    {
        // --- Shooting ---
        if (Input.GetKeyDown(KeyCode.Space))
        {
            Shoot();
        }

        // --- Engine sound control ---
        float moveInput = Input.GetAxis("Vertical");
        float turnInput = Input.GetAxis("Horizontal");
        bool isMoving = Mathf.Abs(moveInput) > 0.01f || Mathf.Abs(turnInput) > 0.01f;

        if (engineAudio)
        {
            if (isMoving && !engineAudio.isPlaying)
            {
                engineAudio.Play();
            }
            else if (!isMoving && engineAudio.isPlaying)
            {
                engineAudio.Stop();
            }
        }
    }

    private void Shoot()
    {
        if (isTripleShot)
        {
            ShootBullet(shellSpawnPoint.position, shellSpawnPoint.rotation);

            Quaternion leftRotation = shellSpawnPoint.rotation * Quaternion.Euler(0, -15, 0);
            ShootBullet(shellSpawnPoint.position, leftRotation);

            Quaternion rightRotation = shellSpawnPoint.rotation * Quaternion.Euler(0, 15, 0);
            ShootBullet(shellSpawnPoint.position, rightRotation);
        }
        else
        {
            ShootBullet(shellSpawnPoint.position, shellSpawnPoint.rotation);
        }
    }

    private void ShootBullet(Vector3 position, Quaternion rotation)
    {
        Vector3 spawnPos = position + rotation * Vector3.forward * 1.5f;
        GameObject shell = Instantiate(shellPrefab, spawnPos, rotation);

        Collider shellCol = shell.GetComponent<Collider>();
        Collider[] tankCols = GetComponentsInChildren<Collider>();
        if (shellCol != null)
        {
            foreach (var tc in tankCols)
                if (tc != null)
                    Physics.IgnoreCollision(shellCol, tc);
            StartCoroutine(ReenableCollisions(shellCol, tankCols, 0.2f));
        }

        Rigidbody shellRb = shell.GetComponent<Rigidbody>();
        if (shellRb != null)
        {
            shellRb.linearVelocity = rotation * Vector3.forward * shellSpeed;
        }

        Destroy(shell, 5f);
    }

    private IEnumerator ReenableCollisions(Collider shellCol, Collider[] tankCols, float delay)
    {
        yield return new WaitForSeconds(delay);
        if (shellCol != null)
        {
            foreach (var tc in tankCols)
                if (tc != null)
                    Physics.IgnoreCollision(shellCol, tc, false);
        }
    }

    public IEnumerator TripleShot(float duration)
    {
        isTripleShot = true;
        yield return new WaitForSeconds(duration);
        isTripleShot = false;
    }

    public IEnumerator ActivateShield(float duration)
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
        float moveInput = Input.GetAxis("Vertical");
        float turnInput = Input.GetAxis("Horizontal");

        Vector3 move = transform.forward * moveInput * moveSpeed * Time.fixedDeltaTime;
        rb.MovePosition(rb.position + move);

        float turn = turnInput * turnSpeed * Time.fixedDeltaTime;
        Quaternion turnRotation = Quaternion.Euler(0f, turn, 0f);
        rb.MoveRotation(rb.rotation * turnRotation);
    }

    public IEnumerator SpeedBoost(float boostAmount, float duration)
    {
        moveSpeed *= boostAmount;
        yield return new WaitForSeconds(duration);
        moveSpeed /= boostAmount;
    }
}