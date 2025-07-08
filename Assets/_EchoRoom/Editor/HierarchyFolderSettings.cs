using UnityEngine;

[CreateAssetMenu(fileName = "HierarchyFolderSettings", menuName = "Tools/Hierarchy Folder Settings")]
public class HierarchyFolderSettings : ScriptableObject
{
    [System.Serializable]
    public class FolderEntry
    {
        public string folderName;
        public Color backgroundColor;
    }

    public FolderEntry[] folders;
}