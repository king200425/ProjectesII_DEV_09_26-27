using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(CanvasRenderer))]
public sealed class AftertimeMenuParticles : MaskableGraphic
{
    private const float Width = 1851f;
    private const float Height = 1082f;
    private const float Circle = 2f * Mathf.PI;
    private static readonly Color32[] Palette =
    {
        new(0, 240, 255, 255), new(76, 201, 240, 255), new(181, 23, 158, 255),
        new(114, 9, 183, 255), new(219, 252, 255, 255), new(224, 182, 255, 255),
        new(255, 255, 255, 255),
    };

    private struct Mote
    {
        public Vector2 position;
        public float radius;
        public float speed;
        public float angle;
        public Color32 tint;
    }

    private Mote[] motes;

    private void Update() => SetVerticesDirty();

    protected override void OnPopulateMesh(VertexHelper vertices)
    {
        vertices.Clear();
        motes ??= CreateMotes();

        float time = Application.isPlaying ? Time.unscaledTime : 1.8f;
        Vector2 center = new(Width * .5f, Height * .74f);
        for (int i = 0; i < 90; i++)
        {
            Mote mote = motes[i];
            float x = Mathf.Repeat(mote.position.x + mote.angle * time * 8f + Mathf.Sin(time * 2f + i) * 10f, Width);
            float y = Mathf.Repeat(mote.position.y + mote.speed * time, Height);
            Square(vertices, new Vector2(Mathf.Round(x), Mathf.Round(y)), mote.radius, mote.angle + time * (i % 3 - 1), mote.tint);
        }

        for (int i = 90; i < motes.Length; i++)
        {
            Mote mote = motes[i];
            float distance = 14f + Mathf.Repeat(mote.position.x - time * mote.speed * 3f, 460f);
            float angle = mote.angle + time * (.12f + (i % 9) * .012f);
            Vector2 direction = Polar(angle);
            Vector2 point = center + direction * distance;
            Segment(vertices, point, point + direction * (7f + mote.radius * 3f), mote.radius, mote.tint);
            Square(vertices, point, mote.radius + 1f, 0f, mote.tint);
        }

        for (int i = 0; i < 16; i++)
        {
            Vector2 point = center + Polar(i * Circle / 16f + time * .12f) * 168f;
            Square(vertices, point, 12f, 0f, Palette[i % 2 == 0 ? 3 : 0]);
        }

        for (int echo = 3; echo >= 0; echo--)
        {
            float phase = time - echo * .15f;
            for (int i = 0; i < 6; i++)
            {
                Vector2 point = center + Polar(i * Circle / 6f + phase * .18f) * (90f + Mathf.Sin(phase * 2f + i) * 12f);
                Color32 tint = Palette[echo == 0 && i % 2 == 0 ? 0 : 5];
                tint.a = (byte)(echo == 0 ? 220 : 90 - 16 * echo);
                Segment(vertices, center, point, echo == 0 ? 1.75f : 1f, tint);
                Square(vertices, point, echo == 0 ? 8f : 5f, 0f, tint);
            }
        }

        float scan = Mathf.Repeat(time / 4.8f, 1f);
        DrawTimeScale(vertices, 463f, scan, Palette[0]);
        DrawTimeScale(vertices, 1202f, 1f - scan, Palette[5]);
    }

    private void DrawTimeScale(VertexHelper vertices, float start, float scan, Color32 tint)
    {
        const float y = Height - 535f;
        const int divisions = 12;
        const float spacing = 15.5f;
        float cursor = start + divisions * spacing * scan;

        tint.a = 65;
        Segment(vertices, new Vector2(start, y), new Vector2(start + divisions * spacing, y), 2f, tint);

        for (int i = 0; i <= divisions; i++)
        {
            float x = start + i * spacing;
            bool major = i % 3 == 0;
            float length = major ? 20f : 8f;
            float highlight = 1f - Mathf.Clamp01(Mathf.Abs(x - cursor) / (2f * spacing));
            tint.a = (byte)((major ? 132f : 83f) + highlight * 70f);
            Segment(vertices, new Vector2(x, y - length * .5f), new Vector2(x, y + length * .5f), 2f, tint);
        }

        float fade = Mathf.Clamp01(Mathf.Min(scan, 1f - scan) * 12f);
        tint.a = (byte)(248f * fade);
        Square(vertices, new Vector2(Mathf.Round(cursor), y), 7f, Mathf.PI * .25f, tint);
    }

    private static Vector2 Polar(float angle) => new(Mathf.Cos(angle), Mathf.Sin(angle));

    private static Mote[] CreateMotes()
    {
        var random = new System.Random(307);
        var result = new Mote[126];
        for (int i = 0; i < result.Length; i++)
        {
            Color32 tint = Palette[random.Next(Palette.Length)];
            tint.a = (byte)random.Next(45, i < 90 ? 215 : 165);
            result[i] = new Mote
            {
                position = new Vector2((float)random.NextDouble() * Width, (float)random.NextDouble() * Height),
                radius = i < 90 ? random.Next(2, 5) * 2 : random.Next(2, 5),
                speed = random.Next(17, 56),
                angle = (float)random.NextDouble() * Circle,
                tint = tint,
            };
        }
        return result;
    }

    private void Square(VertexHelper vertices, Vector2 center, float size, float angle, Color32 tint)
    {
        Vector2 x = Polar(angle) * (size * .5f);
        Vector2 y = new(-x.y, x.x);
        Quad(vertices, center - x - y, center - x + y, center + x + y, center + x - y, tint);
    }

    private void Segment(VertexHelper vertices, Vector2 a, Vector2 b, float width, Color32 tint)
    {
        Vector2 direction = (b - a).normalized;
        Vector2 perpendicular = new Vector2(-direction.y, direction.x) * (width * .5f);
        Quad(vertices, a - perpendicular, a + perpendicular, b + perpendicular, b - perpendicular, tint);
    }

    private void Quad(VertexHelper vertices, Vector2 a, Vector2 b, Vector2 c, Vector2 d, Color32 tint)
    {
        int index = vertices.currentVertCount;
        Rect rect = rectTransform.rect;
        vertices.AddVert(new Vector2(rect.xMin + a.x / Width * rect.width, rect.yMin + a.y / Height * rect.height), tint, Vector2.zero);
        vertices.AddVert(new Vector2(rect.xMin + b.x / Width * rect.width, rect.yMin + b.y / Height * rect.height), tint, Vector2.zero);
        vertices.AddVert(new Vector2(rect.xMin + c.x / Width * rect.width, rect.yMin + c.y / Height * rect.height), tint, Vector2.zero);
        vertices.AddVert(new Vector2(rect.xMin + d.x / Width * rect.width, rect.yMin + d.y / Height * rect.height), tint, Vector2.zero);
        vertices.AddTriangle(index, index + 1, index + 2);
        vertices.AddTriangle(index, index + 2, index + 3);
    }
}
