using UnityEngine;
using UnityEngine.UI;

// 主菜单粒子有三个阶段：漂浮、吸收、散开。
[RequireComponent(typeof(CanvasRenderer))]
public sealed class ChronoMenuParticles : MaskableGraphic
{
    [SerializeField] private RawImage[] echoTrails = System.Array.Empty<RawImage>();

    // 原设计图的坐标范围。最后会换算到当前窗口，不是运行分辨率。
    private const float Width = 1816f;
    private const float Height = 1050f;
    private static readonly Vector2 ClockCenter = new Vector2(908f, 802f);
    private static readonly Color32[] Palette =
    {
        new Color32(0, 240, 255, 255), new Color32(125, 244, 255, 255),
        new Color32(224, 182, 255, 255), new Color32(136, 85, 210, 255),
    };

    private enum ParticleState { Floating, Absorbing, Bursting }

    // 每个粒子只记录位置、大小、速度、颜色和运动用的角度。
    private struct MenuParticle
    {
        public Vector2 position;
        public float phase;
        public float size;
        public float speed;
        public Color32 tint;
    }

    private MenuParticle[] particles;
    private Vector2[] launchPositions;
    private Vector2[] trailOrigins;
    private ParticleState state;
    private float stateStarted;
    private float ignition;

    protected override void Awake()
    {
        base.Awake();
        Initialize();
        trailOrigins = new Vector2[echoTrails.Length];
        for (int i = 0; i < echoTrails.Length; i++)
            trailOrigins[i] = echoTrails[i].rectTransform.anchoredPosition;
    }

    public void BeginIgnition()
    {
        Initialize();
        state = ParticleState.Absorbing;
        stateStarted = Time.unscaledTime;
        launchPositions = new Vector2[particles.Length];
        for (int i = 0; i < particles.Length; i++)
            launchPositions[i] = FloatingPosition(particles[i], stateStarted);
    }

    public void SetIgnition(float value)
    {
        ignition = value;
    }

    public void BeginBurst()
    {
        state = ParticleState.Bursting;
        stateStarted = Time.unscaledTime;
    }

    private void Update()
    {
        if (!Application.isPlaying)
            return;
        SetVerticesDirty();
        UpdateCharacterTrails();
    }

    private void UpdateCharacterTrails()
    {
        float time = Time.unscaledTime;
        for (int i = 0; i < echoTrails.Length; i++)
        {
            RawImage echo = echoTrails[i];
            float direction = -1f;
            if (i >= echoTrails.Length / 2)
                direction = 1f;

            float life = Mathf.Repeat(time * Mathf.Lerp(0.23f, 1.2f, ignition) + i * 0.31f, 1f);
            float offset = Mathf.Round((5f + life * 20f) * (1f + ignition * 1.2f));
            echo.rectTransform.anchoredPosition = trailOrigins[i] + new Vector2(direction * offset, 0f);
            Color tint = echo.color;
            float pulse = Mathf.Pow(Mathf.Max(0f, Mathf.Sin(time * 1.3f + i * 0.4f)), 5f);
            tint.a = (1f - life) * Mathf.Lerp(0.025f + 0.045f * pulse, 0.28f, ignition);
            echo.color = tint;
        }
    }

    protected override void OnPopulateMesh(VertexHelper mesh)
    {
        mesh.Clear();
        Initialize();
        float time = 3f;
        if (Application.isPlaying)
            time = Time.unscaledTime;
        float age = time - stateStarted;

        for (int i = 0; i < particles.Length; i++)
        {
            if (state == ParticleState.Absorbing)
                DrawAbsorbingParticle(mesh, i, age);
            else if (state == ParticleState.Bursting)
                DrawBurstingParticle(mesh, i, age);
            else
                DrawFloatingParticle(mesh, i, time);
        }
        if (state == ParticleState.Absorbing)
            DrawAttractionRing(mesh, age);
        UiParticleDrawing.FitToRect(mesh, rectTransform.rect, Width, Height);
    }

