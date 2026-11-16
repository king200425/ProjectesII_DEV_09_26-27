using UnityEngine;

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

    private Rigidbody2D rb;
    private Transform groundCheck;
    private Animator animator;

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
        wasGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer) != null;
    }

    void Update()
    {
        bool isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer) != null;

        if (waitingForTakeoff && !isGrounded)
        {
            waitingForTakeoff = false;
            hasBeenAirborne = true;
        }

        if (isGrounded && !wasGrounded && !waitingForTakeoff && rb.linearVelocity.y <= 0)
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

        if (Input.GetKeyDown(KeyCode.Space) && jumpsRemaining > 0)
        {
            float currentJumpForce = jumpForce;

            if (jumpsRemaining < maxJumps)
                currentJumpForce *= secondJumpMultiplier;

            rb.linearVelocity = new Vector2(rb.linearVelocity.x, currentJumpForce);
            jumpsRemaining--;

            animator.ResetTrigger("LandTrigger");
            animator.SetTrigger("JumpTrigger");

            if (isGrounded)
                waitingForTakeoff = true;
            else
                hasBeenAirborne = true;
        }

        if (Input.GetKeyUp(KeyCode.Space) && rb.linearVelocity.y > 0)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, rb.linearVelocity.y * lowJumpMultiplier);
        }

        animator.SetBool("isGrounded", isGrounded);
        animator.SetFloat("verticalSpeed", rb.linearVelocity.y);
    }

    void FixedUpdate()
    {
        if (rb.linearVelocity.y < 0)
            rb.gravityScale = normalGravity * fallGravityMultiplier;
        else
            rb.gravityScale = normalGravity;
    }
}
