using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.U2D.Sprites;
using UnityEngine;
using UnityEngine.SceneManagement;

[InitializeOnLoad]
public static class ShotPressureSwitchSideViewSetup
{
    const string Root = ShotPressureSwitchSetup.Root;
    const string Request = ShotPressureSwitchSetup.EditorRoot + "/SideView.request.txt";
    const string ImagePath = Root + "/pressure-switch-side-view-detailed.png";

    static ShotPressureSwitchSideViewSetup()
    {
        EditorApplication.delayCall += ApplyPending;
        EditorApplication.playModeStateChanged += state =>
        {
            if (state == PlayModeStateChange.EnteredEditMode) EditorApplication.delayCall += ApplyPending;
        };
    }

    static void ApplyPending()
    {
        if (!File.Exists(Request)) return;
        if (EditorApplication.isCompiling || EditorApplication.isUpdating)
        { EditorApplication.delayCall += ApplyPending; return; }
        if (EditorApplication.isPlayingOrWillChangePlaymode) return;
        try
        {
            Apply();
            File.Delete(Request);
            int count = SceneManager.GetActiveScene().GetRootGameObjects()
                .Sum(go => go.GetComponentsInChildren<ShotPressureSwitch>(true).Length);
            if (count == 1) ShotPressureSwitchCheck.StartCheck();
        }
        catch (Exception error)
        {
            Directory.CreateDirectory(ShotPressureSwitchSetup.OutputRoot);
            File.WriteAllText(ShotPressureSwitchSetup.OutputRoot + "/SideViewError.txt", error.ToString());
            Debug.LogException(error);
        }
    }

    static SpriteRect Rect(string name, float x, float y, float w, float h) => new SpriteRect
    {
        name = name, rect = new Rect(x, y, w, h), spriteID = GUID.Generate(),
        alignment = SpriteAlignment.Custom, pivot = new Vector2(0.5f, 0)
    };

    static Sprite[] ImportParts()
    {
        AssetDatabase.ImportAsset(ImagePath, ImportAssetOptions.ForceSynchronousImport);
        TextureImporter importer = (TextureImporter)AssetImporter.GetAtPath(ImagePath);
        importer.textureType = TextureImporterType.Sprite;
        importer.spriteImportMode = SpriteImportMode.Multiple;
        importer.spritePixelsPerUnit = 32;
        importer.filterMode = FilterMode.Point;
        importer.mipmapEnabled = false;
        importer.textureCompression = TextureImporterCompression.Uncompressed;
        importer.alphaIsTransparency = true;
        importer.maxTextureSize = 128;
        importer.npotScale = TextureImporterNPOTScale.None;
        TextureImporterSettings settings = new TextureImporterSettings();
        importer.ReadTextureSettings(settings);
        settings.spriteMeshType = SpriteMeshType.FullRect;
        importer.SetTextureSettings(settings);
        SpriteDataProviderFactories factory = new SpriteDataProviderFactories();
        factory.Init();
        ISpriteEditorDataProvider provider = factory.GetSpriteEditorDataProviderFromObject(importer);
        provider.InitSpriteEditorDataProvider();
        SpriteRect[] rects =
        {
            Rect("SideView_Head", 152, 1088, 720, 300),
            Rect("SideView_Shaft", 416, 336, 192, 752),
            Rect("SideView_Base", 152, 108, 720, 232)
        };
        provider.SetSpriteRects(rects);
        provider.GetDataProvider<ISpriteNameFileIdDataProvider>().SetNameFileIdPairs(
            rects.Select(r => new SpriteNameFileIdPair(r.name, r.spriteID)));
        provider.Apply();
        importer.SaveAndReimport();
        return AssetDatabase.LoadAllAssetsAtPath(ImagePath).OfType<Sprite>().ToArray();
    }

    static void SetPart(SpriteRenderer renderer, Sprite sprite, float width, float height, float y, Material material)
    {
        renderer.sprite = sprite;
        renderer.sharedMaterial = material;
        renderer.transform.localPosition = new Vector3(0, y, 0);
        renderer.transform.localScale = new Vector3(width / sprite.bounds.size.x, height / sprite.bounds.size.y, 1);
        renderer.enabled = true;
    }

    [MenuItem("地图/射击机关/应用精细侧视 2D 外观")]
    public static void Apply()
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode) return;
        Directory.CreateDirectory(ShotPressureSwitchSetup.OutputRoot);
        ShotPressureSwitchSetup.BackupAsset(ShotPressureSwitchSetup.PrefabPath,
            "ShotPressureSwitch_BeforeSideView_" + DateTime.Now.ToString("yyyyMMdd_HHmmss") + ".prefab");
        Sprite[] parts = ImportParts();
        Shader shader = Shader.Find("ProjectesII/PressureSwitchFlat2D");
        if (shader == null) throw new InvalidOperationException("Side-view sprite shader did not import.");
        string materialPath = ShotPressureSwitchSetup.MaterialPath;
        Material material = AssetDatabase.LoadAssetAtPath<Material>(materialPath);
        if (material == null)
        {
            material = new Material(shader);
            AssetDatabase.CreateAsset(material, materialPath);
        }
        GameObject contents = PrefabUtility.LoadPrefabContents(ShotPressureSwitchSetup.PrefabPath);
        try
        {
            ShotPressureSwitch item = contents.GetComponent<ShotPressureSwitch>();
            SetPart(item.baseRenderer, parts.Single(p => p.name == "SideView_Base"), 1.625f, 0.5f, 0, material);
            SetPart(item.shaftRenderer, parts.Single(p => p.name == "SideView_Shaft"), 0.4375f, 1.6875f, 0.5f, material);
            SetPart(item.capRenderer, parts.Single(p => p.name == "SideView_Head"), 1.625f, 0.625f, 2.1875f, material);
            item.flat2DParts = true;
            item.shaftScaleAtRest = item.shaftRenderer.transform.localScale.y;
            item.capRestY = 2.1875f;
            item.capTravel = 1.5625f;
            item.shaftRestHeight = 1.6875f;
            item.fullHeight = 2.8125f;
            item.completedRenderer.enabled = false;
            ShotPressureSwitchSetup.ConfigureHeadCollider(item);
            PrefabUtility.SaveAsPrefabAsset(contents, ShotPressureSwitchSetup.PrefabPath);
        }
        finally { PrefabUtility.UnloadPrefabContents(contents); }
        AssetDatabase.SaveAssets();
        Texture2D texture = AssetDatabase.LoadAssetAtPath<Texture2D>(ImagePath);
        File.WriteAllText(ShotPressureSwitchSetup.OutputRoot + "/SideViewSetupReport.json", "{\"ready\":true,\"flat2D\":true,\"textureWidth\":"
            + texture.width + ",\"textureHeight\":" + texture.height + ",\"travel\":1.5625,\"hitsRequired\":9}");
        Debug.Log("[PressureSwitch] Detailed flat 2D visuals installed; red/green states share the same head geometry.");
    }
}
