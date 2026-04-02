using System;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameStateManager : MonoBehaviour
{
    public static GameStateManager Instance { get; private set; }

    public GameState CurrentState { get; private set; }

    public event Action<GameState, GameState> OnStateChanged;

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
                SceneLoader.Instance.LoadScene("MainMenu");
                break;

            case GameState.Run:
                SceneLoader.Instance.LoadScene("Gameplay");
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
}