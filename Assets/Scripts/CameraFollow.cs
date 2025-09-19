using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Transform target;      
    public Vector3 offset;        // Position offset from the player
    public float smoothSpeed = 0.125f;

    void LateUpdate()
    {
        if (target == null) return;

        // Desired camera position
        Vector3 desiredPosition = target.position + offset;

        // Smooth movement
        Vector3 smoothedPosition = Vector3.Lerp(transform.position,
                                                desiredPosition,
                                                smoothSpeed);

        transform.position = smoothedPosition;

        // Keep camera looking at the player
        transform.LookAt(target);
    }
}