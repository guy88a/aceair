using System;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameStateManager : MonoBehaviour
{
    public static GameStateManager Instance { get; private set; }

    public GameState CurrentState { get; private set; }

    public event Action<GameState, GameState> OnStateChanged;

    public GameState LoadingTargetState { get; private set; }
    public LoadingVisualType LoadingVisualType { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void SetState(GameState newState)
    {
        if (CurrentState == newState)
            return;

        GameState previousState = CurrentState;
        CurrentState = newState;

        Debug.Log($"Game State Changed: {previousState} -> {newState}");

        HandleStateEnter(newState);
        OnStateChanged?.Invoke(previousState, newState);
    }

    private void HandleStateEnter(GameState state)
    {
        switch (state)
        {
            case GameState.Boot:
                break;

            case GameState.Loading:
                SceneLoader.Instance.LoadScene("Loading");
                break;

            case GameState.MainMenu:
                break;

            case GameState.Run:
                break;

            case GameState.Critical:
                break;

            case GameState.Dead:
                break;

            case GameState.GroundPhase:
                break;

            case GameState.End:
                break;
        }
    }

    public bool IsState(GameState state)
    {
        return CurrentState == state;
    }

    public void StartLoading(GameState targetState)
    {
        LoadingTargetState = targetState;
        LoadingVisualType = targetState == GameState.MainMenu
                            ? LoadingVisualType.Boot
                            : LoadingVisualType.RunStart;

        SetState(GameState.Loading);
    }
}