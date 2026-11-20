using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(CanvasRenderer))]
public sealed class ArchiveGraphic : MaskableGraphic
{
    public enum Surface { Backdrop, Slot }
    [SerializeField] private Surface surface;
    [SerializeField] private bool occupied;
    [SerializeField] private bool focused;
    [SerializeField, Range(0f, 1f)] private float progress;
    [SerializeField] private int slotIndex;

    private static readonly Color Cyan = new(0f, .941f, 1f);
    private static readonly Color Violet = new(.878f, .714f, 1f);
    private static readonly Color Outline = new(.231f, .286f, .294f);

    public void Configure(Surface value, int index = 0)
    {
        surface = value;
        slotIndex = index;
        raycastTarget = value == Surface.Slot;
        SetVerticesDirty();
    }

    public void SetState(bool hasRecord, bool selected, float convergence)
    {
        occupied = hasRecord;
        focused = selected;
        progress = Mathf.Clamp01(convergence);
        SetVerticesDirty();
    }

    private void Update()
    {
        if (Application.isPlaying && (surface == Surface.Backdrop || focused))
            SetVerticesDirty();
    }

    protected override void OnPopulateMesh(VertexHelper mesh)
    {
        mesh.Clear();
        if (surface == Surface.Backdrop)
            Backdrop(mesh);
        else
            Slot(mesh);
    }

    private void Backdrop(VertexHelper mesh)
    {
        float time = Application.isPlaying ? Time.unscaledTime : 2f;
        Rect(mesh, 0, 0, 1920, 1080, new Color(.035f, .05f, .075f));
        Vector2 center = new(960f, 565f);
        for (int ring = 0; ring < 4; ring++)
        {
            float radius = 190f + ring * 88f;
            int count = ring % 2 == 0 ? 112 : 72;
            Color tint = ring % 2 == 0 ? Cyan : Violet;
            tint.a = ring % 2 == 0 ? .105f : .055f;
            for (int tick = 0; tick < count; tick++)
            {
                float angle = tick * Mathf.PI * 2f / count + time * (ring % 2 == 0 ? .014f : -.009f);
                Vector2 direction = new(Mathf.Cos(angle), Mathf.Sin(angle));
                float length = tick % 6 == 0 ? 7f : 2f;
                Line(mesh, center + direction * radius, center + direction * (radius + length), 1.4f, tint);
                if (ring == 2)
                {
                    float end = angle + .035f;
                    Line(mesh, center + direction * radius, center + new Vector2(Mathf.Cos(end), Mathf.Sin(end)) * radius, .8f, tint * .55f);
                }
            }
        }
        for (int index = 0; index < 22; index++)
        {
            float x = Mathf.Repeat(index * 347.37f + Mathf.Sin(time * .16f + index) * 28f, 1870f) + 25f;
            float y = Mathf.Repeat(index * 197.31f - time * (3f + index % 4), 1000f) + 40f;
            Color tint = index % 3 == 0 ? Violet : Cyan;
            tint.a = .12f + Mathf.Sin(time * .7f + index) * .07f;
            Rect(mesh, Mathf.Round(x), Mathf.Round(y), index % 4 == 0 ? 4 : 2, 3, tint);
        }
        for (int y = 0; y < 1080; y += 3)
            Rect(mesh, 0, y, 1920, 1, new Color(0, 0, 0, .16f));
        Rect(mesh, 32, 71, 1856, 1, new Color(.2f, .35f, .4f, .35f));
        Rect(mesh, 32, 1014, 1856, 1, new Color(.2f, .35f, .4f, .35f));
    }

