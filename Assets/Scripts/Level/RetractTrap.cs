using UnityEngine;

public class RetractTrap : MonoBehaviour
{
    public Transform spikes;
    public float cycle = 2f;
    public float extendedHeight = 0.8f;
    private float timer;

    void Update()
    {
        if (spikes == null) return;
        timer += Time.deltaTime;
        float t = Mathf.PingPong(timer / cycle, 1f);
        Vector3 p = spikes.localPosition;
        p.y = Mathf.Lerp(0f, extendedHeight, t);
        spikes.localPosition = p;
    }
}
