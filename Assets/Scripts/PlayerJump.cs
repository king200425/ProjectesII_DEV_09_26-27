
using UnityEngine;

public class PlayerJump : MonoBehaviour
{
    // Jump settings
    public float jumpForce = 10f;
    public float lowJumpMultiplier = 0.5f;

    // Gravity settings
    public float normalGravity = 1f;
    public float fallGravityMultiplier = 2f;

    // Ground detection
    public float groundCheckRadius = 0.15f;
    public LayerMask groundLayer;

    private Rigidbody2D rb;
    private Transform groundCheck;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        groundCheck = transform.Find("GroundCheck");
    }

    void Update()
    {
        // Check if the player is grounded
        bool isGrounded = Physics2D.OverlapCircle(
            groundCheck.position,
            groundCheckRadius,
            groundLayer
        ) != null;

        // Jump when Space is pressed
        if (Input.GetKeyDown(KeyCode.Space) && isGrounded)
        {
            rb.linearVelocity = new Vector2(
                rb.linearVelocity.x,
                jumpForce
            );
        }

        // Reduce jump height when Space is released
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
        // Use stronger gravity while falling
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
