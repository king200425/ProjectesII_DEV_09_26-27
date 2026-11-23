using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

[InitializeOnLoad]
public static class OutpostMotionClarity
{
    const string Root = "Assets/Art/AbandonedOutpost/ReadyToPaint";
    const string Request = "Assets/Art/AbandonedOutpost/Editor/MotionClarity.request.txt";
    const string Pending = "Outpost.MotionClarity.Check";
    static Rigidbody2D body;
    static OutpostPlayerCamera rig;
    static Camera colorCamera;
    static Vector3 initialPosition, hudPosition;
    static RectTransform hud;
    static double started, stageStarted;
    static int stage;
    static bool running;
    static Report report;

    [Serializable]
    class Report
    {
        public bool complete, pointFiltered, interpolated, movedRight, movedLeft;
        public bool cameraSynced, hudStayedFixed, interpolatedRenderFrames, crispRenderedAlpha;
        public int renderedFrames, interpolationSamples, partiallyTransparentPixels, opaquePixels;
        public float rightViewportX, leftViewportX, horizontalDistance;
        public string scene;
        public List<string> errors = new List<string>();
    }

    static OutpostMotionClarity()
    {
        EditorApplication.delayCall += AutoApply;
        EditorApplication.playModeStateChanged += PlayState;
    }

    static T[] InScene<T>() where T : Component => SceneManager.GetActiveScene().GetRootGameObjects()
        .SelectMany(go => go.GetComponentsInChildren<T>(true)).ToArray();

    static void AutoApply()
    {
        if (!File.Exists(Request) || EditorApplication.isPlayingOrWillChangePlaymode) return;
        if (SceneManager.GetActiveScene().path != "Assets/_Recovery/Mapx11.unity") return;
        Apply();
        File.Delete(Request);
        AssetDatabase.Refresh();
        StartCheck();
    }

