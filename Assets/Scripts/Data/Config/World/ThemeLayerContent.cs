using UnityEngine;

[CreateAssetMenu(fileName = "ThemeLayer_", menuName = "AceAir/World/Theme Layer Content")]
public sealed class ThemeLayerContent : ScriptableObject
{
    [Header("Identity")]
    [SerializeField][Min(0)] private int layerIndex;

    [Header("Content")]
    [SerializeField] private bool isEmpty;
    [SerializeField] private GameObject seamlessStripPrefab;
    [SerializeField] private GameObject spawnSetPrefab;

    [Header("Placement")]
    [SerializeField] private Vector2 localOffset;

    public int LayerIndex => layerIndex;
    public bool IsEmpty => isEmpty;
    public GameObject SeamlessStripPrefab => seamlessStripPrefab;
    public GameObject SpawnSetPrefab => spawnSetPrefab;
    public Vector2 LocalOffset => localOffset;
}