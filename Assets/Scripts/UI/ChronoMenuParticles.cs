using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(CanvasRenderer))]
public sealed class ChronoMenuParticles : MaskableGraphic
{
    [SerializeField] private RawImage[] echoTrails = System.Array.Empty<RawImage>();

    private const float Width = 1816f;
    private const float Height = 1050f;
    private static readonly Vector2 ClockCenter = new(908f, 802f);
    private static readonly Color32[] Palette =
    {
        new(0, 240, 255, 255), new(125, 244, 255, 255),
        new(224, 182, 255, 255), new(136, 85, 210, 255),
    };

    private struct Mote
    {
        public Vector2 position;
        public float phase;
        public float size;
        public float speed;
        public Color32 tint;
    }

    private Mote[] motes;
    private Vector2[] launchPositions;
    private Vector2[] trailOrigins;
    private float ignitionStarted = -1f;
    private float ignition;
    private float burstStarted = -1f;

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
        ignitionStarted = Time.unscaledTime;
        launchPositions = new Vector2[motes.Length];
        for (int i = 0; i < motes.Length; i++)
            launchPositions[i] = IdlePosition(motes[i], ignitionStarted);
    }

    public void SetIgnition(float value) => ignition = value;

    public void BeginBurst()
    {
        burstStarted = Time.unscaledTime;
    }

    private void Update()
    {
        if (!Application.isPlaying)
            return;

        SetVerticesDirty();
        float time = Time.unscaledTime;
        for (int i = 0; i < echoTrails.Length; i++)
        {
            RawImage echo = echoTrails[i];
            float direction = i < echoTrails.Length / 2 ? -1f : 1f;
            float life = Mathf.Repeat(time * Mathf.Lerp(.23f, 1.2f, ignition) + i * .31f, 1f);
            float offset = Mathf.Round((5f + life * 20f) * (1f + ignition * 1.2f));
            echo.rectTransform.anchoredPosition = trailOrigins[i] + new Vector2(direction * offset, 0f);
            Color tint = echo.color;
            float idlePulse = Mathf.Pow(Mathf.Max(0f, Mathf.Sin(time * 1.3f + i * .4f)), 5f);
            tint.a = (1f - life) * Mathf.Lerp(.025f + .045f * idlePulse, .28f, ignition);
            echo.color = tint;
        }
    }

    protected override void OnPopulateMesh(VertexHelper mesh)
    {
        mesh.Clear();
        Initialize();
        float time = Application.isPlaying ? Time.unscaledTime : 3f;
        float age = ignitionStarted < 0f ? 0f : time - ignitionStarted;
        float burstAge = burstStarted < 0f ? -1f : time - burstStarted;
        float attraction = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(age / 1.65f));

        for (int i = 0; i < motes.Length; i++)
        {
            Mote mote = motes[i];
            Vector2 position = IdlePosition(mote, time);
            Vector2 direction = Vector2.up;
            Color32 tint = mote.tint;
            if (burstAge >= 0f)
            {
                direction = new Vector2(Mathf.Cos(mote.phase), Mathf.Sin(mote.phase));
                float speed = 600f + mote.speed * 75f;
                float radius = 8f + burstAge * speed;
                position = ClockCenter + direction * radius;
                float fade = 1f - Mathf.Clamp01(burstAge / .95f);
                tint.a = (byte)(230f * fade);
                Line(mesh, position - direction * (18f + burstAge * 80f), position, 1.5f + mote.size * .3f, tint);
            }
            else if (ignitionStarted >= 0f)
            {
                Vector2 relative = launchPositions[i] - ClockCenter;
                float angle = Mathf.Atan2(relative.y, relative.x) + age * (1.1f + mote.phase * .1f);
                float radius = Mathf.Lerp(relative.magnitude, 12f, attraction);
                position = ClockCenter + new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * radius;
                direction = (ClockCenter - position).normalized;
                tint.a = (byte)(Mathf.Lerp(tint.a, 195f, ignition) * (1f - attraction));
                Line(mesh, position - direction * (10f + ignition * 25f), position, mote.size * .55f, tint);
            }
            else
            {
                tint.a = (byte)(tint.a * (.6f + .4f * Mathf.Sin(time * .8f + mote.phase)));
            }
            Square(mesh, position, mote.size * (1f + ignition * .5f), tint);
            if (i % 9 == 0)
            {
                // Sparse broken clock ticks amongst the floating pixels.
                Line(mesh, position + Vector2.right * 4f, position + Vector2.right * 10f, 1.2f, tint);
                Line(mesh, position + Vector2.up * 4f, position + Vector2.up * 9f, 1.2f, tint);
            }
        }

        if (ignitionStarted >= 0f && burstAge < 0f)
        {
            for (int i = 0; i < 32; i++)
            {
                float life = Mathf.Repeat(age * .95f + i / 32f, 1f);
                float angle = i * 2.39996f + age * (1.5f + ignition * 3f);
                float radius = Mathf.Lerp(280f, 12f, life);
                Vector2 position = ClockCenter + new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * radius;
                Color32 tint = Palette[i % Palette.Length];
                tint.a = (byte)(170f * ignition * Mathf.Sin(life * Mathf.PI));
                Square(mesh, position, 3f + 2f * ignition, tint);
                Line(mesh, position, position + (position - ClockCenter).normalized * 12f, 1.2f, tint);
            }
        }
    }

    private void Initialize()
    {
        if (motes != null)
            return;
        var random = new System.Random(1911);
        motes = new Mote[76];
        for (int i = 0; i < motes.Length; i++)
        {
            Color32 tint = Palette[i % Palette.Length];
            tint.a = (byte)random.Next(45, 116);
            motes[i] = new Mote
            {
                position = new Vector2((float)random.NextDouble() * Width, (float)random.NextDouble() * Height),
                phase = (float)random.NextDouble() * 6.28f,
                size = random.Next(2, 5),
                speed = random.Next(7, 19),
                tint = tint,
            };
        }
    }

    private static Vector2 IdlePosition(Mote mote, float time)
    {
        return new Vector2(
            Mathf.Repeat(mote.position.x + Mathf.Sin(time * .24f + mote.phase) * 19f, Width),
            Mathf.Repeat(mote.position.y + time * mote.speed, Height));
    }

    private void Square(VertexHelper mesh, Vector2 center, float size, Color32 tint)
    {
        center = new Vector2(Mathf.Round(center.x), Mathf.Round(center.y));
        Vector2 half = Vector2.one * (size * .5f);
        Quad(mesh, center - half, center + new Vector2(-half.x, half.y), center + half, center + new Vector2(half.x, -half.y), tint);
    }

    private void Line(VertexHelper mesh, Vector2 a, Vector2 b, float width, Color32 tint)
    {
        Vector2 direction = (b - a).normalized;
        Vector2 side = new Vector2(-direction.y, direction.x) * (width * .5f);
        Quad(mesh, a - side, a + side, b + side, b - side, tint);
    }

    private void Quad(VertexHelper mesh, Vector2 a, Vector2 b, Vector2 c, Vector2 d, Color32 tint)
    {
        Rect rect = rectTransform.rect;
        Vector2 origin = rect.min;
        Vector2 scale = new(rect.width / Width, rect.height / Height);
        int start = mesh.currentVertCount;
        mesh.AddVert(origin + Vector2.Scale(a, scale), tint, Vector2.zero);
        mesh.AddVert(origin + Vector2.Scale(b, scale), tint, Vector2.zero);
        mesh.AddVert(origin + Vector2.Scale(c, scale), tint, Vector2.zero);
        mesh.AddVert(origin + Vector2.Scale(d, scale), tint, Vector2.zero);
        mesh.AddTriangle(start, start + 1, start + 2);
        mesh.AddTriangle(start, start + 2, start + 3);
    }
}
