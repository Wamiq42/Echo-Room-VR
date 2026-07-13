using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

public class EchoButtonInteractable : BaseInteractable, IPuzzleElement
{
    [Header("Button Settings")]
    [SerializeField] private Transform buttonTop;
    [SerializeField] private float pressDepth = 0.02f;
    [SerializeField] private float pressSpeed = 10f;
    [SerializeField] private PlayerInputManager inputManager;
    [SerializeField] private bool allowKeyboardTesting = true;
    [SerializeField] private float keyboardTestRange = 2f;
    [SerializeField] private UnityEvent onButtonPressed;

    [Header("Button Audio")]
    [SerializeField] private AudioClip buttonOnClip;
    [SerializeField] private AudioClip buttonOffClip;
    [SerializeField, Range(0f, 1f)] private float buttonAudioVolume = 1f;

    private Vector3 _initialLocalPos;
    private bool _handInRange;
    private bool _wasGripHeld;
    private bool _isPressing;
    private bool _isOn;
    private Animator _animator;

    public bool IsOn => _isOn;

    public static event System.Action<EchoButtonInteractable> OnAnyButtonPressed;

    protected override void Awake()
    {
        base.Awake();
        _animator = GetComponent<Animator>();
        ResolveButtonTop();

        Renderer buttonRenderer = buttonTop != null ? buttonTop.GetComponent<Renderer>() : null;
        if (buttonRenderer == null && buttonTop != null)
            buttonRenderer = buttonTop.GetComponentInChildren<Renderer>(true);
        ConfigureOnStateVisual(buttonRenderer, 0);
        SetOnStateVisual(false, false);

        if (buttonTop != null)
            _initialLocalPos = buttonTop.localPosition;

        ResolveInputManager();
        SetAnimatorOn(false);
    }

    private void Update()
    {
        ResolveInputManager();

        bool gripHeld = inputManager != null && inputManager.ReadGrip();
        bool gripPressed = _handInRange && gripHeld && !_wasGripHeld;
        bool keyboardPressed = allowKeyboardTesting && Keyboard.current != null && Keyboard.current.eKey.wasPressedThisFrame;

        if (!_isOn && gripPressed)
            TurnOn(false);

        if (!_isOn && keyboardPressed && IsPlayerCloseEnoughForKeyboardTest())
        {
            Debug.Log($"[EchoButtonInteractable] E pressed on {name}; button is on.");
            TurnOn(true);
        }

        AnimateFallbackPress();
        _wasGripHeld = gripHeld;
    }

    public void TurnOn(bool fromKeyboard = false)
    {
        if (_isOn) return;

        _isOn = true;
        _isPressing = !CanUseAnimatorBool();
        SetAnimatorOn(true);
        SetOnStateVisual(true, true);

        if (!_isPressing)
            CompletePress(fromKeyboard);
    }

    private void AnimateFallbackPress()
    {
        if (!_isPressing || buttonTop == null) return;

        Vector3 pressedPos = _initialLocalPos - Vector3.up * pressDepth;
        buttonTop.localPosition = Vector3.MoveTowards(buttonTop.localPosition, pressedPos, pressSpeed * Time.deltaTime);

        if (Vector3.Distance(buttonTop.localPosition, pressedPos) < 0.001f)
            CompletePress(false);
    }

    private void CompletePress(bool fromKeyboard)
    {
        _isPressing = false;

        if (fromKeyboard)
            Debug.Log($"[EchoButtonInteractable] {name} is on from E input.");

        Log("Button turned on.");
        onButtonPressed?.Invoke();
        PlayButtonClip(buttonOnClip);

        OnAnyButtonPressed?.Invoke(this);
    }

    private void PlayButtonClip(AudioClip clip)
    {
        if (audioSource == null) return;

        if (clip != null)
            audioSource.PlayOneShot(clip, buttonAudioVolume);
        else if (audioSource.clip != null)
            audioSource.Play();
    }

    private void ResolveInputManager()
    {
        if (inputManager != null) return;

        if (GameManager.Instance != null)
            inputManager = GameManager.Instance.PlayerInputManager;

        if (inputManager == null)
            inputManager = FindObjectOfType<PlayerInputManager>();
    }

    private void ResolveButtonTop()
    {
        if (buttonTop != null) return;

        Transform namedTop = transform.Find("Button_Object");
        if (namedTop != null)
        {
            buttonTop = namedTop;
            return;
        }

        if (transform.childCount > 0)
            buttonTop = transform.GetChild(0);
    }

    private bool IsPlayerCloseEnoughForKeyboardTest()
    {
        Camera mainCamera = Camera.main;
        if (mainCamera == null) return false;

        return Vector3.Distance(mainCamera.transform.position, transform.position) <= keyboardTestRange;
    }

    private bool CanUseAnimatorBool()
    {
        if (_animator == null)
            _animator = GetComponent<Animator>();

        if (_animator == null)
            return false;

        foreach (AnimatorControllerParameter parameter in _animator.parameters)
        {
            if (parameter.type == AnimatorControllerParameterType.Bool && (parameter.name == "Pressed" || parameter.name == "On"))
                return true;
        }

        return false;
    }

    private void SetAnimatorOn(bool isOn)
    {
        if (_animator == null)
            _animator = GetComponent<Animator>();

        if (_animator == null) return;

        foreach (AnimatorControllerParameter parameter in _animator.parameters)
        {
            if (parameter.type != AnimatorControllerParameterType.Bool) continue;
            if (parameter.name == "Pressed" || parameter.name == "On")
                _animator.SetBool(parameter.name, isOn);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.GetComponent<HandPressCollider>() == null) return;
        _handInRange = true;
        Log("Hand in range.");
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.GetComponent<HandPressCollider>() == null) return;
        _handInRange = false;
        Log("Hand left range.");
    }

    public void ResetElement()
    {
        _isOn = false;
        _handInRange = false;
        _wasGripHeld = false;
        _isPressing = false;
        SetAnimatorOn(false);

        if (buttonTop != null)
            buttonTop.localPosition = _initialLocalPos;

        if (_renderer != null && idleMat != null)
            _renderer.material = idleMat;

        SetOnStateVisual(false, false);
        PlayButtonClip(buttonOffClip);
        Log("Button reset.");
    }
}
