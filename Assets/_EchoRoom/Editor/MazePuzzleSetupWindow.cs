using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using UnityEditor;
using UnityEditor.Animations;
using UnityEditor.SceneManagement;
using UnityEngine;

public class MazePuzzleSetupWindow : EditorWindow
{
    private enum PuzzleType
    {
        Levers,
        Buttons
    }

    private const string DefaultLeverPrefabPath = "Assets/_EchoRoom/Prefabs/PuzzleLever_Auto.prefab";
    private const string DefaultButtonPrefabPath = "Assets/_EchoRoom/Prefabs/PuzzleButton_Auto.prefab";
    private const string DefaultLeverControllerPath = "Assets/_EchoRoom/Animations/Lever.controller";
    private const string DefaultButtonControllerPath = "Assets/_EchoRoom/Animations/Auto_P1_Button 1.controller";
    private const string DefaultDoorControllerPath = "Assets/_EchoRoom/Animations/Door_Leaf.controller";
    private const string DoorAnimatorBool = "LeversOn";
    private const string DoorIndicatorsName = "Door Progress Indicators";
    private const string DoorIndicatorMaterialPath = "Assets/_EchoRoom/Materials/Door Indicator Emissive.mat";

    [SerializeField] private GameObject mazeRoot;
    [SerializeField] private UnityEngine.Object blueprintSvg;
    [SerializeField] private GameObject doorLeaf;
    [SerializeField] private PuzzleType puzzleType = PuzzleType.Levers;
    [SerializeField] private GameObject leverPrefab;
    [SerializeField] private GameObject buttonPrefab;
    [SerializeField] private AnimatorController leverController;
    [SerializeField] private AnimatorController buttonController;
    [SerializeField] private AnimatorController doorController;
    [SerializeField] private string doorLeafName = "Door_Leaf";
    [SerializeField] private bool replaceExistingAutoObjects = true;
    [SerializeField] private bool mirrorGridMapping = false;
    [SerializeField] private float placementY = 0f;
    [SerializeField] private float keyboardTestRange = 2f;
    [SerializeField] private Vector3 indicatorPanelLocalPosition = new Vector3(-12.36f, 2.60f, -11.19f);
    [SerializeField] private Vector3 indicatorPanelLocalEulerAngles = new Vector3(0f, 90f, 90f);
    [SerializeField] private Vector3 indicatorPanelLocalScale = new Vector3(0.57f, 0.57f, 0.57f);
    [SerializeField] private Vector3 levelIntroLocalPosition = new Vector3(-4.85f, 1.45f, -1.55f);
    [SerializeField] private Vector3 doorInstructionLocalPosition = new Vector3(-12.43f, 1.48f, -10.65f);
    [SerializeField] private Vector3 doorInstructionLocalEulerAngles = new Vector3(0f, -90f, 0f);

    private string previewText = "No preview yet.";

    [MenuItem("Echo Room/Maze Puzzle Setup")]
    public static void Open()
    {
        GetWindow<MazePuzzleSetupWindow>("Maze Puzzle Setup");
    }

    private void OnEnable()
    {
        if (leverPrefab == null)
            leverPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(DefaultLeverPrefabPath);

        if (buttonPrefab == null)
            buttonPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(DefaultButtonPrefabPath);

        if (leverController == null)
            leverController = AssetDatabase.LoadAssetAtPath<AnimatorController>(DefaultLeverControllerPath);


        if (buttonController == null)
            buttonController = AssetDatabase.LoadAssetAtPath<AnimatorController>(DefaultButtonControllerPath);
        if (doorController == null)
            doorController = AssetDatabase.LoadAssetAtPath<AnimatorController>(DefaultDoorControllerPath);
    }

