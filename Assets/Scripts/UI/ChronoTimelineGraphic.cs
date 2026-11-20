using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(CanvasRenderer))]
// 把底部时间轴的线和节点画在一个对象中，减少 Hierarchy 中的零散对象。
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
            Vector2 a = center + (Vector2)(rotation * new Vector3(-half.x, -half.y, 0f));
            Vector2 b = center + (Vector2)(rotation * new Vector3(-half.x, half.y, 0f));
            Vector2 c = center + (Vector2)(rotation * new Vector3(half.x, half.y, 0f));
            Vector2 d = center + (Vector2)(rotation * new Vector3(half.x, -half.y, 0f));
            UiParticleDrawing.Quad(mesh, a, b, c, d, tint);
        }
    }
}
