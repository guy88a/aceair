using System.Collections.Generic;
using UnityEngine;

public sealed class WorldThemeManager : MonoBehaviour
{
    [Header("Setup")]
    [SerializeField] private ThemeDefinition startingTheme;
    [SerializeField] private List<ThemeTransitionDefinition> transitions = new();
    [SerializeField] private ParallaxWorld parallaxWorld;

    [Header("Runtime")]
    [SerializeField] private ThemeDefinition currentTheme;
    [SerializeField] private ThemeDefinition pendingTheme;
    [SerializeField][Min(0f)] private float distanceSinceActivation;
    [SerializeField][Min(0f)] private float distanceTarget;
    [SerializeField] private bool themeChangeInProgress;

    public ThemeDefinition CurrentTheme => currentTheme;
    public ThemeDefinition PendingTheme => pendingTheme;
    public float DistanceSinceActivation => distanceSinceActivation;
    public float DistanceTarget => distanceTarget;
    public bool IsThemeChangeInProgress => themeChangeInProgress;

    private void Awake()
    {
        if (parallaxWorld == null)
            parallaxWorld = GetComponent<ParallaxWorld>();
    }

    private void Start()
    {
        if (startingTheme == null || parallaxWorld == null)
            return;

        currentTheme = startingTheme;
        pendingTheme = null;
        themeChangeInProgress = false;
        distanceSinceActivation = 0f;
        distanceTarget = currentTheme.RollDistanceTarget();

        parallaxWorld.ApplyInitialTheme(currentTheme);
    }

    public void AddDistance(float deltaDistance)
    {
        if (themeChangeInProgress || currentTheme == null || deltaDistance <= 0f)
            return;

        distanceSinceActivation += deltaDistance;

        if (distanceSinceActivation >= distanceTarget)
            RequestNextTheme();
    }

    public void RequestNextTheme()
    {
        if (themeChangeInProgress || currentTheme == null || parallaxWorld == null)
            return;

        ThemeDefinition nextTheme = currentTheme.GetRandomAllowedNextTheme();

        if (nextTheme == null)
            return;

        if (!TryGetTransition(currentTheme, nextTheme, out ThemeTransitionDefinition transition))
        {
            Debug.LogWarning(
                $"[WorldThemeManager] Missing transition definition: {currentTheme.name} -> {nextTheme.name}",
                this
            );
            return;
        }

        pendingTheme = nextTheme;
        themeChangeInProgress = true;

        parallaxWorld.RequestThemeChange(nextTheme, transition);
    }

    public bool TryGetTransition(
        ThemeDefinition fromTheme,
        ThemeDefinition toTheme,
        out ThemeTransitionDefinition transition
    )
    {
        transition = null;

        if (transitions == null)
            return false;

        for (int i = 0; i < transitions.Count; i++)
        {
            ThemeTransitionDefinition entry = transitions[i];

            if (entry == null)
                continue;

            if (!entry.Matches(fromTheme, toTheme))
                continue;

            transition = entry;
            return true;
        }

        return false;
    }

    public void CompleteThemeChange()
    {
        if (!themeChangeInProgress || pendingTheme == null)
            return;

        currentTheme = pendingTheme;
        pendingTheme = null;
        themeChangeInProgress = false;
        distanceSinceActivation = 0f;
        distanceTarget = currentTheme.RollDistanceTarget();
    }
}