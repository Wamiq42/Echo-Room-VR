using UnityEngine;
using System.Collections;

public class EchoPulseController : MonoBehaviour
{
    [SerializeField] private Material pulseMaterial;
    [SerializeField] private float pingSpeed = 5f;
    [SerializeField] private Color pulseColor = Color.white;
    [SerializeField] private float pulseWidth = 0.5f;
    [SerializeField] private float maxRadius = 20f;

    private Coroutine pulseRoutine;

    public void TriggerPing(Vector3 origin)
    {
        if (pulseRoutine != null)
            StopCoroutine(pulseRoutine);

        pulseRoutine = StartCoroutine(AnimatePulse(origin));
    }

    private IEnumerator AnimatePulse(Vector3 origin)
    {
        float currentRadius = 0f;

        pulseMaterial.SetVector("_PingOrigin", origin);
        pulseMaterial.SetColor("_PulseColor", pulseColor);
        pulseMaterial.SetFloat("_PulseWidth", pulseWidth);

        while (currentRadius < maxRadius)
        {
            currentRadius += Time.deltaTime * pingSpeed;
            pulseMaterial.SetFloat("_PingRadius", currentRadius);
            yield return null;
        }

        pulseMaterial.SetFloat("_PingRadius", 0f);
        pulseRoutine = null;
    }
}