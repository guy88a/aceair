using System.Collections.Generic;
using UnityEngine;

public sealed class ParallaxWorld : MonoBehaviour
{
    [SerializeField] private WorldThemeManager themeManager;
    [SerializeField] private List<ParallaxLayerController> layers = new();

    private int completedLayerCount;
    private bool themeChangeInProgress;

    public IReadOnlyList<ParallaxLayerController> Layers => layers;
    public bool IsThemeChangeInProgress => themeChangeInProgress;

    private void Awake()
    {
        if (themeManager == null)
            themeManager = GetComponent<WorldThemeManager>();

        if (layers == null)
            layers = new List<ParallaxLayerController>();

        if (layers.Count == 0)
        {
            ParallaxLayerController[] foundLayers = GetComponentsInChildren<ParallaxLayerController>(true);

            for (int i = 0; i < foundLayers.Length; i++)
            {
                ParallaxLayerController layer = foundLayers[i];

                if (layer == null)
                    continue;

                if (!layers.Contains(layer))
                    layers.Add(layer);
            }
        }
    }

    public void RegisterLayer(ParallaxLayerController layer)
    {
        if (layer == null)
            return;

        if (layers.Contains(layer))
            return;

        layers.Add(layer);
    }

    public void ApplyInitialTheme(ThemeDefinition theme)
    {
        if (theme == null)
            return;

        for (int i = 0; i < layers.Count; i++)
        {
            ParallaxLayerController layer = layers[i];

            if (layer == null)
                continue;

            layer.InitializeLayer(theme);
        }
    }

    public void RequestThemeChange(
        ThemeDefinition nextTheme,
        ThemeTransitionDefinition transition
    )
    {
        if (nextTheme == null || transition == null)
            return;

        completedLayerCount = 0;
        themeChangeInProgress = true;

        if (layers == null || layers.Count == 0)
        {
            FinishThemeChange();
            return;
        }

        for (int i = 0; i < layers.Count; i++)
        {
            ParallaxLayerController layer = layers[i];

            if (layer == null)
            {
                completedLayerCount++;
                continue;
            }

            layer.RequestThemeChange(nextTheme, transition);
        }

        if (completedLayerCount >= layers.Count)
            FinishThemeChange();
    }

    public void NotifyLayerThemeChangeComplete(ParallaxLayerController layer)
    {
        if (!themeChangeInProgress)
            return;

        completedLayerCount++;

        if (completedLayerCount >= layers.Count)
            FinishThemeChange();
    }

    private void FinishThemeChange()
    {
        themeChangeInProgress = false;
        completedLayerCount = 0;

        if (themeManager != null)
            themeManager.CompleteThemeChange();
    }
}