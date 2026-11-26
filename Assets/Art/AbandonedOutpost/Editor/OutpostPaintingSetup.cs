using System;
using System.Collections.Generic;
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

// Editor-only setup. All generated assets stay beside the downloaded pack.
public static class OutpostPaintingSetup
{
    const string Root = "Assets/Art/AbandonedOutpost";
    const string Generated = Root + "/ReadyToPaint";
    const string Request = Root + "/Editor/Setup.request.txt";
    const string ReportPath = Generated + "/SetupReport.json";
    const string MapRootName = "Outpost_MapGrid";
    static readonly string[] PaletteNames = {
        "AO_01_Terrain", "AO_02_Backgrounds", "AO_03_Structures", "AO_04_Props"
    };
    static bool running;

    [Serializable]
    class Report
    {
        public string scene;
        public string gridPrefab;
        public string[] palettes;
        public int[] tileCounts;
        public string paintTarget;
        public int groundLayer;
        public bool terrainHasCollider;
    }

    [InitializeOnLoadMethod]
    static void OnLoad()
    {
        GridPaintingState.paletteChanged -= MatchPaintingLayer;
        GridPaintingState.paletteChanged += MatchPaintingLayer;
        if (File.Exists(Request) && !File.Exists(ReportPath))
            EditorApplication.delayCall += AutoSetup;
    }

    static void MatchPaintingLayer(GameObject palette)
    {
        if (palette == null || !AssetDatabase.GetAssetPath(palette).StartsWith(Generated + "/Palettes/", StringComparison.Ordinal)) return;
        int group = Array.IndexOf(PaletteNames, palette.name);
        GameObject grid = FindSceneGrid();
        if (group < 0 || grid == null) return;
        string layer = group == 0 ? "01_Terrain" : group == 1 ? "02_Background" : "03_Decoration";
        GameObject target = grid.transform.Find(layer).gameObject;
        Selection.activeGameObject = target;
        GridPaintingState.scenePaintTarget = target;
        GridPaintPaletteWindow window = Resources.FindObjectsOfTypeAll<GridPaintPaletteWindow>().FirstOrDefault();
        if (window != null) FramePalette(window);
    }

    static void FramePalette(GridPaintPaletteWindow window)
    {
        EditorApplication.delayCall += () => {
            if (window == null) return;
            var property = typeof(GridPaintPaletteWindow).GetProperty("clipboardView", BindingFlags.Instance | BindingFlags.NonPublic);
            object clipboard = property != null ? property.GetValue(window) : null;
            if (clipboard != null)
                clipboard.GetType().GetMethod("FrameEntirePalette", BindingFlags.Instance | BindingFlags.Public)?.Invoke(clipboard, null);
            window.Repaint();
        };
    }

    static void AutoSetup()
    {
        if (EditorApplication.isCompiling || EditorApplication.isUpdating)
        {
            EditorApplication.delayCall += AutoSetup;
            return;
        }
        if (!EditorApplication.isPlayingOrWillChangePlaymode && File.Exists(Request)
            && !File.Exists(ReportPath)) Setup();
    }

    static void EnsureFolder(string path)
    {
        if (AssetDatabase.IsValidFolder(path)) return;
        int slash = path.LastIndexOf('/');
        EnsureFolder(path.Substring(0, slash));
        AssetDatabase.CreateFolder(path.Substring(0, slash), path.Substring(slash + 1));
    }

    static Material SpriteMaterial()
    {
        return AssetDatabase.LoadAssetAtPath<Material>(
            "Packages/com.unity.render-pipelines.universal/Runtime/Materials/Sprite-Unlit-Default.mat");
    }

