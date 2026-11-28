using System;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;

[InitializeOnLoad]
public static class Mapx11CameraBoundsSetup
{
    const string Work="C:/Users/chenk/Documents/Codex/2026-11-23/ban-2";
    const string Out=Work+"/outputs/mapx11-camera-bounds";
    const string QA="Mapx11.CameraLeftBounds.QA";
    static double next,caseAt,exitAt;
    static int stage;
    static OutpostPlayerCamera rig;
    static Camera main,color;
    static Rigidbody2D body;
    static Report report;
    static float originalAspect;
    static Vector3 originalCameraLocal;
    [Serializable] class Report
    {
        public string scene,backup,leftWall;
        public bool saved,playerUnchanged,terrainUnchanged,startupInside,firstFrameInside=true,allFramesInside=true,colorCameraSynced=true,followAwayFromBoundary,passed;
        public float worldLeftEdge,visibleWallWidth,minMargin=float.MaxValue;
        public string[] cases={"16:9 near left wall","4:3 near left wall","21:9 near left wall","maximum left look offset","child camera shake","paused resize","follow away from wall"};
        public int[] renderedFrames=new int[7];
    }
    static Mapx11CameraBoundsSetup(){EditorApplication.update+=Tick;EditorApplication.playModeStateChanged+=PlayState;RenderPipelineManager.beginCameraRendering+=RenderCheck;}
    static void Tick()
    {
        if(EditorApplication.isPlaying&&SessionState.GetBool(QA,false))
        {
            try
            {
                if(report==null)return;
                if(exitAt>0){if(EditorApplication.timeSinceStartup>=exitAt){exitAt=0;SessionState.SetBool(QA,false);Time.timeScale=1;EditorApplication.ExitPlaymode();}return;}
                if(EditorApplication.timeSinceStartup-caseAt<.55)return;
                if(stage==0)ScreenCapture.CaptureScreenshot(Out+"/left-edge-gameplay.png");
                if(stage==6)
                {
                    report.followAwayFromBoundary=main.transform.position.x>report.worldLeftEdge+main.orthographicSize*main.aspect+4&&rig.CurrentLookOffset.x>.02f;
                    report.passed=report.saved&&report.playerUnchanged&&report.terrainUnchanged&&report.startupInside&&report.firstFrameInside&&report.allFramesInside&&report.colorCameraSynced&&report.followAwayFromBoundary&&report.renderedFrames.All(n=>n>0);
                    Write();File.WriteAllText(Out+"/runtime.done.txt",report.passed?"PASS":"FAIL");exitAt=EditorApplication.timeSinceStartup+.1;return;
                }
                BeginCase(stage+1);
            }
            catch(Exception e){File.WriteAllText(Out+"/runtime-error.txt",e.ToString());Time.timeScale=1;SessionState.SetBool(QA,false);EditorApplication.ExitPlaymode();}
            return;
        }
        if(EditorApplication.isCompiling||EditorApplication.isUpdating||EditorApplication.isPlayingOrWillChangePlaymode||EditorApplication.timeSinceStartup<next)return;next=EditorApplication.timeSinceStartup+1;
        if(!File.Exists(Out+"/setup.request.txt"))return;File.Delete(Out+"/setup.request.txt");
        try{Configure();}catch(Exception e){File.WriteAllText(Out+"/setup-error.txt",e.ToString());Debug.LogException(e);}
    }
    static string Terrain(Scene s){return string.Join("\n",s.GetRootGameObjects().SelectMany(g=>g.GetComponentsInChildren<Collider2D>(true)).Select(c=>c.GetInstanceID()+EditorJsonUtility.ToJson(c)+c.transform.localToWorldMatrix.ToString("R")));}
    [MenuItem("地图/Sci-fi Lab/设置 Mapx11 左侧镜头边界")]
    public static void Configure()
    {
        Scene scene=SceneManager.GetActiveScene();if(scene.path!="Assets/Scenes/Levels/Mapx11.unity"||EditorApplication.isPlayingOrWillChangePlaymode)throw new Exception("Open Mapx11 in Edit mode first.");
        rig=scene.GetRootGameObjects().SelectMany(g=>g.GetComponentsInChildren<OutpostPlayerCamera>(true)).Single();main=rig.viewCamera;
        if(!main||!main.transform.IsChildOf(rig.transform))throw new Exception("Expected the existing child camera rig.");
        var wall=scene.GetRootGameObjects().Single(g=>g.name=="GroundMap").transform.Find("Ground (4)").GetComponent<Collider2D>();if(!wall||!wall.enabled)throw new Exception("The left wall collider is required.");
        string player=EditorJsonUtility.ToJson(rig.target),terrain=Terrain(scene);
        Directory.CreateDirectory(Out);Directory.CreateDirectory("Logs/LevelArt/Backups");if(!EditorSceneManager.SaveScene(scene))throw new IOException("Could not save the live scene for backup.");
        string backup="Logs/LevelArt/Backups/Mapx11_BeforeLeftCameraLimit_"+DateTime.Now.ToString("yyyyMMdd_HHmmss");File.Copy(scene.path,backup+".unity",false);File.Copy(Work+"/work/OutpostPlayerCamera_BeforeLeftBounds.cs.txt",backup+"_OutpostPlayerCamera.cs.txt",false);
        Undo.RecordObjects(new UnityEngine.Object[]{rig,rig.transform},"Limit left camera view");rig.limitLeftEdge=true;rig.leftBoundaryWall=wall;rig.visibleLeftWallWidth=.75f;rig.SnapToTarget();EditorUtility.SetDirty(rig);
        foreach(var sync in scene.GetRootGameObjects().SelectMany(g=>g.GetComponentsInChildren<CameraSync>(true)).Where(c=>c.mainCamera==main))
        {
            var camera=sync.GetComponent<Camera>();Undo.RecordObjects(new UnityEngine.Object[]{camera,camera.transform},"Sync bounded camera preview");camera.transform.SetPositionAndRotation(main.transform.position,main.transform.rotation);camera.orthographic=main.orthographic;camera.orthographicSize=main.orthographicSize;camera.aspect=main.aspect;
        }
        report=new Report{scene=scene.path,backup=backup+".unity",leftWall=wall.name,worldLeftEdge=wall.bounds.max.x-rig.visibleLeftWallWidth,visibleWallWidth=rig.visibleLeftWallWidth,playerUnchanged=player==EditorJsonUtility.ToJson(rig.target),terrainUnchanged=terrain==Terrain(scene)};
        if(!report.playerUnchanged||!report.terrainUnchanged)throw new Exception("Unexpected player or terrain change.");
        EditorSceneManager.MarkSceneDirty(scene);report.saved=EditorSceneManager.SaveScene(scene);if(!report.saved)throw new IOException("Could not save bounded camera setup.");Write();SessionState.SetBool(QA,true);EditorApplication.delayCall+=EditorApplication.EnterPlaymode;
    }
    static void PlayState(PlayModeStateChange state)
    {
        if(state!=PlayModeStateChange.EnteredPlayMode||!SessionState.GetBool(QA,false))return;
        report=JsonUtility.FromJson<Report>(File.ReadAllText(Out+"/camera-bounds-report.json"));rig=UnityEngine.Object.FindFirstObjectByType<OutpostPlayerCamera>();main=rig.viewCamera;color=UnityEngine.Object.FindObjectsByType<CameraSync>(FindObjectsSortMode.None).Single(c=>c.mainCamera==main).GetComponent<Camera>();body=rig.target.GetComponent<Rigidbody2D>();originalAspect=main.aspect;originalCameraLocal=main.transform.localPosition;
        report.startupInside=main.ViewportToWorldPoint(new Vector3(0,.5f,0)).x>=report.worldLeftEdge-.002f;
        var movement=rig.target.GetComponent<PlayerMovement>();var jump=rig.target.GetComponent<PlayerJump>();if(movement)movement.enabled=false;if(jump)jump.enabled=false;body.simulated=false;BeginCase(0);
    }
    static void BeginCase(int value)
    {
        stage=value;caseAt=EditorApplication.timeSinceStartup;Time.timeScale=1;main.transform.localPosition=originalCameraLocal;
        main.aspect=value==1?4f/3f:(value==2||value==5?21f/9f:16f/9f);
        Vector3 p=rig.target.position;p.x=value==6?12:rig.leftBoundaryWall.bounds.max.x+.65f;rig.target.position=p;body.linearVelocity=new Vector2(value==6?7:-7,0);rig.SnapToTarget();
        if(value==3)typeof(OutpostPlayerCamera).GetField("lookOffset",BindingFlags.Instance|BindingFlags.NonPublic).SetValue(rig,new Vector2(-main.orthographicSize*2*main.aspect*.12f,0));
        if(value==4){var shake=main.GetComponent<CameraShake>();if(shake)shake.Shake();else main.transform.localPosition=originalCameraLocal+new Vector3(-.12f,0,0);}
        if(value==5)Time.timeScale=0;
        if(value==6)main.aspect=originalAspect;
    }
    static void RenderCheck(ScriptableRenderContext context,Camera camera)
    {
        if(!EditorApplication.isPlaying||!SessionState.GetBool(QA,false)||report==null||camera!=main)return;
        float margin=main.ViewportToWorldPoint(new Vector3(0,.5f,0)).x-report.worldLeftEdge;bool inside=margin>=-.002f;
        if(report.renderedFrames.Sum()==0)report.firstFrameInside=inside;report.allFramesInside&=inside;report.minMargin=Mathf.Min(report.minMargin,margin);report.renderedFrames[stage]++;
        report.colorCameraSynced&=Vector3.Distance(main.transform.position,color.transform.position)<.001f&&Quaternion.Angle(main.transform.rotation,color.transform.rotation)<.001f&&Mathf.Abs(main.aspect-color.aspect)<.001f;
    }
    static void Write(){string json=JsonUtility.ToJson(report,true);File.WriteAllText(Out+"/camera-bounds-report.json",json);File.WriteAllText("Logs/LevelArt/Mapx11_CameraBounds_Report.json",json);}
}
