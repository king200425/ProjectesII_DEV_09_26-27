using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using Object=UnityEngine.Object;

[InitializeOnLoad]
public static class PlayerWallFrictionSetup
{
    static string Out => Path.GetFullPath(Path.Combine(Application.dataPath, "../Logs/PlayerMovement"));
    const string MaterialPath="Assets/PhysicsMaterials/Player_NoFriction.physicsMaterial2D";
    const string PlayerPrefab="Assets/Prefabs/Player/Player.prefab";
    const string Key="PlayerWallFriction.QA";
    static double next,started;
    static bool running;
    static int fixtureIndex;
    static PlayerMovement source;
    static PlayerJump sourceJump;
    static BoxCollider2D sourceBox;
    static Report report;
    static readonly BindingFlags Private=BindingFlags.Instance|BindingFlags.NonPublic;
    [Serializable] class Result{public string name;public float drop,rise,velocityY;public int wallSide;public bool passed;}
    [Serializable] class Report{public string scene,backup,material;public bool saved,playerSizeUnchanged,otherPhysicsUnchanged,baselineReproduced,fallbackApplied,ordinaryWallsFall,ordinaryWallsDoNotWallJump,normalJumpClear,taggedSlide,taggedJump,groundedStop,groundedJump,passed;public List<Result> cases=new List<Result>();public List<string> errors=new List<string>();}
    class Fixture{public GameObject root,actor;public Rigidbody2D body;public BoxCollider2D box;public PlayerMovement move;public PlayerJump jump;public Animator animator;}
    static PlayerWallFrictionSetup(){EditorApplication.update+=Tick;EditorApplication.playModeStateChanged+=PlayState;}
    static void Folder(string path){if(AssetDatabase.IsValidFolder(path))return;int i=path.LastIndexOf('/');Folder(path.Substring(0,i));AssetDatabase.CreateFolder(path.Substring(0,i),path.Substring(i+1));}
    static void Tick()
    {
        if(running){if(EditorApplication.timeSinceStartup-started>35)Finish("Physics regression check timed out.");return;}
        if(EditorApplication.timeSinceStartup<next||EditorApplication.isUpdating||EditorApplication.isCompiling||EditorApplication.isPlayingOrWillChangePlaymode)return;next=EditorApplication.timeSinceStartup+1;
        if(!File.Exists(Out+"/setup.request.txt"))return;File.Delete(Out+"/setup.request.txt");try{Configure();}catch(Exception e){File.WriteAllText(Out+"/error.txt",e.ToString());Debug.LogException(e);}
    }
    static string OtherPhysics(Scene s,Rigidbody2D player){return string.Join("\n",s.GetRootGameObjects().SelectMany(g=>g.GetComponentsInChildren<Collider2D>(true)).Where(c=>c.attachedRigidbody!=player).Select(c=>c.GetInstanceID()+EditorJsonUtility.ToJson(c)+c.transform.localToWorldMatrix.ToString("R")));}
    static void Bind(GameObject player,PhysicsMaterial2D material,bool undo)
    {
        var body=player.GetComponent<Rigidbody2D>();var solid=player.GetComponentsInChildren<Collider2D>(true).Where(c=>c.attachedRigidbody==body&&!c.isTrigger).ToArray();if(undo)Undo.RecordObjects(solid.Cast<Object>().Concat(new Object[]{body}).ToArray(),"Remove player wall friction");body.sharedMaterial=material;foreach(var c in solid)c.sharedMaterial=material;
    }
    [MenuItem("地图/角色/修复普通墙黏墙并验证蹬墙")]
    public static void Configure()
    {
        var scene=SceneManager.GetActiveScene();if(EditorApplication.isPlayingOrWillChangePlaymode||scene.path!="Assets/_Recovery/Mapx11.unity")throw new Exception("Open Mapx11 in Edit mode first.");
        var player=scene.GetRootGameObjects().SelectMany(g=>g.GetComponentsInChildren<PlayerMovement>(true)).Single();var body=player.GetComponent<Rigidbody2D>();string transformBefore=EditorJsonUtility.ToJson(player.transform),others=OtherPhysics(scene,body);
        Directory.CreateDirectory(Out);Directory.CreateDirectory("Logs/PlayerMovement/Backups");if(!EditorSceneManager.SaveScene(scene))throw new IOException("Could not save current live scene before backup.");string stamp=DateTime.Now.ToString("yyyyMMdd_HHmmss"),backup="Logs/PlayerMovement/Backups/BeforeWallFriction_"+stamp;File.Copy(scene.path,backup+"_Mapx11.unity",false);File.Copy(PlayerPrefab,backup+"_Player.prefab",false);File.Copy(PlayerPrefab+".meta",backup+"_Player.prefab.meta",false);File.Copy("Assets/Scripts/Player/PlayerMovement.cs",backup+"_PlayerMovement.cs.txt",false);
        Folder("Assets/PhysicsMaterials");var material=AssetDatabase.LoadAssetAtPath<PhysicsMaterial2D>(MaterialPath);if(!material){material=new PhysicsMaterial2D("Player_NoFriction"){friction=0,bounciness=0};AssetDatabase.CreateAsset(material,MaterialPath);}if(material.friction!=0||material.bounciness!=0)throw new Exception("The player material must have zero friction and bounce.");
        var prefab=PrefabUtility.LoadPrefabContents(PlayerPrefab);try{Bind(prefab,material,false);if(!PrefabUtility.SaveAsPrefabAsset(prefab,PlayerPrefab))throw new IOException("Could not save player prefab collision material.");}finally{PrefabUtility.UnloadPrefabContents(prefab);}
        Bind(player.gameObject,material,true);report=new Report{scene=scene.path,backup=backup,material=MaterialPath,playerSizeUnchanged=transformBefore==EditorJsonUtility.ToJson(player.transform),otherPhysicsUnchanged=others==OtherPhysics(scene,body)};
        if(!report.playerSizeUnchanged||!report.otherPhysicsUnchanged)throw new Exception("Unexpected player size or level collision change.");EditorSceneManager.MarkSceneDirty(scene);report.saved=EditorSceneManager.SaveScene(scene);if(!report.saved)throw new IOException("Could not save player collision material in Mapx11.");Write();SessionState.SetBool(Key,true);EditorApplication.delayCall+=EditorApplication.EnterPlaymode;
    }
    static void PlayState(PlayModeStateChange state)
    {
        if(state==PlayModeStateChange.EnteredPlayMode&&SessionState.GetBool(Key,false))
        {
            report=JsonUtility.FromJson<Report>(File.ReadAllText(Out+"/report.json"));source=Object.FindFirstObjectByType<PlayerMovement>();sourceJump=source.GetComponent<PlayerJump>();sourceBox=source.GetComponent<BoxCollider2D>();running=true;fixtureIndex=0;started=EditorApplication.timeSinceStartup;source.StartCoroutine(Drive(Run()));
        }
        if(state==PlayModeStateChange.ExitingPlayMode&&running)Finish("Physics verification interrupted.");
    }
    static IEnumerator Drive(IEnumerator routine)
    {
        while(running)
        {
            bool move;object current=null;try{move=routine.MoveNext();if(move)current=routine.Current;}catch(Exception e){Finish(e.ToString());yield break;}if(!move)yield break;if(current is IEnumerator child)yield return Drive(child);else yield return current;
        }
    }
    static void Call(object target,string method){target.GetType().GetMethod(method,Private).Invoke(target,null);}
    static void Step(Fixture f,int horizontal,bool jump=false)
    {
        typeof(PlayerMovement).GetField("horizontal",Private).SetValue(f.move,(float)horizontal);if(jump)typeof(PlayerJump).GetField("jumpPressed",Private).SetValue(f.jump,true);Call(f.jump,"FixedUpdate");Call(f.move,"FixedUpdate");
    }
    static Fixture Create(int side,bool tagged,bool baseline,bool floor=false)
    {
        var f=new Fixture();f.root=new GameObject("Wall friction QA "+fixtureIndex);f.root.transform.position=new Vector3(1000+fixtureIndex++*20,8,0);f.actor=new GameObject("QA Player");f.actor.SetActive(false);f.actor.transform.SetParent(f.root.transform,false);f.body=f.actor.AddComponent<Rigidbody2D>();f.body.constraints=RigidbodyConstraints2D.FreezeRotation;f.body.mass=source.GetComponent<Rigidbody2D>().mass;
        f.box=f.actor.AddComponent<BoxCollider2D>();Vector3 scale=sourceBox.transform.lossyScale;f.box.size=Vector2.Scale(sourceBox.size,new Vector2(Mathf.Abs(scale.x),Mathf.Abs(scale.y)));f.box.offset=(Vector2)sourceBox.transform.TransformVector(sourceBox.offset);
        var check=new GameObject("GroundCheck");check.transform.SetParent(f.actor.transform,false);check.transform.localPosition=source.transform.Find("GroundCheck").position-source.transform.position;
        var visual=new GameObject("Visual",typeof(SpriteRenderer),typeof(Animator));visual.transform.SetParent(f.actor.transform,false);visual.GetComponent<SpriteRenderer>().enabled=false;f.animator=visual.GetComponent<Animator>();f.animator.runtimeAnimatorController=source.GetComponentInChildren<Animator>().runtimeAnimatorController;f.animator.cullingMode=AnimatorCullingMode.AlwaysAnimate;
        f.move=f.actor.AddComponent<PlayerMovement>();f.move.moveSpeed=source.moveSpeed;f.move.sprintMultiplier=source.sprintMultiplier;f.jump=f.actor.AddComponent<PlayerJump>();f.jump.jumpForce=sourceJump.jumpForce;f.jump.lowJumpMultiplier=sourceJump.lowJumpMultiplier;f.jump.maxJumps=sourceJump.maxJumps;f.jump.secondJumpMultiplier=sourceJump.secondJumpMultiplier;f.jump.normalGravity=sourceJump.normalGravity;f.jump.fallGravityMultiplier=sourceJump.fallGravityMultiplier;f.jump.groundLayer=sourceJump.groundLayer;f.jump.groundCheckRadius=sourceJump.groundCheckRadius;
        foreach(string name in new[]{"wallSlideSpeed","wallJumpHorizontalForce","wallJumpVerticalMultiplier","wallJumpInputBuffer"}){var field=typeof(PlayerJump).GetField(name,Private);field.SetValue(f.jump,field.GetValue(sourceJump));}
        f.actor.SetActive(true);f.animator.Rebind();f.animator.Update(0);f.move.enabled=false;f.jump.enabled=false;Call(f.move,"Start");
        report.fallbackApplied|=f.body.sharedMaterial&&f.body.sharedMaterial.friction==0&&f.box.sharedMaterial&&f.box.sharedMaterial.friction==0;
        if(baseline){f.body.sharedMaterial=null;f.box.sharedMaterial=null;}
        var wall=new GameObject(floor?"QA Floor":tagged?"QA WallJump":"QA Ordinary Wall",typeof(BoxCollider2D));wall.transform.SetParent(f.root.transform,false);wall.layer=LayerMask.NameToLayer("Ground");wall.tag=tagged?"WallJump":"Untagged";var box=wall.GetComponent<BoxCollider2D>();
        if(floor){box.size=new Vector2(30,1);wall.transform.localPosition=new Vector3(f.box.offset.x,f.box.offset.y-f.box.size.y*.5f-.65f-.5f,0);}else{box.size=new Vector2(1,200);wall.transform.localPosition=new Vector3(f.box.offset.x+side*(f.box.size.x*.5f+.505f),0,0);}
        Physics2D.SyncTransforms();return f;
    }
    static IEnumerator Fall(int side,bool baseline)
    {
        Fixture f=Create(side,false,baseline);float start=f.body.position.y;bool ordinary=true;for(int i=0;i<30;i++){Step(f,side);yield return new WaitForFixedUpdate();ordinary&=f.jump.WallSide==0;}
        float drop=start-f.body.position.y;bool passed=ordinary&&(baseline?Mathf.Abs(drop)<.15f:drop>1f);report.cases.Add(new Result{name=baseline?"Original default friction reproduces sticking":side<0?"Hold A against ordinary wall":"Hold D against ordinary wall",drop=drop,wallSide=f.jump.WallSide,velocityY=f.body.linearVelocity.y,passed=passed});if(baseline)report.baselineReproduced=passed;else{report.ordinaryWallsFall&=passed;report.ordinaryWallsDoNotWallJump&=ordinary;}Object.Destroy(f.root);
    }
    static IEnumerator NormalJump(int side)
    {
        Fixture f=Create(side,false,false);float start=f.body.position.y,highest=start;bool ordinary=true;for(int i=0;i<45;i++){Step(f,side,i==0);yield return new WaitForFixedUpdate();highest=Mathf.Max(highest,f.body.position.y);ordinary&=f.jump.WallSide==0;}
        bool passed=ordinary&&highest-start>1f;report.normalJumpClear&=passed;report.cases.Add(new Result{name=side<0?"Ordinary left wall jump remains free":"Ordinary right wall jump remains free",rise=highest-start,wallSide=f.jump.WallSide,passed=passed});Object.Destroy(f.root);
    }
    static IEnumerator Tagged(int side)
    {
        Fixture f=Create(side,true,false);for(int i=0;i<12;i++){Step(f,side);yield return new WaitForFixedUpdate();}
        float speed=(float)typeof(PlayerJump).GetField("wallSlideSpeed",Private).GetValue(f.jump);bool slide=f.jump.WallSide==side&&Mathf.Abs(f.body.linearVelocity.y+speed)<.15f;report.taggedSlide&=slide;report.cases.Add(new Result{name=side<0?"Tagged left wall slide":"Tagged right wall slide",velocityY=f.body.linearVelocity.y,wallSide=f.jump.WallSide,passed=slide});
        Step(f,side,true);yield return new WaitForFixedUpdate();bool jump=f.body.linearVelocity.x*side<-.1f&&f.body.linearVelocity.y>0;report.taggedJump&=jump;report.cases.Add(new Result{name=side<0?"Tagged left wall kick":"Tagged right wall kick",velocityY=f.body.linearVelocity.y,passed=jump});Object.Destroy(f.root);
    }
    static IEnumerator Ground()
    {
        Fixture f=Create(0,false,false,true);float x=f.body.position.x;for(int i=0;i<30;i++){Step(f,0);yield return new WaitForFixedUpdate();}report.groundedStop=Mathf.Abs(f.body.position.x-x)<.02f&&Mathf.Abs(f.body.linearVelocity.y)<.05f&&f.animator.GetBool("isGrounded");float y=f.body.position.y;for(int i=0;i<12;i++){Step(f,0,i==0);yield return new WaitForFixedUpdate();}report.groundedJump=f.body.position.y>y+.5f;report.cases.Add(new Result{name="Ground idle and jump",rise=f.body.position.y-y,passed=report.groundedStop&&report.groundedJump});Object.Destroy(f.root);
    }
    static IEnumerator Run()
    {
        yield return new WaitForSeconds(.25f);report.ordinaryWallsFall=report.ordinaryWallsDoNotWallJump=report.normalJumpClear=report.taggedSlide=report.taggedJump=true;
        yield return Fall(1,true);yield return Fall(-1,false);yield return Fall(1,false);yield return NormalJump(-1);yield return NormalJump(1);yield return Tagged(-1);yield return Tagged(1);yield return Ground();Finish(null);
    }
    static void Finish(string error)
    {
        if(!running)return;running=false;if(error!=null)report.errors.Add(error);report.passed=report.saved&&report.playerSizeUnchanged&&report.otherPhysicsUnchanged&&report.baselineReproduced&&report.fallbackApplied&&report.ordinaryWallsFall&&report.ordinaryWallsDoNotWallJump&&report.normalJumpClear&&report.taggedSlide&&report.taggedJump&&report.groundedStop&&report.groundedJump&&report.cases.All(c=>c.passed)&&report.errors.Count==0;Write();SessionState.SetBool(Key,false);Debug.Log("[Player] Wall friction / wall slide / wall jump: "+(report.passed?"PASS":"See player wall friction report"));EditorApplication.ExitPlaymode();
    }
    static void Write(){string json=JsonUtility.ToJson(report,true);File.WriteAllText(Out+"/report.json",json);File.WriteAllText("Logs/PlayerMovement/WallFrictionReport.json",json);}
}
