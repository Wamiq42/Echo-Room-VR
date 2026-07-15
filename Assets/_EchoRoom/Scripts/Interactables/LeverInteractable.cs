using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

public class LeverInteractable : BaseInteractable, IPuzzleElement
{
    [Header("Lever Settings")]
    [SerializeField] private PlayerInputManager inputManager;
    [SerializeField] private bool allowKeyboardTesting = true;
    [SerializeField] private float keyboardTestRange = 2f;
    [SerializeField] private AudioClip leverOnClip;
    [SerializeField] private AudioClip leverOffClip;
    [SerializeField] private UnityEvent onLeverTurnedOn;
    [SerializeField] private UnityEvent onLeverTurnedOff;

    private bool _handInRange;
    private bool _wasGripHeld;
    private bool _isOn;
    private Animator _animator;
    private HapticHand _interactionHand;

    public bool IsOn => _isOn;

    public static event System.Action<LeverInteractable> OnAnyLeverTurnedOn;
    public static event System.Action<LeverInteractable> OnAnyLeverStateChanged;

    protected override void Awake()
    {
        base.Awake();
        _animator = GetComponent<Animator>();
        ConfigureLeverOnStateVisual();
        SetOnStateVisual(false, false);
        SetAnimatorOn(false);

        if (GameManager.Instance != null)
            inputManager = GameManager.Instance.PlayerInputManager;
    }

    private void Update()
    {
        bool gripHeld = inputManager != null && inputManager.ReadGrip();
        bool gripPressed = _handInRange && gripHeld && !_wasGripHeld;
        bool keyboardPressed = allowKeyboardTesting && Keyboard.current != null && Keyboard.current.eKey.wasPressedThisFrame;

        if (gripPressed)
            Toggle(_interactionHand);
        else if (keyboardPressed && IsPlayerCloseEnoughForKeyboardTest())
            Toggle(HapticHand.None);

        _wasGripHeld = gripHeld;
    }

    public void Toggle()
    {
        Toggle(HapticHand.None);
    }

    public void Toggle(HapticHand hand)
    {
        SetOn(!_isOn, hand);
    }

    public void TurnOn()
    {
        SetOn(true, HapticHand.None);
    }

    public void TurnOff()
    {
        SetOn(false, HapticHand.None);
    }

    private void SetOn(bool isOn, HapticHand hand)
    {
        if (_isOn == isOn)
        {
            SetAnimatorOn(_isOn);
            SetOnStateVisual(_isOn, false);
            return;
        }

        _isOn = isOn;
        Log(_isOn ? "Lever turned on." : "Lever turned off.");
        SetAnimatorOn(_isOn);
        SetOnStateVisual(_isOn, _isOn);

        if (_isOn)
        {
            onLeverTurnedOn?.Invoke();
            OnAnyLeverTurnedOn?.Invoke(this);
        }
        else
        {
            onLeverTurnedOff?.Invoke();
        }

        PlayLeverAudio(_isOn ? leverOnClip : leverOffClip);
        EchoHaptics.PlayLever(hand);
        OnAnyLeverStateChanged?.Invoke(this);
    }

    private bool IsPlayerCloseEnoughForKeyboardTest()
    {
        Camera mainCamera = Camera.main;
        if (mainCamera == null) return false;

        return Vector3.Distance(mainCamera.transform.position, transform.position) <= keyboardTestRange;
    }

    private void OnTriggerEnter(Collider other)
    {
        HandPressCollider handCollider = other.GetComponent<HandPressCollider>();
        if (handCollider == null) return;

        _handInRange = true;
        _interactionHand = handCollider.Hand;
        Log("Hand in range.");
    }

    private void OnTriggerExit(Collider other)
    {
        HandPressCollider handCollider = other.GetComponent<HandPressCollider>();
        if (handCollider == null) return;

        _handInRange = false;
        if (_interactionHand == handCollider.Hand)
            _interactionHand = HapticHand.None;
        Log("Hand left range.");
    }

    public void ResetElement()
    {
        _isOn = false;
        _handInRange = false;
        _wasGripHeld = false;
        _interactionHand = HapticHand.None;

        if (_renderer != null && idleMat != null)
            _renderer.material = idleMat;

        SetOnStateVisual(false, false);
        SetAnimatorOn(false);
        Log("Lever reset.");
    }

    private void ConfigureLeverOnStateVisual()
    {
        Transform leverBody = transform.Find("Lever_Body");
        Renderer bodyRenderer = leverBody != null ? leverBody.GetComponent<Renderer>() : null;
        if (bodyRenderer == null)
            bodyRenderer = GetComponentInChildren<Renderer>(true);

        int indicatorMaterialIndex = 0;
        if (bodyRenderer != null)
        {
            Material[] materials = bodyRenderer.sharedMaterials;
            for (int i = 0; i < materials.Length; i++)
            {
                if (materials[i] != null && materials[i].name == "Lever_Red")
                {
                    indicatorMaterialIndex = i;
                    break;
                }
            }
        }

        ConfigureOnStateVisual(bodyRenderer, indicatorMaterialIndex);
    }

    private void SetAnimatorOn(bool isOn)
    {
        if (_animator == null)
            _animator = GetComponent<Animator>();

        if (_animator != null)
            _animator.SetBool("On", isOn);
    }

    private void PlayLeverAudio(AudioClip clip)
    {
        if (audioSource == null || clip == null) return;

        audioSource.clip = clip;
        audioSource.Play();
    }
}
