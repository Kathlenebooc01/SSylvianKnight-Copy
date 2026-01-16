using UnityEngine;

public class ArrowDamage : MonoBehaviour
{
    public int damageAmount = 10;

    void OnTriggerEnter2D(Collider2D other)
    {
        // Check if we hit the Enemy
        EnemyHealth enemy = other.GetComponent<EnemyHealth>();

        if (enemy != null)
        {
            // FIX: Added 'transform' as the second argument
            enemy.TakeDamage(damageAmount, transform); 
            Destroy(gameObject); // Destroy the Arrow
        }
        // If we hit the ground, destroy arrow
        else if (other.gameObject.CompareTag("Ground"))
        {
            Destroy(gameObject);
        }
    }
}