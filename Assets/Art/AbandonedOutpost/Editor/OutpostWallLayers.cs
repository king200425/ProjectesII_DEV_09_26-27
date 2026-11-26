using System;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEditor.EditorTools;
using UnityEditor.SceneManagement;
using UnityEditor.Tilemaps;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Tilemaps;

[InitializeOnLoad]
public static class OutpostWallLayers
{
    const string Root = "Assets/Art/AbandonedOutpost/ReadyToPaint";
    const string NormalName = "04_普通墙";
    const string JumpName = "05_蹬墙墙壁";
    const string InstallRequest = "Assets/Art/AbandonedOutpost/Editor/WallLayers.request.txt";
    const string InstallReport = Root + "/WallLayersSetup.json";
    const string CheckKey = "Outpost.WallLayers.Check";
    static double started;
    static double stepStarted;
    static int step;
    static GameObject actor;
    static Rigidbody2D body;
    static Collider2D actorCollider;
    static Component jump;
    static MethodInfo hasWall;
    static TestReport report;

    [Serializable]
    class TestReport
    {
        public bool completed;
        public bool normalWallBlocks;
        public bool normalWallRejectsWallJump;
        public bool wallJumpWallDetected;
        public bool actualWallJumpPushesAway;
        public string error = "";
    }

    static OutpostWallLayers()
    {
        EditorApplication.delayCall += Bootstrap;
        EditorApplication.playModeStateChanged += PlayState;
    }

    static GameObject SceneGrid()
    {
        Scene scene = SceneManager.GetActiveScene();
        return scene.IsValid() && scene.isLoaded
            ? scene.GetRootGameObjects().FirstOrDefault(go => go.name == "Outpost_MapGrid") : null;
    }

    static void Bootstrap()
    {
        if (EditorApplication.isCompiling || EditorApplication.isUpdating)
        { EditorApplication.delayCall += Bootstrap; return; }
        if (EditorApplication.isPlayingOrWillChangePlaymode) return;
        if (File.Exists(InstallRequest) && (!File.Exists(InstallReport)
            || !File.ReadAllText(InstallReport).Contains(NormalName))) Install();
    }

    static void EnsureWall(GameObject root, string name, bool wallJump)
    {
        Transform child = root.transform.Find(name);
        if (child == null) child = root.transform.Find(wallJump ? "05_WallJump_蹬墙" : "04_NormalWall_普通墙");
        GameObject wall = child != null ? child.gameObject : new GameObject(name, typeof(Tilemap), typeof(TilemapRenderer));
        wall.name = name;
        wall.transform.SetParent(root.transform, false);
        wall.layer = LayerMask.NameToLayer("Ground");
        wall.tag = wallJump ? "WallJump" : "Untagged";
        TilemapRenderer renderer = wall.GetComponent<TilemapRenderer>();
        renderer.sortingOrder = 0;
        Material material = AssetDatabase.LoadAssetAtPath<Material>(
            "Packages/com.unity.render-pipelines.universal/Runtime/Materials/Sprite-Unlit-Default.mat");
        if (material != null) renderer.sharedMaterial = material;
        Rigidbody2D physics = wall.GetComponent<Rigidbody2D>();
        if (physics == null) physics = wall.AddComponent<Rigidbody2D>();
        physics.bodyType = RigidbodyType2D.Static;
        TilemapCollider2D tiles = wall.GetComponent<TilemapCollider2D>();
        if (tiles == null) tiles = wall.AddComponent<TilemapCollider2D>();
        CompositeCollider2D composite = wall.GetComponent<CompositeCollider2D>();
        if (composite == null) composite = wall.AddComponent<CompositeCollider2D>();
        composite.geometryType = CompositeCollider2D.GeometryType.Polygons;
        composite.generationType = CompositeCollider2D.GenerationType.Synchronous;
        tiles.compositeOperation = Collider2D.CompositeOperation.Merge;
    }

