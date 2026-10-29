
using System.Collections;
using UnityEngine;

public class PlayerHealth : MonoBehaviour
{
    private enum HealthState
    {
        Normal,
        Invincible,
        Weak,
        Dead
    }

    [Header("Durations")]
    public float invincibleDuration = 2f;
    public float weakDuration = 4f;

    [Header("Blink / Weak Visual")]
    public float blinkInterval = 0.1f;
    public float redFlashInterval = 0.4f;
    public float redFlashDuration = 0.1f;

    [Header("References")]
    public SpriteRenderer playerSprite;
    public GameObject lightningObject;
    public GameObject gunPivot;

    private HealthState currentState = HealthState.Normal;
    private Color originalColor;

    // Components
    private Animator animator;
    private Rigidbody2D rb;
    private PlayerMovement playerMovement;
    private PlayerJump playerJump;
    private PlayerShooting playerShooting;
    private PlayerAim playerAim;

    void Start()
    {
        originalColor = playerSprite.color;

        animator = GetComponentInChildren<Animator>();
        rb = GetComponent<Rigidbody2D>();
        playerMovement = GetComponent<PlayerMovement>();
        playerJump = GetComponent<PlayerJump>();
        playerShooting = GetComponent<PlayerShooting>();
        playerAim = GetComponent<PlayerAim>();

        if (lightningObject != null)
        {
            lightningObject.SetActive(false);
        }
    }

    void Update()
    {
        // Press H to test damage
        if (Input.GetKeyDown(KeyCode.H))
        {
            TakeDamage();
        }
    }

    public void TakeDamage()
    {
        // Ignore damage when dead or invincible
        if (currentState == HealthState.Dead ||
            currentState == HealthState.Invincible)
        {
            return;
        }

        // Second hit during weak state
        if (currentState == HealthState.Weak)
        {
            Die();
            return;
        }

        // First hit
        StartCoroutine(DamageSequence());
    }

    IEnumerator DamageSequence()
    {
        // 1. Invincible blink
        currentState = HealthState.Invincible;

        float elapsed = 0f;

        while (elapsed < invincibleDuration)
        {
            playerSprite.enabled = !playerSprite.enabled;

            yield return new WaitForSeconds(blinkInterval);
            elapsed += blinkInterval;
        }

        playerSprite.enabled = true;

        // 2. Weak state
        currentState = HealthState.Weak;

        if (lightningObject != null)
        {
            lightningObject.SetActive(true);
        }

        elapsed = 0f;
        float nextRedFlash = 0f;

        while (elapsed < weakDuration)
        {
            if (elapsed >= nextRedFlash)
            {
                StartCoroutine(FlashRed());
                nextRedFlash += redFlashInterval;
            }

            elapsed += Time.deltaTime;
            yield return null;
        }

        // 3. Recover
        if (lightningObject != null)
        {
            lightningObject.SetActive(false);
        }

        playerSprite.color = originalColor;
        playerSprite.enabled = true;
        currentState = HealthState.Normal;
    }

    IEnumerator FlashRed()
    {
        playerSprite.color = Color.red;

        yield return new WaitForSeconds(redFlashDuration);

        playerSprite.color = originalColor;
    }

    void Die()
    {
        // Stop damage and flashing effects
        StopAllCoroutines();
        currentState = HealthState.Dead;

        playerSprite.enabled = true;
        playerSprite.color = originalColor;

        // Stop lightning
        if (lightningObject != null)
        {
            lightningObject.SetActive(false);
        }

        // Hide gun
        if (gunPivot != null)
        {
            gunPivot.SetActive(false);
        }

        // Disable player controls
        if (playerMovement != null)
            playerMovement.enabled = false;

        if (playerJump != null)
            playerJump.enabled = false;

        if (playerShooting != null)
            playerShooting.enabled = false;

        if (playerAim != null)
            playerAim.enabled = false;

        // Stop horizontal movement
        rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);

        // Play death animation
        animator.ResetTrigger("JumpTrigger");
        animator.ResetTrigger("LandTrigger");
        animator.SetBool("isMoving", false);
        animator.SetBool("isMovingBackward", false);
        animator.SetTrigger("DeathTrigger");
    }
}
