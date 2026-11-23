using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    // Player movement speed
    public float moveSpeed = 5f;

    // Sprint multiplier
    public float sprintMultiplier = 1.5f;

    // Components
    private Rigidbody2D rb;
    private Animator animator;
    private PlayerJump playerJump;

    // Movement input
    private float horizontal;
    private bool isSprinting;

    void Start()
    {
        // Get player components
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponentInChildren<Animator>();
        playerJump = GetComponent<PlayerJump>();
    }

    void Update()
    {
        if (Time.timeScale == 0f)
            return;

        // Get horizontal movement input
        horizontal = Input.GetAxisRaw("Horizontal");
        if (horizontal == 0f)
        {
            if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow)) horizontal = -1f;
            else if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow)) horizontal = 1f;
        }

        // Hold Shift to sprint
        isSprinting =
            Input.GetKey(KeyCode.LeftShift) && horizontal != 0;

        // Switch between Idle and Run
        animator.SetBool("isMoving", horizontal != 0);

        // Change running animation speed
        animator.SetFloat(
            "runSpeedMultiplier",
            isSprinting ? sprintMultiplier : 1f
        );
    }

    void FixedUpdate()
    {
        if (playerJump != null && playerJump.OverridesHorizontalMovement)
            return;

        // Calculate current movement speed
        float currentSpeed = moveSpeed;

        if (isSprinting)
        {
            currentSpeed *= sprintMultiplier;
        }

        // Apply horizontal velocity while keeping gravity
        rb.linearVelocity = new Vector2(
            horizontal * currentSpeed,
            rb.linearVelocity.y
        );
    }
}
