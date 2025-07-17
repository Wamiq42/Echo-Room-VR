using UnityEngine;

public class HideRendererOnPlay : MonoBehaviour
{
    private void Awake()
    {
        var renderer = GetComponent<Renderer>();
        if (renderer != null)
        {
            renderer.enabled = false;
        }
    }
}