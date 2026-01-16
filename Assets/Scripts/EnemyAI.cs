using UnityEngine;

public class EnemyAI : MonoBehaviour
{
    [Header("Movement Settings")]
    public float speed = 3f;
    public float chaseRange = 6f;

    [Header("Patrol Settings")]
    public float patrolDistance = 3f; 
    private Vector2 startPosition;
    private int patrolDirection = 1; 

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
    private EnemyHealth health; 
    private float nextAttackTime = 0f;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        sr = GetComponent<SpriteRenderer>();
        anim = GetComponent<Animator>();
        health = GetComponent<EnemyHealth>();
        
        startPosition = transform.position;

        if (rb != null) rb.constraints = RigidbodyConstraints2D.FreezeRotation;

        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null) player = playerObj.transform;
    }

    void Update()
    {
        // Ensure Z position stays at 0
        transform.position = new Vector3(transform.position.x, transform.position.y, 0);

        // --- RECOIL CHECK ---
        // If getting hit, don't run AI logic. Once recoil finishes, health.IsRecoiling becomes false.
        if (health != null && health.IsRecoiling) return; 

        if (player == null) return;

        float dist = Vector2.Distance(transform.position, player.position);

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
            Patrol();
        }
    }

    void Patrol()
    {
        float distanceMoved = transform.position.x - startPosition.x;

        if (patrolDirection == 1 && distanceMoved >= patrolDistance) patrolDirection = -1;
        else if (patrolDirection == -1 && distanceMoved <= -patrolDistance) patrolDirection = 1;

        if (rb != null) rb.linearVelocity = new Vector2(patrolDirection * speed * 0.5f, rb.linearVelocity.y);

        FaceMovementDirection(patrolDirection);
        if (anim != null) anim.SetBool("isChasing", true); 
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
        sr.flipX = (direction > 0) ? spriteFacesLeft : !spriteFacesLeft;
    }
}