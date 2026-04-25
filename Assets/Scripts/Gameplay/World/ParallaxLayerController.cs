using UnityEngine;

public enum ParallaxLayerState
{
    Stable,
    WaitingForTransitionReposition,
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
    [SerializeField][Min(0f)] private float testScrollSpeed = 2f;

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
            case ParallaxLayerState.WaitingForTransitionReposition:
                UpdateStableLoop();
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

        if (propSpawner != null)
        {
            propSpawner.StopStableSpawning();

            propSpawner.SetIncomingSpawnSet(
                incomingLayerContent != null && !incomingLayerContent.IsEmpty
                    ? incomingLayerContent.SpawnSetPrefab
                    : null
            );
        }

        if (CanWaitForLoopReposition())
            currentState = ParallaxLayerState.WaitingForTransitionReposition;
        else
            StartVisibleSwap();
    }

    private void UpdateStableLoop()
    {
        if (activeThemeRoot == null)
            return;

        if (activeThemeRoot.childCount == 0)
        {
            if (currentState == ParallaxLayerState.WaitingForTransitionReposition)
                StartVisibleSwap();

            return;
        }

        float scrollAmount = testScrollSpeed * speedFactor * Time.deltaTime;

        for (int i = 0; i < activeThemeRoot.childCount; i++)
            activeThemeRoot.GetChild(i).localPosition += Vector3.left * scrollAmount;

        float cameraLeftEdge = GetCameraLeftEdgeX();
        float rightMostX = float.MinValue;
        bool repositionedThisFrame = false;

        for (int i = 0; i < activeThemeRoot.childCount; i++)
        {
            Transform child = activeThemeRoot.GetChild(i);

            if (child.localPosition.x > rightMostX)
                rightMostX = child.localPosition.x;
        }

        for (int i = 0; i < activeThemeRoot.childCount; i++)
        {
            Transform child = activeThemeRoot.GetChild(i);
            float stripWidth = GetLoopWidth(child);

            if (stripWidth <= 0f)
                continue;

            float childRightEdge = child.localPosition.x + stripWidth;

            if (childRightEdge <= cameraLeftEdge)
            {
                child.localPosition = new Vector3(
                    rightMostX + stripWidth,
                    child.localPosition.y,
                    child.localPosition.z
                );

                rightMostX = child.localPosition.x;
                repositionedThisFrame = true;
            }
        }

        if (
            currentState == ParallaxLayerState.WaitingForTransitionReposition &&
            repositionedThisFrame
        )
        {
            StartVisibleSwap();
        }
    }

    private bool CanWaitForLoopReposition()
    {
        if (activeThemeRoot == null)
            return false;

        if (activeThemeRoot.childCount == 0)
            return false;

        if (testScrollSpeed * speedFactor <= 0f)
            return false;

        for (int i = 0; i < activeThemeRoot.childCount; i++)
        {
            if (GetLoopWidth(activeThemeRoot.GetChild(i)) > 0f)
                return true;
        }

        return false;
    }

    private void StartVisibleSwap()
    {
        float seamX = GetRootRightEdge(activeThemeRoot);

        ApplyLayerContentToRoot(incomingThemeRoot, incomingLayerContent, seamX);
        ApplyTransitionContentToRoot(transitionRoot, transitionLayerContent, seamX);

        currentState = ParallaxLayerState.Transitioning;
    }

    private void UpdateTransition()
    {
        float scrollAmount = testScrollSpeed * speedFactor * Time.deltaTime;

        MoveRootChildrenLeft(activeThemeRoot, scrollAmount);
        MoveRootChildrenLeft(incomingThemeRoot, scrollAmount);
        MoveRootChildrenLeft(transitionRoot, scrollAmount);

        bool hasActiveVisual = activeThemeRoot != null && activeThemeRoot.childCount > 0;
        bool hasIncomingVisual = incomingThemeRoot != null && incomingThemeRoot.childCount > 0;
        bool hasTransitionVisual = transitionRoot != null && transitionRoot.childCount > 0;

        float cameraLeftEdge = GetCameraLeftEdgeX();

        if (!hasIncomingVisual)
        {
            if (hasTransitionVisual)
            {
                float transitionRightEdge = GetRootRightEdge(transitionRoot);

                if (transitionRightEdge <= cameraLeftEdge)
                    currentState = ParallaxLayerState.FinalizeNewTheme;

                return;
            }

            if (hasActiveVisual)
            {
                float outgoingRightEdge = GetRootRightEdge(activeThemeRoot);

                if (outgoingRightEdge <= cameraLeftEdge)
                    currentState = ParallaxLayerState.FinalizeNewTheme;

                return;
            }

            currentState = ParallaxLayerState.FinalizeNewTheme;
            return;
        }

        float targetX = cameraLeftEdge;

        if (incomingLayerContent != null)
            targetX += incomingLayerContent.LocalOffset.x;

        float incomingLeftX = GetRootLeftMostX(incomingThemeRoot);

        if (incomingLeftX <= targetX)
            currentState = ParallaxLayerState.FinalizeNewTheme;
    }

    private float GetCameraRightEdgeX()
    {
        Camera cam = Camera.main;

        if (cam == null || !cam.orthographic)
            return 0f;

        return cam.transform.position.x + (GetCameraWorldWidth() * 0.5f);
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
        if (content == null)
        {
            ClearRoot(root);
            return;
        }

        ApplyLayerContentToRoot(root, content, GetCameraLeftEdgeX() + content.LocalOffset.x);
    }

    private void ApplyLayerContentToRoot(Transform root, ThemeLayerContent content, float startX)
    {
        if (root == null)
            return;

        ClearRoot(root);

        if (content == null || content.IsEmpty || content.SeamlessStripPrefab == null)
            return;

        GameObject first = Instantiate(content.SeamlessStripPrefab, root);
        first.transform.localRotation = Quaternion.identity;
        first.transform.localScale = Vector3.one;

        float stripWidth = GetLoopWidth(first.transform);

        if (stripWidth <= 0f)
            return;

        float cameraWidth = GetCameraWorldWidth();
        float y = content.LocalOffset.y;

        int copiesNeeded = Mathf.Max(2, Mathf.CeilToInt(cameraWidth / stripWidth) + 1);

        first.transform.localPosition = new Vector3(startX, y, 0f);

        for (int i = 1; i < copiesNeeded; i++)
        {
            GameObject copy = Instantiate(content.SeamlessStripPrefab, root);
            copy.transform.localRotation = Quaternion.identity;
            copy.transform.localScale = Vector3.one;
            copy.transform.localPosition = new Vector3(startX + (stripWidth * i), y, 0f);
        }
    }

    private void ApplyTransitionContentToRoot(Transform root, LayerTransitionContent content)
    {
        if (content == null)
        {
            ClearRoot(root);
            return;
        }

        ApplyTransitionContentToRoot(root, content, GetCameraLeftEdgeX() + content.LocalOffset.x);
    }

    private void ApplyTransitionContentToRoot(Transform root, LayerTransitionContent content, float seamX)
    {
        if (root == null)
            return;

        ClearRoot(root);

        if (content == null || content.IsEmpty || content.TransitionStripPrefab == null)
            return;

        GameObject instance = Instantiate(content.TransitionStripPrefab, root);
        instance.transform.localRotation = Quaternion.identity;
        instance.transform.localScale = Vector3.one;

        float width = GetLoopWidth(instance.transform);
        float startX = seamX - (width * 0.5f) + content.LocalOffset.x;

        instance.transform.localPosition = new Vector3(startX, content.LocalOffset.y, 0f);
    }

    private void MoveRootChildrenLeft(Transform root, float scrollAmount)
    {
        if (root == null)
            return;

        for (int i = 0; i < root.childCount; i++)
            root.GetChild(i).localPosition += Vector3.left * scrollAmount;
    }

    private float GetRootRightEdge(Transform root)
    {
        if (root == null || root.childCount == 0)
            return GetCameraRightEdgeX();

        float rightEdge = float.MinValue;

        for (int i = 0; i < root.childCount; i++)
        {
            Transform child = root.GetChild(i);
            float width = GetLoopWidth(child);

            if (width <= 0f)
                continue;

            float childRightEdge = child.localPosition.x + width;

            if (childRightEdge > rightEdge)
                rightEdge = childRightEdge;
        }

        if (rightEdge == float.MinValue)
            return GetCameraRightEdgeX();

        return rightEdge;
    }

    private float GetRootLeftMostX(Transform root)
    {
        if (root == null || root.childCount == 0)
            return float.MaxValue;

        float leftMostX = float.MaxValue;

        for (int i = 0; i < root.childCount; i++)
        {
            Transform child = root.GetChild(i);

            if (child.localPosition.x < leftMostX)
                leftMostX = child.localPosition.x;
        }

        return leftMostX;
    }

    private float GetLoopWidth(Transform stripRoot)
    {
        if (stripRoot == null)
            return 0f;

        Renderer[] renderers = stripRoot.GetComponentsInChildren<Renderer>();

        if (renderers == null || renderers.Length == 0)
            return 0f;

        Bounds bounds = renderers[0].bounds;

        for (int i = 1; i < renderers.Length; i++)
            bounds.Encapsulate(renderers[i].bounds);

        return bounds.size.x;
    }

    private float GetCameraWorldWidth()
    {
        Camera cam = Camera.main;

        if (cam == null || !cam.orthographic)
            return 0f;

        return cam.orthographicSize * 2f * cam.aspect;
    }

    private float GetCameraLeftEdgeX()
    {
        Camera cam = Camera.main;

        if (cam == null || !cam.orthographic)
            return 0f;

        return cam.transform.position.x - (GetCameraWorldWidth() * 0.5f);
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