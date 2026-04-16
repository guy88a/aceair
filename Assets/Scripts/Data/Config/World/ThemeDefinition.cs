using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public sealed class WeightedThemeEntry
{
    [SerializeField] private ThemeDefinition theme;
    [SerializeField][Min(0)] private int weight = 1;

    public ThemeDefinition Theme => theme;
    public int Weight => weight;
}

[CreateAssetMenu(fileName = "Theme_", menuName = "AceAir/World/Theme Definition")]
public sealed class ThemeDefinition : ScriptableObject
{
    [Header("Identity")]
    [SerializeField] private string themeId;

    [Header("Distance Trigger")]
    [SerializeField][Min(0f)] private float minDistanceToChange = 25f;
    [SerializeField][Min(0f)] private float maxDistanceToChange = 40f;

    [Header("Progression")]
    [SerializeField] private List<WeightedThemeEntry> allowedNextThemes = new();

    [Header("Layer Content")]
    [SerializeField] private List<ThemeLayerContent> layerContents = new();

    public string ThemeId => themeId;
    public float MinDistanceToChange => minDistanceToChange;
    public float MaxDistanceToChange => maxDistanceToChange;
    public IReadOnlyList<WeightedThemeEntry> AllowedNextThemes => allowedNextThemes;
    public IReadOnlyList<ThemeLayerContent> LayerContents => layerContents;

    public float RollDistanceTarget()
    {
        float min = Mathf.Min(minDistanceToChange, maxDistanceToChange);
        float max = Mathf.Max(minDistanceToChange, maxDistanceToChange);

        if (Mathf.Approximately(min, max))
            return min;

        return UnityEngine.Random.Range(min, max);
    }

    public ThemeDefinition GetRandomAllowedNextTheme()
    {
        if (allowedNextThemes == null || allowedNextThemes.Count == 0)
            return null;

        int totalWeight = 0;

        for (int i = 0; i < allowedNextThemes.Count; i++)
        {
            WeightedThemeEntry entry = allowedNextThemes[i];

            if (entry == null || entry.Theme == null || entry.Weight <= 0)
                continue;

            totalWeight += entry.Weight;
        }

        if (totalWeight <= 0)
            return null;

        int roll = UnityEngine.Random.Range(0, totalWeight);
        int runningWeight = 0;

        for (int i = 0; i < allowedNextThemes.Count; i++)
        {
            WeightedThemeEntry entry = allowedNextThemes[i];

            if (entry == null || entry.Theme == null || entry.Weight <= 0)
                continue;

            runningWeight += entry.Weight;

            if (roll < runningWeight)
                return entry.Theme;
        }

        return null;
    }

    public ThemeLayerContent GetLayerContent(int layerIndex)
    {
        if (layerContents == null)
            return null;

        for (int i = 0; i < layerContents.Count; i++)
        {
            ThemeLayerContent content = layerContents[i];

            if (content == null)
                continue;

            if (content.LayerIndex == layerIndex)
                return content;
        }

        return null;
    }
}