    static List<string> TexturePaths(int group)
    {
        if (group == 0)
            return Enumerable.Range(1, 3).Select(i =>
                Root + "/tilesets/tileset-platforms-" + i + "-16x16.png").ToList();
        if (group == 1)
            return Enumerable.Range(1, 2).Select(i =>
                Root + "/tilesets/tileset-bg-" + i + "-16x16.png").ToList();
        if (group == 2)
            return Directory.GetFiles(Root + "/tilesets", "*.png")
                .Select(p => p.Replace('\\', '/'))
                .Where(p => !Path.GetFileName(p).StartsWith("tileset-bg-")
                    && !Path.GetFileName(p).Contains("platforms-")
                    && !Path.GetFileName(p).StartsWith("animated tiles-water-"))
                .OrderBy(p => p).ToList();
        return new[] { "props", "items", "resources" }
            .SelectMany(folder => Directory.GetFiles(Root + "/" + folder, "*.png"))
            .Select(p => p.Replace('\\', '/')).OrderBy(p => p).ToList();
    }

    static GameObject MakePalette(int group, out int count)
    {
        string name = PaletteNames[group];
        string palettePath = Generated + "/Palettes/" + name + ".prefab";
        string libraryPath = Generated + "/Tiles/" + name + ".asset";
        if (AssetDatabase.LoadAssetAtPath<Tile>(libraryPath) == null)
        {
            Tile library = ScriptableObject.CreateInstance<Tile>();
            library.name = name + "_LibraryRoot";
            library.colliderType = Tile.ColliderType.None;
            AssetDatabase.CreateAsset(library, libraryPath);
        }
        var libraryTiles = AssetDatabase.LoadAllAssetsAtPath(libraryPath).OfType<Tile>()
            .Where(t => t.sprite != null).ToDictionary(t => t.name);
        GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(palettePath);
        if (prefab == null)
            prefab = GridPaletteUtility.CreateNewPalette(Generated + "/Palettes", name,
                GridLayout.CellLayout.Rectangle, GridPalette.CellSizing.Manual,
                Vector3.one, GridLayout.CellSwizzle.XYZ);
        if (prefab == null) throw new InvalidOperationException("Could not create " + name);

        GameObject content = PrefabUtility.LoadPrefabContents(palettePath);
        count = 0;
        try
        {
            Tilemap map = content.GetComponentInChildren<Tilemap>();
            map.ClearAllTiles();
            Material material = SpriteMaterial();
            if (material != null) map.GetComponent<TilemapRenderer>().sharedMaterial = material;
            var positions = new List<Vector3Int>();
            var tiles = new List<TileBase>();
            int offsetX = 0;
            int offsetY = 0;
            int shelfHeight = 0;
            AssetDatabase.StartAssetEditing();
            try
            {
                foreach (string path in TexturePaths(group))
                {
                    Texture2D texture = AssetDatabase.LoadAssetAtPath<Texture2D>(path);
                    Sprite[] sprites = AssetDatabase.LoadAllAssetsAtPath(path).OfType<Sprite>()
                        .OrderBy(s => s.name).ToArray();
                    if (texture == null || sprites.Length == 0)
                        throw new InvalidOperationException("Missing imported sprites: " + path);
                    int width = Mathf.CeilToInt(texture.width / 16f);
                    int height = Mathf.CeilToInt(texture.height / 16f);
                    if (offsetX != 0 && offsetX + width > 42)
                    {
                        offsetX = 0;
                        offsetY -= shelfHeight + 3;
                        shelfHeight = 0;
                    }
                    foreach (Sprite sprite in sprites)
                    {
                        if (!libraryTiles.TryGetValue(sprite.name, out Tile tile))
                        {
                            tile = ScriptableObject.CreateInstance<Tile>();
                            tile.name = sprite.name;
                            AssetDatabase.AddObjectToAsset(tile, libraryPath);
                            libraryTiles.Add(sprite.name, tile);
                        }
                        tile.sprite = sprite;
                        tile.color = Color.white;
                        tile.transform = Matrix4x4.identity;
                        tile.flags = TileFlags.LockAll;
                        tile.colliderType = group == 0 ? Tile.ColliderType.Grid : Tile.ColliderType.None;
                        EditorUtility.SetDirty(tile);
                        int x = Mathf.RoundToInt(sprite.rect.x / 16f);
                        int y = Mathf.RoundToInt((texture.height - sprite.rect.yMax) / 16f);
                        positions.Add(new Vector3Int(offsetX + x, offsetY - y, 0));
                        tiles.Add(tile);
                        count++;
                    }
                    offsetX += width + 3;
                    shelfHeight = Mathf.Max(shelfHeight, height);
                }
            }
            finally { AssetDatabase.StopAssetEditing(); }
            map.SetTiles(positions.ToArray(), tiles.ToArray());
            map.CompressBounds();
            EditorUtility.SetDirty(map);
            PrefabUtility.SaveAsPrefabAsset(content, palettePath);
        }
        finally { PrefabUtility.UnloadPrefabContents(content); }
        AssetDatabase.SaveAssets();
        return AssetDatabase.LoadAssetAtPath<GameObject>(palettePath);
    }

