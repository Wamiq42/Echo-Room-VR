using UnityEngine;

[DisallowMultipleComponent]
public sealed class MazeWorldTextController : MonoBehaviour
{
    [Header("Level Intro")]
    [SerializeField] private TextMesh introText;
    [SerializeField, Min(0f)] private float introDuration = 5f;
    [SerializeField, Min(0.01f)] private float introFadeInDuration = 0.35f;
    [SerializeField, Min(0.01f)] private float introFadeOutDuration = 0.75f;

    [Header("Door Prompt")]
    [SerializeField] private TextMesh doorPromptText;
    [SerializeField] private Transform door;
    [SerializeField] private PuzzleBase puzzle;
    [SerializeField, Min(0f)] private float doorPromptDistance = 2.25f;
    [SerializeField, Min(0.01f)] private float doorFadeDuration = 0.4f;

    private float introElapsed;
    private float doorAlpha;

    private void OnEnable()
    {
        introElapsed = 0f;
        doorAlpha = 0f;
        SetAlpha(introText, 0f);
        SetAlpha(doorPromptText, 0f);
    }

    private void Update()
    {
        UpdateIntro();
        UpdateDoorPrompt();
    }

    private void UpdateIntro()
    {
        if (introText == null)
            return;

        introElapsed += Time.deltaTime;
        float alpha;

        if (introElapsed < introFadeInDuration)
        {
            alpha = introElapsed / introFadeInDuration;
        }
        else if (introElapsed < introDuration - introFadeOutDuration)
        {
            alpha = 1f;
        }
        else if (introElapsed < introDuration)
        {
            alpha = 1f - ((introElapsed - (introDuration - introFadeOutDuration)) / introFadeOutDuration);
        }
        else
        {
            alpha = 0f;
        }

        SetAlpha(introText, Mathf.Clamp01(alpha));
    }

    private void UpdateDoorPrompt()
    {
        if (doorPromptText == null || door == null)
            return;

        Camera playerCamera = Camera.main;
        bool puzzleIncomplete = puzzle == null || !puzzle.IsSolved;
        bool closeEnough = playerCamera != null
            && Vector3.Distance(playerCamera.transform.position, door.position) <= doorPromptDistance;
        float targetAlpha = closeEnough && puzzleIncomplete ? 1f : 0f;
        float fadeSpeed = 1f / doorFadeDuration;

        doorAlpha = Mathf.MoveTowards(doorAlpha, targetAlpha, fadeSpeed * Time.deltaTime);
        SetAlpha(doorPromptText, doorAlpha);
    }

    private static void SetAlpha(TextMesh target, float alpha)
    {
        if (target == null)
            return;

        Color color = target.color;
        color.a = alpha;
        target.color = color;

        Renderer textRenderer = target.GetComponent<Renderer>();
        if (textRenderer != null)
            textRenderer.enabled = alpha > 0.001f;
    }
}
