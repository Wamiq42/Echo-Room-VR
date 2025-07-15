using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class EchoPulseController : MonoBehaviour
{
    [Header("Shader Reference")]
    [SerializeField] private Shader pulseShader;

    [Header("Pulse Settings")]
    [SerializeField] private float pingSpeed = 5f;
    [SerializeField] private float pulseWidth = 0.5f;
    [SerializeField] private float fadeDuration = 1.0f;
    [SerializeField] private float maxRadius = 20f;

    private MaterialPropertyBlock propBlock;
    private List<Renderer> pulseRenderers = new List<Renderer>();
    private Coroutine pulseRoutine;
    
    private void Awake()
    {
        InitializePropertyBlock();
        CachePulseRenderers();
    }
    
    /// <summary>
    /// Triggers a visual ping from the given world-space origin.
    /// Stops any running pulse and starts a new one.
    /// </summary>
    /// <param name="origin">World position of the ping center.</param>
    public void TriggerPing(Vector3 origin)
    {
        if (pulseRoutine != null)
            StopCoroutine(pulseRoutine);

        PingEmitter.PlayPingSound?.Invoke();
        pulseRoutine = StartCoroutine(AnimatePulse(origin));
    }
    
    /// <summary>
    /// Initializes the reusable MaterialPropertyBlock instance.
    /// </summary>
    private void InitializePropertyBlock()
    {
        propBlock = new MaterialPropertyBlock();
    }

    /// <summary>
    /// Finds and caches all renderers in the scene that use the specified pulse shader.
    /// </summary>
    private void CachePulseRenderers()
    {
        Renderer[] allRenderers = FindObjectsOfType<Renderer>();

        foreach (Renderer rend in allRenderers)
        {
            foreach (var mat in rend.sharedMaterials)
            {
                if (mat != null && mat.shader == pulseShader)
                {
                    pulseRenderers.Add(rend);
                    break;
                }
            }
        }
    }

  

    /// <summary>
    /// Coroutine that animates the sonar pulse outward from the origin,
    /// expanding and then fading the effect over time.
    /// </summary>
    /// <param name="origin">Center point of the pulse effect.</param>
    /// <returns>IEnumerator for coroutine execution.</returns>
    private IEnumerator AnimatePulse(Vector3 origin)
    {
        float currentRadius = 0f;

        SetPulseStart(origin);

        // Phase 1: Expand
        while (currentRadius < maxRadius)
        {
            currentRadius += Time.deltaTime * pingSpeed;
            UpdatePulseRadius(currentRadius);
            yield return null;
        }

        // Phase 2: Fade
        float fadeTimer = 0f;
        while (fadeTimer < fadeDuration)
        {
            fadeTimer += Time.deltaTime;
            float radius = Mathf.Lerp(maxRadius, 0f, fadeTimer / fadeDuration);
            UpdatePulseRadius(radius);
            yield return null;
        }

        ResetPulseRadius();
        pulseRoutine = null;
    }

    /// <summary>
    /// Initializes pulse parameters for all target renderers.
    /// </summary>
    /// <param name="origin">Origin point of the pulse.</param>
    private void SetPulseStart(Vector3 origin)
    {
        foreach (Renderer rend in pulseRenderers)
        {
            rend.GetPropertyBlock(propBlock);
            propBlock.SetVector("_PingOrigin", origin);
            propBlock.SetFloat("_PulseWidth", pulseWidth);
            propBlock.SetFloat("_PingRadius", 0f);
            rend.SetPropertyBlock(propBlock);
        }
    }

    /// <summary>
    /// Updates the current radius of the pulse for all renderers.
    /// </summary>
    /// <param name="radius">Current radius value to apply.</param>
    private void UpdatePulseRadius(float radius)
    {
        foreach (Renderer rend in pulseRenderers)
        {
            rend.GetPropertyBlock(propBlock);
            propBlock.SetFloat("_PingRadius", radius);
            rend.SetPropertyBlock(propBlock);
        }
    }

    /// <summary>
    /// Resets pulse radius to zero for all renderers.
    /// </summary>
    private void ResetPulseRadius()
    {
        UpdatePulseRadius(0f);
    }
}
