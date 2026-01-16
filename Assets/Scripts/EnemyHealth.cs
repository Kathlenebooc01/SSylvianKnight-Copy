using UnityEngine;
using System.Collections;

public class EnemyHealth : MonoBehaviour
{
    [Header("Health Settings")]
    public int maxHealth = 50;
    private int currentHealth;
    
    [Header("Death Settings")]
    public int flashCount = 4;
    public float flickerSpeed = 0.1f;
    public float fadeSpeed = 2f;
    public float timeBeforeFreeze = 0.5f; 

    private SpriteRenderer spriteRenderer;
    private Animator anim;
    private Rigidbody2D rb;
    private Collider2D col;
    private bool isDead = false;

    void Start()
    {
        currentHealth = maxHealth;
        spriteRenderer = GetComponent<SpriteRenderer>();
        anim = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();
        col = GetComponent<Collider2D>();
    }

    public void TakeDamage(int damage)
    {
        if (isDead) return;
        
        currentHealth -= damage;
        
        // TRIGGER THE HIT ANIMATION
        if (anim != null)
        {
            anim.ResetTrigger("hit"); 
            anim.SetTrigger("hit");
        }
        
        // Start the red flash visual
        StopCoroutine(nameof(FlashRed));
        StartCoroutine(nameof(FlashRed));

        if (currentHealth <= 0) 
        {
            Die();
        }
    }

    IEnumerator FlashRed()
    {
        if (spriteRenderer != null && !isDead)
        {
            spriteRenderer.color = Color.red;
            yield return new WaitForSeconds(0.1f);
            if(!isDead) spriteRenderer.color = Color.white;
        }
    }

    void Die()
    {
        if (isDead) return;
        isDead = true;

        // Stop all hit effects/flashes
        StopAllCoroutines();

        if (anim != null) anim.SetTrigger("die");

        // ONLY disable the AI script when dead
        // Ensure your AI script is actually named 'EnemyAI'
        MonoBehaviour aiScript = GetComponent<EnemyAI>() as MonoBehaviour;
        if (aiScript != null) aiScript.enabled = false;

        if (rb != null)
        {
            rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
        }

        StartCoroutine(DeathSequence());
    }

    IEnumerator DeathSequence()
    {
        yield return new WaitForSeconds(timeBeforeFreeze);

        if (rb != null) 
        {
            rb.linearVelocity = Vector2.zero;
            rb.simulated = false; 
        }
        if (col != null) col.enabled = false; 

        for (int i = 0; i < flashCount; i++)
        {
            spriteRenderer.color = Color.white;
            yield return new WaitForSeconds(flickerSpeed);
            spriteRenderer.color = new Color(1, 1, 1, 0.5f); 
            yield return new WaitForSeconds(flickerSpeed);
        }

        float alpha = spriteRenderer.color.a;
        while (alpha > 0)
        {
            alpha -= Time.deltaTime * fadeSpeed;
            spriteRenderer.color = new Color(1, 1, 1, alpha);
            yield return null; 
        }

        Destroy(gameObject);
    }
}