using UnityEngine;

public class Door : MonoBehaviour
{
    public bool open;

    [SerializeField] private Animator animator;
    [SerializeField] private string openParameter = "LeversOn";

    public AudioSource asource;
    public AudioClip openDoor, closeDoor;

    private int _openParameterHash;

    private void Awake()
    {
        _openParameterHash = Animator.StringToHash(openParameter);
    }

    private void Start()
    {
        if (asource == null)
            asource = GetComponent<AudioSource>();

        if (animator == null)
            animator = GetComponent<Animator>();

        SetAnimatorOpen(open);
    }

    public void OpenDoor()
    {
        SetOpen(!open);
    }

    public void Open()
    {
        SetOpen(true);
    }

    public void Close()
    {
        SetOpen(false);
    }

    public void SetOpen(bool shouldOpen)
    {
        if (open == shouldOpen)
            return;

        open = shouldOpen;
        SetAnimatorOpen(open);

        AudioClip clip = open ? openDoor : closeDoor;
        if (asource != null && clip != null)
        {
            asource.clip = clip;
            asource.Play();
        }
    }

    private void SetAnimatorOpen(bool shouldOpen)
    {
        if (animator != null)
            animator.SetBool(_openParameterHash, shouldOpen);
    }
}
