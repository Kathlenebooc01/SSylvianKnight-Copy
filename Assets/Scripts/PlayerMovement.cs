using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [Header("Stats")]
    public bool isControlLocked = false;
    public float moveSpeed = 8f;
    public float jumpForce = 15f;
    public int maxHealth = 100;

    [Header("Soul System")]
    public int maxSoul = 99;
    public int currentSoul = 0;
    public int soulGainPerHit = 11;
    public int soulCostPerSpell = 33;

    [Header("Healing Settings")]
    public float timeToHeal = 1.5f;
    private float healTimer = 0f;
    private bool isHealing = false;

    [Header("Recoil Settings")]
    [SerializeField] private float recoilDuration = 0.15f;
    private float recoilTimer;
    private bool isRecoiling;

    [Header("Components")]
    public Transform groundCheck;
    public LayerMask groundLayer;

    private Rigidbody2D rb;
    private Animator anim;
    private bool isGrounded;
    private int currentHealth;
    private bool isDead = false;
    private bool isAnimationLocked = false;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        currentHealth = maxHealth;
    }

    public bool IsGroundedState() => isGrounded;
    public int GetCurrentHealth() => currentHealth;
    public int GetCurrentSoul() => currentSoul;

    void Update()
    {
        if (isDead) { rb.linearVelocity = Vector2.zero; return; }

        if (isRecoiling)
        {
            recoilTimer -= Time.deltaTime;
            if (recoilTimer <= 0) isRecoiling = false;
            else return;
        }

        if (isControlLocked)
        {
            rb.linearVelocity = Vector2.zero;
            anim.SetBool("IsRunning", false);
            return;
        }

        isGrounded = Physics2D.OverlapCircle(groundCheck.position, 0.2f, groundLayer);

        // HEALING
        if (Input.GetKey(KeyCode.L) && isGrounded && currentSoul >= soulCostPerSpell && currentHealth < maxHealth)
        {
            rb.linearVelocity = Vector2.zero;
            anim.SetBool("IsHealing", true);
            isHealing = true;

            healTimer += Time.deltaTime;

            if (healTimer >= timeToHeal)
            {
                PerformHeal(20);
                healTimer = 0f;
            }
            return;
        }
        else
        {
            anim.SetBool("IsHealing", false);
            isHealing = false;
            healTimer = 0f;
        }

        float moveInput = Input.GetAxisRaw("Horizontal");
        if (Input.GetKeyDown(KeyCode.H)) TakeDamage(20);

        // --- FIXED FACING LOGIC ---
        // Now checks !isAnimationLocked before flipping
        if (moveInput != 0 && !isHealing && !isAnimationLocked)
        {
            SetFacingDirection(moveInput);
        }

        // PHYSICS
        if (!isHealing)
        {
            rb.linearVelocity = new Vector2(moveInput * moveSpeed, rb.linearVelocity.y);
        }

        if (Input.GetButtonDown("Jump") && isGrounded && !isHealing)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
        }

        // ANIMATION
        if (isAnimationLocked)
        {
            anim.SetBool("IsGrounded", true);
            anim.SetFloat("VerticalSpeed", 0);
            anim.SetFloat("Speed", 0);
        }
        else
        {
            anim.SetBool("IsGrounded", isGrounded);
            anim.SetFloat("Speed", Mathf.Abs(moveInput));
            anim.SetFloat("VerticalSpeed", rb.linearVelocity.y);
        }
    }

    void SetFacingDirection(float direction)
    {
        if (direction > 0) transform.localScale = new Vector3(-1, 1, 1);
        else if (direction < 0) transform.localScale = new Vector3(1, 1, 1);
    }

    public void ApplyRecoil(float horizontalDir, float force)
    {
        isRecoiling = true;
        recoilTimer = recoilDuration;
        rb.linearVelocity = new Vector2(horizontalDir * force, rb.linearVelocity.y);
    }

    public void TakeDamage(int damage)
    {
        if (isDead) return;
        currentHealth -= damage;
        anim.SetTrigger("Hurt");
        rb.linearVelocity = Vector2.zero;
        if (currentHealth <= 0) Die();
    }

    void PerformHeal(int healthAmount)
    {
        if (currentSoul >= soulCostPerSpell)
        {
            currentSoul -= soulCostPerSpell;
            currentHealth += healthAmount;
            if (currentHealth > maxHealth) currentHealth = maxHealth;
        }
    }

    public void GainSoul(int amount)
    {
        currentSoul += amount;
        if (currentSoul > maxSoul) currentSoul = maxSoul;
    }

    public bool TrySpendSoul(int amount)
    {
        if (currentSoul >= amount)
        {
            currentSoul -= amount;
            return true;
        }
        return false;
    }

    void Die()
    {
        isDead = true;
        anim.SetTrigger("Death");
    }

    public void LockAnimation(float duration) => StartCoroutine(LockAnimationRoutine(duration));
    private System.Collections.IEnumerator LockAnimationRoutine(float duration)
    {
        isAnimationLocked = true;
        yield return new WaitForSeconds(duration);
        isAnimationLocked = false;
    }
}