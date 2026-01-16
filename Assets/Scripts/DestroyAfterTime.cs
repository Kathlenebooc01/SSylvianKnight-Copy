using UnityEngine;

public class DestroyAfterTime : MonoBehaviour
{
    public float lifetime = 3f;

    void Start()
    {
        // This command tells Unity to delete the object after 3 seconds
        Destroy(gameObject, lifetime);
    }
}