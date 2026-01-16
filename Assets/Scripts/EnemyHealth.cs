using UnityEngine;
using System.Collections;

public class EnemyHealth : MonoBehaviour
{
    [Header("Health Settings")]
    public int maxHealth = 50;
    private int currentHealth;
    
    [Header("Recoil Settings")]
    public float recoilForce = 10f;
    public float recoilDuration = 0.15f;
    private bool isRecoiling = false;

    // Property for the EnemyAI to read so it knows when to pause
    public bool IsRecoiling => isRecoiling;

    [Header("Death Settings")]
    public int flashCount = 4;
    public float flickerSpeed = 0.1f;
    public float fadeSpeed = 2f;
    public float timeBeforeFreeze = 0.5f; 

    private SpriteRenderer spriteRenderer;
    private Rigidbody2D rb;
    private Collider2D col;
    private Animator anim;
    private bool isDead = false;

    void Start()
    {
        currentHealth = maxHealth;
        spriteRenderer = GetComponent<SpriteRenderer>();
        anim = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();
        col = GetComponent<Collider2D>();
    }

    public void TakeDamage(int damage, Transform attacker)
    {
        if (isDead) return;
        
        currentHealth -= damage;
        
        if (anim != null)
        {
            anim.ResetTrigger("hit"); 
            anim.SetTrigger("hit");
        }
        
        StopCoroutine(nameof(FlashRed));
        StartCoroutine(nameof(FlashRed));

        // Start Recoil
        if (attacker != null) 
        {
            StopCoroutine(nameof(ApplyRecoil));
            StartCoroutine(ApplyRecoil(attacker));
        }

        if (currentHealth <= 0) Die();
    }

    IEnumerator ApplyRecoil(Transform attacker)
    {
        isRecoiling = true;
        
        // Calculate direction away from the attacker
        Vector2 knockbackDirection = (transform.position - attacker.position).normalized;
        
        if (rb != null)
        {
            // Set velocity for immediate backward movement
            rb.linearVelocity = new Vector2(knockbackDirection.x * recoilForce, rb.linearVelocity.y);
        }

        yield return new WaitForSeconds(recoilDuration);

        // Reset velocity so they don't slide forever
        if (rb != null && !isDead) 
        {
            rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
        }

        isRecoiling = false; // AI will start working again now
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
        StopAllCoroutines();
        
        if (anim != null) anim.SetTrigger("die");
        
        // Stop movement immediately on death
        if (rb != null) rb.linearVelocity = Vector2.zero;
        
        StartCoroutine(DeathSequence());
    }

    IEnumerator DeathSequence()
    {
        yield return new WaitForSeconds(timeBeforeFreeze);
        if (rb != null) rb.simulated = false; 
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