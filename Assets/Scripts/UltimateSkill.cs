using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class UltimateSkill : MonoBehaviour
{
    [Header("Assign These")]
    public Animator animator;
    public GameObject arrowPrefab;
    public PlayerMovement playerMovement;
    public Rigidbody2D rb;

    [Header("Soul Settings")] // --- NEW ---
    public int soulCost = 33; // Cost to use the Rain of Arrows

    [Header("Rain Settings")]
    public int arrowCount = 15;
    public float rainDuration = 1.5f;
    public float chantDuration = 1.0f;

    [Header("Targeting Logic")]
    public float spawnHeight = 10f;
    public float searchRadius = 10f;
    public float accuracyScatter = 0.5f;

    private bool isAttacking = false;

    void Start()
    {
        if (playerMovement == null) playerMovement = GetComponent<PlayerMovement>();
        if (rb == null) rb = GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        // Check for Input AND if we are not already attacking
        if (Input.GetKeyDown(KeyCode.F) && !isAttacking)
        {
            AttemptUltimate();
        }
    }

    void AttemptUltimate()
    {
        // 1. CHECK SOUL
        // We ask the PlayerMovement script: "Do we have 33 soul?"
        // If yes, it automatically subtracts it and returns true.
        if (playerMovement != null && playerMovement.TrySpendSoul(soulCost))
        {
            StartCoroutine(CastUltimate());
        }
        else
        {
            Debug.Log("Not enough Soul for Ultimate!");
            // Optional: Play a "fail" sound effect here
        }
    }

    IEnumerator CastUltimate()
    {
        isAttacking = true;

        // 1. FREEZE PLAYER
        if (playerMovement != null)
        {
            playerMovement.isControlLocked = true;
            rb.linearVelocity = Vector2.zero;
        }

        // 2. ANIMATE
        if (animator != null) animator.SetTrigger("UseUlt");

        // 3. CHANT DELAY
        yield return new WaitForSeconds(chantDuration);

        // 4. UNFREEZE PLAYER (You can move while arrows fall)
        if (playerMovement != null) playerMovement.isControlLocked = false;

        // 5. SMART RAIN LOOP
        float timeBetween = rainDuration / arrowCount;
        for (int i = 0; i < arrowCount; i++)
        {
            SpawnSmartArrow();
            yield return new WaitForSeconds(timeBetween);
        }

        isAttacking = false;
    }

    void SpawnSmartArrow()
    {
        float targetX;

        // A. RADAR CHECK
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, searchRadius);
        List<Transform> nearbyEnemies = new List<Transform>();

        foreach (Collider2D hit in hits)
        {
            if (hit.GetComponent<EnemyHealth>() != null)
            {
                nearbyEnemies.Add(hit.transform);
            }
        }

        // B. DECIDE TARGET
        if (nearbyEnemies.Count > 0)
        {
            Transform victim = nearbyEnemies[Random.Range(0, nearbyEnemies.Count)];
            targetX = victim.position.x + Random.Range(-accuracyScatter, accuracyScatter);
        }
        else
        {
            // Fallback: Rain around player
            targetX = transform.position.x + Random.Range(-searchRadius / 2f, searchRadius / 2f);
        }

        // C. SPAWN
        Vector3 dropPos = new Vector3(targetX, transform.position.y + spawnHeight, 0);
        Quaternion dropRot = Quaternion.Euler(0, 0, -90);

        Instantiate(arrowPrefab, dropPos, dropRot);
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, searchRadius);
    }
}