    private void OnGUI()
    {
        EditorGUILayout.LabelField("Maze Source", EditorStyles.boldLabel);
        mazeRoot = (GameObject)EditorGUILayout.ObjectField("Maze Root", mazeRoot, typeof(GameObject), true);
        blueprintSvg = EditorGUILayout.ObjectField("Blueprint SVG", blueprintSvg, typeof(UnityEngine.Object), false);

        EditorGUILayout.Space(8);
        EditorGUILayout.LabelField("Door Reference", EditorStyles.boldLabel);
        doorLeaf = (GameObject)EditorGUILayout.ObjectField("Door Leaf", doorLeaf, typeof(GameObject), true);
        doorLeafName = EditorGUILayout.TextField("Door Leaf Name", doorLeafName);
        indicatorPanelLocalPosition = EditorGUILayout.Vector3Field("Indicator Position", indicatorPanelLocalPosition);
        indicatorPanelLocalEulerAngles = EditorGUILayout.Vector3Field("Indicator Rotation", indicatorPanelLocalEulerAngles);
        indicatorPanelLocalScale = EditorGUILayout.Vector3Field("Indicator Scale", indicatorPanelLocalScale);
        levelIntroLocalPosition = EditorGUILayout.Vector3Field("Level Text Position", levelIntroLocalPosition);
        doorInstructionLocalPosition = EditorGUILayout.Vector3Field("Door Text Position", doorInstructionLocalPosition);
        doorInstructionLocalEulerAngles = EditorGUILayout.Vector3Field("Door Text Rotation", doorInstructionLocalEulerAngles);

        EditorGUILayout.Space(8);
        EditorGUILayout.LabelField("Puzzle", EditorStyles.boldLabel);
        puzzleType = (PuzzleType)EditorGUILayout.EnumPopup("Puzzle Type", puzzleType);
        leverPrefab = (GameObject)EditorGUILayout.ObjectField("Lever Prefab", leverPrefab, typeof(GameObject), false);
        buttonPrefab = (GameObject)EditorGUILayout.ObjectField("Button Prefab", buttonPrefab, typeof(GameObject), false);
        leverController = (AnimatorController)EditorGUILayout.ObjectField("Lever Controller", leverController, typeof(AnimatorController), false);
        buttonController = (AnimatorController)EditorGUILayout.ObjectField("Button Controller", buttonController, typeof(AnimatorController), false);
        doorController = (AnimatorController)EditorGUILayout.ObjectField("Door Controller", doorController, typeof(AnimatorController), false);
        placementY = EditorGUILayout.FloatField("Placement Y", placementY);
        keyboardTestRange = EditorGUILayout.FloatField("Keyboard Test Range", keyboardTestRange);
        mirrorGridMapping = EditorGUILayout.Toggle("Mirror Grid Mapping", mirrorGridMapping);
        replaceExistingAutoObjects = EditorGUILayout.Toggle("Replace Auto Objects", replaceExistingAutoObjects);

        EditorGUILayout.Space(8);
        using (new EditorGUILayout.HorizontalScope())
        {
            if (GUILayout.Button("Find Door"))
                FindSceneReferences();

            if (GUILayout.Button("Preview"))
                Preview();

            if (GUILayout.Button("Apply Setup"))
                ApplySetup();
        }

        EditorGUILayout.Space(8);
        EditorGUILayout.LabelField("Preview", EditorStyles.boldLabel);
        EditorGUILayout.HelpBox(previewText, MessageType.Info);
    }

    private void FindSceneReferences()
    {
        if (mazeRoot == null)
        {
            previewText = "Assign a maze root first.";
            return;
        }

        if (doorLeaf == null)
        {
            Transform foundDoorLeaf = FindDeepChild(mazeRoot.transform, doorLeafName);
            if (foundDoorLeaf != null)
                doorLeaf = foundDoorLeaf.gameObject;
        }

        previewText = $"References found. Door Leaf: {NameOrMissing(doorLeaf)}";
    }

    private void Preview()
    {
        if (!TryBuildPlan(out PuzzleSetupPlan plan, out string error))
        {
            previewText = error;
            return;
        }

        previewText = BuildPreview(plan);
    }

    private void ApplySetup()
    {
        if (!TryBuildPlan(out PuzzleSetupPlan plan, out string error))
        {
            previewText = error;
            return;
        }

        if (replaceExistingAutoObjects)
            RemoveExistingAutoObjects(mazeRoot.transform);

        EchoPuzzleController puzzle = mazeRoot.GetComponent<EchoPuzzleController>();
        if (puzzle == null)
            puzzle = Undo.AddComponent<EchoPuzzleController>(mazeRoot);

        foreach (MarkerPlacement placement in plan.PuzzleMarkers)
            CreatePuzzleObject(placement);

        ConfigureDoor(puzzle);
        ConfigureDoorProgressIndicators();
        ConfigureMazeWorldText(puzzle);

        EditorUtility.SetDirty(mazeRoot);
        EditorSceneManager.MarkSceneDirty(mazeRoot.scene);
        previewText = BuildPreview(plan) + "\n\nApplied setup, wired the door, and marked the scene dirty.";
    }

    private bool TryBuildPlan(out PuzzleSetupPlan plan, out string error)
    {
        plan = null;
        error = string.Empty;

        FindSceneReferences();

        if (mazeRoot == null)
            return Fail("Assign a maze root.", out error);

        if (blueprintSvg == null)
            return Fail("Assign a blueprint SVG asset.", out error);

        if (doorLeaf == null)
            return Fail("Door leaf is required. Name it Door_Leaf or assign it manually.", out error);

        GameObject selectedPrefab = puzzleType == PuzzleType.Levers ? leverPrefab : buttonPrefab;
        if (selectedPrefab == null)
            return Fail($"Assign a {puzzleType.ToString().ToLowerInvariant()} prefab.", out error);

        string path = AssetDatabase.GetAssetPath(blueprintSvg);
        if (string.IsNullOrEmpty(path) || !path.EndsWith(".svg", StringComparison.OrdinalIgnoreCase))
            return Fail("Blueprint asset must be an SVG file.", out error);

        BlueprintMarkers markers;
        try
        {
            markers = BlueprintParser.Parse(path);
        }
        catch (Exception ex)
        {
            return Fail($"Could not parse SVG: {ex.Message}", out error);
        }

        if (!markers.Points.ContainsKey("E"))
            return Fail("Blueprint must contain a red E door marker.", out error);

        if (markers.PuzzleLabels.Count == 0)
            return Fail("Blueprint must contain at least one P marker.", out error);

        if (!markers.GridCoordinates.ContainsKey("E"))
            return Fail("Blueprint red door marker could not be mapped to the detected grid.", out error);

        if (!TryGetMazeBounds(out Bounds mazeBounds))
            return Fail("Could not read the maze size from its visible model.", out error);

        DoorLeafGridMapper mapper = new DoorLeafGridMapper(
            markers.GridCoordinates["E"],
            ToXZ(doorLeaf.transform.position),
            mazeBounds,
            mirrorGridMapping);

        List<MarkerPlacement> placements = new List<MarkerPlacement>();
        foreach (string label in markers.PuzzleLabels.OrderBy(LabelSortKey))
        {
            if (!markers.GridCoordinates.TryGetValue(label, out Vector2 gridCoordinate))
                continue;

            Vector2 worldXZ = mapper.Map(gridCoordinate);
            placements.Add(new MarkerPlacement(label, new Vector3(worldXZ.x, placementY, worldXZ.y)));
        }

        plan = new PuzzleSetupPlan(markers, placements);
        return true;
    }

