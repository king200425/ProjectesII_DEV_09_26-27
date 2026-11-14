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
    public float respawnDelay = 1f;

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

    // Checkpoint
    private Vector3 checkpointPosition;

    // Components
    private Animator animator;
    private Rigidbody2D rb;
    private PlayerMovement playerMovement;
    private PlayerJump playerJump;
    private PlayerShooting playerShooting;
    private PlayerAim playerAim;
    private CameraShake cameraShake;
    private DamageScreenFX damageScreenFX;

    void Start()
    {
        originalColor = playerSprite.color;

        animator = GetComponentInChildren<Animator>();
        rb = GetComponent<Rigidbody2D>();
        playerMovement = GetComponent<PlayerMovement>();
        playerJump = GetComponent<PlayerJump>();
        playerShooting = GetComponent<PlayerShooting>();
        playerAim = GetComponent<PlayerAim>();
        cameraShake = FindFirstObjectByType<CameraShake>();

        // Find screen damage effects
        damageScreenFX = FindFirstObjectByType<DamageScreenFX>();

        // Starting position is the default checkpoint
        checkpointPosition = transform.position;

        if (lightningObject != null)
            lightningObject.SetActive(false);
    }

    void Update()
    {
        // Press H to test damage
        if (Input.GetKeyDown(KeyCode.H))
        {
            TakeDamage();
        }
    }

    public void SetCheckpoint(Vector3 position)
    {
        checkpointPosition = position;
        UnityEngine.Debug.Log("Checkpoint saved: " + checkpointPosition);
    }

    public void TakeDamage()
    {
        if (currentState == HealthState.Dead ||
            currentState == HealthState.Invincible)
        {
            return;
        }

        // Shake camera when taking damage
        if (cameraShake != null)
        {
            cameraShake.Shake();
        }

        if (currentState == HealthState.Weak)
        {
            Die();
            return;
        }

        // Start screen damage effects
        if (damageScreenFX != null)
        {
            damageScreenFX.StartDamage();
        }

        StartCoroutine(DamageSequence());
    }

    IEnumerator DamageSequence()
    {
        // 1. Invincible
        currentState = HealthState.Invincible;

        float elapsed = 0f;

        while (elapsed < invincibleDuration)
        {
            playerSprite.enabled = !playerSprite.enabled;

            yield return new WaitForSeconds(blinkInterval);
            elapsed += blinkInterval;
        }

        playerSprite.enabled = true;

        // 2. Weak
        currentState = HealthState.Weak;

        if (lightningObject != null)
            lightningObject.SetActive(true);

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
            lightningObject.SetActive(false);

        // Fade out screen damage effects
        if (damageScreenFX != null)
        {
            damageScreenFX.Recover();
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

    public void Die()
    {
        if (currentState == HealthState.Dead)
            return;

        StopAllCoroutines();
        currentState = HealthState.Dead;

        // Clear screen damage effects
        // Later we will replace this with the death fade
        if (damageScreenFX != null)
        {
            damageScreenFX.StopEffects();
        }

        playerSprite.enabled = true;
        playerSprite.color = originalColor;

        if (lightningObject != null)
            lightningObject.SetActive(false);

        if (gunPivot != null)
            gunPivot.SetActive(false);

        // Disable controls
        if (playerMovement != null)
            playerMovement.enabled = false;

        if (playerJump != null)
            playerJump.enabled = false;

        if (playerShooting != null)
            playerShooting.enabled = false;

        if (playerAim != null)
            playerAim.enabled = false;

        // Stop movement
        rb.linearVelocity = Vector2.zero;
        rb.simulated = false;

        // Play death animation
        animator.ResetTrigger("JumpTrigger");
        animator.ResetTrigger("LandTrigger");
        animator.SetBool("isMoving", false);
        animator.SetBool("isMovingBackward", false);
        animator.SetTrigger("DeathTrigger");

        // Start respawn
        StartCoroutine(Respawn());
    }

    IEnumerator Respawn()
    {
        // Wait for death animation
        yield return new WaitForSeconds(respawnDelay);

        // Move player to the last checkpoint
        transform.position = checkpointPosition;

        // Restore physics
        rb.simulated = true;
        rb.linearVelocity = Vector2.zero;

        // Restore player appearance
        playerSprite.enabled = true;
        playerSprite.color = originalColor;

        if (lightningObject != null)
            lightningObject.SetActive(false);

        if (gunPivot != null)
            gunPivot.SetActive(true);

        // Restore controls
        if (playerMovement != null)
            playerMovement.enabled = true;

        if (playerJump != null)
            playerJump.enabled = true;

        if (playerShooting != null)
            playerShooting.enabled = true;

        if (playerAim != null)
            playerAim.enabled = true;

        // Return to idle animation
        animator.ResetTrigger("DeathTrigger");
        animator.SetTrigger("RespawnTrigger");

        currentState = HealthState.Normal;

        UnityEngine.Debug.Log("Player respawned!");
    }
}
