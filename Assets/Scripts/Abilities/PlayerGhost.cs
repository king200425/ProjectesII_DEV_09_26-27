
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerGhost : MonoBehaviour
{
    [Header("Disappear Settings")]
    public float disappearDuration = 0.5f;

    private List<RecordedFrame> frames;
    private GhostAnimation ghostAnimation;
    private SpriteRenderer ghostSprite;

    private bool isReplaying = false;

    void Awake()
    {
        ghostAnimation = GetComponent<GhostAnimation>();
        ghostSprite = GetComponentInChildren<SpriteRenderer>();

        if (ghostSprite == null)
        {
            UnityEngine.Debug.LogError("Ghost SpriteRenderer missing!");
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

            float t = Mathf.InverseLerp(a.time, b.time, elapsed);

            transform.position = Vector3.Lerp(
                a.position,
                b.position,
                t
            );

            // Restore recorded facing direction
            if (ghostSprite != null)
                ghostSprite.flipX = a.facingLeft;

            // Restore recorded movement animation
            if (ghostAnimation != null)
                ghostAnimation.SetMoving(a.isMoving);

            yield return null;
        }

        // Final recorded position
        RecordedFrame lastFrame = frames[frames.Count - 1];

        transform.position = lastFrame.position;

        if (ghostSprite != null)
            ghostSprite.flipX = lastFrame.facingLeft;

        // Stop movement and play disappear animation
        if (ghostAnimation != null)
        {
            ghostAnimation.SetMoving(false);
            ghostAnimation.Disappear();
        }

        isReplaying = false;

        UnityEngine.Debug.Log("Ghost disappearing!");

        // Wait for the disappear animation
        yield return new WaitForSeconds(disappearDuration);

        // Remove ghost
        Destroy(gameObject);
    }
}
