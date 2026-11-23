using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

[InitializeOnLoad]
public static class OutpostCameraSetup
{
    const string Root = "Assets/Art/AbandonedOutpost/ReadyToPaint";
    const string Request = "Assets/Art/AbandonedOutpost/Editor/CameraSetup.request.txt";
    const string SetupPath = Root + "/CameraSetupReport.json";
    const string CheckPath = Root + "/CameraRuntimeCheck.json";
    const string CheckKey = "Outpost.CameraQA";
    static double started, stageStarted, previousTick, validMouseSince;
    static int stage;
    static GameObject actor;
    static Rigidbody2D body;
    static OutpostPlayerCamera rig;
    static Camera main;
    static QSkillHUD qHud;
    static ESkillHUD eHud;
    static PlayerRecorder recorder;
    static TimeSlow timeSlow;
    static Vector3 hudPosition;
    static EditorWindow gameWindow;
    static QA result;

    [Serializable]
    class Audit
    {
        public string scene;
        public int playerCount, mainCameraCount, eventSystemCount, missingScripts;
        public bool aimReferences, qHudReferences, eHudReferences, recordingHudReferences;
        public bool hudScreenOverlay, secondaryCameraLinked, secondaryTextureLinked;
        public string cameraRig;
        public bool ready;
    }

    [Serializable]
    class QA
    {
        public string stage;
        public bool completed, lowScreenFraming, rightLookAhead, leftLookAhead;
        public bool mouseLooksLeft, mouseLooksRight, secondaryCameraSynced;
        public bool hudStayedFixed, eRecordingHudWorks, qSkillHudWorks;
        public float restingViewportY, rightViewportX, leftViewportX;
        public Vector2 mouseLeftScreen, mouseRightScreen;
        public List<string> errors = new List<string>();
    }

    static OutpostCameraSetup()
    {
        EditorApplication.delayCall += Bootstrap;
        EditorApplication.playModeStateChanged += PlayState;
    }

    static T[] Components<T>() where T : Component => SceneManager.GetActiveScene().GetRootGameObjects()
        .SelectMany(go => go.GetComponentsInChildren<T>(true)).ToArray();

    static void Bootstrap()
    {
        if (EditorApplication.isCompiling || EditorApplication.isUpdating)
        { EditorApplication.delayCall += Bootstrap; return; }
        if (EditorApplication.isPlayingOrWillChangePlaymode) return;
        if (File.Exists(Request) && (!File.Exists(SetupPath)
            || !JsonUtility.FromJson<Audit>(File.ReadAllText(SetupPath)).ready)) Configure();
    }

    [MenuItem("地图/配置主角 HUD 与跟随摄像头")]
    public static void Configure()
    {
        Scene scene = SceneManager.GetActiveScene();
        if (scene.path != "Assets/_Recovery/Mapx11.unity")
            throw new InvalidOperationException("Open the confirmed Mapx11 scene before configuring the camera.");
        PlayerMovement[] players = Components<PlayerMovement>();
        Camera[] cameras = Components<Camera>().Where(c => c.CompareTag("MainCamera")).ToArray();
        if (players.Length != 1 || cameras.Length != 1)
            throw new InvalidOperationException("Expected one player and one MainCamera in this scene.");
        if (!EditorSceneManager.SaveScene(scene, Root + "/Backups/Mapx11_BeforeCamera_"
            + DateTime.Now.ToString("yyyyMMdd_HHmmss") + ".unity", true)) throw new IOException("Scene backup failed.");

        GameObject player = players[0].gameObject;
        Camera camera = cameras[0];
        // This camera had a disabled, missing legacy TimeSlowScreenFX component.
        GameObjectUtility.RemoveMonoBehavioursWithMissingScript(camera.gameObject);
        GameObject rigObject = scene.GetRootGameObjects().FirstOrDefault(go => go.name == "PlayerCameraRig");
        if (rigObject == null)
        {
            rigObject = new GameObject("PlayerCameraRig");
            Undo.RegisterCreatedObjectUndo(rigObject, "Create player camera rig");
        }
        OutpostPlayerCamera controller = rigObject.GetComponent<OutpostPlayerCamera>();
        if (controller == null) controller = Undo.AddComponent<OutpostPlayerCamera>(rigObject);
        Undo.SetTransformParent(camera.transform, rigObject.transform, "Parent camera under follow rig");
        camera.transform.localPosition = Vector3.zero;
        camera.transform.localRotation = Quaternion.identity;
        camera.orthographic = true;
        controller.target = player.transform;
        controller.viewCamera = camera;
        CameraFollow oldFollow = camera.GetComponent<CameraFollow>();
        if (oldFollow != null) oldFollow.enabled = false;
        controller.SnapToTarget();

        PlayerRecorder record = player.GetComponent<PlayerRecorder>();
        TimeSlow slow = player.GetComponent<TimeSlow>();
        RecordingHUD recording = Components<RecordingHUD>().Single();
        record.recordingHUD = recording;
        foreach (ESkillHUD hud in Components<ESkillHUD>()) hud.playerRecorder = record;
        foreach (QSkillHUD hud in Components<QSkillHUD>()) hud.timeSlow = slow;
        foreach (CameraSync sync in Components<CameraSync>())
        {
            sync.mainCamera = camera;
            sync.transform.SetPositionAndRotation(camera.transform.position, camera.transform.rotation);
            Camera secondary = sync.GetComponent<Camera>();
            secondary.orthographic = camera.orthographic;
            secondary.orthographicSize = camera.orthographicSize;
            secondary.aspect = camera.aspect;
        }
        foreach (TimeSlowOverlayFX overlay in Components<TimeSlowOverlayFX>())
        { overlay.player = player.transform; overlay.mainCamera = camera; overlay.timeSlow = slow; }
        EditorUtility.SetDirty(controller);
        EditorSceneManager.MarkSceneDirty(scene);
        if (!EditorSceneManager.SaveScene(scene)) throw new IOException("Could not save the camera setup.");
        Audit audit = Inspect();
        File.WriteAllText(SetupPath, JsonUtility.ToJson(audit, true));
        AssetDatabase.ImportAsset(SetupPath);
        Debug.Log("[Outpost] Player / HUD / camera audit: " + (audit.ready ? "PASS" : "Needs attention"));
    }

