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
    public float patrolSpeed = 1.2f;
    public float returnSpeed = 2.5f;

    [Header("Detection")]
    public float detectionRange = 8f;
    public float leashRange = 10f;
    public float chaseSpeed = 3.2f;
    public float chaseVerticalTolerance = 2.5f;
    public float losePlayerDelay = 1.2f;

    [Header("Attack")]
    public float attackRange = 1.25f;
    public float attackVerticalTolerance = 1.5f;
    public float attackWindup = 0.3f;
    public float attackRecover = 0.22f;
    public float attackCooldown = 1.4f;
    public float attackLungeDistance = 0.45f;
    public float attackLungeDuration = 0.18f;
    public Sprite attackEffectSprite;

    [Header("Ground")]
    public float edgeCheckDistance = 0.6f;
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
    private bool isStaggered;
    private float lastSeenTime;
    private Vector3 lastKnownPlayerPosition;
    private Color baseColor = Color.white;

    void Awake()
    {
        health = GetComponent<EnemyHealth>();
        animator = GetComponent<EnemyAnimator>();
        spriteRenderer = GetComponent<SpriteRenderer>();

        if (spriteRenderer == null)
            spriteRenderer = GetComponentInChildren<SpriteRenderer>();

        if (spriteRenderer != null)
            baseColor = spriteRenderer.color;

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

        if (isAttacking || isStaggered)
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
        float distance = Vector2.Distance(
            transform.position,
            playerPosition);

        bool playerVisible =
            distance <= detectionRange &&
            dy <= chaseVerticalTolerance &&
            playerPosition.x >= leashLeft &&
            playerPosition.x <= leashRight;

        if (playerVisible)
        {
            state = State.Chase;
            lastSeenTime = Time.time;
            lastKnownPlayerPosition = playerPosition;

            Face(dx);

            if (Mathf.Abs(dx) <= attackRange &&
                dy <= attackVerticalTolerance)
            {
                StopMoving();

                if (Time.time >= nextAttackTime)
                    StartCoroutine(AttackRoutine());

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

        if (Mathf.Abs(
                transform.position.x - lastKnownPlayerPosition.x) < 0.1f)
        {
            StopMoving();
        }
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

        if (animator != null)
            animator.SetFacing(faceLeft);
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

        if (player != null)
            Face(player.transform.position.x - transform.position.x);

        if (animator != null)
            animator.PlayAttack();

        if (spriteRenderer != null)
            spriteRenderer.color = new Color(1f, 0.78f, 0.45f, 1f);

        yield return new WaitForSeconds(attackWindup);

        if (health != null && health.IsDead)
        {
            isAttacking = false;
            ResetColor();
            yield break;
        }

        float lungeDirection = 1f;

        if (player != null)
            lungeDirection = player.transform.position.x >=
                transform.position.x ? 1f : -1f;

        Vector3 startPosition = transform.position;
        float elapsed = 0f;

        while (elapsed < attackLungeDuration)
        {
            elapsed += Time.deltaTime;
            float progress = Mathf.Clamp01(
                elapsed / attackLungeDuration);
            float curve = Mathf.Sin(progress * Mathf.PI);
            transform.position = startPosition +
                Vector3.right * lungeDirection *
                attackLungeDistance * curve;
            yield return null;
        }

        transform.position = startPosition;

        if (player != null && IsPlayerInAttackRange())
        {
            player.TakeDamage();
            StartCoroutine(AttackEffect(
                transform.position +
                Vector3.right * lungeDirection * 0.7f +
                Vector3.up * 1.1f));
        }

        yield return new WaitForSeconds(attackRecover);

        if (animator != null)
            animator.PlayIdle();

        ResetColor();
        isAttacking = false;
    }

    IEnumerator AttackEffect(Vector3 position)
    {
        if (attackEffectSprite == null)
            yield break;

        GameObject effect = new GameObject("EnemyAttackEffect");
        effect.transform.position = position;
        effect.transform.localScale = Vector3.one * 0.5f;

        SpriteRenderer renderer = effect.AddComponent<SpriteRenderer>();
        renderer.sprite = attackEffectSprite;
        renderer.sortingOrder = 12;
        renderer.color = new Color(1f, 0.45f, 0.2f, 0.9f);

        float elapsed = 0f;
        float duration = 0.22f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float progress = Mathf.Clamp01(elapsed / duration);
            effect.transform.localScale =
                Vector3.one * Mathf.Lerp(0.5f, 1.4f, progress);
            Color color = renderer.color;
            color.a = Mathf.Lerp(0.9f, 0f, progress);
            renderer.color = color;
            yield return null;
        }

        Destroy(effect);
    }

    bool IsPlayerInAttackRange()
    {
        if (player == null)
            return false;

        float dx = Mathf.Abs(
            player.transform.position.x - transform.position.x);

        float dy = Mathf.Abs(
            player.transform.position.y - transform.position.y);

        return dx <= attackRange + 0.35f &&
               dy <= attackVerticalTolerance;
    }

    public void OnHit()
    {
        if (health != null && health.IsDead)
            return;

        if (isAttacking)
        {
            StopAllCoroutines();
            isAttacking = false;

            if (animator != null)
                animator.PlayIdle();

            ResetColor();
        }

        StartCoroutine(Stagger());
    }

    IEnumerator Stagger()
    {
        if (isStaggered)
            yield break;

        isStaggered = true;
        yield return new WaitForSeconds(0.18f);
        isStaggered = false;
    }

    void ResetColor()
    {
        if (spriteRenderer != null)
            spriteRenderer.color = baseColor;
    }

    void FindPlayer()
    {
        player = FindAnyObjectByType<PlayerHealth>();
    }

    void OnDisable()
    {
        StopAllCoroutines();
        isAttacking = false;
        isStaggered = false;
        ResetColor();
    }

    void OnDrawGizmosSelected()
    {
        float centerX = Application.isPlaying
            ? spawnX
            : transform.position.x;
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
