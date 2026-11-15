using UnityEngine;

public class TimeSwitch : MonoBehaviour
{
    public float activeWindow = 3f;
    public Color activeColor = Color.red;
    public Color ringColor = Color.red;
    public float ringRadius = 1.15f;
    public float ringWidth = 0.12f;
    public int ringSegments = 64;

    public bool IsActive { get; private set; }

    private float timer;
    private SpriteRenderer spriteRenderer;
    private Color startColor;
    private LineRenderer ring;

    void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();

        if (spriteRenderer != null)
            startColor = spriteRenderer.color;

        activeColor = Color.red;
        ringColor = Color.red;
        CreateRing();
    }

    void CreateRing()
    {
        GameObject ringObject = new GameObject("CountdownRing");
        ringObject.transform.SetParent(transform, false);
        ringObject.transform.localPosition = Vector3.zero;

        ring = ringObject.AddComponent<LineRenderer>();
        ring.useWorldSpace = false;
        ring.loop = false;
        ring.positionCount = ringSegments + 1;
        ring.startWidth = ringWidth;
        ring.endWidth = ringWidth;
        ring.numCapVertices = 4;
        ring.numCornerVertices = 4;
        ring.alignment = LineAlignment.View;
        ring.textureMode = LineTextureMode.Stretch;
        ring.material = new Material(Shader.Find("Sprites/Default"));
        ring.startColor = ringColor;
        ring.endColor = ringColor;
        ring.sortingOrder = 20;
        ring.enabled = false;
    }

    public void Activate()
    {
        timer = activeWindow;
        IsActive = true;

        if (spriteRenderer != null)
            spriteRenderer.color = activeColor;

        if (ring != null)
        {
            ring.startColor = ringColor;
            ring.endColor = ringColor;
            ring.enabled = true;
            UpdateRing();
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.GetComponent<Bullet>() == null)
            return;

        Destroy(other.gameObject);
        Activate();
    }

    void Update()
    {
        if (!IsActive)
            return;

        timer -= Time.deltaTime * TimeSlowPrototype.WorldScale;

        if (timer <= 0f)
        {
            timer = 0f;
            IsActive = false;

            if (spriteRenderer != null)
                spriteRenderer.color = startColor;

            if (ring != null)
                ring.enabled = false;

            return;
        }

        UpdateRing();
    }

    void UpdateRing()
    {
        if (ring == null)
            return;

        float fraction = Mathf.Clamp01(timer / activeWindow);

        if (fraction <= 0f)
        {
            ring.enabled = false;
            return;
        }

        int points = Mathf.Max(
            2,
            Mathf.CeilToInt(ringSegments * fraction) + 1);

        ring.positionCount = points;

        for (int i = 0; i < points; i++)
        {
            float step = points == 1 ? 0f : (float)i / (points - 1);
            float angle = Mathf.PI * 0.5f -
                Mathf.PI * 2f * fraction * step;
            float x = Mathf.Cos(angle) * ringRadius;
            float y = Mathf.Sin(angle) * ringRadius;
            ring.SetPosition(i, new Vector3(x, y, 0f));
        }
    }

    void OnDisable()
    {
        IsActive = false;

        if (spriteRenderer != null)
            spriteRenderer.color = startColor;

        if (ring != null)
            ring.enabled = false;
    }
}
