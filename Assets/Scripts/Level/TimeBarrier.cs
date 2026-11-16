using UnityEngine;

public class TimeBarrier : MonoBehaviour
{
    public Collider2D barrier;
    public SpriteRenderer visual;

    void Update()
    {
        bool open = TimeSlowPrototype.IsSlowed;

        if (barrier != null)
            barrier.enabled = !open;

        if (visual != null)
        {
            Color c = visual.color;
            c.a = open ? 0.18f : 0.72f;
            visual.color = c;
        }
    }
}
