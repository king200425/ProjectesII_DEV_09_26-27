using System.Collections;
using System.Diagnostics;
using UnityEngine;

public class LaserEmitter : MonoBehaviour
{
    [Header("Trap Root")]
    public GameObject trapRoot;

    [Header("References")]
    public GameObject emitterVisual;
    public GameObject laserBeam;
    public GameObject laserParticles;
    public GameObject explosion;

    [Header("Health")]
    public int maxHealth = 6;

    [Header("Hit Flash")]
    public Color hitColor = Color.white;
    public float flashDuration = 0.1f;

    [Header("Explosion")]
    public float explosionDuration = 0.5f;

    [Header("Ghost Settings")]
    public bool ghostOnly = false;

    private int currentHealth;
    private bool isDestroyed = false;

    private SpriteRenderer emitterSprite;
    private Color originalColor;
    private Coroutine flashCoroutine;

    void Start()
    {
        currentHealth = Mathf.Max(1, maxHealth);

        if (emitterVisual != null)
        {
            emitterSprite =
                emitterVisual.GetComponent<SpriteRenderer>();

            if (emitterSprite == null)
            {
                emitterSprite =
                    emitterVisual.GetComponentInChildren<SpriteRenderer>();
            }

            if (emitterSprite != null)
                originalColor = emitterSprite.color;
        }

        if (explosion != null)
            explosion.SetActive(false);
    }

    // Called when a bullet hits the emitter
    public void Hit(bool isGhostBullet)
    {
        if (isDestroyed)
            return;

        // Ignore normal bullets when Ghost Only is enabled
        if (ghostOnly && !isGhostBullet)
            return;

        currentHealth--;

        UnityEngine.Debug.Log("Laser Emitter HP: " + currentHealth);

        if (currentHealth <= 0)
        {
            isDestroyed = true;
            StartCoroutine(DestroyEmitter());
        }
        else
        {
            // Flash when hit
            if (emitterSprite != null)
            {
                if (flashCoroutine != null)
                    StopCoroutine(flashCoroutine);

                flashCoroutine = StartCoroutine(HitFlash());
            }
        }
    }

    IEnumerator HitFlash()
    {
        emitterSprite.color = hitColor;

        yield return new WaitForSeconds(flashDuration);

        if (emitterSprite != null)
            emitterSprite.color = originalColor;

        flashCoroutine = null;
    }

    IEnumerator DestroyEmitter()
    {
        // Stop hit flash
        if (flashCoroutine != null)
        {
            StopCoroutine(flashCoroutine);
            flashCoroutine = null;
        }

        // Disable emitter collision
        Collider2D col = GetComponent<Collider2D>();

        if (col != null)
            col.enabled = false;

        float shutdownDuration = 0f;
        if (laserBeam != null)
        {
            LaserShutdown shutdown = laserBeam.GetComponentInChildren<LaserShutdown>();
            if (shutdown != null)
                shutdownDuration = shutdown.Play();
        }

        if (laserBeam != null && shutdownDuration <= 0f)
            laserBeam.SetActive(false);

        if (laserParticles != null)
            laserParticles.SetActive(false);

        // Hide emitter visual
        if (emitterVisual != null)
            emitterVisual.SetActive(false);

        // Play explosion
        if (explosion != null)
            explosion.SetActive(true);

        if (shutdownDuration > 0f)
        {
            yield return new WaitForSeconds(shutdownDuration);
            if (laserBeam != null)
                laserBeam.SetActive(false);
        }

        // Let the explosion finish if it lasts longer than the beam.
        float remainingExplosionTime = explosionDuration - shutdownDuration;
        if (remainingExplosionTime > 0f)
            yield return new WaitForSeconds(remainingExplosionTime);

        // Only destroy this trap
        Destroy(trapRoot != null ? trapRoot : gameObject);
    }
}
