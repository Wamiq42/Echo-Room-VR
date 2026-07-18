using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

public sealed class PrefabBakedLightingWindow : EditorWindow
{
    private const string DefaultLevelDataPath = "Assets/_EchoRoom/SObjects/LevelData.asset";
    private const string DefaultOutputFolder = "Assets/_EchoRoom/Lighting/Prefab Lightmaps";

    [SerializeField] private GameObject levelRoot;
    [SerializeField] private LevelData levelData;
    [SerializeField] private string outputFolder = DefaultOutputFolder;

    [MenuItem("Tools/Echo Room/Prefab Baked Lighting")]
    public static void Open()
    {
        GetWindow<PrefabBakedLightingWindow>("Prefab Baked Lighting");
    }

    private void OnEnable()
    {
        if (levelData == null)
            levelData = AssetDatabase.LoadAssetAtPath<LevelData>(DefaultLevelDataPath);

        if (Selection.activeGameObject != null && Selection.activeGameObject.scene.IsValid())
            levelRoot = Selection.activeGameObject;
    }

    private void OnGUI()
    {
        EditorGUILayout.HelpBox(
            "Edit and save the prefab in Prefab Mode. For baking, return to MainScene, enable one puzzle " +
            "level or the configured tutorial instance, disable the other levels, select its root, then use this tool.",
            MessageType.Info);

        levelRoot = (GameObject)EditorGUILayout.ObjectField("Level Root In Scene", levelRoot, typeof(GameObject), true);
        levelData = (LevelData)EditorGUILayout.ObjectField("Level Data", levelData, typeof(LevelData), false);
        outputFolder = EditorGUILayout.TextField("Output Folder", outputFolder);

        using (new EditorGUILayout.HorizontalScope())
        {
            if (GUILayout.Button("Use Current Selection"))
                levelRoot = Selection.activeGameObject;

            if (GUILayout.Button("Open Lighting Window"))
                EditorApplication.ExecuteMenuItem("Window/Rendering/Lighting");
        }

        EditorGUILayout.Space(8f);
        EditorGUILayout.HelpBox(
            "Bake Selected Level & Capture bakes the currently active scene. Make sure only the intended " +
            "level is active before pressing it. Capture Existing Bake only records the current bake.",
            MessageType.Warning);

        using (new EditorGUI.DisabledScope(EditorApplication.isPlaying || Lightmapping.isRunning))
        {
            if (GUILayout.Button("Bake Selected Level & Capture", GUILayout.Height(32f)))
                BakeAndCapture();

            if (GUILayout.Button("Capture Existing Bake", GUILayout.Height(26f)))
                CaptureExistingBake();

            EditorGUILayout.Space(5f);
            if (GUILayout.Button("Clear Temporary Scene Bake"))
                ClearTemporarySceneBake();
        }

        if (Lightmapping.isRunning)
            EditorGUILayout.HelpBox("Unity is currently baking lighting.", MessageType.Info);
    }

    private void BakeAndCapture()
    {
        if (!ValidateBeforeCapture(requireActiveRoot: true, requireExistingBake: false)) return;

        try
        {
            if (!Lightmapping.Bake())
            {
                EditorUtility.DisplayDialog("Lighting Bake Failed",
                    "Unity did not complete the lighting bake. Check the Console for details.", "OK");
                return;
            }

            CaptureExistingBake();
        }
        catch (Exception exception)
        {
            Debug.LogException(exception);
            EditorUtility.DisplayDialog("Lighting Bake Failed", exception.Message, "OK");
        }
    }

    private void CaptureExistingBake()
    {
        if (!ValidateBeforeCapture(requireActiveRoot: false, requireExistingBake: true)) return;

        try
        {
            Capture();
        }
        catch (Exception exception)
        {
            Debug.LogException(exception);
            EditorUtility.DisplayDialog("Lighting Capture Failed", exception.Message, "OK");
        }
    }