    static void EnsureBoth(GameObject root)
    {
        EnsureWall(root, NormalName, false);
        EnsureWall(root, JumpName, true);
    }

    [MenuItem("地图/配置普通墙与蹬墙")]
    public static void Install()
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode) return;
        GameObject grid = SceneGrid();
        if (grid == null) throw new InvalidOperationException("Open Mapx11 before configuring wall layers.");
        Scene scene = SceneManager.GetActiveScene();
        string backup = Root + "/Backups/" + scene.name + "_BeforeWallLayers_" + DateTime.Now.ToString("yyyyMMdd_HHmmss") + ".unity";
        if (!EditorSceneManager.SaveScene(scene, backup, true)) throw new IOException("Could not back up the map scene.");
        EnsureBoth(grid);
        EditorSceneManager.MarkSceneDirty(scene);
        if (!EditorSceneManager.SaveScene(scene)) throw new IOException("Could not save wall layer configuration.");
        string prefabPath = Root + "/Prefabs/Outpost_MapGrid.prefab";
        GameObject prefab = PrefabUtility.LoadPrefabContents(prefabPath);
        try { EnsureBoth(prefab); PrefabUtility.SaveAsPrefabAsset(prefab, prefabPath); }
        finally { PrefabUtility.UnloadPrefabContents(prefab); }
        File.WriteAllText(InstallReport, "{\"floor\":\"01_Terrain\",\"normalWall\":\"" + NormalName
            + "\",\"wallJump\":\"" + JumpName + "\",\"wallJumpTag\":\"WallJump\"}");
        AssetDatabase.ImportAsset(InstallReport);
        AssetDatabase.SaveAssets();
        EditorApplication.delayCall += OutpostQuickStart.StartBuilding;
        Debug.Log("[Outpost] F6 地板/平台；F7 普通墙；F8 蹬墙墙壁。蹬墙层的碰撞体已设置 WallJump 标签。");
    }

    [MenuItem("地图/地板与平台")]
    static void Floor() { OutpostQuickStart.StartBuilding(); }
    [MenuItem("地图/普通墙 _F7")]
    public static void NormalWall() { SelectWall(NormalName); }
    [MenuItem("地图/蹬墙墙壁 _F8")]
    public static void JumpWall() { SelectWall(JumpName); }

    static void SelectWall(string name)
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode) return;
        GameObject grid = SceneGrid();
        if (grid == null) return;
        EnsureBoth(grid);
        GameObject palette = AssetDatabase.LoadAssetAtPath<GameObject>(Root + "/Palettes/AO_00_QuickBlocks.prefab");
        GridPaintPaletteWindow window = EditorWindow.GetWindow<GridPaintPaletteWindow>();
        window.Show();
        if (GridPaintingState.palette != palette) GridPaintingState.palette = palette;
        GameObject target = grid.transform.Find(name).gameObject;
        Selection.activeGameObject = target;
        GridPaintingState.scenePaintTarget = target;
        GridBrush brush = GridPaintingState.gridBrush as GridBrush;
        Tile tile = AssetDatabase.LoadAllAssetsAtPath(Root + "/Tiles/AO_00_QuickBlocks.asset")
            .OfType<Tile>().First(t => t.name == "Block_01");
        if (brush != null)
        {
            brush.Reset();
            brush.SetTile(Vector3Int.zero, tile);
            brush.SetColor(Vector3Int.zero, Color.white);
            brush.SetMatrix(Vector3Int.zero, Matrix4x4.identity);
        }
        ToolManager.SetActiveTool<PaintTool>();
        SceneView.lastActiveSceneView?.Focus();
        SceneView.RepaintAll();
    }

    [MenuItem("地图/检查两种墙的蹬墙区别")]
    public static void StartCheck()
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode || SceneGrid() == null) return;
        EditorSceneManager.SaveScene(SceneManager.GetActiveScene());
        SessionState.SetBool(CheckKey, true);
        EditorApplication.isPlaying = true;
    }

    static void PlayState(PlayModeStateChange state)
    {
        if (state == PlayModeStateChange.EnteredPlayMode && SessionState.GetBool(CheckKey, false))
        {
            report = new TestReport();
            step = 0;
            started = stepStarted = EditorApplication.timeSinceStartup;
            EditorApplication.update += Tick;
        }
        if (state == PlayModeStateChange.ExitingPlayMode) EditorApplication.update -= Tick;
    }

    static void PositionBeside(float wallX)
    {
        Vector3 offset = actorCollider.bounds.center - actor.transform.position;
        actor.transform.position = new Vector3(wallX - actorCollider.bounds.extents.x - 0.01f, 4.5f, 0) - offset;
        body.linearVelocity = Vector2.zero;
        Physics2D.SyncTransforms();
    }

    static void Tick()
    {
        try
        {
            double now = EditorApplication.timeSinceStartup;
            if (now - started > 8) throw new TimeoutException("Wall jump check timed out.");
            if (step == 0 && now - stepStarted > 0.5)
            {
                GameObject grid = SceneGrid();
                EnsureBoth(grid);
                Tile tile = AssetDatabase.LoadAllAssetsAtPath(Root + "/Tiles/AO_00_QuickBlocks.asset")
                    .OfType<Tile>().First(t => t.sprite != null);
                foreach (var spec in new[] { (NormalName, 100), (JumpName, 110) })
                {
                    Tilemap map = grid.transform.Find(spec.Item1).GetComponent<Tilemap>();
                    for (int y = 0; y < 8; y++) map.SetTile(new Vector3Int(spec.Item2, y, 0), tile);
                    map.GetComponent<TilemapCollider2D>().ProcessTilemapChanges();
                }
                actor = GameObject.Find("Player");
                body = actor.GetComponent<Rigidbody2D>();
                actorCollider = actor.GetComponent<Collider2D>();
                jump = actor.GetComponent("PlayerJump");
                hasWall = jump.GetType().GetMethod("HasWall", BindingFlags.Instance | BindingFlags.NonPublic);
                PositionBeside(100);
                RaycastHit2D[] hits = Physics2D.RaycastAll(actorCollider.bounds.center, Vector2.right, actorCollider.bounds.extents.x + 0.1f);
                report.normalWallBlocks = hits.Any(hit => hit.collider.gameObject.name == NormalName);
                report.normalWallRejectsWallJump = !(bool)hasWall.Invoke(jump, new object[] { 1 });
                PositionBeside(110);
                report.wallJumpWallDetected = (bool)hasWall.Invoke(jump, new object[] { 1 });
                step = 1;
                stepStarted = now;
            }
            else if (step == 1 && now - stepStarted > 0.08)
            {
                int side = (int)jump.GetType().GetProperty("WallSide").GetValue(jump);
                if (side != 1) return;
                jump.GetType().GetField("jumpPressed", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(jump, true);
                step = 2;
                stepStarted = now;
            }
            else if (step == 2 && now - stepStarted > 0.05)
            {
                report.actualWallJumpPushesAway = body.linearVelocity.x < -1 && body.linearVelocity.y > 0.5f;
                report.completed = report.normalWallBlocks && report.normalWallRejectsWallJump
                    && report.wallJumpWallDetected && report.actualWallJumpPushesAway;
                Finish();
            }
        }
        catch (Exception exception) { report.error = exception.ToString(); Finish(); }
    }

    static void Finish()
    {
        EditorApplication.update -= Tick;
        SessionState.SetBool(CheckKey, false);
        File.WriteAllText(Root + "/WallLayersRuntimeCheck.json", JsonUtility.ToJson(report, true));
        Debug.Log("[Outpost] Normal wall / wall jump distinction: " + (report.completed ? "PASS" : JsonUtility.ToJson(report)));
        EditorApplication.isPlaying = false;
    }
}
