using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEditor.U2D.Sprites;
using UnityEngine;
using UnityEngine.SceneManagement;

[InitializeOnLoad]
public static class ShotPressureSwitchSetup
{
    public const string Root = "Assets/Art/Traps/ShotPressureSwitch";
    public const string PrefabPath = "Assets/Prefabs/Level/ShotPressureSwitch.prefab";
    public const string CeilingPrefabPath = "Assets/Prefabs/Level/ShotPressureSwitch_Ceiling.prefab";
    public const string EditorRoot = "Assets/Scripts/Editor/ShotPressureSwitch";
    public const string MaterialPath = "Assets/Materials/Level/ShotPressureSwitch.mat";
    public static string OutputRoot => Path.GetFullPath(Path.Combine(Application.dataPath, "../Logs/ShotPressureSwitch"));
    public static string BackupRoot => Path.Combine(OutputRoot, "Backups");
    const string Request = EditorRoot + "/Setup.request.txt";

    public static void BackupAsset(string path, string name)
    {
        Directory.CreateDirectory(BackupRoot);
        string destination = Path.Combine(BackupRoot, name);
        File.Copy(path, destination, false);
        if (File.Exists(path + ".meta")) File.Copy(path + ".meta", destination + ".meta", false);
    }

    public static void ConfigureHeadCollider(ShotPressureSwitch item)
    {
        if (item.capRenderer == null || item.capRenderer.sprite == null)
            throw new InvalidOperationException("A visible pressure head is required for the hitbox.");
        BoxCollider2D box = item.GetComponent<BoxCollider2D>();
        if (box == null) box = item.gameObject.AddComponent<BoxCollider2D>();
        foreach (Collider2D old in item.GetComponentsInChildren<Collider2D>(true))
            if (old != box) UnityEngine.Object.DestroyImmediate(old);
        box.isTrigger = false;
        box.edgeRadius = 0;
        item.hitbox = box;
        item.FitHitboxToHead();
    }

    static ShotPressureSwitchSetup()
    {
        EditorApplication.delayCall += InstallPending;
        EditorApplication.playModeStateChanged += state =>
        {
            if (state == PlayModeStateChange.EnteredEditMode) EditorApplication.delayCall += InstallPending;
        };
    }

    static void InstallPending()
    {
        if (!File.Exists(Request)) return;
        if (EditorApplication.isCompiling || EditorApplication.isUpdating)
        { EditorApplication.delayCall += InstallPending; return; }
        if (EditorApplication.isPlayingOrWillChangePlaymode) return;
        try
        {
            Configure();
            File.Delete(Request);
            ShotPressureSwitchCheck.StartCheck();
        }
        catch (Exception error)
        {
            Directory.CreateDirectory(OutputRoot);
            File.WriteAllText(OutputRoot + "/SetupError.txt", error.ToString());
            Debug.LogException(error);
        }
    }

    static void Folder(string path)
    {
        if (AssetDatabase.IsValidFolder(path)) return;
        int split = path.LastIndexOf('/');
        Folder(path.Substring(0, split));
        AssetDatabase.CreateFolder(path.Substring(0, split), path.Substring(split + 1));
    }

