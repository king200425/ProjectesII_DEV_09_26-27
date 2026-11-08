using System.Collections;
using UnityEngine;

[RequireComponent(typeof(EnemyHealth))]
public class EnemyAI : MonoBehaviour
{
    private enum State
    {
        Patrol,
        Chase,
        Return
    }

    [Header("Patrol")]
    public float patrolRange = 2f;
    public float patrolSpeed = 1f;
    public float returnSpeed = 2f;

    [Header("Chase")]
    public float detectionRange = 10f;
    public float leashRange = 12f;
    public float chaseSpeed = 3f;
    public float chaseVerticalTolerance = 6f;
    public float losePlayerDelay = 1.5f;

    [Header("Attack")]
    public float attackRange = 1.4f;
    public float attackVerticalTolerance = 2f;
    public float attackWindup = 0.3f;
    public float attackCooldown = 1.6f;

    [Header("Ground")]
    public float edgeCheckDistance = 0.7f;
    public LayerMask groundLayer = 1 << 3;

    private EnemyHealth health;
    private EnemyAnimator animator;
    private SpriteRenderer spriteRenderer;
    private PlayerHealth player;

    private State state = State.Patrol;
    private float spawnX;
    private float patrolLeft;
    private float patrolRight;
    private float leashLeft;
    private float leashRight;
    private int patrolDirection = 1;
    private float nextAttackTime;
    private bool isAttacking;
    private float lastSeenTime;
    private Vector3 lastKnownPlayerPosition;

    void Awake()
    {
        health = GetComponent<EnemyHealth>();
        animator = GetComponent<EnemyAnimator>();
        spriteRenderer = GetComponent<SpriteRenderer>();

        if (spriteRenderer == null)
            spriteRenderer = GetComponentInChildren<SpriteRenderer>();

        spawnX = transform.position.x;
        patrolLeft = spawnX - patrolRange;
        patrolRight = spawnX + patrolRange;
        leashLeft = spawnX - leashRange;
        leashRight = spawnX + leashRange;
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
                if (state == State.Return)
                    ReturnToPatrol();
                else
                    Patrol();

                return;
            }
        }

        Vector3 playerPosition = player.transform.position;
        float dx = playerPosition.x - transform.position.x;
        float dy = Mathf.Abs(playerPosition.y - transform.position.y);

        bool playerVisible =
            Mathf.Abs(dx) <= detectionRange &&
            Mathf.Abs(dy) <= chaseVerticalTolerance &&
            playerPosition.x >= leashLeft &&
            playerPosition.x <= leashRight;

        if (playerVisible)
        {
            state = State.Chase;
            lastSeenTime = Time.time;
            lastKnownPlayerPosition = playerPosition;

            Face(dx);

            if (Mathf.Abs(dx) <= attackRange)
            {
                StopMoving();

                if (dy <= attackVerticalTolerance &&
                    Time.time >= nextAttackTime)
                {
                    StartCoroutine(AttackRoutine());
                }

                return;
            }

            ChasePlayer(playerPosition.x);
            return;
        }

        if (state == State.Chase)
        {
            if (Time.time - lastSeenTime <= losePlayerDelay)
            {
                SearchLastKnownPosition();
                return;
            }

            state = State.Return;
        }

        if (state == State.Return)
        {
            ReturnToPatrol();
            return;
        }

        Patrol();
    }

    void Patrol()
    {
        state = State.Patrol;

        float targetX = patrolDirection > 0 ? patrolRight : patrolLeft;

        if (Mathf.Abs(transform.position.x - targetX) < 0.1f ||
            !HasGroundAhead(patrolDirection))
        {
            patrolDirection *= -1;
            targetX = patrolDirection > 0 ? patrolRight : patrolLeft;
        }

        MoveTowards(targetX, patrolSpeed, patrolLeft, patrolRight);

        if (Mathf.Abs(transform.position.x - targetX) < 0.1f)
            patrolDirection *= -1;
    }

    void ChasePlayer(float targetX)
    {
        MoveTowards(targetX, chaseSpeed, leashLeft, leashRight);
    }

    void SearchLastKnownPosition()
    {
        MoveTowards(
            lastKnownPlayerPosition.x,
            chaseSpeed,
            leashLeft,
            leashRight);

        if (Mathf.Abs(transform.position.x - lastKnownPlayerPosition.x) < 0.1f)
            StopMoving();
    }

    void ReturnToPatrol()
    {
        state = State.Return;

        MoveTowards(spawnX, returnSpeed, leashLeft, leashRight);

        if (Mathf.Abs(transform.position.x - spawnX) < 0.1f)
        {
            state = State.Patrol;
            StopMoving();
        }
    }

    void MoveTowards(float targetX, float speed, float minX, float maxX)
    {
        float distance = targetX - transform.position.x;

        if (Mathf.Abs(distance) < 0.05f)
        {
            StopMoving();
            return;
        }

        int direction = distance > 0f ? 1 : -1;

        if (!HasGroundAhead(direction))
        {
            StopMoving();
            return;
        }

        float nextX = Mathf.MoveTowards(
            transform.position.x,
            targetX,
            speed * Time.deltaTime);

        nextX = Mathf.Clamp(nextX, minX, maxX);

        if (Mathf.Abs(nextX - transform.position.x) < 0.0001f)
        {
            StopMoving();
            return;
        }

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
               dy <= attackVerticalTolerance;
    }

    void FindPlayer()
    {
        player = FindAnyObjectByType<PlayerHealth>();
    }

    void OnDisable()
    {
        StopAllCoroutines();
        isAttacking = false;
    }

    void OnDrawGizmosSelected()
    {
        float centerX = Application.isPlaying ? spawnX : transform.position.x;
        float y = transform.position.y;

        Gizmos.color = Color.green;
        Gizmos.DrawLine(
            new Vector3(centerX - patrolRange, y, 0f),
            new Vector3(centerX + patrolRange, y, 0f));

        Gizmos.color = Color.cyan;
        Gizmos.DrawLine(
            new Vector3(centerX - leashRange, y, 0f),
            new Vector3(centerX + leashRange, y, 0f));

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRange);

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}