    private void DrawFloatingParticle(VertexHelper mesh, int index, float time)
    {
        MenuParticle particle = particles[index];
        Vector2 position = FloatingPosition(particle, time);
        Color32 tint = particle.tint;
        tint.a = (byte)(tint.a * (0.6f + 0.4f * Mathf.Sin(time * 0.8f + particle.phase)));
        DrawParticleSymbol(mesh, index, position, tint);
    }

    private void DrawAbsorbingParticle(VertexHelper mesh, int index, float age)
    {
        MenuParticle particle = particles[index];
        float progress = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(age / 1.65f));
        Vector2 relative = launchPositions[index] - ClockCenter;
        float angle = Mathf.Atan2(relative.y, relative.x) + age * (1.1f + particle.phase * 0.1f);
        float radius = Mathf.Lerp(relative.magnitude, 12f, progress);
        Vector2 position = ClockCenter + new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * radius;
        Vector2 direction = (ClockCenter - position).normalized;
        Color32 tint = particle.tint;
        tint.a = (byte)(Mathf.Lerp(tint.a, 195f, ignition) * (1f - progress));
        UiParticleDrawing.Line(mesh, position - direction * (10f + ignition * 25f), position, particle.size * 0.55f, tint);
        DrawParticleSymbol(mesh, index, position, tint);
    }

    private void DrawBurstingParticle(VertexHelper mesh, int index, float age)
    {
        MenuParticle particle = particles[index];
        Vector2 direction = new Vector2(Mathf.Cos(particle.phase), Mathf.Sin(particle.phase));
        float radius = 8f + age * (600f + particle.speed * 75f);
        Vector2 position = ClockCenter + direction * radius;
        Color32 tint = particle.tint;
        tint.a = (byte)(230f * (1f - Mathf.Clamp01(age / 0.95f)));
        UiParticleDrawing.Line(mesh, position - direction * (18f + age * 80f), position, 1.5f + particle.size * 0.3f, tint);
        DrawParticleSymbol(mesh, index, position, tint);
    }

    private void DrawParticleSymbol(VertexHelper mesh, int index, Vector2 position, Color32 tint)
    {
        UiParticleDrawing.Square(mesh, position, particles[index].size * (1f + ignition * 0.5f), tint);
        if (index % 9 != 0)
            return;
        // Sparse broken clock ticks amongst the floating pixels.
        UiParticleDrawing.Line(mesh, position + Vector2.right * 4f, position + Vector2.right * 10f, 1.2f, tint);
        UiParticleDrawing.Line(mesh, position + Vector2.up * 4f, position + Vector2.up * 9f, 1.2f, tint);
    }

    private void DrawAttractionRing(VertexHelper mesh, float age)
    {
        for (int i = 0; i < 32; i++)
        {
            float life = Mathf.Repeat(age * 0.95f + i / 32f, 1f);
            float angle = i * 2.39996f + age * (1.5f + ignition * 3f);
            float radius = Mathf.Lerp(280f, 12f, life);
            Vector2 position = ClockCenter + new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * radius;
            Color32 tint = Palette[i % Palette.Length];
            tint.a = (byte)(170f * ignition * Mathf.Sin(life * Mathf.PI));
            UiParticleDrawing.Square(mesh, position, 3f + 2f * ignition, tint);
            UiParticleDrawing.Line(mesh, position, position + (position - ClockCenter).normalized * 12f, 1.2f, tint);
        }
    }

    private void Initialize()
    {
        if (particles != null)
            return;
        System.Random random = new System.Random(1911);
        particles = new MenuParticle[76];
        for (int i = 0; i < particles.Length; i++)
        {
            Color32 tint = Palette[i % Palette.Length];
            tint.a = (byte)random.Next(45, 116);
            particles[i] = new MenuParticle
            {
                position = new Vector2((float)random.NextDouble() * Width, (float)random.NextDouble() * Height),
                phase = (float)random.NextDouble() * 6.28f,
                size = random.Next(2, 5),
                speed = random.Next(7, 19),
                tint = tint,
            };
        }
    }

    private static Vector2 FloatingPosition(MenuParticle particle, float time)
    {
        return new Vector2(
            Mathf.Repeat(particle.position.x + Mathf.Sin(time * 0.24f + particle.phase) * 19f, Width),
            Mathf.Repeat(particle.position.y + time * particle.speed, Height));
    }
}
