using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

[InitializeOnLoad]
public static class ShotPressureSwitchCheck
{
    const string Key = "PressureSwitch.RuntimeCheck";
    static string Root => ShotPressureSwitchSetup.OutputRoot;
    static ShotPressureSwitch target;
    static GameObject bulletPrefab, testDoor;
    static float started, phaseStarted, nextShot, halfCapY, baseY;
    static int phase, sent, expected, activationEvents;
    static bool running;
    static Report report;
    static GameObject shaftProbe, baseProbe;
    static bool shaftPassed, basePassed;
    static float initialCapY, initialHitboxY, ceilingStartY;

    [Serializable] class Report
    {
        public bool complete, materialsCompile, realBulletsReachedSwitch, fiveHitsPartial, shaftMovesBaseFixed;
        public bool recoversGradually, backToNineHitsWithinOneSecond, eightHitsNotActive;
        public bool ninthHitActivates, greenSpriteShown, activationEventOnce, doorCallbackWorks;
        public bool permanentAfterWaitingAndExtraShots, survivesDisableEnable, successEffectOnce;
        public bool ghostFilterWorks, acceptedGhostWorks, oneBulletCountsOnce;
        public bool headOnlyCollider, rodAndBaseShotsPass, hitboxTracksHead;
        public bool ceilingFacesDown, ceilingPressMovesUp, ceilingNineHitsActivate;
        public float halfProgress, recoveryMidProgress, recoveredProgress;
        public int acceptedShotsAtUnlock;
        public string scene;
        public List<string> errors = new List<string>();
    }

    static ShotPressureSwitchCheck()
    {
        EditorApplication.playModeStateChanged += PlayState;
        EditorApplication.delayCall += CheckPending;
        EditorApplication.projectChanged += CheckPending;
    }

    static void CheckPending()
    {
        if (EditorApplication.isCompiling || EditorApplication.isUpdating) return;
        string request = ShotPressureSwitchSetup.EditorRoot + "/RuntimeCheck.request.txt";
        if (File.Exists(request) && !EditorApplication.isPlayingOrWillChangePlaymode)
        { File.Delete(request); StartCheck(); }
    }