    static GameObject MakeGridPrefab()
    {
        string path = Generated + "/Prefabs/Outpost_MapGrid.prefab";
        GameObject existing = AssetDatabase.LoadAssetAtPath<GameObject>(path);
        if (existing != null) return existing;
        GameObject root = new GameObject(MapRootName, typeof(Grid));
        try
        {
            root.GetComponent<Grid>().cellSize = Vector3.one;
            AddLayer(root, "01_Terrain", 0, true);
            AddLayer(root, "02_Background", -20, false);
            AddLayer(root, "03_Decoration", 10, false);
            return PrefabUtility.SaveAsPrefabAsset(root, path);
        }
        finally { Object.DestroyImmediate(root); }
    }

    static void AddLayer(GameObject root, string name, int order, bool collision)
    {
        GameObject layer = new GameObject(name, typeof(Tilemap), typeof(TilemapRenderer));
        layer.transform.SetParent(root.transform, false);
        TilemapRenderer renderer = layer.GetComponent<TilemapRenderer>();
        renderer.sortingOrder = order;
        Material material = SpriteMaterial();
        if (material != null) renderer.sharedMaterial = material;
        if (collision)
        {
            int ground = LayerMask.NameToLayer("Ground");
            layer.layer = ground >= 0 ? ground : 0;
            layer.AddComponent<TilemapCollider2D>();
        }
    }

    static GameObject FindSceneGrid()
    {
        return SceneManager.GetActiveScene().GetRootGameObjects()
            .FirstOrDefault(go => go.name == MapRootName);
    }

    [MenuItem("Tools/Abandoned Outpost/Setup painting assets")]
    public static void Setup()
    {
        if (running || EditorApplication.isPlayingOrWillChangePlaymode) return;
        running = true;
        try
        {
            foreach (string folder in new[] { "Palettes", "Tiles", "Prefabs", "Maps" })
                EnsureFolder(Generated + "/" + folder);
            int[] counts = new int[4];
            for (int i = 0; i < PaletteNames.Length; i++)
            {
                EditorUtility.DisplayProgressBar("Abandoned Outpost", "Preparing " + PaletteNames[i], i / 4f);
                MakePalette(i, out counts[i]);
            }
            GameObject gridPrefab = MakeGridPrefab();
            Scene scene = SceneManager.GetActiveScene();
            if (!scene.IsValid() || !scene.isLoaded)
                throw new InvalidOperationException("Open the map scene before running setup.");
            GameObject sceneGrid = FindSceneGrid();
            if (sceneGrid == null)
            {
                sceneGrid = (GameObject)PrefabUtility.InstantiatePrefab(gridPrefab, scene);
                Undo.RegisterCreatedObjectUndo(sceneGrid, "Create Outpost painting layers");
            }
            if (PrefabUtility.IsPartOfPrefabInstance(sceneGrid))
                PrefabUtility.UnpackPrefabInstance(sceneGrid, PrefabUnpackMode.Completely, InteractionMode.AutomatedAction);
            string originalName = string.IsNullOrEmpty(scene.name) ? "Map" : scene.name;
            string scenePath = scene.path;
            if (!scenePath.StartsWith(Generated + "/Maps/", StringComparison.Ordinal))
                scenePath = AssetDatabase.GenerateUniqueAssetPath(Generated + "/Maps/" + originalName + "_PaintReady.unity");
            if (!EditorSceneManager.SaveScene(scene, scenePath))
                throw new IOException("Unable to save the ready-to-paint scene.");

            var report = new Report {
                scene = scenePath,
                gridPrefab = AssetDatabase.GetAssetPath(gridPrefab),
                palettes = PaletteNames,
                tileCounts = counts,
                paintTarget = "01_Terrain",
                groundLayer = sceneGrid.transform.Find("01_Terrain").gameObject.layer,
                terrainHasCollider = sceneGrid.transform.Find("01_Terrain").GetComponent<TilemapCollider2D>() != null
            };
            File.WriteAllText(ReportPath, JsonUtility.ToJson(report, true));
            AssetDatabase.ImportAsset(ReportPath);
            AssetDatabase.SaveAssets();
            EditorApplication.delayCall += () => SelectGroup(0, true);
            Debug.Log("[Abandoned Outpost] Ready to paint: " + counts.Sum() + " tiles, 4 palettes, 3 scene layers. " + scenePath);
        }
        catch (Exception exception)
        {
            Debug.LogException(exception);
        }
        finally
        {
            EditorUtility.ClearProgressBar();
            running = false;
        }
    }