    private void ClearTemporarySceneBake()
    {
        if (!EditorUtility.DisplayDialog(
                "Clear Temporary Scene Bake",
                "Clear the current scene's baked data? Captured per-level copies will not be deleted.",
                "Clear",
                "Cancel"))
            return;

        Lightmapping.Clear();
        Debug.Log("[PrefabBakedLighting] Cleared the temporary scene bake. Captured level lighting assets were kept.");
    }

    private bool ValidateBeforeCapture(bool requireActiveRoot, bool requireExistingBake)
    {
        if (EditorApplication.isPlaying)
        {
            EditorUtility.DisplayDialog("Prefab Baked Lighting", "Exit Play Mode before baking or capturing.", "OK");
            return false;
        }

        if (levelRoot == null || !levelRoot.scene.IsValid() || !levelRoot.scene.isLoaded)
        {
            EditorUtility.DisplayDialog("Prefab Baked Lighting",
                "Select the level prefab instance in the loaded scene, not the prefab asset.", "OK");
            return false;
        }

        if (requireActiveRoot && !levelRoot.activeInHierarchy)
        {
            EditorUtility.DisplayDialog("Prefab Baked Lighting",
                "The selected level must be active in the scene before baking.", "OK");
            return false;
        }

        if (levelData == null)
        {
            EditorUtility.DisplayDialog("Prefab Baked Lighting", "Assign the project's LevelData asset.", "OK");
            return false;
        }

        if (string.IsNullOrWhiteSpace(outputFolder) || !outputFolder.StartsWith("Assets/", StringComparison.Ordinal))
        {
            EditorUtility.DisplayDialog("Prefab Baked Lighting",
                "The output folder must be inside Assets/.", "OK");
            return false;
        }

        if (!requireExistingBake)
            return true;

        LightmapData[] maps = LightmapSettings.lightmaps;
        if (maps == null || maps.Length == 0)
        {
            EditorUtility.DisplayDialog("Prefab Baked Lighting",
                "No baked lightmaps are loaded. Bake the active scene first.", "OK");
            return false;
        }

        LightmapsMode mode = LightmapSettings.lightmapsMode;
        if (!Enum.IsDefined(typeof(LightmapsMode), mode))
        {
            EditorUtility.DisplayDialog("Prefab Baked Lighting",
                "The current bake reports an invalid lightmaps mode (" + (int)mode +
                "). Reopen the scene or rebake lighting before capturing.", "OK");
            return false;
        }

        return true;
    }

