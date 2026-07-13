using System;
using UnityEngine;

/// <summary>
/// Applies inexpensive Unity distance fog per loaded level while preserving sonar reveal behavior.
/// The EchoSonarReveal shader already gates visible surface color by sonar; this controller only
/// changes RenderSettings fog, so hidden surfaces remain black until a ping reveals them.
/// </summary>
public class EchoFogController : MonoBehaviour
{
    [Serializable]
    private struct LevelFogProfile
    {
        public string levelName;
        public Color fogColor;
        [Min(0f)] public float startDistance;
        [Min(0.01f)] public float endDistance;
    }

    [Header("Level Fog")]
    [SerializeField] private bool applyOnStart = true;
    [SerializeField] private FogMode fogMode = FogMode.Linear;
    [SerializeField] private Color defaultFogColor = new Color(0.015f, 0.02f, 0.035f, 1f);
    [SerializeField, Min(0f)] private float defaultStartDistance = 16f;
    [SerializeField, Min(0.01f)] private float defaultEndDistance = 62f;

    [Header("Tutorial")]
    [SerializeField] private bool useTutorialFog = true;
    [SerializeField] private Color tutorialFogColor = new Color(0.02f, 0.024f, 0.04f, 1f);
    [SerializeField, Min(0f)] private float tutorialStartDistance = 18f;
    [SerializeField, Min(0.01f)] private float tutorialEndDistance = 70f;

    [Header("Profiles")]
    [SerializeField]
    private LevelFogProfile[] profiles =
    {
        new LevelFogProfile { levelName = "Maze A", fogColor = new Color(0.014f, 0.018f, 0.032f, 1f), startDistance = 16f, endDistance = 64f },
        new LevelFogProfile { levelName = "Maze B", fogColor = new Color(0.018f, 0.02f, 0.034f, 1f), startDistance = 15f, endDistance = 58f },
        new LevelFogProfile { levelName = "Maze C", fogColor = new Color(0.02f, 0.022f, 0.036f, 1f), startDistance = 14f, endDistance = 54f },
        new LevelFogProfile { levelName = "Maze D", fogColor = new Color(0.018f, 0.018f, 0.03f, 1f), startDistance = 13f, endDistance = 50f },
        new LevelFogProfile { levelName = "Maze E", fogColor = new Color(0.025f, 0.012f, 0.016f, 1f), startDistance = 12f, endDistance = 46f }
    };

    private GameManager gameManager;

    private void Awake()
    {
        gameManager = GetComponent<GameManager>();
        if (gameManager == null)
            gameManager = FindObjectOfType<GameManager>();
    }

    private void OnEnable()
    {
        if (gameManager == null)
            gameManager = GetComponent<GameManager>();

        if (gameManager != null)
            gameManager.OnLevelLoaded += HandleLevelLoaded;
    }

    private void Start()
    {
        if (applyOnStart)
            ApplyDefaultFog();
    }

    private void OnDisable()
    {
        if (gameManager != null)
            gameManager.OnLevelLoaded -= HandleLevelLoaded;
    }

    private void HandleLevelLoaded(Level level)
    {
        if (level == null)
        {
            ApplyDefaultFog();
            return;
        }

        ApplyProfile(level.levelName);
    }

    public void ApplyTutorialFog()
    {
        if (useTutorialFog)
            ApplyFog(tutorialFogColor, tutorialStartDistance, tutorialEndDistance);
        else
            ApplyDefaultFog();
    }

    public void ApplyDefaultFog()
    {
        ApplyFog(defaultFogColor, defaultStartDistance, defaultEndDistance);
    }

    private void ApplyProfile(string levelName)
    {
        for (int i = 0; i < profiles.Length; i++)
        {
            if (string.Equals(profiles[i].levelName, levelName, StringComparison.OrdinalIgnoreCase))
            {
                ApplyFog(profiles[i].fogColor, profiles[i].startDistance, profiles[i].endDistance);
                return;
            }
        }

        ApplyDefaultFog();
    }

    private void ApplyFog(Color color, float startDistance, float endDistance)
    {
        if (endDistance <= startDistance)
            endDistance = startDistance + 0.01f;

        RenderSettings.fog = true;
        RenderSettings.fogMode = fogMode;
        RenderSettings.fogColor = color;
        RenderSettings.fogStartDistance = startDistance;
        RenderSettings.fogEndDistance = endDistance;
    }
}
