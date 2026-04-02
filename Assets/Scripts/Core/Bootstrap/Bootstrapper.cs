using UnityEngine;

public class Bootstrapper : MonoBehaviour
{
    public static Bootstrapper Instance { get; private set; }

    [Header("Persistent References")]
    [SerializeField] private Camera mainCamera;
    [SerializeField] private Canvas splashCanvas;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        if (mainCamera != null)
        {
            DontDestroyOnLoad(mainCamera.gameObject);
        }

        if (splashCanvas != null)
        {
            DontDestroyOnLoad(splashCanvas.gameObject);
        }
    }

    private void Start()
    {
        Initialize();
    }

    private void Initialize()
    {

    }
}