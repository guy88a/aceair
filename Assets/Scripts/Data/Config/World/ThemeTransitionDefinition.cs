using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public sealed class LayerTransitionContent
{
    [SerializeField][Min(0)] private int layerIndex;
    [SerializeField] private bool isEmpty;
    [SerializeField] private GameObject transitionStripPrefab;
    [SerializeField] private Vector2 localOffset;

    public int LayerIndex => layerIndex;
    public bool IsEmpty => isEmpty;
    public GameObject TransitionStripPrefab => transitionStripPrefab;
    public Vector2 LocalOffset => localOffset;
}

[CreateAssetMenu(fileName = "Transition_", menuName = "AceAir/World/Theme Transition Definition")]
public sealed class ThemeTransitionDefinition : ScriptableObject
{
    [Header("Direction")]
    [SerializeField] private ThemeDefinition fromTheme;
    [SerializeField] private ThemeDefinition toTheme;

    [Header("Per Layer Transition Content")]
    [SerializeField] private List<LayerTransitionContent> layerContents = new();

    public ThemeDefinition FromTheme => fromTheme;
    public ThemeDefinition ToTheme => toTheme;
    public IReadOnlyList<LayerTransitionContent> LayerContents => layerContents;

    public bool Matches(ThemeDefinition from, ThemeDefinition to)
    {
        return fromTheme == from && toTheme == to;
    }

    public LayerTransitionContent GetLayerContent(int layerIndex)
    {
        if (layerContents == null)
            return null;

        for (int i = 0; i < layerContents.Count; i++)
        {
            LayerTransitionContent content = layerContents[i];

            if (content == null)
                continue;

            if (content.LayerIndex == layerIndex)
                return content;
        }

        return null;
    }
}