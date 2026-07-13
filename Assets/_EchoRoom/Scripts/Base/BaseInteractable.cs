using System.Collections;
using UnityEngine;

public abstract class BaseInteractable : MonoBehaviour, IEchoInteractable
{
    [SerializeField] protected bool enableDebugging = false;

    [Header("Visuals")]
    [SerializeField] protected Material idleMat;
    [SerializeField] protected Material flashMat;
    [SerializeField] protected float flashDuration = 0.2f;

    [Header("Audio")]
    [SerializeField] protected AudioSource audioSource;

    [Header("On-State Feedback")]
    [SerializeField] private Color onStateColor = new Color(0.07f, 0.32f, 0.10f, 1f);
    [SerializeField] private Color onStatePulseColor = new Color(0.18f, 0.65f, 0.24f, 1f);
    [SerializeField, Min(0f)] private float onStatePulseDuration = 0.35f;

    protected Renderer _renderer;

    private static readonly int BaseColorId = Shader.PropertyToID("_BaseColor");
    private static readonly int ColorId = Shader.PropertyToID("_Color");

    private Renderer _onStateRenderer;
    private MaterialPropertyBlock _onStateProperties;
    private Coroutine _onStatePulseRoutine;
    private Color _originalOnStateColor = Color.white;
    private int _onStateMaterialIndex;
    private int _onStateColorPropertyId = -1;

    protected virtual void Awake()
    {
        _renderer = GetComponent<Renderer>();
        if (_renderer != null && idleMat != null)
            _renderer.material = idleMat;
    }

    protected void ConfigureOnStateVisual(Renderer targetRenderer, int materialIndex = 0)
    {
        _onStateRenderer = targetRenderer;
        _onStateColorPropertyId = -1;

        if (_onStateRenderer == null)
            return;

        Material[] materials = _onStateRenderer.sharedMaterials;
        if (materials == null || materials.Length == 0)
            return;

        _onStateMaterialIndex = Mathf.Clamp(materialIndex, 0, materials.Length - 1);
        Material targetMaterial = materials[_onStateMaterialIndex];
        if (targetMaterial == null)
            return;

        if (targetMaterial.HasProperty(BaseColorId))
            _onStateColorPropertyId = BaseColorId;
        else if (targetMaterial.HasProperty(ColorId))
            _onStateColorPropertyId = ColorId;

        if (_onStateColorPropertyId >= 0)
            _originalOnStateColor = targetMaterial.GetColor(_onStateColorPropertyId);
    }

    protected void SetOnStateVisual(bool isOn, bool pulse = true)
    {
        if (_onStatePulseRoutine != null)
        {
            StopCoroutine(_onStatePulseRoutine);
            _onStatePulseRoutine = null;
        }

        if (!CanShowOnStateVisual())
            return;

        if (!isOn)
        {
            ApplyOnStateColor(_originalOnStateColor);
            return;
        }

        if (pulse && onStatePulseDuration > 0f && isActiveAndEnabled)
            _onStatePulseRoutine = StartCoroutine(PulseOnStateVisual());
        else
            ApplyOnStateColor(onStateColor);
    }

    private bool CanShowOnStateVisual()
    {
        return _onStateRenderer != null && _onStateColorPropertyId >= 0;
    }

    private IEnumerator PulseOnStateVisual()
    {
        ApplyOnStateColor(onStatePulseColor);

        float elapsed = 0f;
        while (elapsed < onStatePulseDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(elapsed / onStatePulseDuration));
            ApplyOnStateColor(Color.Lerp(onStatePulseColor, onStateColor, t));
            yield return null;
        }

        ApplyOnStateColor(onStateColor);
        _onStatePulseRoutine = null;
    }

    private void ApplyOnStateColor(Color color)
    {
        if (!CanShowOnStateVisual())
            return;

        if (_onStateProperties == null)
            _onStateProperties = new MaterialPropertyBlock();

        _onStateRenderer.GetPropertyBlock(_onStateProperties, _onStateMaterialIndex);
        _onStateProperties.SetColor(_onStateColorPropertyId, color);
        _onStateRenderer.SetPropertyBlock(_onStateProperties, _onStateMaterialIndex);
    }

    public virtual void OnPingHit()
    {
        Log("Ping hit");
        if (flashMat != null)
            StartCoroutine(FlashEffect());
        if (audioSource != null)
            audioSource.Play();
    }

    protected IEnumerator FlashEffect()
    {
        if (_renderer == null || flashMat == null || idleMat == null)
            yield break;

        _renderer.material = flashMat;
        yield return new WaitForSeconds(flashDuration);
        _renderer.material = idleMat;
    }

    protected void Log(string message)
    {
        if (enableDebugging)
            Debug.Log($"[{GetType().Name}] {message}");
    }
}