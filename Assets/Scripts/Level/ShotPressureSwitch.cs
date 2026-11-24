using UnityEngine;
using UnityEngine.Events;

[DisallowMultipleComponent]
public sealed class ShotPressureSwitch : MonoBehaviour
{
    [Header("射击与回弹")]
    [Min(1)] public int hitsRequired = 9;
    [Tooltip("从最后一发命中算起，完全弹回所需的时间。")]
    [Min(0.05f)] public float returnSeconds = 1f;
    [Tooltip("容许正常连发的短间隔；计入总回弹时间。")]
    [Min(0f)] public float shotGraceSeconds = 0.18f;
    [Min(0.01f)] public float pressTravelSeconds = 0.5f;
    public bool acceptGhostBullets = true;

    [Header("柱子部件（已在预制体中配置）")]
    public SpriteRenderer baseRenderer;
    public SpriteRenderer shaftRenderer;
    public SpriteRenderer capRenderer;
    public SpriteRenderer completedRenderer;
    public Sprite pressedRedSprite;
    public Sprite unlockedGreenSprite;
    public BoxCollider2D hitbox;
    public ShotPressureSwitchFeedback feedback;
    public float capRestY = 1.375f;
    public float capTravel = 0.3125f;
    public float shaftRestHeight = 0.25f;
    public float fullHeight = 2.8125f;
    public bool flat2DParts;
    public float shaftScaleAtRest = 1f;

    [Header("激活联动：以后把门的 Open 方法拖到这里")]
    public UnityEvent onActivated = new UnityEvent();

    public bool IsActive { get; private set; }
    public bool IsLatched => latched;
    public float Progress01 => progress;
    public float VisualProgress01 => visualProgress;
    public int AcceptedShots { get; private set; }
    public int RemainingHits => latched ? 0 : Mathf.Max(0,
        Mathf.CeilToInt((1f - progress) * Mathf.Max(1, hitsRequired) - 0.00001f));

    float progress;
    float progressAtLastShot;
    float visualProgress;
    float lastShotTime = float.NegativeInfinity;
    float fullyPressedTime = -1f;
    bool latched;
    MaterialPropertyBlock capProperties;
    int renderedCapState = -1;
    static readonly int ActivatedProperty = Shader.PropertyToID("_Activated");
    const float GreenConfirmSeconds = 0.06f;

    void Awake() => RefreshVisuals();

    void OnEnable()
    {
        renderedCapState = -1;
        RefreshVisuals();
    }

    // Bullet owns impact/consumption. This component never independently counts collider callbacks.
    public bool ReceiveShot(bool ghostBullet = false)
    {
        if (!isActiveAndEnabled || latched || (ghostBullet && !acceptGhostBullets)
            || Time.timeScale <= 0f) return false;
        Recover(Time.time);
        AcceptedShots++;
        progress = Mathf.Min(1f, progress + 1f / Mathf.Max(1, hitsRequired));
        if (progress >= 1f - 0.00001f)
        {
            progress = 1f;
            latched = true;
        }
        lastShotTime = Time.time;
        progressAtLastShot = progress;
        return true;
    }

    void Recover(float now)
    {
        if (latched || progress <= 0f) return;
        float duration = Mathf.Max(0.05f, returnSeconds);
        float grace = Mathf.Clamp(shotGraceSeconds, 0f, duration * 0.8f);
        float elapsed = now - lastShotTime;
        if (elapsed <= grace) return;
        progress = Mathf.Lerp(progressAtLastShot, 0f,
            Mathf.Clamp01((elapsed - grace) / (duration - grace)));
    }

    void Update()
    {
        if (Time.timeScale <= 0f) return;
        Recover(Time.time);
        visualProgress = Mathf.MoveTowards(visualProgress, progress,
            Time.deltaTime / Mathf.Max(0.01f, pressTravelSeconds));
        if (latched && visualProgress >= 1f)
        {
            if (fullyPressedTime < 0f) fullyPressedTime = Time.time;
            if (!IsActive && Time.time - fullyPressedTime >= GreenConfirmSeconds)
            {
                IsActive = true;
                RefreshVisuals();
                if (feedback != null) feedback.Play(flat2DParts && capRenderer != null
                    ? capRenderer.bounds.center : completedRenderer != null
                        ? completedRenderer.bounds.center : transform.position);
                onActivated.Invoke();
                return;
            }
        }
        RefreshVisuals();
    }

    void RefreshVisuals()
    {
        bool showFullSprite = !flat2DParts && (IsActive || (latched && visualProgress >= 1f));
        if (baseRenderer != null) baseRenderer.enabled = !showFullSprite;
        if (shaftRenderer != null)
        {
            float remaining = Mathf.Max(0f, shaftRestHeight - capTravel * visualProgress);
            shaftRenderer.enabled = !showFullSprite && remaining > 0.00001f;
            Vector3 scale = shaftRenderer.transform.localScale;
            scale.y = shaftScaleAtRest * remaining / Mathf.Max(0.00001f, shaftRestHeight);
            shaftRenderer.transform.localScale = scale;
        }
        if (capRenderer != null)
        {
            capRenderer.enabled = !showFullSprite;
            Vector3 position = capRenderer.transform.localPosition;
            position.y = capRestY - capTravel * visualProgress;
            capRenderer.transform.localPosition = position;
            if (flat2DParts && renderedCapState != (IsActive ? 1 : 0))
            {
                capProperties ??= new MaterialPropertyBlock();
                capRenderer.GetPropertyBlock(capProperties);
                capProperties.SetFloat(ActivatedProperty, IsActive ? 1 : 0);
                capRenderer.SetPropertyBlock(capProperties);
                renderedCapState = IsActive ? 1 : 0;
            }
        }
        if (completedRenderer != null)
        {
            completedRenderer.enabled = showFullSprite;
            completedRenderer.sprite = IsActive ? unlockedGreenSprite : pressedRedSprite;
        }
        FitHitboxToHead();
    }

    // Fit only the pressure plate, in collider-local coordinates; the rod and base stay non-solid.
    public void FitHitboxToHead()
    {
        if (hitbox == null || capRenderer == null || capRenderer.sprite == null) return;
        Bounds face = capRenderer.sprite.bounds;
        Vector3 center = hitbox.transform.InverseTransformPoint(capRenderer.transform.TransformPoint(
            face.center + Vector3.up * (face.size.y * 0.04f)));
        Vector3 horizontal = hitbox.transform.InverseTransformVector(capRenderer.transform.TransformVector(
            Vector3.right * (face.size.x * 0.96f)));
        Vector3 vertical = hitbox.transform.InverseTransformVector(capRenderer.transform.TransformVector(
            Vector3.up * (face.size.y * 0.84f)));
        Vector2 size = new Vector2(Mathf.Abs(horizontal.x) + Mathf.Abs(vertical.x),
            Mathf.Abs(horizontal.y) + Mathf.Abs(vertical.y));
        Vector2 offset = new Vector2(center.x, center.y);
        if ((hitbox.size - size).sqrMagnitude > 0.00000001f) hitbox.size = size;
        if ((hitbox.offset - offset).sqrMagnitude > 0.00000001f) hitbox.offset = offset;
    }

    void OnValidate()
    {
        hitsRequired = Mathf.Max(1, hitsRequired);
        returnSeconds = Mathf.Max(0.05f, returnSeconds);
        pressTravelSeconds = Mathf.Max(0.01f, pressTravelSeconds);
        shotGraceSeconds = Mathf.Clamp(shotGraceSeconds, 0f, returnSeconds * 0.8f);
    }
}