    static void Import(string name, bool parts)
    {
        string path = Root + "/" + name + ".png";
        AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceSynchronousImport);
        TextureImporter importer = (TextureImporter)AssetImporter.GetAtPath(path);
        importer.textureType = TextureImporterType.Sprite;
        importer.spriteImportMode = parts ? SpriteImportMode.Multiple : SpriteImportMode.Single;
        importer.spritePixelsPerUnit = 16;
        importer.filterMode = FilterMode.Point;
        importer.mipmapEnabled = false;
        importer.textureCompression = TextureImporterCompression.Uncompressed;
        importer.alphaIsTransparency = true;
        importer.npotScale = TextureImporterNPOTScale.None;
        importer.spritePivot = new Vector2(0.5f, 0);
        TextureImporterSettings settings = new TextureImporterSettings();
        importer.ReadTextureSettings(settings);
        settings.spriteAlignment = (int)SpriteAlignment.Custom;
        settings.spriteMeshType = SpriteMeshType.FullRect;
        importer.SetTextureSettings(settings);
        if (parts)
        {
            SpriteDataProviderFactories factory = new SpriteDataProviderFactories();
            factory.Init();
            ISpriteEditorDataProvider provider = factory.GetSpriteEditorDataProviderFromObject(importer);
            provider.InitSpriteEditorDataProvider();
            SpriteRect Make(string part, Rect rect) => new SpriteRect
            {
                name = part, rect = rect, spriteID = GUID.Generate(),
                alignment = SpriteAlignment.Custom, pivot = new Vector2(0.5f, 0)
            };
            SpriteRect[] rects =
            {
                Make("Piston_Base", new Rect(0, 0, 26, 18)),
                Make("Piston_Shaft", new Rect(0, 18, 26, 4)),
                Make("Piston_Cap", new Rect(0, 22, 26, 23))
            };
            provider.SetSpriteRects(rects);
            provider.GetDataProvider<ISpriteNameFileIdDataProvider>().SetNameFileIdPairs(
                rects.Select(r => new SpriteNameFileIdPair(r.name, r.spriteID)));
            provider.Apply();
        }
        importer.SaveAndReimport();
    }

    static Sprite Sprite(string file, string name = null)
    {
        return AssetDatabase.LoadAllAssetsAtPath(Root + "/" + file + ".png")
            .OfType<Sprite>().Single(s => name == null || s.name == name);
    }

    static SpriteRenderer Part(GameObject parent, string name, Sprite sprite, float y, int order, Material material)
    {
        GameObject part = new GameObject(name);
        part.transform.SetParent(parent.transform, false);
        part.transform.localPosition = new Vector3(0, y, 0);
        SpriteRenderer renderer = part.AddComponent<SpriteRenderer>();
        renderer.sprite = sprite;
        renderer.sharedMaterial = material;
        renderer.sortingOrder = order;
        return renderer;
    }

    [MenuItem("地图/射击机关/创建或选中测试机关")]
    public static void Configure()
    {
        Scene scene = SceneManager.GetActiveScene();
        if (EditorApplication.isPlayingOrWillChangePlaymode || scene.path != "Assets/Scenes/Tests/TrapTesting.unity")
            throw new InvalidOperationException("Open the confirmed TrapTesting scene in edit mode.");
        Folder("Assets/Prefabs/Level");
        Directory.CreateDirectory(OutputRoot);
        Directory.CreateDirectory(BackupRoot);
        GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(PrefabPath);
        if (prefab == null)
        {
            Import("button_machanics_0", true);
            Import("button_machanics_1", false);
            Import("button_machanics_unlock_1", false);
            Material material = AssetDatabase.LoadAssetAtPath<Material>(
                "Packages/com.unity.render-pipelines.universal/Runtime/Materials/Sprite-Unlit-Default.mat");
            if (material == null) throw new InvalidOperationException("Sprite unlit material is unavailable.");
            GameObject root = new GameObject("ShotPressureSwitch");
            ShotPressureSwitch item = root.AddComponent<ShotPressureSwitch>();
            item.baseRenderer = Part(root, "Fixed Base", Sprite("button_machanics_0", "Piston_Base"), 0, 4, material);
            item.shaftRenderer = Part(root, "Sliding Shaft", Sprite("button_machanics_0", "Piston_Shaft"), 18f / 16, 5, material);
            item.capRenderer = Part(root, "Moving Red Cap", Sprite("button_machanics_0", "Piston_Cap"), 22f / 16, 6, material);
            item.pressedRedSprite = Sprite("button_machanics_1");
            item.unlockedGreenSprite = Sprite("button_machanics_unlock_1");
            item.completedRenderer = Part(root, "Pressed Green Display", item.pressedRedSprite, 0, 6, material);
            item.completedRenderer.enabled = false;
            ConfigureHeadCollider(item);
            item.feedback = root.AddComponent<ShotPressureSwitchFeedback>();
            item.feedback.sparkMaterial = AssetDatabase.LoadAssetAtPath<Material>("Assets/Materials/Projectiles/ProjectileSparks.mat");
            prefab = PrefabUtility.SaveAsPrefabAsset(root, PrefabPath);
            UnityEngine.Object.DestroyImmediate(root);
        }
        ShotPressureSwitch existing = scene.GetRootGameObjects()
            .SelectMany(go => go.GetComponentsInChildren<ShotPressureSwitch>(true)).FirstOrDefault();
        if (existing == null)
        {
            if (!EditorSceneManager.SaveScene(scene)) throw new IOException("Scene backup save failed.");
            BackupAsset(scene.path, "TrapTesting_BeforePressureSwitch_" + DateTime.Now.ToString("yyyyMMdd_HHmmss") + ".unity");
            GameObject instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab, scene);
            Undo.RegisterCreatedObjectUndo(instance, "Place shooting pressure switch");
            instance.name = "射击压柱机关_9发激活";
            Collider2D ground = scene.GetRootGameObjects().FirstOrDefault(go => go.name == "Ground")?.GetComponent<Collider2D>();
            instance.transform.position = new Vector3(2.5f, ground != null ? ground.bounds.max.y : -0.5f, 0);
            existing = instance.GetComponent<ShotPressureSwitch>();
            EditorSceneManager.MarkSceneDirty(scene);
            if (!EditorSceneManager.SaveScene(scene)) throw new IOException("Scene save failed.");
        }
        Selection.activeGameObject = existing.gameObject;
        AssetDatabase.SaveAssets();
        File.WriteAllText(OutputRoot + "/SetupReport.json", "{\"scene\":\"" + scene.path
            + "\",\"prefab\":\"" + PrefabPath + "\",\"hitsRequired\":9,\"returnSeconds\":1,\"ready\":true}");
        Debug.Log("[PressureSwitch] Prefab and TrapTesting example are ready.");
    }
}
