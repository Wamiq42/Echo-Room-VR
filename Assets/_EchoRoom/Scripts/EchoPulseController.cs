using UnityEngine;
using System.Collections;

public class EchoPulseController : MonoBehaviour
{
    [SerializeField] private Material[] pulseMaterials;

    [SerializeField] private float pingSpeed = 5f;
    [SerializeField] private float pulseWidth = 0.5f;
    [SerializeField] private float fadeDuration = 1.0f;
    [SerializeField] private float maxRadius = 20f;

    private bool isPulsing = false;
    
    private Coroutine pulseRoutine;

    public void TriggerPing(Vector3 origin)
    {
        //if (isPulsing) return;

        if (pulseRoutine != null)
            StopCoroutine(pulseRoutine);

        PingEmitter.PlayPingSound?.Invoke();
        pulseRoutine = StartCoroutine(AnimatePulse(origin));
    }

    private IEnumerator AnimatePulse(Vector3 origin)
    {
        isPulsing = true;
        
        float currentRadius = 0f;

        foreach (var mat in pulseMaterials)
        {
            if (mat != null)
            {
                mat.SetVector("_PingOrigin", origin);
                mat.SetFloat("_PulseWidth", pulseWidth);
            }
        }

        // Phase 1: Expand Pulse
        while (currentRadius < maxRadius)
        {
            currentRadius += Time.deltaTime * pingSpeed;

            foreach (var mat in pulseMaterials)
                if (mat != null) mat.SetFloat("_PingRadius", currentRadius);

            yield return null;
        }

        // Phase 2: Fade Out
        float fadeTimer = 0f;

        while (fadeTimer < fadeDuration)
        {
            fadeTimer += Time.deltaTime;
            float radius = Mathf.Lerp(maxRadius, 0f, fadeTimer / fadeDuration);

            foreach (var mat in pulseMaterials)
                if (mat != null) mat.SetFloat("_PingRadius", radius);

            yield return null;
        }

        foreach (var mat in pulseMaterials)
            if (mat != null) mat.SetFloat("_PingRadius", 0f);

        pulseRoutine = null;
       // isPulsing = false;
    }

}