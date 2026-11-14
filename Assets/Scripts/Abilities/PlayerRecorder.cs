using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// One recorded movement frame
[System.Serializable]
public class RecordedFrame
{
    public Vector3 position;
    public float time;
    public bool facingLeft;
    public bool isMoving;
    public float gunAngle;

    public RecordedFrame(Vector3 pos, float t, bool left,
                         bool moving, float angle)
    {
        position = pos;
        time = t;
        facingLeft = left;
        isMoving = moving;
        gunAngle = angle;
    }
}

// One recorded shot
[System.Serializable]
public class RecordedShot
{
    public float time;
    public Vector3 position;
    public Quaternion rotation;

    public RecordedShot(float t, Vector3 pos, Quaternion rot)
    {
        time = t;
        position = pos;
        rotation = rot;
    }
}

public class PlayerRecorder : MonoBehaviour
{
    [Header("Recording Settings")]
    public float recordDuration = 4f;

    [Header("Ghost")]
    public GameObject ghostPrefab;

    [Header("Player References")]
    public SpriteRenderer playerSprite;
    public Transform gunPivot;

    [Header("Recording HUD")]
    public RecordingHUD recordingHUD;

    private GameObject currentGhost;

    private bool isRecording = false;
    private bool hasRecording = false;

    public bool IsRecording => isRecording;
    public bool IsReplaying { get; private set; }
    public bool HasRecording => hasRecording;

    public float RecordingRemaining =>
        isRecording
            ? Mathf.Max(0f, recordDuration - (Time.time - recordStartTime))
            : 0f;

    public float RecordingElapsed =>
        isRecording
            ? Mathf.Clamp(Time.time - recordStartTime, 0f, recordDuration)
            : 0f;

    public float ReplayElapsed =>
        IsReplaying
            ? Mathf.Clamp(Time.time - replayStartTime, 0f, recordDuration)
            : 0f;

    private List<RecordedFrame> recordedFrames =
        new List<RecordedFrame>();

    private List<RecordedShot> recordedShots =
        new List<RecordedShot>();

    private float recordStartTime;
    private float replayStartTime;

    private TimeSlow timeSlow;

    void Start()
    {
        if (playerSprite == null)
            playerSprite = GetComponentInChildren<SpriteRenderer>();

        timeSlow = GetComponent<TimeSlow>();

        if (recordingHUD != null)
        {
            recordingHUD.duration = recordDuration;
            recordingHUD.HideHUD();
        }
    }

    void Update()
    {
        // Update recording HUD
        if (recordingHUD != null)
        {
            if (isRecording)
            {
                recordingHUD.ShowRecording(RecordingElapsed);
            }
            else if (IsReplaying)
            {
                recordingHUD.ShowPlaying(ReplayElapsed);
            }
            else if (hasRecording)
            {
                recordingHUD.ShowReady();
            }
        }

        if (Input.GetKeyDown(KeyCode.E))
        {
            // Cannot use E while Q is active
            if (timeSlow != null && timeSlow.IsTimeSlowed)
                return;

            // Cannot start another action while recording or replaying
            if (isRecording || IsReplaying)
                return;

            if (!hasRecording)
            {
                StartCoroutine(Record());
            }
            else if (currentGhost != null)
            {
                PlayerGhost ghost =
                    currentGhost.GetComponent<PlayerGhost>();

                if (ghost != null)
                {
                    ghost.StartReplay(recordedFrames, recordedShots);
                    hasRecording = false;

                    // Keep the HUD in PLAY mode
                    if (recordingHUD != null && IsReplaying)
                    {
                        recordingHUD.ShowPlaying(0f);
                    }
                }
            }
        }
    }

    void LateUpdate()
    {
        if (isRecording)
        {
            SaveFrame(Time.time - recordStartTime);
        }
    }

    void SaveFrame(float time)
    {
        bool facingLeft = playerSprite.flipX;

        float horizontal = Input.GetAxisRaw("Horizontal");
        bool moving = Mathf.Abs(horizontal) > 0.01f;

        float angle = gunPivot.eulerAngles.z;

        recordedFrames.Add(
            new RecordedFrame(
                transform.position,
                time,
                facingLeft,
                moving,
                angle
            )
        );
    }

    // Record actual shots during recording
    public void RecordShot(Vector3 position, Quaternion rotation)
    {
        if (!isRecording)
            return;

        float shotTime = Time.time - recordStartTime;

        recordedShots.Add(
            new RecordedShot(
                shotTime,
                position,
                rotation
            )
        );

        UnityEngine.Debug.Log("Shot recorded!");
    }

    // Called when ghost replay starts or ends
    public void SetReplaying(bool replaying)
    {
        IsReplaying = replaying;

        if (replaying)
        {
            replayStartTime = Time.time;

            if (recordingHUD != null)
                recordingHUD.ShowPlaying(0f);
        }
        else
        {
            if (recordingHUD != null)
                recordingHUD.HideHUD();
        }

        UnityEngine.Debug.Log(
            "E Replay State: " + IsReplaying
        );
    }

    IEnumerator Record()
    {
        isRecording = true;
        hasRecording = false;

        recordedFrames.Clear();
        recordedShots.Clear();

        recordStartTime = Time.time;

        // Show recording HUD
        if (recordingHUD != null)
            recordingHUD.ShowRecording(0f);

        // Record starting position and direction
        SaveFrame(0f);

        // Spawn ghost at recording start
        if (ghostPrefab != null)
        {
            currentGhost = Instantiate(
                ghostPrefab,
                transform.position,
                Quaternion.identity
            );

            PlayerGhost ghost =
                currentGhost.GetComponent<PlayerGhost>();

            if (ghost != null)
            {
                ghost.SetInitialPose(
                    recordedFrames[0].facingLeft,
                    recordedFrames[0].gunAngle
                );
            }
        }

        UnityEngine.Debug.Log("Recording started!");

        yield return new WaitForSeconds(recordDuration);

        // Save final frame before finishing
        SaveFrame(Time.time - recordStartTime);

        isRecording = false;
        hasRecording = true;

        // Show READY HUD
        if (recordingHUD != null)
            recordingHUD.ShowReady();

        UnityEngine.Debug.Log(
            "Recording completed! Frames: " +
            recordedFrames.Count +
            " | Shots: " +
            recordedShots.Count
        );
    }
}
