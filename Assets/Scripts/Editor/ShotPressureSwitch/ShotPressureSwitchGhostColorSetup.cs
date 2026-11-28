using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

[InitializeOnLoad]
public static class ShotPressureSwitchGhostColorSetup
{
    const string Work="C:/Users/chenk/Documents/Codex/2026-11-23/ban-2";
    const string Out=Work+"/outputs/pressure-switch-blue";
    const string Prefab="Assets/Prefabs/Level/ShotPressureSwitch_GhostOnly_Ceiling.prefab";
    const string Normal="Assets/Prefabs/Level/ShotPressureSwitch_Ceiling.prefab";
    const string OriginalMaterial="Assets/Materials/Level/ShotPressureSwitch.mat";
    const string MaterialPath="Assets/Materials/Level/ShotPressureSwitch_GhostOnly_Blue.mat";
    static double next;
    [Serializable] class Report {public string prefab,material,backup;public bool saved,shaderCompiled,ghostFilterPreserved,ceilingPreserved,normalMaterialPreserved,normalPrefabPreserved,sceneUntouched;public Color idleLamp,metalTint;}
    static ShotPressureSwitchGhostColorSetup(){EditorApplication.update+=Tick;}
    static string Hash(string path){using(var s=SHA256.Create())return BitConverter.ToString(s.ComputeHash(File.ReadAllBytes(path)));}
    static void Folder(string path){if(AssetDatabase.IsValidFolder(path))return;int i=path.LastIndexOf('/');Folder(path.Substring(0,i));AssetDatabase.CreateFolder(path.Substring(0,i),path.Substring(i+1));}
    static void Tick()
    {
        if(EditorApplication.isPlayingOrWillChangePlaymode||EditorApplication.isCompiling||EditorApplication.isUpdating||EditorApplication.timeSinceStartup<next)return;next=EditorApplication.timeSinceStartup+1;
        if(!File.Exists(Out+"/apply.request.txt"))return;File.Delete(Out+"/apply.request.txt");
        try{Apply();}catch(Exception e){File.WriteAllText(Out+"/error.txt",e.ToString());Debug.LogException(e);}
    }
    [MenuItem("地图/射击机关/E 分身专用版改为蓝色")]
    static void Apply()
    {
        Directory.CreateDirectory(Out);Directory.CreateDirectory("Logs/ShotPressureSwitch/Backups");
        var scene=UnityEngine.SceneManagement.SceneManager.GetActiveScene();bool dirty=scene.isDirty;string originalMaterial=Hash(OriginalMaterial),ordinary=Hash(Normal);
        string backup="Logs/ShotPressureSwitch/Backups/ShotPressureSwitch_GhostOnly_BeforeBlue_"+DateTime.Now.ToString("yyyyMMdd_HHmmss")+".prefab";File.Copy(Prefab,backup,false);File.Copy(Prefab+".meta",backup+".meta",false);
        var shader=Shader.Find("ProjectesII/PressureSwitchGhostFlat2D");if(!shader||ShaderUtil.ShaderHasError(shader))throw new Exception("Blue switch shader failed to compile.");
        Folder("Assets/Materials/Level");var mat=AssetDatabase.LoadAssetAtPath<Material>(MaterialPath);
        if(!mat){mat=new Material(AssetDatabase.LoadAssetAtPath<Material>(OriginalMaterial)){name="ShotPressureSwitch_GhostOnly_Blue"};mat.shader=shader;AssetDatabase.CreateAsset(mat,MaterialPath);}
        mat.SetColor("_IdleLampColor",new Color(.05f,.85f,1,1));mat.SetColor("_MetalTint",new Color(.12f,1.35f,1.55f,1));mat.SetFloat("_Activated",0);EditorUtility.SetDirty(mat);AssetDatabase.SaveAssetIfDirty(mat);
        var editable=PrefabUtility.LoadPrefabContents(Prefab);
        try
        {
            foreach(var sprite in editable.GetComponentsInChildren<SpriteRenderer>(true))sprite.sharedMaterial=mat;
            if(!PrefabUtility.SaveAsPrefabAsset(editable,Prefab))throw new IOException("Could not save blue variant material overrides.");
        }
        finally{PrefabUtility.UnloadPrefabContents(editable);}
        var asset=AssetDatabase.LoadAssetAtPath<GameObject>(Prefab);var item=asset.GetComponent<ShotPressureSwitch>();
        var report=new Report{prefab=Prefab,material=MaterialPath,backup=backup,saved=true,shaderCompiled=!ShaderUtil.ShaderHasError(shader),ghostFilterPreserved=item.ghostOnly&&item.acceptGhostBullets,ceilingPreserved=Vector3.Dot(asset.transform.up,Vector3.down)>.99f,normalMaterialPreserved=originalMaterial==Hash(OriginalMaterial),normalPrefabPreserved=ordinary==Hash(Normal),idleLamp=mat.GetColor("_IdleLampColor"),metalTint=mat.GetColor("_MetalTint")};
        RenderComparison();report.sceneUntouched=UnityEngine.SceneManagement.SceneManager.GetActiveScene()==scene&&scene.isDirty==dirty;
        if(!report.shaderCompiled||!report.ghostFilterPreserved||!report.ceilingPreserved||!report.normalMaterialPreserved||!report.normalPrefabPreserved||!report.sceneUntouched)throw new Exception("Color setup validation failed: "+JsonUtility.ToJson(report));
        string json=JsonUtility.ToJson(report,true);File.WriteAllText(Out+"/report.json",json);File.WriteAllText("Logs/ShotPressureSwitch/GhostBlueReport.json",json);Selection.activeObject=asset;EditorGUIUtility.PingObject(asset);SceneView.RepaintAll();EditorApplication.QueuePlayerLoopUpdate();
    }
    static void RenderComparison()
    {
        var staging=EditorSceneManager.NewPreviewScene();var preview=new PreviewRenderUtility();
        try
        {
            var objects=new GameObject[3];
            for(int i=0;i<3;i++)
            {
                objects[i]=(GameObject)PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath<GameObject>(i==0?Normal:Prefab),staging);objects[i].transform.position=new Vector3((i-1)*2.25f,0,0);var item=objects[i].GetComponent<ShotPressureSwitch>();item.enabled=false;
                if(i==2)
                {
                    Vector3 p=item.capRenderer.transform.localPosition;p.y=item.capRestY-item.capTravel;item.capRenderer.transform.localPosition=p;float remaining=Mathf.Max(0,item.shaftRestHeight-item.capTravel);Vector3 shaftScale=item.shaftRenderer.transform.localScale;shaftScale.y=item.shaftScaleAtRest*remaining/Mathf.Max(.00001f,item.shaftRestHeight);item.shaftRenderer.transform.localScale=shaftScale;item.shaftRenderer.enabled=remaining>.00001f;item.completedRenderer.enabled=false;item.capRenderer.enabled=true;item.baseRenderer.enabled=true;var properties=new MaterialPropertyBlock();properties.SetFloat("_Activated",1);item.capRenderer.SetPropertyBlock(properties);
                }
                preview.AddSingleGO(objects[i]);
            }
            var renderers=objects.SelectMany(o=>o.GetComponentsInChildren<SpriteRenderer>()).Where(s=>s.enabled).ToArray();Bounds bounds=renderers[0].bounds;foreach(var r in renderers.Skip(1))bounds.Encapsulate(r.bounds);
            const int width=1200,height=720;preview.camera.orthographic=true;preview.camera.orthographicSize=Mathf.Max(bounds.extents.y,bounds.extents.x/(width/(float)height))*1.13f;preview.camera.transform.SetPositionAndRotation(bounds.center+Vector3.back*10,Quaternion.identity);preview.camera.nearClipPlane=.1f;preview.camera.farClipPlane=50;preview.camera.clearFlags=CameraClearFlags.SolidColor;preview.camera.backgroundColor=new Color32(34,40,51,255);
            preview.BeginStaticPreview(new Rect(0,0,width,height));preview.Render(true);Texture2D texture=preview.EndStaticPreview();File.WriteAllBytes(Out+"/normal-blue-green.png",texture.EncodeToPNG());UnityEngine.Object.DestroyImmediate(texture);
        }
        finally{preview.Cleanup();EditorSceneManager.ClosePreviewScene(staging);}
    }
}
