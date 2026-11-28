using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using Object=UnityEngine.Object;

[InitializeOnLoad]
public static class ShotPressureSwitchGhostOnlySetup
{
    public const string PrefabPath="Assets/Prefabs/Level/ShotPressureSwitch_GhostOnly_Ceiling.prefab";
    const string Ordinary="Assets/Prefabs/Level/ShotPressureSwitch_Ceiling.prefab";
    const string Base="Assets/Prefabs/Level/ShotPressureSwitch.prefab";
    const string Work="C:/Users/chenk/Documents/Codex/2026-11-23/ban-2";
    const string Out=Work+"/outputs/pressure-switch-ghost";
    const string QA="PressureSwitch.GhostOnly.QA";
    static double next,started,phaseAt,nextShot;
    static bool running,bodyDuringRecovery;
    static int phase,sent,events;
    static ShotPressureSwitch target,ordinary;
    static GameObject playerBullet,ghostBullet,ghostPrefab;
    static PlayerGhost replay;
    static PlayerRecorder recorder;
    static float initialHeadY;
    static Report report;
    [Serializable] class Report
    {
        public string prefab,scene,baseHash,ceilingHash;
        public bool prefabVariant,ceilingOrientation,sceneUntouched,originalAssetsPreserved,bodyRejected,normalStillWorks,partialGhostShots,recoveryDespiteBodyShots,actualEReplay,ghostNineHitsActivate,greenAndEffectOnce,permanentLatch,headOnly,headMovesUp,passed;
        public int acceptedShots,activationEvents;public List<string> errors=new List<string>();
    }
    static ShotPressureSwitchGhostOnlySetup(){EditorApplication.update+=Tick;EditorApplication.playModeStateChanged+=PlayState;}
    static string Hash(string path){using(var sha=SHA256.Create())return BitConverter.ToString(sha.ComputeHash(File.ReadAllBytes(path))).Replace("-","");}
    static void Tick()
    {
        if(running){try{Check();}catch(Exception e){Finish(e.ToString());}return;}
        if(EditorApplication.isCompiling||EditorApplication.isUpdating||EditorApplication.isPlayingOrWillChangePlaymode||EditorApplication.timeSinceStartup<next)return;next=EditorApplication.timeSinceStartup+1;
        if(!File.Exists(Out+"/create.request.txt"))return;File.Delete(Out+"/create.request.txt");
        try{Create();}catch(Exception e){File.WriteAllText(Out+"/setup-error.txt",e.ToString());Debug.LogException(e);}
    }
    [MenuItem("地图/射击机关/创建 E 分身限定倒挂版")]
    public static void Create()
    {
        if(EditorApplication.isPlayingOrWillChangePlaymode)return;
        Directory.CreateDirectory(Out);Directory.CreateDirectory("Logs/ShotPressureSwitch/Backups");
        Scene scene=SceneManager.GetActiveScene();bool dirty=scene.isDirty;
        report=new Report{prefab=PrefabPath,scene=scene.path,baseHash=Hash(Base),ceilingHash=Hash(Ordinary)};
        string stamp=DateTime.Now.ToString("yyyyMMdd_HHmmss");File.Copy(Work+"/work/pressure-switch-ghost/ShotPressureSwitch.before.txt","Logs/ShotPressureSwitch/Backups/ShotPressureSwitch_BeforeGhostFilter_"+stamp+".cs.txt",false);
        if(!AssetDatabase.LoadAssetAtPath<GameObject>(PrefabPath))
        {
            Scene preview=EditorSceneManager.NewPreviewScene();
            try
            {
                var instance=(GameObject)PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath<GameObject>(Ordinary),preview);instance.name="ShotPressureSwitch_GhostOnly_Ceiling";
                var item=instance.GetComponent<ShotPressureSwitch>();item.ghostOnly=true;item.acceptGhostBullets=true;PrefabUtility.RecordPrefabInstancePropertyModifications(item);
                if(!PrefabUtility.SaveAsPrefabAsset(instance,PrefabPath))throw new IOException("Could not save the E-only ceiling variant.");
            }
            finally{EditorSceneManager.ClosePreviewScene(preview);}
        }
        var asset=AssetDatabase.LoadAssetAtPath<GameObject>(PrefabPath);var component=asset.GetComponent<ShotPressureSwitch>();
        if(!component.ghostOnly||!component.acceptGhostBullets)throw new Exception("The new prefab does not have the expected bullet filter.");
        var normal=AssetDatabase.LoadAssetAtPath<GameObject>(Ordinary).GetComponent<ShotPressureSwitch>();if(normal.ghostOnly||!normal.acceptGhostBullets)throw new Exception("Existing switch defaults changed.");
        report.prefabVariant=PrefabUtility.GetPrefabAssetType(asset)==PrefabAssetType.Variant;
        report.ceilingOrientation=Vector3.Dot(asset.transform.up,Vector3.down)>.99f;
        report.sceneUntouched=SceneManager.GetActiveScene()==scene&&scene.isDirty==dirty;
        report.originalAssetsPreserved=Hash(Base)==report.baseHash&&Hash(Ordinary)==report.ceilingHash;
        if(!report.prefabVariant||!report.ceilingOrientation||!report.sceneUntouched||!report.originalAssetsPreserved)throw new Exception("Creation validation failed.");
        Write();SessionState.SetBool(QA,true);Selection.activeObject=asset;EditorGUIUtility.PingObject(asset);EditorApplication.delayCall+=EditorApplication.EnterPlaymode;
    }
    static void PlayState(PlayModeStateChange state)
    {
        if(state==PlayModeStateChange.EnteredPlayMode&&SessionState.GetBool(QA,false))
        {
            report=JsonUtility.FromJson<Report>(File.ReadAllText(Out+"/report.json"));running=true;phase=sent=events=0;bodyDuringRecovery=false;target=ordinary=null;replay=null;started=EditorApplication.timeSinceStartup;phaseAt=Time.time;Application.logMessageReceived+=Log;
        }
        if(state==PlayModeStateChange.ExitingPlayMode&&running)Finish("Validation interrupted.");
        if(state==PlayModeStateChange.EnteredEditMode&&SessionState.GetBool(QA+".Select",false))
        {SessionState.SetBool(QA+".Select",false);EditorApplication.delayCall+=()=>{var asset=AssetDatabase.LoadAssetAtPath<GameObject>(PrefabPath);Selection.activeObject=asset;EditorGUIUtility.PingObject(asset);};}
    }
    static void Log(string message,string trace,LogType type){if(running&&(type==LogType.Error||type==LogType.Exception))report.errors.Add(message);}
    static void Stage(int value){phase=value;phaseAt=Time.time;sent=0;nextShot=Time.time;}
    static void Fire(ShotPressureSwitch item,GameObject prefab)
    {
        Physics2D.SyncTransforms();Bounds b=item.hitbox.bounds;Vector3 position=b.center+Vector3.down*(b.extents.y+.65f);position.z=item.transform.position.z;Object.Instantiate(prefab,position,Quaternion.Euler(0,0,90));
    }
    static bool FitsHead(ShotPressureSwitch item)
    {
        Bounds head=item.capRenderer.bounds,box=item.hitbox.bounds;return item.GetComponentsInChildren<Collider2D>(true).Length==1&&Vector3.Distance(head.center,box.center)<head.size.y*.15f+.01f&&box.size.x<=head.size.x+.01f&&box.size.y<=head.size.y+.01f;
    }
    static void Check()
    {
        if(EditorApplication.timeSinceStartup-started>25){Finish("Timed out at phase "+phase);return;}
        if(phase==0)
        {
            if(Time.time-phaseAt<.2f)return;
            foreach(var p in Object.FindObjectsByType<PlayerShooting>(FindObjectsSortMode.None))p.enabled=false;
            var player=Object.FindFirstObjectByType<PlayerShooting>();recorder=Object.FindFirstObjectByType<PlayerRecorder>();if(!player||!recorder)throw new Exception("A player with E recorder is required for live projectile verification.");
            playerBullet=player.bulletPrefab;ghostPrefab=recorder.ghostPrefab;ghostBullet=ghostPrefab.GetComponent<PlayerGhost>().bulletPrefab;
            if(!playerBullet||!ghostBullet||playerBullet.GetComponent<Bullet>().isGhostBullet||!ghostBullet.GetComponent<Bullet>().isGhostBullet)throw new Exception("Player/ghost bullet prefabs are not distinguishable.");
            var asset=AssetDatabase.LoadAssetAtPath<GameObject>(PrefabPath);target=Object.Instantiate(asset,new Vector3(500,500,0),asset.transform.rotation).GetComponent<ShotPressureSwitch>();
            var normal=AssetDatabase.LoadAssetAtPath<GameObject>(Ordinary);ordinary=Object.Instantiate(normal,new Vector3(510,500,0),normal.transform.rotation).GetComponent<ShotPressureSwitch>();
            Physics2D.SyncTransforms();initialHeadY=target.capRenderer.bounds.center.y;report.headOnly=FitsHead(target)&&!target.hitbox.OverlapPoint(target.shaftRenderer.bounds.center)&&!target.hitbox.OverlapPoint(target.baseRenderer.bounds.center);target.onActivated.AddListener(()=>events++);Fire(ordinary,playerBullet);Stage(1);
        }
        else if(phase==1)
        {
            if(sent<9&&Time.time>=nextShot){Fire(target,playerBullet);sent++;nextShot=Time.time+.06;}
            if(sent<9||Time.time<nextShot+.2)return;
            report.bodyRejected=target.AcceptedShots==0&&target.Progress01==0&&target.VisualProgress01==0&&!target.IsActive;
            report.normalStillWorks=ordinary.AcceptedShots==1;Stage(2);
        }
        else if(phase==2)
        {
            if(sent<5&&Time.time>=nextShot){Fire(target,ghostBullet);sent++;nextShot=Time.time+.06;}
            if(target.AcceptedShots<5)return;
            report.partialGhostShots=target.AcceptedShots==5&&!target.IsLatched&&target.Progress01>.45f&&target.RemainingHits==4;Stage(3);
        }
        else if(phase==3)
        {
            report.headMovesUp|=target.capRenderer.bounds.center.y>initialHeadY+.05f;
            if(!bodyDuringRecovery&&Time.time-phaseAt>.45f){Fire(target,playerBullet);bodyDuringRecovery=true;}
            if(Time.time-phaseAt<1.12f)return;
            report.recoveryDespiteBodyShots=target.AcceptedShots==5&&target.Progress01<.001f&&target.RemainingHits==9;
            replay=Object.Instantiate(ghostPrefab,Vector3.zero,Quaternion.identity).GetComponent<PlayerGhost>();const float angle=89.9f;replay.SetInitialPose(false,angle);Vector3 launch=target.hitbox.bounds.center+Vector3.down*3.5f;replay.transform.position+=launch-replay.muzzlePoint.position;
            Vector3 root=replay.transform.position;var frames=new List<RecordedFrame>{new RecordedFrame(root,0,false,false,angle),new RecordedFrame(root,1.25f,false,false,angle)};var shots=Enumerable.Range(0,9).Select(i=>new RecordedShot(.08f+i*.07f,launch,Quaternion.Euler(0,0,angle))).ToList();replay.StartReplay(frames,shots);report.actualEReplay=recorder.IsReplaying;Stage(4);
        }
        else if(phase==4)
        {
            if(!target.IsActive)return;
            report.ghostNineHitsActivate=target.AcceptedShots==14&&target.RemainingHits==0&&target.IsLatched;
            var properties=new MaterialPropertyBlock();target.capRenderer.GetPropertyBlock(properties);report.greenAndEffectOnce=properties.GetFloat("_Activated")>.99f&&target.feedback.PlayCount==1&&events==1;
            Fire(target,playerBullet);Fire(target,ghostBullet);Fire(ordinary,playerBullet);Fire(ordinary,ghostBullet);Stage(5);
        }
        else if(phase==5&&Time.time-phaseAt>1.3f)
        {
            report.normalStillWorks&=ordinary.AcceptedShots==3;
            report.permanentLatch=target.IsActive&&target.IsLatched&&target.Progress01==1&&target.AcceptedShots==14&&events==1&&target.feedback.PlayCount==1;
            report.headOnly&=FitsHead(target);report.originalAssetsPreserved&=Hash(Base)==report.baseHash&&Hash(Ordinary)==report.ceilingHash;report.acceptedShots=target.AcceptedShots;report.activationEvents=events;Finish(null);
        }
    }
    static void Finish(string error)
    {
        if(!running)return;running=false;Application.logMessageReceived-=Log;if(error!=null)report.errors.Add(error);
        report.passed=report.prefabVariant&&report.ceilingOrientation&&report.sceneUntouched&&report.originalAssetsPreserved&&report.bodyRejected&&report.normalStillWorks&&report.partialGhostShots&&report.recoveryDespiteBodyShots&&report.actualEReplay&&report.ghostNineHitsActivate&&report.greenAndEffectOnce&&report.permanentLatch&&report.headOnly&&report.headMovesUp&&report.errors.Count==0;
        Write();SessionState.SetBool(QA,false);SessionState.SetBool(QA+".Select",true);Debug.Log("[PressureSwitch] E-only ceiling variant: "+(report.passed?"PASS":"See GhostOnly report"));EditorApplication.ExitPlaymode();
    }
    static void Write(){string text=JsonUtility.ToJson(report,true);File.WriteAllText(Out+"/report.json",text);File.WriteAllText("Logs/ShotPressureSwitch/GhostOnlyReport.json",text);}
}
