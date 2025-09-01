using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class PlayerMovement : MonoBehaviour
{
    public float moveSpeed = 5f;     // Forward/backward speed
    public float turnSpeed = 180f;   // Degrees per second
    private Rigidbody rb;

    public GameObject shellPrefab;        // CompleteShell prefab here
    public Transform shellSpawnPoint;     // ShellSpawnPoint here
    public float shellSpeed = 20f;        // How fast the shell moves

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
        // Instantiate the shell at the spawn point
        GameObject shell = Instantiate(shellPrefab, shellSpawnPoint.position, shellSpawnPoint.rotation);

        // Give it a forward velocity
        Rigidbody shellRb = shell.GetComponent<Rigidbody>();
        if (shellRb != null)
        {
            shellRb.linearVelocity = shellSpawnPoint.forward * shellSpeed;
        }

        // Optional: destroy shell after 5 seconds to avoid clutter
        Destroy(shell, 5f);
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
}