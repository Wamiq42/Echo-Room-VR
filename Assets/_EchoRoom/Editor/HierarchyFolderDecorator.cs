#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;

[InitializeOnLoad]
public static class HierarchyFolderHighlighter
{
    static HierarchyFolderSettings settings;

    static HierarchyFolderHighlighter()
    {
        EditorApplication.hierarchyWindowItemOnGUI += DrawBackground;
        LoadSettings();
    }

    static void LoadSettings()
    {
        string[] guids = AssetDatabase.FindAssets("t:HierarchyFolderSettings");
        if (guids.Length > 0)
        {
            string path = AssetDatabase.GUIDToAssetPath(guids[0]);
            settings = AssetDatabase.LoadAssetAtPath<HierarchyFolderSettings>(path);
        }
    }

    static void DrawBackground(int instanceID, Rect selectionRect)
    {
        if (settings == null)
            return;

        GameObject obj = EditorUtility.InstanceIDToObject(instanceID) as GameObject;
        if (obj == null) return;

        foreach (var folder in settings.folders)
        {
            if (obj.name == folder.folderName)
            {
                // Draw background
                Color color = folder.backgroundColor;
                EditorGUI.DrawRect(selectionRect, color);

                // Prepare bold, centered black label style
                var style = new GUIStyle(EditorStyles.boldLabel);
                style.normal.textColor = Color.black;
                style.alignment = TextAnchor.MiddleLeft;
                style.fontStyle = FontStyle.Bold;

                // Adjust label position slightly to avoid overlap with foldout arrow
                Rect labelRect = new Rect(selectionRect.x + 20, selectionRect.y, selectionRect.width, selectionRect.height);
                GUI.Label(labelRect, obj.name, style);

                break;
            }
        }
    }

}
#endif