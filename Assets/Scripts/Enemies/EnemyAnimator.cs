using UnityEngine;

public class EnemyAnimator : MonoBehaviour
{
    [Header("Frames")]
    public Sprite[] idleFrames;
    public Sprite[] walkFrames;
    public Sprite[] attackFrames;
    public Sprite[] deathFrames;

    [Header("Settings")]
    public float framesPerSecond = 8f;

    private SpriteRenderer spriteRenderer;
    private Sprite[] currentFrames;
    private float frameTimer;
    private int frameIndex;
    private bool loop;
    private bool isDead;
    private bool isAttacking;
    private bool isMoving;

    public float AttackDuration
    {
        get
        {
            if (attackFrames == null ||
                attackFrames.Length == 0 ||
                framesPerSecond <= 0f)
            {
                return 0f;
            }

            return attackFrames.Length / framesPerSecond;
        }
    }

    void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();

        if (spriteRenderer == null)
            spriteRenderer = GetComponentInChildren<SpriteRenderer>();

        if ((idleFrames == null || idleFrames.Length == 0) &&
            spriteRenderer != null &&
            spriteRenderer.sprite != null)
        {
            idleFrames = new Sprite[] { spriteRenderer.sprite };
        }

        PlayIdle();
    }

    void Update()
    {
        if (isDead || currentFrames == null || currentFrames.Length == 0)
            return;

        if (framesPerSecond <= 0f)
            return;

        if (!loop && frameIndex >= currentFrames.Length - 1)
            return;

        frameTimer += Time.deltaTime;

        float frameDuration = 1f / framesPerSecond;

        while (frameTimer >= frameDuration)
        {
            frameTimer -= frameDuration;

            if (loop)
            {
                frameIndex = (frameIndex + 1) % currentFrames.Length;
            }
            else
            {
                frameIndex = Mathf.Min(
                    frameIndex + 1,
                    currentFrames.Length - 1);
            }

            ApplyFrame();

            if (!loop && frameIndex >= currentFrames.Length - 1)
                break;
        }
    }

    public void SetMoving(bool moving)
    {
        if (isDead || isAttacking)
            return;

        if (isMoving == moving)
            return;

        isMoving = moving;

        if (moving && walkFrames != null && walkFrames.Length > 0)
        {
            PlayFrames(walkFrames, true);
        }
        else
        {
            PlayIdle();
        }
    }

    public void PlayIdle()
    {
        if (isDead)
            return;

        isAttacking = false;
        isMoving = false;
        PlayFrames(idleFrames, true);
    }

    public void PlayAttack()
    {
        if (isDead)
            return;

        isAttacking = true;
        isMoving = false;

        if (attackFrames != null && attackFrames.Length > 0)
        {
            PlayFrames(attackFrames, false);
        }
        else
        {
            PlayFrames(idleFrames, true);
        }
    }

    public void PlayDeath()
    {
        isDead = true;
        isAttacking = false;
        isMoving = false;

        if (deathFrames != null && deathFrames.Length > 0)
            PlayFrames(deathFrames, false);
    }

    public void SetFacing(bool faceLeft)
    {
        if (spriteRenderer != null)
            spriteRenderer.flipX = faceLeft;
    }

    void PlayFrames(Sprite[] frames, bool shouldLoop)
    {
        if (frames == null || frames.Length == 0)
            return;

        currentFrames = frames;
        loop = shouldLoop;
        frameTimer = 0f;
        frameIndex = 0;

        ApplyFrame();
    }

    void ApplyFrame()
    {
        if (spriteRenderer != null &&
            currentFrames != null &&
            currentFrames.Length > 0)
        {
            spriteRenderer.sprite = currentFrames[frameIndex];
        }
    }
}
