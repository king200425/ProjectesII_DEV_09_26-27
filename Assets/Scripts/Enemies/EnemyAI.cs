using System.Collections;
using UnityEngine;

[RequireComponent(typeof(EnemyHealth))]
public class EnemyAI : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 1f;
    public float patrolRange = 2f;
    public float edgeCheckDistance = 0.7f;
    public LayerMask groundLayer = 1 << 3;

    [Header("Detection")]
    public float detectionRange = 5f;
    public float attackRange = 1.4f;
    public float verticalTolerance = 2f;

    [Header("Attack")]
    public float attackWindup = 0.3f;
    public float attackCooldown = 1.6f;

    private EnemyHealth health;
    private EnemyAnimator animator;
    private SpriteRenderer spriteRenderer;
    private PlayerHealth player;

    private float leftBound;
    private float rightBound;
    private int patrolDirection = 1;
    private float nextAttackTime;
    private bool isAttacking;

    void Awake()
    {
        health = GetComponent<EnemyHealth>();
        animator = GetComponent<EnemyAnimator>();
        spriteRenderer = GetComponent<SpriteRenderer>();

        if (spriteRenderer == null)
            spriteRenderer = GetComponentInChildren<SpriteRenderer>();

        leftBound = transform.position.x - patrolRange;
        rightBound = transform.position.x + patrolRange;
    }

    void Start()
    {
        FindPlayer();
    }

    void Update()
    {
        if (health != null && health.IsDead)
            return;

        if (isAttacking)
            return;

        if (player == null)
        {
            FindPlayer();

            if (player == null)
            {
                Patrol();
                return;
            }
        }

        float dx = player.transform.position.x - transform.position.x;

        float dy = Mathf.Abs(
            player.transform.position.y - transform.position.y);

        if (Mathf.Abs(dx) <= attackRange && dy <= verticalTolerance)
        {
            Face(dx);
            StopMoving();

            if (Time.time >= nextAttackTime)
                StartCoroutine(AttackRoutine());
        }
        else if (Mathf.Abs(dx) <= detectionRange && dy <= verticalTolerance)
        {
            Chase(dx);
        }
        else
        {
            Patrol();
        }
    }

    void Patrol()
    {
        if (transform.position.x >= rightBound)
        {
            patrolDirection = -1;
        }
        else if (transform.position.x <= leftBound)
        {
            patrolDirection = 1;
        }

        if (!HasGroundAhead(patrolDirection))
        {
            patrolDirection *= -1;
            StopMoving();
            return;
        }

        Move(patrolDirection);
    }

    void Chase(float dx)
    {
        int direction = dx > 0f ? 1 : -1;

        float nextX =
            transform.position.x + direction * moveSpeed * Time.deltaTime;

        if (nextX < leftBound ||
            nextX > rightBound ||
            !HasGroundAhead(direction))
        {
            Face(dx);
            StopMoving();
            return;
        }

        Move(direction);
    }

    void Move(int direction)
    {
        float nextX =
            transform.position.x + direction * moveSpeed * Time.deltaTime;

        nextX = Mathf.Clamp(nextX, leftBound, rightBound);

        transform.position = new Vector3(
            nextX,
            transform.position.y,
            transform.position.z);

        Face(direction);
        SetMoving(true);
    }

    void StopMoving()
    {
        SetMoving(false);
    }

    void SetMoving(bool moving)
    {
        if (animator != null)
            animator.SetMoving(moving);
    }

    void Face(float dx)
    {
        if (Mathf.Abs(dx) < 0.01f)
            return;

        bool faceLeft = dx < 0f;

        if (spriteRenderer != null)
            spriteRenderer.flipX = faceLeft;
    }

    bool HasGroundAhead(int direction)
    {
        if (groundLayer.value == 0)
            return true;

        Vector2 origin = new Vector2(
            transform.position.x + direction * edgeCheckDistance,
            transform.position.y + 0.5f);

        RaycastHit2D hit = Physics2D.Raycast(
            origin,
            Vector2.down,
            1.5f,
            groundLayer);

        return hit.collider != null;
    }

    IEnumerator AttackRoutine()
    {
        isAttacking = true;
        nextAttackTime = Time.time + attackCooldown;

        StopMoving();

        if (animator != null)
            animator.PlayAttack();

        yield return new WaitForSeconds(attackWindup);

        if (health != null && health.IsDead)
        {
            isAttacking = false;
            yield break;
        }

        if (player != null && IsPlayerInAttackRange())
        {
            player.TakeDamage();
            UnityEngine.Debug.Log(name + " attacked the player.");
        }

        float remaining = 0f;

        if (animator != null)
            remaining = animator.AttackDuration - attackWindup;

        if (remaining > 0f)
            yield return new WaitForSeconds(remaining);

        if (animator != null)
            animator.PlayIdle();

        isAttacking = false;
    }

    bool IsPlayerInAttackRange()
    {
        if (player == null)
            return false;

        float dx = Mathf.Abs(
            player.transform.position.x - transform.position.x);

        float dy = Mathf.Abs(
            player.transform.position.y - transform.position.y);

        return dx <= attackRange + 0.3f &&
               dy <= verticalTolerance;
    }

    void FindPlayer()
    {
        player = FindFirstObjectByType<PlayerHealth>();
    }

    void OnDisable()
    {
        StopAllCoroutines();
        isAttacking = false;
    }
}
