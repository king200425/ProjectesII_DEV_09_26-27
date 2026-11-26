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

    private static PhysicsMaterial2D runtimeNoFriction;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        if (rb == null) return;

        // Input pushes against walls every physics step. Surface friction must not cancel gravity.
        PhysicsMaterial2D material = rb.sharedMaterial;
        if (material == null || material.friction != 0f || material.bounciness != 0f)
        {
            if (runtimeNoFriction == null)
                runtimeNoFriction = new PhysicsMaterial2D("Player_NoFriction_Runtime")
                {
                    friction = 0f,
                    bounciness = 0f,
                    hideFlags = HideFlags.DontSave
                };
            material = runtimeNoFriction;
        }
        rb.sharedMaterial = material;
        foreach (Collider2D body in GetComponentsInChildren<Collider2D>(true))
            if (body.attachedRigidbody == rb && !body.isTrigger)
                body.sharedMaterial = material;
    }

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
