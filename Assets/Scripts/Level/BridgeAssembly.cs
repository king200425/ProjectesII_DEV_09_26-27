using UnityEngine;

public class BridgeAssembly : MonoBehaviour
{
    public BridgeDoor door;
    public Transform[] pieces;
    public Collider2D[] colliders;
    public float delay = 0.12f;
    public float riseDuration = 0.5f;

    private bool started;
    private bool complete;

    void Awake()
    {
        if (pieces != null)
        {
            foreach (Transform piece in pieces)
            {
                if (piece != null) piece.gameObject.SetActive(false);
            }
        }
        SetColliders(false);
    }

    void Update()
    {
        if (door == null || !door.IsOpen || started)
            return;

        started = true;
        for (int i = 0; i < pieces.Length; i++)
        {
            if (pieces[i] == null) continue;
            StartCoroutine(RisePiece(pieces[i], i * delay));
        }

        Invoke(nameof(EnableColliders), pieces.Length * delay + riseDuration);
    }

    System.Collections.IEnumerator RisePiece(Transform piece, float wait)
    {
        if (wait > 0f) yield return new WaitForSeconds(wait);

        Vector3 target = piece.localPosition;
        Vector3 from = target + Vector3.down * 4f;
        piece.localPosition = from;
        piece.gameObject.SetActive(true);

        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime / riseDuration;
            piece.localPosition = Vector3.Lerp(from, target, Mathf.SmoothStep(0f, 1f, t));
            yield return null;
        }
        piece.localPosition = target;
    }

    void EnableColliders()
    {
        complete = true;
        SetColliders(true);
    }

    void SetColliders(bool active)
    {
        if (colliders == null) return;
        foreach (Collider2D c in colliders)
            if (c != null) c.enabled = active;
    }
}
