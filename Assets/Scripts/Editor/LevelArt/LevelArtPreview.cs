using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

// Read-only preview compositor. No scene objects or assets are changed or saved.
[InitializeOnLoad]
public static class LevelArtPreview
{
    const string Root = "C:/Users/chenk/Documents/Codex/2026-11-23/ban-2/outputs/mapx11-scifi-lab-preview";
    const string Art = "C:/Users/chenk/Documents/Codex/2026-11-23/ban-2/work/SciFiLab-extracted/Foozle_2DT0001_Science_Fiction_Labs_Tileset";
    static double next;
    static Material material;
    static readonly List<Texture2D> loaded = new List<Texture2D>();
    [Serializable] public class Item { public string name, parent; public Vector3 position, scale; public Bounds bounds; public bool enabled; public string tag; }
    [Serializable] public class Info { public string scene, selected; public bool dirty; public Vector3 playerPosition, playerScale; public Bounds playerBounds; public float cameraSize; public Item[] grounds, playerSprites; }
    [Serializable] public class Request { public string mode="inspect", output="preview"; public string[] blocks; public float xMin,yMin,xMax,yMax,unit=1; public int width=1400,height=800; public bool plain; }
    static LevelArtPreview() { EditorApplication.update += Tick; }
    static void Tick()
    {
        if (EditorApplication.timeSinceStartup < next || EditorApplication.isCompiling || EditorApplication.isUpdating || EditorApplication.isPlayingOrWillChangePlaymode) return;
        next=EditorApplication.timeSinceStartup+1;
        string path=Root+"/request.json";
        if(!File.Exists(path)) return;
        try
        {
            Request request=JsonUtility.FromJson<Request>(File.ReadAllText(path));
            File.Delete(path);
            Directory.CreateDirectory(Root);
            if(request.mode=="inspect") Inspect(); else Render(request);
        }
        catch(Exception e) { File.WriteAllText(Root+"/error.txt",e.ToString()); Debug.LogException(e); }
        finally { if(material) UnityEngine.Object.DestroyImmediate(material); material=null; foreach(var t in loaded) if(t) UnityEngine.Object.DestroyImmediate(t); loaded.Clear(); }
    }
    static SpriteRenderer[] All()
    {
        return SceneManager.GetActiveScene().GetRootGameObjects().SelectMany(g=>g.GetComponentsInChildren<SpriteRenderer>(true)).ToArray();
    }
    static Item Describe(SpriteRenderer s) { return new Item {name=s.name,parent=s.transform.parent?s.transform.parent.name:"",position=s.transform.position,scale=s.transform.lossyScale,bounds=s.bounds,enabled=s.enabled&&s.gameObject.activeInHierarchy,tag=s.tag}; }
    static SpriteRenderer[] PlayerSprites() { var p=GameObject.Find("Player"); return p?p.GetComponentsInChildren<SpriteRenderer>().Where(s=>s.enabled && s.sprite && s.gameObject.activeInHierarchy).ToArray():new SpriteRenderer[0]; }
    static void Inspect()
    {
        var p=GameObject.Find("Player"); var sprites=PlayerSprites(); var b=new Bounds(p?p.transform.position:Vector3.zero,Vector3.zero);
        if(sprites.Length>0) { b=sprites[0].bounds; foreach(var s in sprites.Skip(1)) b.Encapsulate(s.bounds); }
        var info=new Info { scene=SceneManager.GetActiveScene().path,dirty=SceneManager.GetActiveScene().isDirty,selected=Selection.activeGameObject?Selection.activeGameObject.name:"",playerPosition=p?p.transform.position:Vector3.zero,playerScale=p?p.transform.lossyScale:Vector3.one,playerBounds=b,cameraSize=Camera.main?Camera.main.orthographicSize:5,grounds=All().Where(s=>s.name=="Ground"||s.name.StartsWith("Ground (")).Select(Describe).ToArray(),playerSprites=sprites.Select(Describe).ToArray()};
        File.WriteAllText(Root+"/scene-info.json",JsonUtility.ToJson(info,true));
        var rq=new Request {output="player-current",xMin=b.min.x-.1f,xMax=b.max.x+.1f,yMin=b.min.y-.1f,yMax=b.max.y+.1f,width=256,height=256};
        RenderActor(rq);
        File.WriteAllText(Root+"/inspect.done.txt","Read current scene without saving or changing it.");
    }
    static Texture2D Load(string relative)
    {
        var t=new Texture2D(2,2,TextureFormat.RGBA32,false); t.LoadImage(File.ReadAllBytes(Art+"/"+relative)); t.filterMode=FilterMode.Point;t.wrapMode=TextureWrapMode.Clamp;loaded.Add(t);return t;
    }
    static void Setup() { var s=Shader.Find("Hidden/LevelArt/Preview"); if(!s) throw new Exception("Preview shader unavailable"); material=new Material(s){hideFlags=HideFlags.HideAndDontSave}; }
    static Vector3 Pixel(Vector3 p,Request r) { return new Vector3((p.x-r.xMin)/(r.xMax-r.xMin)*r.width,(r.yMax-p.y)/(r.yMax-r.yMin)*r.height,0); }
    static RenderTexture Begin(Request r,Color background,out RenderTexture previous)
    {
        Setup(); previous=RenderTexture.active; var rt=RenderTexture.GetTemporary(r.width,r.height,0,RenderTextureFormat.ARGB32,RenderTextureReadWrite.sRGB); RenderTexture.active=rt;
        GL.Clear(true,true,background); GL.PushMatrix();GL.LoadPixelMatrix(0,r.width,r.height,0);return rt;
    }
    static void End(RenderTexture rt,RenderTexture previous,Request r)
    {
        GL.PopMatrix();var png=new Texture2D(r.width,r.height,TextureFormat.RGBA32,false);png.ReadPixels(new Rect(0,0,r.width,r.height),0,0);png.Apply();File.WriteAllBytes(Root+"/"+r.output+".png",png.EncodeToPNG());UnityEngine.Object.DestroyImmediate(png);RenderTexture.active=previous;RenderTexture.ReleaseTemporary(rt);
        File.WriteAllText(Root+"/"+r.output+".done.txt","Preview only. Scene remains "+(SceneManager.GetActiveScene().isDirty?"unsaved":"saved")+"; no scene writes.");
    }
    static void Sprite(SpriteRenderer sr,Request r)
    {
        var s=sr.sprite; var vertices=s.vertices;var uv=s.uv;var triangles=s.triangles;material.SetTexture("_MainTex",s.texture); material.SetPass(0);GL.Begin(GL.TRIANGLES);GL.Color(sr.color);
        foreach(var i in triangles) { Vector3 p=vertices[i];if(sr.flipX)p.x=-p.x;if(sr.flipY)p.y=-p.y;GL.TexCoord(uv[i]);GL.Vertex(Pixel(sr.transform.TransformPoint(p),r)); } GL.End();
    }
    static void Actor(Request r) { foreach(var s in PlayerSprites().OrderBy(s=>s.sortingOrder)) Sprite(s,r); }
    static void AppliedMesh(SpriteRenderer source,Request r)
    {
        var visual=source.transform.Find("SciFiLab_Visual");if(!visual)throw new Exception("Expected applied surface on "+source.name);var filter=visual.GetComponent<MeshFilter>();var mesh=filter.sharedMesh;var v=mesh.vertices;var uv=mesh.uv;material.SetTexture("_MainTex",visual.GetComponent<MeshRenderer>().sharedMaterial.mainTexture);material.SetPass(0);GL.Begin(GL.TRIANGLES);GL.Color(Color.white);foreach(var i in mesh.triangles){GL.TexCoord(uv[i]);GL.Vertex(Pixel(filter.transform.TransformPoint(v[i]),r));}GL.End();
    }
    static void RenderActor(Request r) { var rt=Begin(r,Color.clear,out var previous);Actor(r);End(rt,previous,r); }
    static void Quad(Texture texture,Rect source,Rect target,Color color,Request r,Rect? clip=null)
    {
        Rect draw=target;
        if(clip.HasValue) {var c=clip.Value;draw=Rect.MinMaxRect(Mathf.Max(target.xMin,c.xMin),Mathf.Max(target.yMin,c.yMin),Mathf.Min(target.xMax,c.xMax),Mathf.Min(target.yMax,c.yMax));if(draw.width<=0||draw.height<=0)return;}
        float u0=source.xMin+(draw.xMin-target.xMin)/target.width*source.width,u1=source.xMin+(draw.xMax-target.xMin)/target.width*source.width;
        float v0=source.yMin+(draw.yMin-target.yMin)/target.height*source.height,v1=source.yMin+(draw.yMax-target.yMin)/target.height*source.height;
        material.SetTexture("_MainTex",texture);material.SetPass(0);GL.Begin(GL.QUADS);GL.Color(color);
        GL.TexCoord2(u0,v0);GL.Vertex(Pixel(new Vector3(draw.xMin,draw.yMin),r)); GL.TexCoord2(u0,v1);GL.Vertex(Pixel(new Vector3(draw.xMin,draw.yMax),r));GL.TexCoord2(u1,v1);GL.Vertex(Pixel(new Vector3(draw.xMax,draw.yMax),r));GL.TexCoord2(u1,v0);GL.Vertex(Pixel(new Vector3(draw.xMax,draw.yMin),r));GL.End();
    }
    static Rect UV(Texture t,int col,int row,int w=32,int h=32) { return new Rect(col*32f/t.width,1-(row*32f+h)/t.height,w/(float)t.width,h/(float)t.height); }
    static Rect BoundsRect(Bounds b) {return Rect.MinMaxRect(b.min.x,b.min.y,b.max.x,b.max.y);}
    static bool Inside(IEnumerable<Rect> blocks,Vector2 p) {return blocks.Any(b=>b.Contains(p));}
    static void PanelBank(Texture tile, Rect b,float x,float y,int columns,int rows,Request r)
    {
        // Metal equipment bays are surface art, entirely inside the solid white area.
        for(int iy=0;iy<rows;iy++) for(int ix=0;ix<columns;ix++)
        {
            int col=8,row=3;
            if(iy==rows-1 && (ix==1||ix==columns-2)) { col=6;row=4; }
            else if(ix==0 && iy<rows-1) { col=7;row=3; }
            Quad(tile,UV(tile,col,row),new Rect(x+ix*r.unit,y+iy*r.unit,r.unit,r.unit),Color.white,r,b);
        }
    }
    static void Render(Request r)
    {
        var scene=SceneManager.GetActiveScene();bool dirty=scene.isDirty;var actor=GameObject.Find("Player");var actorPosition=actor.transform.position;var actorScale=actor.transform.localScale;
        var all=All();var selected=all.Where(s=>r.blocks!=null && r.blocks.Contains(s.name)&&(s.enabled||r.mode=="applied")&&s.gameObject.activeInHierarchy).ToArray();
        if(selected.Length!=r.blocks.Length)throw new Exception("Expected exactly "+r.blocks.Length+" selected terrain blocks, got "+selected.Length);
        var blocks=selected.Select(s=>BoundsRect(s.bounds)).ToArray();var tile=Load("Tileset/level_tileset.png");var decor=Load("Decor/full decor tiles.png");
        var rt=Begin(r,((Color)new Color32(68,68,68,255)).linear,out var previous);
        foreach(var s in all.Where(s=>s.enabled&&s.sprite&&s.gameObject.activeInHierarchy&&(s.name=="Ground"||s.name.StartsWith("Ground ("))))
        {
            if(!selected.Contains(s) || r.plain) Sprite(s,r);
        }
        if(r.mode=="applied") {foreach(var s in selected)AppliedMesh(s,r);}
        else if(!r.plain)
        {
            foreach(var b in blocks)
            {
                // Sparse dark structural fill. All drawing is clipped to the original white rectangle.
                for(float x=Mathf.Floor(b.xMin/r.unit)*r.unit;x<b.xMax;x+=r.unit)
                for(float y=Mathf.Floor(b.yMin/r.unit)*r.unit;y<b.yMax;y+=r.unit)
                {
                    int ix=Mathf.RoundToInt(x/r.unit),iy=Mathf.RoundToInt(y/r.unit);int col=1+Mathf.Abs(ix+iy)%2,row=1+Mathf.Abs(ix*3+iy)%2;
                    Quad(tile,UV(tile,col,row),new Rect(x,y,r.unit,r.unit),Color.white,r,b);
                }
                // Edge strips sit inside the original collision bounds. Covered seams have no border.
                float step=r.unit/8;
                for(float x=b.xMin;x<b.xMax;x+=step)
                {
                    float w=Mathf.Min(step,b.xMax-x),mid=x+w/2;float phase=Mathf.Repeat(x-b.xMin,r.unit);
                    if(!Inside(blocks,new Vector2(mid,b.yMax+.005f))) Quad(tile,new Rect((32+phase/r.unit*32)/tile.width,1-32f/tile.height,w/r.unit*32/tile.width,32f/tile.height),new Rect(x,b.yMax-r.unit,w,r.unit),Color.white,r,b);
                    if(!Inside(blocks,new Vector2(mid,b.yMin-.005f))) Quad(tile,new Rect((32+phase/r.unit*32)/tile.width,1-128f/tile.height,w/r.unit*32/tile.width,32f/tile.height),new Rect(x,b.yMin,w,r.unit),Color.white,r,b);
                }
                for(float y=b.yMin;y<b.yMax;y+=step)
                {
                    float h=Mathf.Min(step,b.yMax-y),mid=y+h/2,phase=Mathf.Repeat(y-b.yMin,r.unit);
                    if(!Inside(blocks,new Vector2(b.xMin-.005f,mid))) Quad(tile,new Rect(0,(tile.height-96+phase/r.unit*32)/tile.height,32f/tile.width,h/r.unit*32/tile.height),new Rect(b.xMin,y,r.unit,h),Color.white,r,b);
                    if(!Inside(blocks,new Vector2(b.xMax+.005f,mid))) Quad(tile,new Rect(96f/tile.width,(tile.height-96+phase/r.unit*32)/tile.height,32f/tile.width,h/r.unit*32/tile.height),new Rect(b.xMax-r.unit,y,r.unit,h),Color.white,r,b);
                }
            }
            for(int i=0;i<selected.Length;i++)
            {
                Rect b=blocks[i];
                if(selected[i].name=="Ground")
                {
                    PanelBank(tile,b,0,b.yMax-r.unit*3.4f,5,2,r);
                    PanelBank(tile,b,13,b.yMax-r.unit*3.4f,5,2,r);
                }
                else if(selected[i].name=="Ground (1)") PanelBank(tile,b,b.xMin+r.unit*.8f,b.yMin+r.unit*2.2f,4,3,r);
                else if(selected[i].name=="Ground (4)") PanelBank(tile,b,b.xMin+r.unit*1.1f,b.yMin+r.unit*9f,3,4,r);
            }
        }
        Actor(r);End(rt,previous,r);
        if(scene.isDirty!=dirty||actor.transform.position!=actorPosition||actor.transform.localScale!=actorScale)throw new Exception("Read-only invariant failed");
    }
}
