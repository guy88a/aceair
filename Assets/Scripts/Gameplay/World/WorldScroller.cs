using UnityEngine;

public class WorldScroller : MonoBehaviour
{
    [SerializeField] private float scrollSpeed = 5f;

    private void Update()
    {
        Scroll();
    }

    private void Scroll()
    {
        transform.position += Vector3.left * scrollSpeed * Time.deltaTime;
    }
}