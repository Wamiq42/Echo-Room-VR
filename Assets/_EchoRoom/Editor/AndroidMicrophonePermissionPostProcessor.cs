#if UNITY_ANDROID
using System.IO;
using System.Xml;
using UnityEditor.Android;
using UnityEngine;

/// <summary>
/// Adds RECORD_AUDIO to the Gradle-generated Android manifest so push-to-talk can
/// reach a microphone on Quest. A hand-authored Assets/Plugins/Android/AndroidManifest.xml
/// would take over from the entries the Meta XR and OpenXR plugins inject, so the
/// permission is merged into Unity's own generated manifest instead.
/// </summary>
public class AndroidMicrophonePermissionPostProcessor : IPostGenerateGradleAndroidProject
{
    private const string PermissionElement = "uses-permission";
    private const string ApplicationElement = "application";
    private const string RecordAudioPermission = "android.permission.RECORD_AUDIO";
    private const string AndroidXmlNamespace = "http://schemas.android.com/apk/res/android";

    // Oculus XR Plugin runs at 10000 and Meta XR core at 99999. Running last means the
    // duplicate check reads the manifest exactly as it will be handed to the merger.
    public int callbackOrder => int.MaxValue;

    public void OnPostGenerateGradleAndroidProject(string path)
    {
        string manifestPath = Path.Combine(path, "src", "main", "AndroidManifest.xml");
        if (!File.Exists(manifestPath))
        {
            Debug.LogWarning(
                $"[AndroidMicrophonePermissionPostProcessor] No manifest at '{manifestPath}'; " +
                $"{RecordAudioPermission} was not injected and the microphone will be dead on device.");
            return;
        }

        XmlDocument manifest = new XmlDocument();
        manifest.Load(manifestPath);

        XmlElement root = manifest.DocumentElement;
        if (root == null)
        {
            Debug.LogWarning(
                $"[AndroidMicrophonePermissionPostProcessor] '{manifestPath}' has no root element; " +
                $"{RecordAudioPermission} was not injected.");
            return;
        }

        if (HasRecordAudioPermission(root))
        {
            Debug.Log(
                $"[AndroidMicrophonePermissionPostProcessor] {RecordAudioPermission} already declared " +
                $"in '{manifestPath}'; left untouched.");
            return;
        }

        XmlElement permission = manifest.CreateElement(PermissionElement);
        XmlAttribute nameAttribute = manifest.CreateAttribute("android", "name", AndroidXmlNamespace);
        nameAttribute.Value = RecordAudioPermission;
        permission.Attributes.Append(nameAttribute);

        // Permissions conventionally precede <application>; the parser does not care but
        // lint and diffs read better when the injected node sits with its siblings.
        XmlNode application = FindApplicationElement(root);
        if (application != null)
            root.InsertBefore(permission, application);
        else
            root.AppendChild(permission);

        manifest.Save(manifestPath);

        Debug.Log($"[AndroidMicrophonePermissionPostProcessor] Injected {RecordAudioPermission} into '{manifestPath}'.");
    }

    private static bool HasRecordAudioPermission(XmlElement root)
    {
        // Walked by LocalName instead of XPath because the manifest root declares no
        // default namespace while android:name is namespace qualified.
        foreach (XmlNode child in root.ChildNodes)
        {
            if (child.LocalName != PermissionElement)
                continue;

            if (child is XmlElement element &&
                element.GetAttribute("name", AndroidXmlNamespace) == RecordAudioPermission)
                return true;
        }

        return false;
    }

    private static XmlNode FindApplicationElement(XmlElement root)
    {
        foreach (XmlNode child in root.ChildNodes)
        {
            if (child is XmlElement && child.LocalName == ApplicationElement)
                return child;
        }

        return null;
    }
}
#endif
