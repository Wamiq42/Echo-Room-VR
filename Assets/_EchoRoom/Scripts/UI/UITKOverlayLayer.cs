using UnityEngine;
using UnityEngine.UIElements;

namespace EchoRoom.UI
{
    /// <summary>
    /// Keeps scene-authored and runtime-created UI on the dedicated UITK overlay layer.
    /// </summary>
    public static class UITKOverlayLayer
    {
        public const string LayerName = "UITKOverlay";

        static int cachedLayer = int.MinValue;

        public static int Index
        {
            get
            {
                if (cachedLayer == int.MinValue)
                    cachedLayer = LayerMask.NameToLayer(LayerName);

                return cachedLayer;
            }
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void ApplyToLoadedUI()
        {
            UIDocument[] documents = Resources.FindObjectsOfTypeAll<UIDocument>();
            for (int i = 0; i < documents.Length; i++)
            {
                UIDocument document = documents[i];
                if (document != null && document.gameObject.scene.IsValid()) Apply(document.gameObject);
            }

            Canvas[] canvases = Resources.FindObjectsOfTypeAll<Canvas>();
            for (int i = 0; i < canvases.Length; i++)
            {
                Canvas canvas = canvases[i];
                if (canvas != null && canvas.gameObject.scene.IsValid()) Apply(canvas.gameObject);
            }
        }

        public static bool Apply(GameObject target, bool includeChildren = true)
        {
            if (target == null) return false;

            int layer = Index;
            if (layer < 0)
            {
                Debug.LogError("[UITKOverlayLayer] Project layer '" + LayerName + "' is missing.", target);
                return false;
            }

            if (includeChildren) ApplyRecursive(target.transform, layer);
            else target.layer = layer;
            return true;
        }

        public static Camera FindRenderingCamera()
        {
            int layer = Index;
            if (layer < 0) return Camera.main;

            int mask = 1 << layer;
            Camera main = Camera.main;
            Camera[] cameras = Resources.FindObjectsOfTypeAll<Camera>();
            for (int i = 0; i < cameras.Length; i++)
            {
                Camera candidate = cameras[i];
                if (candidate == null || !candidate.gameObject.scene.IsValid()) continue;
                if ((candidate.cullingMask & mask) == 0) continue;
                if (candidate != main) return candidate;
            }

            return main;
        }

        static void ApplyRecursive(Transform root, int layer)
        {
            root.gameObject.layer = layer;
            for (int i = 0; i < root.childCount; i++) ApplyRecursive(root.GetChild(i), layer);
        }
    }
}