    private void CreatePuzzleObject(MarkerPlacement placement)
    {
        GameObject prefab = puzzleType == PuzzleType.Levers ? leverPrefab : buttonPrefab;
        GameObject instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab, mazeRoot.transform);
        Undo.RegisterCreatedObjectUndo(instance, $"Create {placement.Label}");

        instance.name = $"Auto_{placement.Label}_{puzzleType.ToString().TrimEnd('s')}";
        instance.transform.position = placement.WorldPosition;

        if (puzzleType == PuzzleType.Levers)
            ConfigureLever(instance);
        else
            ConfigureButton(instance);

        EditorUtility.SetDirty(instance);
    }

    private void ConfigureLever(GameObject lever)
    {
        Animator animator = lever.GetComponent<Animator>();
        if (animator == null)
            animator = Undo.AddComponent<Animator>(lever);
        if (leverController != null)
            animator.runtimeAnimatorController = leverController;

        AudioSource audioSource = lever.GetComponent<AudioSource>();
        if (audioSource == null)
            audioSource = Undo.AddComponent<AudioSource>(lever);

        Collider collider = lever.GetComponent<Collider>();
        if (collider == null)
            collider = Undo.AddComponent<BoxCollider>(lever);
        collider.isTrigger = true;

        Rigidbody body = lever.GetComponent<Rigidbody>();
        if (body == null)
            body = Undo.AddComponent<Rigidbody>(lever);
        body.isKinematic = true;
        body.useGravity = false;

        LeverInteractable interactable = lever.GetComponent<LeverInteractable>();
        if (interactable == null)
            interactable = Undo.AddComponent<LeverInteractable>(lever);

        SerializedObject serialized = new SerializedObject(interactable);
        SetIfFound(serialized, "audioSource", audioSource);
        SetIfFound(serialized, "allowKeyboardTesting", true);
        SetIfFound(serialized, "keyboardTestRange", keyboardTestRange);
        serialized.ApplyModifiedPropertiesWithoutUndo();
    }

    private void ConfigureButton(GameObject button)
    {
        Animator animator = button.GetComponent<Animator>();
        if (animator == null)
            animator = Undo.AddComponent<Animator>(button);
        if (buttonController != null)
            animator.runtimeAnimatorController = buttonController;
        animator.applyRootMotion = false;
        animator.cullingMode = AnimatorCullingMode.AlwaysAnimate;

        AudioSource audioSource = button.GetComponent<AudioSource>();
        if (audioSource == null)
            audioSource = Undo.AddComponent<AudioSource>(button);

        Collider collider = button.GetComponent<Collider>();
        if (collider == null)
            collider = Undo.AddComponent<BoxCollider>(button);
        collider.isTrigger = true;

        EchoButtonInteractable interactable = button.GetComponent<EchoButtonInteractable>();
        if (interactable == null)
            interactable = Undo.AddComponent<EchoButtonInteractable>(button);

        SerializedObject serialized = new SerializedObject(interactable);
        SetIfFound(serialized, "audioSource", audioSource);
        SetIfFound(serialized, "allowKeyboardTesting", true);
        SetIfFound(serialized, "keyboardTestRange", keyboardTestRange);
        SerializedProperty buttonTop = serialized.FindProperty("buttonTop");
        if (buttonTop != null && buttonTop.objectReferenceValue == null && button.transform.childCount > 0)
            buttonTop.objectReferenceValue = button.transform.GetChild(0);
        serialized.ApplyModifiedPropertiesWithoutUndo();
    }

    private void ConfigureDoor(EchoPuzzleController puzzle)
    {
        AudioSource audioSource = doorLeaf.GetComponent<AudioSource>();
        if (audioSource == null)
            audioSource = Undo.AddComponent<AudioSource>(doorLeaf);

        Animator animator = doorLeaf.GetComponent<Animator>();
        if (animator == null)
            animator = Undo.AddComponent<Animator>(doorLeaf);
        if (doorController != null)
            animator.runtimeAnimatorController = doorController;
        animator.applyRootMotion = false;
        animator.cullingMode = AnimatorCullingMode.AlwaysAnimate;
        animator.SetBool(DoorAnimatorBool, false);

        Door door = doorLeaf.GetComponent<Door>();
        if (door == null)
            door = Undo.AddComponent<Door>(doorLeaf);

        SerializedObject doorSerialized = new SerializedObject(door);
        SetIfFound(doorSerialized, "open", false);
        SetIfFound(doorSerialized, "animator", animator);
        SetIfFound(doorSerialized, "openParameter", DoorAnimatorBool);
        SetIfFound(doorSerialized, "asource", audioSource);
        doorSerialized.ApplyModifiedPropertiesWithoutUndo();

        DoorPuzzleBinder binder = doorLeaf.GetComponent<DoorPuzzleBinder>();
        if (binder == null)
            binder = Undo.AddComponent<DoorPuzzleBinder>(doorLeaf);

        SerializedObject binderSerialized = new SerializedObject(binder);
        SetIfFound(binderSerialized, "puzzle", puzzle);
        binderSerialized.ApplyModifiedPropertiesWithoutUndo();

        EditorUtility.SetDirty(doorLeaf);
    }

    private void ConfigureMazeWorldText(PuzzleBase puzzle)
    {
        Transform introAnchor = mazeRoot.transform.Find("Writing placement");
        if (introAnchor == null)
        {
            GameObject anchorObject = new GameObject("Writing placement");
            Undo.RegisterCreatedObjectUndo(anchorObject, "Create level text anchor");
            introAnchor = anchorObject.transform;
            introAnchor.SetParent(mazeRoot.transform, false);
        }
        introAnchor.localPosition = levelIntroLocalPosition;
        introAnchor.localRotation = Quaternion.identity;
        introAnchor.localScale = Vector3.one;

        Transform oldIntro = introAnchor.Find("Level Intro Text");
        if (oldIntro != null)
            Undo.DestroyObjectImmediate(oldIntro.gameObject);

        int levelNumber = ResolveLevelNumber(mazeRoot.name);
        string introContent = levelNumber == 1
            ? "LEVEL 1\nFind the exit."
            : $"LEVEL {levelNumber}";
        TextMesh introText = CreateWorldText(
            "Level Intro Text",
            introContent,
            0.035f,
            new Color(0.72f, 0.82f, 0.72f, 0f));
        Undo.RegisterCreatedObjectUndo(introText.gameObject, "Create level intro text");
        introText.transform.SetParent(introAnchor, false);
        introText.transform.localPosition = Vector3.zero;
        introText.transform.localRotation = Quaternion.identity;
        introText.transform.localScale = Vector3.one;

        Transform oldDoorText = mazeRoot.transform.Find("Door Instruction Text");
        if (oldDoorText != null)
            Undo.DestroyObjectImmediate(oldDoorText.gameObject);

        string controlType = puzzleType == PuzzleType.Buttons ? "buttons" : "levers";
        TextMesh doorText = CreateWorldText(
            "Door Instruction Text",
            $"Activate the {controlType} to unlock.",
            0.018f,
            new Color(0.72f, 0.80f, 0.64f, 0f));
        Undo.RegisterCreatedObjectUndo(doorText.gameObject, "Create door instruction text");
        doorText.transform.SetParent(mazeRoot.transform, false);
        doorText.transform.localPosition = doorInstructionLocalPosition;
        doorText.transform.localEulerAngles = doorInstructionLocalEulerAngles;
        doorText.transform.localScale = Vector3.one;

        MazeWorldTextController controller = mazeRoot.GetComponent<MazeWorldTextController>();
        if (controller == null)
            controller = Undo.AddComponent<MazeWorldTextController>(mazeRoot);

        SerializedObject serialized = new SerializedObject(controller);
        SetIfFound(serialized, "introText", introText);
        SetIfFound(serialized, "introDuration", 5f);
        SetIfFound(serialized, "introFadeInDuration", 0.35f);
        SetIfFound(serialized, "introFadeOutDuration", 0.75f);
        SetIfFound(serialized, "doorPromptText", doorText);
        SetIfFound(serialized, "door", doorLeaf.transform);
        SetIfFound(serialized, "puzzle", puzzle);
        SetIfFound(serialized, "doorPromptDistance", 2.25f);
        SetIfFound(serialized, "doorFadeDuration", 0.4f);
        serialized.ApplyModifiedPropertiesWithoutUndo();

        EditorUtility.SetDirty(controller);
        EditorUtility.SetDirty(introAnchor.gameObject);
        EditorUtility.SetDirty(doorText.gameObject);
    }

    private static TextMesh CreateWorldText(string objectName, string content, float characterSize, Color color)
    {
        GameObject textObject = new GameObject(objectName);
        TextMesh text = textObject.AddComponent<TextMesh>();
        text.text = content;
        text.fontSize = 64;
        text.characterSize = characterSize;
        text.lineSpacing = 1f;
        text.anchor = TextAnchor.MiddleCenter;
        text.alignment = TextAlignment.Center;
        text.fontStyle = FontStyle.Bold;
        text.richText = true;
        text.color = color;

        MeshRenderer renderer = textObject.GetComponent<MeshRenderer>();
        renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        renderer.receiveShadows = false;
        renderer.lightProbeUsage = UnityEngine.Rendering.LightProbeUsage.Off;
        renderer.reflectionProbeUsage = UnityEngine.Rendering.ReflectionProbeUsage.Off;
        renderer.sortingOrder = 20;
        return text;
    }

    private static int ResolveLevelNumber(string objectName)
    {
        if (string.IsNullOrWhiteSpace(objectName))
            return 1;

        string cleanName = objectName.Replace("(Clone)", string.Empty).Trim();
        char suffix = char.ToUpperInvariant(cleanName[cleanName.Length - 1]);
        if (suffix >= 'A' && suffix <= 'E')
            return (suffix - 'A') + 1;

        for (int i = cleanName.Length - 1; i >= 0; i--)
        {
            if (char.IsDigit(cleanName[i]))
                return cleanName[i] - '0';
        }

        return 1;
    }

    private void ConfigureDoorProgressIndicators()
    {
        Transform existingPanel = mazeRoot.transform.Find(DoorIndicatorsName);
        if (existingPanel != null)
            Undo.DestroyObjectImmediate(existingPanel.gameObject);

        List<MonoBehaviour> sources = mazeRoot.GetComponentsInChildren<LeverInteractable>(true)
            .Cast<MonoBehaviour>()
            .Concat(mazeRoot.GetComponentsInChildren<EchoButtonInteractable>(true).Cast<MonoBehaviour>())
            .Where(source => IsAutoPuzzleObject(source.transform))
            .OrderBy(source => SourceOrder(source.name))
            .ThenBy(source => source.name, StringComparer.Ordinal)
            .Take(3)
            .ToList();

        GameObject panel = new GameObject(DoorIndicatorsName);
        Undo.RegisterCreatedObjectUndo(panel, "Create door progress indicators");
        panel.transform.SetParent(mazeRoot.transform, false);
        panel.transform.localPosition = indicatorPanelLocalPosition;
        panel.transform.localEulerAngles = indicatorPanelLocalEulerAngles;
        panel.transform.localScale = indicatorPanelLocalScale;

        Material indicatorMaterial = AssetDatabase.LoadAssetAtPath<Material>(DoorIndicatorMaterialPath);
        Renderer[] indicatorRenderers = new Renderer[3];
        for (int i = 0; i < 3; i++)
        {
            GameObject indicator = GameObject.CreatePrimitive(PrimitiveType.Cube);
            string sourceName = i < sources.Count ? sources[i].name : $"P{i + 1}";
            indicator.name = $"Indicator {i + 1} ({sourceName})";
            Undo.RegisterCreatedObjectUndo(indicator, $"Create indicator {i + 1}");
            indicator.transform.SetParent(panel.transform, false);
            indicator.transform.localPosition = new Vector3(0f, 0.22f - (i * 0.22f), 0f);
            indicator.transform.localRotation = Quaternion.identity;
            indicator.transform.localScale = new Vector3(0.16f, 0.16f, 0.045f);

            Collider indicatorCollider = indicator.GetComponent<Collider>();
            if (indicatorCollider != null)
                Undo.DestroyObjectImmediate(indicatorCollider);

            Renderer renderer = indicator.GetComponent<Renderer>();
            if (indicatorMaterial != null)
                renderer.sharedMaterial = indicatorMaterial;
            renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            renderer.receiveShadows = false;
            indicatorRenderers[i] = renderer;
        }

        DoorProgressIndicators progress = mazeRoot.GetComponent<DoorProgressIndicators>();
        if (progress == null)
            progress = Undo.AddComponent<DoorProgressIndicators>(mazeRoot);

        SerializedObject serialized = new SerializedObject(progress);
        SerializedProperty sourceArray = serialized.FindProperty("sources");
        SerializedProperty indicatorArray = serialized.FindProperty("indicators");
        sourceArray.arraySize = 3;
        indicatorArray.arraySize = 3;
        serialized.FindProperty("inactiveColor").colorValue = new Color(0.32f, 0.04f, 0.03f, 1f);
        serialized.FindProperty("activeColor").colorValue = new Color(0.07f, 0.32f, 0.10f, 1f);
        serialized.FindProperty("emissionIntensity").floatValue = 0.6f;
        for (int i = 0; i < 3; i++)
        {
            sourceArray.GetArrayElementAtIndex(i).objectReferenceValue = i < sources.Count ? sources[i] : null;
            indicatorArray.GetArrayElementAtIndex(i).objectReferenceValue = indicatorRenderers[i];
        }
        serialized.ApplyModifiedPropertiesWithoutUndo();

        EditorUtility.SetDirty(progress);
        EditorUtility.SetDirty(panel);
    }

    private static int SourceOrder(string objectName)
    {
        for (int i = 1; i <= 3; i++)
        {
            if (objectName.IndexOf($"P{i}", StringComparison.OrdinalIgnoreCase) >= 0)
                return i;
        }

        return int.MaxValue;
    }

    private static bool IsAutoPuzzleObject(Transform transform)
    {
        while (transform != null)
        {
            if (transform.name.StartsWith("Auto_P", StringComparison.Ordinal))
                return true;

            transform = transform.parent;
        }

        return false;
    }

    private bool TryGetMazeBounds(out Bounds bounds)
    {
        bounds = default;
        Renderer[] renderers = mazeRoot.GetComponentsInChildren<Renderer>(true)
            .Where(renderer => renderer != null)
            .Where(renderer => !IsAutoPuzzleObject(renderer.transform))
            .Where(renderer => doorLeaf == null || !renderer.transform.IsChildOf(doorLeaf.transform))
            .ToArray();

        if (renderers.Length == 0)
            return false;

        bounds = renderers[0].bounds;
        for (int i = 1; i < renderers.Length; i++)
            bounds.Encapsulate(renderers[i].bounds);

        return bounds.size.x > Mathf.Epsilon && bounds.size.z > Mathf.Epsilon;
    }

    private static void RemoveExistingAutoObjects(Transform root)
    {
        List<GameObject> toRemove = new List<GameObject>();
        foreach (Transform child in root)
        {
            if (child.name.StartsWith("Auto_P", StringComparison.Ordinal))
                toRemove.Add(child.gameObject);
        }

        foreach (GameObject child in toRemove)
            Undo.DestroyObjectImmediate(child);
    }

    private static string BuildPreview(PuzzleSetupPlan plan)
    {
        string startPoint = plan.Markers.Points.TryGetValue("S", out Vector2 start) ? start.ToString() : "not used";

        List<string> lines = new List<string>
        {
            $"Parsed markers: S={startPoint}, E={plan.Markers.Points["E"]}",
            "Puzzle placements:"
        };

        foreach (MarkerPlacement placement in plan.PuzzleMarkers)
            lines.Add($"{placement.Label}: {placement.WorldPosition}");

        return string.Join("\n", lines);
    }

    private static Transform FindDeepChild(Transform root, string childName)
    {
        if (root == null || string.IsNullOrWhiteSpace(childName))
            return null;

        foreach (Transform child in root.GetComponentsInChildren<Transform>(true))
        {
            if (child.name == childName)
                return child;
        }

        return null;
    }

    private static void SetIfFound(SerializedObject serialized, string propertyName, UnityEngine.Object value)
    {
        SerializedProperty property = serialized.FindProperty(propertyName);
        if (property != null)
            property.objectReferenceValue = value;
    }

    private static void SetIfFound(SerializedObject serialized, string propertyName, bool value)
    {
        SerializedProperty property = serialized.FindProperty(propertyName);
        if (property != null)
            property.boolValue = value;
    }

    private static void SetIfFound(SerializedObject serialized, string propertyName, float value)
    {
        SerializedProperty property = serialized.FindProperty(propertyName);
        if (property != null)
            property.floatValue = value;
    }

    private static void SetIfFound(SerializedObject serialized, string propertyName, string value)
    {
        SerializedProperty property = serialized.FindProperty(propertyName);
        if (property != null)
            property.stringValue = value;
    }

    private static Vector2 ToXZ(Vector3 worldPosition)
    {
        return new Vector2(worldPosition.x, worldPosition.z);
    }

    private static string NameOrMissing(UnityEngine.Object obj)
    {
        return obj != null ? obj.name : "missing";
    }

    private static bool Fail(string message, out string error)
    {
        error = message;
        return false;
    }

    private static int LabelSortKey(string label)
    {
        if (label.Length > 1 && int.TryParse(label.Substring(1), out int number))
            return number;

        return int.MaxValue;
    }

    private sealed class PuzzleSetupPlan
    {
        public readonly BlueprintMarkers Markers;
        public readonly List<MarkerPlacement> PuzzleMarkers;

        public PuzzleSetupPlan(BlueprintMarkers markers, List<MarkerPlacement> puzzleMarkers)
        {
            Markers = markers;
            PuzzleMarkers = puzzleMarkers;
        }
    }

    private sealed class MarkerPlacement
    {
        public readonly string Label;
        public readonly Vector3 WorldPosition;

        public MarkerPlacement(string label, Vector3 worldPosition)
        {
            Label = label;
            WorldPosition = worldPosition;
        }
    }

    private sealed class BlueprintMarkers
    {
        public readonly Dictionary<string, Vector2> Points = new Dictionary<string, Vector2>();
        public readonly Dictionary<string, Vector2> GridCoordinates = new Dictionary<string, Vector2>();
        public readonly List<string> PuzzleLabels = new List<string>();
    }

    private static class BlueprintParser
    {
        public static BlueprintMarkers Parse(string assetPath)
        {
            string fullPath = Path.GetFullPath(assetPath);
            XDocument document = XDocument.Parse(File.ReadAllText(fullPath));
            List<SvgCircle> circles = document.Descendants()
                .Where(element => element.Name.LocalName == "circle")
                .Select(SvgCircle.TryCreate)
                .Where(circle => circle.HasValue)
                .Select(circle => circle.Value)
                .ToList();
            List<SvgRect> rects = document.Descendants()
                .Where(element => element.Name.LocalName == "rect")
                .Select(SvgRect.TryCreate)
                .Where(rect => rect.HasValue)
                .Select(rect => rect.Value)
                .ToList();
            MazePuzzleSetupWindow.SvgGrid grid = MazePuzzleSetupWindow.SvgGrid.FromGuideDots(circles);

            BlueprintMarkers markers = new BlueprintMarkers();

            foreach (XElement text in document.Descendants().Where(element => element.Name.LocalName == "text"))
            {
                string label = text.Value.Trim();
                if (!IsMarkerLabel(label))
                    continue;

                Vector2 textPoint = new Vector2(ReadFloat(text, "x"), ReadFloat(text, "y"));
                Vector2 markerPoint = ResolveMarkerPoint(label, textPoint, circles, rects);
                markers.Points[label] = markerPoint;
                if (grid.IsValid)
                    markers.GridCoordinates[label] = grid.ToGridCoordinate(markerPoint);

                if (label.StartsWith("P", StringComparison.OrdinalIgnoreCase))
                    markers.PuzzleLabels.Add(label);
            }

            return markers;
        }

        private static Vector2 ResolveMarkerPoint(string label, Vector2 textPoint, List<SvgCircle> circles, List<SvgRect> rects)
        {
            if (label == "E")
                return ResolveExitDoorPoint(textPoint, circles, rects);

            string color = null;
            if (label == "S")
                color = "#34d399";
            else if (label.StartsWith("P", StringComparison.OrdinalIgnoreCase))
                color = "#fbbf24";

            if (color == null)
                return textPoint;

            SvgCircle? nearest = circles
                .Where(circle => string.Equals(circle.Fill, color, StringComparison.OrdinalIgnoreCase))
                .OrderBy(circle => Vector2.SqrMagnitude(circle.Center - textPoint))
                .Cast<SvgCircle?>()
                .FirstOrDefault();

            if (nearest.HasValue && Vector2.Distance(nearest.Value.Center, textPoint) <= 32f)
                return nearest.Value.Center;

            return textPoint;
        }

        private static Vector2 ResolveExitDoorPoint(Vector2 textPoint, List<SvgCircle> circles, List<SvgRect> rects)
        {
            const string exitColor = "#fb7185";

            SvgRect? nearestRect = rects
                .Where(rect => string.Equals(rect.Fill, exitColor, StringComparison.OrdinalIgnoreCase))
                .OrderBy(rect => Vector2.SqrMagnitude(rect.Center - textPoint))
                .Cast<SvgRect?>()
                .FirstOrDefault();

            if (nearestRect.HasValue && Vector2.Distance(nearestRect.Value.Center, textPoint) <= 96f)
                return nearestRect.Value.Center;

            SvgCircle? nearestCircle = circles
                .Where(circle => string.Equals(circle.Fill, exitColor, StringComparison.OrdinalIgnoreCase))
                .OrderBy(circle => Vector2.SqrMagnitude(circle.Center - textPoint))
                .Cast<SvgCircle?>()
                .FirstOrDefault();

            if (nearestCircle.HasValue && Vector2.Distance(nearestCircle.Value.Center, textPoint) <= 96f)
                return nearestCircle.Value.Center;

            return textPoint;
        }

        private static bool IsMarkerLabel(string label)
        {
            if (label == "S" || label == "E")
                return true;

            return label.Length > 1
                && label[0] == 'P'
                && label.Skip(1).All(char.IsDigit);
        }

        private static float ReadFloat(XElement element, string attributeName)
        {
            XAttribute attribute = element.Attribute(attributeName);
            if (attribute == null)
                throw new FormatException($"Missing SVG attribute '{attributeName}'.");

            return float.Parse(attribute.Value, CultureInfo.InvariantCulture);
        }
    }

    private readonly struct SvgCircle
    {
        public readonly Vector2 Center;
        public readonly string Fill;

        private SvgCircle(Vector2 center, string fill)
        {
            Center = center;
            Fill = fill;
        }

        public static SvgCircle? TryCreate(XElement element)
        {
            XAttribute cx = element.Attribute("cx");
            XAttribute cy = element.Attribute("cy");
            if (cx == null || cy == null)
                return null;

            string fill = element.Attribute("fill")?.Value ?? string.Empty;
            Vector2 center = new Vector2(
                float.Parse(cx.Value, CultureInfo.InvariantCulture),
                float.Parse(cy.Value, CultureInfo.InvariantCulture));

            return new SvgCircle(center, fill);
        }
    }

    private readonly struct SvgRect
    {
        public readonly Vector2 Center;
        public readonly string Fill;

        private SvgRect(Vector2 center, string fill)
        {
            Center = center;
            Fill = fill;
        }

        public static SvgRect? TryCreate(XElement element)
        {
            XAttribute x = element.Attribute("x");
            XAttribute y = element.Attribute("y");
            XAttribute width = element.Attribute("width");
            XAttribute height = element.Attribute("height");
            if (x == null || y == null || width == null || height == null)
                return null;

            Vector2 center = new Vector2(
                float.Parse(x.Value, CultureInfo.InvariantCulture) + float.Parse(width.Value, CultureInfo.InvariantCulture) * 0.5f,
                float.Parse(y.Value, CultureInfo.InvariantCulture) + float.Parse(height.Value, CultureInfo.InvariantCulture) * 0.5f);
            string fill = element.Attribute("fill")?.Value ?? string.Empty;

            return new SvgRect(center, fill);
        }
    }

    private readonly struct SvgGrid
    {
        private readonly float firstCenterX;
        private readonly float firstCenterY;
        private readonly float cellWidth;
        private readonly float cellHeight;

        public readonly bool IsValid;

        private SvgGrid(float firstCenterX, float firstCenterY, float cellWidth, float cellHeight)
        {
            this.firstCenterX = firstCenterX;
            this.firstCenterY = firstCenterY;
            this.cellWidth = cellWidth;
            this.cellHeight = cellHeight;
            IsValid = cellWidth > Mathf.Epsilon && cellHeight > Mathf.Epsilon;
        }

        public static SvgGrid FromGuideDots(List<SvgCircle> circles)
        {
            List<float> xs = circles
                .Where(circle => string.Equals(circle.Fill, "#1d3b66", StringComparison.OrdinalIgnoreCase))
                .Select(circle => circle.Center.x)
                .Distinct()
                .OrderBy(value => value)
                .ToList();
            List<float> ys = circles
                .Where(circle => string.Equals(circle.Fill, "#1d3b66", StringComparison.OrdinalIgnoreCase))
                .Select(circle => circle.Center.y)
                .Distinct()
                .OrderBy(value => value)
                .ToList();

            if (xs.Count < 2 || ys.Count < 2)
                return default;

            float firstCenterX = (xs[0] + xs[1]) * 0.5f;
            float firstCenterY = (ys[0] + ys[1]) * 0.5f;
            float cellWidth = xs[1] - xs[0];
            float cellHeight = ys[1] - ys[0];
            return new SvgGrid(firstCenterX, firstCenterY, cellWidth, cellHeight);
        }

        public Vector2 ToGridCoordinate(Vector2 svgPoint)
        {
            return new Vector2(
                (svgPoint.x - firstCenterX) / cellWidth,
                (svgPoint.y - firstCenterY) / cellHeight);
        }
    }

    private readonly struct DoorLeafGridMapper
    {
        private readonly Vector2 gridOrigin;
        private readonly Vector2 destinationOrigin;
        private readonly Vector2 columnAxis;
        private readonly Vector2 rowAxis;

        public DoorLeafGridMapper(Vector2 exitGrid, Vector2 doorLeafWorldXZ, Bounds mazeBounds, bool mirror)
        {
            gridOrigin = exitGrid;
            destinationOrigin = doorLeafWorldXZ;

            float cellSize = Mathf.Min(Mathf.Abs(mazeBounds.size.x), Mathf.Abs(mazeBounds.size.z)) / 5f;
            if (cellSize <= Mathf.Epsilon)
                cellSize = 2.5f;

            const float minGridEdge = -0.5f;
            const float maxGridEdge = 4.5f;
            float leftDistance = Mathf.Abs(exitGrid.x - minGridEdge);
            float rightDistance = Mathf.Abs(exitGrid.x - maxGridEdge);
            float topDistance = Mathf.Abs(exitGrid.y - minGridEdge);
            float bottomDistance = Mathf.Abs(exitGrid.y - maxGridEdge);

            Vector2 resolvedColumnAxis;
            Vector2 resolvedRowAxis;

            if (rightDistance <= leftDistance && rightDistance <= topDistance && rightDistance <= bottomDistance)
            {
                resolvedColumnAxis = new Vector2(-cellSize, 0f);
                resolvedRowAxis = new Vector2(0f, cellSize);
            }
            else if (leftDistance <= topDistance && leftDistance <= bottomDistance)
            {
                resolvedColumnAxis = new Vector2(cellSize, 0f);
                resolvedRowAxis = new Vector2(0f, cellSize);
            }
            else if (topDistance <= bottomDistance)
            {
                resolvedColumnAxis = new Vector2(cellSize, 0f);
                resolvedRowAxis = new Vector2(0f, cellSize);
            }
            else
            {
                resolvedColumnAxis = new Vector2(cellSize, 0f);
                resolvedRowAxis = new Vector2(0f, -cellSize);
            }

            if (mirror)
                resolvedRowAxis = -resolvedRowAxis;

            columnAxis = resolvedColumnAxis;
            rowAxis = resolvedRowAxis;
        }

        public Vector2 Map(Vector2 gridPoint)
        {
            Vector2 relative = gridPoint - gridOrigin;
            return destinationOrigin + columnAxis * relative.x + rowAxis * relative.y;
        }
    }
}


