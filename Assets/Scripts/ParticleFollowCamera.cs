using UnityEngine;

public class ParticleFollowCamera : MonoBehaviour
{
    public Camera targetCamera;
    public float distance = 5f;

    void LateUpdate()
    {
        if (targetCamera == null) targetCamera = Camera.main;

        // Place in front of the camera
        transform.position = targetCamera.transform.position + targetCamera.transform.forward * distance;

        // Face the same way as camera (optional)
        transform.rotation = targetCamera.transform.rotation;
    }
}