    static Audit Inspect()
    {
        var audit = new Audit { scene = SceneManager.GetActiveScene().path };
        var players = Components<PlayerMovement>();
        audit.playerCount = players.Length;
        audit.mainCameraCount = Components<Camera>().Count(c => c.CompareTag("MainCamera"));
        audit.eventSystemCount = Components<EventSystem>().Length;
        audit.missingScripts = SceneManager.GetActiveScene().GetRootGameObjects()
            .SelectMany(go => go.GetComponentsInChildren<Transform>(true))
            .Sum(t => GameObjectUtility.GetMonoBehavioursWithMissingScriptCount(t.gameObject));
        GameObject player = players.Single().gameObject;
        Camera camera = Components<Camera>().Single(c => c.CompareTag("MainCamera"));
        PlayerAim aim = player.GetComponent<PlayerAim>();
        audit.aimReferences = aim != null && aim.gunPivot != null && aim.gunFlip != null && aim.playerSprite != null;
        QSkillHUD q = Components<QSkillHUD>().Single();
        ESkillHUD e = Components<ESkillHUD>().Single();
        RecordingHUD recording = Components<RecordingHUD>().Single();
        audit.qHudReferences = q.timeSlow == player.GetComponent<TimeSlow>() && q.cooldownFill != null
            && q.countdownText != null && q.skillImage != null;
        audit.eHudReferences = e.playerRecorder == player.GetComponent<PlayerRecorder>()
            && e.skillImage != null && e.countdownText != null;
        audit.recordingHudReferences = recording.recordingHUDRoot != null && recording.statusBorder != null
            && recording.statusFrame != null && recording.statusIcon != null && recording.statusText != null
            && recording.timerText != null && player.GetComponent<PlayerRecorder>().recordingHUD == recording;
        Canvas canvas = q.GetComponentInParent<Canvas>();
        audit.hudScreenOverlay = canvas != null && canvas.renderMode == RenderMode.ScreenSpaceOverlay
            && !canvas.transform.IsChildOf(player.transform);
        CameraSync sync = Components<CameraSync>().Single();
        audit.secondaryCameraLinked = sync.mainCamera == camera;
        RenderTexture texture = sync.GetComponent<Camera>().targetTexture;
        audit.secondaryTextureLinked = texture != null && Components<RawImage>().Any(image => image.texture == texture);
        audit.cameraRig = camera.transform.parent != null ? camera.transform.parent.name : "";
        audit.ready = audit.playerCount == 1 && audit.mainCameraCount == 1 && audit.eventSystemCount == 1
            && audit.missingScripts == 0 && audit.aimReferences && audit.qHudReferences && audit.eHudReferences
            && audit.recordingHudReferences && audit.hudScreenOverlay && audit.secondaryCameraLinked && audit.secondaryTextureLinked;
        return audit;
    }

