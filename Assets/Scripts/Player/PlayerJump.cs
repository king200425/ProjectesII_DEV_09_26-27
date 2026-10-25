
using UnityEngine;

public class PlayerJump : MonoBehaviour
{
    // Jump settings
    public float jumpForce = 10f;
    public float lowJumpMultiplier = 0.5f;
    public int maxJumps = 2;
    public float secondJumpMultiplier = 0.8f;

    // Gravity settings
    public float normalGravity = 1f;
    public float fallGravityMultiplier = 2f;

    // Ground detection
    public float groundCheckRadius = 0.15f;
    public LayerMask groundLayer;

    // Components
    private Rigidbody2D rb;
    private Transform groundCheck;
    private Animator animator;

    // Jump state
    private int jumpsRemaining;
    private bool wasGrounded;
    private bool waitingForTakeoff;
    private bool hasBeenAirborne;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        groundCheck = transform.Find("GroundCheck");
        animator = GetComponentInChildren<Animator>();

        jumpsRemaining = maxJumps;

        // Check initial ground state
        wasGrounded = Physics2D.OverlapCircle(
            groundCheck.position,
            groundCheckRadius,
            groundLayer
        ) != null;
    }

    void Update()
    {
        // Check ground
        bool isGrounded = Physics2D.OverlapCircle(
            groundCheck.position,
            groundCheckRadius,
            groundLayer
        ) != null;

        // Detect takeoff
        if (waitingForTakeoff && !isGrounded)
        {
            waitingForTakeoff = false;
            hasBeenAirborne = true;
        }

        // Detect landing
        if (isGrounded && !wasGrounded &&
            !waitingForTakeoff && rb.linearVelocity.y <= 0)
        {
            jumpsRemaining = maxJumps;

            if (hasBeenAirborne)
            {
                animator.ResetTrigger("JumpTrigger");
                animator.SetTrigger("LandTrigger");
            }

            hasBeenAirborne = false;
        }

        wasGrounded = isGrounded;

        // Jump
        if (Input.GetKeyDown(KeyCode.Space) && jumpsRemaining > 0)
        {
            float currentJumpForce = jumpForce;

            // Reduce second jump strength
            if (jumpsRemaining < maxJumps)
            {
                currentJumpForce *= secondJumpMultiplier;
            }

            rb.linearVelocity = new Vector2(
                rb.linearVelocity.x,
                currentJumpForce
            );

            jumpsRemaining--;

            // Play jump animation
            animator.ResetTrigger("LandTrigger");
            animator.SetTrigger("JumpTrigger");

            if (isGrounded)
            {
                waitingForTakeoff = true;
            }
            else
            {
                hasBeenAirborne = true;
            }
        }

        // Short jump when Space is released
        if (Input.GetKeyUp(KeyCode.Space) && rb.linearVelocity.y > 0)
        {
            rb.linearVelocity = new Vector2(
                rb.linearVelocity.x,
                rb.linearVelocity.y * lowJumpMultiplier
            );
        }

        // Update Animator
        animator.SetBool("isGrounded", isGrounded);
        animator.SetFloat("verticalSpeed", rb.linearVelocity.y);
    }

    void FixedUpdate()
    {
        // Stronger gravity when falling
        if (rb.linearVelocity.y < 0)
        {
            rb.gravityScale = normalGravity * fallGravityMultiplier;
        }
        else
        {
            rb.gravityScale = normalGravity;
        }
    }

    void OnDrawGizmosSelected()
    {
        Transform check = transform.Find("GroundCheck");

        if (check != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(check.position, groundCheckRadius);
        }
    }
}