    private void Slot(VertexHelper mesh)
    {
        float time = Application.isPlaying ? Time.unscaledTime : 2f;
        Color accent = occupied && slotIndex % 2 == 1 ? Violet : Cyan;
        Color border = focused ? accent : occupied ? WithAlpha(accent, .38f) : Outline;
        Rect(mesh, 5, 6, 512, 500, new Color(0, 0, 0, .65f));
        Rect(mesh, 0, 0, 512, 500, focused ? new Color(.042f, .079f, .10f, .98f) : new Color(.046f, .061f, .084f, .96f));
        if (occupied || focused)
            Frame(mesh, 0, 0, 512, 500, focused ? 2 : 1, border);
        else
        {
            for (int x = 0; x < 512; x += 12)
            {
                Rect(mesh, x, 0, 6, 1, border);
                Rect(mesh, x, 499, 6, 1, border);
            }
            for (int y = 0; y < 500; y += 12)
            {
                Rect(mesh, 0, y, 1, 6, border);
                Rect(mesh, 511, y, 1, 6, border);
            }
        }
        foreach (Vector2 corner in new[] { Vector2.zero, new Vector2(512, 0), new Vector2(0, 500), new Vector2(512, 500) })
            Rect(mesh, corner.x - 4, corner.y - 4, 8, 8, border);
        Rect(mesh, 24, 67, 464, 1, WithAlpha(border, .65f));
        if (focused)
        {
            Rect(mesh, 140, -20, 232, 19, WithAlpha(accent, .85f));
            Frame(mesh, -4, -4, 520, 508, 1, WithAlpha(accent, .13f + .05f * Mathf.Sin(time * 3f)));
        }
        if (occupied)
        {
            Rect(mesh, 24, 86, 464, 98, new Color(.025f, .039f, .055f));
            Frame(mesh, 32, 97, 66, 76, 1, WithAlpha(accent, .52f));
            Rect(mesh, 24, 229, 464, 32, new Color(.11f, .14f, .18f, .55f));
            Rect(mesh, 24, 229, 2, 32, accent);
            Frame(mesh, 24, 356, 464, 20, 1, Outline);
            for (int index = 0; index < 10; index++)
                Rect(mesh, 29 + index * 46, 361, 41, 10, WithAlpha(accent, index < Mathf.RoundToInt(progress * 10) ? .95f : .10f));
        }
        else
        {
            Frame(mesh, 24, 90, 464, 205, 1, WithAlpha(Outline, .5f));
            Frame(mesh, 227, 132, 58, 58, 1, WithAlpha(accent, focused ? .7f : .35f));
            Rect(mesh, 226, 131, 5, 5, accent);
            Rect(mesh, 281, 186, 5, 5, accent);
            float alpha = focused ? .75f + .2f * Mathf.Sin(time * 2.8f) : .5f;
            Rect(mesh, 247, 160, 18, 2, WithAlpha(accent, alpha));
            Rect(mesh, 255, 152, 2, 18, WithAlpha(accent, alpha));
        }
        Rect(mesh, 24, 388, 464, 1, WithAlpha(Outline, .6f));
        Rect(mesh, 28, 447, 464, 36, Color.black);
        Rect(mesh, 24, 443, 464, 36, focused ? accent : new Color(.12f, .15f, .19f));
        Frame(mesh, 24, 443, 464, 36, 1, WithAlpha(accent, focused ? 1f : .4f));
        if (focused)
            for (int y = 446; y < 479; y += 3)
                Rect(mesh, 25, y, 462, 1, new Color(0, 0, 0, .1f));
    }

    private static Color WithAlpha(Color tint, float alpha) { tint.a = alpha; return tint; }

    private void Frame(VertexHelper mesh, float x, float y, float width, float height, float size, Color tint)
    {
        Rect(mesh, x, y, width, size, tint);
        Rect(mesh, x, y + height - size, width, size, tint);
        Rect(mesh, x, y, size, height, tint);
        Rect(mesh, x + width - size, y, size, height, tint);
    }

    private void Rect(VertexHelper mesh, float x, float y, float width, float height, Color tint)
    {
        float designWidth = surface == Surface.Backdrop ? 1920f : 512f;
        float designHeight = surface == Surface.Backdrop ? 1080f : 500f;
        Rect area = rectTransform.rect;
        Vector2 scale = new(area.width / designWidth, area.height / designHeight);
        Vector2 a = new(area.xMin + x * scale.x, area.yMax - (y + height) * scale.y);
        Vector2 b = new(area.xMin + (x + width) * scale.x, area.yMax - y * scale.y);
        int first = mesh.currentVertCount;
        mesh.AddVert(a, tint, Vector2.zero);
        mesh.AddVert(new Vector2(a.x, b.y), tint, Vector2.zero);
        mesh.AddVert(b, tint, Vector2.zero);
        mesh.AddVert(new Vector2(b.x, a.y), tint, Vector2.zero);
        mesh.AddTriangle(first, first + 1, first + 2);
        mesh.AddTriangle(first, first + 2, first + 3);
    }

    private void Line(VertexHelper mesh, Vector2 a, Vector2 b, float width, Color tint)
    {
        Vector2 side = new Vector2(-(b - a).y, (b - a).x).normalized * width * .5f;
        Rect area = rectTransform.rect;
        int first = mesh.currentVertCount;
        Vector2 scale = new(area.width / 1920f, -area.height / 1080f);
        Vector2 origin = new(area.xMin, area.yMax);
        mesh.AddVert(origin + Vector2.Scale(a - side, scale), tint, Vector2.zero);
        mesh.AddVert(origin + Vector2.Scale(a + side, scale), tint, Vector2.zero);
        mesh.AddVert(origin + Vector2.Scale(b + side, scale), tint, Vector2.zero);
        mesh.AddVert(origin + Vector2.Scale(b - side, scale), tint, Vector2.zero);
        mesh.AddTriangle(first, first + 1, first + 2);
        mesh.AddTriangle(first, first + 2, first + 3);
    }
}
