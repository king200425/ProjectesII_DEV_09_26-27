
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

    // Movement input
    private float horizontal;
    private bool isSprinting;

    void Start()
    {
        // Get player components
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponentInChildren<Animator>();
    }

    void Update()
    {
        // Get horizontal movement input
        horizontal = Input.GetAxisRaw("Horizontal");

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
