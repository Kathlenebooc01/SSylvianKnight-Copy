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
            enemy.TakeDamage(damageAmount); // Deal Damage
            Destroy(gameObject); // Destroy the Arrow
        }
        // If we hit the ground (optional check), destroy arrow
        else if (other.gameObject.tag == "Ground")
        {
            Destroy(gameObject);
        }
    }
}