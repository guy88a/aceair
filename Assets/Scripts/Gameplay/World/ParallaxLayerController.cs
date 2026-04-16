using UnityEngine;

public enum ParallaxLayerState
{
    Stable,
    WaitingForSeam,
    Transitioning,
    FinalizeNewTheme
}

public sealed class ParallaxLayerController : MonoBehaviour
{
    [Header("Setup")]
    [SerializeField][Min(0)] private int layerIndex;
    [SerializeField] private float speedFactor = 1f;
    [SerializeField] private ParallaxWorld parallaxWorld;
    [SerializeField] private LayerPropSpawner propSpawner;

    [Header("Roots")]
    [SerializeField] private Transform activeThemeRoot;
    [SerializeField] private Transform incomingThemeRoot;
    [SerializeField] private Transform transitionRoot;

    [Header("Runtime")]
    [SerializeField] private ParallaxLayerState currentState = ParallaxLayerState.Stable;
    [SerializeField] private ThemeDefinition activeTheme;
    [SerializeField] private ThemeDefinition incomingTheme;
    [SerializeField] private ThemeTransitionDefinition activeTransition;
    [SerializeField] private ThemeLayerContent activeLayerContent;
    [SerializeField] private ThemeLayerContent incomingLayerContent;

    private LayerTransitionContent transitionLayerContent;
    private bool completionReported;

    public int LayerIndex => layerIndex;
    public float SpeedFactor => speedFactor;
    public ParallaxLayerState CurrentState => currentState;
    public ThemeDefinition ActiveTheme => activeTheme;

    private void Awake()
    {
        if (parallaxWorld == null)
            parallaxWorld = GetComponentInParent<ParallaxWorld>();

        if (propSpawner == null)
            propSpawner = GetComponentInChildren<LayerPropSpawner>(true);

        EnsureChildRoot(ref activeThemeRoot, "ActiveThemeRoot");
        EnsureChildRoot(ref incomingThemeRoot, "IncomingThemeRoot");
        EnsureChildRoot(ref transitionRoot, "TransitionRoot");

        if (parallaxWorld != null)
            parallaxWorld.RegisterLayer(this);
    }

    private void Update()
    {
        switch (currentState)
        {
            case ParallaxLayerState.Stable:
                UpdateStableLoop();
                break;

            case ParallaxLayerState.WaitingForSeam:
                UpdateStableLoop();

                if (IsAtSafeSeamPoint())
                    StartVisibleSwap();
                break;

            case ParallaxLayerState.Transitioning:
                UpdateTransition();
                break;

            case ParallaxLayerState.FinalizeNewTheme:
                FinalizeThemeSwap();
                break;
        }
    }

    public void InitializeLayer(ThemeDefinition theme)
    {
        activeTheme = theme;
        incomingTheme = null;
        activeTransition = null;
        transitionLayerContent = null;

        currentState = ParallaxLayerState.Stable;
        completionReported = false;

        activeLayerContent = activeTheme != null
            ? activeTheme.GetLayerContent(layerIndex)
            : null;

        ClearRoot(incomingThemeRoot);
        ClearRoot(transitionRoot);
        ApplyLayerContentToRoot(activeThemeRoot, activeLayerContent);

        if (propSpawner != null)
        {
            propSpawner.SetActiveSpawnSet(
                activeLayerContent != null && !activeLayerContent.IsEmpty
                    ? activeLayerContent.SpawnSetPrefab
                    : null
            );
        }
    }

    public void RequestThemeChange(
        ThemeDefinition nextTheme,
        ThemeTransitionDefinition transition
    )
    {
        incomingTheme = nextTheme;
        activeTransition = transition;

        incomingLayerContent = incomingTheme != null
            ? incomingTheme.GetLayerContent(layerIndex)
            : null;

        transitionLayerContent = activeTransition != null
            ? activeTransition.GetLayerContent(layerIndex)
            : null;

        completionReported = false;
        currentState = ParallaxLayerState.WaitingForSeam;

        if (propSpawner != null)
        {
            propSpawner.StopStableSpawning();

            propSpawner.SetIncomingSpawnSet(
                incomingLayerContent != null && !incomingLayerContent.IsEmpty
                    ? incomingLayerContent.SpawnSetPrefab
                    : null
            );
        }
    }

    private void UpdateStableLoop()
    {
        // Base shell only.
        // Stable looping logic for the active seamless strip will go here.
    }

    private bool IsAtSafeSeamPoint()
    {
        // Base shell only.
        // Replace this with actual seam-safe detection when the strip loop is implemented.
        return true;
    }

    private void StartVisibleSwap()
    {
        ApplyLayerContentToRoot(incomingThemeRoot, incomingLayerContent);
        ApplyTransitionContentToRoot(transitionRoot, transitionLayerContent);

        currentState = ParallaxLayerState.Transitioning;
    }

    private void UpdateTransition()
    {
        // Base shell only.
        // Replace this with actual current/transition/incoming movement logic.
        currentState = ParallaxLayerState.FinalizeNewTheme;
    }

    private void FinalizeThemeSwap()
    {
        activeTheme = incomingTheme;
        incomingTheme = null;
        activeLayerContent = incomingLayerContent;
        incomingLayerContent = null;
        activeTransition = null;
        transitionLayerContent = null;

        ApplyLayerContentToRoot(activeThemeRoot, activeLayerContent);
        ClearRoot(incomingThemeRoot);
        ClearRoot(transitionRoot);

        if (propSpawner != null)
            propSpawner.FinalizeIncomingSpawnSet();

        currentState = ParallaxLayerState.Stable;

        if (completionReported)
            return;

        completionReported = true;

        if (parallaxWorld != null)
            parallaxWorld.NotifyLayerThemeChangeComplete(this);
    }

    private void ApplyLayerContentToRoot(Transform root, ThemeLayerContent content)
    {
        if (root == null)
            return;

        ClearRoot(root);

        if (content == null || content.IsEmpty || content.SeamlessStripPrefab == null)
            return;

        GameObject instance = Instantiate(content.SeamlessStripPrefab, root);
        instance.transform.localPosition = content.LocalOffset;
        instance.transform.localRotation = Quaternion.identity;
        instance.transform.localScale = Vector3.one;
    }

    private void ApplyTransitionContentToRoot(Transform root, LayerTransitionContent content)
    {
        if (root == null)
            return;

        ClearRoot(root);

        if (content == null || content.IsEmpty || content.TransitionStripPrefab == null)
            return;

        GameObject instance = Instantiate(content.TransitionStripPrefab, root);
        instance.transform.localPosition = content.LocalOffset;
        instance.transform.localRotation = Quaternion.identity;
        instance.transform.localScale = Vector3.one;
    }

    private void ClearRoot(Transform root)
    {
        if (root == null)
            return;

        for (int i = root.childCount - 1; i >= 0; i--)
            Destroy(root.GetChild(i).gameObject);
    }

    private void EnsureChildRoot(ref Transform root, string childName)
    {
        if (root != null)
            return;

        Transform found = transform.Find(childName);

        if (found != null)
        {
            root = found;
            return;
        }

        GameObject child = new GameObject(childName);
        child.transform.SetParent(transform, false);
        root = child.transform;
    }
}