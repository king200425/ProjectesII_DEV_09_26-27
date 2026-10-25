
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    // Player movement speed
    public float moveSpeed = 5f;

    // Components
    private Rigidbody2D rb;
    private Animator animator;

    // Horizontal input (-1, 0, 1)
    private float horizontal;

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

        // Switch between Idle and Run
        animator.SetBool("isMoving", horizontal != 0);
    }

    void FixedUpdate()
    {
        // Apply horizontal velocity while keeping gravity
        rb.linearVelocity = new Vector2(
            horizontal * moveSpeed,
            rb.linearVelocity.y
        );
    }
}