    [MenuItem("地图/检查镜头与 Q E HUD")]
    public static void StartCheck()
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode) return;
        EditorSceneManager.SaveScene(SceneManager.GetActiveScene());
        Type type = typeof(Editor).Assembly.GetType("UnityEditor.GameView");
        gameWindow = EditorWindow.GetWindow(type);
        SessionState.SetBool(CheckKey + ".Maximized", gameWindow.maximized);
        gameWindow.maximized = true;
        gameWindow.Focus();
        SessionState.SetBool(CheckKey, true);
        EditorApplication.isPlaying = true;
    }

    static void PlayState(PlayModeStateChange state)
    {
        if (state == PlayModeStateChange.EnteredPlayMode && SessionState.GetBool(CheckKey, false))
        {
            result = new QA { stage = "Boot" };
            stage = 0;
            started = stageStarted = previousTick = EditorApplication.timeSinceStartup;
            gameWindow = EditorWindow.GetWindow(typeof(Editor).Assembly.GetType("UnityEditor.GameView"));
            gameWindow.Focus();
            Application.logMessageReceived += Log;
            EditorApplication.update += Tick;
        }
        if (state == PlayModeStateChange.ExitingPlayMode)
        { EditorApplication.update -= Tick; Application.logMessageReceived -= Log; }
        if (state == PlayModeStateChange.EnteredEditMode && SessionState.GetBool(CheckKey + ".RestoreWindow", false))
        {
            SessionState.SetBool(CheckKey + ".RestoreWindow", false);
            EditorWindow window = EditorWindow.GetWindow(typeof(Editor).Assembly.GetType("UnityEditor.GameView"));
            window.maximized = SessionState.GetBool(CheckKey + ".Maximized", false);
            AssetDatabase.ImportAsset(CheckPath);
        }
    }

    static void Log(string message, string stack, LogType type)
    {
        if (result != null && (type == LogType.Error || type == LogType.Exception)) result.errors.Add(message);
    }

    static void Stage(int value, string label)
    {
        stage = value;
        stageStarted = EditorApplication.timeSinceStartup;
        result.stage = label;
        File.WriteAllText(CheckPath, JsonUtility.ToJson(result, true));
    }

    static void Tick()
    {
        try
        {
            double now = EditorApplication.timeSinceStartup;
            float dt = Mathf.Min((float)(now - previousTick), 0.05f);
            previousTick = now;
            if (now - started > 90) throw new TimeoutException("Camera check timed out waiting for input.");
            if (stage == 0 && now - stageStarted > 0.5)
            {
                Audit audit = Inspect();
                if (!audit.ready) throw new InvalidOperationException("Player/HUD audit did not pass.");
                rig = Components<OutpostPlayerCamera>().Single();
                main = rig.viewCamera;
                actor = rig.target.gameObject;
                body = actor.GetComponent<Rigidbody2D>();
                actor.GetComponent<PlayerMovement>().enabled = false;
                actor.GetComponent<PlayerJump>().enabled = false;
                body.simulated = false;
                body.linearVelocity = Vector2.zero;
                rig.mouseLookEnabled = false;
                rig.SnapToTarget();
                qHud = Components<QSkillHUD>().Single();
                eHud = Components<ESkillHUD>().Single();
                recorder = actor.GetComponent<PlayerRecorder>();
                timeSlow = actor.GetComponent<TimeSlow>();
                hudPosition = qHud.skillImage.rectTransform.position;
                Stage(1, "Rest framing");
            }
            else if (stage == 1 && now - stageStarted > 0.3)
            {
                result.restingViewportY = main.WorldToViewportPoint(actor.transform.position).y;
                result.lowScreenFraming = Mathf.Abs(result.restingViewportY - 0.4f) < 0.025f;
                body.linearVelocity = new Vector2(5, 0);
                Stage(2, "Move right");
            }
            else if (stage == 2)
            {
                actor.transform.position += Vector3.right * (3f * dt);
                if (now - stageStarted > 1.1)
                {
                    result.rightViewportX = main.WorldToViewportPoint(actor.transform.position).x;
                    result.rightLookAhead = result.rightViewportX < 0.48f;
                    body.linearVelocity = new Vector2(-5, 0);
                    Stage(3, "Move left");
                }
            }
            else if (stage == 3)
            {
                actor.transform.position += Vector3.left * (3f * dt);
                if (now - stageStarted > 1.3)
                {
                    result.leftViewportX = main.WorldToViewportPoint(actor.transform.position).x;
                    result.leftLookAhead = result.leftViewportX > 0.52f;
                    body.linearVelocity = Vector2.zero;
                    rig.movementLookFraction = 0;
                    rig.mouseLookEnabled = true;
                    CameraSync sync = Components<CameraSync>().Single();
                    Camera secondary = sync.GetComponent<Camera>();
                    result.secondaryCameraSynced = Vector3.Distance(secondary.transform.position, main.transform.position) < 0.002f
                        && Mathf.Abs(secondary.orthographicSize - main.orthographicSize) < 0.001f
                        && Mathf.Abs(secondary.aspect - main.aspect) < 0.001f;
                    result.hudStayedFixed = Vector3.Distance(qHud.skillImage.rectTransform.position, hudPosition) < 0.1f;
                    Rect rect = gameWindow.position;
                    result.mouseLeftScreen = new Vector2(rect.x + rect.width * 0.2f, rect.center.y);
                    result.mouseRightScreen = new Vector2(rect.x + rect.width * 0.8f, rect.center.y);
                    validMouseSince = 0;
                    Stage(4, "MouseLeft");
                }
            }
            else if (stage == 4 || stage == 5)
            {
                float cursorX = main.ScreenToViewportPoint(Input.mousePosition).x;
                bool correctSide = main.pixelRect.Contains(Input.mousePosition)
                    && (stage == 4 ? cursorX < 0.25f : cursorX > 0.75f);
                if (!correctSide) validMouseSince = 0;
                else if (validMouseSince == 0) validMouseSince = now;
                else if (now - validMouseSince > 0.9)
                {
                    float offset = main.transform.position.x - actor.transform.position.x;
                    if (stage == 4)
                    {
                        result.mouseLooksLeft = offset < -0.1f;
                        validMouseSince = 0;
                        Stage(5, "MouseRight");
                    }
                    else
                    {
                        result.mouseLooksRight = offset > 0.1f;
                        recorder.recordDuration = 0.9f;
                        IEnumerator routine = (IEnumerator)typeof(PlayerRecorder).GetMethod("Record", BindingFlags.Instance | BindingFlags.NonPublic).Invoke(recorder, null);
                        recorder.StartCoroutine(routine);
                        Stage(6, "Recording E HUD");
                    }
                }
            }
            else if (stage == 6 && now - stageStarted > 0.35)
            {
                RecordingHUD hud = recorder.recordingHUD;
                result.eRecordingHudWorks = recorder.IsRecording && hud.recordingHUDRoot.activeInHierarchy
                    && hud.statusText.text == "REC" && eHud.countdownText.gameObject.activeInHierarchy;
                Stage(7, "Wait for E recording");
            }
            else if (stage == 7 && !recorder.IsRecording)
            {
                timeSlow.duration = 0.9f;
                IEnumerator routine = (IEnumerator)typeof(TimeSlow).GetMethod("ActivateTimeSlow", BindingFlags.Instance | BindingFlags.NonPublic).Invoke(timeSlow, null);
                timeSlow.StartCoroutine(routine);
                Stage(8, "Q HUD");
            }
            else if (stage == 8 && now - stageStarted > 0.25)
            {
                result.qSkillHudWorks = timeSlow.IsTimeSlowed && qHud.countdownText.gameObject.activeInHierarchy
                    && !string.IsNullOrEmpty(qHud.countdownText.text);
                result.completed = result.lowScreenFraming && result.rightLookAhead && result.leftLookAhead
                    && result.mouseLooksLeft && result.mouseLooksRight && result.secondaryCameraSynced
                    && result.hudStayedFixed && result.eRecordingHudWorks && result.qSkillHudWorks && result.errors.Count == 0;
                Finish();
            }
        }
        catch (Exception exception) { result.errors.Add(exception.ToString()); Finish(); }
    }

    static void Finish()
    {
        EditorApplication.update -= Tick;
        Application.logMessageReceived -= Log;
        SessionState.SetBool(CheckKey, false);
        SessionState.SetBool(CheckKey + ".RestoreWindow", true);
        result.stage = "Complete";
        File.WriteAllText(CheckPath, JsonUtility.ToJson(result, true));
        Debug.Log("[Outpost] Camera and HUD runtime check: " + (result.completed ? "PASS" : JsonUtility.ToJson(result)));
        EditorApplication.isPlaying = false;
    }
}
