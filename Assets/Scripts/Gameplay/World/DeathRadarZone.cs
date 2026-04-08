using UnityEngine;

public class DeathRadarZone : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D other)
    {
        RadarCrossMarker radarCross = other.GetComponent<RadarCrossMarker>();

        if (radarCross == null)
            return;

        Debug.Log("Player destroyed (DeathRadarZone).");

        // later:
        // GameManager.Instance.PlayerDied();
    }
}