using UnityEngine;

public class PlayerFallDeath : MonoBehaviour
{
    public float deathY = -20f;

    void Update()
    {
        if (transform.position.y <= deathY)
        {
            PlayerHealth health = GetComponent<PlayerHealth>();
            if (health != null)
                health.Die();
        }
    }
}
