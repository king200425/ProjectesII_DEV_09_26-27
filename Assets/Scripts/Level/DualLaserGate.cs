using UnityEngine;

public class DualLaserGate : MonoBehaviour
{
    public LaserReceiver blue;
    public LaserReceiver yellow;
    public GameObject laserRoot;
    public float window = 0.7f;

    private float blueTime = -99f;
    private float yellowTime = -99f;
    private bool solved;

    public void Register(LaserReceiver receiver)
    {
        if (solved) return;
        if (receiver == blue) blueTime = Time.time;
        if (receiver == yellow) yellowTime = Time.time;
    }

    void Update()
    {
        if (solved) return;
        if (Mathf.Abs(blueTime - yellowTime) <= window &&
            Time.time - blueTime <= window &&
            Time.time - yellowTime <= window &&
            blueTime > 0f && yellowTime > 0f)
        {
            solved = true;
            if (laserRoot != null) laserRoot.SetActive(false);
        }
    }
}
