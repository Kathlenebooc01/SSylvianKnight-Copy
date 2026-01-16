using UnityEngine;
using System.Collections;

public class PlayerMeleeAttack : MonoBehaviour
{
    [Header("Setup")]
    public Animator animator;
    public Transform attackPoint;

    [Header("Attack Timing")]
    public float hitDelay = 0.05f;
    public float attackDuration = 0.4f;
    public float attackCooldown = 0.5f;

    [Header("Combat Stats")]
    public float attackRange = 1.5f;
    public int damagePerHit = 15;
    public int soulGainAmount = 11; // Soul per hit

    [Header("Recoil & Polish")]
    public float recoilForceX = 12f;
    public float hitStopDuration = 0.05f;
    public LayerMask enemyLayer;

    [Header("Pogo Settings")]
    public float pogoForce = 15f;

    public bool isAttacking { get; private set; }

    private float nextAttackTime = 0f;
    private Rigidbody2D rb;
    private PlayerMovement playerMovement;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        playerMovement = GetComponent<PlayerMovement>();
        isAttacking = false;
    }

    void Update()
    {
        if (Input.GetButtonDown("Fire1") && Time.time >= nextAttackTime && !isAttacking)
        {
            float yInput = Input.GetAxisRaw("Vertical");
            StartCoroutine(PerformDirectionalSlash(yInput));
            nextAttackTime = Time.time + attackCooldown;
        }
    }

    IEnumerator PerformDirectionalSlash(float yInput)
    {
        isAttacking = true;
        bool isPogoMove = false;

        if (playerMovement != null) playerMovement.LockAnimation(attackDuration);

        animator.ResetTrigger("Attack");
        animator.ResetTrigger("AttackUp");
        animator.ResetTrigger("AttackDown");

        if (yInput > 0)
        {
            // ATTACK UP
            animator.SetTrigger("AttackUp");
            attackPoint.localPosition = new Vector3(0, attackRange, 0);
        }
        else if (yInput < 0 && !playerMovement.IsGroundedState())
        {
            // ATTACK DOWN (Pogo)
            isPogoMove = true;
            animator.SetTrigger("AttackDown");
            attackPoint.localPosition = new Vector3(0, -attackRange, 0);
        }
        else
        {
            // FORWARD ATTACK
            animator.SetTrigger("Attack");

            // --- THE FIX ---
            // OLD: attackPoint.localPosition = new Vector3(-1.2f, 0, 0);

            // NEW: Change -1.2f to -0.5f. 
            // This pulls the center of the hit closer to your chest.
            attackPoint.localPosition = new Vector3(-0.5f, 0, 0);
        }

        yield return new WaitForSeconds(hitDelay);
        CheckForDamage(isPogoMove);

        float remainingTime = attackDuration - hitDelay;
        if (remainingTime > 0) yield return new WaitForSeconds(remainingTime);

        isAttacking = false;
    }

    void CheckForDamage(bool isPogo)
    {
        Collider2D[] hitEnemies = Physics2D.OverlapCircleAll(attackPoint.position, attackRange, enemyLayer);

        bool hasAppliedEffects = false;
        bool hitSomething = false;

        foreach (Collider2D hit in hitEnemies)
        {
            EnemyHealth enemy = hit.GetComponent<EnemyHealth>();
            bool isBouncable = hit.gameObject.layer == LayerMask.NameToLayer("Bouncables");

            if (enemy != null || isBouncable)
            {
                if (enemy != null)
                {
                    enemy.TakeDamage(damagePerHit);
                    // Add Soul logic
                    if (playerMovement != null) playerMovement.GainSoul(soulGainAmount);
                }

                hitSomething = true;

                if (!hasAppliedEffects)
                {
                    StartCoroutine(HitStop());

                    // Apply Recoil
                    if (!isPogo && playerMovement != null)
                    {
                        float recoilDir = Mathf.Sign(transform.position.x - hit.transform.position.x);
                        playerMovement.ApplyRecoil(recoilDir, recoilForceX);
                    }
                    else if (!isPogo && rb != null)
                    {
                        float recoilDir = Mathf.Sign(transform.position.x - hit.transform.position.x);
                        rb.linearVelocity = Vector2.zero;
                        rb.AddForce(new Vector2(recoilDir * recoilForceX, 1f), ForceMode2D.Impulse);
                    }
                    hasAppliedEffects = true;
                }
            }
            else if (hit.gameObject.layer == LayerMask.NameToLayer("Ground"))
            {
                hitSomething = true;
            }
        }

        if (isPogo && hitSomething && rb != null)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, pogoForce);
            nextAttackTime = Time.time;
            isAttacking = false;
        }
    }

    IEnumerator HitStop()
    {
        float originalScale = Time.timeScale;
        Time.timeScale = 0f;
        yield return new WaitForSecondsRealtime(hitStopDuration);
        Time.timeScale = originalScale;
    }

    void OnDrawGizmosSelected()
    {
        if (attackPoint == null) return;
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(attackPoint.position, attackRange);
    }
}