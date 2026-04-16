using UnityEngine;

public sealed class LayerPropSpawner : MonoBehaviour
{
    [Header("Roots")]
    [SerializeField] private Transform activeSpawnRoot;
    [SerializeField] private Transform incomingSpawnRoot;

    [Header("Runtime")]
    [SerializeField] private GameObject activeSpawnSetPrefab;
    [SerializeField] private GameObject incomingSpawnSetPrefab;
    [SerializeField] private bool stableSpawningEnabled;

    public bool StableSpawningEnabled => stableSpawningEnabled;
    public GameObject ActiveSpawnSetPrefab => activeSpawnSetPrefab;
    public GameObject IncomingSpawnSetPrefab => incomingSpawnSetPrefab;

    private void Awake()
    {
        EnsureChildRoot(ref activeSpawnRoot, "ActiveSpawnRoot");
        EnsureChildRoot(ref incomingSpawnRoot, "IncomingSpawnRoot");
    }

    public void SetActiveSpawnSet(GameObject spawnSetPrefab)
    {
        activeSpawnSetPrefab = spawnSetPrefab;
        stableSpawningEnabled = activeSpawnSetPrefab != null;

        ApplySpawnSetToRoot(activeSpawnRoot, activeSpawnSetPrefab);
    }

    public void SetIncomingSpawnSet(GameObject spawnSetPrefab)
    {
        incomingSpawnSetPrefab = spawnSetPrefab;

        ApplySpawnSetToRoot(incomingSpawnRoot, incomingSpawnSetPrefab);
    }

    public void StopStableSpawning()
    {
        stableSpawningEnabled = false;
    }

    public void FinalizeIncomingSpawnSet()
    {
        activeSpawnSetPrefab = incomingSpawnSetPrefab;
        incomingSpawnSetPrefab = null;

        ApplySpawnSetToRoot(activeSpawnRoot, activeSpawnSetPrefab);
        ClearRoot(incomingSpawnRoot);

        stableSpawningEnabled = activeSpawnSetPrefab != null;
    }

    private void ApplySpawnSetToRoot(Transform root, GameObject spawnSetPrefab)
    {
        if (root == null)
            return;

        ClearRoot(root);

        if (spawnSetPrefab == null)
            return;

        GameObject instance = Instantiate(spawnSetPrefab, root);
        instance.transform.localPosition = Vector3.zero;
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