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

    protected Renderer _renderer;

    protected virtual void Awake()
    {
        _renderer = GetComponent<Renderer>();
        if (_renderer != null && idleMat != null)
            _renderer.material = idleMat;
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