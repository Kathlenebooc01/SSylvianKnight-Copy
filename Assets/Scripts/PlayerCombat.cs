using UnityEngine;

public class ProceduralVisuals : MonoBehaviour
{
    [Header("Settings")]
    public Rigidbody2D rb; // Or Reference your Movement Script
    public Transform visualMesh; // Drag the child object with the MeshRenderer here

    [Header("Idle 'Breathing' Settings")]
    public float breatheSpeed = 2f;
    public float breatheAmount = 0.05f;

    [Header("Movement Tilt")]
    public float tiltAmount = 5f;
    public float smoothTime = 0.1f;

    private Vector3 originalScale;
    private Quaternion originalRotation;
    private Vector3 currentVelocity; // For smoothing

    void Start()
    {
        if (visualMesh == null) visualMesh = transform;
        originalScale = visualMesh.localScale;
        originalRotation = visualMesh.localRotation;
    }

    void Update()
    {
        // 1. Determine if we are IDLE or MOVING
        bool isMoving = rb.linearVelocity.magnitude > 0.1f;

        if (isMoving)
        {
            HandleMovingVisuals();
        }
        else
        {
            HandleIdleVisuals();
        }
    }

    // THE "CODED" IDLE
    // Instead of an animation clip, we use Math.Sin to scale the object up and down
    void HandleIdleVisuals()
    {
        // Reset rotation smoothly
        visualMesh.localRotation = Quaternion.Slerp(visualMesh.localRotation, originalRotation, Time.deltaTime * 10f);

        // Calculate the "Breathing" factor based on Time
        float yChange = Mathf.Sin(Time.time * breatheSpeed) * breatheAmount;

        // Apply to Scale (Preserving the Center pivot logic)
        // Note: For this to look like it's grounded, the parent pivot should be at the feet, 
        // OR we offset the position slightly if the pivot is Center.
        Vector3 targetScale = originalScale + new Vector3(-yChange, yChange, 0);

        visualMesh.localScale = Vector3.Lerp(visualMesh.localScale, targetScale, Time.deltaTime * 5f);
    }

    // THE "CODED" RUN
    // Simple tilt in the direction of movement
    void HandleMovingVisuals()
    {
        // Reset scale smoothly
        visualMesh.localScale = Vector3.Lerp(visualMesh.localScale, originalScale, Time.deltaTime * 10f);

        // Calculate Tilt
        float targetZ = -rb.linearVelocity.x * tiltAmount;
        Quaternion targetRot = Quaternion.Euler(0, 0, targetZ);

        visualMesh.localRotation = Quaternion.Slerp(visualMesh.localRotation, targetRot, smoothTime);
    }
}