using UnityEngine;

public class SpikeTrapAnim : MonoBehaviour
{
    public Sprite[] frames;
    public float framesPerSecond = 8f;
    public Collider2D hazardCollider;
    public BridgeDoor bridge;

    private SpriteRenderer sr;
    private int index;

    void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
    }

    void Update()
    {
        if (sr == null || frames == null || frames.Length == 0)
            return;

        if (bridge != null && bridge.IsOpen)
        {
            if (hazardCollider != null) hazardCollider.enabled = false;
            sr.sprite = frames[0];
            return;
        }

        index = Mathf.FloorToInt(Time.time * framesPerSecond) % frames.Length;
        sr.sprite = frames[index];

        // Only the fully extended frames are dangerous.
        if (hazardCollider != null)
            hazardCollider.enabled = (index == 4 || index == 5);
    }
}
