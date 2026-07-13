using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;

[InitializeOnLoad]
internal static class EchoSonarSceneViewPreview
{
    const string SceneViewCameraProperty = "_EchoSceneViewCamera";

    static EchoSonarSceneViewPreview()
    {
        RenderPipelineManager.beginCameraRendering -= BeginCameraRendering;
        RenderPipelineManager.beginCameraRendering += BeginCameraRendering;
        RenderPipelineManager.endCameraRendering -= EndCameraRendering;
        RenderPipelineManager.endCameraRendering += EndCameraRendering;
        EditorApplication.playModeStateChanged -= PlayModeStateChanged;
        EditorApplication.playModeStateChanged += PlayModeStateChanged;
        PrefabStage.prefabStageOpened -= PrefabStageOpened;
        PrefabStage.prefabStageOpened += PrefabStageOpened;
        EditorApplication.delayCall += EnsurePrefabStageLighting;
        Shader.SetGlobalFloat(SceneViewCameraProperty, 0f);
    }

    static void PrefabStageOpened(PrefabStage stage)
    {
        EnsurePrefabStageLighting();
    }

    static void EnsurePrefabStageLighting()
    {
        if (PrefabStageUtility.GetCurrentPrefabStage() == null) return;

        foreach (SceneView sceneView in SceneView.sceneViews)
        {
            if (sceneView == null) continue;
            sceneView.sceneLighting = true;
            sceneView.Repaint();
        }
    }

    static void BeginCameraRendering(ScriptableRenderContext context, Camera camera)
    {
        bool isEditModeSceneView = !EditorApplication.isPlaying &&
                                   camera != null &&
                                   camera.cameraType == CameraType.SceneView;
        Shader.SetGlobalFloat(SceneViewCameraProperty, isEditModeSceneView ? 1f : 0f);

        if (isEditModeSceneView && PrefabStageUtility.GetCurrentPrefabStage() != null)
            EnsurePrefabStageLighting();
    }

    static void EndCameraRendering(ScriptableRenderContext context, Camera camera)
    {
        Shader.SetGlobalFloat(SceneViewCameraProperty, 0f);
    }

    static void PlayModeStateChanged(PlayModeStateChange state)
    {
        Shader.SetGlobalFloat(SceneViewCameraProperty, 0f);
    }
}
