using System;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

[InitializeOnLoad]
public static class ShotPressureSwitchHeadSetup
{
    const string Request = ShotPressureSwitchSetup.EditorRoot + "/HeadCollider.request.txt";

    static ShotPressureSwitchHeadSetup()
    {
        EditorApplication.delayCall += Pending;
        EditorApplication.playModeStateChanged += state =>
        {
            if (state == PlayModeStateChange.EnteredEditMode) EditorApplication.delayCall += Pending;
        };
    }

    static void Pending()
    {
        if (!File.Exists(Request)) return;
        if (EditorApplication.isCompiling || EditorApplication.isUpdating)
        { EditorApplication.delayCall += Pending; return; }
        if (EditorApplication.isPlayingOrWillChangePlaymode) return;
        try
        {
            Apply();
            File.Delete(Request);
            ShotPressureSwitchCheck.StartCheck();
        }
        catch (Exception error)
        {
            Directory.CreateDirectory(ShotPressureSwitchSetup.OutputRoot);
            File.WriteAllText(Path.Combine(ShotPressureSwitchSetup.OutputRoot, "HeadColliderError.txt"), error.ToString());
            Debug.LogException(error);
        }
    }

    [MenuItem("地图/射击机关/压头碰撞与天花板版本")]
    public static void Apply()
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode) return;
        string stamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
        ShotPressureSwitchSetup.BackupAsset(ShotPressureSwitchSetup.PrefabPath,
            "ShotPressureSwitch_BeforeHeadCollider_" + stamp + ".prefab");
        GameObject editable = PrefabUtility.LoadPrefabContents(ShotPressureSwitchSetup.PrefabPath);
        Vector2 size, offset;
        try
        {
            ShotPressureSwitch item = editable.GetComponent<ShotPressureSwitch>();
            ShotPressureSwitchSetup.ConfigureHeadCollider(item);
            size = item.hitbox.size;
            offset = item.hitbox.offset;
            if (PrefabUtility.SaveAsPrefabAsset(editable, ShotPressureSwitchSetup.PrefabPath) == null)
                throw new IOException("Could not save the head-only hitbox.");
        }
        finally { PrefabUtility.UnloadPrefabContents(editable); }

        if (AssetDatabase.LoadAssetAtPath<GameObject>(ShotPressureSwitchSetup.CeilingPrefabPath) == null)
        {
            Scene preview = EditorSceneManager.NewPreviewScene();
            try
            {
                GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(ShotPressureSwitchSetup.PrefabPath);
                GameObject hanging = (GameObject)PrefabUtility.InstantiatePrefab(prefab, preview);
                hanging.name = "ShotPressureSwitch_Ceiling";
                hanging.transform.localRotation = Quaternion.Euler(0, 0, 180);
                if (PrefabUtility.SaveAsPrefabAsset(hanging, ShotPressureSwitchSetup.CeilingPrefabPath) == null)
                    throw new IOException("Could not save the ceiling variant.");
            }
            finally { EditorSceneManager.ClosePreviewScene(preview); }
        }
        AssetDatabase.SaveAssets();
        File.WriteAllText(Path.Combine(ShotPressureSwitchSetup.OutputRoot, "HeadColliderSetupReport.json"),
            "{\"headOnly\":true,\"width\":" + size.x.ToString(System.Globalization.CultureInfo.InvariantCulture)
            + ",\"height\":" + size.y.ToString(System.Globalization.CultureInfo.InvariantCulture)
            + ",\"offsetY\":" + offset.y.ToString(System.Globalization.CultureInfo.InvariantCulture)
            + ",\"ceilingPrefab\":\"" + ShotPressureSwitchSetup.CeilingPrefabPath + "\"}");
        Debug.Log("[PressureSwitch] Head-only collider configured; ceiling variant rotates 180 degrees around the base mount.");
    }
}