    [MenuItem("Tools/Abandoned Outpost/Paint Terrain (solid)")]
    static void Terrain() { SelectGroup(0, true); }
    [MenuItem("Tools/Abandoned Outpost/Paint Background")]
    static void Background() { SelectGroup(1); }
    [MenuItem("Tools/Abandoned Outpost/Paint Structures")]
    static void Structures() { SelectGroup(2); }
    [MenuItem("Tools/Abandoned Outpost/Paint Props")]
    static void Props() { SelectGroup(3); }

    static void SelectGroup(int group, bool frame = false)
    {
        GameObject sceneGrid = FindSceneGrid();
        if (sceneGrid == null) throw new InvalidOperationException("Open the saved PaintReady scene first.");
        if (PrefabUtility.IsPartOfPrefabInstance(sceneGrid))
        {
            PrefabUtility.UnpackPrefabInstance(sceneGrid, PrefabUnpackMode.Completely, InteractionMode.AutomatedAction);
            Scene scene = SceneManager.GetActiveScene();
            if (scene.path.StartsWith(Generated + "/Maps/", StringComparison.Ordinal))
                EditorSceneManager.SaveScene(scene);
        }
        string layerName = group == 0 ? "01_Terrain" : group == 1 ? "02_Background" : "03_Decoration";
        GameObject target = sceneGrid.transform.Find(layerName).gameObject;
        GameObject palette = AssetDatabase.LoadAssetAtPath<GameObject>(
            Generated + "/Palettes/" + PaletteNames[group] + ".prefab");
        if (palette == null) throw new InvalidOperationException("Palette is missing. Run Setup painting assets.");
        GridPaintPaletteWindow window = EditorWindow.GetWindow<GridPaintPaletteWindow>();
        window.minSize = new Vector2(400, 400);
        window.Show();
        // Force native palette discovery after creation before setting the active palette.
        if (!GridPaintingState.palettes.Contains(palette))
            throw new InvalidOperationException("Unity has not indexed the palette yet. Try the Paint menu again.");
        GridPaintingState.palette = palette;
        Selection.activeGameObject = target;
        GridPaintingState.scenePaintTarget = target;
        GridBrush brush = GridPaintingState.gridBrush as GridBrush;
        TileBase first = palette.GetComponentInChildren<Tilemap>().GetTilesBlock(
            palette.GetComponentInChildren<Tilemap>().cellBounds).FirstOrDefault(t => t != null);
        if (brush != null && first != null)
        {
            brush.Init(Vector3Int.one, Vector3Int.zero);
            brush.SetTile(Vector3Int.zero, first);
        }
        ToolManager.SetActiveTool<PaintTool>();
        SceneView view = SceneView.lastActiveSceneView;
        if (view != null)
        {
            view.in2DMode = true;
            view.sceneLighting = false;
            if (frame) view.LookAt(Vector3.zero, Quaternion.identity, 10f, true, true);
            view.Focus();
        }
        window.Focus();
        SceneView.RepaintAll();
        FramePalette(window);
        Debug.Log("[Abandoned Outpost] Painting ready: " + PaletteNames[group] + " -> " + layerName);
    }
}
