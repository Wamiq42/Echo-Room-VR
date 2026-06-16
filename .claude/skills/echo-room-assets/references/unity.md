# Unity side — URP materials via the `ai-game-developer` MCP

Project is **URP**; the Lit shader is `Universal Render Pipeline/Lit`. The FBX only brings Diffuse + Normal, so
metallic/roughness are set here. Do it with one C# pass through `script-execute` (full-code mode: a class with a
static method that returns a string for the report).

## Why a script (not the material tools)

The model imports with `materialImportMode` possibly = None (no material sub-assets to remap by name). The robust
recipe: force `ImportStandard`, read the real slot names off the renderers, create proper URP materials, then
`ModelImporter.AddRemap` each slot → `SaveAndReimport`. The remap lives in the `.fbx.meta`, so it **survives
re-exporting/overwriting the FBX** (as long as slot names stay the same).

## The working script (adapt paths/IDs)

```csharp
using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

public class PropMat {
    static Texture2D T(string p){ return AssetDatabase.LoadAssetAtPath<Texture2D>(p); }
    static void SetNormal(string p){               // normals MUST be NormalMap type
        var ti = AssetImporter.GetAtPath(p) as TextureImporter;
        if (ti != null && ti.textureType != TextureImporterType.NormalMap){
            ti.textureType = TextureImporterType.NormalMap; ti.SaveAndReimport();
        }
    }
    public static string Run(){
        string fbx = "Assets/_EchoRoom/Models/Lever.fbx";
        string TL  = "Assets/_EchoRoom/Textures/Lever/";
        string ADG = "Assets/ADG_Textures/walls_vol1/wall03/";
        SetNormal(TL+"rusty_metal_03_nor_gl.jpg");
        SetNormal(TL+"weathered_planks_nor_gl.jpg");
        SetNormal(ADG+"wall03_Normal.tga");

        string dir = "Assets/_EchoRoom/Materials/Lever";
        if(!AssetDatabase.IsValidFolder("Assets/_EchoRoom/Materials")) AssetDatabase.CreateFolder("Assets/_EchoRoom","Materials");
        if(!AssetDatabase.IsValidFolder(dir)) AssetDatabase.CreateFolder("Assets/_EchoRoom/Materials","Lever");
        var lit = Shader.Find("Universal Render Pipeline/Lit");

        Material Mk(string name, Texture2D alb, Texture2D nrm, float metal, float smooth, float tile, Color? bc){
            string path = dir+"/"+name+".mat";
            var m = AssetDatabase.LoadAssetAtPath<Material>(path);
            if(m==null){ m = new Material(lit); AssetDatabase.CreateAsset(m, path); }
            m.shader = lit;
            if(alb!=null) m.SetTexture("_BaseMap", alb);
            m.SetColor("_BaseColor", bc ?? Color.white);
            if(nrm!=null){ m.SetTexture("_BumpMap", nrm); m.EnableKeyword("_NORMALMAP"); m.SetFloat("_BumpScale",1f); }
            m.SetFloat("_Metallic", metal); m.SetFloat("_Smoothness", smooth);
            m.SetTextureScale("_BaseMap", new Vector2(tile,tile));   // matches the Blender UV tiling
            EditorUtility.SetDirty(m); return m;
        }
        var iron = Mk("Lever_Iron",  T(TL+"rusty_metal_03_Diffuse.jpg"),  T(TL+"rusty_metal_03_nor_gl.jpg"),  1f,0.45f,3f,null);
        var brass= Mk("Lever_Brass", T(TL+"rusty_metal_03_Diffuse.jpg"),  T(TL+"rusty_metal_03_nor_gl.jpg"),  1f,0.6f, 4f, new Color(1f,0.82f,0.4f));
        var wood = Mk("Lever_Wood",  T(TL+"weathered_planks_Diffuse.jpg"),T(TL+"weathered_planks_nor_gl.jpg"),0f,0.35f,1.8f,null);
        var grip = Mk("Lever_Grip",  T(TL+"weathered_planks_Diffuse.jpg"),T(TL+"weathered_planks_nor_gl.jpg"),0f,0.3f, 3f, new Color(0.45f,0.36f,0.28f));
        var stone= Mk("Lever_Stone", T(ADG+"wall03_Diffuse.tga"),         T(ADG+"wall03_Normal.tga"),         0f,0.25f,1.6f,null);
        var red  = Mk("Lever_Red",   null, null, 0.2f,0.45f,1f, new Color(0.42f,0.07f,0.05f));

        var imp = AssetImporter.GetAtPath(fbx) as ModelImporter;
        imp.materialImportMode = ModelImporterMaterialImportMode.ImportStandard;   // so slots/identifiers exist
        imp.SaveAndReimport();

        var names = new HashSet<string>();
        var root = AssetDatabase.LoadAssetAtPath<GameObject>(fbx);
        foreach(var r in root.GetComponentsInChildren<MeshRenderer>(true))
            foreach(var sm in r.sharedMaterials) if(sm!=null) names.Add(sm.name);
        foreach(var o in AssetDatabase.LoadAllAssetsAtPath(fbx)){ var m=o as Material; if(m!=null) names.Add(m.name); }

        var remapped = new List<string>();
        foreach(var nm in names){
            string k=nm.ToLower(); Material c=null;
            if(k.Contains("iron"))c=iron; else if(k.Contains("brass"))c=brass; else if(k.Contains("wood"))c=wood;
            else if(k.Contains("grip"))c=grip; else if(k.Contains("stone"))c=stone; else if(k.Contains("red"))c=red;
            if(c!=null){ imp.AddRemap(new AssetImporter.SourceAssetIdentifier(typeof(Material), nm), c); remapped.Add(nm); }
        }
        imp.SaveAndReimport(); AssetDatabase.SaveAssets(); AssetDatabase.Refresh();
        return "remapped: "+string.Join(", ", remapped);
    }
}
```

## Verify

Read back the renderer materials (base/normal/metallic) to confirm:

```csharp
foreach(var r in root.GetComponentsInChildren<MeshRenderer>(true))
  foreach(var m in r.sharedMaterials)
    Debug.Log(m.name+": base="+m.GetTexture("_BaseMap")+" metal="+m.GetFloat("_Metallic"));
```

For a visual check, **instantiate** the FBX into the scene first (a bare-asset `screenshot-isolated` is blank),
then `screenshot-isolated` by the instance name, then delete the temp instance.

## Gotchas

- **Normal textures need `NormalMap` import type** (the `SetNormal` step) or they render as flat color.
- **`screenshot-isolated` on the FBX asset → blank.** Instantiate (`PrefabUtility.InstantiatePrefab`) into the
  active scene, screenshot by name, clean up.
- **Play Mode blocks scene edits** — `MarkSceneDirty`/instantiate-into-scene throws "cannot be used during play
  mode". Need Edit Mode (`editor-application-set-state` to Stop, or ask the user).
- After overwriting the FBX from Blender, just `assets-refresh` (ForceUpdate) — the material remap persists.
