using System.Collections;
using UnityEngine;

public class EnemyHealth : MonoBehaviour
{
    [Header("Health")]
    public int maxHealth = 3;
    public float hitFlashDuration = 0.12f;
    public float deathDelay = 0.6f;
    public float fadeDuration = 0.25f;

    public bool IsDead { get; private set; }
    public int CurrentHealth { get; private set; }

    private SpriteRenderer spriteRenderer;
    private Collider2D bodyCollider;
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

        bodyCollider = GetComponent<Collider2D>();
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

        if (bodyCollider != null)
            bodyCollider.enabled = false;

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
        float elapsed = 0f;

        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            color.a = Mathf.Lerp(1f, 0f, elapsed / fadeDuration);
            spriteRenderer.color = color;
            yield return null;
        }

        Destroy(gameObject);
    }
}
