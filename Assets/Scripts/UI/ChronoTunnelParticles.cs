using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(CanvasRenderer))]
public sealed class ChronoTunnelParticles : MaskableGraphic
{
    private struct Chronon
    {
        public float phase;
        public float angle;
        public float speed;
        public float size;
        public Color tint;
    }

    private static readonly Vector2[] HourglassPixels =
    {
        new(-2, 2), new(-1, 2), new(0, 2), new(1, 2), new(2, 2),
        new(-1, 1), new(1, 1), new(0, 0), new(-1, -1), new(1, -1),
        new(-2, -2), new(-1, -2), new(0, -2), new(1, -2), new(2, -2),
    };
    private Chronon[] chronons;
    private float cover;
    private bool revealing;
    private float travel;
    private Vector2 departureUV;

    public void SetPhase(float coverage, bool exit, float distance, Vector2 origin)
    {
        cover = Mathf.Clamp01(coverage);
        revealing = exit;
        travel = distance;
        departureUV = origin;
        raycastTarget = false;
        SetVerticesDirty();
    }

    protected override void OnPopulateMesh(VertexHelper mesh)
    {
        mesh.Clear();
        float envelope = Mathf.SmoothStep(0f, 1f, cover / .26f);
        if (envelope < .001f)
            return;
        Initialize();
        Rect rect = rectTransform.rect;
        float pixelScale = rect.height / 1080f;
        Vector2 focusUV = revealing ? new Vector2(.5f, .5f) : Vector2.Lerp(departureUV, new Vector2(.5f, .5f), Mathf.SmoothStep(0f, 1f, cover));
        Vector2 focus = rect.min + Vector2.Scale(focusUV, rect.size);
        if (revealing)
            envelope *= cover * cover;

        for (int index = 0; index < chronons.Length; index++)
        {
            Chronon mote = chronons[index];
            float depth = Mathf.Repeat(mote.phase + travel * mote.speed, 1f);
            float opacity = envelope * Mathf.SmoothStep(0f, 1f, depth / .12f) * (1f - Mathf.SmoothStep(0f, 1f, (depth - .88f) / .12f));
            if (opacity < .008f)
                continue;
            Color tint = mote.tint;
            tint.a = opacity * .78f;
            Vector2 position = Point(mote, depth, focus, rect.height);
            float size = Mathf.Round((mote.size + depth * depth * 4f) * pixelScale);
            size = Mathf.Max(pixelScale * 2f, size);
            if (index < 144)
            {
                // Perspective acceleration pulls each point into a straight streak.
                float tail = (.045f + depth * .09f) * Mathf.Lerp(.5f, 1.7f, revealing ? 1f : cover);
                Vector2 previous = Point(mote, Mathf.Max(0f, depth - tail), focus, rect.height);
                for (int segment = 1; segment <= 3; segment++)
                {
                    float trailDepth = Mathf.Lerp(Mathf.Max(0f, depth - tail), depth, segment / 3f);
                    Vector2 next = Point(mote, trailDepth, focus, rect.height);
                    Color trailTint = tint;
                    trailTint.a *= segment / 3f * .52f;
                    Line(mesh, previous, next, pixelScale * (1f + depth * 1.5f), trailTint);
                    trailTint.a *= .1f;
                    Line(mesh, previous, next, pixelScale * (7f + depth * 8f), trailTint);
                    previous = next;
                }
                GlowPixel(mesh, position, size, tint);
                // A displaced color echo follows only a few of the brighter particles.
                if (index % 8 == 0)
                {
                    Color echo = new(.72f, .3f, 1f, tint.a * .26f);
                    GlowPixel(mesh, position + new Vector2(-size * 2f, size), size * .7f, echo);
                }
            }
            else if (index < 184)
            {
                GlowPixel(mesh, position, Mathf.Max(pixelScale * 2f, size * .6f), tint);
            }
            else
            {
                // Sparse hourglass glyphs make the particle field read as displaced time.
                float dot = Mathf.Max(pixelScale, Mathf.Round((1.2f + depth * 1.2f) * pixelScale));
                Color halo = tint;
                halo.a *= .08f;
                Square(mesh, position, dot * 11f, halo);
                foreach (Vector2 pixel in HourglassPixels)
                    Square(mesh, position + pixel * dot, dot, tint);
            }
        }

    }

    private static float Radius(float depth) => .018f * Mathf.Exp(depth * 4.6f);

    private static Vector2 Point(Chronon mote, float depth, Vector2 focus, float height)
    {
        float radius = Radius(depth);
        float angle = mote.angle;
        return focus + new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * (radius * height);
    }

    private void Initialize()
    {
        if (chronons != null)
            return;
        var random = new System.Random(11731);
        chronons = new Chronon[192];
        for (int index = 0; index < chronons.Length; index++)
        {
            chronons[index] = new Chronon
            {
                phase = (float)random.NextDouble(),
                angle = (float)random.NextDouble() * Mathf.PI * 2f,
                speed = .35f + (float)random.NextDouble() * .3f,
                size = random.Next(2, 5),
                tint = index % 4 == 0 ? new Color(.72f, .38f, 1f) : index % 7 == 0 ? new Color(.76f, .95f, 1f) : new Color(.05f, .91f, 1f),
            };
        }
    }

    private static void GlowPixel(VertexHelper mesh, Vector2 position, float size, Color tint)
    {
        Color halo = tint;
        halo.a *= .065f;
        Square(mesh, position, size * 3f, halo);
        Square(mesh, position, size, tint);
        if (size >= 4f)
            Square(mesh, position, Mathf.Max(1f, size * .4f), new Color(.82f, 1f, 1f, tint.a * .85f));
    }

    private static void Square(VertexHelper mesh, Vector2 center, float size, Color tint)
    {
        center = new Vector2(Mathf.Round(center.x), Mathf.Round(center.y));
        Vector2 half = Vector2.one * (size * .5f);
        Quad(mesh, center - half, center + new Vector2(-half.x, half.y), center + half, center + new Vector2(half.x, -half.y), tint);
    }

    private static void Line(VertexHelper mesh, Vector2 a, Vector2 b, float width, Color tint)
    {
        Vector2 direction = (b - a).normalized;
        Vector2 side = new(-direction.y * width * .5f, direction.x * width * .5f);
        Quad(mesh, a - side, a + side, b + side, b - side, tint);
    }

    private static void Quad(VertexHelper mesh, Vector2 a, Vector2 b, Vector2 c, Vector2 d, Color32 tint)
    {
        int first = mesh.currentVertCount;
        mesh.AddVert(a, tint, Vector2.zero);
        mesh.AddVert(b, tint, Vector2.zero);
        mesh.AddVert(c, tint, Vector2.zero);
        mesh.AddVert(d, tint, Vector2.zero);
        mesh.AddTriangle(first, first + 1, first + 2);
        mesh.AddTriangle(first, first + 2, first + 3);
    }
}
