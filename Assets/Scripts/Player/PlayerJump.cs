using System.Collections.Generic;
using UnityEngine;

[DefaultExecutionOrder(-20)]
public class PlayerJump : MonoBehaviour
{
    public float jumpForce = 10f;
    public float lowJumpMultiplier = 0.5f;
    public int maxJumps = 2;
    public float secondJumpMultiplier = 0.8f;

    public float normalGravity = 1f;
    public float fallGravityMultiplier = 2f;
    public float groundCheckRadius = 0.15f;
    public LayerMask groundLayer;

    [Header("Wall Jump")]
    [SerializeField, Min(0.1f)] private float wallSlideSpeed = 2.4f;
    [SerializeField, Min(0.1f)] private float wallJumpHorizontalForce = 9f;
    [SerializeField, Min(0.1f)] private float wallJumpVerticalMultiplier = 0.85f;
    [SerializeField, Min(0f)] private float wallJumpInputBuffer = 0.16f;

    private const float WallLaunchTime = 0.14f;
    private const float MinimumWallRiseTime = 0.08f;
    private const float WallReleaseGraceTime = 0.08f;
    private static readonly int WallSliding = Animator.StringToHash("isWallSliding");
    private static readonly int WallJumpTrigger = Animator.StringToHash("WallJumpTrigger");
    private readonly List<RaycastHit2D> wallHits = new(4);
    private ContactFilter2D wallFilter;
    private Rigidbody2D rb;
    private Collider2D bodyCollider;
    private Transform groundCheck;
    private Animator animator;
    private int jumpsRemaining;
    private bool wasGrounded;
    private bool waitingForTakeoff;
    private bool hasBeenAirborne;
    private bool jumpPressed;
    private bool jumpReleased;
    private bool canShortenJump;
    private int launchWallSide;
    private float launchTimeRemaining;
    private bool leftLaunchWall;
    private int detachedWallSide;
    private float detachGraceRemaining;
    private bool ignoreHeldAwayInput;
    private float bufferedWallJumpTime;
    private int ignoredWallSide;

