using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerGhost : MonoBehaviour
{
    [Header("Disappear Settings")]
    public float disappearDuration = 0.5f;

    [Header("Ghost Gun")]
    public Transform gunPivot;
    public Transform gunFlip;

    private List<RecordedFrame> frames;
    private GhostAnimation ghostAnimation;
    private SpriteRenderer ghostSprite;

    private bool isReplaying = false;

    void Awake()
    {
        ghostAnimation = GetComponent<GhostAnimation>();

        // Find the ghost body SpriteRenderer
        SpriteRenderer[] sprites =
            GetComponentsInChildren<SpriteRenderer>(true);

        foreach (SpriteRenderer sprite in sprites)
        {
            if (sprite.gameObject.name == "Visual")
            {
                ghostSprite = sprite;
                break;
            }
        }

        if (ghostSprite == null)
        {
            UnityEngine.Debug.LogError("Ghost Visual SpriteRenderer missing!");
        }
    }

    // Set the initial ghost direction and gun angle
    public void SetInitialPose(bool facingLeft, float gunAngle)
    {
        if (ghostSprite != null)
            ghostSprite.flipX = facingLeft;

        if (gunPivot != null)
        {
            gunPivot.rotation = Quaternion.Euler(
                0f, 0f, gunAngle);
        }

        if (gunFlip != null)
        {
            gunFlip.localScale = new Vector3(
                1f,
                facingLeft ? -1f : 1f,
                1f
            );
        }
    }

    public void StartReplay(List<RecordedFrame> recordedFrames)
    {
        if (isReplaying || recordedFrames.Count < 2)
            return;

        frames = new List<RecordedFrame>(recordedFrames);
        isReplaying = true;

        StartCoroutine(Replay());
    }

    IEnumerator Replay()
    {
        float elapsed = 0f;
        int index = 0;

        while (elapsed < frames[frames.Count - 1].time)
        {
            elapsed += Time.deltaTime;

            while (index < frames.Count - 2 &&
                   frames[index + 1].time < elapsed)
            {
                index++;
            }

            RecordedFrame a = frames[index];
            RecordedFrame b = frames[index + 1];

            float t = Mathf.InverseLerp(
                a.time,
                b.time,
                elapsed
            );

            // Replay movement
            transform.position = Vector3.Lerp(
                a.position,
                b.position,
                t
            );

            // Replay body direction
            if (ghostSprite != null)
                ghostSprite.flipX = a.facingLeft;

            // Replay walking animation
            if (ghostAnimation != null)
                ghostAnimation.SetMoving(a.isMoving);

            // Replay gun direction
            float angle = Mathf.LerpAngle(
                a.gunAngle,
                b.gunAngle,
                t
            );

            if (gunPivot != null)
            {
                gunPivot.rotation = Quaternion.Euler(
                    0f, 0f, angle);
            }

            if (gunFlip != null)
            {
                bool aimingLeft =
                    Mathf.Cos(angle * Mathf.Deg2Rad) < 0f;

                gunFlip.localScale = new Vector3(
                    1f,
                    aimingLeft ? -1f : 1f,
                    1f
                );
            }

            yield return null;
        }

        // Restore final frame
        RecordedFrame lastFrame = frames[frames.Count - 1];

        transform.position = lastFrame.position;

        if (ghostSprite != null)
            ghostSprite.flipX = lastFrame.facingLeft;

        if (gunPivot != null)
        {
            gunPivot.rotation = Quaternion.Euler(
                0f, 0f, lastFrame.gunAngle);
        }

        if (gunFlip != null)
        {
            gunFlip.localScale = new Vector3(
                1f,
                lastFrame.facingLeft ? -1f : 1f,
                1f
            );
        }

        // Play disappear animation
        if (ghostAnimation != null)
        {
            ghostAnimation.SetMoving(false);
            ghostAnimation.Disappear();
        }

        isReplaying = false;

        UnityEngine.Debug.Log("Ghost disappearing!");

        yield return new WaitForSeconds(disappearDuration);

        Destroy(gameObject);
    }
}
