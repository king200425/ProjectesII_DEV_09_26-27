using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
using Object=UnityEngine.Object;

[InitializeOnLoad]
public static class Mapx11LabSetup
{
    const string Workspace="C:/Users/chenk/Documents/Codex/2026-11-23/ban-2";
    const string Out=Workspace+"/outputs/mapx11-scifi-lab-applied";
    const string AtlasPath="Assets/Art/Environment/SciFiLab/level_tileset.png";
    const string Geometry="Assets/Meshes/Environment/SciFiLab";
    const string MaterialPath="Assets/Materials/LevelArt/SciFiLab_Unlit.mat";
    const string Visual="SciFiLab_Visual";
    const float Unit=1.25f;
    static readonly string[] Names={"Ground","Ground (1)","Ground (4)","Ground (2)"};
    static double next;
    static double runtimeAt,exitAt;
    const string QAKey="Mapx11Lab.StandingQA";
    [Serializable] class Report {public string scene,backup,platformShape,platformMeshBackup;public string[] blocks;public bool saved,playerUnchanged,camerasUnchanged,otherPhysicsUnchanged,flatPlatformTop,triangleVoidClear,geometryClipped,standingTest,noForwardExtension,otherGeometryUnchanged;public float platformTop,platformWidth,gapToNextPlatform,bottomFlatWidth,slopeStartX,platformRightEdge;public int meshVertices,triangleCount;}
    struct Vertex { public Vector2 p,uv;public Vertex(Vector2 p,Vector2 uv){this.p=p;this.uv=uv;} }
    class Builder
    {
        public Transform owner;public Rect bounds;public bool wedge;public Texture atlas;public float bottomRightX=float.NaN;
        public readonly List<Vector3> vertices=new List<Vector3>();public readonly List<Vector2> uv=new List<Vector2>();public readonly List<int> indices=new List<int>();
        float SlopeStart=>float.IsNaN(bottomRightX)?bounds.xMin:bottomRightX;
        float Distance(Vector2 p){return p.y-bounds.yMin-(p.x-SlopeStart)/(bounds.xMax-SlopeStart)*bounds.height;}
        static List<Vertex> Clip(List<Vertex> input,Func<Vector2,float> distance)
        {
            var output=new List<Vertex>();
            for(int i=0;i<input.Count;i++){var a=input[i];var b=input[(i+1)%input.Count];float da=distance(a.p),db=distance(b.p);bool ina=da>=-.00001f,inb=db>=-.00001f;if(ina)output.Add(a);if(ina!=inb){float t=da/(da-db);output.Add(new Vertex(Vector2.Lerp(a.p,b.p,t),Vector2.Lerp(a.uv,b.uv,t)));}}
            return output;
        }
        public void Polygon(List<Vertex> input)
        {
            input=Clip(input,p=>p.x-bounds.xMin);input=Clip(input,p=>bounds.xMax-p.x);input=Clip(input,p=>p.y-bounds.yMin);input=Clip(input,p=>bounds.yMax-p.y);
            if(wedge)
            {
                var output=new List<Vertex>();
                for(int i=0;i<input.Count;i++)
                {
                    var a=input[i];var b=input[(i+1)%input.Count];float da=Distance(a.p),db=Distance(b.p);bool ina=da>=-.00001f,inb=db>=-.00001f;
                    if(ina)output.Add(a);if(ina!=inb){float t=da/(da-db);output.Add(new Vertex(Vector2.Lerp(a.p,b.p,t),Vector2.Lerp(a.uv,b.uv,t)));}
                }
                input=output;
            }
            if(input.Count<3)return;
            int start=vertices.Count;foreach(var v in input){vertices.Add(owner.InverseTransformPoint(new Vector3(v.p.x,v.p.y,owner.position.z)));uv.Add(v.uv);}
            for(int i=1;i<input.Count-1;i++){indices.Add(start);indices.Add(start+i);indices.Add(start+i+1);}
        }
        public void Quad(Rect source,Rect target)
        {
            var d=Rect.MinMaxRect(Mathf.Max(target.xMin,bounds.xMin),Mathf.Max(target.yMin,bounds.yMin),Mathf.Min(target.xMax,bounds.xMax),Mathf.Min(target.yMax,bounds.yMax));if(d.width<=0||d.height<=0)return;
            float u0=source.xMin+(d.xMin-target.xMin)/target.width*source.width,u1=source.xMin+(d.xMax-target.xMin)/target.width*source.width;
            float v0=source.yMin+(d.yMin-target.yMin)/target.height*source.height,v1=source.yMin+(d.yMax-target.yMin)/target.height*source.height;
            Polygon(new List<Vertex>{new Vertex(new Vector2(d.xMin,d.yMin),new Vector2(u0,v0)),new Vertex(new Vector2(d.xMax,d.yMin),new Vector2(u1,v0)),new Vertex(new Vector2(d.xMax,d.yMax),new Vector2(u1,v1)),new Vertex(new Vector2(d.xMin,d.yMax),new Vector2(u0,v1))});
        }
        public Rect UV(int col,int row){return new Rect(col*32f/atlas.width,1-(row+1)*32f/atlas.height,32f/atlas.width,32f/atlas.height);}
        public void Panels(float x,float y,int columns,int rows)
        {
            for(int iy=0;iy<rows;iy++)for(int ix=0;ix<columns;ix++){int col=8,row=3;if(iy==rows-1&&(ix==1||ix==columns-2)){col=6;row=4;}else if(ix==0&&iy<rows-1){col=7;row=3;}Quad(UV(col,row),new Rect(x+ix*Unit,y+iy*Unit,Unit,Unit));}
        }
        public void DiagonalTrim()
        {
            Vector2 a=new Vector2(SlopeStart,bounds.yMin),b=new Vector2(bounds.xMax,bounds.yMax);float length=Vector2.Distance(a,b);Vector2 direction=(b-a).normalized,normal=new Vector2(-direction.y,direction.x)*.16f;
            for(float t=0;t<length;t+=Unit)
            {
                float size=Mathf.Min(Unit,length-t);Vector2 p=a+direction*t,q=a+direction*(t+size);var u=new Rect(32f/atlas.width,160f/atlas.height,32f/atlas.width*size/Unit,5f/atlas.height);
                Polygon(new List<Vertex>{new Vertex(p,new Vector2(u.xMin,u.yMin)),new Vertex(q,new Vector2(u.xMax,u.yMin)),new Vertex(q+normal,new Vector2(u.xMax,u.yMax)),new Vertex(p+normal,new Vector2(u.xMin,u.yMax))});
            }
        }
        public Mesh Mesh(string name){var m=new Mesh{name=name};if(vertices.Count>65535)m.indexFormat=UnityEngine.Rendering.IndexFormat.UInt32;m.SetVertices(vertices);m.SetUVs(0,uv);m.SetTriangles(indices,0);m.RecalculateBounds();return m;}
    }
    static Mapx11LabSetup(){EditorApplication.update+=Tick;EditorApplication.playModeStateChanged+=PlayState;}
    static void PlayState(PlayModeStateChange state)
    {
        if(state==PlayModeStateChange.EnteredPlayMode&&SessionState.GetBool(QAKey,false))runtimeAt=EditorApplication.timeSinceStartup+2;
        if(state==PlayModeStateChange.EnteredEditMode&&SessionState.GetBool("Mapx11Lab.FrameOnce",false)){SessionState.SetBool("Mapx11Lab.FrameOnce",false);EditorApplication.delayCall+=FrameSection;}
    }
    static void Tick()
    {
        if(EditorApplication.isPlaying&&SessionState.GetBool(QAKey,false))
        {
            if(runtimeAt>0&&EditorApplication.timeSinceStartup>=runtimeAt){runtimeAt=0;RuntimeCheck();exitAt=EditorApplication.timeSinceStartup+2;}
            if(exitAt>0&&EditorApplication.timeSinceStartup>=exitAt){exitAt=0;SessionState.SetBool(QAKey,false);SessionState.SetBool("Mapx11Lab.FrameOnce",true);EditorApplication.ExitPlaymode();}
            return;
        }
        if(EditorApplication.timeSinceStartup<next||EditorApplication.isCompiling||EditorApplication.isUpdating||EditorApplication.isPlayingOrWillChangePlaymode)return;next=EditorApplication.timeSinceStartup+1;
        string retry=Out+"/runtime.request.txt";if(File.Exists(retry)){File.Delete(retry);SessionState.SetBool(QAKey,true);EditorApplication.EnterPlaymode();return;}
        string adjust=Out+"/platform-adjust.request.txt";if(File.Exists(adjust)){File.Delete(adjust);try{AdjustPlatform();}catch(Exception e){File.WriteAllText(Out+"/platform-adjust-error.txt",e.ToString());Debug.LogException(e);}return;}
        string path=Out+"/apply.request.txt";if(!File.Exists(path))return;File.Delete(path);
        try{Apply();}catch(Exception e){Directory.CreateDirectory(Out);File.WriteAllText(Out+"/error.txt",e.ToString());Debug.LogException(e);}
    }
    static void Folder(string path){if(AssetDatabase.IsValidFolder(path))return;int at=path.LastIndexOf('/');Folder(path.Substring(0,at));AssetDatabase.CreateFolder(path.Substring(0,at),path.Substring(at+1));}
    static Rect RectOf(Bounds b){return Rect.MinMaxRect(b.min.x,b.min.y,b.max.x,b.max.y);}
    static bool Occupied(Rect[] blocks,int wedgeIndex,Vector2 p)
    {
        for(int i=0;i<blocks.Length;i++){Rect b=blocks[i];if(!b.Contains(p))continue;if(i!=wedgeIndex||p.y>=b.yMin+(p.x-b.xMin)/b.width*b.height)return true;}return false;
    }
    static string PhysicsSnapshot(Scene scene,IEnumerable<GameObject> except)
    {
        var skip=new HashSet<GameObject>(except);return string.Join("\n",scene.GetRootGameObjects().SelectMany(g=>g.GetComponentsInChildren<Collider2D>(true)).Where(c=>!skip.Contains(c.gameObject)).OrderBy(c=>c.GetInstanceID()).Select(c=>c.GetInstanceID()+EditorJsonUtility.ToJson(c)+c.transform.localToWorldMatrix.ToString("R")));
    }
    static string Cameras(Scene scene){return string.Join("\n",scene.GetRootGameObjects().SelectMany(g=>g.GetComponentsInChildren<Camera>(true)).Select(c=>EditorJsonUtility.ToJson(c)+EditorJsonUtility.ToJson(c.transform)));}
    [MenuItem("地图/Sci-fi Lab/铺设第一小段（右侧平顶三角平台）")]
    public static void Apply()
    {
        if(EditorApplication.isPlayingOrWillChangePlaymode)throw new Exception("Exit Play mode before applying terrain art.");
        Scene scene=SceneManager.GetActiveScene();if(scene.path!="Assets/Scenes/Levels/Mapx11.unity")throw new Exception("Expected the active Mapx11 scene; no changes made.");
        Directory.CreateDirectory(Out);Directory.CreateDirectory("Logs/LevelArt/Backups");
        var root=scene.GetRootGameObjects().Single(g=>g.name=="GroundMap");var sources=Names.Select(n=>root.GetComponentsInChildren<SpriteRenderer>(true).Single(s=>s.name==n)).ToArray();
        if(sources.Any(s=>s.transform.Find(Visual)))throw new Exception("This section already has Sci-fi Lab art; use Undo or restore its backup before applying again.");
        var player=scene.GetRootGameObjects().Single(g=>g.name=="Player");string playerBefore=EditorJsonUtility.ToJson(player.transform),cameraBefore=Cameras(scene),physicsBefore=PhysicsSnapshot(scene,new[]{sources[3].gameObject});
        var rects=sources.Select(s=>RectOf(s.bounds)).ToArray();rects[3].xMax+=.75f;
        if(!EditorSceneManager.SaveScene(scene))throw new IOException("Could not save the current live scene before backup.");
        string backup="Logs/LevelArt/Backups/Mapx11_BeforeSciFiLab_"+DateTime.Now.ToString("yyyyMMdd_HHmmss")+".unity";File.Copy(scene.path,backup,false);
        File.Copy(scene.path,Out+"/Mapx11_BeforeSciFiLab.unity",true);
        Folder(Geometry);Folder("Assets/Materials/LevelArt");
        AssetDatabase.ImportAsset(AtlasPath,ImportAssetOptions.ForceSynchronousImport);var importer=(TextureImporter)AssetImporter.GetAtPath(AtlasPath);importer.textureType=TextureImporterType.Default;importer.filterMode=FilterMode.Point;importer.mipmapEnabled=false;importer.textureCompression=TextureImporterCompression.Uncompressed;importer.wrapMode=TextureWrapMode.Clamp;importer.npotScale=TextureImporterNPOTScale.None;importer.alphaIsTransparency=true;importer.maxTextureSize=2048;importer.SaveAndReimport();
        var atlas=AssetDatabase.LoadAssetAtPath<Texture2D>(AtlasPath);var shader=Shader.Find("LevelArt/SciFiLabUnlit");if(!shader||ShaderUtil.ShaderHasError(shader))throw new Exception("Sci-fi Lab shader is missing or failed compilation.");
        var material=AssetDatabase.LoadAssetAtPath<Material>(MaterialPath);if(!material){material=new Material(shader){name="SciFiLab_Unlit"};AssetDatabase.CreateAsset(material,MaterialPath);}material.mainTexture=atlas;EditorUtility.SetDirty(material);
        Undo.IncrementCurrentGroup();int group=Undo.GetCurrentGroup();Undo.SetCurrentGroupName("试铺 Sci-fi Lab 第一小段");
        var report=new Report {scene=scene.path,backup=backup,blocks=Names,geometryClipped=true,platformTop=rects[3].yMax,platformWidth=rects[3].width};
        try
        {
            for(int i=0;i<sources.Length;i++)
            {
                var sr=sources[i];Rect b=rects[i];var builder=new Builder{owner=sr.transform,bounds=b,wedge=i==3,atlas=atlas};
                for(float x=Mathf.Floor(b.xMin/Unit)*Unit;x<b.xMax;x+=Unit)for(float y=Mathf.Floor(b.yMin/Unit)*Unit;y<b.yMax;y+=Unit){int ix=Mathf.RoundToInt(x/Unit),iy=Mathf.RoundToInt(y/Unit);builder.Quad(builder.UV(1+Mathf.Abs(ix+iy)%2,1+Mathf.Abs(ix*3+iy)%2),new Rect(x,y,Unit,Unit));}
                float step=Unit/8;
                for(float x=b.xMin;x<b.xMax;x+=step)
                {
                    float w=Mathf.Min(step,b.xMax-x),mid=x+w/2,phase=Mathf.Repeat(x-b.xMin,Unit);
                    if(!Occupied(rects,3,new Vector2(mid,b.yMax+.005f)))builder.Quad(new Rect((32+phase/Unit*32)/atlas.width,1-32f/atlas.height,w/Unit*32/atlas.width,32f/atlas.height),new Rect(x,b.yMax-Unit,w,Unit));
                    if(i!=3&&!Occupied(rects,3,new Vector2(mid,b.yMin-.005f)))builder.Quad(new Rect((32+phase/Unit*32)/atlas.width,1-128f/atlas.height,w/Unit*32/atlas.width,32f/atlas.height),new Rect(x,b.yMin,w,Unit));
                }
                for(float y=b.yMin;y<b.yMax;y+=step)
                {
                    float h=Mathf.Min(step,b.yMax-y),mid=y+h/2,phase=Mathf.Repeat(y-b.yMin,Unit);
                    if(!Occupied(rects,3,new Vector2(b.xMin-.005f,mid)))builder.Quad(new Rect(0,(atlas.height-96+phase/Unit*32)/atlas.height,32f/atlas.width,h/Unit*32/atlas.height),new Rect(b.xMin,y,Unit,h));
                    if(i!=3&&!Occupied(rects,3,new Vector2(b.xMax+.005f,mid)))builder.Quad(new Rect(96f/atlas.width,(atlas.height-96+phase/Unit*32)/atlas.height,32f/atlas.width,h/Unit*32/atlas.height),new Rect(b.xMax-Unit,y,Unit,h));
                }
                if(i==0){builder.Panels(0,b.yMax-Unit*3.4f,5,2);builder.Panels(13,b.yMax-Unit*3.4f,5,2);}else if(i==1)builder.Panels(b.xMin+Unit*.8f,b.yMin+Unit*2.2f,4,3);else if(i==2)builder.Panels(b.xMin+Unit*1.1f,b.yMin+Unit*9f,3,4);else builder.DiagonalTrim();
                foreach(Vector3 local in builder.vertices){Vector3 p=sr.transform.TransformPoint(local);if(p.x<b.xMin-.001f||p.x>b.xMax+.001f||p.y<b.yMin-.001f||p.y>b.yMax+.001f)report.geometryClipped=false;}
                var mesh=builder.Mesh("Mapx11_Lab_"+i);string meshPath=Geometry+"/Mapx11_Lab_"+i+".asset";var existing=AssetDatabase.LoadAssetAtPath<Mesh>(meshPath);if(existing){EditorUtility.CopySerialized(mesh,existing);Object.DestroyImmediate(mesh);mesh=existing;}else AssetDatabase.CreateAsset(mesh,meshPath);report.meshVertices+=mesh.vertexCount;report.triangleCount+=mesh.triangles.Length/3;
                var child=new GameObject(Visual,typeof(MeshFilter),typeof(MeshRenderer));Undo.RegisterCreatedObjectUndo(child,"Add laboratory art");child.layer=sr.gameObject.layer;child.transform.SetParent(sr.transform,false);child.GetComponent<MeshFilter>().sharedMesh=mesh;var renderer=child.GetComponent<MeshRenderer>();renderer.sharedMaterial=material;renderer.sortingLayerID=sr.sortingLayerID;renderer.sortingOrder=sr.sortingOrder;renderer.shadowCastingMode=UnityEngine.Rendering.ShadowCastingMode.Off;renderer.receiveShadows=false;
                Undo.RecordObject(sr,"Hide white block renderer");sr.enabled=false;
            }
            var box=sources[3].GetComponent<BoxCollider2D>();if(!box)throw new Exception("Right platform has no original BoxCollider2D");Undo.RecordObject(box,"Replace platform box with triangular collision");box.enabled=false;
            var polygon=Undo.AddComponent<PolygonCollider2D>(sources[3].gameObject);Rect platform=rects[3];polygon.points=new[]{new Vector2(platform.xMin,platform.yMin),new Vector2(platform.xMax,platform.yMax),new Vector2(platform.xMin,platform.yMax)}.Select(p=>(Vector2)polygon.transform.InverseTransformPoint(new Vector3(p.x,p.y,polygon.transform.position.z))).ToArray();polygon.sharedMaterial=box.sharedMaterial;
            Physics2D.SyncTransforms();report.flatPlatformTop=true;
            for(int i=0;i<5;i++){float x=Mathf.Lerp(platform.xMin+.2f,platform.xMax-.2f,i/4f);var hit=Physics2D.Raycast(new Vector2(x,platform.yMax+1),Vector2.down,1.1f,1<<sources[3].gameObject.layer);if(hit.collider!=polygon||Mathf.Abs(hit.point.y-platform.yMax)>.01f)report.flatPlatformTop=false;}
            report.triangleVoidClear=!polygon.OverlapPoint(new Vector2(platform.xMin+platform.width*.8f,platform.yMin+platform.height*.2f));
            report.playerUnchanged=playerBefore==EditorJsonUtility.ToJson(player.transform);report.camerasUnchanged=cameraBefore==Cameras(scene);report.otherPhysicsUnchanged=physicsBefore==PhysicsSnapshot(scene,new[]{sources[3].gameObject});
            var nextPlatform=root.transform.Find("Ground (3)");report.gapToNextPlatform=nextPlatform?nextPlatform.GetComponent<Collider2D>().bounds.min.x-platform.xMax:0;
            if(!report.geometryClipped||!report.flatPlatformTop||!report.triangleVoidClear||!report.playerUnchanged||!report.camerasUnchanged||!report.otherPhysicsUnchanged)throw new Exception("Level surface validation failed: "+JsonUtility.ToJson(report));
            AssetDatabase.SaveAssets();EditorSceneManager.MarkSceneDirty(scene);report.saved=EditorSceneManager.SaveScene(scene);if(!report.saved)throw new IOException("Could not save the dressed scene");Undo.CollapseUndoOperations(group);SceneView.RepaintAll();EditorApplication.QueuePlayerLoopUpdate();
            Directory.CreateDirectory("Logs/LevelArt");string json=JsonUtility.ToJson(report,true);File.WriteAllText(Out+"/applied-report.json",json);File.WriteAllText("Logs/LevelArt/Mapx11_SciFiLab_Report.json",json);Debug.Log("[LevelArt] Four blocks dressed; flat-top triangular platform saved. Starting runtime landing validation.");
            SessionState.SetBool(QAKey,true);EditorApplication.delayCall+=EditorApplication.EnterPlaymode;
        }
        catch{Undo.RevertAllDownToGroup(group);EditorSceneManager.SaveScene(scene);throw;}
    }
    static void RuntimeCheck()
    {
        try
        {
            Scene scene=SceneManager.GetActiveScene();var root=scene.GetRootGameObjects().Single(g=>g.name=="GroundMap");var platform=root.transform.Find("Ground (2)").GetComponent<PolygonCollider2D>();var player=scene.GetRootGameObjects().Single(g=>g.name=="Player");
            var report=JsonUtility.FromJson<Report>(File.ReadAllText(Out+"/applied-report.json"));report.standingTest=StandTest(platform,RectOf(platform.bounds),player.GetComponent<BoxCollider2D>());
            string json=JsonUtility.ToJson(report,true);File.WriteAllText(Out+"/applied-report.json",json);File.WriteAllText("Logs/LevelArt/Mapx11_SciFiLab_Report.json",json);
            if(!report.standingTest)throw new Exception("Standing test failed");
            ScreenCapture.CaptureScreenshot(Out+"/gameplay.png");File.WriteAllText(Out+"/runtime.done.txt","Three landing positions on the actual flat-top platform collision passed in an isolated runtime PhysicsScene2D.");
        }
        catch(Exception e){File.WriteAllText(Out+"/runtime-error.txt",e.ToString());Debug.LogException(e);}
    }
    static Rect SourceBounds(SpriteRenderer source)
    {
        if(source.drawMode!=SpriteDrawMode.Simple||Mathf.Abs(source.transform.eulerAngles.z)>.01f)throw new Exception("Expected an unrotated original white block.");
        Vector3 a=source.transform.TransformPoint(source.sprite.bounds.min),b=source.transform.TransformPoint(source.sprite.bounds.max);return Rect.MinMaxRect(Mathf.Min(a.x,b.x),Mathf.Min(a.y,b.y),Mathf.Max(a.x,b.x),Mathf.Max(a.y,b.y));
    }
    [MenuItem("地图/Sci-fi Lab/右侧平台改为倒直角梯形")]
    public static void AdjustPlatform()
    {
        Scene scene=SceneManager.GetActiveScene();if(EditorApplication.isPlayingOrWillChangePlaymode||scene.path!="Assets/Scenes/Levels/Mapx11.unity")throw new Exception("Open Mapx11 in Edit mode first.");
        var root=scene.GetRootGameObjects().Single(g=>g.name=="GroundMap");var platform=root.transform.Find("Ground (2)");var source=platform.GetComponent<SpriteRenderer>();var polygon=platform.GetComponent<PolygonCollider2D>();var visual=platform.Find(Visual);if(!polygon||!visual)throw new Exception("Expected the previously dressed right-side platform.");
        Rect b=SourceBounds(source);float edge=root.transform.Find("Ground").GetComponent<Collider2D>().bounds.max.x;
        if(edge<=b.xMin+.1f||edge>=b.xMax-.1f)throw new Exception("The floor edge must fall within the original platform width; no changes made.");
        var player=scene.GetRootGameObjects().Single(g=>g.name=="Player");string playerBefore=EditorJsonUtility.ToJson(player.transform),cameraBefore=Cameras(scene),physicsBefore=PhysicsSnapshot(scene,new[]{platform.gameObject});
        Func<string> fixedArt=()=>string.Join("\n",Enumerable.Range(0,3).Select(i=>File.ReadAllText(Geometry+"/Mapx11_Lab_"+i+".asset")));string fixedBefore=fixedArt();
        var mesh=visual.GetComponent<MeshFilter>().sharedMesh;string meshPath=AssetDatabase.GetAssetPath(mesh);
        if(!EditorSceneManager.SaveScene(scene))throw new IOException("Could not save the live scene for backup.");string stamp=DateTime.Now.ToString("yyyyMMdd_HHmmss"),backup="Logs/LevelArt/Backups/Mapx11_BeforeTrapezoid_"+stamp;Directory.CreateDirectory("Logs/LevelArt/Backups");File.Copy(scene.path,backup+".unity",false);File.Copy(meshPath,backup+"_PlatformMesh.asset",false);File.Copy(meshPath+".meta",backup+"_PlatformMesh.asset.meta",false);
        var atlas=AssetDatabase.LoadAssetAtPath<Texture2D>(AtlasPath);var builder=new Builder{owner=platform,bounds=b,wedge=true,atlas=atlas,bottomRightX=edge};
        for(float x=Mathf.Floor(b.xMin/Unit)*Unit;x<b.xMax;x+=Unit)for(float y=Mathf.Floor(b.yMin/Unit)*Unit;y<b.yMax;y+=Unit){int ix=Mathf.RoundToInt(x/Unit),iy=Mathf.RoundToInt(y/Unit);builder.Quad(builder.UV(1+Mathf.Abs(ix+iy)%2,1+Mathf.Abs(ix*3+iy)%2),new Rect(x,y,Unit,Unit));}
        float step=Unit/8;
        for(float x=b.xMin;x<b.xMax;x+=step){float w=Mathf.Min(step,b.xMax-x),phase=Mathf.Repeat(x-b.xMin,Unit);builder.Quad(new Rect((32+phase/Unit*32)/atlas.width,1-32f/atlas.height,w/Unit*32/atlas.width,32f/atlas.height),new Rect(x,b.yMax-Unit,w,Unit));}
        for(float y=b.yMin;y<b.yMax;y+=step){float h=Mathf.Min(step,b.yMax-y),phase=Mathf.Repeat(y-b.yMin,Unit);builder.Quad(new Rect(0,(atlas.height-96+phase/Unit*32)/atlas.height,32f/atlas.width,h/Unit*32/atlas.height),new Rect(b.xMin,y,Unit,h));}
        builder.DiagonalTrim();
        Undo.IncrementCurrentGroup();int group=Undo.GetCurrentGroup();Undo.SetCurrentGroupName("右侧平台改为倒直角梯形");
        try
        {
            Undo.RecordObjects(new Object[]{mesh,polygon},"Change platform outline and collision");mesh.Clear();mesh.SetVertices(builder.vertices);mesh.SetUVs(0,builder.uv);mesh.SetTriangles(builder.indices,0);mesh.RecalculateBounds();EditorUtility.SetDirty(mesh);var filter=visual.GetComponent<MeshFilter>();filter.sharedMesh=null;filter.sharedMesh=mesh;
            polygon.points=new[]{new Vector2(b.xMin,b.yMin),new Vector2(edge,b.yMin),new Vector2(b.xMax,b.yMax),new Vector2(b.xMin,b.yMax)}.Select(p=>(Vector2)platform.InverseTransformPoint(new Vector3(p.x,p.y,platform.position.z))).ToArray();Physics2D.SyncTransforms();
            var report=JsonUtility.FromJson<Report>(File.ReadAllText(Out+"/applied-report.json"));report.backup=backup+".unity";report.platformMeshBackup=backup+"_PlatformMesh.asset";report.platformShape="inverted right trapezoid";report.slopeStartX=edge;report.platformRightEdge=b.xMax;report.platformTop=b.yMax;report.platformWidth=b.width;report.bottomFlatWidth=edge-b.xMin;report.standingTest=false;
            report.flatPlatformTop=true;for(int i=0;i<5;i++){float x=Mathf.Lerp(b.xMin+.2f,b.xMax-.2f,i/4f);var hit=Physics2D.Raycast(new Vector2(x,b.yMax+1),Vector2.down,1.1f,1<<platform.gameObject.layer);if(hit.collider!=polygon||Mathf.Abs(hit.point.y-b.yMax)>.01f)report.flatPlatformTop=false;}
            report.triangleVoidClear=!polygon.OverlapPoint(new Vector2(Mathf.Lerp(edge,b.xMax,.8f),Mathf.Lerp(b.yMin,b.yMax,.2f)));
            bool flatBasePresent=polygon.OverlapPoint(new Vector2(Mathf.Lerp(b.xMin,edge,.5f),b.yMin+.02f));
            report.geometryClipped=builder.vertices.All(v=>{Vector3 p=platform.TransformPoint(v);return p.x>=b.xMin-.001f&&p.x<=b.xMax+.001f&&p.y>=b.yMin-.001f&&p.y<=b.yMax+.001f&&(p.x<=edge||p.y>=b.yMin+(p.x-edge)/(b.xMax-edge)*b.height-.001f);});
            var beyond=Physics2D.Raycast(new Vector2(b.xMax+.05f,b.yMax+1),Vector2.down,1.1f,1<<platform.gameObject.layer);report.noForwardExtension=beyond.collider!=polygon&&Mathf.Abs(mesh.vertices.Max(v=>platform.TransformPoint(v).x)-b.xMax)<.001f;
            report.playerUnchanged=playerBefore==EditorJsonUtility.ToJson(player.transform);report.camerasUnchanged=cameraBefore==Cameras(scene);report.otherPhysicsUnchanged=physicsBefore==PhysicsSnapshot(scene,new[]{platform.gameObject});report.otherGeometryUnchanged=fixedBefore==fixedArt();
            report.gapToNextPlatform=root.transform.Find("Ground (3)").GetComponent<Collider2D>().bounds.min.x-b.xMax;
            var filters=Names.Select(n=>root.transform.Find(n).Find(Visual).GetComponent<MeshFilter>());report.meshVertices=filters.Sum(f=>f.sharedMesh.vertexCount);report.triangleCount=filters.Sum(f=>f.sharedMesh.triangles.Length/3);
            if(!report.flatPlatformTop||!report.triangleVoidClear||!flatBasePresent||!report.geometryClipped||!report.noForwardExtension||!report.playerUnchanged||!report.camerasUnchanged||!report.otherPhysicsUnchanged||!report.otherGeometryUnchanged)throw new Exception("Trapezoid validation failed: "+JsonUtility.ToJson(report));
            AssetDatabase.SaveAssetIfDirty(mesh);EditorSceneManager.MarkSceneDirty(scene);report.saved=EditorSceneManager.SaveScene(scene);if(!report.saved)throw new IOException("Could not save the adjusted scene.");Undo.CollapseUndoOperations(group);string json=JsonUtility.ToJson(report,true);File.WriteAllText(Out+"/applied-report.json",json);File.WriteAllText("Logs/LevelArt/Mapx11_SciFiLab_Report.json",json);SceneView.RepaintAll();EditorApplication.QueuePlayerLoopUpdate();SessionState.SetBool(QAKey,true);EditorApplication.delayCall+=EditorApplication.EnterPlaymode;
        }
        catch{Undo.RevertAllDownToGroup(group);AssetDatabase.SaveAssetIfDirty(mesh);EditorSceneManager.SaveScene(scene);throw;}
    }
    [MenuItem("地图/Sci-fi Lab/查看第一小段")]
    static void FrameSection()
    {
        if(SceneManager.GetActiveScene().path!="Assets/Scenes/Levels/Mapx11.unity")return;var root=SceneManager.GetActiveScene().GetRootGameObjects().Single(g=>g.name=="GroundMap");var renderers=Names.Select(n=>root.transform.Find(n).Find(Visual).GetComponent<Renderer>()).ToArray();Bounds b=renderers[0].bounds;foreach(var r in renderers.Skip(1))b.Encapsulate(r.bounds);Selection.activeGameObject=root.transform.Find("Ground (2)").gameObject;if(SceneView.lastActiveSceneView)SceneView.lastActiveSceneView.Frame(b,true);SceneView.RepaintAll();
    }
    static bool StandTest(PolygonCollider2D source,Rect platform,BoxCollider2D originalBody)
    {
        Scene test=SceneManager.CreateScene("SciFiLab_StandingQA",new CreateSceneParameters(LocalPhysicsMode.Physics2D));
        try
        {
            var ground=new GameObject("QAPlatform",typeof(PolygonCollider2D));SceneManager.MoveGameObjectToScene(ground,test);ground.GetComponent<PolygonCollider2D>().points=source.points.Select(p=>(Vector2)source.transform.TransformPoint(p)).ToArray();
            float width=originalBody.size.x*Mathf.Abs(originalBody.transform.lossyScale.x),height=originalBody.size.y*Mathf.Abs(originalBody.transform.lossyScale.y);
            var body=new GameObject("QAPlayer",typeof(Rigidbody2D),typeof(BoxCollider2D));SceneManager.MoveGameObjectToScene(body,test);var rb=body.GetComponent<Rigidbody2D>();rb.gravityScale=5;rb.constraints=RigidbodyConstraints2D.FreezeRotation;body.GetComponent<BoxCollider2D>().size=new Vector2(width,height);
            var physics=test.GetPhysicsScene2D();
            for(int i=0;i<3;i++){float x=Mathf.Lerp(platform.xMin+width,platform.xMax-width,i/2f);rb.position=new Vector2(x,platform.yMax+height/2+1);rb.linearVelocity=Vector2.zero;rb.WakeUp();for(int n=0;n<100;n++)physics.Simulate(.02f);if(Mathf.Abs(rb.position.y-height/2-platform.yMax)>.035f||Mathf.Abs(rb.linearVelocity.y)>.05f)return false;}
            return true;
        }
        finally{SceneManager.UnloadSceneAsync(test);}
    }
}
