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
using Object = UnityEngine.Object;

[InitializeOnLoad]
public static class OutpostQuickStart
{
    const string Root = "Assets/Art/AbandonedOutpost/ReadyToPaint";
    const string PalettePath = Root + "/Palettes/AO_00_QuickBlocks.prefab";
    const string LibraryPath = Root + "/Tiles/AO_00_QuickBlocks.asset";
    const string ConfigureReport = Root + "/QuickStartReport.json";
    const string CheckRequest = "Outpost.QuickStart.RuntimeCheck";
    const string InstallRequest = "Assets/Art/AbandonedOutpost/Editor/QuickStart.request.txt";
    static bool restoring;
    static double checkStarted;
    static double stageStarted;
    static int stage;
    static float groundedY;
    static float savedTimeScale;
    static GameObject player;
    static Rigidbody2D playerBody;
    static Tilemap groundMap;
    static CheckReport check;

    [Serializable]
    class CheckReport
    {
        public string scene;
        public bool completed;
        public bool paletteHas12Blocks;
        public bool correctGroundLayer;
        public bool compositeCollider;
        public bool realPlayerLandedOnTiles;
        public bool realPlayerJumped;
        public bool realPlayerLandedAfterJump;
        public string error = "";
    }

    static OutpostQuickStart()
    {
        EditorSceneManager.sceneOpened += (scene, mode) => ScheduleRestore(true);
        EditorApplication.playModeStateChanged += PlayStateChanged;
        GridPaintingState.paletteChanged += PaletteChanged;
        EditorApplication.delayCall += Bootstrap;
    }

    static void Bootstrap()
    {
        if (EditorApplication.isCompiling || EditorApplication.isUpdating)
        { EditorApplication.delayCall += Bootstrap; return; }
        if (EditorApplication.isPlayingOrWillChangePlaymode) return;
        if (File.Exists(InstallRequest) && !File.Exists(ConfigureReport))
        {
            ConfigureCurrentScene();
            return;
        }
        ScheduleRestore(false);
    }

    static GameObject GridRoot()
    {
        Scene scene = SceneManager.GetActiveScene();
        return scene.IsValid() && scene.isLoaded
            ? scene.GetRootGameObjects().FirstOrDefault(go => go.name == "Outpost_MapGrid") : null;
    }

    static void Folder(string path)
    {
        if (AssetDatabase.IsValidFolder(path)) return;
        int slash = path.LastIndexOf('/');
        Folder(path.Substring(0, slash));
        AssetDatabase.CreateFolder(path.Substring(0, slash), path.Substring(slash + 1));
    }

    static GameObject MakeQuickPalette()
    {
        GameObject existing = AssetDatabase.LoadAssetAtPath<GameObject>(PalettePath);
        if (existing != null) return existing;
        Sprite[] sprites = AssetDatabase.LoadAllAssetsAtPath(
            "Assets/Art/AbandonedOutpost/tilesets/tileset-platforms-3-16x16.png").OfType<Sprite>().ToArray();
        string[] cells = {
            "r00_c02", "r00_c03", "r01_c02", "r01_c03",
            "r00_c04", "r01_c04", "r00_c07", "r01_c07",
            "r00_c00", "r00_c01", "r01_c00", "r01_c01"
        };
        Tile library = ScriptableObject.CreateInstance<Tile>();
        library.name = "QuickBlocks_Library";
        AssetDatabase.CreateAsset(library, LibraryPath);
        GameObject palette = GridPaletteUtility.CreateNewPalette(Root + "/Palettes", "AO_00_QuickBlocks",
            GridLayout.CellLayout.Rectangle, GridPalette.CellSizing.Manual, Vector3.one, GridLayout.CellSwizzle.XYZ);
        GameObject editable = PrefabUtility.LoadPrefabContents(PalettePath);
        try
        {
            Tilemap map = editable.GetComponentInChildren<Tilemap>();
            for (int i = 0; i < cells.Length; i++)
            {
                Sprite sprite = sprites.FirstOrDefault(s => s.name.EndsWith(cells[i], StringComparison.Ordinal));
                if (sprite == null) throw new InvalidOperationException("Missing quick block " + cells[i]);
                Tile tile = ScriptableObject.CreateInstance<Tile>();
                tile.name = "Block_" + (i + 1).ToString("D2");
                tile.sprite = sprite;
                tile.colliderType = Tile.ColliderType.Grid;
                tile.flags = TileFlags.LockAll;
                AssetDatabase.AddObjectToAsset(tile, LibraryPath);
                map.SetTile(new Vector3Int((i % 4) * 2, -(i / 4) * 2, 0), tile);
            }
            Material material = AssetDatabase.LoadAssetAtPath<Material>(
                "Packages/com.unity.render-pipelines.universal/Runtime/Materials/Sprite-Unlit-Default.mat");
            if (material != null) map.GetComponent<TilemapRenderer>().sharedMaterial = material;
            map.CompressBounds();
            PrefabUtility.SaveAsPrefabAsset(editable, PalettePath);
        }
        finally { PrefabUtility.UnloadPrefabContents(editable); }
        AssetDatabase.SaveAssets();
        return AssetDatabase.LoadAssetAtPath<GameObject>(PalettePath);
    }

