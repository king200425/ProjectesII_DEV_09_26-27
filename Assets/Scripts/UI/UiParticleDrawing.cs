using UnityEngine;
using UnityEngine.UI;

// 粒子绘制共用的小工具。只负责画形状，不负责粒子怎么运动。
public static class UiParticleDrawing
{
    public static void Square(VertexHelper mesh, Vector2 center, float size, Color32 color)
    {
        // 方块中心对齐像素，避免像素特效看起来发虚。
        center.x = Mathf.Round(center.x);
        center.y = Mathf.Round(center.y);
        Vector2 half = Vector2.one * size * 0.5f;
        Quad(mesh, center - half, center + new Vector2(-half.x, half.y),
            center + half, center + new Vector2(half.x, -half.y), color);
    }

    public static void Line(VertexHelper mesh, Vector2 start, Vector2 end, float width, Color32 color)
    {
        // 在线段两侧各加一半宽度，就得到一个细长的矩形。
        Vector2 direction = (end - start).normalized;
        Vector2 side = new Vector2(-direction.y, direction.x) * width * 0.5f;
        Quad(mesh, start - side, start + side, end + side, end - side, color);
    }

    public static void Quad(VertexHelper mesh, Vector2 a, Vector2 b, Vector2 c, Vector2 d, Color32 color)
    {
        // Unity 用两个三角形组成一个四边形，这段固定写法集中在这里。
        int first = mesh.currentVertCount;
        mesh.AddVert(a, color, Vector2.zero);
        mesh.AddVert(b, color, Vector2.zero);
        mesh.AddVert(c, color, Vector2.zero);
        mesh.AddVert(d, color, Vector2.zero);
        mesh.AddTriangle(first, first + 1, first + 2);
        mesh.AddTriangle(first, first + 2, first + 3);
    }

    public static void FitToRect(VertexHelper mesh, Rect rect, float designWidth, float designHeight)
    {
        // 把设计图上的坐标换成当前 UI 大小下的坐标，窗口缩放后位置仍然正确。
        float scaleX = rect.width / designWidth;
        float scaleY = rect.height / designHeight;
        UIVertex vertex = new UIVertex();
        for (int i = 0; i < mesh.currentVertCount; i++)
        {
            mesh.PopulateUIVertex(ref vertex, i);
            Vector3 position = vertex.position;
            position.x = rect.xMin + position.x * scaleX;
            position.y = rect.yMin + position.y * scaleY;
            vertex.position = position;
            mesh.SetUIVertex(vertex, i);
        }
    }
}
