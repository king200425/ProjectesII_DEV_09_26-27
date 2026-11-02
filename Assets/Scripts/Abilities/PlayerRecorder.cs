
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// One recorded frame
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
                    ghost.StartReplay(recordedFrames);
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

    IEnumerator Record()
    {
        isRecording = true;
        hasRecording = false;

        recordedFrames.Clear();
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

            // Set initial facing direction and gun angle
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
            "Recording completed! Frames: " + recordedFrames.Count);
    }
}