    private void Capture()
    {
        GameObject prefabSource = PrefabUtility.GetCorrespondingObjectFromOriginalSource(levelRoot);
        if (prefabSource == null)
            prefabSource = PrefabUtility.GetCorrespondingObjectFromSource(levelRoot);

        Level level = FindMatchingLevel(prefabSource);
        GameObject tutorialPrefab = level == null ? FindMatchingTutorialPrefab(prefabSource) : null;
        bool isTutorial = tutorialPrefab != null;

        if (level == null && !isTutorial)
            throw new InvalidOperationException(
                "The selected scene object does not match a puzzle prefab in LevelData or the tutorial prefab " +
                "configured on GameManager in this scene.");

        Renderer[] bakedRenderers = levelRoot.GetComponentsInChildren<Renderer>(true)
            .Where(IsValidBakedRenderer)
            .ToArray();

        if (bakedRenderers.Length == 0)
            throw new InvalidOperationException(
                "No baked renderers were found under the selected root. Enable Contribute GI, generate lightmap UVs, and bake first.");

        List<int> usedGlobalIndices = bakedRenderers
            .Select(renderer => renderer.lightmapIndex)
            .Distinct()
            .OrderBy(index => index)
            .ToList();

        Dictionary<int, int> globalToLocal = new Dictionary<int, int>();
        for (int i = 0; i < usedGlobalIndices.Count; i++)
            globalToLocal.Add(usedGlobalIndices[i], i);

        string captureId = isTutorial
            ? tutorialPrefab.name
            : (string.IsNullOrWhiteSpace(level.puzzleId) ? levelRoot.name : level.puzzleId);
        string levelId = SanitizeFileName(captureId);
        string levelFolder = outputFolder.TrimEnd('/') + "/" + levelId;
        EnsureAssetFolder(levelFolder);

        LightmapData[] currentMaps = LightmapSettings.lightmaps;
        LevelLightingData.LightmapTextureSet[] copiedMaps =
            new LevelLightingData.LightmapTextureSet[usedGlobalIndices.Count];

        for (int localIndex = 0; localIndex < usedGlobalIndices.Count; localIndex++)
        {
            int globalIndex = usedGlobalIndices[localIndex];
            LightmapData source = currentMaps[globalIndex];
            copiedMaps[localIndex] = new LevelLightingData.LightmapTextureSet
            {
                color = CopyTextureAsset(source.lightmapColor, levelFolder, localIndex, "Color"),
                direction = CopyTextureAsset(source.lightmapDir, levelFolder, localIndex, "Direction"),
                shadowMask = CopyTextureAsset(source.shadowMask, levelFolder, localIndex, "ShadowMask")
            };
        }

        LevelLightingData.RendererBinding[] bindings =
            new LevelLightingData.RendererBinding[bakedRenderers.Length];

        for (int i = 0; i < bakedRenderers.Length; i++)
        {
            Renderer renderer = bakedRenderers[i];
            Renderer[] componentsOnTransform = renderer.transform.GetComponents<Renderer>();
            bindings[i] = new LevelLightingData.RendererBinding
            {
                childIndices = BuildChildIndexPath(levelRoot.transform, renderer.transform),
                rendererComponentIndex = Array.IndexOf(componentsOnTransform, renderer),
                rendererPath = AnimationUtility.CalculateTransformPath(renderer.transform, levelRoot.transform),
                lightmapIndex = globalToLocal[renderer.lightmapIndex],
                lightmapScaleOffset = renderer.lightmapScaleOffset
            };
        }

        string dataPath = levelFolder + "/" + levelId + "_Lighting.asset";
        LevelLightingData lightingAsset = AssetDatabase.LoadAssetAtPath<LevelLightingData>(dataPath);
        if (lightingAsset == null)
        {
            lightingAsset = CreateInstance<LevelLightingData>();
            AssetDatabase.CreateAsset(lightingAsset, dataPath);
        }

        GameObject configuredPrefab = isTutorial ? tutorialPrefab : level.levelPrefab;
        GameObject capturedPrefab = prefabSource != null ? prefabSource : configuredPrefab;
        string prefabPath = capturedPrefab != null ? AssetDatabase.GetAssetPath(capturedPrefab) : string.Empty;
        lightingAsset.SetCapturedData(
            LightmapSettings.lightmapsMode,
            copiedMaps,
            bindings,
            SceneManager.GetActiveScene().path,
            prefabPath,
            DateTime.UtcNow.ToString("O"));

        if (isTutorial)
            levelData.tutorialBakedLighting = lightingAsset;
        else
            level.bakedLighting = lightingAsset;

        EditorUtility.SetDirty(lightingAsset);
        EditorUtility.SetDirty(levelData);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Selection.activeObject = lightingAsset;
        EditorGUIUtility.PingObject(lightingAsset);
        string captureName = isTutorial ? tutorialPrefab.name + " (Tutorial)" : level.levelName;
        Debug.Log("[PrefabBakedLighting] Captured " + bindings.Length + " renderers and " +
                  copiedMaps.Length + " lightmaps for '" + captureName + "' at " + dataPath + ".");
    }

