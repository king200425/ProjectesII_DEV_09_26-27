using UnityEngine;
using UnityEngine.Rendering;

[ExecuteAlways]
[DisallowMultipleComponent]
[RequireComponent(typeof(LineRenderer), typeof(BoxCollider2D))]
public sealed class VerticalLaser : MonoBehaviour
{
    [Header("Beam")]
    [SerializeField, Min(0.1f)] private float length = 4f;
    [SerializeField, Min(0.05f), Tooltip("Width of the lethal beam in local units.")] private float width = 0.65f;
    [SerializeField, ColorUsage(false, true)] private Color beamColor = new(1f, 0.07f, 0.22f);
    [SerializeField, ColorUsage(false, true)] private Color coreColor = new(1f, 0.95f, 0.88f);
    [SerializeField, Range(0f, 2f)] private float glowIntensity = 1f;
    [SerializeField, Min(0f)] private float animationSpeed = 1f;
    [SerializeField, Range(0f, 1f), Tooltip("How much the light and width vary along the beam.")] private float beamVariation = 0.9f;
    [SerializeField, Range(12, 64), Tooltip("Size of the effect's pixel grid. Lower values give larger pixels.")] private int pixelsPerUnit = 24;

    [Header("Electricity")]
    [SerializeField, Range(0f, 1f), Tooltip("Brightness of the lightning that wraps around the beam.")] private float lightningIntensity = 0.85f;
    [SerializeField, Range(0f, 1f), Tooltip("Controls how many sparks fly out of the emitter.")] private float sparkDensity = 0.8f;

    private static readonly int BeamColor = Shader.PropertyToID("_BeamColor");
    private static readonly int CoreColor = Shader.PropertyToID("_CoreColor");
    private static readonly int BeamLength = Shader.PropertyToID("_BeamLength");
    private static readonly int BeamWidth = Shader.PropertyToID("_BeamWidth");
    private static readonly int Glow = Shader.PropertyToID("_Glow");
    private static readonly int Speed = Shader.PropertyToID("_Speed");
    private static readonly int Variation = Shader.PropertyToID("_Variation");
    private static readonly int PixelsPerUnit = Shader.PropertyToID("_PixelsPerUnit");
    private static readonly int Lightning = Shader.PropertyToID("_Lightning");
    private static readonly int Sparks = Shader.PropertyToID("_Sparks");
    private static readonly int ShutdownProgress = Shader.PropertyToID("_ShutdownProgress");

    private LineRenderer beam;
    private BoxCollider2D hitbox;
    private MaterialPropertyBlock properties;
    private bool needsRefresh;

    private void OnEnable()
    {
        beam = GetComponent<LineRenderer>();
        hitbox = GetComponent<BoxCollider2D>();
        properties ??= new MaterialPropertyBlock();
        ApplySettings();
        SetShutdownProgress(0f);
        beam.enabled = true;
        hitbox.enabled = true;
    }

    private void OnDisable()
    {
        if (beam != null)
            beam.enabled = false;
        if (hitbox != null)
            hitbox.enabled = false;
    }

    private void OnValidate()
    {
        needsRefresh = true;
    }

    private void Update()
    {
        if (needsRefresh)
            ApplySettings();
    }

    private void ApplySettings()
    {
        needsRefresh = false;
        beam.useWorldSpace = false;
        beam.alignment = LineAlignment.TransformZ;
        beam.textureMode = LineTextureMode.Stretch;
        beam.loop = false;
        beam.numCapVertices = 0;
        beam.positionCount = 2;
        beam.SetPosition(0, Vector3.zero);
        beam.SetPosition(1, Vector3.down * length);
        beam.widthCurve = AnimationCurve.Constant(0f, 1f, 1f);
        beam.widthMultiplier = width * 4f;
        beam.shadowCastingMode = ShadowCastingMode.Off;
        beam.receiveShadows = false;

        hitbox.isTrigger = true;
        hitbox.offset = new Vector2(0f, -length * 0.5f);
        hitbox.size = new Vector2(width, length);

        beam.GetPropertyBlock(properties);
        properties.SetColor(BeamColor, beamColor);
        properties.SetColor(CoreColor, coreColor);
        properties.SetFloat(BeamLength, length);
        properties.SetFloat(BeamWidth, width);
        properties.SetFloat(Glow, glowIntensity);
        properties.SetFloat(Speed, animationSpeed);
        properties.SetFloat(Variation, beamVariation);
        properties.SetFloat(PixelsPerUnit, pixelsPerUnit);
        properties.SetFloat(Lightning, lightningIntensity);
        properties.SetFloat(Sparks, sparkDensity);
        beam.SetPropertyBlock(properties);
    }

    public void StopDamage()
    {
        hitbox.enabled = false;
    }

    public void SetShutdownProgress(float progress)
    {
        if (beam == null || properties == null)
            return;

        beam.GetPropertyBlock(properties);
        properties.SetFloat(ShutdownProgress, progress);
        beam.SetPropertyBlock(properties);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (isActiveAndEnabled && hitbox.enabled && other.GetComponentInParent<PlayerHealth>() is PlayerHealth player)
            player.Die();
    }
}
