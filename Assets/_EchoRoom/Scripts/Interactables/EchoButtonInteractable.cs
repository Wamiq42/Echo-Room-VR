using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

public class EchoButtonInteractable : BaseInteractable , IPuzzleElement
{
    [Header("Button Settings")]
    [SerializeField] private Transform buttonTop;       // The part of the button that moves
    [SerializeField] private float pressDepth = 0.02f; // How far it moves down
    [SerializeField] private float pressSpeed = 10f;   // How fast it animates
    [SerializeField] private PlayerInputManager inputManager;
    [SerializeField] private UnityEvent onButtonPressed;

    
    private Vector3 _initialLocalPos;
    private bool _handInRange;
    private bool _gripHeld;
    private bool _hasTriggered; // to ensure we only notify once
    
    public static event System.Action<EchoButtonInteractable> OnAnyButtonPressed;

    protected override void Awake()
    {
        base.Awake();
        if (buttonTop != null)
            _initialLocalPos = buttonTop.localPosition;

        // Prefer the central input manager, but don't NRE if no GameManager exists yet
        // (e.g. testing a button in isolation). Falls back to the serialized reference.
        if (GameManager.Instance != null)
            inputManager = GameManager.Instance.PlayerInputManager;
    }

    private void Update()
    {
        _gripHeld = inputManager != null && inputManager.ReadGrip();
        AnimateButton();
    }

 
    /// <summary>
    /// Animates the button top smoothly between pressed and idle positions,
    /// and fires events once fully pressed.
    /// </summary>
    private void AnimateButton()
    {
        if (buttonTop == null) return;

        Vector3 currentPos = buttonTop.localPosition;

        if (!_hasTriggered)
        {
            if (_handInRange && _gripHeld)
            {
                // Move the button down toward its press depth
                buttonTop.localPosition = Vector3.MoveTowards(
                    currentPos,
                    _initialLocalPos - Vector3.up * pressDepth,
                    pressSpeed * Time.deltaTime
                );

                // Check if it has reached full press depth
                if (Vector3.Distance(buttonTop.localPosition, _initialLocalPos - Vector3.up * pressDepth) < 0.001f)
                {
                    _hasTriggered = true;

                    Log("Button fully pressed!");
                    onButtonPressed?.Invoke();                 // UnityEvent for custom logic
                    if (audioSource != null) audioSource.Play();
                    OnAnyButtonPressed?.Invoke(this);          // Static event broadcast to puzzle controller(s)

                    // TODO: Play particle effect or special visual feedback here
                }
            }
            else
            {
                // Grip released early before full press depth: reset toward idle position
                buttonTop.localPosition = Vector3.MoveTowards(
                    currentPos,
                    _initialLocalPos,
                    pressSpeed * Time.deltaTime
                );
            }
        }
    }



    /// <summary>
    /// Trigger detection for hand/controller proximity.
    /// </summary>
    private void OnTriggerEnter(Collider other)
    {
        if (other.GetComponent<HandPressCollider>() != null)

        {
            _handInRange = true;
            Log("Hand in range.");
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.GetComponent<HandPressCollider>() != null)
        {
            _handInRange = false;
            Log("Hand left range.");
        }
    }

    /// <summary>
    /// Restores the button to its un-pressed state so the puzzle can be replayed:
    /// lifts the top back up, clears the press latches, and restores the idle material.
    /// </summary>
    public void ResetElement()
    {
        _hasTriggered = false;
        _handInRange = false;

        if (buttonTop != null)
            buttonTop.localPosition = _initialLocalPos;

        if (_renderer != null && idleMat != null)
            _renderer.material = idleMat;

        Log("Button reset.");
    }
}
