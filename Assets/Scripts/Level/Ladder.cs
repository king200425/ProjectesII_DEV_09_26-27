using UnityEngine;

public class Ladder : MonoBehaviour
{
    public float climbSpeed = 4f;

    void OnTriggerStay2D(Collider2D other)
    {
        PlayerJump jump = other.GetComponentInParent<PlayerJump>();
        Rigidbody2D rb = other.GetComponentInParent<Rigidbody2D>();
        if (jump == null || rb == null) return;

        if (Input.GetKey(KeyCode.W))
        {
            jump.onLadder = true;
            rb.gravityScale = 0f;
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, climbSpeed);
        }
        else
        {
            jump.onLadder = false;
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        PlayerJump jump = other.GetComponentInParent<PlayerJump>();
        if (jump != null) jump.onLadder = false;
    }
}
