using UnityEngine;

public class EnemyAI : MonoBehaviour
{
    [Header("Movement Settings")]
    public float speed = 3f;
    public float chaseRange = 6f;

    [Header("Patrol Settings")]
    public float patrolDistance = 3f; // How far to walk left/right
    private Vector2 startPosition;
    private int patrolDirection = 1; // 1 for right, -1 for left

    [Header("Attack Settings")]
    public float attackRange = 1.5f;
    public float attackCooldown = 2f;
    public int damage = 1;

    [Header("Art Settings")]
    public bool spriteFacesLeft = false;

    private Transform player;
    private Rigidbody2D rb;
    private SpriteRenderer sr;
    private Animator anim;
    private float nextAttackTime = 0f;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        sr = GetComponent<SpriteRenderer>();
        anim = GetComponent<Animator>();
        
        startPosition = transform.position; // Remember where we started

        if (rb != null) rb.constraints = RigidbodyConstraints2D.FreezeRotation;

        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null) player = playerObj.transform;
    }

    void Update()
    {
        transform.position = new Vector3(transform.position.x, transform.position.y, 0);

        if (player == null) return;

        float dist = Vector2.Distance(transform.position, player.position);

        // DECISION TREE
        if (dist < attackRange)
        {
            AttackLogic();
        }
        else if (dist < chaseRange)
        {
            Chase();
        }
        else
        {
            // Player is far away, go back to patrolling
            Patrol();
        }
    }

    void Patrol()
    {
        // 1. Calculate how far we have moved from the start point
        float distanceMoved = transform.position.x - startPosition.x;

        // 2. Switch direction if we hit the patrol limits
        if (patrolDirection == 1 && distanceMoved >= patrolDistance)
        {
            patrolDirection = -1;
        }
        else if (patrolDirection == -1 && distanceMoved <= -patrolDistance)
        {
            patrolDirection = 1;
        }

        // 3. Apply movement
        if (rb != null) rb.linearVelocity = new Vector2(patrolDirection * speed * 0.5f, rb.linearVelocity.y); // Walk slower while patrolling

        // 4. Visuals
        FaceMovementDirection(patrolDirection);
        if (anim != null) anim.SetBool("isChasing", true); // Using chase animation for walking
    }

    void Chase()
    {
        FacePlayer();
        float direction = Mathf.Sign(player.position.x - transform.position.x);
        if (rb != null) rb.linearVelocity = new Vector2(direction * speed, rb.linearVelocity.y);
        if (anim != null) anim.SetBool("isChasing", true);
    }

    void AttackLogic()
    {
        if (Time.time >= nextAttackTime)
        {
            StopMoving();
            if (anim != null)
            {
                anim.SetTrigger("attack");
                anim.SetBool("isChasing", false);
            }
            nextAttackTime = Time.time + attackCooldown;
            Debug.Log("Pow! Mushroom hit the player!");
        }
        else
        {
            StopMoving();
            FacePlayer();
        }
    }

    void StopMoving()
    {
        if (rb != null) rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
    }

    void FacePlayer()
    {
        float dir = Mathf.Sign(player.position.x - transform.position.x);
        FaceMovementDirection(dir);
    }

    void FaceMovementDirection(float direction)
    {
        if (sr == null) return;
        // If moving right (1), flip based on spriteFacesLeft setting
        sr.flipX = (direction > 0) ? spriteFacesLeft : !spriteFacesLeft;
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, chaseRange);
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, attackRange);

        // Draw Patrol Range in Blue
        Gizmos.color = Color.blue;
        Vector3 start = (Application.isPlaying) ? (Vector3)startPosition : transform.position;
        Gizmos.DrawLine(start + Vector3.left * patrolDistance, start + Vector3.right * patrolDistance);
    }
}