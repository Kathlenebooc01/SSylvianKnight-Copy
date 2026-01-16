using UnityEngine;

public class EnemyAI : MonoBehaviour
{
    [Header("Movement Settings")]
    public float speed = 3f;
    public float chaseRange = 6f;

    [Header("Attack Settings")]
    public float attackRange = 1.5f; // How close to stand before punching
    public float attackCooldown = 2f; // Wait 2 seconds between hits
    public int damage = 1; // How much it hurts

    [Header("Art Settings")]
    [Tooltip("Check this if sprite looks Left by default")]
    public bool spriteFacesLeft = false;

    private Transform player;
    private Rigidbody2D rb;
    private SpriteRenderer sr;
    private Animator anim;

    // Timer to track cooldown
    private float nextAttackTime = 0f;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        sr = GetComponent<SpriteRenderer>();
        anim = GetComponent<Animator>();

        if (rb != null) rb.constraints = RigidbodyConstraints2D.FreezeRotation;

        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null) player = playerObj.transform;
    }

    void Update()
    {
        // Safety: Keep Z axis at 0
        transform.position = new Vector3(transform.position.x, transform.position.y, 0);

        if (player == null) return;

        float dist = Vector2.Distance(transform.position, player.position);

        // DECISION TREE
        if (dist < attackRange)
        {
            // We are close enough to hit!
            if (Time.time >= nextAttackTime)
            {
                Attack();
            }
            else
            {
                // Waiting for cooldown... just stand still and face player
                StopMoving();
                FacePlayer();
            }
        }
        else if (dist < chaseRange)
        {
            // Too far to hit, but close enough to chase
            Chase();
        }
        else
        {
            // Player lost, chill out
            StopChasing();
        }
    }

    void Chase()
    {
        FacePlayer();

        // Move
        float direction = Mathf.Sign(player.position.x - transform.position.x);
        if (rb != null) rb.linearVelocity = new Vector2(direction * speed, rb.linearVelocity.y);

        if (anim != null) anim.SetBool("isChasing", true);
    }

    void Attack()
    {
        // 1. Stop moving so we don't slide while punching
        StopMoving();

        // 2. Play Animation
        if (anim != null)
        {
            anim.SetTrigger("attack");
            anim.SetBool("isChasing", false); // Stop run animation
        }

        // 3. Reset Cooldown
        nextAttackTime = Time.time + attackCooldown;

        // 4. Deal Damage (Simple Logic)
        // This instantly hurts the player. Later we can make it sync with the punch frame.
        Debug.Log("Pow! Mushroom hit the player!");

        // TODO: Add code here later to actually subtract HP from Player
        // player.GetComponent<PlayerHealth>().TakeDamage(damage);
    }

    void StopChasing()
    {
        StopMoving();
        if (anim != null) anim.SetBool("isChasing", false);
    }

    void StopMoving()
    {
        if (rb != null) rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
    }

    void FacePlayer()
    {
        if (sr == null) return;

        bool playerIsRight = (player.position.x > transform.position.x);

        if (playerIsRight)
            sr.flipX = spriteFacesLeft;
        else
            sr.flipX = !spriteFacesLeft;
    }

    // DRAW RANGES
    void OnDrawGizmosSelected()
    {
        // Chase Range (Red)
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, chaseRange);

        // Attack Range (Yellow)
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}