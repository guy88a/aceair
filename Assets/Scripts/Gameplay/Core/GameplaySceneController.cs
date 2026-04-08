using UnityEngine;

public class GameplaySceneController : MonoBehaviour
{
    [Header("Scene References")]
    [SerializeField] private PlayerInput playerInput;
    [SerializeField] private PlayerMovement playerMovement;

    private void Awake()
    {
        ValidateReferences();
    }

    private void Start()
    {
        InitializeGameplay();
    }

    private void ValidateReferences()
    {
        if (playerInput == null)
            Debug.LogError("GameplaySceneController: PlayerInput reference is missing.");

        if (playerMovement == null)
            Debug.LogError("GameplaySceneController: PlayerMovement reference is missing.");
    }

    private void InitializeGameplay()
    {
        if (playerInput == null || playerMovement == null)
            return;

        playerMovement.Initialize(playerInput);
    }
}