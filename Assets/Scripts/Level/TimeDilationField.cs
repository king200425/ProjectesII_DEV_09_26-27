using UnityEngine;

public class TimeDilationField : MonoBehaviour
{
    public float radius = 2.8f;
    public float dilationScale = 0.35f;

    void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(0f, 0.9f, 1f, 0.7f);
        Gizmos.DrawWireSphere(transform.position, radius);
    }
}
