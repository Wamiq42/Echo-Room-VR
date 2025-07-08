using UnityEditor;
using UnityEngine;

public class URPMaterialConverter : MonoBehaviour
{
    [MenuItem("Tools/URP/Upgrade All Materials to URP Lit")]
    static void UpgradeMaterials()
    {
        string[] materialGUIDs = AssetDatabase.FindAssets("t:Material");
        int upgradedCount = 0;

        foreach (string guid in materialGUIDs)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            Material mat = AssetDatabase.LoadAssetAtPath<Material>(path);

            if (mat.shader.name.Contains("Standard")) // Built-in shader
            {
                mat.shader = Shader.Find("Universal Render Pipeline/Lit");
                EditorUtility.SetDirty(mat);
                upgradedCount++;
            }
        }

        AssetDatabase.SaveAssets();
        Debug.Log($"[URPMaterialConverter] Upgraded {upgradedCount} materials to URP Lit.");
    }
}