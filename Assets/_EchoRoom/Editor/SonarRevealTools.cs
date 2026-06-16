using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEngine;

/// <summary>
/// Sonar Reveal Converter window.
///
/// Drop FBX models or prefabs in. For each one it finds every material, AUTO-DETECTS the
/// best reveal quality from that material's maps (normal map -> Normals; metallic/AO -> PBR;
/// base only -> Flat), switches it to EchoRoom/EchoSonarReveal carrying its maps over, and
/// darkens the base so it reads as a dark room. FBX-embedded materials (read-only) are
/// extracted to a sibling "Materials" folder first so they can be edited.
///
/// Open via: Tools > Sonar > Reveal Converter.
/// </summary>
public class SonarRevealTools : EditorWindow
{
    private const string ShaderName = "EchoRoom/EchoSonarReveal";

    private readonly List<GameObject> _targets = new List<GameObject>();
    private Color _darkBase = new Color(0.04f, 0.045f, 0.05f, 1f);
    private bool _autoQuality = true;
    private int _forcedQuality = 1;
    private Vector2 _scroll;
    private string _log = "";

    [MenuItem("Tools/Sonar/Reveal Converter")]
    private static void Open()
    {
        var w = GetWindow<SonarRevealTools>("Sonar Reveal");
        w.minSize = new Vector2(360, 360);
    }

    private void OnGUI()
    {
        EditorGUILayout.LabelField("Sonar Reveal Converter", EditorStyles.boldLabel);
        EditorGUILayout.HelpBox(
            "Drop FBX models or prefabs below. The tool finds every material, auto-detects the " +
            "best reveal quality (Flat / Normals / PBR) from each material's maps, converts them " +
            "to the sonar reveal shader, and darkens the base. FBX-embedded materials are " +
            "extracted automatically so they can be edited.", MessageType.Info);

        DrawDropArea();
        DrawTargetList();

        EditorGUILayout.Space();
        _autoQuality = EditorGUILayout.ToggleLeft("Auto-detect quality (recommended)", _autoQuality);
        using (new EditorGUI.DisabledScope(_autoQuality))
            _forcedQuality = EditorGUILayout.Popup("Force quality", _forcedQuality, new[] { "Flat", "Normals", "PBR" });
        _darkBase = EditorGUILayout.ColorField("Dark Base Color", _darkBase);

        EditorGUILayout.Space();
        using (new EditorGUI.DisabledScope(_targets.Count == 0))
        {
            if (GUILayout.Button("Convert", GUILayout.Height(30)))
                ConvertAll();
        }

        if (!string.IsNullOrEmpty(_log))
        {
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Results", EditorStyles.boldLabel);
            _scroll = EditorGUILayout.BeginScrollView(_scroll, GUILayout.MinHeight(140));
            EditorGUILayout.TextArea(_log, GUILayout.ExpandHeight(true));
            EditorGUILayout.EndScrollView();
        }
    }

    private void DrawDropArea()
    {
        var rect = GUILayoutUtility.GetRect(0, 52, GUILayout.ExpandWidth(true));
        GUI.Box(rect, "Drop FBX / Prefab(s) here", EditorStyles.helpBox);

        var e = Event.current;
        if ((e.type == EventType.DragUpdated || e.type == EventType.DragPerform) && rect.Contains(e.mousePosition))
        {
            DragAndDrop.visualMode = DragAndDropVisualMode.Copy;
            if (e.type == EventType.DragPerform)
            {
                DragAndDrop.AcceptDrag();
                foreach (var o in DragAndDrop.objectReferences)
                {
                    var go = o as GameObject;
                    if (go != null && !_targets.Contains(go)) _targets.Add(go);
                }
                e.Use();
            }
        }
    }

    private void DrawTargetList()
    {
        if (_targets.Count == 0) return;
        for (int i = 0; i < _targets.Count; i++)
        {
            EditorGUILayout.BeginHorizontal();
            _targets[i] = (GameObject)EditorGUILayout.ObjectField(_targets[i], typeof(GameObject), false);
            if (GUILayout.Button("✕", GUILayout.Width(24))) { _targets.RemoveAt(i); i--; }
            EditorGUILayout.EndHorizontal();
        }
        if (GUILayout.Button("Clear list")) _targets.Clear();
    }

