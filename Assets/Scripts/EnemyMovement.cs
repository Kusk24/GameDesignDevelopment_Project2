using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class EnemyMovement : MonoBehaviour
{
    public float moveSpeed = 3f;     // Movement speed
    private Rigidbody rb;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        // Start moving forward in current facing
    }

    private void FixedUpdate()
    {
        // Move forward constantly
        Vector3 move = transform.forward * moveSpeed * Time.fixedDeltaTime;
        rb.MovePosition(rb.position + move);
    }

    private void OnCollisionEnter(Collision collision)
    {
        // Turn 90 degrees randomly left or right when hitting anything
        float turnAngle = (Random.value < 0.5f) ? 90f : -90f;
        rb.rotation = Quaternion.Euler(0f, rb.rotation.eulerAngles.y + turnAngle, 0f);
    }
}