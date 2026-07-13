using UnityEngine;

[DisallowMultipleComponent]
public sealed class DoorProgressIndicators : MonoBehaviour
{
    [SerializeField] private MonoBehaviour[] sources = new MonoBehaviour[3];
    [SerializeField] private Renderer[] indicators = new Renderer[3];
    [SerializeField, ColorUsage(false, true)] private Color inactiveColor = new Color(0.32f, 0.04f, 0.03f, 1f);
    [SerializeField, ColorUsage(false, true)] private Color activeColor = new Color(0.07f, 0.32f, 0.10f, 1f);
    [SerializeField, Min(0f)] private float emissionIntensity = 0.6f;

    private MaterialPropertyBlock propertyBlock;
    private readonly bool[] lastStates = new bool[3];
    private bool initialized;

    private void Awake()
    {
        propertyBlock = new MaterialPropertyBlock();
        Refresh(true);
    }

    private void OnEnable()
    {
        Refresh(true);
    }

    private void Update()
    {
        Refresh(false);
    }

    private void Refresh(bool force)
    {
        if (propertyBlock == null)
            propertyBlock = new MaterialPropertyBlock();

        int count = Mathf.Min(sources != null ? sources.Length : 0, indicators != null ? indicators.Length : 0);
        for (int i = 0; i < count; i++)
        {
            bool isActive = ReadState(sources[i]);
            if (!force && initialized && lastStates[i] == isActive)
                continue;

            lastStates[i] = isActive;
            ApplyColor(indicators[i], isActive ? activeColor : inactiveColor);
        }

        initialized = true;
    }

    private static bool ReadState(MonoBehaviour source)
    {
        if (source is LeverInteractable lever)
            return lever.IsOn;

        if (source is EchoButtonInteractable button)
            return button.IsOn;

        return false;
    }

    private void ApplyColor(Renderer target, Color color)
    {
        if (target == null)
            return;

        target.GetPropertyBlock(propertyBlock);
        propertyBlock.SetColor("_BaseColor", color);
        propertyBlock.SetColor("_Color", color);
        propertyBlock.SetColor("_EmissionColor", color * emissionIntensity);
        target.SetPropertyBlock(propertyBlock);
    }
}