    [MenuItem("地图/开始搭建 _F6")]
    public static void StartBuilding()
    {
        if (GridRoot() == null) ConfigureCurrentScene();
        else ScheduleRestore(true);
    }

    static void ConfigureCurrentScene()
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode) return;
        Scene scene = SceneManager.GetActiveScene();
        if (!scene.IsValid() || !scene.isLoaded || string.IsNullOrEmpty(scene.path))
            throw new InvalidOperationException("Open the Mapx11 scene first.");
        Folder(Root + "/Backups");
        EditorSceneManager.SaveScene(scene, Root + "/Backups/" + scene.name + "_BeforeQuickStart_"
            + DateTime.Now.ToString("yyyyMMdd_HHmmss") + ".unity", true);
        MakeQuickPalette();
        GameObject grid = GridRoot();
        if (grid == null)
        {
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(Root + "/Prefabs/Outpost_MapGrid.prefab");
            if (prefab == null) throw new InvalidOperationException("Map grid prefab is missing.");
            grid = (GameObject)PrefabUtility.InstantiatePrefab(prefab, scene);
            Undo.RegisterCreatedObjectUndo(grid, "Prepare block building");
        }
        if (PrefabUtility.IsPartOfPrefabInstance(grid))
            PrefabUtility.UnpackPrefabInstance(grid, PrefabUnpackMode.Completely, InteractionMode.AutomatedAction);
        GameObject terrain = grid.transform.Find("01_Terrain").gameObject;
        terrain.layer = LayerMask.NameToLayer("Ground");
        TilemapCollider2D tiles = terrain.GetComponent<TilemapCollider2D>();
        if (tiles == null) tiles = Undo.AddComponent<TilemapCollider2D>(terrain);
        Rigidbody2D body = terrain.GetComponent<Rigidbody2D>();
        if (body == null) body = Undo.AddComponent<Rigidbody2D>(terrain);
        body.bodyType = RigidbodyType2D.Static;
        CompositeCollider2D composite = terrain.GetComponent<CompositeCollider2D>();
        if (composite == null) composite = Undo.AddComponent<CompositeCollider2D>(terrain);
        composite.geometryType = CompositeCollider2D.GeometryType.Polygons;
        composite.generationType = CompositeCollider2D.GenerationType.Synchronous;
        tiles.compositeOperation = Collider2D.CompositeOperation.Merge;
        EditorSceneManager.MarkSceneDirty(scene);
        if (!EditorSceneManager.SaveScene(scene)) throw new IOException("Could not save the configured map scene.");
        File.WriteAllText(ConfigureReport, "{\"scene\":\"" + scene.path + "\",\"quickBlocks\":12,\"autoRestore\":true}");
        AssetDatabase.ImportAsset(ConfigureReport);
        AssetDatabase.SaveAssets();
        ScheduleRestore(true);
        Debug.Log("[Outpost] 常用方块已就绪。打开 Mapx11 或按 F6，就能选块绘制。");
    }

    static void ScheduleRestore(bool frame)
    {
        if (restoring) return;
        restoring = true;
        EditorApplication.delayCall += () => {
            try { Restore(frame); }
            finally { restoring = false; }
        };
    }

    static void Restore(bool frame)
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode || GridRoot() == null) return;
        GameObject palette = AssetDatabase.LoadAssetAtPath<GameObject>(PalettePath);
        if (palette == null) return;
        GridPaintPaletteWindow window = EditorWindow.GetWindow<GridPaintPaletteWindow>();
        window.Show();
        if (!GridPaintingState.palettes.Contains(palette)) return;
        GridPaintingState.palette = palette;
        GameObject target = GridRoot().transform.Find("01_Terrain").gameObject;
        Selection.activeGameObject = target;
        GridPaintingState.scenePaintTarget = target;
        GridBrush brush = GridPaintingState.gridBrush as GridBrush;
        Tile block = AssetDatabase.LoadAllAssetsAtPath(LibraryPath).OfType<Tile>()
            .FirstOrDefault(t => t.name == "Block_01");
        if (brush != null && block != null)
        {
            brush.Reset();
            brush.SetTile(Vector3Int.zero, block);
            brush.SetColor(Vector3Int.zero, Color.white);
            brush.SetMatrix(Vector3Int.zero, Matrix4x4.identity);
        }
        ToolManager.SetActiveTool<PaintTool>();
        SceneView view = SceneView.lastActiveSceneView;
        if (view != null)
        {
            view.in2DMode = true;
            if (frame)
            {
                GameObject actor = GameObject.Find("Player");
                Vector3 center = actor != null ? actor.transform.position : Vector3.zero;
                view.LookAt(new Vector3(center.x, center.y, 0), Quaternion.identity, 9f, true, true);
            }
            view.Focus();
        }
        FrameQuickPalette(window);
    }

    static void PaletteChanged(GameObject palette)
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode || palette == null
            || AssetDatabase.GetAssetPath(palette) != PalettePath || GridRoot() == null) return;
        GameObject target = GridRoot().transform.Find("01_Terrain").gameObject;
        Selection.activeGameObject = target;
        GridPaintingState.scenePaintTarget = target;
        var window = Resources.FindObjectsOfTypeAll<GridPaintPaletteWindow>().FirstOrDefault();
        if (window != null) FrameQuickPalette(window);
    }

    static void FrameQuickPalette(GridPaintPaletteWindow window)
    {
        EditorApplication.delayCall += () => {
            if (window == null) return;
            object clipboard = typeof(GridPaintPaletteWindow).GetProperty("clipboardView",
                BindingFlags.Instance | BindingFlags.NonPublic)?.GetValue(window);
            clipboard?.GetType().GetMethod("FrameEntirePalette", BindingFlags.Instance | BindingFlags.Public)?.Invoke(clipboard, null);
            window.Repaint();
        };
    }

    [MenuItem("地图/检查方块碰撞和跳跃")]
    public static void StartRuntimeCheck()
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode || GridRoot() == null) return;
        EditorSceneManager.SaveScene(SceneManager.GetActiveScene());
        SessionState.SetBool(CheckRequest, true);
        EditorApplication.isPlaying = true;
    }

    static void PlayStateChanged(PlayModeStateChange change)
    {
        if (change == PlayModeStateChange.EnteredPlayMode && SessionState.GetBool(CheckRequest, false))
        {
            check = new CheckReport { scene = SceneManager.GetActiveScene().path };
            stage = 0;
            checkStarted = EditorApplication.timeSinceStartup;
            stageStarted = checkStarted;
            savedTimeScale = Time.timeScale;
            Time.timeScale = 1;
            EditorApplication.update += TickCheck;
        }
        if (change == PlayModeStateChange.ExitingPlayMode)
        {
            EditorApplication.update -= TickCheck;
            if (check != null) Time.timeScale = savedTimeScale;
        }
        if (change == PlayModeStateChange.EnteredEditMode)
        {
            if (File.Exists(Root + "/RuntimeCheck.json")) AssetDatabase.ImportAsset(Root + "/RuntimeCheck.json");
            ScheduleRestore(false);
        }
    }

    static void TickCheck()
    {
        try
        {
            double now = EditorApplication.timeSinceStartup;
            if (now - checkStarted > 12) throw new TimeoutException("Player did not complete the landing/jump check within 12 seconds.");
            if (stage == 0 && now - stageStarted > 0.5)
            {
                GameObject grid = GridRoot();
                player = GameObject.Find("Player");
                if (grid == null || player == null) throw new InvalidOperationException("Player or map grid is missing.");
                playerBody = player.GetComponent<Rigidbody2D>();
                groundMap = grid.transform.Find("01_Terrain").GetComponent<Tilemap>();
                check.correctGroundLayer = groundMap.gameObject.layer == LayerMask.NameToLayer("Ground");
                check.compositeCollider = groundMap.GetComponent<CompositeCollider2D>() != null;
                Tile[] blocks = AssetDatabase.LoadAllAssetsAtPath(LibraryPath).OfType<Tile>().Where(t => t.sprite != null).ToArray();
                check.paletteHas12Blocks = blocks.Length == 12;
                for (int x = 100; x < 114; x++) groundMap.SetTile(new Vector3Int(x, 0, 0), blocks[0]);
                groundMap.GetComponent<TilemapCollider2D>().ProcessTilemapChanges();
                player.transform.position = groundMap.GetCellCenterWorld(new Vector3Int(106, 5, 0));
                playerBody.linearVelocity = Vector2.zero;
                Physics2D.SyncTransforms();
                stage = 1;
                stageStarted = now;
            }
            else if (stage == 1 && now - stageStarted > 0.3 && OnTestGround())
            {
                check.realPlayerLandedOnTiles = true;
                groundedY = player.transform.position.y;
                Component controller = player.GetComponent("PlayerJump");
                FieldInfo button = controller != null ? controller.GetType().GetField("jumpPressed", BindingFlags.Instance | BindingFlags.NonPublic) : null;
                if (button == null) throw new InvalidOperationException("Player jump input could not be triggered.");
                button.SetValue(controller, true);
                stage = 2;
                stageStarted = now;
            }
            else if (stage == 2 && player.transform.position.y > groundedY + 0.4f)
            {
                check.realPlayerJumped = true;
                stage = 3;
                stageStarted = now;
            }
            else if (stage == 3 && now - stageStarted > 0.3 && OnTestGround())
            {
                check.realPlayerLandedAfterJump = true;
                check.completed = check.paletteHas12Blocks && check.correctGroundLayer && check.compositeCollider;
                FinishCheck();
            }
        }
        catch (Exception exception)
        {
            check.error = exception.ToString();
            FinishCheck();
        }
    }

    static bool OnTestGround()
    {
        Transform feet = player.transform.Find("GroundCheck");
        if (feet == null || Mathf.Abs(playerBody.linearVelocity.y) > 0.2f) return false;
        Collider2D[] contacts = Physics2D.OverlapCircleAll(feet.position, 0.16f, 1 << groundMap.gameObject.layer);
        return contacts.Any(c => c.gameObject == groundMap.gameObject);
    }

    static void FinishCheck()
    {
        EditorApplication.update -= TickCheck;
        SessionState.SetBool(CheckRequest, false);
        File.WriteAllText(Root + "/RuntimeCheck.json", JsonUtility.ToJson(check, true));
        Debug.Log("[Outpost] Block collision/jump check: " + (check.completed ? "PASS" : check.error));
        EditorApplication.isPlaying = false;
    }
}
