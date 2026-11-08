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

    private GameObject currentGhost;

    private bool isRecording = false;
    private bool hasRecording = false;

    private List<RecordedFrame> recordedFrames = new List<RecordedFrame>();
    private List<RecordedShot> recordedShots = new List<RecordedShot>();

    private float recordStartTime;

    void Start()
    {
        if (playerSprite == null)
            playerSprite = GetComponentInChildren<SpriteRenderer>();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.E))
        {
            if (!isRecording && !hasRecording)
            {
                StartCoroutine(Record());
            }
            else if (hasRecording && currentGhost != null)
            {
                PlayerGhost ghost = currentGhost.GetComponent<PlayerGhost>();

                if (ghost != null)
                {
                    ghost.StartReplay(recordedFrames, recordedShots);
                    hasRecording = false;
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

    IEnumerator Record()
    {
        isRecording = true;
        hasRecording = false;

        recordedFrames.Clear();
        recordedShots.Clear();

        recordStartTime = Time.time;

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

            PlayerGhost ghost = currentGhost.GetComponent<PlayerGhost>();

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

        isRecording = false;
        hasRecording = true;

        SaveFrame(Time.time - recordStartTime);

        UnityEngine.Debug.Log(
            "Recording completed! Frames: " +
            recordedFrames.Count +
            " | Shots: " +
            recordedShots.Count
        );
    }
}
