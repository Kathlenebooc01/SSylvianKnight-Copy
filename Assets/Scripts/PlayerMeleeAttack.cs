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
    public int soulGainAmount = 11;

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

        if (yInput > 0)
        {
            animator.SetTrigger("AttackUp");
            attackPoint.localPosition = new Vector3(0, attackRange, 0);
        }
        else if (yInput < 0 && !playerMovement.IsGroundedState())
        {
            isPogoMove = true;
            animator.SetTrigger("AttackDown");
            attackPoint.localPosition = new Vector3(0, -attackRange, 0);
        }
        else
        {
            animator.SetTrigger("Attack");
            attackPoint.localPosition = new Vector3(-0.5f, 0, 0);
        }

        yield return new WaitForSeconds(hitDelay);
        CheckForDamage(isPogoMove);
        yield return new WaitForSeconds(attackDuration - hitDelay);
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
            if (enemy != null)
            {
                // FIX: Added 'transform'
                enemy.TakeDamage(damagePerHit, transform);
                if (playerMovement != null) playerMovement.GainSoul(soulGainAmount);
                hitSomething = true;

                if (!hasAppliedEffects)
                {
                    StartCoroutine(HitStop());
                    float recoilDir = Mathf.Sign(transform.position.x - hit.transform.position.x);
                    if (!isPogo && playerMovement != null) playerMovement.ApplyRecoil(recoilDir, recoilForceX);
                    hasAppliedEffects = true;
                }
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
}