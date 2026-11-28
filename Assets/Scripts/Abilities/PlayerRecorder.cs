using System.Collections.Generic;
using UnityEngine;

// One recorded movement frame.
[System.Serializable]
public class RecordedFrame
{
    public Vector3 position;
    public float time;
    public bool facingLeft;
    public bool isMoving;
    public float gunAngle;

    public RecordedFrame(Vector3 pos, float t, bool left, bool moving, float angle)
    {
        position = pos;
        time = t;
        facingLeft = left;
        isMoving = moving;
        gunAngle = angle;
    }
}

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
    public static event System.Action ReplayStarted;

    [Header("Recording Settings")]
    [Min(0.01f), Tooltip("Maximum length. Press E again to finish recording early.")]
    public float recordDuration = 4f;
    [SerializeField] private bool requireEchoUnlock;

    [Header("Ghost")]
    public GameObject ghostPrefab;

    [Header("Player References")]
    public SpriteRenderer playerSprite;
    public Transform gunPivot;

    [Header("Recording HUD")]
    public RecordingHUD recordingHUD;

    private GameObject currentGhost;
    private bool isRecording;
    private bool hasRecording;
    private float recordStartTime;
    private float replayStartTime;
    private TimeSlow timeSlow;
    private readonly List<RecordedFrame> recordedFrames = new List<RecordedFrame>();
    private readonly List<RecordedShot> recordedShots = new List<RecordedShot>();

    private float MaximumDuration => Mathf.Max(0.01f, recordDuration);
    public bool IsRecording => isRecording;
    public bool IsReplaying { get; private set; }
    public bool HasRecording => hasRecording;
    public float RecordedDuration { get; private set; }
    public float RecordingElapsed => isRecording ? Mathf.Clamp(Time.time - recordStartTime, 0f, MaximumDuration) : 0f;
    public float RecordingRemaining => isRecording ? Mathf.Max(0f, MaximumDuration - RecordingElapsed) : 0f;
    public float ReplayElapsed => IsReplaying ? Mathf.Clamp(Time.time - replayStartTime, 0f, RecordedDuration) : 0f;
    public float ReplayRemaining => IsReplaying ? Mathf.Max(0f, RecordedDuration - ReplayElapsed) : 0f;

    void Start()
    {
        if (playerSprite == null)
            playerSprite = GetComponentInChildren<SpriteRenderer>();
        timeSlow = GetComponent<TimeSlow>();
        if (recordingHUD == null)
            recordingHUD = FindFirstObjectByType<RecordingHUD>();
        if (recordingHUD != null)
        {
            recordingHUD.duration = MaximumDuration;
            recordingHUD.HideHUD();
        }
    }

    void Update()
    {
        if (Time.timeScale <= 0f)
            return;
        if (Input.GetKeyDown(KeyCode.E))
            UseEchoSkill();
        RefreshHUD();
    }

    // E cycles: idle -> recording -> ready -> replaying. Replaying cannot be interrupted by E.
    public bool UseEchoSkill()
    {
        if (!isActiveAndEnabled || Time.timeScale <= 0f ||
            (requireEchoUnlock && !SkillUnlockState.EchoUnlocked) ||
            (timeSlow != null && timeSlow.IsTimeSlowed) || IsReplaying)
            return false;

        if (isRecording)
        {
            FinishRecording();
            return true;
        }
        if (!hasRecording)
        {
            BeginRecording();
            return true;
        }

        PlayerGhost ghost = currentGhost != null ? currentGhost.GetComponent<PlayerGhost>() : null;
        if (ghost == null || !ghost.TryStartReplay(recordedFrames, recordedShots, this))
            return false;

        hasRecording = false;
        ReplayStarted?.Invoke();
        RefreshHUD();
        return true;
    }

    void LateUpdate()
    {
        if (!isRecording || Time.timeScale <= 0f)
            return;
        float elapsed = Time.time - recordStartTime;
        if (elapsed >= MaximumDuration)
            FinishRecording();
        else
            SaveFrame(elapsed);
    }

    void BeginRecording()
    {
        if (currentGhost != null)
            Destroy(currentGhost);
        recordedFrames.Clear();
        recordedShots.Clear();
        RecordedDuration = 0f;
        recordStartTime = Time.time;
        isRecording = true;
        hasRecording = false;
        SaveFrame(0f);

        if (ghostPrefab != null)
        {
            currentGhost = Instantiate(ghostPrefab, transform.position, Quaternion.identity);
            var ghost = currentGhost.GetComponent<PlayerGhost>();
            if (ghost != null)
                ghost.SetInitialPose(recordedFrames[0].facingLeft, recordedFrames[0].gunAngle);
        }
        RefreshHUD();
    }

    void FinishRecording()
    {
        // Keep both endpoints even for a very quick tap, so the clip is always replayable.
        RecordedDuration = Mathf.Clamp(Time.time - recordStartTime, 0.0001f, MaximumDuration);
        SaveFrame(RecordedDuration);
        isRecording = false;
        hasRecording = true;
        RefreshHUD();
    }

    void SaveFrame(float time)
    {
        recordedFrames.Add(new RecordedFrame(transform.position, time,
            playerSprite != null && playerSprite.flipX,
            Mathf.Abs(Input.GetAxisRaw("Horizontal")) > 0.01f,
            gunPivot != null ? gunPivot.eulerAngles.z : 0f));
    }

    public void RecordShot(Vector3 position, Quaternion rotation)
    {
        if (!isRecording || Time.timeScale <= 0f)
            return;
        float elapsed = Time.time - recordStartTime;
        if (elapsed <= MaximumDuration)
            recordedShots.Add(new RecordedShot(elapsed, position, rotation));
    }

    public void SetReplaying(bool replaying)
    {
        IsReplaying = replaying;
        if (replaying)
            replayStartTime = Time.time;
        RefreshHUD();
    }

    void RefreshHUD()
    {
        if (recordingHUD == null)
            return;
        if (isRecording)
        {
            recordingHUD.duration = MaximumDuration;
            recordingHUD.ShowRecording(RecordingElapsed);
        }
        else if (IsReplaying)
        {
            recordingHUD.duration = RecordedDuration;
            recordingHUD.ShowPlaying(ReplayElapsed);
        }
        else if (hasRecording)
        {
            recordingHUD.duration = RecordedDuration;
            recordingHUD.ShowReady();
        }
        else
            recordingHUD.HideHUD();
    }

    void OnDisable()
    {
        isRecording = false;
        IsReplaying = false;
        hasRecording = false;
        RecordedDuration = 0f;
        recordedFrames.Clear();
        recordedShots.Clear();
        if (currentGhost != null)
            Destroy(currentGhost);
        currentGhost = null;
        if (recordingHUD != null && recordingHUD.isActiveAndEnabled)
            recordingHUD.HideHUD();
    }
}
