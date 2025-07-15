using System.Collections;
using UnityEngine;

public class EchoTarget : MonoBehaviour, IEchoInteractable
{
    [SerializeField] private bool enableDebugging = false;   // debug toggle always on top

    [SerializeField] private Material idleMat;
    [SerializeField] private Material flashMat;
    [SerializeField] private float flashDuration = 0.2f;
    [SerializeField] private AudioSource audioSource;

    private Renderer _renderer;
    private bool _isActivated;

    void Awake()
    {
        _renderer = GetComponent<Renderer>();
        _renderer.material = idleMat;
    }

    /// <summary>
    /// Called when a ping hits this target.
    /// </summary>
    public void OnPingHit()
    {
        Log($"Ping hit on {name}");

        if (_isActivated) return;

        _isActivated = true;
        if(audioSource != null)
        audioSource?.Play();
        StartCoroutine(FlashEffect());

        // TODO: Add EchoPuzzleController logic here later
    }

    private IEnumerator FlashEffect()
    {
        _renderer.material = flashMat;
        yield return new WaitForSeconds(flashDuration);
        _renderer.material = idleMat;
    }

    public void ResetTarget()
    {
        _isActivated = false;
        _renderer.material = idleMat;
    }

    public bool IsActivated => _isActivated;

    
    
    
    // ---------- Debugging ----------
    private void Log(string message)
    {
        if (enableDebugging) Debug.Log($"[EchoTarget] {message}");
    }
}