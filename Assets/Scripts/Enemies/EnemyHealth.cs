using System.Collections;
using UnityEngine;

public class EnemyHealth : MonoBehaviour
{
    [Header("Health")]
    public int maxHealth = 4;
    public float hitFlashDuration = 0.12f;
    public float deathDelay = 0.8f;
    public float fadeDuration = 0.35f;

    public bool IsDead { get; private set; }
    public int CurrentHealth { get; private set; }

    private SpriteRenderer spriteRenderer;
    private EnemyAI enemyAI;
    private EnemyAnimator enemyAnimator;
    private Color originalColor = Color.white;
    private Coroutine flashRoutine;

    void Awake()
    {
        CurrentHealth = maxHealth;

        spriteRenderer = GetComponent<SpriteRenderer>();

        if (spriteRenderer == null)
            spriteRenderer = GetComponentInChildren<SpriteRenderer>();

        enemyAI = GetComponent<EnemyAI>();
        enemyAnimator = GetComponent<EnemyAnimator>();

        if (spriteRenderer != null)
            originalColor = spriteRenderer.color;
    }

    public void TakeDamage(int amount)
    {
        if (IsDead || amount <= 0)
            return;

        CurrentHealth = Mathf.Max(0, CurrentHealth - amount);

        UnityEngine.Debug.Log(
            name + " HP: " + CurrentHealth + "/" + maxHealth);

        if (CurrentHealth <= 0)
        {
            Die();
            return;
        }

        if (enemyAI != null)
            enemyAI.OnHit();

        if (flashRoutine != null)
            StopCoroutine(flashRoutine);

        flashRoutine = StartCoroutine(HitFlash());
    }

    IEnumerator HitFlash()
    {
        if (spriteRenderer == null)
            yield break;

        spriteRenderer.color = Color.red;

        yield return new WaitForSeconds(hitFlashDuration);

        if (!IsDead)
            spriteRenderer.color = originalColor;

        flashRoutine = null;
    }

    void Die()
    {
        IsDead = true;

        if (flashRoutine != null)
        {
            StopCoroutine(flashRoutine);
            flashRoutine = null;
        }

        foreach (Collider2D collider in
                 GetComponentsInChildren<Collider2D>(true))
        {
            collider.enabled = false;
        }

        if (enemyAI != null)
            enemyAI.enabled = false;

        if (spriteRenderer != null)
            spriteRenderer.color = originalColor;

        if (enemyAnimator != null)
            enemyAnimator.PlayDeath();

        UnityEngine.Debug.Log(name + " died.");

        StartCoroutine(DeathRoutine());
    }

    IEnumerator DeathRoutine()
    {
        yield return new WaitForSeconds(deathDelay);

        if (spriteRenderer == null)
        {
            Destroy(gameObject);
            yield break;
        }

        Color color = spriteRenderer.color;
        Vector3 startPosition = transform.position;
        float elapsed = 0f;

        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            float progress = Mathf.Clamp01(elapsed / fadeDuration);
            color.a = Mathf.Lerp(1f, 0f, progress);
            spriteRenderer.color = color;
            transform.position = startPosition +
                Vector3.down * (progress * 0.15f);
            transform.rotation = Quaternion.Euler(
                0f, 0f, Mathf.Lerp(0f, -12f, progress));
            yield return null;
        }

        Destroy(gameObject);
    }
}
