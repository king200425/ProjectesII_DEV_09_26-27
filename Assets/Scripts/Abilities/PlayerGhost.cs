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
    public Transform muzzlePoint;
    public GameObject bulletPrefab;

    private List<RecordedFrame> frames;
    private List<RecordedShot> shots;

    private GhostAnimation ghostAnimation;
    private SpriteRenderer ghostSprite;
    private GunRecoil gunRecoil;

    private bool isReplaying = false;

    void Awake()
    {
        ghostAnimation = GetComponent<GhostAnimation>();

        // Find ghost body, not gun
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
            UnityEngine.Debug.LogError(
                "Ghost Visual SpriteRenderer missing!");
        }

        if (gunPivot != null)
        {
            gunRecoil = gunPivot.GetComponentInChildren<GunRecoil>();
        }
    }

    public void SetInitialPose(bool facingLeft, float gunAngle)
    {
        if (ghostSprite != null)
            ghostSprite.flipX = facingLeft;

        if (gunPivot != null)
            gunPivot.rotation = Quaternion.Euler(0f, 0f, gunAngle);

        if (gunFlip != null)
        {
            gunFlip.localScale = new Vector3(
                1f,
                facingLeft ? -1f : 1f,
                1f
            );
        }
    }

    public void StartReplay(
        List<RecordedFrame> recordedFrames,
        List<RecordedShot> recordedShots)
    {
        if (isReplaying || recordedFrames.Count < 2)
            return;

        frames = new List<RecordedFrame>(recordedFrames);
        shots = new List<RecordedShot>(recordedShots);

        isReplaying = true;
        StartCoroutine(Replay());
    }

    IEnumerator Replay()
    {
        float elapsed = 0f;
        int frameIndex = 0;
        int shotIndex = 0;

        float replayDuration = frames[frames.Count - 1].time;

        while (elapsed < replayDuration)
        {
            elapsed += Time.deltaTime;

            // Find movement frame
            while (frameIndex < frames.Count - 2 &&
                   frames[frameIndex + 1].time < elapsed)
            {
                frameIndex++;
            }

            RecordedFrame a = frames[frameIndex];
            RecordedFrame b = frames[frameIndex + 1];

            float t = Mathf.InverseLerp(a.time, b.time, elapsed);

            // Replay movement
            transform.position = Vector3.Lerp(
                a.position,
                b.position,
                t
            );

            // Replay facing direction
            if (ghostSprite != null)
                ghostSprite.flipX = a.facingLeft;

            // Replay movement animation
            if (ghostAnimation != null)
                ghostAnimation.SetMoving(a.isMoving);

            // Replay gun rotation
            float angle = Mathf.LerpAngle(
                a.gunAngle,
                b.gunAngle,
                t
            );

            if (gunPivot != null)
                gunPivot.rotation = Quaternion.Euler(0f, 0f, angle);

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

            // Replay shots at recorded times
            while (shotIndex < shots.Count &&
                   shots[shotIndex].time <= elapsed)
            {
                FireRecordedShot(shots[shotIndex]);
                shotIndex++;
            }

            yield return null;
        }

        // Restore final recorded frame
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

        // Fire any shots recorded at the very end
        while (shotIndex < shots.Count)
        {
            FireRecordedShot(shots[shotIndex]);
            shotIndex++;
        }

        // Play disappear animation
        if (ghostAnimation != null)
        {
            ghostAnimation.SetMoving(false);
            ghostAnimation.Disappear();
        }

        UnityEngine.Debug.Log("Ghost disappearing!");

        yield return new WaitForSeconds(disappearDuration);

        Destroy(gameObject);
    }

    void FireRecordedShot(RecordedShot shot)
    {
        if (bulletPrefab == null || muzzlePoint == null)
            return;

        // Spawn bullet from the ghost's own gun
        Instantiate(
            bulletPrefab,
            muzzlePoint.position,
            shot.rotation
        );

        // Play gun recoil
        if (gunRecoil != null)
            gunRecoil.PlayRecoil();
    }
}
