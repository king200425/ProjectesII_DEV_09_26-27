using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(CanvasRenderer))]
// 光速转场粒子。运动只改变离中心的距离，方向固定，所以不会旋转成漩涡。
public sealed class ChronoTunnelParticles : MaskableGraphic
{
    private struct Particle
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
    private Particle[] particles;
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
        // 淡入淡出控制整体亮度，不改变每个粒子的运动方向。
        float envelope = Mathf.SmoothStep(0f, 1f, cover / .26f);
        if (envelope < .001f)
            return;
        Initialize();
        Rect rect = rectTransform.rect;
        float pixelScale = rect.height / 1080f;
        Vector2 focusUV = new Vector2(0.5f, 0.5f);
        if (!revealing)
            focusUV = Vector2.Lerp(departureUV, focusUV, Mathf.SmoothStep(0f, 1f, cover));
        Vector2 focus = rect.min + Vector2.Scale(focusUV, rect.size);
        if (revealing)
            envelope *= cover * cover;

        for (int index = 0; index < particles.Length; index++)
        {
            Particle mote = particles[index];
            float depth = Mathf.Repeat(mote.phase + travel * mote.speed, 1f);
            // depth 循环从 0 到 1，表示从远处接近镜头的过程。
            float fadeIn = Mathf.SmoothStep(0f, 1f, depth / 0.12f);
            float fadeOut = 1f - Mathf.SmoothStep(0f, 1f, (depth - 0.88f) / 0.12f);
            float opacity = envelope * fadeIn * fadeOut;
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
                    UiParticleDrawing.Line(mesh, previous, next, pixelScale * (1f + depth * 1.5f), trailTint);
                    trailTint.a *= .1f;
                    UiParticleDrawing.Line(mesh, previous, next, pixelScale * (7f + depth * 8f), trailTint);
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
                UiParticleDrawing.Square(mesh, position, dot * 11f, halo);
                foreach (Vector2 pixel in HourglassPixels)
                    UiParticleDrawing.Square(mesh, position + pixel * dot, dot, tint);
            }
        }

    }

    private static float Radius(float depth)
    {
        // 距离增长越来越快，产生接近镜头时突然掠过的感觉。
        return 0.018f * Mathf.Exp(depth * 4.6f);
    }

    private static Vector2 Point(Particle mote, float depth, Vector2 focus, float height)
    {
        float radius = Radius(depth);
        float angle = mote.angle;
        return focus + new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * (radius * height);
    }

    private void Initialize()
    {
        if (particles != null)
            return;
        var random = new System.Random(11731);
        particles = new Particle[192];
        for (int index = 0; index < particles.Length; index++)
        {
            Color tint = new Color(0.05f, 0.91f, 1f);
            if (index % 4 == 0)
                tint = new Color(0.72f, 0.38f, 1f);
            else if (index % 7 == 0)
                tint = new Color(0.76f, 0.95f, 1f);

            particles[index] = new Particle
            {
                phase = (float)random.NextDouble(),
                angle = (float)random.NextDouble() * Mathf.PI * 2f,
                speed = .35f + (float)random.NextDouble() * .3f,
                size = random.Next(2, 5),
                tint = tint,
            };
        }
    }

    private static void GlowPixel(VertexHelper mesh, Vector2 position, float size, Color tint)
    {
        Color halo = tint;
        halo.a *= .065f;
        UiParticleDrawing.Square(mesh, position, size * 3f, halo);
        UiParticleDrawing.Square(mesh, position, size, tint);
        if (size >= 4f)
            UiParticleDrawing.Square(mesh, position, Mathf.Max(1f, size * .4f), new Color(.82f, 1f, 1f, tint.a * .85f));
    }

}
