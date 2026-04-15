using UnityEngine;

public class TopRadarSystem : MonoBehaviour
{
    [Header("Radar Strength")]
    [SerializeField][Range(0f, 1f)] private float radarStrength = 0.5f;

    public bool IsRadarLocked { get; private set; }

    private BoxCollider2D radarZoneCollider;
    private RadarCrossMarker activeRadarCross;

    private void Awake()
    {
        radarZoneCollider = GetComponent<BoxCollider2D>();

        if (radarZoneCollider == null)
            Debug.LogError("TopRadarSystem: BoxCollider2D is missing.");
    }

    private void Update()
    {
        UpdateRadarLock();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        RadarCrossMarker radarCross = other.GetComponent<RadarCrossMarker>();

        if (radarCross == null)
            return;

        activeRadarCross = radarCross;
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        RadarCrossMarker radarCross = other.GetComponent<RadarCrossMarker>();

        if (radarCross == null)
            return;

        if (activeRadarCross == radarCross)
            activeRadarCross = null;
    }

    private void UpdateRadarLock()
    {
        if (radarZoneCollider == null)
            return;

        if (activeRadarCross == null)
        {
            SetRadarLock(false);
            return;
        }

        BoxCollider2D crossCollider = activeRadarCross.GetComponent<BoxCollider2D>();

        if (crossCollider == null)
        {
            SetRadarLock(false);
            return;
        }

        Bounds radarBounds = radarZoneCollider.bounds;
        Bounds crossBounds = crossCollider.bounds;

        float overlapMinY = Mathf.Max(radarBounds.min.y, crossBounds.min.y);
        float overlapMaxY = Mathf.Min(radarBounds.max.y, crossBounds.max.y);
        float overlapHeight = Mathf.Max(0f, overlapMaxY - overlapMinY);

        float crossHeight = crossBounds.size.y;

        if (crossHeight <= 0f)
        {
            SetRadarLock(false);
            return;
        }

        float overlapPercent = overlapHeight / crossHeight;
        float requiredCrossPercent = 1f - radarStrength;

        SetRadarLock(overlapPercent >= requiredCrossPercent);
    }

    private void SetRadarLock(bool isLocked)
    {
        if (IsRadarLocked == isLocked)
            return;

        IsRadarLocked = isLocked;

        if (IsRadarLocked)
            Debug.Log("Radar lock triggered.");
        else
            Debug.Log("Radar lock cleared.");
    }
}