    private Level FindMatchingLevel(GameObject prefabSource)
    {
        if (levelData.levels == null) return null;

        for (int i = 0; i < levelData.levels.Length; i++)
        {
            Level candidate = levelData.levels[i];
            if (candidate != null && candidate.levelPrefab == prefabSource)
                return candidate;
        }

        string sceneRootName = levelRoot.name.Replace("(Clone)", string.Empty).Trim();
        Level nameMatch = null;

        for (int i = 0; i < levelData.levels.Length; i++)
        {
            Level candidate = levelData.levels[i];
            if (candidate == null || candidate.levelPrefab == null ||
                !string.Equals(candidate.levelPrefab.name, sceneRootName, StringComparison.Ordinal))
                continue;

            if (nameMatch != null)
                return null;

            nameMatch = candidate;
        }

        return nameMatch;
    }

    private GameObject FindMatchingTutorialPrefab(GameObject prefabSource)
    {
        GameManager[] managers = FindObjectsOfType<GameManager>(true);
        string sceneRootName = levelRoot.name.Replace("(Clone)", string.Empty).Trim();
        GameObject nameMatch = null;

        for (int i = 0; i < managers.Length; i++)
        {
            GameManager manager = managers[i];
            if (manager == null || manager.gameObject.scene != levelRoot.scene)
                continue;

            GameObject candidate = manager.TutorialLevelPrefab;
            if (candidate == null)
                continue;

            if (candidate == prefabSource)
                return candidate;

            if (!string.Equals(candidate.name, sceneRootName, StringComparison.Ordinal))
                continue;

            if (nameMatch != null && nameMatch != candidate)
                return null;

            nameMatch = candidate;
        }

        return nameMatch;
    }

    private static bool IsValidBakedRenderer(Renderer renderer)
    {
        if (renderer == null) return false;
        int index = renderer.lightmapIndex;
        LightmapData[] maps = LightmapSettings.lightmaps;
        return index >= 0 && maps != null && index < maps.Length;
    }

    private static Texture2D CopyTextureAsset(Texture source, string levelFolder, int index, string suffix)
    {
        if (source == null) return null;

        string sourcePath = AssetDatabase.GetAssetPath(source);
        if (string.IsNullOrEmpty(sourcePath))
            throw new InvalidOperationException("Lightmap texture '" + source.name + "' is not a persistent asset.");

        string extension = Path.GetExtension(sourcePath);
        string destination = levelFolder + "/Lightmap-" + index.ToString("000") + "_" + suffix + extension;

        if (AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(destination) != null &&
            !AssetDatabase.DeleteAsset(destination))
            throw new IOException("Could not replace existing lightmap asset: " + destination);

        if (!AssetDatabase.CopyAsset(sourcePath, destination))
            throw new IOException("Could not copy lightmap from '" + sourcePath + "' to '" + destination + "'.");

        AssetDatabase.ImportAsset(destination, ImportAssetOptions.ForceSynchronousImport);
        return AssetDatabase.LoadAssetAtPath<Texture2D>(destination);
    }

    private static int[] BuildChildIndexPath(Transform root, Transform target)
    {
        if (target == root) return Array.Empty<int>();

        List<int> reversed = new List<int>();
        Transform current = target;
        while (current != null && current != root)
        {
            reversed.Add(current.GetSiblingIndex());
            current = current.parent;
        }

        if (current != root)
            throw new InvalidOperationException("Renderer is not a child of the selected level root.");

        reversed.Reverse();
        return reversed.ToArray();
    }

    private static void EnsureAssetFolder(string folderPath)
    {
        string normalized = folderPath.Replace((char)92, '/').TrimEnd('/');
        string[] parts = normalized.Split('/');
        if (parts.Length == 0 || parts[0] != "Assets")
            throw new InvalidOperationException("Lighting output must be inside Assets/.");

        string current = "Assets";
        for (int i = 1; i < parts.Length; i++)
        {
            string next = current + "/" + parts[i];
            if (!AssetDatabase.IsValidFolder(next))
                AssetDatabase.CreateFolder(current, parts[i]);
            current = next;
        }
    }

    private static string SanitizeFileName(string value)
    {
        char[] invalid = Path.GetInvalidFileNameChars();
        char[] characters = value.Select(character => invalid.Contains(character) ? '_' : character).ToArray();
        return new string(characters).Replace(' ', '_');
    }
}
