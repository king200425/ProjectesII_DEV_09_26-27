using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(CanvasRenderer))]
public sealed class ChronoTimelineGraphic : MaskableGraphic
{
    [System.Serializable]
    public struct Segment
    {
        public Vector2 anchorMin;
        public Vector2 anchorMax;
        public float rotation;
        public Color tint;
    }

    [SerializeField] private Segment[] segments = System.Array.Empty<Segment>();

    public void SetSegments(Segment[] value)
    {
        segments = value;
        SetVerticesDirty();
    }

    protected override void OnPopulateMesh(VertexHelper mesh)
    {
        mesh.Clear();
        Rect rect = rectTransform.rect;
        foreach (Segment segment in segments)
        {
            Vector2 min = rect.min + Vector2.Scale(segment.anchorMin, rect.size);
            Vector2 max = rect.min + Vector2.Scale(segment.anchorMax, rect.size);
            Vector2 center = (min + max) * .5f;
            Vector2 half = (max - min) * .5f;
            Quaternion rotation = Quaternion.Euler(0f, 0f, segment.rotation);
            Color32 tint = segment.tint * color;
            int first = mesh.currentVertCount;
            mesh.AddVert(center + (Vector2)(rotation * new Vector3(-half.x, -half.y, 0f)), tint, Vector2.zero);
            mesh.AddVert(center + (Vector2)(rotation * new Vector3(-half.x, half.y, 0f)), tint, Vector2.zero);
            mesh.AddVert(center + (Vector2)(rotation * new Vector3(half.x, half.y, 0f)), tint, Vector2.zero);
            mesh.AddVert(center + (Vector2)(rotation * new Vector3(half.x, -half.y, 0f)), tint, Vector2.zero);
            mesh.AddTriangle(first, first + 1, first + 2);
            mesh.AddTriangle(first, first + 2, first + 3);
        }
    }
}
