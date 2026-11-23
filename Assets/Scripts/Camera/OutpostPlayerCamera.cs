using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

// The main Camera is a child of this rig, so existing local camera shake stays independent.
[DefaultExecutionOrder(-150)]
public class OutpostPlayerCamera : MonoBehaviour
{
    public Transform target;
    public Camera viewCamera;
    [Range(0.25f, 0.6f)] public float playerViewportY = 0.4f;
    [Range(0f, 0.2f)] public float movementLookFraction = 0.07f;
    public Vector2 mouseLookFraction = new Vector2(0.025f, 0.02f);
    [Range(0.1f, 0.25f)] public float maximumHorizontalLookFraction = 0.12f;
    public float followSmoothTime = 0.12f;
    public float lookSmoothTime = 0.6f;
    public float turnDelay = 0.3f;
    [Range(0.02f, 0.3f)] public float horizontalLookSpeedFraction = 0.1f;
    public Vector2 verticalDeadZone = new Vector2(0.25f, 0.68f);
    public float fallingLookAhead = 0.8f;
    public bool mouseLookEnabled = true;

    Rigidbody2D targetBody;
    PlayerJump jumpController;
    Transform groundCheck;
    float groundedAnchorY;
    int facing;
    int pendingFacing;
    float turnTimer;
    Vector2 lookOffset;
    Vector2 lookVelocity;
    Vector3 followVelocity;
    Vector3 previousTarget;
    bool initialized;
    readonly List<RaycastResult> uiHits = new List<RaycastResult>();

    public Vector2 CurrentLookOffset => lookOffset;
    public int FacingDirection => facing;

    void OnEnable()
    {
        CacheTarget();
        if (target != null && viewCamera != null) SnapToTarget();
    }

    void CacheTarget()
    {
        if (target == null) return;
        targetBody = target.GetComponent<Rigidbody2D>();
        jumpController = target.GetComponent<PlayerJump>();
        groundCheck = target.Find("GroundCheck");
    }

    public void SnapToTarget()
    {
        if (target == null || viewCamera == null) return;
        CacheTarget();
        groundedAnchorY = target.position.y;
        previousTarget = target.position;
        lookOffset = lookVelocity = Vector2.zero;
        followVelocity = Vector3.zero;
        facing = pendingFacing = 0;
        turnTimer = 0;
        float height = viewCamera.orthographicSize * 2f;
        transform.position = new Vector3(target.position.x,
            target.position.y + (0.5f - playerViewportY) * height, -10f);
        initialized = true;
    }

    bool IsGrounded()
    {
        if (jumpController == null || groundCheck == null) return false;
        if (targetBody != null && targetBody.linearVelocity.y > 0.2f) return false;
        return Physics2D.OverlapCircle(groundCheck.position,
            jumpController.groundCheckRadius, jumpController.groundLayer) != null;
    }

    bool IsOverInteractiveUI(Vector2 position)
    {
        EventSystem events = EventSystem.current;
        if (events == null) return false;
        uiHits.Clear();
        events.RaycastAll(new PointerEventData(events) { position = position }, uiHits);
        // Full-screen visual overlays are allowed; actual buttons and controls suppress mouse look.
        foreach (RaycastResult hit in uiHits)
            if (hit.gameObject.GetComponentInParent<Selectable>() != null) return true;
        return false;
    }

    Vector2 ReadMouseAim()
    {
        if (!mouseLookEnabled || !Application.isFocused) return Vector2.zero;
        Vector3 screen = Input.mousePosition;
        if (!viewCamera.pixelRect.Contains(screen) || IsOverInteractiveUI(screen)) return Vector2.zero;
        Vector3 cursor = viewCamera.ScreenToViewportPoint(screen);
        Vector3 player = viewCamera.WorldToViewportPoint(target.position);
        Vector2 delta = new Vector2(cursor.x - player.x, cursor.y - player.y);
        const float quiet = 0.035f;
        return new Vector2(
            Mathf.Sign(delta.x) * Mathf.Clamp01((Mathf.Abs(delta.x) - quiet) / 0.4f),
            Mathf.Sign(delta.y) * Mathf.Clamp01((Mathf.Abs(delta.y) - quiet) / 0.35f));
    }

    void UpdateFacing(float horizontal, float dt)
    {
        int requested = Mathf.Abs(horizontal) > 0.15f ? (horizontal > 0 ? 1 : -1) : 0;
        if (requested == 0) { turnTimer = 0; pendingFacing = 0; return; }
        if (facing == 0) { facing = requested; return; }
        if (requested == facing) { turnTimer = 0; pendingFacing = 0; return; }
        if (pendingFacing != requested) { pendingFacing = requested; turnTimer = 0; }
        turnTimer += dt;
        if (turnTimer >= turnDelay) { facing = requested; pendingFacing = 0; turnTimer = 0; }
    }

    void LateUpdate()
    {
        if (target == null || viewCamera == null || Time.timeScale <= 0f) return;
        if (!initialized || Vector3.Distance(target.position, previousTarget) > 12f) SnapToTarget();
        previousTarget = target.position;
        float dt = Mathf.Min(Time.unscaledDeltaTime, 0.05f);
        float height = viewCamera.orthographicSize * 2f;
        float width = height * viewCamera.aspect;
        float vx = targetBody != null ? targetBody.linearVelocity.x : 0;
        float horizontal = Mathf.Abs(vx) > 0.2f ? vx : Input.GetAxisRaw("Horizontal");
        UpdateFacing(horizontal, dt);

        Vector2 mouse = ReadMouseAim();
        float movementLead = facing * movementLookFraction * width;
        // Blend opposing mouse aim continuously so crossing the player never flips the lead abruptly.
        movementLead *= Mathf.Lerp(1f, 0.7f, Mathf.Clamp01(-mouse.x * facing));
        Vector2 desiredLook = new Vector2(
            Mathf.Clamp(movementLead + mouse.x * mouseLookFraction.x * width,
                -maximumHorizontalLookFraction * width, maximumHorizontalLookFraction * width),
            mouse.y * mouseLookFraction.y * height);
        // Limit look-ahead travel independently of the responsive player-follow movement.
        lookOffset.x = Mathf.SmoothDamp(lookOffset.x, desiredLook.x, ref lookVelocity.x,
            lookSmoothTime, width * horizontalLookSpeedFraction, dt);
        lookOffset.y = Mathf.SmoothDamp(lookOffset.y, desiredLook.y, ref lookVelocity.y,
            lookSmoothTime, Mathf.Infinity, dt);

        bool grounded = IsGrounded();
        if (grounded) groundedAnchorY = target.position.y;
        float y = groundedAnchorY + (0.5f - playerViewportY) * height;
        if (!grounded)
        {
            // Small jumps use the quiet region; larger ascents and falls move the frame.
            y = Mathf.Max(y, target.position.y + (0.5f - verticalDeadZone.y) * height);
            y = Mathf.Min(y, target.position.y + (0.5f - verticalDeadZone.x) * height);
            float vy = targetBody != null ? targetBody.linearVelocity.y : 0;
            if (vy < -2.5f) y -= Mathf.Clamp01((-vy - 2.5f) / 7f) * fallingLookAhead;
        }
        Vector3 destination = new Vector3(target.position.x + lookOffset.x, y + lookOffset.y, -10f);
        transform.position = Vector3.SmoothDamp(transform.position, destination,
            ref followVelocity, followSmoothTime, Mathf.Infinity, dt);
    }
}
