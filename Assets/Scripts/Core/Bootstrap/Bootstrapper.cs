using UnityEngine;

public class Bootstrapper : MonoBehaviour
{
    public static Bootstrapper Instance { get; private set; }

    [Header("Persistent References")]
    [SerializeField] private Camera mainCamera;

    [Header("Splash")]
    [SerializeField] private float splashDuration = 1.5f;

    private float bootStartTime;
    private bool splashTimeReached;

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

        bootStartTime = Time.realtimeSinceStartup;
    }

    private void Update()
    {
        if (splashTimeReached)
            return;

        if (Time.realtimeSinceStartup - bootStartTime >= splashDuration)
        {
            splashTimeReached = true;
            Debug.Log("Splash duration reached.");
            GameStateManager.Instance.StartLoading(GameState.MainMenu);
        }
    }
}