
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

    private Rigidbody2D rb;
    private Transform groundCheck;
    private int jumpsRemaining;
    private bool wasGrounded;
    private bool waitingForTakeoff;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        groundCheck = transform.Find("GroundCheck");

        jumpsRemaining = maxJumps;
        wasGrounded = false;
    }

    void Update()
    {
        // Check ground
        bool isGrounded = Physics2D.OverlapCircle(
            groundCheck.position,
            groundCheckRadius,
            groundLayer
        ) != null;

        // Wait until the player leaves the ground after jumping
        if (waitingForTakeoff && !isGrounded)
        {
            waitingForTakeoff = false;
        }

        // Restore jumps after landing
        if (isGrounded && !wasGrounded && !waitingForTakeoff
            && rb.linearVelocity.y <= 0)
        {
            jumpsRemaining = maxJumps;
        }

        wasGrounded = isGrounded;

        // Jump
        if (Input.GetKeyDown(KeyCode.Space) && jumpsRemaining > 0)
        {
            float currentJumpForce = jumpForce;

            // Lower second jump
            if (jumpsRemaining < maxJumps)
            {
                currentJumpForce = jumpForce * secondJumpMultiplier;
            }

            rb.linearVelocity = new Vector2(
                rb.linearVelocity.x,
                currentJumpForce
            );

            jumpsRemaining--;

            if (isGrounded)
            {
                waitingForTakeoff = true;
            }
        }

        // Short jump when releasing Space
        if (Input.GetKeyUp(KeyCode.Space) && rb.linearVelocity.y > 0)
        {
            rb.linearVelocity = new Vector2(
                rb.linearVelocity.x,
                rb.linearVelocity.y * lowJumpMultiplier
            );
        }
    }

    void FixedUpdate()
    {
        // Stronger gravity while falling
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
