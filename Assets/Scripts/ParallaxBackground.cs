using UnityEngine;

public class ParallaxBackground : MonoBehaviour
{
    [Header("Settings")]
    [Tooltip("1.0 = Sky (Still), 0.5 = Mid, 0.0 = Ground, Negative = Foreground")]
    public float parallaxFactor;

    private GameObject cam;
    private float startPos;
    private float startY;

    void Start()
    {
        cam = Camera.main.gameObject;
        startPos = transform.position.x;
        startY = transform.position.y;
    }

    // LateUpdate moves the background AFTER the camera has settled for the frame
    void LateUpdate()
    {
        float dist = (cam.transform.position.x * parallaxFactor);

        // This locks the Y position so backgrounds don't jump when you jump
        transform.position = new Vector3(startPos + dist, startY, transform.position.z);
    }
}