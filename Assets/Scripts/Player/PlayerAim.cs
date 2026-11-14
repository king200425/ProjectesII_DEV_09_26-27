using UnityEngine;

public class PlayerAim : MonoBehaviour
{
    public Transform gunPivot;
    public Transform gunFlip;
    public SpriteRenderer playerSprite;

    private Camera mainCamera;

    private Animator animator;
    private PlayerJump playerJump;

    void Start()
    {
        mainCamera = Camera.main;
        animator = GetComponentInChildren<Animator>();
        playerJump = GetComponent<PlayerJump>();
    }

    void LateUpdate()
    {
        // Get mouse position in world space
        Vector3 mousePosition =
            mainCamera.ScreenToWorldPoint(Input.mousePosition);

        // Calculate aiming direction
        Vector2 direction =
            mousePosition - gunPivot.position;

        float angle = Mathf.Atan2(direction.y, direction.x)
                      * Mathf.Rad2Deg;

        // Rotate the gun toward the mouse
        gunPivot.rotation = Quaternion.Euler(0, 0, angle);

        // Check if player is aiming left
        bool aimingLeft = direction.x < 0;

        float horizontal = Input.GetAxisRaw("Horizontal");

        bool movingBackward =
            (horizontal > 0 && aimingLeft) ||
            (horizontal < 0 && !aimingLeft);

        animator.SetBool("isMovingBackward", movingBackward);

        // Flip the player
        int wallSide = playerJump != null ? playerJump.WallSide : 0;
        int launchDirection = playerJump != null ? playerJump.WallLaunchDirection : 0;
        playerSprite.flipX = wallSide != 0 ? wallSide > 0
                           : launchDirection != 0 ? launchDirection < 0
                           : aimingLeft;

        // Flip the gun vertically when aiming left
        gunFlip.localScale = new Vector3(
            1f,
            aimingLeft ? -1f : 1f,
            1f
        );
    }
}