    [MenuItem("地图/修正主角移动清晰度")]
    public static void Apply()
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode) return;
        Scene scene = SceneManager.GetActiveScene();
        if (scene.path != "Assets/_Recovery/Mapx11.unity")
            throw new InvalidOperationException("Open Mapx11 before applying the clarity settings.");
        Rigidbody2D playerBody = InScene<PlayerMovement>().Single().GetComponent<Rigidbody2D>();
        Camera auxiliary = InScene<CameraSync>().Single().GetComponent<Camera>();
        RenderTexture texture = auxiliary.targetTexture;
        if (texture == null) throw new InvalidOperationException("Player color RenderTexture is missing.");
        string stamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
        EditorSceneManager.SaveScene(scene, Root + "/Backups/Mapx11_BeforeClarity_" + stamp + ".unity", true);
        File.Copy(AssetDatabase.GetAssetPath(texture), Root + "/Backups/PlayerColorRT_BeforeClarity_" + stamp + ".txt", false);
        Undo.RecordObject(playerBody, "Smooth player render motion");
        playerBody.interpolation = RigidbodyInterpolation2D.Interpolate;
        Undo.RecordObject(texture, "Use point filtering for pixel character");
        texture.filterMode = FilterMode.Point;
        EditorUtility.SetDirty(playerBody);
        EditorUtility.SetDirty(texture);
        EditorSceneManager.MarkSceneDirty(scene);
        if (!EditorSceneManager.SaveScene(scene)) throw new IOException("Could not save Mapx11.");
        AssetDatabase.SaveAssets();
        SetPreviewOneToOne();
        Debug.Log("[Outpost] Motion clarity: player interpolation enabled; color texture uses Point filtering.");
    }

    static void SetPreviewOneToOne()
    {
        Type type = typeof(Editor).Assembly.GetType("UnityEditor.GameView");
        EditorWindow game = EditorWindow.GetWindow(type);
        FieldInfo field = type.GetField("m_ZoomArea", BindingFlags.Instance | BindingFlags.NonPublic);
        object zoom = field?.GetValue(game);
        PropertyInfo scale = zoom?.GetType().GetProperty("scale", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        if (scale != null && scale.CanWrite) scale.SetValue(zoom, Vector2.one);
        game.Repaint();
    }

    [MenuItem("地图/检查主角移动清晰度")]
    public static void StartCheck()
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode) return;
        SessionState.SetBool(Pending, true);
        EditorApplication.isPlaying = true;
    }

    static void PlayState(PlayModeStateChange state)
    {
        if (state == PlayModeStateChange.EnteredPlayMode && SessionState.GetBool(Pending, false))
        {
            report = new Report { scene = SceneManager.GetActiveScene().path, cameraSynced = true, hudStayedFixed = true };
            started = stageStarted = EditorApplication.timeSinceStartup;
            stage = 0;
            running = true;
            Application.logMessageReceived += Log;
            EditorApplication.update += Tick;
            RenderPipelineManager.endCameraRendering += Sample;
        }
        if (state == PlayModeStateChange.ExitingPlayMode)
        {
            if (running) Finish("Play mode exited before the check completed.");
            EditorApplication.update -= Tick;
            RenderPipelineManager.endCameraRendering -= Sample;
            Application.logMessageReceived -= Log;
        }
        if (state == PlayModeStateChange.EnteredEditMode)
        {
            SessionState.SetBool(Pending, false);
            SetPreviewOneToOne();
        }
    }

    static void Log(string message, string trace, LogType type)
    {
        if (running && (type == LogType.Error || type == LogType.Exception)) report.errors.Add(message);
    }

    static void Tick()
    {
        if (!running) return;
        try
        {
            double now = EditorApplication.timeSinceStartup;
            if (now - started > 30) { Finish("Movement render check timed out."); return; }
            if (stage == 0 && now - started > 0.7)
            {
                rig = InScene<OutpostPlayerCamera>().Single();
                GameObject actor = rig.target.gameObject;
                body = actor.GetComponent<Rigidbody2D>();
                actor.GetComponent<PlayerMovement>().enabled = false;
                actor.GetComponent<PlayerJump>().enabled = false;
                body.gravityScale = 0;
                body.linearVelocity = new Vector2(5, 0);
                actor.GetComponentInChildren<Animator>().SetBool("isMoving", true);
                rig.mouseLookEnabled = false;
                colorCamera = InScene<CameraSync>().Single().GetComponent<Camera>();
                hud = InScene<QSkillHUD>().Single().skillImage.rectTransform;
                hudPosition = hud.position;
                initialPosition = actor.transform.position;
                report.pointFiltered = colorCamera.targetTexture.filterMode == FilterMode.Point;
                report.interpolated = body.interpolation == RigidbodyInterpolation2D.Interpolate;
                stage = 1; stageStarted = now;
            }
            else if (stage == 1 && now - stageStarted > 1.1)
            {
                report.horizontalDistance = body.position.x - initialPosition.x;
                report.rightViewportX = rig.viewCamera.WorldToViewportPoint(rig.target.position).x;
                report.movedRight = report.horizontalDistance > 1 && report.rightViewportX < 0.5f;
                body.linearVelocity = new Vector2(-5, 0);
                stage = 2; stageStarted = now;
            }
            else if (stage == 2 && now - stageStarted > 1.6)
            {
                report.leftViewportX = rig.viewCamera.WorldToViewportPoint(rig.target.position).x;
                report.movedLeft = body.position.x < initialPosition.x && report.leftViewportX > 0.5f;
                Finish(null);
            }
        }
        catch (Exception ex) { Finish(ex.ToString()); }
    }

    static void Sample(ScriptableRenderContext context, Camera camera)
    {
        if (!running || stage == 0 || camera != colorCamera) return;
        report.renderedFrames++;
        float gap = Mathf.Abs(body.position.x - rig.target.position.x);
        if (gap > 0.0001f && gap < 0.3f) report.interpolationSamples++;
        report.cameraSynced &= Vector3.Distance(camera.transform.position, rig.viewCamera.transform.position) < 0.001f
            && Mathf.Abs(camera.orthographicSize - rig.viewCamera.orthographicSize) < 0.0001f;
        report.hudStayedFixed &= Vector3.Distance(hud.position, hudPosition) < 0.01f;
        if (stage == 2 && report.opaquePixels == 0)
        {
            RenderTexture old = RenderTexture.active;
            RenderTexture.active = colorCamera.targetTexture;
            Texture2D frame = new Texture2D(colorCamera.targetTexture.width, colorCamera.targetTexture.height, TextureFormat.RGBA32, false);
            frame.ReadPixels(new Rect(0, 0, frame.width, frame.height), 0, 0);
            frame.Apply();
            foreach (Color32 pixel in frame.GetPixels32())
            {
                if (pixel.a == 255) report.opaquePixels++;
                else if (pixel.a != 0) report.partiallyTransparentPixels++;
            }
            RenderTexture.active = old;
            UnityEngine.Object.DestroyImmediate(frame);
        }
    }

    static void Finish(string error)
    {
        if (!running) return;
        running = false;
        if (error != null) report.errors.Add(error);
        report.interpolatedRenderFrames = report.interpolationSamples > 0;
        report.crispRenderedAlpha = report.opaquePixels > 100 && report.partiallyTransparentPixels == 0;
        report.complete = report.pointFiltered && report.interpolated && report.movedRight && report.movedLeft
            && report.cameraSynced && report.hudStayedFixed && report.interpolatedRenderFrames
            && report.crispRenderedAlpha && report.errors.Count == 0;
        File.WriteAllText(Root + "/MotionClarityCheck.json", JsonUtility.ToJson(report, true));
        Debug.Log("[Outpost] Movement render check: " + (report.complete ? "PASS" : "See MotionClarityCheck.json"));
        SessionState.SetBool(Pending, false);
        EditorApplication.isPlaying = false;
    }
}