    [MenuItem("地图/射击机关/运行实弹验证")]
    public static void StartCheck()
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode) return;
        Directory.CreateDirectory(Root);
        SessionState.SetBool(Key, true);
        EditorApplication.isPlaying = true;
    }

    static T[] Find<T>() where T : Component => SceneManager.GetActiveScene().GetRootGameObjects()
        .SelectMany(root => root.GetComponentsInChildren<T>(true)).ToArray();

    static void PlayState(PlayModeStateChange state)
    {
        if (state == PlayModeStateChange.EnteredPlayMode && SessionState.GetBool(Key, false))
        {
            report = new Report { scene = SceneManager.GetActiveScene().path };
            started = phaseStarted = Time.time;
            phase = sent = activationEvents = 0;
            running = true;
            EditorApplication.update += Tick;
            Application.logMessageReceived += Log;
        }
        if (state == PlayModeStateChange.ExitingPlayMode)
        {
            if (running) Finish("Check interrupted before completion.");
            EditorApplication.update -= Tick;
            Application.logMessageReceived -= Log;
            SessionState.SetBool(Key, false);
        }
    }

    static void Log(string message, string trace, LogType type)
    {
        if (running && (type == LogType.Error || type == LogType.Exception)) report.errors.Add(message);
    }

    static void Stage(int value) { phase = value; phaseStarted = Time.time; }

    static void Fire(bool ghost = false)
    {
        Bounds bounds = target.hitbox.bounds;
        Vector3 position = bounds.center + Vector3.left * (bounds.extents.x + 0.45f);
        position.z = target.transform.position.z;
        GameObject shot = UnityEngine.Object.Instantiate(bulletPrefab, position, Quaternion.identity);
        shot.GetComponent<Bullet>().isGhostBullet = ghost;
    }

    static GameObject Probe(Vector3 point)
    {
        Vector3 position = new Vector3(target.hitbox.bounds.min.x - 0.45f, point.y, target.transform.position.z);
        return UnityEngine.Object.Instantiate(bulletPrefab, position, Quaternion.identity);
    }

    static bool FitsHead()
    {
        Bounds box = target.hitbox.bounds;
        Bounds face = target.capRenderer.bounds;
        return Mathf.Abs(box.center.x - face.center.x) + box.extents.x <= face.extents.x + 0.01f
            && Mathf.Abs(box.center.y - face.center.y) + box.extents.y <= face.extents.y + 0.01f;
    }

    static void FireFromBelow()
    {
        Bounds bounds = target.hitbox.bounds;
        float gap = 0.18f + (sent * 0.11f) % 0.7f;
        Vector3 position = bounds.center + Vector3.down * (bounds.extents.y + gap);
        position.z = target.transform.position.z;
        UnityEngine.Object.Instantiate(bulletPrefab, position, Quaternion.Euler(0, 0, 90));
    }

    static void Burst(int count)
    {
        if (sent < count && Time.time >= nextShot)
        { Fire(); sent++; nextShot = Time.time + 0.1f; }
    }

    static void Capture(string name)
    {
        Directory.CreateDirectory(Root + "/Checks");
        ScreenCapture.CaptureScreenshot(Root + "/Checks/" + name + ".png");
    }

    static void Tick()
    {
        if (!running) return;
        try
        {
            if (Time.time - started > 30) { Finish("Timed out waiting for real bullet collisions in phase " + phase); return; }
            if (phase == 0 && Time.time - started > 0.5f)
            {
                foreach (PlayerShooting shooting in Find<PlayerShooting>()) shooting.enabled = false;
                foreach (PlayerMovement movement in Find<PlayerMovement>()) movement.enabled = false;
                foreach (PlayerJump jump in Find<PlayerJump>()) jump.enabled = false;
                foreach (CameraFollow follow in Find<CameraFollow>()) follow.enabled = false;
                foreach (OutpostPlayerCamera follow in Find<OutpostPlayerCamera>()) follow.enabled = false;
                target = Find<ShotPressureSwitch>().Single();
                report.materialsCompile = !ShaderUtil.ShaderHasError(target.capRenderer.sharedMaterial.shader);
                if (!report.materialsCompile) { Finish("Pressure switch material failed shader compilation."); return; }
                Camera camera = Camera.main;
                camera.transform.position = new Vector3(0, 1, -10);
                bulletPrefab = Find<PlayerShooting>().First().bulletPrefab;
                testDoor = new GameObject("Runtime Door Callback Probe");
                target.onActivated.AddListener(() => { activationEvents++; testDoor.SetActive(false); });
                baseY = target.baseRenderer.transform.localPosition.y;
                Physics2D.SyncTransforms();
                initialCapY = target.capRenderer.bounds.center.y;
                initialHitboxY = target.hitbox.bounds.center.y;
                report.headOnlyCollider = target.GetComponentsInChildren<Collider2D>(true).Length == 1 && FitsHead()
                    && !target.hitbox.OverlapPoint(target.shaftRenderer.bounds.center)
                    && !target.hitbox.OverlapPoint(target.baseRenderer.bounds.center);
                Capture("01-ready");
                shaftPassed = basePassed = false;
                shaftProbe = Probe(target.shaftRenderer.bounds.center);
                baseProbe = Probe(target.baseRenderer.bounds.center);
                Stage(20);
            }
            else if (phase == 20)
            {
                float farSide = target.hitbox.bounds.max.x + 0.1f;
                shaftPassed |= shaftProbe != null && shaftProbe.transform.position.x > farSide;
                basePassed |= baseProbe != null && baseProbe.transform.position.x > farSide;
                if (Time.time - phaseStarted < 0.15f) return;
                report.rodAndBaseShotsPass = shaftPassed && basePassed && target.AcceptedShots == 0;
                if (!report.rodAndBaseShotsPass) { Finish("A shot hitting only the rod/base did not pass through."); return; }
                sent = 0; nextShot = Time.time;
                Stage(1);
            }
            else if (phase == 1)
            {
                Burst(5);
                if (sent == 5 && target.AcceptedShots == 5)
                {
                    report.realBulletsReachedSwitch = true;
                    report.halfProgress = target.Progress01;
                    report.fiveHitsPartial = !target.IsActive && !target.IsLatched && Mathf.Abs(target.Progress01 - 5f / 9) < 0.01f;
                    Stage(2);
                }
            }
            else if (phase == 2 && Time.time - phaseStarted > 0.12f)
            {
                Physics2D.SyncTransforms();
                halfCapY = target.capRenderer.transform.localPosition.y;
                report.hitboxTracksHead = FitsHead() && Mathf.Abs(
                    (target.hitbox.bounds.center.y - initialHitboxY) - (target.capRenderer.bounds.center.y - initialCapY)) < 0.01f;
                report.shaftMovesBaseFixed = halfCapY < target.capRestY - 0.1f
                    && Mathf.Abs(target.baseRenderer.transform.localPosition.y - baseY) < 0.0001f;
                Capture("02-five-hits");
                Stage(3);
            }
            else if (phase == 3 && Time.time - phaseStarted > 0.38f)
            {
                report.recoveryMidProgress = target.Progress01;
                report.recoversGradually = target.Progress01 > 0.05f && target.Progress01 < report.halfProgress - 0.05f
                    && target.capRenderer.transform.localPosition.y > halfCapY;
                Stage(4);
            }
            else if (phase == 4 && Time.time - phaseStarted > 0.58f)
            {
                report.recoveredProgress = target.Progress01;
                report.backToNineHitsWithinOneSecond = target.Progress01 < 0.001f && target.RemainingHits == 9
                    && target.VisualProgress01 < 0.01f;
                Capture("03-recovered");
                sent = 0; nextShot = Time.time;
                Stage(5);
            }
            else if (phase == 5)
            {
                Burst(8);
                if (sent == 8 && target.AcceptedShots == 13)
                {
                    report.eightHitsNotActive = !target.IsLatched && !target.IsActive && target.RemainingHits == 1;
                    Fire();
                    Stage(6);
                }
            }
            else if (phase == 6 && target.IsActive)
            {
                report.acceptedShotsAtUnlock = target.AcceptedShots;
                report.ninthHitActivates = target.AcceptedShots == 14 && target.IsLatched && target.Progress01 == 1f;
                if (target.flat2DParts)
                {
                    MaterialPropertyBlock colors = new MaterialPropertyBlock();
                    target.capRenderer.GetPropertyBlock(colors);
                    report.greenSpriteShown = target.capRenderer.enabled && !target.completedRenderer.enabled
                        && colors.GetFloat("_Activated") > 0.5f;
                }
                else report.greenSpriteShown = target.completedRenderer.enabled && target.completedRenderer.sprite == target.unlockedGreenSprite;
                report.doorCallbackWorks = !testDoor.activeSelf;
                Capture("04-unlocked");
                sent = 0; nextShot = Time.time;
                Stage(7);
            }
            else if (phase == 7)
            {
                Burst(3);
                if (Time.time - phaseStarted > 1.3f)
                {
                    report.permanentAfterWaitingAndExtraShots = target.IsActive && target.Progress01 == 1f && target.AcceptedShots == 14;
                    report.activationEventOnce = activationEvents == 1;
                    report.successEffectOnce = target.feedback.PlayCount == 1;
                    target.gameObject.SetActive(false);
                    target.gameObject.SetActive(true);
                    report.survivesDisableEnable = target.IsActive && target.Progress01 == 1f && activationEvents == 1;
                    Vector3 position = target.transform.position;
                    target.gameObject.SetActive(false);
                    GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(ShotPressureSwitchSetup.PrefabPath);
                    target = UnityEngine.Object.Instantiate(prefab, position, Quaternion.identity).GetComponent<ShotPressureSwitch>();
                    target.acceptGhostBullets = false;
                    BoxCollider2D duplicate = target.gameObject.AddComponent<BoxCollider2D>();
                    duplicate.size = target.hitbox.size;
                    duplicate.offset = target.hitbox.offset;
                    Physics2D.SyncTransforms();
                    Fire(true);
                    Stage(8);
                }
            }
            else if (phase == 8 && Time.time - phaseStarted > 0.2f)
            {
                report.ghostFilterWorks = target.AcceptedShots == 0;
                Fire();
                Stage(9);
            }
            else if (phase == 9 && Time.time - phaseStarted > 0.15f)
            {
                report.oneBulletCountsOnce = target.AcceptedShots == 1;
                target.acceptGhostBullets = true;
                Fire(true);
                Stage(10);
            }
            else if (phase == 10 && Time.time - phaseStarted > 0.15f)
            {
                report.acceptedGhostWorks = target.AcceptedShots == 2;
                Vector3 position = new Vector3(target.transform.position.x, 4.2f, target.transform.position.z);
                target.gameObject.SetActive(false);
                GameObject ceilingPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(ShotPressureSwitchSetup.CeilingPrefabPath);
                if (ceilingPrefab == null) { Finish("Ceiling prefab is missing."); return; }
                target = UnityEngine.Object.Instantiate(ceilingPrefab, position, ceilingPrefab.transform.rotation)
                    .GetComponent<ShotPressureSwitch>();
                Physics2D.SyncTransforms();
                report.ceilingFacesDown = Vector3.Dot(target.transform.up, Vector3.down) > 0.99f && FitsHead();
                ceilingStartY = target.capRenderer.bounds.center.y;
                GameObject floor = GameObject.Find("Ground");
                SpriteRenderer source = floor != null ? floor.GetComponent<SpriteRenderer>() : null;
                if (source != null)
                {
                    SpriteRenderer beam = new GameObject("Temporary Ceiling Beam", typeof(SpriteRenderer)).GetComponent<SpriteRenderer>();
                    beam.sprite = source.sprite;
                    beam.sharedMaterial = source.sharedMaterial;
                    beam.transform.localScale = new Vector3(3f / source.sprite.bounds.size.x, 0.22f / source.sprite.bounds.size.y, 1);
                    beam.transform.position = position + Vector3.up * 0.11f;
                }
                Capture("05-ceiling-ready");
                sent = 0; nextShot = Time.time;
                Stage(21);
            }
            else if (phase == 21)
            {
                if (sent < 9 && Time.time >= nextShot)
                { FireFromBelow(); sent++; nextShot = Time.time + 0.1f; }
                if (target.AcceptedShots >= 5)
                    report.ceilingPressMovesUp |= target.capRenderer.bounds.center.y > ceilingStartY + 0.25f;
                if (!target.IsActive) return;
                report.ceilingNineHitsActivate = sent == 9 && target.AcceptedShots == 9 && FitsHead();
                Capture("06-ceiling-active");
                Finish(null);
            }
        }
        catch (Exception error) { Finish(error.ToString()); }
    }

    static void Finish(string error)
    {
        if (!running) return;
        running = false;
        if (error != null) report.errors.Add(error);
        report.complete = report.materialsCompile && report.realBulletsReachedSwitch && report.fiveHitsPartial && report.shaftMovesBaseFixed
            && report.recoversGradually && report.backToNineHitsWithinOneSecond && report.eightHitsNotActive
            && report.ninthHitActivates && report.greenSpriteShown && report.activationEventOnce && report.doorCallbackWorks
            && report.permanentAfterWaitingAndExtraShots && report.survivesDisableEnable && report.successEffectOnce
            && report.ghostFilterWorks && report.acceptedGhostWorks && report.oneBulletCountsOnce
            && report.headOnlyCollider && report.rodAndBaseShotsPass && report.hitboxTracksHead
            && report.ceilingFacesDown && report.ceilingPressMovesUp && report.ceilingNineHitsActivate && report.errors.Count == 0;
        File.WriteAllText(Root + "/RuntimeCheck.json", JsonUtility.ToJson(report, true));
        Debug.Log("[PressureSwitch] Real bullet / recovery / unlock / door event checks: " + (report.complete ? "PASS" : "See RuntimeCheck.json"));
        EditorApplication.isPlaying = false;
    }
}