    public int WallSide { get; private set; }
    public int WallLaunchDirection => launchTimeRemaining > 0f ? -launchWallSide : 0;
    public bool OverridesHorizontalMovement => WallSide != 0 || launchWallSide != 0;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        bodyCollider = GetComponent<Collider2D>();
        groundCheck = transform.Find("GroundCheck");
        animator = GetComponentInChildren<Animator>();
        wallFilter.useTriggers = false;
    }

    private void OnEnable()
    {
        jumpsRemaining = maxJumps;
        wasGrounded = IsGrounded();
    }

    private void OnDisable()
    {
        WallSide = 0;
        if (animator != null)
            animator.SetBool(WallSliding, false);
        jumpPressed = false;
        jumpReleased = false;
        canShortenJump = false;
        ClearWallLaunch();
        animator?.ResetTrigger(WallJumpTrigger);
        waitingForTakeoff = false;
        hasBeenAirborne = false;
        ignoreHeldAwayInput = false;
        bufferedWallJumpTime = 0f;
        ignoredWallSide = 0;
        detachedWallSide = 0;
        detachGraceRemaining = 0f;
    }

    private void Update()
    {
        jumpPressed |= Input.GetKeyDown(KeyCode.Space);
        jumpReleased |= Input.GetKeyUp(KeyCode.Space);
    }

    private void FixedUpdate()
    {
        bool isGrounded = IsGrounded();
        bool justLanded = isGrounded && !wasGrounded && !waitingForTakeoff && rb.linearVelocity.y <= 0f;
        if (waitingForTakeoff && !isGrounded)
        {
            waitingForTakeoff = false;
            hasBeenAirborne = true;
        }

        if (justLanded)
        {
            jumpsRemaining = maxJumps;
            canShortenJump = false;
            ClearWallLaunch();
            ignoreHeldAwayInput = false;
            bufferedWallJumpTime = 0f;
            ignoredWallSide = 0;
            detachedWallSide = 0;
            detachGraceRemaining = 0f;
            if (hasBeenAirborne)
            {
                animator.ResetTrigger("JumpTrigger");
                animator.ResetTrigger(WallJumpTrigger);
                animator.SetTrigger("LandTrigger");
            }
            hasBeenAirborne = false;
        }
        wasGrounded = isGrounded;
        bufferedWallJumpTime = Mathf.Max(0f, bufferedWallJumpTime - Time.fixedDeltaTime);
        detachGraceRemaining = Mathf.Max(0f, detachGraceRemaining - Time.fixedDeltaTime);

        float horizontal = Input.GetAxisRaw("Horizontal");
        UpdateWallLaunch(horizontal);
        if (ignoredWallSide != 0 && !HasWall(ignoredWallSide))
            ignoredWallSide = 0;

        int previousWallSide = WallSide;
        if (previousWallSide != 0 && horizontal * previousWallSide >= 0f)
            ignoreHeldAwayInput = false;

        WallSide = isGrounded ? 0 : FindWallSide(horizontal);
        if (WallSide != 0 && launchWallSide != 0)
        {
            // A direction held during the crossing cannot release the newly grabbed wall.
            ignoreHeldAwayInput = horizontal * WallSide < 0f;
            ClearWallLaunch();
        }
        if (WallSide != 0)
            detachGraceRemaining = 0f;

        // A fresh Space press takes priority over releasing the same wall in this step.
        if (WallSide == 0 && previousWallSide != 0 && !isGrounded && HasWall(previousWallSide))
        {
            detachedWallSide = previousWallSide;
            detachGraceRemaining = WallReleaseGraceTime;
        }

        int jumpWallSide = WallSide;
        if (jumpWallSide == 0 && jumpPressed)
        {
            if (justLanded)
                jumpWallSide = previousWallSide;
            else if (!isGrounded && detachGraceRemaining > 0f)
                jumpWallSide = detachedWallSide;
        }

        if (jumpWallSide != 0 && (jumpPressed || bufferedWallJumpTime > 0f))
        {
            StartWallJump(jumpWallSide, isGrounded, horizontal);
        }
        else if (jumpPressed && !isGrounded && jumpsRemaining == 0)
        {
            bufferedWallJumpTime = wallJumpInputBuffer;
        }
        else if (jumpPressed && jumpsRemaining > 0)
        {
            // A ground jump beside a wall must have room to rise before grabbing that side again.
            if (isGrounded)
                ignoredWallSide = FindWallSide(horizontal);

            float force = jumpsRemaining < maxJumps ? jumpForce * secondJumpMultiplier : jumpForce;
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, force);
            jumpsRemaining--;
            waitingForTakeoff = isGrounded;
            PlayJump();
        }
        jumpPressed = false;

        if (canShortenJump && jumpReleased)
        {
            if (launchWallSide == 0 || launchTimeRemaining <= WallLaunchTime - MinimumWallRiseTime)
            {
                canShortenJump = false;
                if (rb.linearVelocity.y > 0f)
                    rb.linearVelocity = new Vector2(rb.linearVelocity.x, rb.linearVelocity.y * lowJumpMultiplier);
            }
        }
        if (!canShortenJump || launchWallSide == 0)
        {
            jumpReleased = false;
        }

        if (WallSide != 0)
        {
            // Maintain contact while the player is holding still.
            rb.linearVelocity = new Vector2(WallSide * 0.85f, -wallSlideSpeed);
            rb.gravityScale = 0f;
            canShortenJump = false;
            jumpReleased = false;
            hasBeenAirborne = true;
        }
        else
        {
            rb.gravityScale = rb.linearVelocity.y < 0f ? normalGravity * fallGravityMultiplier : normalGravity;
        }

        animator.SetBool(WallSliding, WallSide != 0);
        animator.SetBool("isGrounded", isGrounded && !waitingForTakeoff);
        animator.SetFloat("verticalSpeed", rb.linearVelocity.y);
    }

    private bool IsGrounded()
    {
        return Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer) != null;
    }

    private int FindWallSide(float horizontal)
    {
        if (WallSide != 0)
            return (ignoreHeldAwayInput || horizontal * WallSide >= 0f) && HasWall(WallSide) ? WallSide : 0;

        if (launchWallSide != 0)
            return HasWall(-launchWallSide) ? -launchWallSide : 0;

        if (ignoredWallSide != -1 && horizontal <= 0f && HasWall(-1))
            return -1;
        if (ignoredWallSide != 1 && horizontal >= 0f && HasWall(1))
            return 1;
        return 0;
    }

    private bool HasWall(int side)
    {
        if (bodyCollider == null)
            return false;

        Bounds bounds = bodyCollider.bounds;
        // Shorten the cast vertically so the head and feet can clear wall corners.
        Vector2 probeSize = new(bounds.size.x - 0.04f, bounds.size.y * 0.72f);
        Physics2D.BoxCast(bounds.center, probeSize, 0f, Vector2.right * side, wallFilter, wallHits, 0.075f);
        foreach (RaycastHit2D hit in wallHits)
        {
            if (hit.collider.attachedRigidbody != rb && hit.normal.x * side < -0.85f)
                return hit.collider.CompareTag("WallJump");
        }
        return false;
    }

    private void UpdateWallLaunch(float horizontal)
    {
        if (launchWallSide == 0)
            return;

        launchTimeRemaining = Mathf.Max(0f, launchTimeRemaining - Time.fixedDeltaTime);
        leftLaunchWall |= !HasWall(launchWallSide);
        if (horizontal * launchWallSide <= 0f)
            ignoreHeldAwayInput = false;

        if (launchTimeRemaining <= 0f && leftLaunchWall &&
            ((horizontal * launchWallSide > 0f && !ignoreHeldAwayInput) || rb.linearVelocity.x == 0f))
            ClearWallLaunch();
    }

    private void ClearWallLaunch()
    {
        launchWallSide = 0;
        launchTimeRemaining = 0f;
        leftLaunchWall = false;
    }

    private void StartWallJump(int wallSide, bool isGrounded, float horizontal)
    {
        rb.linearVelocity = new Vector2(-wallSide * wallJumpHorizontalForce,
                                        jumpForce * wallJumpVerticalMultiplier);
        WallSide = 0;
        launchWallSide = wallSide;
        launchTimeRemaining = WallLaunchTime;
        leftLaunchWall = false;
        ignoreHeldAwayInput = horizontal * wallSide > 0f;
        detachGraceRemaining = 0f;
        bufferedWallJumpTime = 0f;
        jumpsRemaining = 0;
        waitingForTakeoff = isGrounded;
        hasBeenAirborne = true;
        canShortenJump = true;
        jumpReleased = !Input.GetKey(KeyCode.Space);
        animator.ResetTrigger("LandTrigger");
        animator.ResetTrigger("JumpTrigger");
        animator.SetTrigger(WallJumpTrigger);
    }

    private void PlayJump()
    {
        hasBeenAirborne = true;
        canShortenJump = true;
        animator.ResetTrigger("LandTrigger");
        animator.SetTrigger("JumpTrigger");
    }

    private void OnDrawGizmosSelected()
    {
        Transform check = transform.Find("GroundCheck");
        if (check != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(check.position, groundCheckRadius);
        }
    }
}
