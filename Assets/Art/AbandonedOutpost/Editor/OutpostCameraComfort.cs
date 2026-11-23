using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;

[InitializeOnLoad]
public static class OutpostCameraComfort
{
    const string Root = "Assets/Art/AbandonedOutpost/ReadyToPaint";
    const string Request = "Assets/Art/AbandonedOutpost/Editor/CameraComfort.request.txt";
    const string Key = "Outpost.CameraComfort.Check";
    static OutpostPlayerCamera rig;
    static Rigidbody2D body;
    static Camera secondary;
    static RectTransform hud;
    static Vector3 hudPosition;
    static float started, stageStarted, previousOffset;
    static int stage;
    static bool running;
    static Report report;

    [Serializable] class Report
    {
        public bool complete, rightLead, shortTapsKeepDirection = true, sustainedTurnChangesDirection;
        public bool lookSpeedBounded, cameraSynced = true, hudStayedFixed = true;
        public float maximumLookSpeedFraction, allowedLookSpeedFraction;
        public float rightLookOffset, leftLookOffset, turnDelay, lookSmoothTime;
        public int sampledFrames, rapidTapFrames;
        public List<string> errors = new List<string>();
    }

    static OutpostCameraComfort()
    {
        EditorApplication.delayCall += ApplyPending;
        EditorApplication.playModeStateChanged += PlayState;
    }

    static T[] Find<T>() where T : Component => SceneManager.GetActiveScene().GetRootGameObjects()
        .SelectMany(go => go.GetComponentsInChildren<T>(true)).ToArray();

    static void ApplyPending()
    {
        if (!File.Exists(Request) || EditorApplication.isPlayingOrWillChangePlaymode) return;
        Scene scene = SceneManager.GetActiveScene();
        if (scene.path != "Assets/_Recovery/Mapx11.unity") return;
        EditorSceneManager.SaveScene(scene, Root + "/Backups/Mapx11_BeforeComfort_"
            + DateTime.Now.ToString("yyyyMMdd_HHmmss") + ".unity", true);
        OutpostPlayerCamera camera = Find<OutpostPlayerCamera>().Single();
        Undo.RecordObject(camera, "Calm camera reversals");
        camera.movementLookFraction = 0.07f;
        camera.mouseLookFraction = new Vector2(0.025f, 0.02f);
        camera.maximumHorizontalLookFraction = 0.12f;
        camera.lookSmoothTime = 0.6f;
        camera.turnDelay = 0.3f;
        camera.horizontalLookSpeedFraction = 0.1f;
        EditorUtility.SetDirty(camera);
        EditorSceneManager.MarkSceneDirty(scene);
        if (!EditorSceneManager.SaveScene(scene)) throw new IOException("Mapx11 save failed.");
        File.Delete(Request);
        SessionState.SetBool(Key, true);
        EditorApplication.isPlaying = true;
    }

    static void PlayState(PlayModeStateChange state)
    {
        if (state == PlayModeStateChange.EnteredPlayMode && SessionState.GetBool(Key, false))
        {
            started = Time.unscaledTime;
            stage = 0;
            running = true;
            report = new Report();
            Application.logMessageReceived += Log;
            RenderPipelineManager.endCameraRendering += Sample;
            EditorApplication.update += Timeout;
        }
        if (state == PlayModeStateChange.ExitingPlayMode)
        {
            if (running) Finish("Check was interrupted.");
            RenderPipelineManager.endCameraRendering -= Sample;
            Application.logMessageReceived -= Log;
            EditorApplication.update -= Timeout;
            SessionState.SetBool(Key, false);
        }
    }

    static void Log(string message, string trace, LogType type)
    {
        if (running && (type == LogType.Exception || type == LogType.Error)) report.errors.Add(message);
    }

    static void Timeout()
    {
        if (running && Time.unscaledTime - started > 25) Finish("Camera render check timed out.");
    }

    static void Sample(ScriptableRenderContext context, Camera camera)
    {
        if (!running || !camera.CompareTag("MainCamera")) return;
        try
        {
            float now = Time.unscaledTime;
            if (stage == 0)
            {
                if (now - started < 0.5f) return;
                rig = Find<OutpostPlayerCamera>().Single();
                GameObject actor = rig.target.gameObject;
                actor.GetComponent<PlayerMovement>().enabled = false;
                actor.GetComponent<PlayerJump>().enabled = false;
                body = actor.GetComponent<Rigidbody2D>();
                body.gravityScale = 0;
                body.linearVelocity = new Vector2(5, 0);
                rig.mouseLookEnabled = false;
                rig.SnapToTarget();
                secondary = Find<CameraSync>().Single().GetComponent<Camera>();
                hud = Find<QSkillHUD>().Single().skillImage.rectTransform;
                hudPosition = hud.position;
                previousOffset = 0;
                report.turnDelay = rig.turnDelay;
                report.lookSmoothTime = rig.lookSmoothTime;
                report.allowedLookSpeedFraction = rig.horizontalLookSpeedFraction;
                stage = 1; stageStarted = now;
                return;
            }
            float width = camera.orthographicSize * 2 * camera.aspect;
            float offset = rig.CurrentLookOffset.x;
            if (Time.unscaledDeltaTime > 0)
                report.maximumLookSpeedFraction = Mathf.Max(report.maximumLookSpeedFraction,
                    Mathf.Abs(offset - previousOffset) / (Time.unscaledDeltaTime * width));
            previousOffset = offset;
            report.sampledFrames++;
            report.cameraSynced &= Vector3.Distance(camera.transform.position, secondary.transform.position) < 0.001f;
            report.hudStayedFixed &= Vector3.Distance(hud.position, hudPosition) < 0.01f;

            if (stage == 1 && now - stageStarted > 2)
            {
                report.rightLookOffset = offset;
                report.rightLead = rig.FacingDirection == 1 && offset > 0.5f;
                stage = 2; stageStarted = now;
                body.linearVelocity = new Vector2(-5, 0);
            }
            else if (stage == 2)
            {
                report.rapidTapFrames++;
                report.shortTapsKeepDirection &= rig.FacingDirection == 1;
                float elapsed = now - stageStarted;
                int tap = Mathf.FloorToInt(elapsed / 0.1f);
                body.linearVelocity = new Vector2(tap % 2 == 0 ? -5 : 5, 0);
                if (elapsed > 1)
                {
                    stage = 3; stageStarted = now;
                    body.linearVelocity = new Vector2(-5, 0);
                }
            }
            else if (stage == 3 && now - stageStarted > 3)
            {
                report.leftLookOffset = offset;
                report.sustainedTurnChangesDirection = rig.FacingDirection == -1 && offset < -0.5f;
                Finish(null);
            }
        }
        catch (Exception exception) { Finish(exception.ToString()); }
    }

    static void Finish(string error)
    {
        if (!running) return;
        running = false;
        if (error != null) report.errors.Add(error);
        report.lookSpeedBounded = report.maximumLookSpeedFraction <= report.allowedLookSpeedFraction + 0.005f;
        report.complete = report.rightLead && report.shortTapsKeepDirection && report.sustainedTurnChangesDirection
            && report.lookSpeedBounded && report.cameraSynced && report.hudStayedFixed
            && report.sampledFrames > 20 && report.rapidTapFrames > 5 && report.errors.Count == 0;
        File.WriteAllText(Root + "/CameraComfortCheck.json", JsonUtility.ToJson(report, true));
        Debug.Log("[Outpost] Camera reversal comfort check: " + (report.complete ? "PASS" : "See CameraComfortCheck.json"));
        EditorApplication.isPlaying = false;
    }
}