    private void ConvertAll()
    {
        var shader = Shader.Find(ShaderName);
        if (shader == null)
        {
            _log = "ERROR: shader '" + ShaderName + "' not found.";
            return;
        }

        var log = new StringBuilder();
        int totalMats = 0;
        var processed = new HashSet<Material>();

        foreach (var t in _targets)
        {
            if (t == null) continue;
            string path = AssetDatabase.GetAssetPath(t);
            if (string.IsNullOrEmpty(path)) { log.AppendLine("• Skipped (not an asset): " + t.name); continue; }
            log.AppendLine("▶ " + Path.GetFileName(path));

            // Extract FBX-embedded materials so they become editable.
            var importer = AssetImporter.GetAtPath(path) as ModelImporter;
            if (importer != null && importer.materialLocation == ModelImporterMaterialLocation.InPrefab)
            {
                int extracted = ExtractEmbeddedMaterials(path, log);
                if (extracted > 0) log.AppendLine($"   extracted {extracted} embedded material(s)");
            }

            // Reload (after any reimport) and gather the materials it actually uses.
            var go = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (go == null) { log.AppendLine("   ! could not load as GameObject"); continue; }

            var mats = new HashSet<Material>();
            foreach (var r in go.GetComponentsInChildren<Renderer>(true))
                foreach (var m in r.sharedMaterials)
                    if (m != null) mats.Add(m);

            if (mats.Count == 0) { log.AppendLine("   (no materials found)"); continue; }

            foreach (var m in mats)
            {
                if (!processed.Add(m)) continue;   // shared across targets — convert once
                int q = _autoQuality ? DecideQuality(m) : _forcedQuality;
                string reason = _autoQuality ? DecideReason(q) : "forced";
                ConvertMaterial(m, q, shader);
                log.AppendLine($"   ✓ {m.name} → {QualityName(q)}  ({reason})");
                totalMats++;
            }
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        log.AppendLine($"\nDone. Converted {totalMats} material(s).");
        _log = log.ToString();
        Debug.Log("[Sonar] Reveal Converter:\n" + _log);
    }

    // ---- intelligence: pick a quality from the material's maps ----
    private static int DecideQuality(Material m)
    {
        bool hasMetallicGloss = HasTex(m, "_MetallicGlossMap");
        bool hasAO = HasTex(m, "_OcclusionMap");
        bool metallic = m.HasProperty("_Metallic") && m.GetFloat("_Metallic") > 0.01f;
        bool hasNormal = HasTex(m, "_BumpMap");

        if (hasMetallicGloss || hasAO || metallic) return 2; // PBR
        if (hasNormal) return 1;                             // Normals
        return 0;                                            // Flat
    }

    private static string DecideReason(int q) =>
        q == 2 ? "has metallic / AO maps" : q == 1 ? "has a normal map" : "base texture only";

    private static string QualityName(int q) => q == 2 ? "PBR" : q == 1 ? "Normals" : "Flat";

    // ---- convert one material, carrying over its maps ----
    private void ConvertMaterial(Material m, int quality, Shader shader)
    {
        Undo.RecordObject(m, "Sonar Reveal Convert");

        Texture baseTex = GetTex(m, "_BaseMap", "_MainTex");
        Vector2 tiling = GetScale(m, "_BaseMap", "_MainTex");
        Vector2 offset = GetOffset(m, "_BaseMap", "_MainTex");
        Texture normalTex = GetTex(m, "_BumpMap");
        float bumpScale = GetFloat(m, 1f, "_BumpScale");
        Texture mgTex = GetTex(m, "_MetallicGlossMap");
        float metallic = GetFloat(m, 0f, "_Metallic");
        float smoothness = GetFloat(m, 0.5f, "_Smoothness", "_Glossiness");
        Texture aoTex = GetTex(m, "_OcclusionMap");
        float aoStrength = GetFloat(m, 1f, "_OcclusionStrength");

        m.shader = shader;

        if (baseTex != null) m.SetTexture("_BaseMap", baseTex);
        m.SetTextureScale("_BaseMap", tiling);
        m.SetTextureOffset("_BaseMap", offset);
        m.SetColor("_BaseColor", _darkBase);
        if (normalTex != null) m.SetTexture("_BumpMap", normalTex);
        m.SetFloat("_BumpScale", bumpScale);
        if (mgTex != null) m.SetTexture("_MetallicGlossMap", mgTex);
        m.SetFloat("_Metallic", metallic);
        m.SetFloat("_Smoothness", smoothness);
        if (aoTex != null) m.SetTexture("_OcclusionMap", aoTex);
        m.SetFloat("_OcclusionStrength", aoStrength);
        m.SetFloat("_RevealQuality", quality);

        EditorUtility.SetDirty(m);
    }

    private static int ExtractEmbeddedMaterials(string modelPath, StringBuilder log)
    {
        string dir = Path.GetDirectoryName(modelPath).Replace("\\", "/");
        string matFolder = dir + "/Materials";
        if (!AssetDatabase.IsValidFolder(matFolder)) AssetDatabase.CreateFolder(dir, "Materials");

        int count = 0;
        foreach (var obj in AssetDatabase.LoadAllAssetsAtPath(modelPath))
        {
            var em = obj as Material;
            if (em == null) continue;
            string dest = AssetDatabase.GenerateUniqueAssetPath(matFolder + "/" + Sanitize(em.name) + ".mat");
            string err = AssetDatabase.ExtractAsset(em, dest);
            if (string.IsNullOrEmpty(err)) count++;
            else log.AppendLine("   ! extract failed for " + em.name + ": " + err);
        }

        if (count > 0)
        {
            var importer = AssetImporter.GetAtPath(modelPath) as ModelImporter;
            if (importer != null) importer.materialLocation = ModelImporterMaterialLocation.External;
            AssetDatabase.WriteImportSettingsIfDirty(modelPath);
            AssetDatabase.ImportAsset(modelPath, ImportAssetOptions.ForceUpdate);
        }
        return count;
    }

    // ---- small helpers ----
    private static bool HasTex(Material m, string name) => m.HasProperty(name) && m.GetTexture(name) != null;

    private static Texture GetTex(Material m, params string[] names)
    {
        foreach (var n in names)
            if (m.HasProperty(n)) { var t = m.GetTexture(n); if (t != null) return t; }
        return null;
    }

    private static float GetFloat(Material m, float fallback, params string[] names)
    {
        foreach (var n in names)
            if (m.HasProperty(n)) return m.GetFloat(n);
        return fallback;
    }

    private static Vector2 GetScale(Material m, params string[] names)
    {
        foreach (var n in names)
            if (m.HasProperty(n)) return m.GetTextureScale(n);
        return Vector2.one;
    }

    private static Vector2 GetOffset(Material m, params string[] names)
    {
        foreach (var n in names)
            if (m.HasProperty(n)) return m.GetTextureOffset(n);
        return Vector2.zero;
    }

    private static string Sanitize(string s)
    {
        foreach (var c in Path.GetInvalidFileNameChars()) s = s.Replace(c, '_');
        return string.IsNullOrEmpty(s) ? "Material" : s;
    }
}
